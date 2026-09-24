# Yatta (Yet Another Time Tracker App)

> Yatta (やった) es una expresión coloquial japonesa que significa "¡Lo hice!", "¡Lo logré!" o "¡Bien!". Se utiliza para expresar alegría, alivio o celebración tras alcanzar una meta, superar un desafío o finalizar un trabajo. Proviene del verbo yaru (hacer) y se traduce frecuentemente como "listo" o "viola".

Esta aplicación de escritorio para Windows utiliza Blazor Hybrid con Blazor Blueprint dentro de ventanas WPF. Permite registrar y gestionar el tiempo de trabajo, las jornadas y el porcentaje de teletrabajo.

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
- **Yatta.App** - Pantallas Razor con Blazor Blueprint; WPF aloja BlazorWebView y conserva las integraciones de Windows
- **Yatta.Core** - Lógica de negocio y modelos
- **Yatta.Data** - Capa de persistencia (EF Core + SQLite)

## 🛠️ Tecnologías

- **.NET 10** - Framework de desarrollo
- **Blazor Hybrid y Blazor Blueprint** - Interfaz, navegación y componentes
- **WPF, WPF-UI y WebView2** - Ventanas e integraciones nativas de Windows
- **Microsoft.Extensions.DependencyInjection** - Inyección de dependencias
- **Entity Framework Core 10** - ORM para acceso a datos
- **SQLite** - Base de datos local

## 🚀 Requisitos previos

- Windows 10 o superior
- .NET 10 SDK
- Microsoft Edge WebView2 Runtime para ejecución fuera del instalador. El paquete Velopack lo instala como requisito.

## 🔧 Compilación y ejecución

### Clonar el repositorio
```bash
git clone https://github.com/jaumeroig/yatta.git
cd yatta
```

### Compilar la solución
```bash
# Build en modo Debug
dotnet build src/Yatta.slnx

# Build en modo Release
dotnet build src/Yatta.slnx -c Release
```

### Ejecutar la aplicación
```bash
dotnet run --project src/Yatta.App/Yatta.App.csproj
```

### Ejecutar tests
```bash
# Ejecutar todos los tests
dotnet test src/Yatta.slnx

# Ejecutar un test específico
dotnet test --filter "FullyQualifiedName~ValidationServiceTests.ValidateTimeRange_ShouldReturnTrue"
```

La compilación cruzada desde macOS es posible con `EnableWindowsTargeting`, pero la aplicación y las pruebas que referencian WPF se ejecutan en Windows. La publicación usa `dotnet publish src/Yatta.App/Yatta.App.csproj -c Release -r win-x64 --self-contained true`; el flujo de release mantiene `Yatta.exe`, `packId Yatta` y añade el requisito `webview2` al instalador Velopack.

## 📦 Estructura del proyecto
```
src/
├── Yatta.App/           # Host WPF y pantallas Blazor Hybrid
│   ├── Blazor/               # Avui, Històric, Activitats, Informes, Configuració i Novetats
│   ├── wwwroot/              # Estilos y host HTML de BlazorWebView
│   ├── Services/             # Acciones compartidas e integraciones Windows
│   └── Resources/            # Recursos es-ES y ca-ES
├── Yatta.Core/          # Lógica de negocio
│   ├── Models/               # Modelos de dominio (TimeRecord, Activity, Workday, etc.)
│   ├── Interfaces/           # Interfaces de servicios y repositorios
│   ├── Services/             # Implementación de servicios de negocio
│   ├── Extensions/           # Métodos de extensión
│   └── Attributes/           # Atributos personalizados
├── Yatta.Data/          # Capa de datos
│   ├── Repositories/         # Implementación de repositorios
│   ├── Configurations/       # Configuraciones de Entity Framework
│   └── Migrations/           # Migraciones de base de datos
└── Yatta.Tests/         # Tests unitarios (xUnit + Moq)
    └── Core/                 # Tests de Yatta.Core
```


## 🗄️ Base de datos

La aplicación utiliza SQLite como base de datos local. El archivo de base de datos se almacena en:
```
%LOCALAPPDATA%\Yatta\Yatta.db
```

Las migraciones de Entity Framework se aplican automáticamente al iniciar la aplicación.
La migración de interfaz reutiliza esta misma base de datos y no modifica las entidades ni las migraciones.

## ✨ Características destacadas

### Interfaz moderna
- Interfaz Blazor Blueprint de base neutra con acento azul
- Soporte completo para temas claro, oscuro y del sistema
- Animaciones y transiciones fluidas
- Pantallas adaptadas a la ventana mínima, uso con teclado y acciones rápidas desde la bandeja

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
- Servicios de acciones compartidas por las ventanas Blazor
- Patrón Repository para acceso a datos
- Separación clara de responsabilidades (App, Core, Data)
- Tests unitarios con xUnit y Moq
