# 🕒 TimeTracker – Definició completa de l’aplicació

## 1. Visió general

**TimeTracker** és una aplicació de control de temps per a Windows, dissenyada per registrar tant el temps dedicat a activitats concretes (projectes, tasques, reunions, etc.) com la jornada laboral diària (entrades, sortides i teletreball).

L’aplicació està pensada com una eina **personal i local**, amb dades persistides en local, enfocada a:
- Control d’hores treballades
- Seguiment de teletreball vs oficina
- Compliment de jornada laboral
- Visualització clara i moderna (estil Windows 11)

---

## 2. Stack tecnològic

- **Plataforma:** Windows
- **Framework:** .NET 10
- **UI:** WPF + WPF UI
- **Arquitectura:** MVVM
- **Persistència:** SQLite + Entity Framework Core
- **Patrons:** Repository, Dependency Injection
- **Estil visual:** Windows moderna (cards, espaiat generós, tipografia clara)

---

## 3. Arquitectura de la solució

### 3.1 Projectes

```
TimeTracker.sln
├── TimeTracker.App
├── TimeTracker.Core
└── TimeTracker.Data
```

### 3.2 Dependències

```
TimeTracker.App
        ↓
TimeTracker.Core
        ↓
TimeTracker.Data
```

- `App` depèn de `Core`
- `Core` no depèn de `Data`
- `Data` implementa interfícies definides a `Core`

---

## 4. Descripció de projectes

### 4.1 TimeTracker.App (UI)

Responsable de la interfície d’usuari i la navegació.

**Contingut:**
- Views (XAML)
- ViewModels (CommunityToolkit.Mvvm)
- Navegació (Shell + Pages)
- Converters i Behaviors
- Injecció de dependències
- Bootstrap de l’aplicació

**No conté:**
- Accés directe a base de dades
- Lògica de negoci

---

### 4.2 TimeTracker.Core (Lògica i negoci)

Cor funcional de l’aplicació.

**Responsabilitats:**
- Regles de negoci
- Validacions
- Càlculs de temps
- Casos d’ús
- Interfícies de repositoris

**Estructura orientativa:**
```
Models/
Services/
Interfaces/
Calculators/
```

---

### 4.3 TimeTracker.Data (Persistència)

Capa de dades i infraestructura.

**Responsabilitats:**
- EF Core + SQLite
- DbContext
- Entitats EF
- Migracions
- Repositoris

**Ubicació de la BBDD:**
```
%LocalAppData%\TimeTracker\TimeTracker.db
```

---

## 5. Funcionalitats de l’aplicació

### 5.1 Navegació

Menú lateral amb les seccions:
- Registres
- Jornada
- Activitats
- Opcions

---

### 5.2 Registres

Gestió de registres de temps associats a activitats.

**Cada registre inclou:**
- Data
- Hora d’inici
- Hora de fi
- Durada calculada
- Activitat
- Notes opcionals

**Funcionalitats:**
- Crear, editar i eliminar registres
- Agrupació per dia
- Total diari treballat
- Filtres per data i activitat
- Cerca per text (activitat o notes)

---

### 5.3 Jornada

Control de la jornada laboral diària.

**Funcionalitats:**
- Calendari mensual
- Selecció de dia
- Gestió de franges horàries
- Indicació de lloc de treball:
  - Casa (teletreball)
  - Oficina
- Càlcul automàtic de:
  - Total diari
  - % teletreball diari
  - Diferència respecte jornada objectiu
- Resum mensual:
  - Total treballat
  - Percentatge oficina / teletreball

---

### 5.4 Activitats

Manteniment d’activitats o conceptes.

**Funcionalitats:**
- Crear, editar i eliminar activitats
- Visualització en format card
- Temps total acumulat per activitat
- Nombre de registres associats
- Color identificatiu opcional

---

### 5.5 Opcions

Configuració general de l’aplicació.

**Seccions:**
- Aparença:
  - Clar
  - Fosc
  - Sistema
- Notificacions:
  - Activar/desactivar recordatoris
- Temporitzador:
  - Inici automàtic (base per funcionalitats futures)
- Regional:
  - Primer dia de la setmana
- Sobre l’aplicació:
  - Versió
  - Plataforma

---

## 6. Models de dades (conceptual)

### Activitat
- Id
- Nom
- Color
- Activa/Inactiva

### Registre de temps
- Id
- Data
- Hora inici
- Hora fi
- Durada
- Activitat
- Notes

### Franja de jornada
- Id
- Data
- Hora inici
- Hora fi
- Tipus (Casa / Oficina)

### Configuració
- Tema
- Notificacions
- Opcions regionals

---

## 7. Regles de negoci clau

- L’hora de fi ha de ser posterior a l’hora d’inici
- No es permeten franges solapades en una mateixa jornada
- La durada sempre es calcula automàticament
- El percentatge de teletreball es calcula sobre hores totals
- Les activitats amb registres associats requereixen confirmació per eliminar-se

---

## 8. Fora d’abast (MVP)

- Autenticació / login
- Sincronització al núvol
- Exportacions (CSV / PDF)
- Integració amb calendaris
- Tracking automàtic en segon pla

---

## 9. Evolució futura (possibles extensions)

- App resident a la safata del sistema
- Temporitzador actiu amb notificacions
- Exportació d’hores
- Estadístiques avançades
- Sincronització multi-dispositiu
