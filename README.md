# Metaverso MVP (Babylon.js + WebXR)

Runtime web single-user para visitas arquitectónicas. **Babylon.js** es el motor; **Revit/Rhino** son authoring BIM; **Unity** es baker opcional.

## Requisitos

- Node.js 18+ recomendado
- Navegador con WebGL2
- Meta Quest Browser para WebXR (HTTPS o localhost)

## Instalación y ejecución

```bash
cd metaverso-mvp
npm install
npm run dev
```

Abrir la URL que imprime Vite (por defecto `http://localhost:5173`).

### Producción estática

```bash
npm run build
npm run preview
```

El build genera `dist/` desplegable en cualquier hosting estático.

**Vite `base`:** `/metaverso-mvp/` (GitHub Project Pages).  
URL prevista: [https://juliojulioso.github.io/metaverso-mvp/](https://juliojulioso.github.io/metaverso-mvp/)  
Repo: [github.com/JulioJulioso/metaverso-mvp](https://github.com/JulioJulioso/metaverso-mvp)

### GitHub Pages

**Importante:** no publiques la rama `main` desde la **raíz del repo**. Eso sirve el `index.html` de desarrollo y el navegador pide `/src/main.js` → pantalla blanca (MIME `text/html` por 404).

Debes servir el **build de producción** (`docs/` tras `npm run build:pages`).

Cada `build:pages` **estampa una versión nueva** (`package.json` patch + `src/config/buildInfo.js`). Esa etiqueta aparece siempre en el **HUD** (esquina inferior derecha), p. ej. `v0.1.13 | 2026-08-12 11:05`. Úsala para confirmar que Quest/Pages tiene el build que acabas de subir (hard refresh si no coincide).

1. Genera el sitio (stamp + build):

```bash
npm run build:pages
```

2. Commit y push de `docs/`, `package.json` y `src/config/buildInfo.js` (tú; el agente no hace commit/push). En PowerShell:

```powershell
git add src package.json docs scripts
git commit -m "chore: deploy pages build"
git push origin main
```

3. En GitHub: **Settings → Pages → Build and deployment**
   - **Source:** Deploy from a branch  
   - **Branch:** `main`  
   - **Folder:** `/docs`  
4. Espera 1–2 min y abre:  
   **https://juliojulioso.github.io/metaverso-mvp/**

Si F12 pide `https://juliojulioso.github.io/src/main.js`, Pages sigue en la raíz sin build: repite los pasos y fuerza un hard refresh (Ctrl+F5).

Local con la base de Pages:

```bash
npm run dev
npm run build:pages
npx vite preview --outDir docs --base /metaverso-mvp/
```

## Controles

| Input | Acción |
|-------|--------|
| WASD / flechas | Mover visitante |
| E / Espacio | Interactuar (recoger esfera) |
| F | Soltar esfera |
| Clic izquierdo | Info BIM de malla (demo) |
| Botón VR (Babylon) | Entrar `immersive-vr` |

Las monedas (marcadores) se recogen por proximidad. Completar todas muestra “¡Misión completada!”.

## Arquitectura (carpetas)

- `src/core` — motor, input, XR, unidades, carga GLB
- `src/entities` — player, monedas, esfera, plataformas
- `src/systems` — colisión, interacción, logros, cámara, BIM stub, picking, network stub
- `src/config/levelConfig.js` — layout del nivel de demo (metros)
- `assets/models` — destino de `.glb` runtime
- `assets/bim` — sidecars `bim-index`
- `docs/AEC_PIPELINE.md` — estándar IFC/Speckle → glTF
- `pipeline/` — schema y checklist de bake

## Unidades y AEC

- **1 unidad de mundo = 1 metro**
- Cotas BIM en sidecar en **mm** (ver `pipeline/bimIndex.schema.json`)
- Orígenes de obra grandes: activar `useLargeWorldRendering` en `SceneManager` al cargar modelos reales (ver docs Babylon Large World)

## Decisiones del MVP

- Primitivas en código (no GLB aún) para validar loop y sistemas
- `AchievementSystem` (no MissionSystem) + `InteractionSystem` genérico
- `NetworkStub` sin red real
- Sin física externa; suelo por AABB de plataformas
- PWA: solo `manifest.json` + icono (sin service worker)

Siguiente fase: bake de un IFC de Revit → `building.glb` + `building.bim.json` y sustituir el suelo de demo.
