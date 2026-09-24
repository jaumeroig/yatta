# Agent Guidelines for TimeTracker

This document provides guidelines for AI coding agents working in this repository.

## Project Overview

Yatta is a Windows time tracking application built with .NET 10, Blazor Hybrid, WPF, and SQLite.

### Architecture
- **Yatta.App** - Razor presentation layer in WPF BlazorWebView windows; WPF owns native Windows integrations
- **Yatta.Core** - Business logic, services, and models
- **Yatta.Data** - Data persistence with Entity Framework Core + SQLite

### Key Technologies
- .NET 10 (net10.0 / net10.0-windows)
- WPF BlazorWebView and Blazor Blueprint components
- WPF-UI for native window chrome and system tray
- Entity Framework Core 10
- SQLite database
- Dependency Injection (Microsoft.Extensions.DependencyInjection)

## Build, Test, and Run Commands

### Building the Project
```bash
# Build entire solution
dotnet build src/Yatta.slnx

# Build specific project
dotnet build src/Yatta.App/Yatta.App.csproj
dotnet build src/Yatta.Core/Yatta.Core.csproj
dotnet build src/Yatta.Data/Yatta.Data.csproj

# Build in Release mode
dotnet build src/Yatta.slnx -c Release
```

### Running the Application
```bash
# Run from App project directory
dotnet run --project src/Yatta.App/Yatta.App.csproj
```

### Testing
```bash
# Yatta.Tests uses xUnit and Moq

# Run tests (when available)
dotnet test src/Yatta.slnx

# Run specific test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
dotnet test --filter "FullyQualifiedName~ValidationServiceTests.ValidateTimeRange_ShouldReturnTrue"
```

### Database Migrations
```bash
# Add migration (run from Data project directory)
dotnet ef migrations add MigrationName --project src/Yatta.Data --startup-project src/Yatta.App

# Update database
dotnet ef database update --project src/Yatta.Data --startup-project src/Yatta.App

# Remove last migration
dotnet ef migrations remove --project src/Yatta.Data --startup-project src/Yatta.App
```

### Code Analysis
```bash
# Format code (if using dotnet format)
dotnet format src/Yatta.slnx

# Restore packages
dotnet restore src/Yatta.slnx
```

## Code Style Guidelines

