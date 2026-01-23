#!/bin/bash

# Script per crear els issues de TimeTracker
# Requereix GitHub CLI (gh) instal·lat i autenticat

REPO="jaumeroig/time-tracker"

# Validació de prerequisits
echo "🔍 Validant prerequisits..."

# Comprovar si gh està instal·lat
if ! command -v gh &> /dev/null; then
    echo "❌ Error: GitHub CLI (gh) no està instal·lat."
    echo "   Instal·la'l des de: https://cli.github.com/"
    exit 1
fi

# Comprovar si l'usuari està autenticat
if ! gh auth status &> /dev/null; then
    echo "❌ Error: No estàs autenticat amb GitHub CLI."
    echo "   Executa: gh auth login"
    exit 1
fi

echo "✅ Prerequisites correctes!"
echo ""
echo "🚀 Creant issues per al projecte TimeTracker..."
echo ""

# Issue #1: Arquitectura
gh issue create \
  --repo "$REPO" \
  --title "Configurar arquitectura base amb projectes App, Core i Data" \
  --label "architecture,setup,priority:high" \
  --body "Crear l'estructura de la solució amb els tres projectes principals segons l'arquitectura definida:

### Projectes a crear:
- \`TimeTracker.App\` - Capa de presentació (WPF + WPF UI)
- \`TimeTracker.Core\` - Lògica de negoci i models
- \`TimeTracker.Data\` - Capa de persistència (EF Core + SQLite)

### Dependències:
- \`App\` → \`Core\`
- \`Core\` (no depèn de Data)
- \`Data\` implementa interfícies de \`Core\`

### Tecnologies:
- .NET 10
- WPF + WPF UI
- CommunityToolkit.Mvvm
- Entity Framework Core
- SQLite

### Criteris d'acceptació:
- [ ] Els tres projectes estan creats i configurats
- [ ] Les dependències entre projectes són correctes
- [ ] Els paquets NuGet necessaris estan instal·lats
- [ ] La solució compila sense errors
- [ ] S'ha configurat la injecció de dependències al projecte App"

echo "✅ Issue #1 creat"

# Issue #2: Data Layer
gh issue create \
  --repo "$REPO" \
  --title "Crear DbContext, entitats EF i repositoris per SQLite" \
  --label "data-layer,database,priority:high" \
  --body "Implementar la capa de persistència amb Entity Framework Core i SQLite.

### Tasques:
- [ ] Crear \`TimeTrackerDbContext\` amb configuració SQLite
- [ ] Definir entitats EF per:
  - \`Activity\` (Activitat)
  - \`TimeRecord\` (Registre de temps)
  - \`WorkdaySlot\` (Franja de jornada)
  - \`AppSettings\` (Configuració)
- [ ] Configurar fluent API per les relacions
- [ ] Implementar repositoris:
  - \`IActivityRepository\` / \`ActivityRepository\`
  - \`ITimeRecordRepository\` / \`TimeRecordRepository\`
  - \`IWorkdaySlotRepository\` / \`WorkdaySlotRepository\`
  - \`ISettingsRepository\` / \`SettingsRepository\`
- [ ] Crear migracions inicials
- [ ] Configurar ubicació de la BBDD: \`%LocalAppData%\\TimeTracker\\TimeTracker.db\`

### Criteris d'acceptació:
- [ ] DbContext està configurat correctament
- [ ] Totes les entitats estan definides amb les seves propietats
- [ ] Els repositoris implementen les interfícies de Core
- [ ] Les migracions es creen correctament
- [ ] La base de dades es crea a la ubicació correcta"

echo "✅ Issue #2 creat"

# Issue #3: Core Models
gh issue create \
  --repo "$REPO" \
  --title "Crear models de negoci, interfícies de repositoris i serveis" \
  --label "core,models,priority:high" \
  --body "Implementar els models de domini i les interfícies que defineixen els contractes de la capa de dades.

### Models a crear:
- \`Activity\` amb propietats: Id, Nom, Color, Activa/Inactiva
- \`TimeRecord\` amb propietats: Id, Data, Hora inici, Hora fi, Durada, Activitat, Notes
- \`WorkdaySlot\` amb propietats: Id, Data, Hora inici, Hora fi, Tipus (Casa/Oficina)
- \`AppSettings\` amb propietats: Tema, Notificacions, Opcions regionals

### Interfícies de repositoris:
- \`IActivityRepository\`
- \`ITimeRecordRepository\`
- \`IWorkdaySlotRepository\`
- \`ISettingsRepository\`

### Estructura:
\`\`\`
Core/
├── Models/
├── Interfaces/
├── Services/
└── Calculators/
\`\`\`

### Criteris d'acceptació:
- [ ] Tots els models de domini estan definits
- [ ] Les interfícies de repositoris estan definides
- [ ] Els models són independents d'EF (no references a Data)
- [ ] Les propietats reflecteixen l'especificació"

echo "✅ Issue #3 creat"

# Issue #4: Business Logic
gh issue create \
  --repo "$REPO" \
  --title "Crear serveis de validació i calculadores de temps" \
  --label "core,business-logic,validation,priority:high" \
  --body "Implementar les regles de negoci, validacions i càlculs necessaris per l'aplicació.

### Regles de negoci:
- L'hora de fi ha de ser posterior a l'hora d'inici
- No es permeten franges solapades en una mateixa jornada
- La durada sempre es calcula automàticament
- El percentatge de teletreball es calcula sobre hores totals
- Les activitats amb registres associats requereixen confirmació per eliminar-se

### Serveis a implementar:
- \`ITimeCalculatorService\` - Càlcul de durades, totals diaris, percentatges
- \`IValidationService\` - Validacions de registres i franges
- \`IWorkdayService\` - Lògica específica de jornada laboral

### Criteris d'acceptació:
- [ ] Totes les regles de negoci estan implementades
- [ ] Les validacions retornen missatges d'error clars
- [ ] Els càlculs de temps són precisos
- [ ] Els serveis tenen tests unitaris"

echo "✅ Issue #4 creat"

# Issue #5: Navigation
gh issue create \
  --repo "$REPO" \
  --title "Implementar navegació amb menú lateral i shell de l'aplicació" \
  --label "ui,navigation,priority:high" \
  --body "Crear l'estructura de navegació principal de l'aplicació amb WPF UI.

### Components:
- Shell principal amb menú lateral
- Sistema de navegació entre pàgines
- Menú amb les seccions:
  - 📝 Registres
  - 📅 Jornada
  - 🏷️ Activitats
  - ⚙️ Opcions

### Tecnologia:
- WPF UI NavigationView
- MVVM amb CommunityToolkit.Mvvm
- Dependency Injection per ViewModels

### Criteris d'acceptació:
- [ ] Shell principal està implementat
- [ ] Menú lateral mostra les 4 seccions
- [ ] La navegació entre pàgines funciona
- [ ] El disseny segueix l'estil Windows 11 modern
- [ ] Els ViewModels estan injectats correctament"

echo "✅ Issue #5 creat"

# Issue #6: Registres Feature
gh issue create \
  --repo "$REPO" \
  --title "Crear vista i funcionalitats per gestionar registres de temps" \
  --label "ui,feature:registres,priority:high" \
  --body "Desenvolupar la funcionalitat completa de gestió de registres de temps.

### Funcionalitats:
- [ ] Llistar registres amb informació: Data, Hora d'inici i fi, Durada calculada, Activitat, Notes opcionals
- [ ] Crear nou registre
- [ ] Editar registre existent
- [ ] Eliminar registre
- [ ] Agrupació per dia
- [ ] Càlcul de total diari treballat
- [ ] Filtre per data (calendari/rang)
- [ ] Filtre per activitat (dropdown)
- [ ] Cerca per text (activitat o notes)

### Components UI:
- \`RegistresView.xaml\` / \`RegistresViewModel\`
- Formulari d'edició (diàleg o panel)
- Controls de filtre
- Llista/grid amb agrupació

### Criteris d'acceptació:
- [ ] Es poden crear, editar i eliminar registres
- [ ] Els filtres funcionen correctament
- [ ] La cerca per text és funcional
- [ ] L'agrupació per dia mostra totals
- [ ] El disseny és clar i modern (estil cards)"

echo "✅ Issue #6 creat"

# Issue #7: Jornada Feature
gh issue create \
  --repo "$REPO" \
  --title "Crear vista de calendari i gestió de jornada laboral" \
  --label "ui,feature:jornada,calendar,priority:high" \
  --body "Desenvolupar la funcionalitat de control de jornada diària amb calendari mensual.

### Funcionalitats:
- [ ] Calendari mensual navegable
- [ ] Selecció de dia
- [ ] Gestió de franges horàries per dia seleccionat
- [ ] Indicació de lloc de treball: 🏠 Casa (teletreball) / 🏢 Oficina
- [ ] Càlcul automàtic per dia: Total d'hores, % teletreball, Diferència respecte jornada objectiu
- [ ] Resum mensual: Total hores treballat al mes, % oficina vs teletreball del mes

### Components UI:
- \`JornadaView.xaml\` / \`JornadaViewModel\`
- Control de calendari mensual
- Panel de detall del dia
- Llista de franges horàries
- Resum mensual (cards amb estadístiques)

### Criteris d'acceptació:
- [ ] El calendari mostra el mes actual
- [ ] Es pot navegar entre mesos
- [ ] Es poden afegir/editar/eliminar franges per dia
- [ ] Els càlculs de percentatges són correctes
- [ ] El resum mensual s'actualitza automàticament
- [ ] No es permeten franges solapades (validació)"

echo "✅ Issue #7 creat"

# Issue #8: Activitats Feature
gh issue create \
  --repo "$REPO" \
  --title "Crear gestió de activitats/conceptes amb visualització en cards" \
  --label "ui,feature:activitats,priority:medium" \
  --body "Desenvolupar la funcionalitat de manteniment d'activitats o conceptes.

### Funcionalitats:
- [ ] Llistar activitats en format card
- [ ] Crear nova activitat
- [ ] Editar activitat existent
- [ ] Eliminar activitat (amb confirmació si té registres)
- [ ] Cada card mostra: Nom, Color identificatiu, Temps total acumulat, Nombre de registres associats, Estat (Activa/Inactiva)

### Components UI:
- \`ActivitatsView.xaml\` / \`ActivitatsViewModel\`
- Cards per cada activitat
- Formulari d'edició (diàleg)
- Selector de color

### Criteris d'acceptació:
- [ ] Les activitats es mostren en cards
- [ ] Es poden crear, editar i eliminar activitats
- [ ] El selector de color funciona
- [ ] Les estadístiques (temps i registres) són correctes
- [ ] Es demana confirmació per eliminar activitats amb registres
- [ ] El disseny segueix l'estil modern amb cards"

echo "✅ Issue #8 creat"

# Issue #9: Settings Feature
gh issue create \
  --repo "$REPO" \
  --title "Crear configuració general de l'aplicació" \
  --label "ui,feature:settings,priority:medium" \
  --body "Desenvolupar la pantalla d'opcions amb configuració general.

### Seccions:
- **Aparença:** ☀️ Clar / 🌙 Fosc / 💻 Sistema
- **Notificacions:** Activar/desactivar recordatoris
- **Temporitzador:** Inici automàtic (preparació per funcionalitats futures)
- **Regional:** Primer dia de la setmana (Dilluns/Diumenge)
- **Sobre l'aplicació:** Versió, Plataforma (.NET 10, Windows), Enllaços (GitHub, llicència)

### Components UI:
- \`OpcionsView.xaml\` / \`OpcionsViewModel\`
- Controls de configuració (RadioButtons, ToggleSwitch, ComboBox)
- Secció \"Sobre\" amb informació de versió

### Criteris d'acceptació:
- [ ] Es pot canviar el tema (clar/fosc/sistema)
- [ ] El canvi de tema s'aplica immediatament
- [ ] Les opcions es guarden a la base de dades
- [ ] La informació de versió es mostra correctament
- [ ] El disseny és clar i organitzat per seccions"

echo "✅ Issue #9 creat"

# Issue #10: Theming
gh issue create \
  --repo "$REPO" \
  --title "Integrar sistema de temes de WPF UI" \
  --label "ui,theming,priority:medium" \
  --body "Implementar el canvi de tema amb suport per mode clar, fosc i seguiment del sistema.

### Funcionalitats:
- Integració amb WPF UI Theme Manager
- Persistència de la selecció a la base de dades
- Aplicació del tema a l'inici de l'aplicació
- Canvi dinàmic sense reiniciar

### Criteris d'acceptació:
- [ ] Els tres temes funcionen correctament
- [ ] El tema seleccionat es guarda i carrega
- [ ] El mode \"Sistema\" segueix el tema de Windows
- [ ] El canvi de tema és immediat"

echo "✅ Issue #10 creat"

# Issue #11: Helpers
gh issue create \
  --repo "$REPO" \
  --title "Crear converters XAML i behaviors per la UI" \
  --label "ui,helpers,priority:low" \
  --body "Implementar els converters i behaviors necessaris per la UI.

### Converters a crear:
- \`TimeSpanToStringConverter\` - Format de durades
- \`BoolToVisibilityConverter\` - Visibilitat condicional
- \`ColorToBrushConverter\` - Colors d'activitats
- \`DateFormatConverter\` - Format de dates
- \`PercentageConverter\` - Percentatges de jornada

### Behaviors:
- \`NumericTextBoxBehavior\` - Input només numèric
- \`SelectAllOnFocusBehavior\` - Selecció automàtica de text

### Criteris d'acceptació:
- [ ] Tots els converters funcionen correctament
- [ ] Els behaviors milloren l'experiència d'usuari
- [ ] El codi és reutilitzable"

echo "✅ Issue #11 creat"

# Issue #12: Dialogs
gh issue create \
  --repo "$REPO" \
  --title "Crear sistema de diàlegs i missatges d'usuari" \
  --label "ui,dialogs,priority:medium" \
  --body "Implementar diàlegs per confirmacions, errors i informació.

### Components:
- Diàlegs de confirmació (eliminar registre/activitat)
- Diàlegs d'error (validacions fallides)
- Notificacions toast (operacions exitoses)
- Diàlegs de selecció (color, data)

### Tecnologia:
- WPF UI ContentDialog
- WPF UI Snackbar/InfoBar

### Criteris d'acceptació:
- [ ] Els diàlegs són consistents visualment
- [ ] Les confirmacions prevenen eliminacions accidentals
- [ ] Els missatges d'error són clars
- [ ] Les notificacions no són intrusives"

echo "✅ Issue #12 creat"

# Issue #13: Unit Tests
gh issue create \
  --repo "$REPO" \
  --title "Crear tests per models, serveis i validacions" \
  --label "testing,core,priority:medium" \
  --body "Implementar tests unitaris per la capa Core.

### Àrees a testejar:
- Validacions (dates, franges, solapaments)
- Càlculs de temps (durades, percentatges)
- Regles de negoci
- Serveis

### Framework:
- xUnit o NUnit
- Moq per mocks

### Criteris d'acceptació:
- [ ] Cobertura >80% a Core
- [ ] Tots els casos límit estan testejats
- [ ] Els tests són mantenibles"

echo "✅ Issue #13 creat"

# Issue #14: Performance
gh issue create \
  --repo "$REPO" \
  --title "Afegir índexs i optimitzar queries a la base de dades" \
  --label "performance,data-layer,priority:low" \
  --body "Optimitzar el rendiment de les consultes més freqüents.

### Optimitzacions:
- Índexs a columnes de cerca (Data, ActivityId)
- Eager loading per relacions
- Paginació a llistes grans
- Projecció a DTOs per queries complexes

### Criteris d'acceptació:
- [ ] Les consultes per data són ràpides
- [ ] El filtratge per activitat és eficient
- [ ] No hi ha consultes N+1"

echo "✅ Issue #14 creat"

# Issue #15: Documentation
gh issue create \
  --repo "$REPO" \
  --title "Crear documentació tècnica del projecte" \
  --label "documentation,priority:low" \
  --body "Documentar l'arquitectura, estructura i guies per a desenvolupadors.

### Documents a crear:
- \`ARCHITECTURE.md\` - Arquitectura de la solució
- \`DEVELOPMENT.md\` - Guia de configuració i desenvolupament
- Comentaris XML a interfícies públiques
- README actualitzat amb captures de pantalla

### Criteris d'acceptació:
- [ ] L'arquitectura està documentada
- [ ] La guia de desenvolupament és clara
- [ ] Els comentaris XML són útils
- [ ] El README inclou captures i instruccions"

echo "✅ Issue #15 creat"

# Issue #16: Icons
gh issue create \
  --repo "$REPO" \
  --title "Configurar icones i recursos gràfics de l'aplicació" \
  --label "ui,design,priority:low" \
  --body "Preparar tots els recursos visuals necessaris.

### Recursos:
- Icona de l'aplicació (.ico)
- Icones del menú (Segoe Fluent Icons)
- Icones d'accions (afegir, editar, eliminar)
- Splash screen (opcional)

### Criteris d'acceptació:
- [ ] L'aplicació té una icona pròpia
- [ ] Les icones del menú són clares
- [ ] El disseny és consistent"

echo "✅ Issue #16 creat"

# Issue #17: Build and Release
gh issue create \
  --repo "$REPO" \
  --title "Preparar configuració per publicació de l'aplicació" \
  --label "devops,deployment,priority:low" \
  --body "Configurar el procés de build i empaquetatge.

### Tasques:
- Configurar versió de l'aplicació
- Self-contained deployment
- Single-file executable (opcional)
- Crear instal·lador amb WiX o MSIX (opcional)

### Criteris d'acceptació:
- [ ] L'aplicació es pot publicar
- [ ] El versionat és correcte
- [ ] L'executable funciona de manera independent"

echo "✅ Issue #17 creat"

echo ""
echo "🎉 Tots els issues han estat creats correctament!"
echo ""
echo "📋 Resum:"
echo "   - 17 issues creats"
echo "   - Labels assignats"
echo "   - Prioritats definides"
echo ""
echo "🔗 Visualitza els issues a: https://github.com/$REPO/issues"
