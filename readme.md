# AmxBookstore API

### Introduction
Welcome to the **AmxBookstore API**! This API is built using .NET 8 and adheres to best practices in software design and architecture to deliver a high-performance, user-friendly experience for managing a catalog of books, stock, and orders.

#### Business Domain
The chosen domain for this project is **Books**, providing a comprehensive example for managing a product catalog.

## Architecture Overview
### Clean Architecture
The AmxBookstore API follows the Clean Architecture principles, ensuring a separation of concerns, scalability, and maintainability. Here's an overview of the project structure:

```
AmxBookstore.sln
src/
|-- API/                   # Presentation Layer
|   |-- AmxBookstore.API/
|   |   |-- Controllers/     # API Controllers
|   |   |-- Middleware/      # Middleware for the API
|   |   |-- program.cs       # Application Entry Point
|
|-- Core/                    # Core Layer
|   |-- Application/         # Application Layer
|   |   |-- UseCases/        # Use Cases
|   |   |   |-- Commands/    # Commands (CQRS)
|   |   |   |-- Queries/     # Queries (CQRS)
|   |   |-- DTOs/            # Data Transfer Objects (DTOs)
|   |   |-- Validators/      # Validators for DTOs
|   |   |-- Interfaces/      # Application Service Interfaces
|   |   |-- Services/        # Application Service Implementations
|
|-- Domain/                  # Domain Layer
|   |-- Entities/            # Domain Entities
|   |-- Interfaces/          # Domain Repository and Service Interfaces
|   |-- Validation/          # Domain Entities Validation
|
|-- Infrastructure/          # Infrastructure Layer
|   |-- DependencyInjection/ # Dependency Injection Configuration
|   |-- Persistence/         # Data Access Implementation (EF Core)
|   |-- Repositories/        # Repository Implementations
|   |-- Identity/            # Identity and Authentication Implementation
|   |-- EventStore/          # Event Sourcing Implementation
|
|-- Shared/                  # Shared Components
|
|-- Tests/                   # Tests
|   |-- UnitTests/           # Unit Tests
|   |-- IntegrationTests/    # Integration Tests

```

### Features of the API
• **Products**: CRUD operations, filtering, and detailed views for books.

• **Stock**: Manage stock levels for books.

• **Orders**: Create and manage orders, with role-based access control.

• **User Authentication and Authorization**: Sign in and role management (Admin, Seller, Client).

#### Additional Features

• **Caching**: Implemented with a 10-second in-memory cache for enhanced performance.

• **Logging**: Structured logging with Serilog.

• **Rate** Limiting and Resilience: Using Polly.

• **Documentation**: API documentation with Swagger and Postman.

• **Health Checks**: For application monitoring.

• **Event Sourcing**: For order management.

## Project Setup

#### Prerequisites
• .NET Core 8 SDK

• Git

• Docker

• docker-compose


### Installation Instructions

1. Clone the repository:

```code
git clone https://github.com/yourusername/amxbookstore.git
cd amxbookstore
```

2. Run the docker-compose:

```code
docker-compose up --build
```


## Development Steps

### Pre-Project Steps

#### 1. Choosing the Project Architecture and Justifying the Choice (Clean Architecture)

Conclusion:
    Clean Architecture is chosen for its flexibility, testability, and maintainability. It separates responsibilities clearly and isolates business logic from implementation details, facilitating maintenance and evolution while allowing easy adaptation to technological changes.

#### 2. Validating the Use of SOLID Principles in the Application

Conclusion:
    Applying SOLID principles helps in building robust, testable, and maintainable systems. Combined with Clean Architecture, these principles ensure clear separation of responsibilities, facilitating the evolution and adaptation of the application to changing requirements and technologies.

#### 3. Validating the Use of CQRS

Conclusion:
    CQRS (Command Query Responsibility Segregation) separates read operations (queries) from write operations (commands), improving performance, scalability, and security, and making the code easier to maintain.

#### 4. Choosing a Logging Tool for the System

Conclusion:
    Serilog.

#### 5. Understanding the Required Features of the System

#### 6. Creating base entities for the system


### Development Steps

#### 1. Creating Schemas for the Service

#### 2. Creating the Core Folder and the Domain and Application Projects

#### 3. Creating the Infrastructure Folder and Its Projects

#### 4. Adding Swagger to the Project for Documentation

#### 5. Creating Authentication Module with Identity (Adding Role-Based Permissions to Controllers)

#### 6. Creating Login/Refresh Endpoints

#### 7. Adding Business Rules to Services + Creating Health Check

#### 8. Creating Filters for Products, Orders, and Clients

#### 9. Creating Unit Tests and Integration Tests

#### 10. Detailing the Project Construction Steps in the README

