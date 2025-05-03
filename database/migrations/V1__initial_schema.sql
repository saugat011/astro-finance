-- AstroFinance PostgreSQL Database Schema - Initial Version
-- Migration V1: Initial Schema

-- Enable UUID extension for GUID support
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create custom types for enums
CREATE TYPE loan_status AS ENUM ('Pending', 'Active', 'Completed', 'Defaulted');
CREATE TYPE loan_type AS ENUM ('Flat', 'Diminishing');
CREATE TYPE transaction_type AS ENUM ('Disbursement', 'Repayment', 'Fee', 'Penalty');
CREATE TYPE sms_status AS ENUM ('Pending', 'Sent', 'Failed');

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) NOT NULL UNIQUE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    last_login_date TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    last_modified_at TIMESTAMP,
    last_modified_by UUID
);

-- Create index on email for faster login
CREATE INDEX idx_users_email ON users(email);

-- Customers table
CREATE TABLE customers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255),
    phone_number VARCHAR(20) NOT NULL,
    address TEXT,
    identification_number VARCHAR(50) NOT NULL,
    identification_type VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES users(id),
    last_modified_at TIMESTAMP,
    last_modified_by UUID REFERENCES users(id)
);

-- Create indexes for customer search
CREATE INDEX idx_customers_name ON customers(first_name, last_name);
CREATE INDEX idx_customers_phone ON customers(phone_number);
CREATE INDEX idx_customers_id_number ON customers(identification_number);

-- Loans table
CREATE TABLE loans (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id UUID NOT NULL REFERENCES customers(id),
    amount DECIMAL(15, 2) NOT NULL,
    interest_rate DECIMAL(5, 2) NOT NULL,
    term INTEGER NOT NULL, -- In months
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    status loan_status NOT NULL DEFAULT 'Pending',
    type loan_type NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES users(id),
    last_modified_at TIMESTAMP,
    last_modified_by UUID REFERENCES users(id)
);

-- Create indexes for loan queries
CREATE INDEX idx_loans_customer ON loans(customer_id);
CREATE INDEX idx_loans_status ON loans(status);
CREATE INDEX idx_loans_dates ON loans(start_date, end_date);

-- Transactions table
CREATE TABLE transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    loan_id UUID REFERENCES loans(id),
    customer_id UUID NOT NULL REFERENCES customers(id),
    amount DECIMAL(15, 2) NOT NULL,
    type transaction_type NOT NULL,
    description TEXT,
    date TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES users(id),
    last_modified_at TIMESTAMP,
    last_modified_by UUID REFERENCES users(id)
);

-- Create indexes for transaction queries
CREATE INDEX idx_transactions_loan ON transactions(loan_id);
CREATE INDEX idx_transactions_customer ON transactions(customer_id);
CREATE INDEX idx_transactions_date ON transactions(date);
CREATE INDEX idx_transactions_type ON transactions(type);

-- SMS Templates table
CREATE TABLE sms_templates (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL UNIQUE,
    content TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES users(id),
    last_modified_at TIMESTAMP,
    last_modified_by UUID REFERENCES users(id)
);

-- SMS History table
CREATE TABLE sms_histories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    recipient_number VARCHAR(20) NOT NULL,
    recipient_name VARCHAR(200),
    message TEXT NOT NULL,
    status sms_status NOT NULL DEFAULT 'Pending',
    sent_at TIMESTAMP NOT NULL,
    error_message TEXT,
    sms_template_id UUID REFERENCES sms_templates(id),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES users(id),
    last_modified_at TIMESTAMP,
    last_modified_by UUID REFERENCES users(id)
);

-- Create indexes for SMS queries
CREATE INDEX idx_sms_histories_recipient ON sms_histories(recipient_number);
CREATE INDEX idx_sms_histories_status ON sms_histories(status);
CREATE INDEX idx_sms_histories_sent_at ON sms_histories(sent_at);

-- Payment Schedule table (for loan repayment schedules)
CREATE TABLE payment_schedules (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    loan_id UUID NOT NULL REFERENCES loans(id),
    due_date DATE NOT NULL,
    amount DECIMAL(15, 2) NOT NULL,
    principal_amount DECIMAL(15, 2) NOT NULL,
    interest_amount DECIMAL(15, 2) NOT NULL,
    is_paid BOOLEAN NOT NULL DEFAULT FALSE,
    payment_date TIMESTAMP,
    transaction_id UUID REFERENCES transactions(id),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES users(id),
    last_modified_at TIMESTAMP,
    last_modified_by UUID REFERENCES users(id)
);

