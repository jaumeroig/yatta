# Issues a crear per TimeTracker

Aquest document conté tots els issues que cal crear per implementar l'aplicació TimeTracker segons l'especificació proporcionada.

---

## 📋 Issue #1: Configurar l'estructura de la solució amb 3 projectes

**Títol:** Configurar arquitectura base amb projectes App, Core i Data

**Descripció:**
Crear l'estructura de la solució amb els tres projectes principals segons l'arquitectura definida:

### Projectes a crear:
- `TimeTracker.App` - Capa de presentació (WPF + WPF UI)
- `TimeTracker.Core` - Lògica de negoci i models
- `TimeTracker.Data` - Capa de persistència (EF Core + SQLite)

### Dependències:
- `App` → `Core`
- `Core` (no depèn de Data)
- `Data` implementa interfícies de `Core`

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
- [ ] S'ha configurat la injecció de dependències al projecte App

**Labels:** architecture, setup, priority:high

---

## 📋 Issue #2: Implementar capa de dades (TimeTracker.Data)

**Títol:** Crear DbContext, entitats EF i repositoris per SQLite

**Descripció:**
Implementar la capa de persistència amb Entity Framework Core i SQLite.

### Tasques:
- [ ] Crear `TimeTrackerDbContext` amb configuració SQLite
- [ ] Definir entitats EF per:
  - `Activity` (Activitat)
  - `TimeRecord` (Registre de temps)
  - `WorkdaySlot` (Franja de jornada)
  - `AppSettings` (Configuració)
- [ ] Configurar fluent API per les relacions
- [ ] Implementar repositoris:
  - `IActivityRepository` / `ActivityRepository`
  - `ITimeRecordRepository` / `TimeRecordRepository`
  - `IWorkdaySlotRepository` / `WorkdaySlotRepository`
  - `ISettingsRepository` / `SettingsRepository`
- [ ] Crear migracions inicials
- [ ] Configurar ubicació de la BBDD: `%LocalAppData%\TimeTracker\TimeTracker.db`

### Criteris d'acceptació:
- [ ] DbContext està configurat correctament
- [ ] Totes les entitats estan definides amb les seves propietats
- [ ] Els repositoris implementen les interfícies de Core
- [ ] Les migracions es creen correctament
- [ ] La base de dades es crea a la ubicació correcta

**Labels:** data-layer, database, priority:high

---

## 📋 Issue #3: Definir models de domini i interfícies (TimeTracker.Core)

**Títol:** Crear models de negoci, interfícies de repositoris i serveis

**Descripció:**
Implementar els models de domini i les interfícies que defineixen els contractes de la capa de dades.

### Models a crear:
- `Activity` amb propietats:
  - Id, Nom, Color, Activa/Inactiva
- `TimeRecord` amb propietats:
  - Id, Data, Hora inici, Hora fi, Durada, Activitat, Notes
- `WorkdaySlot` amb propietats:
  - Id, Data, Hora inici, Hora fi, Tipus (Casa/Oficina)
- `AppSettings` amb propietats:
  - Tema, Notificacions, Opcions regionals

### Interfícies de repositoris:
- `IActivityRepository`
- `ITimeRecordRepository`
- `IWorkdaySlotRepository`
- `ISettingsRepository`

### Estructura:
```
Core/
├── Models/
├── Interfaces/
├── Services/
└── Calculators/
```

### Criteris d'acceptació:
- [ ] Tots els models de domini estan definits
- [ ] Les interfícies de repositoris estan definides
- [ ] Els models són independents d'EF (no references a Data)
- [ ] Les propietats reflecteixen l'especificació

**Labels:** core, models, priority:high

---

## 📋 Issue #4: Implementar validacions i càlculs de negoci

**Títol:** Crear serveis de validació i calculadores de temps

**Descripció:**
Implementar les regles de negoci, validacions i càlculs necessaris per l'aplicació.

