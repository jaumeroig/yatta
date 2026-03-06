# Yatta (Yet Another Time Tracker App)

> Yatta (やった) es una expresión coloquial japonesa que significa "¡Lo hice!", "¡Lo logré!" o "¡Bien!". Se utiliza para expresar alegría, alivio o celebración tras alcanzar una meta, superar un desafío o finalizar un trabajo. Proviene del verbo yaru (hacer) y se traduce frecuentemente como "listo" o "viola".

Esta aplicación de escritorio desarrollada en WPF permite a los usuarios registrar y gestionar su tiempo de trabajo. Con una interfaz moderna basada en Fluent Design, facilita la imputación de horas y el registro de las horas trabajadas en cada jornada y el porcentaje de horas de teletrabajo.

## 🎯 Funcionalidades principales

Yatta te ayuda a registrar y gestionar tu tiempo de trabajo de forma intuitiva y eficiente. Puedes iniciar y detener actividades con un solo clic, registrar las horas de trabajo realizadas en oficina o teletrabajo, y obtener análisis detallados de cómo distribuyes tu tiempo.

La aplicación te permite **trabajar en tiempo real**: simplemente selecciona una actividad y pulsa el botón de inicio. El seguimiento comenzará automáticamente mientras trabajas. Cuando termines, detén la actividad y el registro quedará guardado con la duración exacta. Si necesitas registrar horas de forma manual para completar días anteriores o ajustar entradas, también puedes hacerlo especificando las horas de inicio y fin.

Puedes **marcar qué registros corresponden a teletrabajo**, permitiéndote llevar un control preciso del porcentaje de trabajo remoto. La aplicación calcula automáticamente las horas totales, las horas de teletrabajo y te muestra una barra visual que representa tu jornada completa de un vistazo.

**Analiza tu tiempo** desde múltiples perspectivas: consulta el detalle de un día concreto, revisa cómo has distribuido tu tiempo durante la semana, obtén totales mensuales o visualiza las tendencias anuales. Cada vista incluye gráficos y estadísticas que te ayudan a entender cómo inviertes tu tiempo en cada proyecto o actividad.

El **histórico completo** de tus registros está siempre accesible. Puedes buscar y filtrar por fechas o actividades específicas, editar entradas pasadas si cometiste algún error, y consultar estadísticas acumuladas de cualquier periodo. Esto te permite generar informes precisos de las horas trabajadas en cada proyecto.

Organiza tu trabajo mediante **actividades personalizadas** que representan tus proyectos, tareas. Para cada actividad, puedes consultar cuánto tiempo has dedicado en total, ver todos los registros asociados y activarla o desactivarla según tus necesidades actuales.

La aplicación es completamente **personalizable**: elige entre temas claro, oscuro o automático según tu sistema, activa notificaciones periódicas para recordarte registrar tu tiempo, cambia el idioma entre español y catalán, configura atajos de teclado globales para acceder rápidamente sin salir de otras aplicaciones, e incluso define políticas de retención para limpiar automáticamente registros antiguos manteniendo tu base de datos optimizada.


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

## 📦 Estructura del proyecto
```
src/
├── TimeTracker.App/           # Aplicación WPF (capa de presentación)
│   ├── Views/
│   │   ├── Pages/            # Páginas principales (Hoy, Panel de Control, Histórico, etc.)
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