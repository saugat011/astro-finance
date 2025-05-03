# Astro Finance Backend

This is the .NET backend for the Astro Finance application, a micro-finance/corporate banking solution. The backend is built using Clean Architecture principles to ensure maintainability, testability, and scalability.

## Architecture Overview

The solution follows Clean Architecture with the following layers:

1. **Domain Layer** - Contains business entities, enums, exceptions, interfaces, types and logic
2. **Application Layer** - Contains business logic, interfaces, DTOs, and validation
3. **Infrastructure Layer** - Contains external concerns like database, identity, file system, and external APIs
4. **API Layer** - Contains controllers, middleware, filters, and API endpoints

## Project Structure

```
AstroFinance.Backend/
├── src/
│   ├── AstroFinance.Domain/
│   ├── AstroFinance.Application/
│   ├── AstroFinance.Infrastructure/
│   └── AstroFinance.API/
└── tests/
    ├── AstroFinance.UnitTests/
    ├── AstroFinance.IntegrationTests/
    └── AstroFinance.FunctionalTests/
```

## Key Features

- Authentication and Authorization with JWT
- Loan Management
- Transaction Processing
- SMS Notifications
- Reporting
- User Management

## Technologies Used

- .NET 8
- Entity Framework Core
- AutoMapper
- FluentValidation
- MediatR (CQRS pattern)
- Swagger/OpenAPI
- SQL Server
- xUnit for testing