### Regles de negoci:
- L'hora de fi ha de ser posterior a l'hora d'inici
- No es permeten franges solapades en una mateixa jornada
- La durada sempre es calcula automàticament
- El percentatge de teletreball es calcula sobre hores totals
- Les activitats amb registres associats requereixen confirmació per eliminar-se

### Serveis a implementar:
- `ITimeCalculatorService` - Càlcul de durades, totals diaris, percentatges
- `IValidationService` - Validacions de registres i franges
- `IWorkdayService` - Lògica específica de jornada laboral

### Criteris d'acceptació:
- [ ] Totes les regles de negoci estan implementades
- [ ] Les validacions retornen missatges d'error clars
- [ ] Els càlculs de temps són precisos
- [ ] Els serveis tenen tests unitaris

**Labels:** core, business-logic, validation, priority:high

---

## 📋 Issue #5: Configurar navegació i shell principal

**Títol:** Implementar navegació amb menú lateral i shell de l'aplicació

**Descripció:**
Crear l'estructura de navegació principal de l'aplicació amb WPF UI.

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
- [ ] Els ViewModels estan injectats correctament

**Labels:** ui, navigation, priority:high

---

## 📋 Issue #6: Implementar pàgina de Registres

**Títol:** Crear vista i funcionalitats per gestionar registres de temps

**Descripció:**
Desenvolupar la funcionalitat completa de gestió de registres de temps.

### Funcionalitats:
- [ ] Llistar registres amb informació:
  - Data
  - Hora d'inici i fi
  - Durada calculada
  - Activitat
  - Notes opcionals
- [ ] Crear nou registre
- [ ] Editar registre existent
- [ ] Eliminar registre
- [ ] Agrupació per dia
- [ ] Càlcul de total diari treballat
- [ ] Filtre per data (calendari/rang)
- [ ] Filtre per activitat (dropdown)
- [ ] Cerca per text (activitat o notes)

### Components UI:
- `RegistresView.xaml` / `RegistresViewModel`
- Formulari d'edició (diàleg o panel)
- Controls de filtre
- Llista/grid amb agrupació

### Criteris d'acceptació:
- [ ] Es poden crear, editar i eliminar registres
- [ ] Els filtres funcionen correctament
- [ ] La cerca per text és funcional
- [ ] L'agrupació per dia mostra totals
- [ ] El disseny és clar i modern (estil cards)

**Labels:** ui, feature:registres, priority:high

---

## 📋 Issue #7: Implementar pàgina de Jornada

**Títol:** Crear vista de calendari i gestió de jornada laboral

**Descripció:**
Desenvolupar la funcionalitat de control de jornada diària amb calendari mensual.

### Funcionalitats:
- [ ] Calendari mensual navegable
- [ ] Selecció de dia
- [ ] Gestió de franges horàries per dia seleccionat
- [ ] Indicació de lloc de treball:
  - 🏠 Casa (teletreball)
  - 🏢 Oficina
- [ ] Càlcul automàtic per dia:
  - Total d'hores treballades
  - % teletreball del dia
  - Diferència respecte jornada objectiu
- [ ] Resum mensual:
  - Total hores treballat al mes
  - % oficina vs teletreball del mes

### Components UI:
- `JornadaView.xaml` / `JornadaViewModel`
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
- [ ] No es permeten franges solapades (validació)

**Labels:** ui, feature:jornada, calendar, priority:high

---

## 📋 Issue #8: Implementar pàgina d'Activitats

**Títol:** Crear gestió de activitats/conceptes amb visualització en cards

**Descripció:**
Desenvolupar la funcionalitat de manteniment d'activitats o conceptes.

### Funcionalitats:
- [ ] Llistar activitats en format card
- [ ] Crear nova activitat
- [ ] Editar activitat existent
- [ ] Eliminar activitat (amb confirmació si té registres)
- [ ] Cada card mostra:
  - Nom de l'activitat
  - Color identificatiu
  - Temps total acumulat
  - Nombre de registres associats
  - Estat (Activa/Inactiva)