-- Create indexes for payment schedule queries
CREATE INDEX idx_payment_schedules_loan ON payment_schedules(loan_id);
CREATE INDEX idx_payment_schedules_due_date ON payment_schedules(due_date);
CREATE INDEX idx_payment_schedules_is_paid ON payment_schedules(is_paid);

-- Audit Log table (for tracking all changes)
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    table_name VARCHAR(100) NOT NULL,
    record_id UUID NOT NULL,
    action VARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    old_values JSONB,
    new_values JSONB,
    changed_by UUID REFERENCES users(id),
    changed_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for audit logs
CREATE INDEX idx_audit_logs_table ON audit_logs(table_name);
CREATE INDEX idx_audit_logs_record ON audit_logs(record_id);
CREATE INDEX idx_audit_logs_action ON audit_logs(action);
CREATE INDEX idx_audit_logs_changed_at ON audit_logs(changed_at);

-- Create a function to automatically update last_modified_at
CREATE OR REPLACE FUNCTION update_last_modified_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.last_modified_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create triggers to automatically update last_modified_at
CREATE TRIGGER update_users_last_modified
BEFORE UPDATE ON users
FOR EACH ROW EXECUTE FUNCTION update_last_modified_column();

CREATE TRIGGER update_customers_last_modified
BEFORE UPDATE ON customers
FOR EACH ROW EXECUTE FUNCTION update_last_modified_column();

CREATE TRIGGER update_loans_last_modified
BEFORE UPDATE ON loans
FOR EACH ROW EXECUTE FUNCTION update_last_modified_column();

CREATE TRIGGER update_transactions_last_modified
BEFORE UPDATE ON transactions
FOR EACH ROW EXECUTE FUNCTION update_last_modified_column();

CREATE TRIGGER update_sms_templates_last_modified
BEFORE UPDATE ON sms_templates
FOR EACH ROW EXECUTE FUNCTION update_last_modified_column();

CREATE TRIGGER update_sms_histories_last_modified
BEFORE UPDATE ON sms_histories
FOR EACH ROW EXECUTE FUNCTION update_last_modified_column();

CREATE TRIGGER update_payment_schedules_last_modified
BEFORE UPDATE ON payment_schedules
FOR EACH ROW EXECUTE FUNCTION update_last_modified_column();

-- Create audit trigger function
CREATE OR REPLACE FUNCTION audit_trigger_function()
RETURNS TRIGGER AS $$
DECLARE
    old_row JSONB := NULL;
    new_row JSONB := NULL;
BEGIN
    IF (TG_OP = 'UPDATE' OR TG_OP = 'DELETE') THEN
        old_row = row_to_json(OLD)::JSONB;
    END IF;
    
    IF (TG_OP = 'INSERT' OR TG_OP = 'UPDATE') THEN
        new_row = row_to_json(NEW)::JSONB;
    END IF;
    
    INSERT INTO audit_logs (
        table_name,
        record_id,
        action,
        old_values,
        new_values,
        changed_by,
        changed_at
    ) VALUES (
        TG_TABLE_NAME,
        CASE 
            WHEN TG_OP = 'DELETE' THEN OLD.id
            ELSE NEW.id
        END,
        TG_OP,
        old_row,
        new_row,
        CASE 
            WHEN TG_OP = 'DELETE' THEN OLD.last_modified_by
            ELSE NEW.last_modified_by
        END,
        CURRENT_TIMESTAMP
    );
    
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Create audit triggers for all tables
CREATE TRIGGER audit_users_trigger
AFTER INSERT OR UPDATE OR DELETE ON users
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_customers_trigger
AFTER INSERT OR UPDATE OR DELETE ON customers
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_loans_trigger
AFTER INSERT OR UPDATE OR DELETE ON loans
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_transactions_trigger
AFTER INSERT OR UPDATE OR DELETE ON transactions
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_sms_templates_trigger
AFTER INSERT OR UPDATE OR DELETE ON sms_templates
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_sms_histories_trigger
AFTER INSERT OR UPDATE OR DELETE ON sms_histories
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_payment_schedules_trigger
AFTER INSERT OR UPDATE OR DELETE ON payment_schedules
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

