# AstroFinance Database Architecture

This document outlines the database architecture for the AstroFinance microfinance application using PostgreSQL.

## Overview

The database schema is designed to support all the core functionalities of a microfinance application, including:

- Customer management
- Loan processing and management
- Transaction recording
- Payment scheduling
- SMS notifications
- Double-entry accounting
- Reporting and analytics
- Audit logging

## Entity Relationship Diagram

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│    Users    │     │  Customers  │     │    Loans    │
├─────────────┤     ├─────────────┤     ├─────────────┤
│ id          │     │ id          │     │ id          │
│ email       │     │ first_name  │     │ customer_id │
│ first_name  │     │ last_name   │     │ amount      │
│ last_name   │     │ email       │     │ interest_rate│
│ password_hash│    │ phone_number│     │ term        │
│ role        │     │ address     │     │ start_date  │
│ is_active   │     │ id_number   │     │ end_date    │
│ last_login  │     │ id_type     │     │ status      │
└─────────────┘     └─────────────┘     │ type        │
       │                   │            └─────────────┘
       │                   │                   │
       │                   │                   │
       │                   │                   │
       ▼                   ▼                   ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ Transactions│     │   Payment   │     │ SMS History │
├─────────────┤     │  Schedules  │     ├─────────────┤
│ id          │     ├─────────────┤     │ id          │
│ loan_id     │     │ id          │     │ recipient   │
│ customer_id │     │ loan_id     │     │ message     │
│ amount      │     │ due_date    │     │ status      │
│ type        │     │ amount      │     │ sent_at     │
│ description │     │ principal   │     │ template_id │
│ date        │     │ interest    │     └─────────────┘
└─────────────┘     │ is_paid     │
       │            └─────────────┘
       │                   │
       │                   │
       ▼                   │
┌─────────────┐            │
│   Journal   │            │
│   Entries   │◄───────────┘
├─────────────┤
│ id          │
│ transaction_id│
│ entry_date  │
│ description │
└─────────────┘
       │
       │
       ▼
