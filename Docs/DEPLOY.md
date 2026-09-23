# Build y publicacion

## Como llega al cliente

El cliente abre una URL. No instala nada.

- La primera visita descarga el build (`.wasm`, `.data`, `.framework.js`) a la cache del navegador.
- Con **Data Caching** y **nombres con hash**, la siguiente visita solo baja lo que cambio.
- Para actualizar el mundo: nuevo build, push, y el cliente recarga. El HUD muestra la version.

## Build

1. Unity con **Web Build Support**.
2. Menu **Metaverso > Build Web (desktop + Quest)**.
   - Sube el patch de `Assets/_Metaverso/Resources/BuildInfo.asset` (ej. `v0.1.3 | 2026-09-23 10:00`).
   - Compila `metaverso-web/docs/desktop` (texturas DXT) y `metaverso-web/docs/quest` (ASTC). Al cambiar el formato el editor recarga solo y sigue con la otra variante; no lances el menu otra vez en ese hueco.
   - Escribe `docs/index.html` (elige carpeta por user agent), `docs/version.json` y `docs/.nojekyll`.
   - Si Unity no deja cambiar el formato de textura por codigo, las dos carpetas salen con el de Player Settings. Cada una deja `texture-format.txt`.
3. `BuildSizeCheck` corta el build si el `.data` pasa de 50 MB.

## Publicar en GitHub Pages

Repositorio: [JulioJulioso/metaverso-mvp](https://github.com/JulioJulioso/metaverso-mvp).

| Rama | Contenido |
| --- | --- |
| `main` | Proyecto Unity (este repo) |
| `gh-pages` | Solo el sitio publicado. Un unico commit que se reemplaza en cada publicacion |
| `babylon-archive` / tag `babylon-final` | MVP anterior en Babylon.js, congelado |

`metaverso-web/` es un **git worktree** de la rama `gh-pages`: una segunda carpeta del mismo repositorio, ignorada por `main`. Asi los builds de Unity (decenas de MB) no se acumulan en el historial y el repo no llega al tope de 1 GB de GitHub.

Cada publicacion, desde la raiz del proyecto:

```powershell
.\Tools\publish-web.ps1
```

El script hace `add`, reemplaza el commit de `gh-pages` y hace `push --force` de esa rama (solo de esa).

En GitHub, una sola vez: **Settings > Pages > Deploy from a branch**, branch `gh-pages`, carpeta `/docs`.

URL: `https://juliojulioso.github.io/metaverso-mvp/?room=cliente`

### Primera vez en una maquina nueva

Despues de clonar, crea el worktree del sitio:

```powershell
git fetch origin gh-pages
git worktree add metaverso-web gh-pages
```

Si la rama `gh-pages` todavia no existe en GitHub:

```powershell
git worktree add --orphan -b gh-pages metaverso-web
```

`metaverso-web/.gitattributes` desactiva Git LFS en esa rama: GitHub Pages no sirve archivos LFS.

Por que Brotli con *Decompression Fallback*: GitHub Pages no deja configurar `Content-Encoding`. El fallback descomprime en JavaScript; es un poco mas lento pero funciona. En Cloudflare Pages se puede quitar y servir Brotli nativo.

## En la web del estudio

```html
<iframe
  src="https://juliojulioso.github.io/metaverso-mvp/?room=cliente"
  allow="xr-spatial-tracking; fullscreen; microphone; autoplay"
  allowfullscreen
  style="width:100%;height:80vh;border:0">
</iframe>
<p><a href="https://juliojulioso.github.io/metaverso-mvp/?room=cliente">Abrir en pantalla completa (Quest)</a></p>
```

- `xr-spatial-tracking` es obligatorio para que el boton VR funcione dentro del iframe.
- En Quest el enlace directo es mas fiable que el iframe.
- Los campos "Unity Loader / Data / Framework / Code" de la web del estudio no sirven para VR: esa pagina no carga la plantilla WebXR.

## Cuando salir de GitHub Pages

Con el primer cliente que pague: Cloudflare Pages (sitio) y R2 (modelos pesados o Addressables). Limites de Pages: 1 GB por repo, 100 MB por archivo, ancho de banda con tope blando.
