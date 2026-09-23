# Metaverso Sinestesia

Metaverso de arquitectura que se abre desde un link en el navegador:

- **PC:** tercera persona con teclado y mouse.
- **Meta Quest:** el mismo link en el navegador del visor, boton VR, caminar con los sticks.
- **Multiplayer:** varias personas en la misma sala (`?room=` en la URL) con Photon Fusion 2.

No se instala una aplicacion. La primera visita descarga el mundo en la cache del navegador; las siguientes solo bajan lo que cambio.

Motor: **Unity 6000.3.9f1**, URP, plataforma Web (WebGL) con [WebXR Export](https://github.com/De-Panther/unity-webxr-export) 0.25 y XR Interaction Toolkit 3.2.

## Empezar

1. Instala Unity **6000.3.9f1** con el modulo **Web Build Support** (Unity Hub).
2. Instala [Git LFS](https://git-lfs.com) y clona: `git lfs install` y luego `git clone ...`.
3. Abre la carpeta en Unity Hub. La primera apertura descarga paquetes (OpenUPM incluido) y tarda.
4. Al terminar de compilar se aplican los Player Settings web y se crea la escena `Assets/_Metaverso/Scenes/Circuito.unity`. Si no aparece: menu **Metaverso > Crear mundo de prueba**.
5. Play. Clic en la vista para capturar el mouse, Escape para soltarlo.

| PC | Quest (dentro de VR) |
| --- | --- |
| WASD / flechas: mover | Stick izquierdo: caminar |
| Mouse: mirar | Stick derecho: girar 30 grados |
| Espacio: saltar | Click stick derecho: teletransporte corto |
| E / F: tomar / soltar pelota | Gatillo / grip: tomar / soltar |
| Clic en la pantalla: video | HUD a 2 m delante |

## Menu Metaverso (en el editor)

| Menu | Que hace |
| --- | --- |
| Crear mundo de prueba | Regenera `Circuito.unity` (circuito del MVP + rig VR) |
| Crear rig VR | Rehace solo el XR Origin en la escena abierta |
| Configurar proyecto Web | Reaplica Player Settings (Brotli, hashes, cache, stripping) |
| Configurar WebXR | Copia plantillas WebXR y activa el loader para Web |
| Preparar modelo arquitectonico | Colliders, estaticos, capa Architecture y reporte de presupuesto Quest |
| Build Web (desktop + Quest) | Sube la version y deja el sitio en `metaverso-web/docs` |
| Red > Activar Photon Fusion | Enciende el multiplayer cuando el SDK ya esta importado |

## Estructura

```
Assets/_Metaverso/
  Scripts/Core        Logica pura y testeable (version, sala, presupuesto, modo PC/VR, circuito)
  Scripts/Player      Controlador de PC y camara en tercera persona
  Scripts/World       Circuito de prueba: monedas, pelota, zonas, muros, pantalla, fichas BIM
  Scripts/UI          HUD (pantalla en PC, mundo en VR)
  Scripts/XR          Cambio PC/VR y locomocion del Quest (depende de WebXR y XRI)
  Scripts/Network     Sesion local; Fusion/ se compila solo con PHOTON_FUSION
  Scripts/Editor      Menus, importador de modelos, build web, chequeo de tamano
  Scripts/EditorXR    Configuracion WebXR y constructor del rig VR
  Plugins/WebGL       jslib del overlay de video
  Models/Arch         Aqui van los FBX / GLB de Revit y Rhino
  Scenes              Circuito.unity y su Timeline
  Tests               EditMode y PlayMode
Assets/Settings       URP: Web_RPAsset es el de la plataforma Web
Assets/WebGLTemplates Plantillas de WebXR Export (se usa WebXRFullView2020)
Docs/                 Documentacion (ver abajo)
Tools/qa/             Chequeos del sitio publicado (Node + Playwright)
metaverso-web/        Sitio publicado: worktree de la rama gh-pages (ignorado en main)
Tools/publish-web.ps1 Publica metaverso-web en gh-pages
```

## Repositorio y ramas

[JulioJulioso/metaverso-mvp](https://github.com/JulioJulioso/metaverso-mvp) (el nombre viene del MVP original).

| Rama | Contenido |
| --- | --- |
| `main` | Este proyecto Unity |
| `gh-pages` | Sitio publicado en https://juliojulioso.github.io/metaverso-mvp/ |
| `babylon-archive`, tag `babylon-final` | MVP anterior en Babylon.js, congelado |

Despues de clonar, para poder publicar: `git fetch origin gh-pages` y `git worktree add metaverso-web gh-pages` (detalle en [Docs/DEPLOY.md](Docs/DEPLOY.md)).

## Documentacion

| Documento | Para que |
| --- | --- |
| [ROADMAP.md](ROADMAP.md) | Estado, prioridades y decisiones. Fuente de verdad |
| [Docs/ARQUITECTURA.md](Docs/ARQUITECTURA.md) | Como se conectan los scripts, assemblies y el cambio PC/VR |
| [Docs/PIPELINE_MODELOS.md](Docs/PIPELINE_MODELOS.md) | Exportar de Revit / Rhino e importar sin romper el presupuesto |
| [Docs/DEPLOY.md](Docs/DEPLOY.md) | Build, version, GitHub Pages e iframe en la web del estudio |
| [Docs/MULTIPLAYER.md](Docs/MULTIPLAYER.md) | Activar Photon Fusion 2 |
| [Docs/QA.md](Docs/QA.md) | Tests automaticos y checklist manual PC / Quest |
| [Docs/ESTADO_DEL_ARTE.md](Docs/ESTADO_DEL_ARTE.md) | Alternativas evaluadas y por que Unity |
| [Docs/EVALUACION_COMERCIAL.md](Docs/EVALUACION_COMERCIAL.md) | Que se vende, costos y riesgos |

## Reglas del repo

- Modelos, texturas, audio y video van por Git LFS (`.gitattributes`).
- `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `.csproj` y `.vscode/` no se versionan.
- Un MonoBehaviour por archivo y con el mismo nombre que la clase. Si no, Unity no lo puede guardar en la escena.
- En este proyecto el agente de Cursor no hace commit ni push (`.cursor/rules/deploy-unity-web.mdc`).
