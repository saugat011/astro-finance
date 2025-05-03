-- AstroFinance PostgreSQL Database Schema - Accounting Tables
-- Migration V2: Adding Accounting Tables

-- Journal Entries table (for accounting)
CREATE TABLE journal_entries (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    transaction_id UUID REFERENCES transactions(id),
    entry_date DATE NOT NULL,
    description TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES users(id),
    last_modified_at TIMESTAMP,
    last_modified_by UUID REFERENCES users(id)
);

-- Create index for journal entries
CREATE INDEX idx_journal_entries_date ON journal_entries(entry_date);
CREATE INDEX idx_journal_entries_transaction ON journal_entries(transaction_id);

-- Journal Entry Details table (for double-entry accounting)
CREATE TABLE journal_entry_details (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    journal_entry_id UUID NOT NULL REFERENCES journal_entries(id),
    account_name VARCHAR(100) NOT NULL,
    is_debit BOOLEAN NOT NULL,
    amount DECIMAL(15, 2) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES users(id),
    last_modified_at TIMESTAMP,
    last_modified_by UUID REFERENCES users(id)
);

-- Create index for journal entry details
CREATE INDEX idx_journal_entry_details_entry ON journal_entry_details(journal_entry_id);

-- Chart of Accounts table
CREATE TABLE chart_of_accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    account_code VARCHAR(20) NOT NULL UNIQUE,
    account_name VARCHAR(100) NOT NULL UNIQUE,
    account_type VARCHAR(50) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES users(id),
    last_modified_at TIMESTAMP,
    last_modified_by UUID REFERENCES users(id)
);

-- Create index for chart of accounts
CREATE INDEX idx_chart_of_accounts_type ON chart_of_accounts(account_type);

-- Create triggers for last_modified_at
CREATE TRIGGER update_journal_entries_last_modified
BEFORE UPDATE ON journal_entries
FOR EACH ROW EXECUTE FUNCTION update_last_modified_column();

CREATE TRIGGER update_journal_entry_details_last_modified
BEFORE UPDATE ON journal_entry_details
FOR EACH ROW EXECUTE FUNCTION update_last_modified_column();

CREATE TRIGGER update_chart_of_accounts_last_modified
BEFORE UPDATE ON chart_of_accounts
FOR EACH ROW EXECUTE FUNCTION update_last_modified_column();

-- Create audit triggers
CREATE TRIGGER audit_journal_entries_trigger
AFTER INSERT OR UPDATE OR DELETE ON journal_entries
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_journal_entry_details_trigger
AFTER INSERT OR UPDATE OR DELETE ON journal_entry_details
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_chart_of_accounts_trigger
AFTER INSERT OR UPDATE OR DELETE ON chart_of_accounts
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

-- Create a function to create journal entries for transactions
CREATE OR REPLACE FUNCTION create_journal_entry(
    p_transaction_id UUID,
    p_created_by UUID
)
RETURNS UUID AS $$
DECLARE
    v_transaction RECORD;
    v_journal_entry_id UUID;
