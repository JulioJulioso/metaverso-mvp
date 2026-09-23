# Evaluacion comercial

Lectura de septiembre 2026 para decidir que se vende y que no. Las cifras marcadas "verificar" cambian; no las pongas en un contrato sin mirar la pagina del proveedor ese mes.

## A quien se le vende

Estudios de arquitectura y desarrolladores inmobiliarios que hoy hacen una reunion, un PDF y un render. El producto es una visita compartida: el cliente en su casa (PC, tercera persona) y el arquitecto en el Quest, o al reves, dentro del mismo `?room=`.

No es un juego, ni un metaverso generalista, ni un reemplazo de Enscape para el dia a dia del modelado.

## Ofertas que si cierran

1. **Showroom de preventa, por proyecto.** Un edificio, una sala, un link con fecha de vencimiento. Se cobra la puesta en escena (iluminacion, recorrido, puntos de informacion) mas un mes de hosting. Es el primer contrato realista: un solo mundo, pocos usuarios a la vez.
2. **Suscripcion por mundo.** Despues de la puesta en escena, una cuota mensual por mantener el link, la sala y los cambios menores (un material, un mobiliario). El tope tecnico es el peso del build y los usuarios concurrentes de Photon, no la cantidad de visitas historicas.
3. **Marca blanca para estudios.** El mismo motor, con el dominio y el iframe del estudio. Se cobra setup y una cuota por proyecto activo. El iframe ya esta resuelto; el dominio propio es un CNAME cuando salgamos de GitHub Pages.
4. **Evento.** Una sala abierta unas horas, con voz. Esto espera a Photon Voice y a una prueba con diez personas. No se vende todavia.

## Que no vender todavia

- Mundos persistentes que el usuario amuebla y que siguen ahi manana. Fusion puede guardar estado, pero no hay herramienta de edicion para el cliente.
- App en la tienda de Meta. Rinde un poco mas y complica cada actualizacion. Se ofrece solo si un contrato lo pide.
- Pixel streaming como producto de entrada. El costo por hora se come la preventa chica.

## Costos de operar (verificar)

| Pieza | Orden de magnitud | Nota |
| --- | --- | --- |
| Unity | Personal es gratis bajo un tope de ingresos y de fondos recaudados | El tope cambio mas de una vez. Mirar unity.com/products antes de facturar |
| Photon Fusion | Capa gratis de unos 20 CCU para desarrollar | Un showroom con 10 personas entra. Un evento no. El precio por CCU esta en el dashboard |
| GitHub Pages | Sirve para la demo | Tope de 1 GB por repo y ancho de banda con limite blando. No es el hosting de un cliente que paga |
| Cloudflare Pages + R2 | Capa gratuita holgada para este peso | Destino cuando el primer cliente pague. El pipeline ya separa desktop y quest |
| Dominio y HTTPS | Un dominio del estudio | El WebXR del Quest exige HTTPS. Pages y Cloudflare ya lo dan |

El costo variable de verdad es el tiempo de dejar el modelo de Revit dentro del presupuesto de Quest (triangulos, texturas, luz horneada), no el servidor.

## Precio de referencia para la primera oferta

No es una lista. Es el piso para no regalar el trabajo:

- Puesta en escena de un proyecto, con un FBX ya exportado por el estudio: el equivalente a una o dos semanas de una persona. Si el modelo llega sucio, se cotiza la limpieza aparte.
- Hosting y sala, por mes, por proyecto activo: una fraccion clara de esa puesta en escena, no un "gratis porque es un link".
- Cambios de geometria: se cobran. Cambiar un material o un texto entra en la cuota.

## Riesgos que hay que decir en la propuesta

- La primera carga en el Quest tarda. La segunda no, salvo que borren los datos del navegador. Hay que decirlo en la reunion, no descubrirlo en la demo.
- WebXR Export es un proyecto de la comunidad, fijado en la version 0.25. Si se rompe con un Unity futuro, el plan B es Needle o un build nativo de Quest. El codigo de modo PC/VR esta aislado para poder cambiar el plugin.
- GitHub Pages no es un SLA. El cliente que paga se muda a Cloudflare.
- Dos personas en la misma sala requieren el App ID de Photon. Hasta entonces la demo es de una persona, y se dice asi.
- Safari no entra en el plan de voz, y el VR es el navegador del Quest, no el de un iPhone.

## Como se ve una demo que se puede cobrar

Un link. En la laptop, tercera persona, teclado y mouse. En el Quest, el mismo link, boton de VR, y la otra persona en la sala. El HUD muestra la version. Si el cliente recarga y la version cambio, el mundo cambio. Eso es el argumento de "actualizable" frente a un ejecutable que hay que reinstalar.
