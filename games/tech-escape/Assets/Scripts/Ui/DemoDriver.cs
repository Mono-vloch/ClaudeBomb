using System.Collections;
using System.IO;
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
        app.StartLevel(2);
        yield return new WaitForSeconds(0.6f);
        yield return Shot("2b-warm-up");
        app.StartLevel(3);
        app.ShowRoom();
        yield return new WaitForSeconds(0.8f);
        yield return Shot("3-room");
        app.RunAttack();
        yield return new WaitForSeconds(7f);
        yield return Shot("4-attack");
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
        yield return new WaitForSeconds(0.3f);
        Application.Quit();
    }

    // The cover: the facility in lockdown from above, the first doors held, one breached,
    // the attacker in the corridor, filling the whole window with nothing drawn over it.
    IEnumerator Cover(App app)
    {
        Luma.Silent = true;
        AudioListener.volume = 0f;
        yield return null;
        var canvas = app.Stage.parent.GetComponent<Canvas>();
        var frame = Ui.Node("Cover", app.Stage.parent);
        Ui.Stretch(frame, 30, 30, 30, 30);
        app.Paper(Color.clear);
        app.world.Build();
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i < 5 ? Building.DoorState.Held : i == 5 ? Building.DoorState.Breached : Building.DoorState.Locked);
        app.world.ShowOverview(frame);
        app.world.cam.pitch = 38; app.world.cam.yaw = 32; app.world.cam.Snap();
        app.world.LightRooms(true);
        app.world.Demo(true);
        app.atmosphere.MenuBlur(false);
        canvas.enabled = false;
        yield return new WaitForSeconds(1.6f);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(CoverPath);
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }

    IEnumerator Shot(string name)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(Folder, name + ".png"));
        yield return new WaitForSeconds(0.8f);
    }
}
