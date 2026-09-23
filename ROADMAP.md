# Roadmap

Fuente de verdad de prioridades y estado. El como-se-hace esta en `README.md` y `Docs/`.

## Estado (2026-09-23)

| Fase | Estado |
| --- | --- |
| 0. Repo, estructura, reglas | Hecho |
| 1. Web + WebXR Export 0.25 + XRI 3.2 + URP web | Hecho. Paquetes resueltos y compilando |
| 2. Rig dual PC / VR | Hecho en codigo. Falta probar en Quest |
| 3. Pipeline Revit / Rhino | Herramientas listas. Falta el primer modelo real |
| 4. Multiplayer Photon Fusion 2 | Codigo apagado. Falta SDK + App ID (ver `Docs/MULTIPLAYER.md`) |
| 5. Build y Pages | Pipeline listo. Falta el primer build publicado |
| 6. Mundo de prueba | Hecho (`Metaverso > Crear mundo de prueba`) |

## Proximos pasos, en orden

1. Correr EditMode y PlayMode en el Test Runner. Anotar el resultado en `Docs/QA.md`.
2. Primer `Metaverso > Build Web`, publicar en `metaverso-web` y pasar el checklist de PC y Quest.
3. Traer un modelo real de Revit o Rhino, pasar el reporte de presupuesto y hornear luz.
4. Photon: App ID, SDK, prefab de avatar, prueba PC + Quest en la misma sala.
5. Voz (Photon Voice 2).
6. Cuando haya cliente pago: mover el sitio a Cloudflare Pages y los modelos a Addressables en R2.

## Decisiones (no se reabren sin motivo)

- **Unity es el motor.** Babylon queda archivado en la rama `babylon-archive` (tag `babylon-final`): cada modelo y cada arreglo de VR se programaba a mano.
- **Un repositorio** ([metaverso-mvp](https://github.com/JulioJulioso/metaverso-mvp)): `main` es Unity y `gh-pages` es el sitio (un commit que se reemplaza, para no inflar el historial).
- **WebXR Export (De-Panther) 0.25, fijado.** Todo lo que toca WebXR pasa por `PlatformModeController`; si el plugin cambia, se toca solo `Scripts/XR`.
- **Photon Fusion 2 Shared Mode** para WebGL. `Metaverso.Fusion` compila solo con `PHOTON_FUSION`.
- **Color space lineal**, aunque WebXR Export avise que prefiere Gamma. La arquitectura se ve mejor en lineal.
- **Dos builds** (DXT para PC, ASTC para Quest): un solo formato de textura no sirve en los dos.
- **Controlador de PC propio** sobre CharacterController + Input System. Starter Assets no esta en el registro de paquetes y agrega Cinemachine sin necesidad.
- **Web sin app de tienda.** Build nativo de Quest solo si un contrato lo pide.
- **El agente no hace commit ni push.** Entrega los comandos.

## Fuera de alcance por ahora

- Mundos persistentes editables por el cliente.
- App en la tienda de Meta.
- Pixel streaming.