### Language and Comments
- **Primary language**: English (en-US)
- All comments, documentation, in English (en-US)
- Variable names and code elements in English (en-US) (standard C# conventions)
- User-facing text in Spanish (es-ES) and Catalan (ca-ES) using always resource files.

### File Organization
Always start files with namespace declaration followed by usings:
```
namespace Yatta.Core.Services;  // Add a blank line after namespace

using System;                         // System usings first
using Microsoft.Extensions.Logging;   // Then Microsoft usings
using Yatta.Core.Interfaces;    // Finally project usings
```

### Imports
- Use file-scoped namespaces (no braces)
- Place `using` statements inside namespace declaration
- Order: System → Microsoft → Third-party → Project
- Remove unused usings
- Prefer explicit imports over wildcards

### Formatting
- **Line endings**: Windows (CRLF)
- **Indentation**: 4 spaces (no tabs)
- **Line length**: No hard limit, but keep reasonable (~120 chars)
- **Braces**: Always use braces for control structures
- **Spacing**: Space after keywords, around operators

```csharp
// Good
if (condition)
{
    DoSomething();
}

// Bad - missing braces
if (condition)
    DoSomething();
```

### Naming Conventions
- **Classes/Interfaces**: PascalCase (e.g., `ActivityRepository`, `IActivityRepository`)
- **Methods/Properties**: PascalCase (e.g., `GetAllAsync`, `StartTime`)
- **Parameters/Local vars**: camelCase (e.g., `activityId`, `startTime`)
- **Private fields**: _camelCase with underscore (e.g., `_context`, `_serviceProvider`)
- **Constants**: PascalCase (e.g., `MaxRetryCount`)
- **Interfaces**: Prefix with `I` (e.g., `IActivityRepository`)
- **Async methods**: Suffix with `Async` (e.g., `GetAllAsync`)

### Types and Nullability
- **Nullable reference types**: Enabled (`<Nullable>enable</Nullable>`)
- Use `?` for nullable types: `Activity?`, `TimeOnly?`
- Use `null!` for DbSet initialization: `public DbSet<Activity> Activities { get; set; } = null!;`
- Prefer `string.Empty` over `""` for empty strings
- Use explicit types for clarity: `TimeOnly startTime` not `var startTime`
- Use `var` when type is obvious: `var context = new TimeTrackerDbContext()`

### Methods and Parameters
- **Async all the way**: Repository/service methods should be async
- Return `Task<T>` or `Task` for async methods
- Use `CancellationToken` for long-running operations (optional parameter)
- Validate parameters at method entry

```csharp
public async Task<Activity?> GetByIdAsync(Guid id)
{
    if (id == Guid.Empty)
        throw new ArgumentException("Invalid activity ID", nameof(id));
        
    return await _context.Activities.FindAsync(id);
}
```

### Error Handling
- Use exceptions for exceptional cases
- Return `null` or nullable types for "not found" scenarios
- Provide validation methods with `out string errorMessage` overloads
- Document thrown exceptions in XML comments

```csharp
/// <summary>
/// Validates that the end time is after the start time.
/// </summary>
/// <exception cref="ArgumentException">If the range is invalid.</exception>
public bool ValidateTimeRange(TimeOnly startTime, TimeOnly endTime, out string errorMessage)
{
    if (endTime <= startTime)
    {
        errorMessage = Resources.Validation_EndTimeAfterStartTime;
        return false;
    }
    
    errorMessage = string.Empty;
    return true;
}
```

### XML Documentation
- **Required** for all public types, methods, and properties
- Use English (en-US) for documentation text
- Include `<summary>`, `<param>`, `<returns>`, `<exception>` as needed

```csharp
/// <summary>
/// Gets all active activities.
/// </summary>
/// <returns>A collection of active activities.</returns>
Task<IEnumerable<Activity>> GetActiveAsync();
```

### Blazor Hybrid
- Add screens and dialogs in `Yatta.App/Blazor` as Razor components.
- Use Blazor Blueprint components and localized `ILocalizationService` strings for visible text.
- Put actions shared by the main window, tray, and quick picker in injected services.
- Create a fresh DI scope for every repository operation in long-lived components and singleton services.
- Publish data changes through `UiEventService` so all open windows refresh.

### Dependency Injection
- Register services in `App.xaml.cs` → `ConfigureServices`
- Use constructor injection
- Repositories: Scoped lifetime
- Services: Scoped lifetime
- Blazor components: Created by BlazorWebView
- UI event and time entry coordination services: Singleton, with short-lived repository scopes

### Entity Framework
- Use async methods: `ToListAsync()`, `FindAsync()`, `SaveChangesAsync()`
- Create separate configuration classes: `ActivityConfiguration : IEntityTypeConfiguration<Activity>`
- Define relationships and constraints in configuration classes
- Use migrations for schema changes

## Common Patterns

### Repository Pattern
```csharp
public class ActivityRepository : IActivityRepository
{
    private readonly TimeTrackerDbContext _context;
    
    public ActivityRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Activity>> GetAllAsync()
    {
        return await _context.Activities.ToListAsync();
    }
}
```

### Service Pattern
```csharp
public class ValidationService : IValidationService
{
    public bool ValidateTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        return endTime > startTime;
    }
}
```

## Project-Specific Notes

- Database stored at `%LOCALAPPDATA%\Yatta\Yatta.db`: Use `DatabaseConfiguration.GetConnectionString()`
- Migrations auto-applied on startup in `App.OnStartup`
- Use `TimeOnly` for time values (not `DateTime`)
- Use `DateOnly` for dates without time component
- GUID for all entity IDs

## When Creating New Features

1. Define models in `Yatta.Core/Models`
2. Create interfaces in `Yatta.Core/Interfaces`
3. Implement services in `Yatta.Core/Services`
4. Create repository in `Yatta.Data/Repositories`
5. Add EF configuration in `Yatta.Data/Configurations`
6. Create migration
7. Create or update a component in `Yatta.App/Blazor`
8. Add shared UI operations to an injected service in `Yatta.App/Services`
9. Register all in DI container (`App.xaml.cs`)

## References

- Project documentation: `README.md`, `README_ISSUES.md`
- Issues planning: `ISSUES_TO_CREATE.md`, `ISSUES_DIAGRAM.md`
