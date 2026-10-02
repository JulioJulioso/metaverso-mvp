mergeInto(LibraryManager.library, {
  Metaverso_UserAgent: function () {
    var ua = (typeof navigator !== "undefined" && navigator.userAgent) || "";
    var size = lengthBytesUTF8(ua) + 1;
    var buffer = _malloc(size);
    stringToUTF8(ua, buffer, size);
    return buffer;
  },
  // WebXR Export lee Module.devicePixelRatio cada frame para el tamano del canvas (fuera de VR).
  Metaverso_SetPixelRatioCap: function (cap) {
    var ratio = window.devicePixelRatio || 1;
    Module.devicePixelRatio = cap > 0 ? Math.min(ratio, cap) : ratio;
  },
  Metaverso_ReportGraphics: function (apiPtr, tierPtr) {
    window.metaversoGraphics = { api: UTF8ToString(apiPtr), tier: UTF8ToString(tierPtr) };
  }
});
