using System.IO;
using UnityEditor;
using UnityEngine;

namespace Maaloomatiia.Games.Editor
{
    // The web page every game loads in: the platform's cover while the game downloads,
    // the guard for screens too small to play on, and the way back to the menu. One copy
    // lives in the kit (WebGLTemplates~/Maaloomatiia, which Unity does not import) and is
    // put into the game's Assets/WebGLTemplates before every web build, because Unity only
    // takes templates from there.
    public static class WebTemplate
    {
        public const string Name = "Maaloomatiia";

        static string Source => Path.Combine(UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(WebTemplate).Assembly).resolvedPath, "WebGLTemplates~", Name);
        static string Target => Path.Combine(Application.dataPath, "WebGLTemplates", Name);

        // Copies the page in and makes it the game's web template.
        public static void Install()
        {
            if (!Directory.Exists(Source)) throw new DirectoryNotFoundException("The kit's web page is missing: " + Source);
            if (Directory.Exists(Target)) Directory.Delete(Target, true);
            Directory.CreateDirectory(Target);
            foreach (var file in Directory.GetFiles(Source))
                File.Copy(file, Path.Combine(Target, Path.GetFileName(file)), true);
            AssetDatabase.Refresh();
            PlayerSettings.WebGL.template = "PROJECT:" + Name;
            // The platform's cover is the splash: nothing else flashes between the menu and the game.
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            Debug.Log("[WebTemplate] " + Name + " installed from the kit.");
        }
    }
}
