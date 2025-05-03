# Astro Finance

A modern financial management system for microfinance institutions with a Next.js frontend and .NET backend.

## Overview

Astro Finance is a comprehensive microfinance/corporate banking solution that provides:

- Loan Management
- Transaction Processing
- SMS Notifications
- Reporting
- User Management
- Double-entry Accounting

## System Requirements

- **Frontend**: Node.js 18+ and npm/pnpm
- **Backend**: .NET 8 SDK
- **Database**: PostgreSQL 14+
- **OS**: Windows, macOS, or Linux

## Installation Guide

Follow these steps to set up the complete Astro Finance application:

### 1. Clone the Repository

```bash
git clone https://github.com/notcool100/astro-finance.git
cd astro-finance
```

### 2. Database Setup

The application uses PostgreSQL for data storage:

1. Install PostgreSQL if you haven't already:
   - [PostgreSQL Downloads](https://www.postgresql.org/download/)

2. Create the database:
   ```bash
   cd database
   ./migrate.sh --create
   ```
   
   This script will:
   - Create a new database named "astrofinance"
   - Apply all migration scripts in the correct order
   - Set up the schema version tracking

   **Options:**
   - Use `--user yourusername` to specify a different PostgreSQL user
   - Use `--reset` to drop and recreate the database
   - Use `--backup` to create a backup before migrations

3. Seed the database with initial data:
   ```bash
   ./seed.sh
   ```
   
   This script will:
   - Populate the database with sample data for development and testing
   - Create default users, customers, loans, and transactions
   - Set up SMS templates and other required data
   - Display a summary of the seeded data

   **Options:**
   - Use `--user yourusername` to specify a different PostgreSQL user
   - Use `--database dbname` to specify a different database name

4. Verify the database setup:
   ```bash
   psql -U postgres -d astrofinance -c "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';"
   ```

**Note:** This project uses direct SQL migrations rather than Entity Framework migrations. You do not need to run `dotnet ef database update` or similar commands. The database schema is managed entirely through the SQL scripts in the `database/migrations` directory.

#### Additional Database Management Scripts

The project includes several scripts for database management:

- **backup.sh**: Creates a backup of the database
  ```bash
  ./backup.sh
  ```

- **restore.sh**: Restores the database from a backup
  ```bash
  ./restore.sh backup_filename.sql
  ```

- **maintenance.sh**: Performs routine database maintenance (VACUUM, ANALYZE, etc.)
  ```bash
  ./maintenance.sh
  ```

These scripts support environment variables and command-line arguments for customization. Run any script with `--help` to see available options.

### 3. Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd AstroFinance.Backend
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Configure the application:
   - Create an `appsettings.Development.json` file in the `src/AstroFinance.API` directory with the following content:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=astrofinance;Username=postgres;Password=yourpassword"
     },
     "JwtSettings": {
       "Secret": "your-secret-key-at-least-16-characters-long",
       "Issuer": "astrofinance",
       "Audience": "astrofinance-clients",
       "ExpiryMinutes": 60
     },
     "SmsSettings": {
       "Provider": "test",
       "ApiKey": "your-api-key",
       "SenderName": "AstroFin"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft": "Warning",
         "Microsoft.Hosting.Lifetime": "Information"
       }
     }
   }
   ```
   
   **Note:** Replace "yourpassword" with your actual PostgreSQL password.

5. Run the API:
   ```bash
   cd src/AstroFinance.API
   dotnet run
   ```

6. The API should now be running at `https://localhost:5001` and `http://localhost:5000`

### 4. Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   # or if you prefer pnpm
   pnpm install
   ```

3. Configure environment variables:
   - Create a `.env.local` file with the following content:
   ```
   NEXT_PUBLIC_API_URL=http://localhost:5000
   ```

4. Start the development server:
   ```bash
   npm run dev
   # or
   pnpm dev
   ```

5. The frontend should now be running at `http://localhost:3000`

## Accessing the Application

- **Frontend**: Open your browser and navigate to `http://localhost:3000`
- **API Documentation**: Swagger UI is available at `http://localhost:5000/swagger`
- **Default Admin Account** (created during database seeding):
  - Email: admin@astrofinance.com
  - Password: Admin123!
  
  Additional test accounts may also be available depending on the seed data. Check the `seed.sql` file for details.

## Development Workflow

### Backend Development

- Use Visual Studio or Visual Studio Code to open the solution file `AstroFinance.Backend/AstroFinance.sln`
- Run tests with `dotnet test`
- Apply new database migrations with the migration script

### Frontend Development

- Make changes to the Next.js application in the `frontend` directory
- Run tests with `npm test` or `pnpm test`
- Build for production with `npm run build` or `pnpm build`

## Troubleshooting

### Database Connection Issues

- Verify PostgreSQL is running: `pg_isready`
- Check connection string in `appsettings.Development.json`
- Ensure the database user has appropriate permissions
- If you see Entity Framework errors, remember that this project uses direct SQL migrations. Run the `./migrate.sh` script in the `database` directory instead of EF migrations

### Database Seeding Issues

- If you encounter errors during seeding, check the PostgreSQL logs for detailed error messages
- Ensure you've run the migrations first with `./migrate.sh` before running `./seed.sh`
- If you need to reset the database and start fresh, use `./migrate.sh --reset` followed by `./seed.sh`
- For permission errors, ensure your PostgreSQL user has the necessary privileges

### API Not Starting

- Check for port conflicts on 5000/5001
- Verify .NET 8 SDK is installed: `dotnet --version`
- Check logs for detailed error messages

### Frontend Build Errors

- Clear npm cache: `npm cache clean --force`
- Delete `node_modules` and reinstall dependencies
- Verify Node.js version: `node --version`

## Deployment

For production deployment, refer to:
- [.NET Production Deployment Guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [Next.js Deployment Documentation](https://nextjs.org/docs/deployment)
- [PostgreSQL Production Checklist](https://wiki.postgresql.org/wiki/Don't_Do_This)

## Additional Resources

- Backend architecture details: See `backend/README.md`
- Frontend architecture details: See `frontend/README.md`
- Database schema details: See `database/README.md`
