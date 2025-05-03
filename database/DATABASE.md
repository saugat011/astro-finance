# AstroFinance Database Documentation

## Overview

The AstroFinance database is designed to support a microfinance application with the following key features:

- Customer management
- Loan processing and management
- Transaction recording
- Payment scheduling
- SMS notifications
- Double-entry accounting
- Reporting and analytics
- Audit logging

## Database Schema

The database schema consists of the following main tables:

### Core Tables

1. **users** - Stores user information for authentication and authorization
2. **customers** - Stores customer information for microfinance clients
3. **loans** - Stores loan information including amount, interest rate, and status
4. **transactions** - Records all financial transactions related to loans
5. **payment_schedules** - Stores the repayment schedule for each loan

### Supporting Tables

6. **sms_templates** - Stores templates for SMS notifications
7. **sms_histories** - Records history of all SMS notifications sent
8. **journal_entries** - Stores accounting journal entries
9. **journal_entry_details** - Stores the details of each journal entry for double-entry accounting
10. **chart_of_accounts** - Defines the chart of accounts for the accounting system
11. **audit_logs** - Records all changes to the database for auditing purposes

## Database Setup

### Prerequisites

- PostgreSQL 14 or higher
- PostgreSQL client tools (psql, pg_dump, pg_restore)

### Installation

1. Create a new PostgreSQL database:
   ```bash
   createdb -U postgres astrofinance
   ```

2. Run the migration script to set up the schema:
   ```bash
   ./migrate.sh --create
   ```

3. Seed the database with initial data:
   ```bash
   ./seed.sh
   ```

## Database Scripts

The following scripts are provided for database management:

### migrate.sh

This script applies database migrations in order.

```bash
Usage: ./migrate.sh [options]
Options:
  -h, --help       Show this help message
  -u, --user       Database user (default: postgres)
  -d, --database   Database name (default: astrofinance)
  -c, --create     Create database if it doesn't exist
  -r, --reset      Reset database (drop and recreate)
  -b, --backup     Backup database before migration
```

### seed.sh

This script seeds the database with initial data for development and testing.

```bash
Usage: ./seed.sh [options]
Options:
  -h, --help       Show this help message
  -u, --user       Database user (default: postgres)
  -d, --database   Database name (default: astrofinance)
```

### backup.sh

This script creates a backup of the database.

```bash
Usage: ./backup.sh [options]
Options:
  -h, --help           Show this help message
  -u, --user           Database user (default: postgres)
  -d, --database       Database name (default: astrofinance)
  -o, --output-dir     Backup directory (default: ./backups)
  -r, --retention-days Days to keep backups (default: 30)
  -c, --retention-count Maximum number of backups to keep (default: 10)
```

### restore.sh

This script restores a database from a backup.

```bash
Usage: ./restore.sh [options] backup_file
Options:
  -h, --help           Show this help message
  -u, --user           Database user (default: postgres)
  -d, --database       Database name (default: astrofinance)
  -c, --create         Create database if it doesn't exist
  -r, --reset          Reset database (drop and recreate)
  -l, --list           List available backups and exit

Example:
  ./restore.sh -r backups/astrofinance_20230101_120000.dump
  ./restore.sh --list
```

### maintenance.sh

This script performs routine maintenance on the database.

```bash
Usage: ./maintenance.sh [options]
Options:
  -h, --help           Show this help message
  -u, --user           Database user (default: postgres)
  -d, --database       Database name (default: astrofinance)
  -n, --no-backup      Skip backup before maintenance
```

## Database Functions

The database includes the following stored functions:

1. **generate_payment_schedule(loan_id, created_by)** - Generates a payment schedule for a loan
2. **record_loan_payment(loan_id, amount, payment_date, created_by)** - Records a loan payment and updates payment schedules
3. **create_journal_entry(transaction_id, created_by)** - Creates journal entries for transactions
4. **send_overdue_loan_notifications(days_overdue, created_by)** - Sends SMS notifications for overdue loans
5. **mark_defaulted_loans(days_threshold, created_by)** - Marks loans as defaulted after a specified period
6. **calculate_par(days_threshold)** - Calculates the Portfolio at Risk (PAR) ratio
7. **refresh_monthly_financial_summary()** - Refreshes the monthly financial summary materialized view
8. **perform_database_maintenance()** - Performs routine database maintenance

## Database Views

The database includes the following views for reporting:

1. **vw_loan_summary** - Provides a summary of each loan with customer information and repayment status
2. **vw_overdue_loans** - Shows all overdue loans with customer contact information
3. **vw_daily_transactions** - Summarizes transactions by date and type
4. **mvw_monthly_financial_summary** - Materialized view for monthly financial reports

## Database Configuration

### Environment Variables

The database configuration uses environment variables for flexibility and security. The following environment variables can be set:

#### Database Connection
- `DB_HOST` - Database host (default: localhost)
- `DB_PORT` - Database port (default: 5432)
- `DB_NAME` - Database name (default: astrofinance)
- `DB_USER` - Database user (default: postgres)
- `DB_PASSWORD` - Database password

#### Test Database Connection
- `TEST_DB_HOST` - Test database host
- `TEST_DB_PORT` - Test database port
- `TEST_DB_NAME` - Test database name
- `TEST_DB_USER` - Test database user
- `TEST_DB_PASSWORD` - Test database password

