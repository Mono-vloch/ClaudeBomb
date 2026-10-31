using UnityEngine;
using UnityEngine.SceneManagement;

// Adds the screens that frame a run - the HUD, LUMA's tour, the game menu and the
// opening (title, briefing) - to every load of a scene with a GameManager, so the scene
// itself needs nothing for them. Restart reloads the scene, and they come back with it.
// The room around the device is Bay 3 (Resources/Lab.prefab, from Claude Bomb > Build Lab).
static class GameUi
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Hook()
    {
        SceneManager.sceneLoaded -= Add; // once, even without a domain reload between plays
        SceneManager.sceneLoaded += Add;
    }

    static void Add(Scene scene, LoadSceneMode mode)
    {
        if (GameManager.Instance == null || GameManager.Instance.gameObject.scene != scene) return;
        var go = new GameObject("GameUi");
        go.AddComponent<Hud>();
        go.AddComponent<Tour>();
        go.AddComponent<GameMenu>();
        go.AddComponent<Opening>();
        if (Camera.main != null) Camera.main.backgroundColor = Ui.Ink;
        var lab = Resources.Load<GameObject>("Lab");
        if (lab != null) Object.Instantiate(lab).name = "Lab";
    }
}
