mergeInto(LibraryManager.library, {
  Metaverso_ShowVideo: function (urlPtr) {
    var url = UTF8ToString(urlPtr);
    var existing = document.getElementById("metaverso-video");
    if (existing) existing.remove();
    var wrap = document.createElement("div");
    wrap.id = "metaverso-video";
    wrap.style.cssText = "position:fixed;inset:8%;z-index:40;background:#141210;display:flex;flex-direction:column;border-radius:8px;overflow:hidden;";
    var bar = document.createElement("button");
    bar.textContent = "Cerrar";
    bar.style.cssText = "align-self:flex-end;margin:8px;padding:8px 16px;cursor:pointer;";
    bar.onclick = function () { wrap.remove(); };
    var frame = document.createElement("iframe");
    frame.src = url;
    frame.allow = "autoplay; encrypted-media; fullscreen";
    frame.style.cssText = "flex:1;border:0;width:100%;background:#000;";
    wrap.appendChild(bar);
    wrap.appendChild(frame);
    document.body.appendChild(wrap);
  },
  Metaverso_HideVideo: function () {
    var existing = document.getElementById("metaverso-video");
    if (existing) existing.remove();
  }
});
