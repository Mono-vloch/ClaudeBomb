using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

// Walks through every screen on its own and saves a screenshot of each:
//   "Tech Escape.app/Contents/MacOS/Tech Escape" -screenshots /some/folder
// The walk is silent: LUMA's voice off and the listener at zero.
public class DemoDriver : MonoBehaviour
{
    public static string Folder
    {
        get
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-screenshots") return args[i + 1];
            return null;
        }
    }

    // -cover <file>: one picture of the facility for the platform's menu instead of the walk.
    public static string CoverPath
    {
        get
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-cover") return args[i + 1];
            return null;
        }
    }

    public static bool Wanted => Folder != null || CoverPath != null;

    IEnumerator Start()
    {
        var app = GetComponent<App>();
        if (CoverPath != null) { yield return Cover(app); yield break; }
        Directory.CreateDirectory(Folder);
        Luma.Silent = true;
        AudioListener.volume = 0f;
        yield return new WaitForSeconds(1.2f);
        yield return Shot("1-title");
        app.StartLevel(1);
        yield return new WaitForSeconds(2.5f); // LUMA's line has been typed by then
        yield return Shot("2-briefing");
        app.ShowFile();
        yield return new WaitForSeconds(0.5f);
        yield return Shot("2c-file");
        app.ShowFile();
        app.StartLevel(3);
        app.ShowRoom();
        yield return new WaitForSeconds(9f); // LUMA's line said and gone: the feed as the player works in it
        yield return Shot("3-room");
        Coverage("3-room: the mail sorter", app.world.HeroOf(Facility.RoomIndex(app.room.room)));
        app.RunAttack();
        yield return new WaitForSeconds(3.7f); // on its way to the first door
        yield return Shot("4-attack");
        Coverage("4-attack: the attacker", app.world.AttackerTransform);
        float waited = 0f;
        while (AttackScreen.Running && waited < 40f) { waited += Time.unscaledDeltaTime; yield return null; }
        yield return new WaitForSeconds(0.6f);
        yield return Shot("4b-attack-end");
        app.ShowResults();
        yield return new WaitForSeconds(2.6f); // the medal has been revealed by then
        yield return Shot("5-debrief");
        app.StartLevel(7);
        app.ShowRoom();
        yield return new WaitForSeconds(0.8f);
        yield return Shot("6-room-ordered");
        // Every room from inside, with how much of the frame its prop takes.
        for (int r = 0; r < Facility.Rooms.Length; r++)
        {
            var level = Rooms.All.FirstOrDefault(x => x.room == Facility.Rooms[r]);
            if (level == null) continue;
            app.StartLevel(level.number);
            app.ShowRoom();
            yield return new WaitForSeconds(3.5f);
            string name = $"room-{r + 1:00}-{Facility.Rooms[r].ToLowerInvariant().Replace(' ', '-')}";
            yield return Shot(name);
            Coverage(name, app.world.HeroOf(r));
        }
        yield return new WaitForSeconds(0.3f);
        Application.Quit();
    }

    // The cover, as Claude Bomb's: the post room's mail sorter under its lamp, seen through
    // the security camera's feed, filling the whole window with nothing else drawn over it.
    IEnumerator Cover(App app)
    {
        Luma.Silent = true;
        AudioListener.volume = 0f;
        yield return null;
        var top = app.Stage.parent;
        var frame = Ui.Node("Cover", top);
        Ui.Stretch(frame);
        foreach (Transform t in top) if (t != frame) t.gameObject.SetActive(false);
        app.Paper(Color.clear);
        app.world.Build();
        int index = Facility.RoomIndex("Suspicious Inbox");
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i == index ? Building.DoorState.Open : i < index ? Building.DoorState.Held : Building.DoorState.Locked);
        app.world.ShowRoom(frame, index);
        app.atmosphere.MenuBlur(false);
        // The cover is wider than the room's feed in the game: the rack beside the camera
        // would fill its left edge out of focus, so it stays out of this picture.
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (t.name == "Rack") t.gameObject.SetActive(false);
        if (app.world.HasInterior(index))
            CameraFeed.Build(frame, $"CAM {index + 1:00}  /  SUSPICIOUS INBOX", "PERMIT OFFICE  /  LOCKDOWN");
        else { app.world.ShowOverview(frame); app.world.cam.pitch = 38; app.world.cam.yaw = 32; app.world.cam.Snap(); app.world.Demo(true); }
        yield return new WaitForSeconds(2.5f);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(CoverPath);
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }

    // How much of the frame the hero's bounding box covers, as the realism pass measures it.
    static void Coverage(string what, Transform hero)
    {
        var cam = Camera.main;
        if (hero == null || cam == null) { Debug.Log($"[Coverage] {what}: none"); return; }
        // The body only: a character's skinned meshes when it has them (not its ring on the floor).
        var all = hero.GetComponentsInChildren<Renderer>().Where(r => r.enabled && r.gameObject.activeInHierarchy && !(r is ParticleSystemRenderer)).ToArray();
        var rs = all.OfType<SkinnedMeshRenderer>().Any() ? all.OfType<SkinnedMeshRenderer>().Cast<Renderer>().ToArray() : all;
        if (rs.Length == 0) return;
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        float x0 = 1, x1 = 0, y0 = 1, y1 = 0;
        for (int i = 0; i < 8; i++)
        {
            var c = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            var v = cam.WorldToViewportPoint(c);
            x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
        }
        float w = Mathf.Clamp01(x1) - Mathf.Clamp01(x0), h = Mathf.Clamp01(y1) - Mathf.Clamp01(y0);
        Debug.Log($"[Coverage] {what}: {w:P0} x {h:P0} of the frame = {w * h:P0}");
    }

    IEnumerator Shot(string name)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(Folder, name + ".png"));
        yield return new WaitForSeconds(0.8f);
    }
}
