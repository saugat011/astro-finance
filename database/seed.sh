#!/bin/bash

# AstroFinance Database Seed Script
# This script seeds the database with initial data for development and testing

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
SEED_FILE=${SEED_FILE:-./seed.sql}

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
    echo -e "${YELLOW}Please create the database first or run the migration script with --create option.${NC}"
    exit 1
fi

# Check if seed file exists
if [ ! -f "$SEED_FILE" ]; then
    echo -e "${RED}Error: Seed file $SEED_FILE does not exist.${NC}"
    exit 1
fi

# Seed the database
echo -e "${YELLOW}Seeding database $DB_NAME with data from $SEED_FILE...${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -f "$SEED_FILE"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Database seeded successfully.${NC}"
else
    echo -e "${RED}Error seeding database.${NC}"
    exit 1
fi

# Show summary of seeded data
echo -e "${YELLOW}Summary of seeded data:${NC}"
echo -e "${GREEN}Users:${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "SELECT role, COUNT(*) FROM users GROUP BY role;"

echo -e "${GREEN}Customers:${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "SELECT COUNT(*) FROM customers;"

echo -e "${GREEN}Loans by status:${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "SELECT status, COUNT(*), SUM(amount) FROM loans GROUP BY status;"

echo -e "${GREEN}Transactions by type:${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "SELECT type, COUNT(*), SUM(amount) FROM transactions GROUP BY type;"

echo -e "${GREEN}SMS Templates:${NC}"
psql $DB_HOST_ARG $DB_PORT_ARG -U "$DB_USER" -d "$DB_NAME" -c "SELECT name FROM sms_templates;"

exit 0