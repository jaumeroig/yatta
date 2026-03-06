# TimeTracker

TimeTracker es una aplicación de escritorio desarrollada en WPF que permite a los usuarios registrar y gestionar su tiempo de trabajo. Con una interfaz moderna basada en Fluent Design, facilita la imputación de horas y permite registrar las horas trabajadas en cada jornada y el porcentaje de horas de teletrabajo.

## 🎯 Funcionalidades principales

### Vista Hoy (Today)
Control rápido del día actual con:
- Inicio/parada de actividades en tiempo real
- Visualización de registros activos y completados
- Edición y eliminación de registros del día
- Indicador de teletrabajo por registro

### Panel de Control (Dashboard)
Análisis y estadísticas de tu tiempo de trabajo con vistas:
- **Día**: Resumen detallado de una jornada específica
- **Semana**: Análisis semanal con distribución por días
- **Mes**: Vista mensual con totales y promedios
- **Año**: Resumen anual con tendencias

### Histórico (Historic)
Gestión completa de registros pasados:
- Búsqueda y filtrado por fecha y actividad
- Edición de registros históricos
- Vista detallada por día
- Estadísticas acumuladas

### Actividades (Activities)
Organización de tus actividades o proyectos:
- Creación y gestión de actividades
- Estadísticas de tiempo por actividad
- Detalles de uso y registros asociados
- Activación/desactivación de actividades

### Configuración (Settings)
Personalización de la aplicación:
- **Apariencia**: Temas claro, oscuro o del sistema
- **Notificaciones**: Recordatorios personalizables con intervalo y duración
- **Idioma**: Soporte para Español y Català
- **Sistema**: Inicio automático con Windows, minimizar a bandeja
- **Atajos**: Teclas globales para acceso rápido
- **Retención de datos**: Política de limpieza automática de registros antiguos


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
git clone https://github.com/jaumeroig/yatta.git
cd yatta
```

### Compilar la solución
```bash
# Build en modo Debug
dotnet build src/TimeTracker.slnx

# Build en modo Release
dotnet build src/TimeTracker.slnx -c Release
```

### Ejecutar la aplicación
```bash
dotnet run --project src/TimeTracker.App/TimeTracker.App.csproj
```

### Ejecutar tests
```bash
# Ejecutar todos los tests
dotnet test src/TimeTracker.slnx

# Ejecutar un test específico
dotnet test --filter "FullyQualifiedName~ValidationServiceTests.ValidateTimeRange_ShouldReturnTrue"
```

### Migraciones de base de datos
```bash
# Añadir nueva migración
dotnet ef migrations add MigrationName --project src/TimeTracker.Data --startup-project src/TimeTracker.App

# Aplicar migraciones
dotnet ef database update --project src/TimeTracker.Data --startup-project src/TimeTracker.App

# Eliminar última migración
dotnet ef migrations remove --project src/TimeTracker.Data --startup-project src/TimeTracker.App
```

## 📦 Estructura del proyecto
```
src/
├── TimeTracker.App/           # Aplicación WPF (capa de presentación)
│   ├── Views/
│   │   ├── Pages/            # Páginas principales (Today, Dashboard, Historic, etc.)
│   │   └── Dialogs/          # Controles de diálogos reutilizables
│   ├── ViewModels/           # ViewModels (MVVM)
│   ├── Controls/             # Controles personalizados
│   ├── Services/             # Servicios de UI (navegación, diálogos, notificaciones, etc.)
│   ├── Resources/            # Recursos (cadenas localizadas, estilos)
│   ├── Converters/           # Convertidores de datos para binding
│   └── Models/               # Modelos específicos de UI
├── TimeTracker.Core/          # Lógica de negocio
│   ├── Models/               # Modelos de dominio (TimeRecord, Activity, Workday, etc.)
│   ├── Interfaces/           # Interfaces de servicios y repositorios
│   ├── Services/             # Implementación de servicios de negocio
│   ├── Extensions/           # Métodos de extensión
│   └── Attributes/           # Atributos personalizados
├── TimeTracker.Data/          # Capa de datos
│   ├── Repositories/         # Implementación de repositorios
│   ├── Configurations/       # Configuraciones de Entity Framework
│   └── Migrations/           # Migraciones de base de datos
└── TimeTracker.Tests/         # Tests unitarios (xUnit + Moq)
    └── Core/                 # Tests de TimeTracker.Core
```


## 🗄️ Base de datos

La aplicación utiliza SQLite como base de datos local. El archivo de base de datos se almacena en:
```
%APPDATA%/TimeTracker/timetracker.db
```

Las migraciones de Entity Framework se aplican automáticamente al iniciar la aplicación.

## ✨ Características destacadas

### Interfaz moderna
- Diseño basado en Fluent Design (Windows 11)
- Soporte completo para temas claro, oscuro y del sistema
- Animaciones y transiciones fluidas
- Controles personalizados optimizados (TimePickerControl, HotkeyTextBox, etc.)

### Gestión inteligente de tiempo
- Detección automática de registros obsoletos (actividades abiertas de días anteriores)
- Cálculo automático de duraciones
- Validación de rangos horarios
- Soporte para trabajo activo en tiempo real

### Productividad
- Atajos de teclado globales para acceso rápido
- Notificaciones inteligentes con recordatorios configurables
- Minimización a bandeja del sistema
- Inicio automático con Windows
- Retención automática de datos con políticas configurables

### Localización
- Soporte multiidioma (Español y Català)
- Todos los textos de la interfaz localizados
- Cambio de idioma sin reiniciar la aplicación

### Arquitectura robusta
- Inyección de dependencias en toda la aplicación
- Patrón MVVM con CommunityToolkit.Mvvm
- Patrón Repository para acceso a datos
- Separación clara de responsabilidades (App, Core, Data)
- Tests unitarios con xUnit y Moq