# 📊 TimeTracker - Diagrama d'Issues i Implementació

## 🎯 Visió general

Aquest document proporciona una visualització gràfica de l'estructura d'issues creada per a la implementació del projecte TimeTracker.

---

## 📐 Arquitectura de la Solució

```
┌─────────────────────────────────────────────────────────────┐
│                     TimeTracker.sln                         │
└─────────────────────────────────────────────────────────────┘
                              │
                ┌─────────────┼─────────────┐
                │             │             │
        ┌───────▼──────┐ ┌───▼──────┐ ┌───▼──────┐
        │ TimeTracker  │ │TimeTracker│ │TimeTracker│
        │    .App      │ │  .Core    │ │   .Data   │
        │              │ │           │ │           │
        │ • Views      │ │ • Models  │ │ • DbContext│
        │ • ViewModels │ │ • Services│ │ • Entities│
        │ • Navigation │ │ • Interfaces│ │ • Repos │
        │ • Converters │ │ • Validators│ │ • Migrations│
        └──────────────┘ └───────────┘ └───────────┘
             (UI)          (Business)    (Persistence)
```

---

## 🗂️ Estructura d'Issues per Fase

```
FASE 1: INFRAESTRUCTURA (Issues #1-3)
══════════════════════════════════════
┌─────────────────────────────────────────┐
│ Issue #1: Arquitectura Base             │  Priority: HIGH
│ ✓ Crear 3 projectes                     │
│ ✓ Configurar dependències               │
│ ✓ Instal·lar paquets NuGet              │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│ Issue #2: Capa de Dades                 │  Priority: HIGH
│ ✓ DbContext + SQLite                    │
│ ✓ Entitats EF                           │
│ ✓ Repositoris                           │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│ Issue #3: Models de Domini              │  Priority: HIGH
│ ✓ Models de negoci                      │
│ ✓ Interfícies                           │
└─────────────────────────────────────────┘

FASE 2: NEGOCI + NAVEGACIÓ (Issues #4-5)
═════════════════════════════════════════
┌─────────────────────────────────────────┐
│ Issue #4: Validacions i Càlculs         │  Priority: HIGH
│ ✓ Regles de negoci                      │
│ ✓ Calculadores de temps                 │
│ ✓ Serveis de validació                  │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│ Issue #5: Navegació i Shell             │  Priority: HIGH
│ ✓ Menú lateral                          │
│ ✓ Sistema de navegació                  │
└─────────────────────────────────────────┘

FASE 3: FUNCIONALITATS PRINCIPALS (Issues #6-9)
════════════════════════════════════════════════
┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Issue #6     │  │ Issue #7     │  │ Issue #8     │  │ Issue #9     │
│ REGISTRES    │  │ JORNADA      │  │ ACTIVITATS   │  │ OPCIONS      │
│              │  │              │  │              │  │              │
│ • Llistar    │  │ • Calendari  │  │ • Cards      │  │ • Temes      │
│ • Crear      │  │ • Franges    │  │ • CRUD       │  │ • Config     │
│ • Editar     │  │ • Casa/Of.   │  │ • Colors     │  │ • Sobre      │
│ • Eliminar   │  │ • Càlculs    │  │ • Stats      │  │              │
│ • Filtres    │  │ • Resum      │  │              │  │              │
│ • Cerca      │  │              │  │              │  │              │
│              │  │              │  │              │  │              │
│ Priority:    │  │ Priority:    │  │ Priority:    │  │ Priority:    │
│ HIGH         │  │ HIGH         │  │ MEDIUM       │  │ MEDIUM       │
└──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘

FASE 4: MILLORES UI (Issues #10-12)
════════════════════════════════════
┌─────────────────────────────────────────┐
│ Issue #10: Sistema de Temes             │  Priority: MEDIUM
│ ✓ Clar / Fosc / Sistema                 │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│ Issue #11: Converters i Behaviors       │  Priority: LOW
│ ✓ Helpers XAML                          │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│ Issue #12: Diàlegs i Notificacions      │  Priority: MEDIUM
│ ✓ Confirmacions                         │
│ ✓ Errors                                │
└─────────────────────────────────────────┘

FASE 5: QUALITAT I PUBLICACIÓ (Issues #13-17)
══════════════════════════════════════════════
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Issue #13    │  │ Issue #14    │  │ Issue #15    │
│ TESTS        │  │ PERFORMANCE  │  │ DOCS         │
│              │  │              │  │              │
│ • xUnit      │  │ • Índexs     │  │ • Architecture│
│ • Cobertura  │  │ • Eager Load │  │ • Development│
│              │  │ • Paginació  │  │ • Comments   │
│              │  │              │  │              │
│ Priority:    │  │ Priority:    │  │ Priority:    │
│ MEDIUM       │  │ LOW          │  │ LOW          │
└──────────────┘  └──────────────┘  └──────────────┘
       │                  │                  │
       └──────────────────┼──────────────────┘
                          ↓
       ┌──────────────────────────────────────┐
       │ Issue #16: Icons i Recursos          │  Priority: LOW
       └──────────────────────────────────────┘
                          ↓
       ┌──────────────────────────────────────┐
       │ Issue #17: Build i Release           │  Priority: LOW
       └──────────────────────────────────────┘
```

