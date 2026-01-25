# TimeTracker

TimeTracker es una aplicación de escritorio desarrollada en WPF que permite a los usuarios registrar y gestionar su tiempo lade trabajo. Con una interfaz moderna y fácil de usar, facilita la imputación de horas y permite registrar las horas trabajadas en cada jornada y el porcentaje de horas de teletrabajo.

## 🎯 Funcionalidades principales

- **Registros**: Gestión de registros de tiempo con inicio, fin y duración
- **Jornada**: Vista de calendario para seguimiento diario
- **Actividades**: Clasificación y organización de tareas
- **Opciones**: Configuración de la aplicación y preferencias de usuario


## 📚 Arquitectura

Este proyecto sigue una arquitectura de 3 capas:
- **TimeTracker.App** - Capa de presentación (WPF + WPF UI)
- **TimeTracker.Core** - Lógica de negocio y modelos
- **TimeTracker.Data** - Capa de persistencia (EF Core + SQLite)

## 🛠️ Tecnologías

- **.NET 10** - Framework de desarrollo
- **WPF** - Windows Presentation Foundation
- **WPF-UI** - Biblioteca de componentes UI modernos
- **CommunityToolkit.Mvvm** - Herramientas para implementar MVVM
- **Microsoft.Extensions.DependencyInjection** - Inyección de dependencias
- **Entity Framework Core 10.0.2** - ORM para acceso a datos
- **SQLite** - Base de datos local

## 🚀 Requisitos previos

- Windows 10 o superior
- .NET 10 SDK

## 🔧 Compilación y ejecución

### Clonar el repositorio
```bash
git clone https://github.com/jaumeroig/time-tracker.git
cd time-tracker
```

### Compilar la solución
```bash
dotnet build src/TimeTracker.slnx
```

### Ejecutar la aplicación
```bash
dotnet run --project src/TimeTracker.App/TimeTracker.App.csproj
```

## 📦 Estructura del proyecto
```
src/
├── TimeTracker.App/        # Aplicación WPF (capa de presentación)
│   ├── Views/             # Vistas XAML
│   ├── ViewModels/        # ViewModels (MVVM)
│   ├── Resources/         # Recursos (cadenas, estilos)
│   └── Services/          # Servicios de UI
├── TimeTracker.Core/       # Lógica de negocio
│   ├── Models/            # Modelos de dominio
│   ├── Interfaces/        # Interfaces de servicios
│   └── Services/          # Implementación de servicios
└── TimeTracker.Data/       # Capa de datos
    ├── Repositories/      # Repositorios
    ├── Configurations/    # Configuraciones de EF
    └── Migrations/        # Migraciones de base de datos
```


## 🗄️ Base de datos

La aplicación utiliza SQLite como base de datos local. El archivo de base de datos se almacena en:
```
%APPDATA%/TimeTracker/timetracker.db
```