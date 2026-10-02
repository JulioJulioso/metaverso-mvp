# QA

## Automatico

| Que | Como correrlo |
| --- | --- |
| EditMode: version, sala, presupuesto, reglas de importacion, modo PC/VR, URL, catalogo, nivel de calidad | Unity > Window > General > Test Runner > EditMode > Run All |
| PlayMode: la pose pasa de PC a VR y de vuelta | Test Runner > PlayMode > Run All |
| Tamano del build | Automatico en cada build web (`BuildSizeCheck`, 50 MB) |
| Archivos clave del repo y del sitio | `node Tools/qa/check-shell.mjs` |
| Sitio publicado | `cd Tools/qa`, `npm install`, `npx playwright install chromium`, `npx playwright test` |

Contra el sitio real: `$env:METAVERSO_URL="https://juliojulioso.github.io/metaverso-mvp/"` antes de `npx playwright test`. Sin esa variable, el test del canvas se salta y el del shell usa `metaverso-web/docs` local.

Desde la linea de comandos (con Unity cerrado):

```powershell
& "D:\Unity\6000.3.9f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs\editmode.xml
```

## Manual, antes de cada publicacion

### PC (Chrome, Edge, Firefox)

- [ ] El HUD muestra la version que imprimio el build.
- [ ] WASD mueve relativo a la camara; el mouse gira despues de hacer clic; Escape libera el mouse.
- [ ] Espacio salta y se llega a la plataforma alta (se marca el paso).
- [ ] Se recogen los 5 marcadores.
- [ ] E toma la pelota, F la suelta, cae y rueda.
- [ ] La pelota entregada en 1, 2 y 3 completa los pasos en orden.
- [ ] Cerca de los muros aparecen los botones; levantar y despiece funcionan.
- [ ] Clic en la pantalla abre el video con boton Cerrar.
- [ ] El HUD muestra `PC WebGL2` (o `Ultra WebGPU` con el spike) junto a la version.
- [ ] El boton **Calidad** pasa por PC, Ultra y Quest, se nota el cambio y queda guardado al recargar.
- [ ] `?quality=quest` entra en Quest; `?quality=ultra` entra en Ultra.

### WebGPU (solo si el build desktop salio con `desktopGraphics: webgpu+webgl2`)

- [ ] Chrome o Edge con WebGPU: el HUD dice `Ultra WebGPU`; consola sin errores de WebGPU.
- [ ] Firefox o un navegador sin WebGPU: entra solo en `PC WebGL2`.
- [ ] `?gfx=webgl2` fuerza `PC WebGL2`.
- [ ] Si WebGPU falla al iniciar, la pagina recarga sola con `?gfx=webgl2`.
- [ ] Sombras, SSAO, bloom y video se ven igual que en WebGL2. Anotar FPS de los dos en el mismo PC.
- [ ] Resultado del spike anotado abajo: si no es estable, se apaga la marca y WebGPU queda para Unity 6.6.

### Quest Browser

- [ ] La pagina carga (primera vez tarda; la segunda es mas rapida).
- [ ] El boton VR entra a la sesion inmersiva.
- [ ] Stick izquierdo camina hacia donde miras; stick derecho gira 30 grados.
- [ ] Gatillo cerca de la pelota la toma con la mano; grip la suelta.
- [ ] Los marcadores se recogen al pasar caminando.
- [ ] El HUD se lee a 2 m; los botones de muros aparecen al acercarse.
- [ ] Salir de VR deja al jugador de PC donde estaba la cabeza.
- [ ] El HUD de mundo muestra `Quest WebGL2` junto a la version.
- [ ] Rendimiento: sin tirones al girar (objetivo 72 fps en Quest 3, 60 en Quest 2).

### Multiplayer (cuando Photon este activo)

- [ ] PC y Quest en la misma `?room=` se ven.
- [ ] Salas distintas no se mezclan.
- [ ] La pelota tomada por uno se mueve en el otro.

## Ultimo resultado

| Fecha | Resultado |
| --- | --- |
| 2026-09-23 | Todas las assemblies compilan en Unity 6000.3.9f1. `check-shell` y Playwright (shell) pasan. EditMode/PlayMode pendientes de correr en el Test Runner. Sin build web todavia |
| 2026-10-02 | Niveles de calidad: compila, EditMode 27/28 (falla `RoomQuery_StripsUnsafeCharacters`, test viejo que espera `sala_1` y el codigo da `sala1`, igual que `WorldRulesTests`). En Play del editor: PC entra en `Web_PC`, el selector recorre Ultra, Quest y PC. Spike WebGPU: pendiente de build y prueba en navegador |
