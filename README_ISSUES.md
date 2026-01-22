# TimeTracker - Issues creats per implementació

## Resum

S'han creat **17 issues** que cobreixen tota la implementació de l'aplicació TimeTracker segons l'especificació proporcionada.

## 📚 Documents creats

1. **ISSUES_TO_CREATE.md** - Llista completa de tots els issues amb descripció detallada
2. **create-issues.sh** - Script de Bash per crear els issues automàticament amb GitHub CLI
3. Aquest document (README_ISSUES.md) - Resum i instruccions

## 🎯 Distribució dels issues

### Alta prioritat (9 issues)
Issues fonamentals per tenir l'aplicació funcional:
- #1: Arquitectura de la solució (3 projectes)
- #2: Capa de dades (EF Core + SQLite)
- #3: Models de domini i interfícies
- #4: Validacions i càlculs de negoci
- #5: Navegació i shell principal
- #6: Funcionalitat de Registres
- #7: Funcionalitat de Jornada
- #8: Funcionalitat d'Activitats (reclassificat a medium si cal)
- #9: Funcionalitat d'Opcions (reclassificat a medium si cal)

### Prioritat mitjana (5 issues)
Millores i funcionalitats secundàries:
- #8-9: Activitats i Opcions (si es consideren secundaris)
- #10: Sistema de temes
- #12: Diàlegs i notificacions
- #13: Tests unitaris

### Prioritat baixa (3 issues)
Poliment i preparació per producció:
- #11: Converters i behaviors
- #14: Optimització de rendiment
- #15: Documentació tècnica
- #16: Icones i recursos visuals
- #17: Build i release

## 🏗️ Ordre de desenvolupament recomanat

### Fase 1: Infraestructura (Issues #1-3)
```
└─ Issue #1: Configurar arquitectura
   ├─ Issue #2: Implementar capa de dades
   └─ Issue #3: Definir models de domini
```

### Fase 2: Negoci i navegació (Issues #4-5)
```
├─ Issue #4: Validacions i càlculs
└─ Issue #5: Navegació principal
```

### Fase 3: Funcionalitats principals (Issues #6-9)
```
├─ Issue #6: Pàgina de Registres
├─ Issue #7: Pàgina de Jornada
├─ Issue #8: Pàgina d'Activitats
└─ Issue #9: Pàgina d'Opcions
```

### Fase 4: Millores UI (Issues #10-12)
```
├─ Issue #10: Sistema de temes
├─ Issue #11: Converters i behaviors
└─ Issue #12: Diàlegs i notificacions
```

### Fase 5: Qualitat i publicació (Issues #13-17)
```
├─ Issue #13: Tests unitaris
├─ Issue #14: Optimització
├─ Issue #15: Documentació
├─ Issue #16: Icones i recursos
└─ Issue #17: Build i release
```

## 🏷️ Labels recomanats

Abans de crear els issues, es recomana crear els següents labels al repositori:

### Per component
- `architecture` - Arquitectura i estructura
- `core` - Capa de negoci
- `data-layer` - Capa de dades
- `ui` - Interfície d'usuari

### Per funcionalitat
- `feature:registres` - Funcionalitat de registres
- `feature:jornada` - Funcionalitat de jornada
- `feature:activitats` - Funcionalitat d'activitats
- `feature:settings` - Configuració

### Per tipus
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

### Per prioritat
- `priority:high` - Alta prioritat
- `priority:medium` - Mitjana prioritat
- `priority:low` - Baixa prioritat

## 🚀 Com crear els issues

### Opció 1: Script automàtic (recomanat)

Si tens GitHub CLI (`gh`) instal·lat i autenticat:

```bash
./create-issues.sh
```

Aquest script crearà automàticament tots els 17 issues amb les seves etiquetes corresponents.

### Opció 2: Manual

1. Obre el fitxer `ISSUES_TO_CREATE.md`
2. Copia el contingut de cada issue
3. Crea-ho manualment a GitHub:
   - Ves a https://github.com/jaumeroig/time-tracker/issues/new
   - Enganxa el títol i la descripció
   - Assigna les etiquetes corresponents

### Opció 3: GitHub CLI individual

Per crear un issue específic:

```bash
gh issue create \
  --repo "jaumeroig/time-tracker" \
  --title "Títol de l'issue" \
  --label "label1,label2" \
  --body "Descripció de l'issue"
```

## 📊 Estadístiques del pla

- **Total issues:** 17
- **Issues d'arquitectura:** 3
- **Issues de funcionalitats:** 4
- **Issues de UI:** 7
- **Issues de qualitat:** 3
- **Temps estimat total:** 8-12 setmanes (1 desenvolupador)

## 📝 Notes importants

1. **Dependències entre issues:** Alguns issues depenen d'altres (p.ex., les funcionalitats UI depenen de la navegació)
2. **Flexibilitat:** L'ordre es pot ajustar segons necessitats, però es recomana seguir les fases
3. **Iteracions:** Cada issue pot tenir múltiples iteracions abans de ser considerat complet
4. **MVP:** Els issues de prioritat alta constitueixen el MVP (Minimum Viable Product)

## 🎯 Objectius per fase

### MVP (Prioritat alta)
- Aplicació funcional amb les 4 seccions principals
- Persistència de dades
- Navegació completa
- UI moderna i consistent

### Post-MVP (Prioritat mitjana)
- Temes personalitzables
- Millor experiència d'usuari
- Tests de qualitat

### Release (Prioritat baixa)
- Optimitzacions
- Documentació completa
- Preparació per publicació

## 📞 Contacte

Per qualsevol dubte sobre els issues o la implementació, refereix-te a:
- **Especificació completa:** Problema statement original
- **Arquitectura:** ARCHITECTURE.md (a crear amb issue #15)
- **Guia de desenvolupament:** DEVELOPMENT.md (a crear amb issue #15)

---

**Última actualització:** 2026-01-22
**Versió del pla:** 1.0
**Estat:** Issues preparats per crear
