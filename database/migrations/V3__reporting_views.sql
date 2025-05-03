-- AstroFinance PostgreSQL Database Schema - Reporting Views and Functions
-- Migration V3: Adding Reporting Views and Functions

-- Create view for daily transactions
CREATE VIEW vw_daily_transactions AS
SELECT 
    DATE(t.date) AS transaction_date,
    t.type,
    COUNT(*) AS transaction_count,
    SUM(t.amount) AS total_amount
FROM 
    transactions t
GROUP BY 
    DATE(t.date), t.type
ORDER BY 
    DATE(t.date) DESC, t.type;

-- Create materialized view for monthly financial reports
CREATE MATERIALIZED VIEW mvw_monthly_financial_summary AS
SELECT 
    DATE_TRUNC('month', t.date) AS month,
    SUM(CASE WHEN t.type = 'Disbursement' THEN t.amount ELSE 0 END) AS total_disbursements,
    SUM(CASE WHEN t.type = 'Repayment' THEN t.amount ELSE 0 END) AS total_repayments,
    SUM(CASE WHEN t.type = 'Fee' THEN t.amount ELSE 0 END) AS total_fees,
    SUM(CASE WHEN t.type = 'Penalty' THEN t.amount ELSE 0 END) AS total_penalties,
    COUNT(DISTINCT CASE WHEN t.type = 'Disbursement' THEN t.loan_id END) AS new_loans_count,
    COUNT(DISTINCT CASE WHEN l.status = 'Completed' AND l.last_modified_at >= DATE_TRUNC('month', t.date) 
                        AND l.last_modified_at < DATE_TRUNC('month', t.date) + INTERVAL '1 month' 
                        THEN l.id END) AS completed_loans_count
FROM 
    transactions t
LEFT JOIN 
    loans l ON t.loan_id = l.id
WHERE 
    t.date >= DATE_TRUNC('month', CURRENT_DATE - INTERVAL '12 months')
GROUP BY 
    DATE_TRUNC('month', t.date)
ORDER BY 
    DATE_TRUNC('month', t.date) DESC;

-- Create index on the materialized view
CREATE UNIQUE INDEX idx_mvw_monthly_financial_summary ON mvw_monthly_financial_summary(month);

-- Create a function to refresh the materialized view
CREATE OR REPLACE FUNCTION refresh_monthly_financial_summary()
RETURNS VOID AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY mvw_monthly_financial_summary;
END;
$$ LANGUAGE plpgsql;

-- Create a function to record a loan repayment
CREATE OR REPLACE FUNCTION record_loan_payment(
    p_loan_id UUID,
    p_amount DECIMAL,
    p_payment_date TIMESTAMP,
    p_created_by UUID
)
RETURNS UUID AS $$
DECLARE
    v_customer_id UUID;
    v_transaction_id UUID;
    v_remaining_amount DECIMAL;
    v_payment_schedule RECORD;
BEGIN
    -- Get customer ID from loan
    SELECT customer_id INTO v_customer_id FROM loans WHERE id = p_loan_id;
    
    -- Create transaction record
    INSERT INTO transactions (
        loan_id,
        customer_id,
        amount,
        type,
        description,
        date,
        created_by,
        created_at
    ) VALUES (
        p_loan_id,
        v_customer_id,
        p_amount,
        'Repayment',
        'Loan repayment',
        p_payment_date,
        p_created_by,
        CURRENT_TIMESTAMP
    ) RETURNING id INTO v_transaction_id;
    
    -- Apply payment to unpaid schedules
    v_remaining_amount := p_amount;
    
    FOR v_payment_schedule IN 
        SELECT * FROM payment_schedules 
        WHERE loan_id = p_loan_id AND is_paid = FALSE 
        ORDER BY due_date
    LOOP
        IF v_remaining_amount >= v_payment_schedule.amount THEN
            -- Full payment
            UPDATE payment_schedules 
            SET is_paid = TRUE, 
                payment_date = p_payment_date, 
                transaction_id = v_transaction_id,
                last_modified_by = p_created_by,
                last_modified_at = CURRENT_TIMESTAMP
            WHERE id = v_payment_schedule.id;
            
            v_remaining_amount := v_remaining_amount - v_payment_schedule.amount;
        ELSIF v_remaining_amount > 0 THEN
            -- Partial payment (not implemented in this simplified version)
            -- In a real system, you would track partial payments
            EXIT;
        ELSE
            EXIT;
        END IF;
    END LOOP;
    
    -- Check if loan is fully paid
    IF NOT EXISTS (SELECT 1 FROM payment_schedules WHERE loan_id = p_loan_id AND is_paid = FALSE) THEN
        UPDATE loans 
        SET status = 'Completed', 
            last_modified_by = p_created_by,
            last_modified_at = CURRENT_TIMESTAMP
        WHERE id = p_loan_id;
    END IF;
    
    RETURN v_transaction_id;