#### 11. Creating Dockerfile for Deployment

#### 12. Setting Up CI/CD with GitHub Actions

## Business Rules

### Order

• Product stock must exist and be greater than 0 to create an order.

• The requested quantity per product must be less than or equal to the total stock.

• The product must exist to create an order.

• The client must exist to create an order.

• Orders are created with a "created" status.

• An order must be created with the sellerId of the user who created the order via token.

• Prevent editing products of an order.

### Stock

• The product must exist to create stock.

### User

• Only allow creation and updating of client users by the seller.

• Only allow listing of client users by the seller.

## Test Scenarios

• Admin: Can do everything in any entity.

• Seller: Can only view/edit/create clients, create orders for any client, and only edit/view orders they created.

• Clients: Can only view orders addressed to them.

## Base Schemas

### User
```JS
{
    "name": "string",
    "email": "string",
    "password": "string",
    "role": "admin" | "seller" | "client",
    "createdAt": "date",
    "updatedAt": "date",
    "deleted": false
}
```

### Book (Product)
```JS
{
    "title": "string",
    "description": "string",
    "pages": 100,
    "author": "string",
    "price": 100,
    "createdAt": "date",
    "updatedAt": "date",
    "deleted": false
}
```

### Stock
```JS
{
    "productId": "ID",
    "quantity": 10,
    "createdAt": "date",
    "updatedAt": "date",
    "deleted": false
}
```

### Order
```JS
{
    "products": [
        {
            "productId": "ID",
            "quantity": 1
        }
    ],
    "totalAmount": 100,
    "sellerId": "ID",
    "clientId": "ID",
    "status": "created" | "delivering" | "canceled" | "finished",
    "createdAt": "date",
    "updatedAt": "date",
    "deleted": false
}

```

## API Features

**OK** - Cache (InMemoryCache)

**OK** - Logging (Serilog)

**OK** - Rate Limiting (Polly)

**OK** - Circuit Breaker (Polly)

**OK** - Polly for Resilience (DB)

**OK** - CQRS

**OK** - DTO Validation (FluentValidation)

**OK** - Entities Mapping (Automapper)

**OK** - Database InMemory (Entity Framework Core)

**OK** - Authentication and Authorization (Identity)

**OK** - Documentation (Swagger and Postman)

**OK** - Global Exception Handling (Middleware)

**OK** - Domain Entity Validation (DomainExceptionValidation)

**OK** - Filters (LINQ)

**OK** - API Versioning (Asp.Versioning.Mvc)

**OK** - Health Checks 

**OK** - Event Sourcing (InMemory)

## Packages

### Authentication and Authorization
- **Microsoft.AspNetCore.Authentication.JwtBearer**: Provides support for JWT (JSON Web Tokens) based authentication, a common practice for RESTful APIs. It enables token verification issued by an identity server.
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore**: Simplifies the implementation of ASP.NET Core Identity with Entity Framework Core, facilitating the management of users, roles, and authentication.

### DTO Validation
- **FluentValidation.AspNetCore**: Provides validation for Data Transfer Objects (DTOs) directly in the API layer, allowing the application of consistent and reusable validation rules.

### Object Mapping
- **AutoMapper.Extensions.Microsoft.DependencyInjection**: Facilitates mapping between domain entities and DTOs, reducing boilerplate code and promoting a clear separation of responsibilities.

### In-Memory Database
- **Microsoft.EntityFrameworkCore.InMemory**: Provides an in-memory database, useful for tests and rapid development without the need to configure a persistent database.

### Logging and Monitoring
- **Serilog**: Provides structured logging, facilitating the monitoring and diagnostics of issues in production environments.
- **Serilog.Sinks.Console**: Enables log output to the console, useful for development and debugging.
- **Serilog.Sinks.File**: Enables log storage in files, useful for auditing and later analysis.

### CQRS and MediatR
- **MediatR**: Implements the CQRS (Command Query Responsibility Segregation) pattern, which helps separate read and write operations, promoting a cleaner and more scalable architecture.
- **MediatR.Extensions.Microsoft.DependencyInjection**: Facilitates the integration of MediatR with the DI container of ASP.NET Core.

### Rate Limiting and Resilience
- **AspNetCoreRateLimit**: Implements rate limiting, protecting the API from abuse and excessive use.
- **Polly**: Provides resilience and transient fault handling, including retry and timeout policies, improving application robustness.

### Testing
- **xUnit**: A popular framework for .NET unit testing, known for its simplicity and integration with CI/CD tools.
- **Moq**: A mocking library that facilitates the creation of mock objects for unit testing, promoting isolated and maintainable tests.
- **FluentAssertions**: Makes assertions in unit tests more fluent and expressive, enhancing test readability and maintainability.

