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

Every level is a question and gives two training runs before the student's own: **Watch the pro** (a ready robot that
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
there. Any level can be started from the title screen's track (the one ladder in the game),
so a tester or a class can go straight to one idea. Free play opens every setting.

## How a run goes

The game-feel pass (7 November 2026, `Docs/game-feel/robot-racing.md`) made every decision an
action on the world and took the forms off the screens. Levels 1.1 and 2.3 are built this way
(the pilot); the other levels still open their settings in the pit lane as before.

1. **Briefing.** A transmission from race control, as Claude Bomb's: the car on its grid in
   the trackside camera's feed, three short lines typed in, one button. The level's full text
   is in the file, in the menu.
2. **The pit lane.** The car on its grid, the track beyond, and the rewards as things to
   place on it: on 1.1 the chequered flag alone leans on the pit wall (on the finish it turns
   the points on: the finish is the player's, the rest return to the ready robot's values);
   from 2.3 the shelf holds the cones (a crash), the teal chevrons (each step toward the
   finish), the red one pointing back (each step away), the clock post (each moment) and the
   orange speed sign (the bonus). Drag one onto the track, tap it to turn its number. One line
   says what to do; "Train" is the main action. (`World/Markers.cs`; no physics: the markers
   are picked by projecting them to the screen and dragged along the pointer's ray.)
3. **Training.** The run is computed first (`Core/Replay.cs`) and played back: every attempt
   at race speed, a thirty-second montage that slows for the first attempt, the first finish,
   the settle and the last, or straight to the end (the pace is in the menu; "Skip ahead" is
   the main action). The world shows the learning: every attempt leaves its ghost line on the
   asphalt, each move's points float up from the car, the fuel gauge drains with the
   attempts. "Camera" swaps the chase for the whole track. The curve, the rule book, the
   readings and the seed are behind "Details", closed by default.
4. **The race.** The start lights under the gantry go out, then five laps on the test track
   with the rule book as it is: no new moves, no learning. The lap is on the board by the grid
   and at the top of the screen. Level 3.4 then sends it to the real track, which has oil the
   simulator did not.
5. **The questions**, where the world can carry them (1.1): LUMA asks one at a time and the
   player taps the answer on the world: the car, the track, its sensors' rays, its wheels, the
   points floating over the track, or one of three signs beside the grid (`Ui/QuizScreen.cs`).
   Other levels' questions (the rule book, a line of text) keep the old list.
6. **Debrief.** The car where the race left it, the lap board, the medal with its stars over
   it, LUMA's one line, "Next" and a quiet "Again". The curve of every run, the checks, the
   runs' numbers, the stars in all and the rank, the takeaway and the question for work, and
   on a capstone the two if-then plans, are behind "Details". The platform is told the result
   in the curriculum's form (`2.3:complete`, `:partial` or `:incomplete`, with the checks).

Esc or the icon in the top-right corner opens the game menu (`Ui/GameMenu.cs`): resume, the
briefing, the file, this level again, the title screen, the games menu, and the settings
(sound, LUMA's voice, the training pace). The game pauses while it is open.

## Where things are

| Part | File |
|---|---|
| The tracks: ten courses as rows of text, the start, the finish, the oil | `Scripts/Core/Track.cs` |
| Sensing, the moves, one step of driving, crashes and touches | `Scripts/Core/Driving.cs` |
| What a student sets: sensors, moves, points, new moves, knobs, tracks, measure | `Scripts/Core/Settings.cs` |
| The learner: the rule book, an attempt, training, the race | `Scripts/Core/Learner.cs` |
| A training run computed in one go and kept as a log, for the montage | `Scripts/Core/Replay.cs` |
| The ladder: thirteen levels, what each opens, the ready and broken setups, questions, checks, lines | `Scripts/Core/Lesson.cs` |
| LUMA: the coach, her lines, portrait and voice | `Scripts/Ui/Luma.cs`, `Resources/Luma`, `Resources/LumaVoice`, `Editor/LumaVoiceTools.cs` |
| Screens | `Scripts/Ui/TitleScreen.cs`, `LessonScreen.cs` (the transmission; its warm-up question is turned off), `PitScreen.cs` (the car and the rewards), `RunScreen.cs` (training and the race), `QuizScreen.cs` (the questions on the world), `ResultsScreen.cs` (the debrief) |
| The menu (Esc, the icon) and the HUD's few pieces (the objective line, the fuel gauge, the small buttons) | `Scripts/Ui/GameMenu.cs`, `Hud.cs` |
| The curve | `Scripts/Ui/Chart.cs` |
| The circuit in three dimensions: the track, the stands, the robot, the crash and the finish | `Scripts/World/Circuit.cs`, `Circuit.Kit.cs` (the realism pass's kit) |
| The rewards as markers on the track; the lap board, the start lights, the taps on the world | `Scripts/World/Markers.cs`, `Circuit.Play.cs` |
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
- **LUMA's voice.** After changing or adding any line she says (her cues are in `Luma.cs`,
  the level lines in `Lesson.cs`), run **Robot Racing > Generate LUMA Voice** (macOS `say`; a
  published trailer needs a licensed voice). A build fails when a recording is missing
  (`Editor/LumaVoiceGuard.cs`). The walk below is silent.
- **Settings and progress persist** in PlayerPrefs on the machine.

## Checking it without opening the Editor

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.3.9f1/Unity.app/Contents/MacOS/Unity
$UNITY -batchmode -nographics -projectPath . -executeMethod Builder.BuildMac -logFile Logs/build.log
"Builds/mac/Robot Racing.app/Contents/MacOS/Robot Racing" -screenshots /tmp/shots -screen-width 1600 -screen-height 900 -screen-fullscreen 0
"Builds/mac/Robot Racing.app/Contents/MacOS/Robot Racing" -cover /tmp/cover.png -screen-width 1600 -screen-height 1000 -screen-fullscreen 0
```

The built app, started with `-screenshots <folder>`, walks through every screen of levels 1.1
and 2.3 (title, briefing, pit lane before and after placing, training, race, the questions,
the debrief and its details, the menu) and saves a picture of each, then quits, with
`counts.jsonl` beside them: the words and the controls on each screen, whether any LUMA line
is cut or long, and anything that should not be there (OPENS AT, a ladder, coordinates,
seeds, probabilities). `-cover` takes the platform's cover picture. The current set is in
`Builds/look-audit/game-feel/final`. Grep the build log for `error CS` and `Build:`.

## Web version

**Robot Racing > Build web** (or `-executeMethod Builder.BuildWeb -buildTarget WebGL` from the
command line) builds into `site/games/robot-racing` at the root of the repository, where the
Maaloomatiia Games menu launches it; the platform's page comes from the kit. See the root README.
