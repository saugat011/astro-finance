#!/bin/bash

# AstroFinance Database Migration Script
# This script applies database migrations in order

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
MIGRATIONS_DIR=${MIGRATIONS_DIR:-./migrations}
SCHEMA_VERSION_TABLE=${SCHEMA_VERSION_TABLE:-schema_version}

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
    echo "  -h, --help       Show this help message"
    echo "  -u, --user       Database user (default: postgres)"
    echo "  -d, --database   Database name (default: astrofinance)"
    echo "  -c, --create     Create database if it doesn't exist"
    echo "  -r, --reset      Reset database (drop and recreate)"
    echo "  -b, --backup     Backup database before migration"
    exit 1
}

# Parse command line arguments
CREATE_DB=false
RESET_DB=false
BACKUP_DB=false

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
        -b|--backup)
            BACKUP_DB=true
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

# Function to create schema version table if it doesn't exist
function create_schema_version_table {
    psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "
        CREATE TABLE IF NOT EXISTS $SCHEMA_VERSION_TABLE (
            id SERIAL PRIMARY KEY,
            version VARCHAR(100) NOT NULL,
            description VARCHAR(200) NOT NULL,
            applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        );
    "
}

# Function to check if migration has been applied
function migration_applied {
    local version=$1
    local result=$(psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -t -c "
        SELECT COUNT(*) FROM $SCHEMA_VERSION_TABLE WHERE version = '$version';
    ")
    
    if [ "$result" -gt 0 ]; then
        return 0 # True, migration has been applied
    else
        return 1 # False, migration has not been applied
    fi
}

# Function to record migration
function record_migration {
    local version=$1
    local description=$2
    
    psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "
        INSERT INTO $SCHEMA_VERSION_TABLE (version, description)
        VALUES ('$version', '$description');
    "
}

# Function to backup database
function backup_database {
    local timestamp=$(date +%Y%m%d_%H%M%S)
    local backup_file="backup_${DB_NAME}_${timestamp}.sql"
    
    echo -e "${YELLOW}Backing up database to $backup_file...${NC}"
    pg_dump $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -f "$backup_file"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}Backup completed successfully.${NC}"
    else
        echo -e "${RED}Backup failed.${NC}"
        exit 1
    fi
}

# Reset database if requested
if [ "$RESET_DB" = true ]; then
    echo -e "${YELLOW}Resetting database $DB_NAME...${NC}"
    
    if database_exists; then
        psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -c "DROP DATABASE $DB_NAME;"
    fi
    
    psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -c "CREATE DATABASE $DB_NAME;"
    echo -e "${GREEN}Database reset completed.${NC}"
elif [ "$CREATE_DB" = true ] && ! database_exists; then
    echo -e "${YELLOW}Creating database $DB_NAME...${NC}"
    psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -c "CREATE DATABASE $DB_NAME;"
    echo -e "${GREEN}Database created.${NC}"
elif ! database_exists; then
    echo -e "${RED}Error: Database $DB_NAME does not exist.${NC}"
    echo -e "${YELLOW}Use --create to create it.${NC}"
    exit 1
fi

# Backup database if requested
if [ "$BACKUP_DB" = true ]; then
    backup_database
fi

# Create schema version table
create_schema_version_table

# Get list of migration files
migration_files=$(find "$MIGRATIONS_DIR" -name "V*__*.sql" | sort)

# Apply migrations
for file in $migration_files; do
    # Extract version and description from filename
    filename=$(basename "$file")
    version=$(echo "$filename" | sed -E 's/V([0-9]+)__.*/\1/')
    description=$(echo "$filename" | sed -E 's/V[0-9]+__(.*)\.sql/\1/' | tr '_' ' ')
    
    # Check if migration has already been applied
    if migration_applied "$version"; then
        echo -e "${YELLOW}Migration V$version already applied. Skipping.${NC}"
    else
        echo -e "${YELLOW}Applying migration V$version: $description...${NC}"
        
        # Apply migration
        psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -f "$file"
        
        if [ $? -eq 0 ]; then
            # Record migration
            record_migration "$version" "$description"
            echo -e "${GREEN}Migration V$version applied successfully.${NC}"
        else
            echo -e "${RED}Error applying migration V$version.${NC}"
            exit 1
        fi
    fi
done

echo -e "${GREEN}All migrations applied successfully.${NC}"

# Show current schema version
current_version=$(psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -t -c "
    SELECT version FROM $SCHEMA_VERSION_TABLE ORDER BY id DESC LIMIT 1;
")

echo -e "${GREEN}Current schema version: $current_version${NC}"
exit 0