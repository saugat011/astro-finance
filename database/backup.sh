#!/bin/bash

# AstroFinance Database Backup Script
# This script creates a backup of the database

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
RETENTION_DAYS=${BACKUP_RETENTION_DAYS:-30}
RETENTION_COUNT=${BACKUP_RETENTION_COUNT:-10}

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
    echo "  -o, --output-dir     Backup directory (default: ./backups)"
    echo "  -r, --retention-days Days to keep backups (default: 30)"
    echo "  -c, --retention-count Maximum number of backups to keep (default: 10)"
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
        -o|--output-dir)
            BACKUP_DIR="$2"
            shift
            shift
            ;;
        -r|--retention-days)
            RETENTION_DAYS="$2"
            shift
            shift
            ;;
        -c|--retention-count)
            RETENTION_COUNT="$2"
            shift
            shift
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            show_usage
            ;;
    esac
done

# Check if psql and pg_dump are installed
if ! command -v psql &> /dev/null || ! command -v pg_dump &> /dev/null; then
    echo -e "${RED}Error: PostgreSQL client tools (psql, pg_dump) are not installed.${NC}"
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

# Create backup directory if it doesn't exist
if [ ! -d "$BACKUP_DIR" ]; then
    echo -e "${YELLOW}Creating backup directory $BACKUP_DIR...${NC}"
    mkdir -p "$BACKUP_DIR"
fi

# Generate backup filename with timestamp
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/${DB_NAME}_${TIMESTAMP}.dump"

# Create backup
echo -e "${YELLOW}Creating backup of database $DB_NAME to $BACKUP_FILE...${NC}"
pg_dump $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -Fc -f "$BACKUP_FILE" "$DB_NAME"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Backup created successfully.${NC}"
    
    # Calculate backup size
    BACKUP_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
    echo -e "${GREEN}Backup size: $BACKUP_SIZE${NC}"
    
    # Clean up old backups based on retention policy
    echo -e "${YELLOW}Cleaning up old backups...${NC}"
    
    # Delete backups older than RETENTION_DAYS
    find "$BACKUP_DIR" -name "${DB_NAME}_*.dump" -type f -mtime +$RETENTION_DAYS -delete
    
    # Keep only the most recent RETENTION_COUNT backups
    ls -t "$BACKUP_DIR"/${DB_NAME}_*.dump | tail -n +$((RETENTION_COUNT+1)) | xargs -r rm
    
    # Count remaining backups
    BACKUP_COUNT=$(ls -1 "$BACKUP_DIR"/${DB_NAME}_*.dump | wc -l)
    echo -e "${GREEN}Backup cleanup completed. $BACKUP_COUNT backups remaining.${NC}"
else
    echo -e "${RED}Error creating backup.${NC}"
    exit 1
fi

# Show backup information
echo -e "${YELLOW}Backup Information:${NC}"
echo -e "${GREEN}Database: $DB_NAME${NC}"
echo -e "${GREEN}Backup File: $BACKUP_FILE${NC}"
echo -e "${GREEN}Backup Size: $BACKUP_SIZE${NC}"
echo -e "${GREEN}Timestamp: $TIMESTAMP${NC}"
echo -e "${GREEN}Retention Policy: Keep for $RETENTION_DAYS days or keep last $RETENTION_COUNT backups${NC}"

exit 0