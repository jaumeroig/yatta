# Procediment de publicació d'una nova versió

## Requisits previs

- Accés de push al repositori
- Git configurat localment

---

## Passos

### 1. Actualitzar el changelog

Afegeix una entrada al principi de `src/Yatta.App/Resources/changelog.md` i `src/Yatta.App/Resources/changelog-ca.md`:

```markdown
## v1.2.0 (2026-XX-XX)

### Novetats
- Descripció de la nova funcionalitat

### Correccions
- Descripció del bug corregit
```

Fes commit i push dels changelogs a `master`:

```bash
git add src/Yatta.App/Resources/changelog.md \
        src/Yatta.App/Resources/changelog-ca.md
git commit -m "docs: update changelog for v1.2.0"
git push origin master
```

### 2. Crear i pujar el tag

```bash
git tag v1.2.0
git push origin v1.2.0
```

**Ja està.** Això activa automàticament el workflow `release.yml` de GitHub Actions.

> La versió **no cal tocar-la** a `Directory.Build.props`. El workflow
> l'extrau directament del nom del tag (`v1.2.0 → 1.2.0`) i la passa
> a `dotnet publish` i `vpk pack` en temps de compilació.

### 3. Verificar el release a GitHub Actions

1. Ves a **GitHub → Actions → Release** i comprova que el workflow ha acabat correctament.
2. Un cop finalitzat, el release apareixerà a **GitHub → Releases** amb els artefactes adjunts:
   - `Yatta-1.2.0-Setup.exe` — instal·lador per a usuaris nous
   - `Yatta-1.2.0-delta.nupkg` — paquet d'actualització incremental (Velopack)
   - `Yatta-1.2.0-full.nupkg` — paquet d'actualització complet (Velopack)
   - `RELEASES` — fitxer d'índex que Velopack consulta per detectar actualitzacions

> [!IMPORTANT]
> No elimineu ni modifiqueu mai els artefactes d'un release publicat. Velopack necessita tots els releases anteriors per calcular les actualitzacions incrementals.

---

## Esquema del versionat (Semantic Versioning)

| Tipus de canvi | Exemple | Quan usar-lo |
|---|---|---|
| **Patch** | `1.0.0 → 1.0.1` | Correccions de bugs |
| **Minor** | `1.0.0 → 1.1.0` | Noves funcionalitats compatibles enrere |
| **Major** | `1.0.0 → 2.0.0` | Canvis estructurals que trenquen compatibilitat |

---

## Com funciona l'auto-update

```
Usuari obre l'app
       │
       ▼
UpdateService.IsUpdateAvailableAsync()
  └─ Consulta GitHub Releases via Velopack
       │
       ├─ Cap actualització → continua normalment
       │
       └─ Nova versió disponible
              │
              ▼
         Diàleg: "Hi ha una nova versió. Instal·lar ara?"
              │
              ├─ "Més tard" → continua normalment
              │
              └─ "Instal·la i reinicia"
                     │
                     ▼
              Descarrega + aplica + reinicia
```

Les actualitzacions **només funcionen** si l'app s'ha instal·lat amb l'instal·lador (`Yatta-Setup.exe`). En entorns de desenvolupament (`dotnet run`) l'auto-update queda desactivat automàticament.

---

## Resolució de problemes

### El workflow de release falla

- **Error de compilació**: revisa que el tag té el format correcte (`vX.Y.Z`).
- **Tests fallits**: corregeix els tests i crea un nou tag (`v1.2.1`). No pots reutilitzar un tag existent.
- **Permisos**: assegura't que el repositori té `Settings → Actions → Workflow permissions` configurat com a *Read and write*.

### Els usuaris no reben l'actualització

- Verifica que el release a GitHub és públic i no és un *draft*.
- Comprova que tots els artefactes de Velopack (`.nupkg` i `RELEASES`) estan adjunts al release.
- Els usuaris veuran l'actualització la propera vegada que obrin l'app.