END;
$$ LANGUAGE plpgsql;

-- Create a function to send SMS notifications for overdue loans
CREATE OR REPLACE FUNCTION send_overdue_loan_notifications(
    p_days_overdue INTEGER,
    p_created_by UUID
)
RETURNS INTEGER AS $$
DECLARE
    v_overdue_loan RECORD;
    v_template_id UUID;
    v_message TEXT;
    v_count INTEGER := 0;
BEGIN
    -- Get SMS template for overdue loans
    SELECT id INTO v_template_id FROM sms_templates WHERE name = 'Overdue Loan Reminder' LIMIT 1;
    
    -- If template doesn't exist, create one
    IF v_template_id IS NULL THEN
        INSERT INTO sms_templates (
            name,
            content,
            created_by,
            created_at
        ) VALUES (
            'Overdue Loan Reminder',
            'Dear {CustomerName}, your loan payment of {Amount} was due on {DueDate}. Please make payment as soon as possible to avoid additional penalties.',
            p_created_by,
            CURRENT_TIMESTAMP
        ) RETURNING id INTO v_template_id;
    END IF;
    
    -- Get template content
    SELECT content INTO v_message FROM sms_templates WHERE id = v_template_id;
    
    -- Process overdue loans
    FOR v_overdue_loan IN 
        SELECT 
            c.id AS customer_id,
            c.first_name || ' ' || c.last_name AS customer_name,
            c.phone_number,
            ps.due_date,
            ps.amount,
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
            AND CURRENT_DATE - ps.due_date = p_days_overdue
            AND l.status = 'Active'
    LOOP
        -- Replace placeholders in message
        v_message := REPLACE(v_message, '{CustomerName}', v_overdue_loan.customer_name);
        v_message := REPLACE(v_message, '{Amount}', v_overdue_loan.amount::TEXT);
        v_message := REPLACE(v_message, '{DueDate}', v_overdue_loan.due_date::TEXT);
        
        -- Create SMS history record
        INSERT INTO sms_histories (
            recipient_number,
            recipient_name,
            message,
            status,
            sent_at,
            sms_template_id,
            created_by,
            created_at
        ) VALUES (
            v_overdue_loan.phone_number,
            v_overdue_loan.customer_name,
            v_message,
            'Pending',
            CURRENT_TIMESTAMP,
            v_template_id,
            p_created_by,
            CURRENT_TIMESTAMP
        );
        
        v_count := v_count + 1;
    END LOOP;
    
    RETURN v_count;
END;
$$ LANGUAGE plpgsql;

-- Create a function to mark defaulted loans
CREATE OR REPLACE FUNCTION mark_defaulted_loans(
    p_days_threshold INTEGER,
    p_created_by UUID
)
RETURNS INTEGER AS $$
DECLARE
    v_count INTEGER := 0;
BEGIN
    UPDATE loans
    SET 
        status = 'Defaulted',
        last_modified_by = p_created_by,
        last_modified_at = CURRENT_TIMESTAMP
    WHERE 
        id IN (
            SELECT DISTINCT l.id
            FROM loans l
            JOIN payment_schedules ps ON l.id = ps.loan_id
            WHERE 
                l.status = 'Active'
                AND ps.is_paid = FALSE
                AND CURRENT_DATE - ps.due_date > p_days_threshold
        );
    
    GET DIAGNOSTICS v_count = ROW_COUNT;
    RETURN v_count;
END;
$$ LANGUAGE plpgsql;

