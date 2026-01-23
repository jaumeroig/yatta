# TimeTracker

Una aplicació de control de temps per a Windows amb interfície moderna.

## 📋 Documentació del Projecte

Per a informació detallada sobre els issues i la planificació del projecte, consulta:
- [README_ISSUES.md](README_ISSUES.md) - Resum dels issues i ordre de desenvolupament
- [ISSUES_TO_CREATE.md](ISSUES_TO_CREATE.md) - Llista completa de tots els issues

## 🚀 Crear Issues Automàticament

El projecte inclou un script per crear automàticament tots els issues de desenvolupament.

### Prerequisits

1. **GitHub CLI (gh)** - Instal·la des de: https://cli.github.com/
2. **Autenticació** - Executa `gh auth login` per autenticar-te

### Execució del Script

```bash
./create-issues.sh
```

El script crearà automàticament **17 issues** amb les seves etiquetes i prioritats corresponents.

### Què fa el script?

- Valida que GitHub CLI estigui instal·lat i autenticat
- Crea 17 issues detallats que cobreixen tota la implementació
- Assigna etiquetes i prioritats a cada issue
- Proporciona un resum del procés

## 📚 Arquitectura

Aquest projecte segueix una arquitectura de 3 capes:
- **TimeTracker.App** - Capa de presentació (WPF + WPF UI)
- **TimeTracker.Core** - Lògica de negoci i models
- **TimeTracker.Data** - Capa de persistència (EF Core + SQLite)

Per més detalls, consulta els issues #1-#3.

## 🛠️ Tecnologies

- .NET 10
- WPF + WPF UI
- CommunityToolkit.Mvvm
- Entity Framework Core
- SQLite