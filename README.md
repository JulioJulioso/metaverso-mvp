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

### GitHub Pages (recomendado)

1. Primer push del código a `main` (tú haces commit/push; el agente no).
2. En el repo: **Settings → Pages → Build and deployment → Source: GitHub Actions**.
3. Cada push a `main` ejecuta `.github/workflows/deploy-pages.yml` (`npm ci` + `npm run build` + deploy de `dist/`).
4. Comparte la URL HTTPS con tu equipo o embébela en tu CMS (iframe/URL embebida).

Local con la base de Pages:

```bash
npm run dev
# o tras build:
npx vite preview --base /metaverso-mvp/
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
