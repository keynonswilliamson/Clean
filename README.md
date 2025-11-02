# Clean Architecture Demo

A .NET 8 Clean Architecture sample solution demonstrating best practices for building maintainable and scalable applications.

## Architecture Overview

This solution follows Clean Architecture principles with clear separation of concerns across multiple layers:

```
┌─────────────────────────────────────┐
│         API Layer                   │
│  (CleanArchitecture.Demo.Api)       │
│  - Controllers                      │
│  - Program.cs                       │
│  - Serilog Configuration            │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      Application Layer              │
│  (CleanArchitecture.Demo.Application)│
│  - Commands/Queries (CQRS)         │
│  - Handlers (MediatR)              │
│  - Validators (FluentValidation)   │
│  - DTOs                            │
│  - Interfaces                      │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      Infrastructure Layer           │
│  (CleanArchitecture.Demo.Infrastructure)│
│  - DbContext (EF Core)             │
│  - Repositories                     │
│  - Entity Configurations            │
│  - Migrations                       │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│         Domain Layer                │
│  (CleanArchitecture.Demo.Domain)   │
│  - Entities                        │
│  - Value Objects                   │
│  - Domain Events                   │
│  - Domain Interfaces               │
└─────────────────────────────────────┘
```

## Solution Structure

```
CleanArchitecture.Demo/
├── CleanArchitecture.Demo.Domain/          # Domain entities, events, interfaces
├── CleanArchitecture.Demo.Application/     # Application logic, CQRS, validators
├── CleanArchitecture.Demo.Infrastructure/  # EF Core, repositories, persistence
├── CleanArchitecture.Demo.Api/             # ASP.NET Core Web API
├── CleanArchitecture.Demo.UnitTests/       # Unit tests
└── CleanArchitecture.Demo.IntegrationTests/ # Integration tests
```

## Key Technologies

- **.NET 8.0** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core 8** - ORM for data access
- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **xUnit** - Testing framework
- **Moq** - Mocking framework

## Project Dependencies

- **Domain** - No dependencies (pure domain logic)
- **Application** - Depends on Domain
- **Infrastructure** - Depends on Application and Domain
- **Api** - Depends on Application and Infrastructure

## Domain Model

### Customer Entity
- `Id` (Guid)
- `Name` (string)
- `Email` (string, unique)
- `CreatedOn` (DateTime)
- `Orders` (ICollection<Order>)

### Order Entity
- `Id` (Guid)
- `CustomerId` (Guid, foreign key)
- `OrderDate` (DateTime)
- `Total` (decimal)

### Business Rules
- Customer email must be unique
- Cannot create an order for a non-existent customer
- Domain event `CustomerCreatedEvent` is raised when a new customer is created

## Features

### CQRS with MediatR
- **Commands** - Write operations (e.g., `CreateCustomerCommand`)
- **Queries** - Read operations (e.g., `GetCustomerByIdQuery`)
- Each command/query has its own handler

### Validation
- FluentValidation validators for all commands
- Automatic validation pipeline integration

### Logging
- Serilog configured for structured logging
- Console and file sinks
- Request logging middleware

### Database
- Entity Framework Core with SQL Server
- Code-first migrations
- Entity configurations for data integrity

### API Endpoints

#### Customers
- `POST /api/customers` - Create a new customer
- `GET /api/customers/{id}` - Get customer by ID

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server (LocalDB) or SQL Server Express

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd CleanArchitecture.Demo
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update connection string**
   - Edit `CleanArchitecture.Demo.Api/appsettings.json`
   - Update the `DefaultConnection` connection string to point to your SQL Server instance

4. **Create database**
   ```bash
   cd CleanArchitecture.Demo.Infrastructure
   dotnet ef migrations add InitialCreate --startup-project ../CleanArchitecture.Demo.Api
   dotnet ef database update --startup-project ../CleanArchitecture.Demo.Api
   ```

5. **Run the application**
   ```bash
   cd CleanArchitecture.Demo.Api
   dotnet run
   ```

6. **Access Swagger UI**
   - Navigate to `https://localhost:5001/swagger` (or the port shown in console)

### Running Tests

**Unit Tests**
```bash
dotnet test CleanArchitecture.Demo.UnitTests
```

**Integration Tests**
```bash
dotnet test CleanArchitecture.Demo.IntegrationTests
```

**All Tests**
```bash
dotnet test
```

## Code Examples

### Creating a Customer

```csharp
POST /api/customers
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john.doe@example.com"
}
```

### Getting a Customer

```csharp
GET /api/customers/{customerId}
```

## Architecture Principles

1. **Dependency Rule** - Dependencies point inward. Inner layers don't know about outer layers.
2. **Separation of Concerns** - Each layer has a specific responsibility.
3. **CQRS** - Commands and queries are separated for better scalability.
4. **Domain-Driven Design** - Domain logic is encapsulated in entities.
5. **Testability** - Clear interfaces allow for easy unit and integration testing.

## Project Files

### .csproj Files
All projects use the standard .NET SDK project format with appropriate package references.

### Program.cs
- Configures Serilog logging
- Registers EF Core DbContext
- Registers MediatR
- Registers FluentValidation
- Configures Swagger
- Sets up dependency injection

## Testing

### Unit Tests
- Test individual components in isolation
- Use Moq for mocking dependencies
- Located in `CleanArchitecture.Demo.UnitTests`

### Integration Tests
- Test the full request/response cycle
- Use in-memory database for testing
- Located in `CleanArchitecture.Demo.IntegrationTests`

## Logging

Serilog is configured to log to:
- **Console** - Development and debugging
- **File** - Persistent logs in `logs/` directory

Log levels are configurable via `appsettings.json`.

## Next Steps

To extend this solution, consider:
- Adding more commands/queries (e.g., UpdateCustomer, DeleteCustomer, GetOrdersByCustomer)
- Implementing domain event handlers
- Adding authentication and authorization
- Implementing caching
- Adding API versioning
- Implementing rate limiting
- Adding health checks

## License

This is a sample project for educational purposes.

