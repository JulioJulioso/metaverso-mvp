# Pipeline de bake AEC → runtime

Checklist para llevar modelos de **Revit / Rhinoceros** al viewer Babylon (metros, origen rebased).

## Ruta recomendada (industria)

1. **Authoring:** Revit y/o Rhino.
2. **Intercambio:** export **IFC** (Revit) y/o publicar por **Speckle** (Revit + Rhino).
3. **Bake offline** (script, Blender, Unity como kitchen, o IFC→glTF):
   - Convertir geometría a mesh (triangulación).
   - Convertir a **metros**.
   - Restar anclaje del edificio (centroid o project base) → modelos con coords pequeñas.
   - Exportar **GLB** (glTF 2.0; idealmente Draco/Meshopt en fases posteriores).
   - Generar **bim-index.json** según [bimIndex.schema.json](./bimIndex.schema.json).
4. **Runtime:** copiar a `assets/models/` + `assets/bim/` y cargar vía `AssetLoader` / `BimIndexStub`.

## Checklist exportación

- [ ] Unidades de diseño conocidas (mm en Revit típico).
- [ ] Origen rebased documentado en `origin` del BIM index.
- [ ] Cada elemento interactuable tiene `globalId` estable.
- [ ] Cotas de negocio (espesor, altura libre) en `properties` en **mm**, no solo en la malla.
- [ ] Nombres de mesh enlazados (`meshName` ↔ GLB).
- [ ] Sin coordenadas survey en el rango de millones sin floating origin.

## Unity (opcional)

Unity no es el runtime. Puede usarse solo para importar / limpiar / reexportar GLB. El destino final es este proyecto web.
