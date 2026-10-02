// WebXR Export toma el contexto WebGL de Unity (Module.ctx) al iniciar. Con WebGPU no existe
// y su inicializacion tira una excepcion que tumba el player: sin contexto WebGL, VR queda apagado.
Module['WebXR'] = Module['WebXR'] || {};
(function (webxr) {
  var onUnityLoaded = webxr.onUnityLoaded;
  Object.defineProperty(webxr, 'onUnityLoaded', {
    configurable: true,
    get: function () {
      return function (event) {
        var module = event && event.detail && event.detail.module;
        if (!module || !module.ctx) {
          console.warn('[Metaverso] Sin contexto WebGL (WebGPU): VR desactivado en esta sesion. Usa ?gfx=webgl2 para VR.');
          return;
        }
        if (onUnityLoaded)
          return onUnityLoaded(event);
      };
    },
    set: function (value) {
      onUnityLoaded = value;
    }
  });
})(Module['WebXR']);
