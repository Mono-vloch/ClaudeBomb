// The one thing a web build cannot do from C#: leave the page. Application.OpenURL
// opens a new tab; the platform wants the same tab, so the menu comes back where the
// game was.
mergeInto(LibraryManager.library, {
  MaalPlatform_SetLocation: function (urlPtr) {
    window.location.href = UTF8ToString(urlPtr);
  },
});
