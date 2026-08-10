# Pipeline AEC para metaverso web

## Estándar de capas

| Capa | Formato / herramienta | Uso |
|------|----------------------|-----|
| Diseño | Revit, Rhino | Verdad del proyecto |
| BIM abierto | IFC | Semántica, GlobalId, propiedades |
| Interop | Speckle, APS | Versionado y federación |
| Runtime web | glTF/GLB + bim-index JSON | GPU + interacción |
| Muy grande | 3D Tiles | Streaming / LOD (fase futura) |

Babylon.js **no** carga B-Rep de CAD. Carga mallas optimizadas y consulta metadatos BIM para precisión de cotas e interacción.

## Precisión milimétrica

1. Runtime: **1 unidad = 1 metro**.
2. Cotas BIM en sidecar: **milímetros**.
3. Rebase de origen en bake; opcionalmente `useLargeWorldRendering` en Babylon para obras grandes.
4. Mediciones de negocio: preferir parámetros del índice, no solo pick GPU a gran distancia.

## Flujo objetivo

```
Revit / Rhino → IFC o Speckle → bake GLB (m, rebased) + bim.json → Babylon
```

Unity es un **baker opcional**, no el producto final.

Ver también [`../pipeline/README.md`](../pipeline/README.md).
