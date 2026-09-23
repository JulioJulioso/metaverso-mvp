# Multiplayer (Photon Fusion 2)

Estado: el codigo esta escrito y apagado. Sin el SDK el mundo corre en local y lo dice en la consola.

## Por que Fusion en Shared Mode

Photon recomienda Shared Mode para WebGL: el estado vive en la nube de Photon y ningun navegador hace de servidor. Client/Host en WebGL viene deshabilitado por defecto en Fusion 2.

## Activarlo

1. Crea una app en [dashboard.photonengine.com](https://dashboard.photonengine.com), tipo **Fusion**, y copia el App ID.
2. Descarga el SDK de **Fusion 2** e importalo al proyecto.
3. Menu **Metaverso > Red > Activar Photon Fusion**. Agrega el define `PHOTON_FUSION` para Web y la referencia `Fusion.Runtime` a `Metaverso.Fusion.asmdef`.
4. Pega el App ID en el `PhotonAppSettings` que crea el SDK.
5. Crea un prefab con `NetworkObject` + `NetworkAvatar` (cabeza y dos manos como hijos, con algo visible) y registralo en la config de Fusion.
6. Agrega `FusionSession` a un objeto de la escena y asigna el prefab.
7. Opcional: en la pelota, `NetworkObject` + `NetworkPickupState`.

## Salas

- La sala sale de `?room=` en la URL, saneada a minusculas, numeros, `-` y `_` (max. 32).
- Sin parametro: `lobby`.
- `?name=Ana` pone el nombre del avatar.

Ejemplo: `https://juliojulioso.github.io/metaverso-mvp/?room=torre-norte&name=Ana`

## Probarlo

1. Build web y publicar (ver `DEPLOY.md`).
2. Abre la misma URL en la PC y en el Quest.
3. Cada uno debe ver al otro: en VR cabeza y manos; en PC la cabeza a la altura de la camara.

## Pendiente

- Voz: Photon Voice 2 soporta WebGL (no Safari). No esta integrado.
- Capa gratuita de Photon: del orden de 20 usuarios concurrentes. Revisar el dashboard antes de un evento.