---

## 📊 Distribució per Prioritat

```
HIGH PRIORITY (7 issues)           ████████████████████ 41%
─────────────────────────────────────────────────────────
Issues: #1, #2, #3, #4, #5, #6, #7
Temps estimat: 4-6 setmanes
Objectiu: MVP funcional

MEDIUM PRIORITY (5 issues)         ██████████████ 29%
─────────────────────────────────────────────────────────
Issues: #8, #9, #10, #12, #13
Temps estimat: 2-3 setmanes
Objectiu: Millores i testing

LOW PRIORITY (5 issues)            ██████████████ 29%
─────────────────────────────────────────────────────────
Issues: #11, #14, #15, #16, #17
Temps estimat: 2-3 setmanes
Objectiu: Poliment i release
```

---

## 🏗️ Dependències entre Issues

```
                    Issue #1 (Arquitectura)
                           │
              ┌────────────┼────────────┐
              │            │            │
         Issue #2      Issue #3    Issue #5
         (Data)        (Core)      (Nav)
              │            │            │
              └────────────┼────────────┘
                           │
                      Issue #4
                    (Validacions)
                           │
              ┌────────────┼────────────┐
              │            │            │
         Issue #6      Issue #7    Issue #8
        (Registres)   (Jornada)  (Activitats)
              │            │            │
              └────────────┼────────────┘
                           │
                      Issue #9
                     (Opcions)
                           │
              ┌────────────┼────────────┐
              │            │            │
        Issue #10     Issue #12   Issue #13
        (Temes)      (Diàlegs)   (Tests)
              │            │            │
              └────────────┼────────────┘
                           │
                   Issues #14-17
              (Poliment i publicació)
```

---

## 📋 Labels i Categories

```
COMPONENT LABELS
┌─────────────────┐
│ architecture    │  Issue #1
│ core            │  Issues #3, #4, #13
│ data-layer      │  Issues #2, #14
│ ui              │  Issues #5-12, #16
└─────────────────┘

FEATURE LABELS
┌─────────────────────┐
│ feature:registres   │  Issue #6
│ feature:jornada     │  Issue #7
│ feature:activitats  │  Issue #8
│ feature:settings    │  Issue #9
└─────────────────────┘

TYPE LABELS
┌──────────────────┐
│ business-logic   │  Issue #4
│ validation       │  Issue #4
│ navigation       │  Issue #5
│ calendar         │  Issue #7
│ theming          │  Issue #10
│ helpers          │  Issue #11
│ dialogs          │  Issue #12
│ testing          │  Issue #13
│ performance      │  Issue #14
│ documentation    │  Issue #15
│ design           │  Issue #16
│ devops           │  Issue #17
└──────────────────┘
```

---

## 🎯 MVP (Minimum Viable Product)

Els següents issues constitueixen el MVP:

```
┌─────────────────────────────────────────────────────┐
│ MVP COMPLETO - Issues d'Alta Prioritat              │
├─────────────────────────────────────────────────────┤
│                                                      │
│  ✓ Issue #1: Arquitectura base                      │
│  ✓ Issue #2: Capa de dades                          │
│  ✓ Issue #3: Models de domini                       │
│  ✓ Issue #4: Validacions i càlculs                  │
│  ✓ Issue #5: Navegació                              │
│  ✓ Issue #6: Funcionalitat Registres                │
│  ✓ Issue #7: Funcionalitat Jornada                  │
│                                                      │
│  Resultat: Aplicació funcional amb les dues         │
│           funcionalitats principals de tracking      │
│                                                      │
└─────────────────────────────────────────────────────┘
```

---

## 📅 Timeline Estimat

```
Setmana 1-2    │████████│ Issues #1-3  Infraestructura
               │
Setmana 3-4    │████████│ Issues #4-5  Negoci + Navegació
               │
Setmana 5-7    │████████████│ Issues #6-7  Funcionalitats MVP
               │
Setmana 8-9    │████████│ Issues #8-9  Funcionalitats extra
               │
Setmana 10-11  │████████│ Issues #10-12 Millores UI
               │
Setmana 12     │████│ Issues #13-17 Qualitat + Release
```

---

## ✅ Verificació Final

```
✓ 17 issues definits amb detall complet
✓ Arquitectura de 3 capes coberta
✓ 4 funcionalitats principals implementades
✓ Prioritats assignades correctament
✓ Dependencies entre issues identificades
✓ Timeline i estimacions definides
✓ Labels i categories organitzats
✓ Criteris d'acceptació per cada issue
✓ Script d'automatització creat
✓ Documentació completa generada
```

---

## 📚 Documents de Referència

1. **ISSUES_TO_CREATE.md** - Contingut detallat de cada issue
2. **create-issues.sh** - Script per crear els issues automàticament
3. **README_ISSUES.md** - Guia d'implementació i instruccions
4. **TASK_COMPLETED.md** - Resum de la tasca completada

---

**Creat:** 2026-01-22  
**Repositori:** jaumeroig/time-tracker  
**Total Issues:** 17  
**Estat:** READY TO CREATE