#### Production Database Connection
- `PROD_DB_HOST` - Production database host
- `PROD_DB_PORT` - Production database port
- `PROD_DB_NAME` - Production database name
- `PROD_DB_USER` - Production database user
- `PROD_DB_PASSWORD` - Production database password
- `PROD_DB_POOL_SIZE` - Connection pool size for production
- `PROD_DB_SSL` - Whether to use SSL for production connections

#### Backup Settings
- `BACKUP_ENABLED` - Whether backups are enabled
- `BACKUP_SCHEDULE` - Cron schedule for backups
- `BACKUP_RETENTION_DAYS` - Number of days to keep backups
- `BACKUP_RETENTION_COUNT` - Maximum number of backups to keep
- `BACKUP_PATH` - Path to store backups

#### Monitoring Settings
- `MONITORING_ENABLED` - Whether monitoring is enabled
- `SLOW_QUERY_THRESHOLD` - Threshold for slow queries in milliseconds
- `LOG_QUERIES` - Whether to log all queries
- `LOG_QUERY_PARAMS` - Whether to log query parameters

#### Maintenance Settings
- `BACKUP_BEFORE_MAINTENANCE` - Whether to backup before maintenance
- `VACUUM_ENABLED` - Whether vacuum is enabled
- `VACUUM_SCHEDULE` - Cron schedule for vacuum
- `REINDEX_ENABLED` - Whether reindexing is enabled
- `REINDEX_SCHEDULE` - Cron schedule for reindexing
- `REFRESH_VIEWS_ENABLED` - Whether refreshing views is enabled
- `REFRESH_VIEWS_SCHEDULE` - Cron schedule for refreshing views

### Configuration File

The database configuration is also stored in `database.config.json`, which reads values from environment variables with fallbacks to default values. This file contains settings for:

- Connection parameters for different environments (development, test, production)
- Backup settings
- Monitoring settings
- Maintenance settings

## Backup and Recovery

### Backup Strategy

The backup script (`backup.sh`) creates compressed backups of the database. The backup strategy includes:

1. **Regular Backups**: Schedule daily backups using cron
2. **Retention Policy**: Keep backups for a specified number of days and/or a maximum number of backups
3. **Backup Verification**: The backup script verifies the backup was created successfully

### Recovery Procedure

To recover from a backup:

1. Use the restore script: `./restore.sh -r backup_file`
2. Verify the restored database: `psql -U postgres -d astrofinance -c "SELECT COUNT(*) FROM users;"`

## Maintenance

### Routine Maintenance

The maintenance script (`maintenance.sh`) performs the following tasks:

1. **Vacuum Analyze**: Reclaims space and updates statistics
2. **Reindex**: Rebuilds indexes to improve performance
3. **Refresh Materialized Views**: Updates materialized views with the latest data
4. **Check for Defaulted Loans**: Marks loans as defaulted based on specified criteria
5. **Calculate Portfolio at Risk**: Updates the PAR ratio
6. **Database Statistics**: Collects and reports database statistics

### Scheduled Maintenance

Schedule the maintenance script to run weekly using cron:

```bash
0 2 * * 0 /path/to/astro-finance/database/maintenance.sh >> /path/to/astro-finance/database/logs/cron_maintenance.log 2>&1
```

## Security Considerations

1. **Password Storage**: Passwords are stored as hashes, never in plain text
2. **Audit Logging**: All changes are logged for security auditing
3. **Role-Based Access**: User roles control access to different parts of the system
4. **Connection Security**: Use SSL for database connections in production

## Performance Optimization

1. **Indexes**: Strategic indexes on frequently queried columns
2. **Materialized Views**: For complex reports that don't need real-time data
3. **Regular Maintenance**: VACUUM and ANALYZE to maintain performance
4. **Query Optimization**: Optimized queries for common operations

## Troubleshooting

### Common Issues

1. **Connection Issues**:
   - Check PostgreSQL is running: `pg_isready`
   - Verify connection parameters in `database.config.json`
   - Check PostgreSQL logs: `/var/log/postgresql/postgresql-14-main.log`

2. **Performance Issues**:
   - Run `EXPLAIN ANALYZE` on slow queries
   - Check for missing indexes
   - Run the maintenance script
   - Check for bloated tables and indexes

3. **Space Issues**:
   - Check database size: `SELECT pg_size_pretty(pg_database_size('astrofinance'));`
   - Check table sizes: `SELECT relname, pg_size_pretty(pg_total_relation_size(relid)) FROM pg_catalog.pg_statio_user_tables ORDER BY pg_total_relation_size(relid) DESC;`
   - Run VACUUM FULL on bloated tables

## Best Practices

1. **Regular Backups**: Schedule daily backups
2. **Monitoring**: Monitor database size, performance, and errors
3. **Maintenance**: Run the maintenance script weekly
4. **Testing**: Test backups by restoring to a test database
5. **Documentation**: Keep documentation up to date with schema changes

## Conclusion

This database schema provides a solid foundation for the AstroFinance microfinance application. It is designed to be secure, performant, and maintainable, with comprehensive support for all the required functionality.