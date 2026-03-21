# Procediment de publicació d'una nova versió

## Requisits previs

- Accés de push al repositori (branca `master`)
- Git configurat localment

---

## Passos

### 1. Actualitzar la versió

Edita `src/Directory.Build.props` i incrementa el número de versió seguint [Semantic Versioning](https://semver.org/):

```xml
<Version>1.2.0</Version>
```

| Tipus de canvi | Exemple | Quan usar-lo |
|---|---|---|
| **Patch** `1.0.0 → 1.0.1` | Correccions de bugs | Fixes sense canvis de funcionalitat |
| **Minor** `1.0.0 → 1.1.0` | Noves funcionalitats | Funcions noves compatibles enrere |
| **Major** `1.0.0 → 2.0.0` | Trencament de compatibilitat | Canvis estructurals importants |

### 2. Actualitzar el changelog

Afegeix una entrada al principi de `src/Yatta.App/Resources/changelog.md` i `src/Yatta.App/Resources/changelog-ca.md`:

```markdown
## v1.2.0 (2026-XX-XX)

### Novetats
- Descripció de la nova funcionalitat

### Correccions
- Descripció del bug corregit
```

### 3. Fer commit dels canvis

```bash
git add src/Directory.Build.props \
        src/Yatta.App/Resources/changelog.md \
        src/Yatta.App/Resources/changelog-ca.md
git commit -m "chore: bump version to v1.2.0"
git push origin master
```

### 4. Crear i pujar el tag

```bash
git tag v1.2.0
git push origin v1.2.0
```

Això activa automàticament el workflow `release.yml` de GitHub Actions.

### 5. Verificar el release a GitHub Actions

1. Ves a **GitHub → Actions → Release** i comprova que el workflow ha acabat correctament.
2. Un cop finalitzat, el release apareixerà a **GitHub → Releases** amb els artefactes adjunts:
   - `Yatta-1.2.0-Setup.exe` — instal·lador per a usuaris nous
   - `Yatta-1.2.0-delta.nupkg` — paquet d'actualització incremental (per a Velopack)
   - `Yatta-1.2.0-full.nupkg` — paquet d'actualització complet (per a Velopack)
   - `RELEASES` — fitxer d'índex que Velopack consulta per detectar actualitzacions

> [!IMPORTANT]
> No elimineu ni modifiqueu mai els artefactes d'un release publicat. Velopack necessita tots els releases anteriors per calcular les actualitzacions incrementals.

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

Les actualitzacions **només funcionen** si l'app s'ha instal·lat amb l'instal·lador (`Yatta-Setup.exe`). En entorns de desenvolupament (`dotnet run`) l'auto-update queda desactivat automàticament (`IsInstalled = false`).

---

## Resolució de problemes

### El workflow de release falla

- **Error de compilació**: revisa que `src/Directory.Build.props` té un número de versió vàlid (`X.Y.Z`).
- **Tests fallits**: corregeix els tests abans de publicar.
- **Permisos**: assegura't que el repositori té `Settings → Actions → Workflow permissions` configurat com a *Read and write*.

### Els usuaris no reben l'actualització

- Verifica que el release a GitHub és públic i no és un *draft*.
- Comprova que tots els artefactes de Velopack (`.nupkg` i `RELEASES`) estan adjunts al release.
- Els usuaris veuran l'actualització la propera vegada que obrin l'app.
