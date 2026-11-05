# Robot Racing: Nobody Drives

Game 03 of the Maaloomatiia Academy series. Topic: how a machine learns from reward.

The story: every student gets the same robot, and nobody drives it. The student decides
what it senses, what it may do and what earns points; then the robot tries the track
over and over, crashes, slowly gets good, and finds out what it learns when the points
reward slightly the wrong thing (a speed bonus, and it spins instead of finishing).
Thirteen levels in three tiers teach one idea each, with LUMA, the coach from Claude
Bomb, who opens every level and reads the debrief. The content follows
`Docs/curriculum/robot-racing.md` at the root of the repository, level by level.

## Getting started

1. Install **Unity 6000.3.9f1** through Unity Hub.
2. Open this folder in Unity Hub (Add > Add project from disk). On the first open the
   project sets itself up: it imports the TextMesh Pro resources, bakes the sprites into
   `Assets/Resources/Sprites` and creates `Assets/Scenes/Main.unity`.
3. Open `Assets/Scenes/Main.unity` and press Play.

No Claude, no key, no network: the robot learns on the machine, from the points alone.

## The ladder

Every level is a question, opens with a warm-up question from an earlier level (no score),
and gives two training runs before the student's own: **Watch the pro** (a ready robot that
does the job), **Fix the broken bot** (the same robot with the level's planned mistake in it;
the class trains it, watches it fail, fixes the setting and trains again) and **Your run**,
the one that counts.

| Level | Question | Opens | Fix it: the planned mistake |
|---|---|---|---|
| 1.1 | Who is driving? | nothing: press run | No points at all: attempt 100 looks like attempt 1 |
| 1.2 | Is it learning, or just lucky? | how many attempts | Stopping at the first finish |
| 1.3 | Capstone: What kind of learning is this? | (none) | (none) |
| 2.1 | What does it sense? | the sensors | No sensor facing the bend |
| 2.2 | What may it do? | the allowed moves, a top speed | A robot that cannot slow down |
| 2.3 | What earns points? | the points table | Finish points only |
| 2.4 | What does it learn when you reward the wrong thing? | (same) | The speed bonus |
| 2.5 | Should it try something new? | the chance of trying new things, and how it falls | Never again, and forever |
| 2.6 | Capstone: Your robot, your rules | everything in tier 2 | (none) |
| 3.1 | Did it learn the track, or learn to drive? | training tracks, the test track | One track only |
| 3.2 | Which number tells the truth? | the measure | Judged by finishes alone |
| 3.3 | Which knob, and how do you know? | the knobs, the seed | Three knobs at once |
| 3.4 | Capstone: From the simulator to the real track | everything | The real track has oil the simulator did not |

The highest level finished is remembered on the machine, with the stars each earned (gold,
silver or bronze from the checks; for motivation only), and the title screen continues from
there. Any level can be started from the title screen's track or from the ladder on a
briefing, so a tester or a class can go straight to one idea. Free play opens every setting.

## How a run goes

1. **Briefing.** The warm-up question, then LUMA opens the level: the idea, the track, which
   setting opens. The robot's parts are listed on the right with the ones this level opens.
2. **The pit lane.** The track in three dimensions with the robot on its grid, and the
   settings: the tracks to train on and the test track, the sensors (front, left, right,
   speed, bend ahead), the moves it may make and its top speed, the points table (the
   finish, a crash, each step toward or away, each moment, a speed bonus), how often it
   tries something new and how that falls, the knobs (rewrite speed, attempts, seed) and the
   measure to judge it by. Every setting not opened by the level is shown locked with the
   level that opens it, so only the level's idea is in play.
3. **Training.** The robot tries the track attempt after attempt, at 1x, 10x, 100x or full
   speed. The HUD shows the attempt, what it senses right now, the move it made and what it
   earned, the chance of a new move, and the rule book it writes itself: for each situation
   it has met, the move that paid. The curve of points per attempt grows underneath; banners
   mark the first finish and the moment it settles. "Follow" rides behind the robot.
4. **The race.** Five laps on the test track with the rule book as it is: no new moves, no
   learning. Level 3.4 then sends it to the real track, which has oil the simulator did not.
