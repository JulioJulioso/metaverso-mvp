/**
 * Level layout in meters — circuit demo with platforms, walls, media, ball drops.
 */
export const levelConfig = {
  units: 'm',

  player: {
    startPosition: { x: 0, y: 0, z: 0 },
    height: 1.75,
    radius: 0.28,
    moveSpeed: 4.2,
    jumpSpeed: 6.2,
    gravity: 18,
    maxStepHeight: 0.42,
  },

  groundY: 0,

  /**
   * Platform path: low steps walkable; high peaks require jump (taller than maxStepHeight).
   * Final platform tip holds the last coin.
   */
  platforms: [
    {
      id: 'pad-start',
      position: { x: 2.5, y: 0.2, z: 1.5 },
      size: { x: 2.2, y: 0.4, z: 2.2 },
      albedo: { r: 0.42, g: 0.4, b: 0.38 },
    },
    {
      id: 'pad-mid-low',
      position: { x: 5.2, y: 0.35, z: 2.8 },
      size: { x: 2.0, y: 0.45, z: 2.0 },
      albedo: { r: 0.4, g: 0.39, b: 0.37 },
    },
    {
      id: 'pad-jump-high',
      position: { x: 8.0, y: 1.45, z: 3.5 },
      size: { x: 2.4, y: 0.35, z: 2.4 },
      albedo: { r: 0.36, g: 0.35, b: 0.34 },
      requiresJump: true,
    },
    {
      id: 'pad-bridge',
      position: { x: 10.5, y: 1.5, z: 3.5 },
      size: { x: 2.8, y: 0.3, z: 1.6 },
      albedo: { r: 0.38, g: 0.37, b: 0.35 },
    },
    {
      id: 'pad-final',
      position: { x: 13.2, y: 1.55, z: 3.5 },
      size: { x: 2.5, y: 0.35, z: 2.5 },
      albedo: { r: 0.34, g: 0.33, b: 0.32 },
    },
    {
      id: 'pad-ball-area',
      position: { x: -4, y: 0.15, z: 4 },
      size: { x: 6, y: 0.3, z: 5 },
      albedo: { r: 0.4, g: 0.39, b: 0.38 },
    },
  ],

  coins: [
    { id: 'coin-1', globalId: 'demo-coin-001', position: { x: 2.5, y: 0.85, z: 1.5 } },
    { id: 'coin-2', globalId: 'demo-coin-002', position: { x: 5.2, y: 1.05, z: 2.8 } },
    {
      id: 'coin-3',
      globalId: 'demo-coin-003',
      position: { x: 8.0, y: 2.15, z: 3.5 },
      note: 'On high jump platform',
    },
    { id: 'coin-4', globalId: 'demo-coin-004', position: { x: 10.5, y: 2.1, z: 3.5 } },
    {
      id: 'coin-final',
      globalId: 'demo-coin-final',
      position: { x: 13.5, y: 2.2, z: 3.5 },
      note: 'End of platform zone',
    },
  ],

  coinCollectRadius: 0.9,

  pickupSphere: {
    id: 'sphere-1',
    globalId: 'demo-sphere-001',
    position: { x: -4, y: 0.55, z: 2.5 },
    radius: 0.32,
    interactRadius: 1.25,
  },

  /** Carry the ball here in order after all coins are collected. */
  deliveryZones: [
    {
      id: 'zone-1',
      label: 'Sitio de entrega 1',
      position: { x: -6, y: 0.05, z: 5.5 },
      radius: 1.1,
      color: { r: 0.15, g: 0.45, b: 0.55 },
    },
    {
      id: 'zone-2',
      label: 'Sitio de entrega 2',
      position: { x: -2, y: 0.05, z: 6.5 },
      radius: 1.1,
      color: { r: 0.2, g: 0.4, b: 0.5 },
    },
    {
      id: 'zone-3',
      label: 'Sitio de entrega 3',
      position: { x: -4, y: 0.05, z: 0.5 },
      radius: 1.1,
      color: { r: 0.18, g: 0.38, b: 0.48 },
    },
  ],

  walls: {
    id: 'wall-assembly',
    origin: { x: -2, y: 0, z: -5 },
    count: 5,
    width: 0.18,
    height: 2.8,
    depth: 2.4,
    spacing: 0.22,
    riseDuration: 4.5,
    explodeDistance: 1.15,
    explodeDuration: 1.4,
  },

  video: {
    youtubeId: 'dQw4w9WgXcQ',
    title: 'Referencia de proyecto (demo)',
    screenPosition: { x: 4, y: 1.6, z: -4.5 },
    screenSize: { width: 2.4, height: 1.35 },
  },

  circuitSteps: [
    { id: 'walls_rise', label: 'Observar levantamiento de muros' },
    { id: 'walls_explode', label: 'Activar visión explotada de muros (X)' },
    { id: 'coins_all', label: 'Recoger todos los marcadores del recorrido' },
    { id: 'jump_high', label: 'Alcanzar plataforma alta (salto)' },
    { id: 'ball_zone_1', label: 'Llevar la pelota al sitio 1' },
    { id: 'ball_zone_2', label: 'Llevar la pelota al sitio 2' },
    { id: 'ball_zone_3', label: 'Llevar la pelota al sitio 3' },
    { id: 'circuit_done', label: 'Circuito completo' },
  ],

  bimMockElements: [
    {
      globalId: 'demo-ground',
      name: 'Losa N0',
      category: 'Floor',
      level: 'N0',
      meshName: 'ground',
      properties: { Thickness_mm: 200, Finish: 'Concrete polished' },
    },
    {
      globalId: 'demo-coin-final',
      name: 'Marcador final de recorrido',
      category: 'Annotation',
      level: 'N0',
      properties: { Height_mm: 100 },
    },
  ],
};

export default levelConfig;