### Components UI:
- `ActivitatsView.xaml` / `ActivitatsViewModel`
- Cards per cada activitat
- Formulari d'edició (diàleg)
- Selector de color

### Criteris d'acceptació:
- [ ] Les activitats es mostren en cards
- [ ] Es poden crear, editar i eliminar activitats
- [ ] El selector de color funciona
- [ ] Les estadístiques (temps i registres) són correctes
- [ ] Es demana confirmació per eliminar activitats amb registres
- [ ] El disseny segueix l'estil modern amb cards

**Labels:** ui, feature:activitats, priority:medium

---

## 📋 Issue #9: Implementar pàgina d'Opcions

**Títol:** Crear configuració general de l'aplicació

**Descripció:**
Desenvolupar la pantalla d'opcions amb configuració general.

### Seccions:
- **Aparença:**
  - ☀️ Clar
  - 🌙 Fosc
  - 💻 Sistema
- **Notificacions:**
  - Activar/desactivar recordatoris
- **Temporitzador:**
  - Inici automàtic (preparació per funcionalitats futures)
- **Regional:**
  - Primer dia de la setmana (Dilluns/Diumenge)
- **Sobre l'aplicació:**
  - Versió
  - Plataforma (.NET 10, Windows)
  - Enllaços (GitHub, llicència)

### Components UI:
- `OpcionsView.xaml` / `OpcionsViewModel`
- Controls de configuració (RadioButtons, ToggleSwitch, ComboBox)
- Secció "Sobre" amb informació de versió

### Criteris d'acceptació:
- [ ] Es pot canviar el tema (clar/fosc/sistema)
- [ ] El canvi de tema s'aplica immediatament
- [ ] Les opcions es guarden a la base de dades
- [ ] La informació de versió es mostra correctament
- [ ] El disseny és clar i organitzat per seccions

**Labels:** ui, feature:settings, priority:medium

---

## 📋 Issue #10: Implementar gestió de temes (Clar/Fosc/Sistema)

**Títol:** Integrar sistema de temes de WPF UI

**Descripció:**
Implementar el canvi de tema amb suport per mode clar, fosc i seguiment del sistema.

### Funcionalitats:
- Integració amb WPF UI Theme Manager
- Persistència de la selecció a la base de dades
- Aplicació del tema a l'inici de l'aplicació
- Canvi dinàmic sense reiniciar

### Criteris d'acceptació:
- [ ] Els tres temes funcionen correctament
- [ ] El tema seleccionat es guarda i carrega
- [ ] El mode "Sistema" segueix el tema de Windows
- [ ] El canvi de tema és immediat

**Labels:** ui, theming, priority:medium

---

## 📋 Issue #11: Afegir converters i behaviors necessaris

**Títol:** Crear converters XAML i behaviors per la UI

**Descripció:**
Implementar els converters i behaviors necessaris per la UI.

### Converters a crear:
- `TimeSpanToStringConverter` - Format de durades
- `BoolToVisibilityConverter` - Visibilitat condicional
- `ColorToBrushConverter` - Colors d'activitats
- `DateFormatConverter` - Format de dates
- `PercentageConverter` - Percentatges de jornada

### Behaviors:
- `NumericTextBoxBehavior` - Input només numèric
- `SelectAllOnFocusBehavior` - Selecció automàtica de text

### Criteris d'acceptació:
- [ ] Tots els converters funcionen correctament
- [ ] Els behaviors milloren l'experiència d'usuari
- [ ] El codi és reutilitzable

**Labels:** ui, helpers, priority:low

---

## 📋 Issue #12: Implementar diàlegs i notificacions

**Títol:** Crear sistema de diàlegs i missatges d'usuari

**Descripció:**
Implementar diàlegs per confirmacions, errors i informació.

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
- [ ] Les notificacions no són intrusives

**Labels:** ui, dialogs, priority:medium

---

## 📋 Issue #13: Afegir tests unitaris per Core

**Títol:** Crear tests per models, serveis i validacions

