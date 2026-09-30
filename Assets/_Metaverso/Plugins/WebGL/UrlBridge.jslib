mergeInto(LibraryManager.library, {
  Metaverso_ReplaceQuery: function (searchPtr) {
    var search = UTF8ToString(searchPtr);
    try {
      history.replaceState(history.state, "", location.pathname + search + location.hash);
    } catch (e) {}
  },
  Metaverso_CopyText: function (textPtr) {
    var text = UTF8ToString(textPtr);
    function fallback() {
      var area = document.createElement("textarea");
      area.value = text;
      area.setAttribute("readonly", "");
      area.style.cssText = "position:fixed;left:-9999px;opacity:0;";
      document.body.appendChild(area);
      area.select();
      try { document.execCommand("copy"); } catch (e) {}
      area.remove();
    }
    if (navigator.clipboard && window.isSecureContext) {
      navigator.clipboard.writeText(text).catch(fallback);
    } else {
      fallback();
    }
  }
});
