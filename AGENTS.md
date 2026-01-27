# Agent Guidelines for TimeTracker

This document provides guidelines for AI coding agents working in this repository.

## Project Overview

TimeTracker is a Windows time tracking application built with .NET 10, WPF, and SQLite.

### Architecture
- **TimeTracker.App** - WPF presentation layer (MVVM pattern)
- **TimeTracker.Core** - Business logic, services, and models
- **TimeTracker.Data** - Data persistence with Entity Framework Core + SQLite

### Key Technologies
- .NET 10 (net10.0 / net10.0-windows)
- WPF with WPF-UI library (modern UI)
- CommunityToolkit.Mvvm for MVVM pattern
- Entity Framework Core 10.0.2
- SQLite database
- Dependency Injection (Microsoft.Extensions.DependencyInjection)

## Build, Test, and Run Commands

### Building the Project
```bash
# Build entire solution
dotnet build src/TimeTracker.slnx

# Build specific project
dotnet build src/TimeTracker.App/TimeTracker.App.csproj
dotnet build src/TimeTracker.Core/TimeTracker.Core.csproj
dotnet build src/TimeTracker.Data/TimeTracker.Data.csproj

# Build in Release mode
dotnet build src/TimeTracker.slnx -c Release
```

### Running the Application
```bash
# Run from App project directory
dotnet run --project src/TimeTracker.App/TimeTracker.App.csproj
```

### Testing
```bash
# No test projects currently exist
# When creating tests, use xUnit or NUnit and follow naming:
# TimeTracker.Tests, TimeTracker.Core.Tests, TimeTracker.Data.Tests

# Run tests (when available)
dotnet test src/TimeTracker.slnx

# Run specific test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
dotnet test --filter "FullyQualifiedName~ValidationServiceTests.ValidateTimeRange_ShouldReturnTrue"
```

### Database Migrations
```bash
# Add migration (run from Data project directory)
dotnet ef migrations add MigrationName --project src/TimeTracker.Data --startup-project src/TimeTracker.App

# Update database
dotnet ef database update --project src/TimeTracker.Data --startup-project src/TimeTracker.App

# Remove last migration
dotnet ef migrations remove --project src/TimeTracker.Data --startup-project src/TimeTracker.App
```

### Code Analysis
```bash
# Format code (if using dotnet format)
dotnet format src/TimeTracker.slnx

# Restore packages
dotnet restore src/TimeTracker.slnx
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
namespace TimeTracker.Core.Services;  // Add a blank line after namespace

using System;                         // System usings first
using Microsoft.Extensions.Logging;   // Then Microsoft usings
using TimeTracker.Core.Interfaces;    // Finally project usings
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

### MVVM Pattern
- ViewModels inherit from `ObservableObject` (CommunityToolkit.Mvvm)
- Use `[ObservableProperty]` attribute for properties
- Use `[RelayCommand]` attribute for commands
- Keep ViewModels testable (inject dependencies)

```csharp
public partial class ActivitatsViewModel : ObservableObject
{
    private readonly IActivityRepository _repository;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [RelayCommand]
    private async Task LoadActivitiesAsync()
    {
        // Implementation
    }
}
```

### Dependency Injection
- Register services in `App.xaml.cs` → `ConfigureServices`
- Use constructor injection
- Repositories: Scoped lifetime
- Services: Scoped lifetime
- ViewModels: Singleton (Main) or Transient (Pages)
- Pages: Transient lifetime

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

- Database stored in AppData: Use `DatabaseConfiguration.GetConnectionString()`
- Migrations auto-applied on startup in `App.OnStartup`
- Use `TimeOnly` for time values (not `DateTime`)
- Use `DateOnly` for dates without time component
- GUID for all entity IDs

## When Creating New Features

1. Define models in `TimeTracker.Core/Models`
2. Create interfaces in `TimeTracker.Core/Interfaces`
3. Implement services in `TimeTracker.Core/Services`
4. Create repository in `TimeTracker.Data/Repositories`
5. Add EF configuration in `TimeTracker.Data/Configurations`
6. Create migration
7. Create ViewModel in `TimeTracker.App/ViewModels`
8. Create View (XAML + code-behind) in `TimeTracker.App/Views/Pages`
9. Register all in DI container (`App.xaml.cs`)

## References

- Project documentation: `README.md`, `README_ISSUES.md`
- Issues planning: `ISSUES_TO_CREATE.md`, `ISSUES_DIAGRAM.md`
