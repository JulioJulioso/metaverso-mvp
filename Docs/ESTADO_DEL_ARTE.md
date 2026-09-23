# Estado del arte

Para un metaverso de arquitectura que el cliente abre en el navegador del PC y en el del Quest, sin instalar nada. Fecha de esta lectura: septiembre 2026. Los precios y los cierres de producto hay que volver a mirarlos antes de una oferta.

## Lo que cerro o dejo de servirnos

Spatial cerro la Creator Platform (el hosting gratis y pro) el 27 de julio de 2026. Era el motivo de construir esto en casa. Mozilla Hubs cerro el servicio publico; queda Hubs Community Edition para quien quiera operarlo. No es un producto que se le entrega a un cliente inmobiliario sin un equipo de sistemas.

## Plataformas de mundo en el navegador

| Camino | Que resuelve | Por que no es el nuestro |
| --- | --- | --- |
| VIVERSE, Frame, Mona | Hosting, avatares y salas ya hechas | El modelo es el de ellos. Revit no entra con precision de obra y la marca es la de la plataforma |
| PlayCanvas | Motor web liviano, buen VR | Hay que rehacer el estudio en otro editor. El flujo ya vive en Unity |
| Hyperfy, Hubs CE | Mundos web de codigo abierto | Utiles como referencia de red y avatares. No importan un FBX de Revit con el control que necesitamos |
| Needle Engine | Se edita en Unity y sale a three.js. WebXR, voz y salas incluidas | Encaja tecnico. La licencia comercial ronda 49 euros al mes por asiento, facturados al ano (verificar en needle.tools). La logica pasa a TypeScript |
| Babylon.js (el MVP) | Ya corre en Pages y en Quest | Cada modelo y cada arreglo de VR se programa a mano. Por eso montar un 3D real se volvio dificil |

## Herramientas AEC en VR

The Wild, Arkio, Enscape, Twinmotion y Prospect sirven para revisar un modelo en VR dentro del proceso de diseno. No son el lugar donde un comprador entra con un link, en el navegador, junto con otra persona en otra ciudad. Esas herramientas siguen siendo utiles para producir el modelo. El metaverso es la capa de visita.

## La opcion que elegimos

Unity 6 exportado a web, con WebXR Export para el navegador del Quest y XR Interaction Toolkit para caminar, girar y agarrar. El estudio sigue en Unity, que es donde ya caen Revit y Rhino.

El costo de esa decision es el peso de la primera descarga (decenas de MB) y un techo estricto de geometria para que Quest Browser aguante. No es una app de la tienda de Meta: se abre como una pagina. La segunda visita usa la cache del navegador.

## Si un cliente pide calidad de render y no de visita

Pixel streaming (Unity Render Streaming, o servicios como Vagon y Arcane) manda el frame desde una GPU en la nube. Se ve como un ejecutable de escritorio, dentro del navegador. Cuesta una maquina encendida por sesion y la latencia se siente en VR. Tiene sentido para un render de preventa con el cliente sentado, no para una sala con varias personas en Quest. Se puede ofrecer aparte, con el mismo proyecto de Unity, el dia que alguien lo pague.

## Multiplayer

Para web, Photon recomienda Fusion 2 en modo Shared: el estado vive en la nube de Photon y el navegador no hace de servidor. El plan gratuito de desarrollo es del orden de 20 usuarios concurrentes (verificar en el dashboard). Netcode for GameObjects con Distributed Authority y websockets tambien llega a WebGL; es mas trabajo de hosting y menos voz. Colyseus serviria si el motor siguiera siendo Babylon. Con Unity, Fusion es el camino corto.

## Como se mantiene al dia

El cliente no actualiza una app. Recarga la URL. El hash de los archivos y la cache del navegador bajan solo lo nuevo. Cuando haya varios edificios, Addressables separan el reproductor del modelo para no recompilar por un cambio de mobiliario.
