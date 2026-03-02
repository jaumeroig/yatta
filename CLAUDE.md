# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build src/TimeTracker.slnx
dotnet build src/TimeTracker.slnx -c Release

# Run
dotnet run --project src/TimeTracker.App/TimeTracker.App.csproj

# Test (xUnit + Moq, tests only cover TimeTracker.Core)
dotnet test src/TimeTracker.slnx
dotnet test --filter "FullyQualifiedName~ValidationServiceTests.ValidateTimeRange_ShouldReturnTrue"

# EF Core migrations
dotnet ef migrations add MigrationName --project src/TimeTracker.Data --startup-project src/TimeTracker.App
dotnet ef database update --project src/TimeTracker.Data --startup-project src/TimeTracker.App
dotnet ef migrations remove --project src/TimeTracker.Data --startup-project src/TimeTracker.App

# Format / restore
dotnet format src/TimeTracker.slnx
dotnet restore src/TimeTracker.slnx
```

## Architecture

Three-layer architecture targeting .NET 10 / Windows:

- **TimeTracker.Core** (`net10.0`) — Models, interfaces, and business-logic services. No UI or EF dependencies. This is what the test project references.
- **TimeTracker.Data** (`net10.0`) — EF Core 10 + SQLite. Repositories, entity configurations, and migrations. DB is stored in `%APPDATA%/TimeTracker/timetracker.db` via `DatabaseConfiguration.GetConnectionString()`.
- **TimeTracker.App** (`net10.0-windows`) — WPF presentation layer using WPF-UI (Fluent Design) and CommunityToolkit.Mvvm.

DI is wired entirely in `App.xaml.cs → ConfigureServices`. Migrations are applied automatically on startup. Localization must be initialized (`InitializeLocalization`) **before** any Window or Page is created.

### Key types

| Type | Location | Lifetime |
|------|----------|----------|
| `TimeTrackerDbContext` | `TimeTracker.Data` | Scoped |
| Repositories (`IActivityRepository`, `ITimeRecordRepository`, `ISettingsRepository`, `IWorkdayRepository`) | `TimeTracker.Data/Repositories` | Scoped |
| Core services (`IValidationService`, `ITimeCalculatorService`, `IWorkdayService`, etc.) | `TimeTracker.Core/Services` | Scoped |
| UI singletons (`INavigationService`, `IDialogService`, `ILocalizationService`, `ThemeService`, `INotificationService`, `IGlobalHotkeyService`, `IPageStateService`) | `TimeTracker.App/Services` | Singleton |
| Page ViewModels | `TimeTracker.App/ViewModels` | Transient |
| `MainWindowViewModel` / `MainWindow` | `TimeTracker.App` | Singleton |

### Domain models

- `TimeRecord` — a tracked interval with `DateOnly Date`, `TimeOnly StartTime`, `TimeOnly? EndTime`, FK to `Activity`, and a `Telework` flag.
- `Activity` — categories/tasks that records are assigned to.
- `Workday` — configuration for a specific date (day type, telework percentage).
- `AppSettings` — single-row settings table (language, theme, notifications, global hotkey, retention policy).

## Code Style

### Language

- All code, comments, and XML docs in **English (en-US)**.
- All user-facing strings in **Spanish (es-ES)** and **Catalan (ca-ES)** via `.resx` resource files (`Resources/Resources.resx` and `Resources/Resources.ca-ES.resx`). Never hardcode UI text.
- When validation errors are returned as resource keys with pipe-delimited arguments, localize them using `ILocalizationService` before displaying to the user.

### Namespace / using order

```csharp
namespace TimeTracker.Core.Services;  // file-scoped, no braces, blank line after

using System;                          // System first
using Microsoft.Extensions.Logging;   // Microsoft second
using TimeTracker.Core.Interfaces;    // project last
```

### Types and patterns

- Use `TimeOnly` for times, `DateOnly` for dates, `Guid` for all entity IDs.
- Nullable reference types enabled — use `?` for nullable, `null!` for EF `DbSet` initialization.
- Prefer `string.Empty` over `""`.
- Async all the way in repositories and services (`Task<T>`, `SaveChangesAsync`, `ToListAsync`, etc.).

### MVVM

ViewModels inherit `ObservableObject`, use `[ObservableProperty]` and `[RelayCommand]` from CommunityToolkit.Mvvm. Keep ViewModels constructor-injectable and testable.

### EF Core

Create a separate `IEntityTypeConfiguration<T>` class per entity in `TimeTracker.Data/Configurations`.

## Adding a New Feature

1. Define model in `TimeTracker.Core/Models`
2. Add interface in `TimeTracker.Core/Interfaces`
3. Implement service in `TimeTracker.Core/Services`
4. Create repository in `TimeTracker.Data/Repositories` with interface in `TimeTracker.Core/Interfaces`
5. Add EF configuration in `TimeTracker.Data/Configurations`
6. Create migration
7. Create ViewModel in `TimeTracker.App/ViewModels`
8. Create View (XAML + code-behind) in `TimeTracker.App/Views/Pages`
9. Register everything in `App.xaml.cs → ConfigureServices`
