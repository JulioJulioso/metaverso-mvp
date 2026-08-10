/**
 * Level data in meters. Single source for placing demo entities.
 * When real GLB buildings load, interactables can also key off BIM GlobalIds.
 */
export const levelConfig = {
  units: 'm',

  player: {
    startPosition: { x: 0, y: 0, z: 0 },
    height: 1.7,
    radius: 0.35,
    moveSpeed: 4.5,
  },

  groundY: 0,

  platforms: [
    {
      id: 'platform-a',
      position: { x: 4, y: 0.35, z: 2 },
      size: { x: 3, y: 0.7, z: 2.5 },
      color: { r: 0.45, g: 0.5, b: 0.58 },
    },
    {
      id: 'platform-b',
      position: { x: -3.5, y: 0.9, z: -1 },
      size: { x: 2.5, y: 0.5, z: 2.5 },
      color: { r: 0.4, g: 0.55, b: 0.5 },
    },
  ],

  coins: [
    { id: 'coin-1', globalId: 'demo-coin-001', position: { x: 2, y: 1.2, z: 1 } },
    { id: 'coin-2', globalId: 'demo-coin-002', position: { x: 4, y: 1.5, z: 2 } },
    { id: 'coin-3', globalId: 'demo-coin-003', position: { x: -3.5, y: 2.0, z: -1 } },
    { id: 'coin-4', globalId: 'demo-coin-004', position: { x: -1, y: 1.0, z: 3 } },
    { id: 'coin-5', globalId: 'demo-coin-005', position: { x: 1.5, y: 1.0, z: -2.5 } },
  ],

  coinCollectRadius: 0.85,

  pickupSphere: {
    id: 'sphere-1',
    globalId: 'demo-sphere-001',
    position: { x: -2, y: 0.55, z: 2 },
    radius: 0.35,
    interactRadius: 1.2,
  },

  // Demo BIM properties for pick / index stub (mm for properties)
  bimMockElements: [
    {
      globalId: 'demo-ground',
      name: 'Suelo base',
      category: 'Floor',
      level: 'N0',
      meshName: 'ground',
      properties: { Thickness_mm: 200 },
    },
    {
      globalId: 'demo-coin-001',
      name: 'Marcador de recorrido 1',
      category: 'Annotation',
      level: 'N0',
      properties: { Height_mm: 100 },
    },
  ],
};

export default levelConfig;