-- Create a function to calculate portfolio at risk (PAR)
CREATE OR REPLACE FUNCTION calculate_par(
    p_days_threshold INTEGER
)
RETURNS TABLE (
    par_ratio DECIMAL,
    total_outstanding DECIMAL,
    at_risk_outstanding DECIMAL,
    loan_count INTEGER,
    at_risk_loan_count INTEGER
) AS $$
BEGIN
    RETURN QUERY
    WITH loan_summary AS (
        SELECT
            l.id,
            l.amount,
            COALESCE(SUM(CASE WHEN t.type = 'Repayment' THEN t.amount ELSE 0 END), 0) AS total_repaid,
            l.amount - COALESCE(SUM(CASE WHEN t.type = 'Repayment' THEN t.amount ELSE 0 END), 0) AS outstanding,
            CASE
                WHEN EXISTS (
                    SELECT 1 FROM payment_schedules ps
                    WHERE ps.loan_id = l.id
                    AND ps.is_paid = FALSE
                    AND CURRENT_DATE - ps.due_date > p_days_threshold
                ) THEN TRUE
                ELSE FALSE
            END AS is_at_risk
        FROM
            loans l
        LEFT JOIN
            transactions t ON l.id = t.loan_id
        WHERE
            l.status = 'Active'
        GROUP BY
            l.id, l.amount
    )
    SELECT
        CASE
            WHEN SUM(outstanding) = 0 THEN 0
            ELSE SUM(CASE WHEN is_at_risk THEN outstanding ELSE 0 END) / SUM(outstanding)
        END AS par_ratio,
        SUM(outstanding) AS total_outstanding,
        SUM(CASE WHEN is_at_risk THEN outstanding ELSE 0 END) AS at_risk_outstanding,
        COUNT(*) AS loan_count,
        SUM(CASE WHEN is_at_risk THEN 1 ELSE 0 END) AS at_risk_loan_count
    FROM
        loan_summary;
END;
$$ LANGUAGE plpgsql;

-- Create a database maintenance function
CREATE OR REPLACE FUNCTION perform_database_maintenance()
RETURNS TEXT AS $$
DECLARE
    v_result TEXT;
BEGIN
    -- Vacuum analyze all tables
    VACUUM ANALYZE;
    
    -- Refresh materialized views
    REFRESH MATERIALIZED VIEW CONCURRENTLY mvw_monthly_financial_summary;
    
    -- Reindex important indexes
    REINDEX INDEX idx_loans_customer;
    REINDEX INDEX idx_transactions_loan;
    REINDEX INDEX idx_payment_schedules_loan;
    
    v_result := 'Database maintenance completed at ' || CURRENT_TIMESTAMP;
    RETURN v_result;
END;
$$ LANGUAGE plpgsql;

-- Create a function to generate a database documentation
CREATE OR REPLACE FUNCTION generate_database_documentation()
RETURNS TABLE (
    table_name TEXT,
    column_name TEXT,
    data_type TEXT,
    is_nullable TEXT,
    column_description TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        c.table_name::TEXT,
        c.column_name::TEXT,
        c.data_type::TEXT,
        c.is_nullable::TEXT,
        pg_catalog.col_description(
            format('%I.%I', c.table_schema, c.table_name)::regclass::oid,
            c.ordinal_position
        )::TEXT AS column_description
    FROM
        information_schema.columns c
    WHERE
        c.table_schema = 'public'
    ORDER BY
        c.table_name,
        c.ordinal_position;
END;
$$ LANGUAGE plpgsql;

-- Add comments to tables and columns for documentation
COMMENT ON TABLE users IS 'Stores user information for authentication and authorization';
COMMENT ON TABLE customers IS 'Stores customer information for microfinance clients';
COMMENT ON TABLE loans IS 'Stores loan information including amount, interest rate, and status';
COMMENT ON TABLE transactions IS 'Records all financial transactions related to loans';
COMMENT ON TABLE payment_schedules IS 'Stores the repayment schedule for each loan';
COMMENT ON TABLE sms_templates IS 'Stores templates for SMS notifications';
COMMENT ON TABLE sms_histories IS 'Records history of all SMS notifications sent';
COMMENT ON TABLE journal_entries IS 'Stores accounting journal entries';
COMMENT ON TABLE journal_entry_details IS 'Stores the details of each journal entry for double-entry accounting';
COMMENT ON TABLE chart_of_accounts IS 'Defines the chart of accounts for the accounting system';
COMMENT ON TABLE audit_logs IS 'Records all changes to the database for auditing purposes';

-- Add comments to columns
COMMENT ON COLUMN loans.amount IS 'The principal amount of the loan';
COMMENT ON COLUMN loans.interest_rate IS 'Annual interest rate as a percentage';
COMMENT ON COLUMN loans.term IS 'Loan duration in months';
COMMENT ON COLUMN loans.type IS 'Type of loan: Flat or Diminishing interest calculation method';
COMMENT ON COLUMN transactions.amount IS 'The amount of the transaction';
COMMENT ON COLUMN transactions.type IS 'Type of transaction: Disbursement, Repayment, Fee, or Penalty';
COMMENT ON COLUMN payment_schedules.principal_amount IS 'The principal portion of the installment';
COMMENT ON COLUMN payment_schedules.interest_amount IS 'The interest portion of the installment';