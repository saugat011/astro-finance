#!/bin/bash

# AstroFinance Database Restore Script
# This script restores a database from a backup

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
BACKUP_DIR=${BACKUP_DIR:-./backups}

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
    echo "Usage: $0 [options] backup_file"
    echo "Options:"
    echo "  -h, --help           Show this help message"
    echo "  -u, --user           Database user (default: postgres)"
    echo "  -d, --database       Database name (default: astrofinance)"
    echo "  -c, --create         Create database if it doesn't exist"
    echo "  -r, --reset          Reset database (drop and recreate)"
    echo "  -l, --list           List available backups and exit"
    echo ""
    echo "Example:"
    echo "  $0 -r backups/astrofinance_20230101_120000.dump"
    echo "  $0 --list"
    exit 1
}

# Parse command line arguments
CREATE_DB=false
RESET_DB=false
LIST_BACKUPS=false
BACKUP_FILE=""

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
        -c|--create)
            CREATE_DB=true
            shift
            ;;
        -r|--reset)
            RESET_DB=true
            shift
            ;;
        -l|--list)
            LIST_BACKUPS=true
            shift
            ;;
        *)
            if [[ $1 == -* ]]; then
                echo -e "${RED}Unknown option: $1${NC}"
                show_usage
            else
                BACKUP_FILE="$1"
                shift
            fi
            ;;
    esac
done

# Check if psql and pg_restore are installed
if ! command -v psql &> /dev/null || ! command -v pg_restore &> /dev/null; then
    echo -e "${RED}Error: PostgreSQL client tools (psql, pg_restore) are not installed.${NC}"
    exit 1
fi

# Function to check if database exists
function database_exists {
    psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -lqt | cut -d \| -f 1 | grep -qw "$DB_NAME"
}

# List available backups if requested
if [ "$LIST_BACKUPS" = true ]; then
    echo -e "${YELLOW}Available backups:${NC}"
    if [ -d "$BACKUP_DIR" ]; then
        if [ "$(ls -A $BACKUP_DIR)" ]; then
            echo -e "${GREEN}Backup File\t\t\tSize\t\tDate${NC}"
            for file in "$BACKUP_DIR"/${DB_NAME}_*.dump; do
                if [ -f "$file" ]; then
                    filename=$(basename "$file")
                    size=$(du -h "$file" | cut -f1)
                    date=$(stat -c %y "$file" | cut -d. -f1)
                    echo -e "${filename}\t${size}\t${date}"
                fi
            done | sort -r
        else
            echo -e "${YELLOW}No backups found in $BACKUP_DIR${NC}"
        fi
    else
        echo -e "${YELLOW}Backup directory $BACKUP_DIR does not exist${NC}"
    fi
    exit 0
fi

# Check if backup file is provided
if [ -z "$BACKUP_FILE" ]; then
    echo -e "${RED}Error: No backup file specified.${NC}"
    show_usage
fi

# Check if backup file exists
if [ ! -f "$BACKUP_FILE" ]; then
    echo -e "${RED}Error: Backup file $BACKUP_FILE does not exist.${NC}"
    exit 1
fi

# Reset database if requested
if [ "$RESET_DB" = true ]; then
    echo -e "${YELLOW}Resetting database $DB_NAME...${NC}"
    
    if database_exists; then
        echo -e "${YELLOW}Dropping database $DB_NAME...${NC}"
        psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -c "DROP DATABASE $DB_NAME;"
    fi
    
    echo -e "${YELLOW}Creating database $DB_NAME...${NC}"
    psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -c "CREATE DATABASE $DB_NAME;"
    echo -e "${GREEN}Database reset completed.${NC}"
elif [ "$CREATE_DB" = true ] && ! database_exists; then
    echo -e "${YELLOW}Creating database $DB_NAME...${NC}"
    psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -c "CREATE DATABASE $DB_NAME;"
    echo -e "${GREEN}Database created.${NC}"
elif ! database_exists; then
    echo -e "${RED}Error: Database $DB_NAME does not exist.${NC}"
    echo -e "${YELLOW}Use --create to create it or --reset to reset it.${NC}"
    exit 1
fi

# Restore database
echo -e "${YELLOW}Restoring database $DB_NAME from $BACKUP_FILE...${NC}"
pg_restore $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "$BACKUP_FILE"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Database restored successfully.${NC}"
    
    # Show database information
    echo -e "${YELLOW}Database Information:${NC}"
    echo -e "${GREEN}Database: $DB_NAME${NC}"
    echo -e "${GREEN}Backup File: $BACKUP_FILE${NC}"
    echo -e "${GREEN}Restore Date: $(date)${NC}"
    
    # Show table counts
    echo -e "${YELLOW}Table Counts:${NC}"
    psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "
        SELECT 
            table_name, 
            (SELECT COUNT(*) FROM ${table_name}) AS row_count
        FROM 
            information_schema.tables
        WHERE 
            table_schema = 'public'
            AND table_type = 'BASE TABLE'
        ORDER BY 
            table_name;
    "
else
    echo -e "${RED}Error restoring database.${NC}"
    exit 1
fi

exit 0