BEGIN
    -- Get transaction details
    SELECT * INTO v_transaction FROM transactions WHERE id = p_transaction_id;
    
    -- Create journal entry
    INSERT INTO journal_entries (
        transaction_id,
        entry_date,
        description,
        created_by,
        created_at
    ) VALUES (
        p_transaction_id,
        v_transaction.date,
        CASE 
            WHEN v_transaction.type = 'Disbursement' THEN 'Loan disbursement'
            WHEN v_transaction.type = 'Repayment' THEN 'Loan repayment'
            WHEN v_transaction.type = 'Fee' THEN 'Fee collection'
            WHEN v_transaction.type = 'Penalty' THEN 'Penalty collection'
        END,
        p_created_by,
        CURRENT_TIMESTAMP
    ) RETURNING id INTO v_journal_entry_id;
    
    -- Create journal entry details based on transaction type
    IF v_transaction.type = 'Disbursement' THEN
        -- Debit: Loans Receivable
        INSERT INTO journal_entry_details (
            journal_entry_id, account_name, is_debit, amount, created_by, created_at
        ) VALUES (
            v_journal_entry_id, 'Loans Receivable', TRUE, v_transaction.amount, p_created_by, CURRENT_TIMESTAMP
        );
        
        -- Credit: Cash
        INSERT INTO journal_entry_details (
            journal_entry_id, account_name, is_debit, amount, created_by, created_at
        ) VALUES (
            v_journal_entry_id, 'Cash', FALSE, v_transaction.amount, p_created_by, CURRENT_TIMESTAMP
        );
    ELSIF v_transaction.type = 'Repayment' THEN
        -- Get loan details to calculate interest portion
        DECLARE
            v_loan RECORD;
            v_interest_portion DECIMAL;
            v_principal_portion DECIMAL;
        BEGIN
            SELECT * INTO v_loan FROM loans WHERE id = v_transaction.loan_id;
            
            -- Simplified calculation - in a real system, you would use the payment schedule
            v_interest_portion := v_transaction.amount * 0.3; -- Assuming 30% of payment is interest
            v_principal_portion := v_transaction.amount - v_interest_portion;
            
            -- Debit: Cash
            INSERT INTO journal_entry_details (
                journal_entry_id, account_name, is_debit, amount, created_by, created_at
            ) VALUES (
                v_journal_entry_id, 'Cash', TRUE, v_transaction.amount, p_created_by, CURRENT_TIMESTAMP
            );
            
            -- Credit: Loans Receivable
            INSERT INTO journal_entry_details (
                journal_entry_id, account_name, is_debit, amount, created_by, created_at
            ) VALUES (
                v_journal_entry_id, 'Loans Receivable', FALSE, v_principal_portion, p_created_by, CURRENT_TIMESTAMP
            );
            
            -- Credit: Interest Income
            INSERT INTO journal_entry_details (
                journal_entry_id, account_name, is_debit, amount, created_by, created_at
            ) VALUES (
                v_journal_entry_id, 'Interest Income', FALSE, v_interest_portion, p_created_by, CURRENT_TIMESTAMP
            );
        END;
    ELSIF v_transaction.type = 'Fee' THEN
        -- Debit: Cash
        INSERT INTO journal_entry_details (
            journal_entry_id, account_name, is_debit, amount, created_by, created_at
        ) VALUES (
            v_journal_entry_id, 'Cash', TRUE, v_transaction.amount, p_created_by, CURRENT_TIMESTAMP
        );
        
        -- Credit: Fee Income
        INSERT INTO journal_entry_details (
            journal_entry_id, account_name, is_debit, amount, created_by, created_at
        ) VALUES (
            v_journal_entry_id, 'Fee Income', FALSE, v_transaction.amount, p_created_by, CURRENT_TIMESTAMP
        );
    ELSIF v_transaction.type = 'Penalty' THEN
        -- Debit: Cash
        INSERT INTO journal_entry_details (
            journal_entry_id, account_name, is_debit, amount, created_by, created_at
        ) VALUES (
            v_journal_entry_id, 'Cash', TRUE, v_transaction.amount, p_created_by, CURRENT_TIMESTAMP
        );
        
        -- Credit: Penalty Income
        INSERT INTO journal_entry_details (
            journal_entry_id, account_name, is_debit, amount, created_by, created_at
        ) VALUES (
            v_journal_entry_id, 'Penalty Income', FALSE, v_transaction.amount, p_created_by, CURRENT_TIMESTAMP
        );
    END IF;
    
    RETURN v_journal_entry_id;
END;
$$ LANGUAGE plpgsql;

-- Insert initial chart of accounts
INSERT INTO chart_of_accounts (account_code, account_name, account_type, created_at)
VALUES
    ('1000', 'Cash', 'Asset', CURRENT_TIMESTAMP),
    ('1100', 'Loans Receivable', 'Asset', CURRENT_TIMESTAMP),
    ('1200', 'Interest Receivable', 'Asset', CURRENT_TIMESTAMP),
    ('2000', 'Accounts Payable', 'Liability', CURRENT_TIMESTAMP),
    ('3000', 'Capital', 'Equity', CURRENT_TIMESTAMP),
    ('4000', 'Interest Income', 'Income', CURRENT_TIMESTAMP),
    ('4100', 'Fee Income', 'Income', CURRENT_TIMESTAMP),
    ('4200', 'Penalty Income', 'Income', CURRENT_TIMESTAMP),
    ('5000', 'Salaries Expense', 'Expense', CURRENT_TIMESTAMP),
    ('5100', 'Rent Expense', 'Expense', CURRENT_TIMESTAMP),
    ('5200', 'Utilities Expense', 'Expense', CURRENT_TIMESTAMP),
    ('5300', 'Bad Debt Expense', 'Expense', CURRENT_TIMESTAMP);