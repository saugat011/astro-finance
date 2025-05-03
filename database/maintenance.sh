#!/bin/bash

# AstroFinance Database Maintenance Script
# This script performs routine maintenance on the database

# Load environment variables from .env file if it exists
if [ -f .env ]; then
    echo "Loading environment variables from .env file"
    export $(grep -v '^#' .env | xargs)
fi

# Configuration with environment variable support
DB_NAME=${DB_NAME:-astrofinance}
DB_USER=${DB_USER:-postgres}
DB_PASSWORD=${DB_PASSWORD:-}
DB_HOST=${DB_HOST:-localhost}
DB_PORT=${DB_PORT:-5432}
BACKUP_BEFORE_MAINTENANCE=${BACKUP_BEFORE_MAINTENANCE:-true}
LOG_DIR=${LOG_DIR:-./logs}
LOG_FILE="$LOG_DIR/maintenance_$(date +%Y%m%d_%H%M%S).log"

# Password argument for psql (empty if no password)
if [ -n "$DB_PASSWORD" ]; then
    PGPASSWORD="$DB_PASSWORD"
    export PGPASSWORD
fi

# Host and port arguments for psql
DB_HOST_ARG=""
if [ -n "$DB_HOST" ]; then
    DB_HOST_ARG="-h $DB_HOST"
fi

DB_PORT_ARG=""
if [ -n "$DB_PORT" ]; then
    DB_PORT_ARG="-p $DB_PORT"
fi

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# Function to display usage
function show_usage {
    echo "Usage: $0 [options]"
    echo "Options:"
    echo "  -h, --help           Show this help message"
    echo "  -u, --user           Database user (default: postgres)"
    echo "  -d, --database       Database name (default: astrofinance)"
    echo "  -n, --no-backup      Skip backup before maintenance"
    exit 1
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    key="$1"
    case $key in
        -h|--help)
            show_usage
            ;;
        -u|--user)
            DB_USER="$2"
            shift
            shift
            ;;
        -d|--database)
            DB_NAME="$2"
            shift
            shift
            ;;
        -n|--no-backup)
            BACKUP_BEFORE_MAINTENANCE=false
            shift
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            show_usage
            ;;
    esac
done

# Check if psql is installed
if ! command -v psql &> /dev/null; then
    echo -e "${RED}Error: PostgreSQL client (psql) is not installed.${NC}"
    exit 1
fi

# Function to check if database exists
function database_exists {
    psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -lqt | cut -d \| -f 1 | grep -qw "$DB_NAME"
}

# Check if database exists
if ! database_exists; then
    echo -e "${RED}Error: Database $DB_NAME does not exist.${NC}"
    exit 1
fi

# Create log directory if it doesn't exist
if [ ! -d "$LOG_DIR" ]; then
    echo -e "${YELLOW}Creating log directory $LOG_DIR...${NC}"
    mkdir -p "$LOG_DIR"
fi

# Start logging
exec > >(tee -a "$LOG_FILE") 2>&1
echo "=== AstroFinance Database Maintenance - $(date) ==="

# Backup before maintenance if enabled
if [ "$BACKUP_BEFORE_MAINTENANCE" = true ]; then
    echo -e "${YELLOW}Creating backup before maintenance...${NC}"
    ./backup.sh -d "$DB_NAME" -u "$DB_USER"
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Backup failed. Aborting maintenance.${NC}"
        exit 1
    fi
fi

# Perform database maintenance
echo -e "${YELLOW}Performing database maintenance...${NC}"

# Refresh materialized views
echo -e "${YELLOW}Refreshing materialized views...${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "SELECT refresh_monthly_financial_summary();"

# Vacuum analyze tables
echo -e "${YELLOW}Vacuum analyzing tables...${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "VACUUM ANALYZE;"

# Reindex important indexes
echo -e "${YELLOW}Reindexing important indexes...${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "
    REINDEX INDEX idx_loans_customer;
    REINDEX INDEX idx_transactions_loan;
    REINDEX INDEX idx_payment_schedules_loan;
    REINDEX INDEX idx_customers_name;
    REINDEX INDEX idx_customers_phone;
    REINDEX INDEX idx_transactions_date;
    REINDEX INDEX idx_audit_logs_changed_at;
"

# Check for and mark defaulted loans
echo -e "${YELLOW}Checking for defaulted loans...${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "
    SELECT mark_defaulted_loans(90, '00000000-0000-0000-0000-000000000001');
"

# Calculate portfolio at risk
echo -e "${YELLOW}Calculating portfolio at risk...${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "
    SELECT * FROM calculate_par(30);
"

# Check database statistics
echo -e "${YELLOW}Database statistics:${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "
    SELECT 
        relname as table_name,
        pg_size_pretty(pg_total_relation_size(relid)) as total_size,
        pg_size_pretty(pg_relation_size(relid)) as table_size,
        pg_size_pretty(pg_total_relation_size(relid) - pg_relation_size(relid)) as index_size,
        pg_stat_get_numscans(relid) as sequential_scans,
        pg_stat_get_tuples_inserted(relid) as rows_inserted,
        pg_stat_get_tuples_updated(relid) as rows_updated,
        pg_stat_get_tuples_deleted(relid) as rows_deleted
    FROM pg_catalog.pg_statio_user_tables
    ORDER BY pg_total_relation_size(relid) DESC;
"

# Check for bloated tables
echo -e "${YELLOW}Checking for bloated tables...${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "
    SELECT
        schemaname,
        relname,
        n_dead_tup,
        n_live_tup,
        round(n_dead_tup * 100.0 / (n_live_tup + n_dead_tup), 2) AS dead_percentage
    FROM pg_stat_user_tables
    WHERE n_live_tup > 0
    ORDER BY dead_percentage DESC;
"

# Check for unused indexes
echo -e "${YELLOW}Checking for unused indexes...${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "
    SELECT
        schemaname,
        relname,
        indexrelname,
        idx_scan,
        idx_tup_read,
        idx_tup_fetch,
        pg_size_pretty(pg_relation_size(indexrelid)) as index_size
    FROM pg_stat_user_indexes
    WHERE idx_scan = 0
    ORDER BY pg_relation_size(indexrelid) DESC;
"

# Check for long-running queries
echo -e "${YELLOW}Checking for long-running queries...${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "
    SELECT
        pid,
        now() - pg_stat_activity.query_start AS duration,
        query,
        state
    FROM pg_stat_activity
    WHERE (now() - pg_stat_activity.query_start) > interval '5 minutes'
    AND state != 'idle'
    AND pid != pg_backend_pid();
"

echo -e "${GREEN}Database maintenance completed successfully.${NC}"
echo "=== Maintenance completed at $(date) ==="

exit 0