-- Create views for reporting
CREATE VIEW vw_loan_summary AS
SELECT 
    l.id AS loan_id,
    c.id AS customer_id,
    c.first_name || ' ' || c.last_name AS customer_name,
    l.amount AS loan_amount,
    l.interest_rate,
    l.term,
    l.start_date,
    l.end_date,
    l.status,
    l.type,
    COALESCE(SUM(CASE WHEN t.type = 'Repayment' THEN t.amount ELSE 0 END), 0) AS total_repaid,
    l.amount - COALESCE(SUM(CASE WHEN t.type = 'Repayment' THEN t.amount ELSE 0 END), 0) AS outstanding_balance
FROM 
    loans l
JOIN 
    customers c ON l.customer_id = c.id
LEFT JOIN 
    transactions t ON l.id = t.loan_id
GROUP BY 
    l.id, c.id, c.first_name, c.last_name;

CREATE VIEW vw_overdue_loans AS
SELECT 
    l.id AS loan_id,
    c.id AS customer_id,
    c.first_name || ' ' || c.last_name AS customer_name,
    c.phone_number,
    ps.due_date,
    ps.amount AS installment_amount,
    CURRENT_DATE - ps.due_date AS days_overdue
FROM 
    payment_schedules ps
JOIN 
    loans l ON ps.loan_id = l.id
JOIN 
    customers c ON l.customer_id = c.id
WHERE 
    ps.is_paid = FALSE 
    AND ps.due_date < CURRENT_DATE
    AND l.status = 'Active'
ORDER BY 
    ps.due_date;

-- Create a function to generate payment schedule for a loan
CREATE OR REPLACE FUNCTION generate_payment_schedule(
    p_loan_id UUID,
    p_created_by UUID
)
RETURNS VOID AS $$
DECLARE
    v_loan RECORD;
    v_monthly_rate DECIMAL;
    v_emi DECIMAL;
    v_remaining_principal DECIMAL;
    v_interest_payment DECIMAL;
    v_principal_payment DECIMAL;
    v_payment_date DATE;
    v_month INTEGER;
BEGIN
    -- Get loan details
    SELECT * INTO v_loan FROM loans WHERE id = p_loan_id;
    
    IF v_loan.type = 'Flat' THEN
        -- For flat rate loans
        v_monthly_rate := v_loan.interest_rate / 100 / 12;
        v_emi := v_loan.amount / v_loan.term + (v_loan.amount * v_monthly_rate);
        
        FOR v_month IN 1..v_loan.term LOOP
            v_payment_date := v_loan.start_date + (v_month * INTERVAL '1 month');
            v_principal_payment := v_loan.amount / v_loan.term;
            v_interest_payment := v_loan.amount * v_monthly_rate;
            
            INSERT INTO payment_schedules (
                loan_id,
                due_date,
                amount,
                principal_amount,
                interest_amount,
                is_paid,
                created_by,
                created_at
            ) VALUES (
                p_loan_id,
                v_payment_date,
                v_emi,
                v_principal_payment,
                v_interest_payment,
                FALSE,
                p_created_by,
                CURRENT_TIMESTAMP
            );
        END LOOP;
    ELSE
        -- For diminishing rate loans
        v_monthly_rate := v_loan.interest_rate / 100 / 12;
        v_emi := v_loan.amount * v_monthly_rate * POWER(1 + v_monthly_rate, v_loan.term) / 
                (POWER(1 + v_monthly_rate, v_loan.term) - 1);
        v_remaining_principal := v_loan.amount;
        
        FOR v_month IN 1..v_loan.term LOOP
            v_payment_date := v_loan.start_date + (v_month * INTERVAL '1 month');
            v_interest_payment := v_remaining_principal * v_monthly_rate;
            v_principal_payment := v_emi - v_interest_payment;
            v_remaining_principal := v_remaining_principal - v_principal_payment;
            
            INSERT INTO payment_schedules (
                loan_id,
                due_date,
                amount,
                principal_amount,
                interest_amount,
                is_paid,
                created_by,
                created_at
            ) VALUES (
                p_loan_id,
                v_payment_date,
                v_emi,
                v_principal_payment,
                v_interest_payment,
                FALSE,
                p_created_by,
                CURRENT_TIMESTAMP
            );
        END LOOP;
    END IF;
END;
$$ LANGUAGE plpgsql;