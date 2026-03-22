# Procedimiento de publicación de una nueva versión

## Requisitos previos

- Acceso de push al repositorio
- Git configurado localmente

---

## Pasos

### 1. Actualizar el changelog

Añade una entrada al principio de `src/Yatta.App/Resources/changelog.md` y `src/Yatta.App/Resources/changelog-ca.md`:

```markdown
## v1.2.0 (2026-XX-XX)

### Novedades
- Descripción de la nueva funcionalidad

### Correcciones
- Descripción del bug corregido
```

Haz commit y push de los changelogs a `master`:

```bash
git add src/Yatta.App/Resources/changelog.md \
        src/Yatta.App/Resources/changelog-ca.md
git commit -m "docs: update changelog for v1.2.0"
git push origin master
```

### 2. Crear y subir el tag

```bash
git tag v1.2.0
git push origin v1.2.0
```

**Listo.** Esto activa automáticamente el workflow `release.yml` de GitHub Actions.

> La versión en `Directory.Build.props` **no afecta al release**: el workflow
> la extrae del tag (`v1.2.0 → 1.2.0`) y la sobreescribe en tiempo de compilación
> mediante `-p:Version=`. Sin embargo, actualizarla es recomendable para que refleje
> la versión correcta al ejecutar la app en local con `dotnet run`.

### 3. Verificar el release en GitHub Actions

1. Ve a **GitHub → Actions → Release** y comprueba que el workflow ha finalizado correctamente.
2. Una vez finalizado, el release aparecerá en **GitHub → Releases** con los artefactos adjuntos:
   - `Yatta-1.2.0-Setup.exe` — instalador para usuarios nuevos
   - `Yatta-1.2.0-delta.nupkg` — paquete de actualización incremental (Velopack)
   - `Yatta-1.2.0-full.nupkg` — paquete de actualización completo (Velopack)
   - `RELEASES` — archivo de índice que Velopack consulta para detectar actualizaciones

> [!IMPORTANT]
> No elimines ni modifiques nunca los artefactos de un release publicado. Velopack necesita todos los releases anteriores para calcular las actualizaciones incrementales.

---

## Esquema de versionado (Semantic Versioning)

| Tipo de cambio | Ejemplo | Cuándo usarlo |
|---|---|---|
| **Patch** | `1.0.0 → 1.0.1` | Correcciones de bugs |
| **Minor** | `1.0.0 → 1.1.0` | Nuevas funcionalidades compatibles hacia atrás |
| **Major** | `1.0.0 → 2.0.0` | Cambios estructurales que rompen compatibilidad |

---

## Cómo funciona el auto-update

```
Usuario abre la app
       │
       ▼
UpdateService.IsUpdateAvailableAsync()
  └─ Consulta GitHub Releases via Velopack
       │
       ├─ Sin actualización → continúa normalmente
       │
       └─ Nueva versión disponible
              │
              ▼
         Diálogo: "Hay una nueva versión. ¿Instalar ahora?"
              │
              ├─ "Más tarde" → continúa normalmente
              │
              └─ "Instalar y reiniciar"
                     │
                     ▼
              Descarga + aplica + reinicia
```

Las actualizaciones **solo funcionan** si la app se ha instalado con el instalador (`Yatta-Setup.exe`). En entornos de desarrollo (`dotnet run`) el auto-update queda desactivado automáticamente.

---

## Resolución de problemas

### El workflow de release falla

- **Error de compilación**: comprueba que el tag tiene el formato correcto (`vX.Y.Z`).
- **Tests fallidos**: corrige los tests y crea un nuevo tag (`v1.2.1`). No se puede reutilizar un tag existente.
- **Permisos**: asegúrate de que el repositorio tiene `Settings → Actions → Workflow permissions` configurado como *Read and write*.

### Los usuarios no reciben la actualización

- Verifica que el release en GitHub es público y no es un *draft*.
- Comprueba que todos los artefactos de Velopack (`.nupkg` y `RELEASES`) están adjuntos al release.
- Los usuarios verán la actualización la próxima vez que abran la app.