┌─────────────┐
│Journal Entry│
│   Details   │
├─────────────┤
│ id          │
│ journal_id  │
│ account     │
│ is_debit    │
│ amount      │
└─────────────┘
```

## Key Design Decisions

1. **UUID Primary Keys**: Using UUIDs instead of sequential integers for better scalability and security.

2. **Audit Trails**: All tables include audit fields (created_at, created_by, last_modified_at, last_modified_by) for tracking changes.

3. **Comprehensive Audit Logging**: A separate audit_logs table captures all changes to the database with old and new values.

4. **PostgreSQL Enums**: Using custom enum types for status fields to ensure data integrity.

5. **Double-Entry Accounting**: Implemented journal entries and journal entry details tables to support proper accounting practices.

6. **Optimized Indexes**: Strategic indexes on frequently queried columns to improve performance.

7. **Views for Reporting**: Pre-defined views and materialized views for common reports.

8. **Stored Procedures**: Functions for common operations like generating payment schedules and recording payments.

9. **Triggers**: Automatic updating of audit fields and creation of audit log entries.

10. **Data Integrity**: Foreign key constraints to maintain referential integrity.

## Tables

### Core Tables

1. **users**: Stores user information for authentication and authorization
2. **customers**: Stores customer information for microfinance clients
3. **loans**: Stores loan information including amount, interest rate, and status
4. **transactions**: Records all financial transactions related to loans
5. **payment_schedules**: Stores the repayment schedule for each loan

### Supporting Tables

6. **sms_templates**: Stores templates for SMS notifications
7. **sms_histories**: Records history of all SMS notifications sent
8. **journal_entries**: Stores accounting journal entries
9. **journal_entry_details**: Stores the details of each journal entry for double-entry accounting
10. **chart_of_accounts**: Defines the chart of accounts for the accounting system
11. **audit_logs**: Records all changes to the database for auditing purposes

## Views

1. **vw_loan_summary**: Provides a summary of each loan with customer information and repayment status
2. **vw_overdue_loans**: Shows all overdue loans with customer contact information
3. **vw_daily_transactions**: Summarizes transactions by date and type
4. **mvw_monthly_financial_summary**: Materialized view for monthly financial reports

## Functions

1. **generate_payment_schedule**: Generates a payment schedule for a loan
2. **record_loan_payment**: Records a loan payment and updates payment schedules
3. **create_journal_entry**: Creates journal entries for transactions
4. **send_overdue_loan_notifications**: Sends SMS notifications for overdue loans
5. **mark_defaulted_loans**: Marks loans as defaulted after a specified period
6. **calculate_par**: Calculates the Portfolio at Risk (PAR) ratio
7. **backup_database**: Creates a database backup
8. **perform_database_maintenance**: Performs routine database maintenance
9. **generate_database_documentation**: Generates documentation for the database schema

## Indexing Strategy

The database uses a comprehensive indexing strategy to optimize query performance:

1. **Primary Keys**: All tables have UUID primary keys
2. **Foreign Keys**: Indexed for faster joins
3. **Search Fields**: Indexed for faster searching (e.g., customer name, phone number)
4. **Date Fields**: Indexed for date range queries
5. **Status Fields**: Indexed for filtering by status
6. **Composite Indexes**: Used for frequently combined query conditions

## Security Considerations

1. **Password Storage**: Passwords are stored as hashes, never in plain text
2. **Audit Logging**: All changes are logged for security auditing
3. **Role-Based Access**: User roles control access to different parts of the system
4. **Data Validation**: Constraints and checks ensure data integrity

## Backup and Recovery

The database includes a backup function that can be scheduled to run regularly. The backup strategy includes:

1. **Full Database Backups**: Regular full database backups
2. **Point-in-Time Recovery**: Using PostgreSQL's WAL (Write-Ahead Logging)
3. **Backup Verification**: Procedures to verify backup integrity
4. **Retention Policy**: Guidelines for backup retention

## Performance Optimization

1. **Materialized Views**: For complex reports that don't need real-time data
2. **Strategic Indexing**: Indexes on frequently queried columns
3. **Regular Maintenance**: VACUUM and ANALYZE to maintain performance
4. **Query Optimization**: Optimized queries for common operations

## Scaling Considerations

1. **Connection Pooling**: Recommended for handling multiple connections
2. **Partitioning**: Tables like transactions and audit_logs can be partitioned by date
3. **Read Replicas**: Can be set up for reporting queries
4. **Vertical Scaling**: Guidelines for resource allocation

## Installation and Setup

### Environment Configuration

1. Copy the example environment file:
   ```bash
   cp .env.example .env
   ```

2. Edit the `.env` file with your database credentials:
   ```bash
   # Update these values with your PostgreSQL credentials
   DB_HOST=localhost
   DB_PORT=5432
   DB_NAME=astrofinance
   DB_USER=postgres
   DB_PASSWORD=your_password_here
   ```

### Database Setup

1. Create a PostgreSQL database using the migration script:
   ```bash
   ./migrate.sh --create
   ```

2. Seed the database with initial data:
   ```bash
   ./seed.sh
   ```

3. Verify installation:
   ```bash
   psql -U $DB_USER -h $DB_HOST -d $DB_NAME -c "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';"
   ```

## Maintenance Tasks

1. **Regular Backups**: Schedule daily backups using the backup_database function
2. **Database Maintenance**: Run perform_database_maintenance weekly
3. **Index Optimization**: Monitor and rebuild indexes as needed
4. **Statistics Update**: Ensure ANALYZE is run regularly

## Troubleshooting

1. **Performance Issues**: Check query plans with EXPLAIN ANALYZE
2. **Space Issues**: Monitor table and index sizes
3. **Connection Issues**: Check max_connections setting
4. **Lock Contention**: Monitor for long-running transactions

## Best Practices for Development

1. **Use Prepared Statements**: To prevent SQL injection
2. **Transactions**: Use transactions for operations that modify multiple tables
3. **Connection Pooling**: Use connection pooling in application code
4. **Error Handling**: Implement proper error handling for database operations

## Conclusion

This database schema provides a solid foundation for the AstroFinance microfinance application. It is designed to be secure, performant, and maintainable, with comprehensive support for all the required functionality.