5. **Debrief.** A few questions first (name the five parts, read a rule off the book, say why
   the no-points robot never improved), then the curve of every run of the level, the runs'
   numbers (first finish, settled, the race), the level's checks, a medal, LUMA's takeaway
   and the question to ask back at work, the next level's unlock, and on a capstone the two
   if-then plans. The platform is told the result in the curriculum's form (`2.3:complete`,
   `:partial` or `:incomplete`, with the checks).

## Where things are

| Part | File |
|---|---|
| The tracks: ten courses as rows of text, the start, the finish, the oil | `Scripts/Core/Track.cs` |
| Sensing, the moves, one step of driving, crashes and touches | `Scripts/Core/Driving.cs` |
| What a student sets: sensors, moves, points, new moves, knobs, tracks, measure | `Scripts/Core/Settings.cs` |
| The learner: the rule book, an attempt, training, the race | `Scripts/Core/Learner.cs` |
| The ladder: thirteen levels, what each opens, the ready and broken setups, questions, checks, lines | `Scripts/Core/Lesson.cs` |
| LUMA: the coach, her lines, portrait and voice | `Scripts/Ui/Luma.cs`, `Resources/Luma`, `Resources/LumaVoice`, `Editor/LumaVoiceTools.cs` |
| Screens | `Scripts/Ui/TitleScreen.cs`, `LessonScreen.cs` (the briefing and its warm-up), `PitScreen.cs` (the settings), `RunScreen.cs` (training and the race), `ResultsScreen.cs` (the debrief) |
| The curve | `Scripts/Ui/Chart.cs` |
| The circuit in three dimensions: the track, the stands, the robot, the crash and the finish | `Scripts/World/Circuit.cs` |
| The camera: overview, follow, orbit, portrait | `Scripts/World/SceneCamera.cs` |
| Shapes and materials, made in code; the three shaders | `Scripts/World/MeshKit.cs`, `Look.cs`, `Resources/Shaders` |
| Look and feel, from the Maaloomatiia brand guidelines | `Scripts/Ui/Theme.cs`, `UiKit.cs`, `Sfx.cs` |

Notes:

- **Everything is built in code.** The scene holds a camera and one `App` object;
  `App` builds the canvas and the screens at runtime. There are no prefabs.
- **The learner is a rule book.** For every situation the robot has met (what its
  sensors say) it keeps a value per move; after each move the value is rewritten toward
  the points it got, at the rewrite speed. It picks the best move it knows, except when
  it tries something new, at a chance that falls with every attempt. Nothing in it is
  named on screen with a term the courses do not use.
- **The same seed gives the same training**, so a class can compare one knob at a time.
- **The world is three-dimensional, and also built in code**, the way AI Arena's market
  is: shapes from `MeshKit.cs`, colours and three small shaders in `Look.cs`, a camera
  that keeps the circuit in a given part of the stage so the cards sit next to it.
- **LUMA's voice.** After changing any line she says, run **Robot Racing > Generate LUMA
  Voice** (macOS `say`; a published trailer needs a licensed voice). The walk below is
  silent.
- **Settings and progress persist** in PlayerPrefs on the machine.

## Checking it without opening the Editor

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.3.9f1/Unity.app/Contents/MacOS/Unity
$UNITY -batchmode -nographics -projectPath . -executeMethod Builder.BuildMac -logFile Logs/build.log
"Builds/mac/Robot Racing.app/Contents/MacOS/Robot Racing" -screenshots /tmp/shots -screen-width 1600 -screen-height 900 -screen-fullscreen 0
"Builds/mac/Robot Racing.app/Contents/MacOS/Robot Racing" -cover /tmp/cover.png -screen-width 1600 -screen-height 1000 -screen-fullscreen 0
```

The built app, started with `-screenshots <folder>`, walks through every screen (title,
briefing, warm-up, pit lane, training, race, the debrief's questions, the debrief) and saves a
picture of each, then quits; `-cover` takes the platform's cover picture. The current set is in
`Docs/screenshots`. Grep the build log for `error CS` and `Build:`.

## Web version

**Robot Racing > Build web** (or `-executeMethod Builder.BuildWeb -buildTarget WebGL` from the
command line) builds into `site/games/robot-racing` at the root of the repository, where the
Maaloomatiia Games menu launches it; the platform's page comes from the kit. See the root README.
