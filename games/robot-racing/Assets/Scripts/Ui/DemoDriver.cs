using System.Collections;
using System.IO;
using UnityEngine;

// Walks through every screen on its own and saves a screenshot of each, so the
// layout can be checked from the command line:
//   "Robot Racing.app/Contents/MacOS/Robot Racing" -screenshots /some/folder
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

    // -cover <file>: one picture of the circuit for the platform's menu instead of the walk.
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
        AudioListener.volume = 0f; // the walk is silent: it runs on a desk, not in a class
        yield return new WaitForSeconds(1.2f);
        yield return Shot("1-title");

        app.StartLevel(1);
        yield return new WaitForSeconds(0.5f);
        yield return Shot("2-briefing");
        app.StartLevel(2);
        yield return new WaitForSeconds(0.5f);
        yield return Shot("2b-warm-up");

        app.StartLevel(Ladder.PointsLevel);
        app.SetStep(App.Step.YourTurn);
        app.ShowPit();
        yield return new WaitForSeconds(0.8f);
        yield return Shot("3-pit-lane");

        app.StartLevel(1);
        app.StartTraining();
        yield return new WaitForSeconds(1.5f);
        RunScreen.Current?.SetSpeed(0);
        yield return new WaitForSeconds(1.5f);
        yield return Shot("4-training-slow");
        RunScreen.Current?.SetSpeed(3);
        float waited = 0;
        while (RunScreen.Current != null && RunScreen.Current.phase == RunScreen.Phase.Training && waited < 40f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.8f);
        yield return Shot("5-race");
        RunScreen.Current?.SetSpeed(3);
        waited = 0;
        while (RunScreen.Current != null && RunScreen.Current.phase != RunScreen.Phase.Done && waited < 40f) { waited += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.5f);
        app.FinishLevel();
        yield return new WaitForSeconds(0.5f);
        yield return Shot("6-debrief-questions");
        ResultsScreen.Current?.SkipQuiz();
        yield return new WaitForSeconds(2.4f); // the medal has been revealed by then
        yield return Shot("7-debrief");
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }

    // The cover: the grand circuit at dusk with the robot on its way round, filling the
    // whole window with nothing drawn over it.
    IEnumerator Cover(App app)
    {
        Luma.Silent = true;
        AudioListener.volume = 0f;
        yield return null;
        var canvas = app.Stage.parent.GetComponent<Canvas>();
        var frame = Ui.Node("Cover", app.Stage.parent);
        Ui.Stretch(frame, 30, 30, 30, 30);
        app.Paper(Color.clear);
        app.world.Build(Track.Get("grand"), true);
        app.world.ShowOverview(frame);
        app.world.cam.pitch = 40; app.world.cam.yaw = 28; app.world.cam.Snap();
        app.world.Demo(true);
        canvas.enabled = false;
        yield return new WaitForSeconds(2.5f);
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
