# Pipeline de modelos (Revit / Rhino a Unity)

Unidad del mundo: **1 unidad = 1 metro**.

Todo archivo que entre en `Assets/_Metaverso/Models/Arch/` pasa por `ArchModelPostprocessor`: escala 1 con la escala del archivo, sin camaras ni luces importadas, UV de lightmap generadas y materiales desde la descripcion del archivo.

## Revit

1. Vista 3D con solo lo que se visita. Oculta anotaciones, habitaciones, masas y lineas.
2. Exportar **FBX**. Si Revit pregunta, metros y eje Y arriba.
3. Un FBX por nivel o sector. Un edificio entero en un archivo casi nunca entra en el presupuesto.
4. Copia el FBX a `Models/Arch`.

## Rhino 8

1. Unidades del documento en metros.
2. Exportar **FBX** o **GLB** por capas (estructura, fachada, mobiliario, vidrio).
3. GLB lo lee glTFast (`com.unity.cloud.gltfast`); FBX lo lee Unity. Los dos quedan en metros.
4. Copia el archivo a `Models/Arch`.

## En Unity

1. Arrastra el modelo a la escena.
2. Seleccionalo y usa **Metaverso > Preparar modelo arquitectonico**.
   - Agrega `MeshCollider` (no convexo) a cada malla, para caminar y chocar.
   - Marca las mallas como estaticas (batching, GI, oclusion) y en la capa `Architecture`.
   - Muestra un reporte: draw calls estimados, triangulos y textura mas grande contra el presupuesto.
3. Si el reporte dice ALTO, se simplifica en el programa de origen (menos subdivision, sin tornillos, LOD exportado). Unity no genera LODs solo.
4. Luz: una direccional y **luz horneada** (Window > Rendering > Lighting > Generate) cuando el modelo este fijo.
5. Si quieres una ficha al hacer clic, agrega `BimLabel` al objeto (nombre, categoria, nota).

## Presupuesto Quest Browser

| Medida | Tope |
| --- | --- |
| Draw calls (estimado: un renderer = uno) | 150 |
| Triangulos visibles | 500 000 |
| Textura | 2048 px (ASTC en el build de Quest) |
| `.data` comprimido | 50 MB (el build falla si se pasa) |
| Objetivo | 72 fps en Quest 3 |

## Que no hacer

- No importar el modelo en milimetros "y escalar despues": el postprocesador usa la escala del archivo.
- No dejar camaras ni luces de Revit: se descartan, pero ensucian la jerarquia si se exportan.
- No poner modelos fuera de `Models/Arch` si quieres que se preparen solos.