**Descripció:**
Implementar tests unitaris per la capa Core.

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
- [ ] Els tests són mantenibles

**Labels:** testing, core, priority:medium

---

## 📋 Issue #14: Optimitzar rendiment de consultes EF

**Títol:** Afegir índexs i optimitzar queries a la base de dades

**Descripció:**
Optimitzar el rendiment de les consultes més freqüents.

### Optimitzacions:
- Índexs a columnes de cerca (Data, ActivityId)
- Eager loading per relacions
- Paginació a llistes grans
- Projecció a DTOs per queries complexes

### Criteris d'acceptació:
- [ ] Les consultes per data són ràpides
- [ ] El filtratge per activitat és eficient
- [ ] No hi ha consultes N+1

**Labels:** performance, data-layer, priority:low

---

## 📋 Issue #15: Documentar arquitectura i guia de desenvolupament

**Títol:** Crear documentació tècnica del projecte

**Descripció:**
Documentar l'arquitectura, estructura i guies per a desenvolupadors.

### Documents a crear:
- `ARCHITECTURE.md` - Arquitectura de la solució
- `DEVELOPMENT.md` - Guia de configuració i desenvolupament
- Comentaris XML a interfícies públiques
- README actualitzat amb captures de pantalla

### Criteris d'acceptació:
- [ ] L'arquitectura està documentada
- [ ] La guia de desenvolupament és clara
- [ ] Els comentaris XML són útils
- [ ] El README inclou captures i instruccions

**Labels:** documentation, priority:low

---

## 📋 Issue #16: Preparar sistema d'icones i recursos visuals

**Títol:** Configurar icones i recursos gràfics de l'aplicació

**Descripció:**
Preparar tots els recursos visuals necessaris.

### Recursos:
- Icona de l'aplicació (.ico)
- Icones del menú (Segoe Fluent Icons)
- Icones d'accions (afegir, editar, eliminar)
- Splash screen (opcional)

### Criteris d'acceptació:
- [ ] L'aplicació té una icona pròpia
- [ ] Les icones del menú són clares
- [ ] El disseny és consistent

**Labels:** ui, design, priority:low

---

## 📋 Issue #17: Configurar build i release

**Títol:** Preparar configuració per publicació de l'aplicació

**Descripció:**
Configurar el procés de build i empaquetatge.

### Tasques:
- Configurar versió de l'aplicació
- Self-contained deployment
- Single-file executable (opcional)
- Crear instal·lador amb WiX o MSIX (opcional)

### Criteris d'acceptació:
- [ ] L'aplicació es pot publicar
- [ ] El versionat és correcte
- [ ] L'executable funciona de manera independent

**Labels:** devops, deployment, priority:low

---

## 🏷️ Labels recomanats per crear al repositori:

- `architecture` - Arquitectura i estructura
- `core` - Capa de negoci
- `data-layer` - Capa de dades
- `ui` - Interfície d'usuari
- `feature:registres` - Funcionalitat de registres
- `feature:jornada` - Funcionalitat de jornada
- `feature:activitats` - Funcionalitat d'activitats
- `feature:settings` - Configuració
- `business-logic` - Lògica de negoci
- `validation` - Validacions
- `navigation` - Navegació
- `calendar` - Calendari
- `theming` - Temes visuals
- `helpers` - Converters i behaviors
- `dialogs` - Diàlegs i notificacions
- `testing` - Tests
- `performance` - Optimitzacions
- `documentation` - Documentació
- `design` - Disseny visual
- `devops` - Build i deployment
- `priority:high` - Alta prioritat
- `priority:medium` - Mitjana prioritat
- `priority:low` - Baixa prioritat

---

## 📊 Ordre de desenvolupament recomanat:

1. Issues #1-3: Estructura base (Architecture + Data + Core)
2. Issues #4-5: Negoci + Navegació
3. Issues #6-9: Funcionalitats principals (UI)
4. Issues #10-12: Millores UI
5. Issues #13-17: Qualitat i publicació

