# AI Arena: Send the Robot

Game 02 of the Maaloomatiia Academy series. Topic: what an AI agent is, and how to
work with one.

The story: every evening the family texts what dinner needs. Each student turns the
message into a shopping list for their household robot. Then the robots leave for
the market, and nobody can help them. At every stall a robot looks, decides, moves
and texts home one line (the loop). The right things go in the basket; a wrong one
ruins dinner and sends the robot home. Every LLM concept is one part of the robot
(the list is its system prompt, how far it sees is its context, its memory, its
brain, its pocket money in tokens), so a vague list does not score badly: it comes
home with a tomato in the fruit salad, in front of the class. Six errands teach it
one idea at a time, with LUMA, the coach from Claude Bomb, who tells the story
first and shows the first screen around.

## Getting started

1. Install **Unity 6000.3.9f1** through Unity Hub.
2. Open this folder in Unity Hub (Add > Add project from disk). The first open takes
   a while. On that first open the project sets itself up: it imports the TextMesh Pro
   resources, bakes the sprites into `Assets/Resources/Sprites` and creates
   `Assets/Scenes/Main.unity`.
3. Open `Assets/Scenes/Main.unity` and press Play.

**Claude in the game.** Claude is reached through
[Claude Code](https://claude.com/claude-code), signed in with a Claude subscription
(`claude`, then `/login`). No API key is used anywhere. On the desktop the game runs
`claude -p` itself; in the browser the page asks the Maaloomatiia Games platform it
runs on (see "Web version"). Each turn takes a few seconds.

Without Claude Code the game still runs: a simple **stand-in brain** moves the agents.
It reads the instructions like a very literal machine and is nothing like Claude, so
use it to learn the controls, not to teach.

## The ladder

Students may arrive not knowing what an agent is, so the game starts with the story
(six animated beats, told by LUMA), then teaches one idea per errand and opens one more
dial each time. The errands follow tier 1 of the curriculum (`Docs/curriculum/ai-arena.md`):
each is a question and gives two
training runs before the students' own. LUMA opens every errand, gives a tour of the
garage on the first one, reads out the first turn of the first errand as one loop, speaks
up during a run the first time something worth naming happens, and reads the recap on the
debrief. Her words are the ones Maaloomatiia's courses use for an agent: a new colleague
with a narrow job, with a job description, a brain, what it sees, a notebook, a budget,
and a manager who keeps the accountability.

| Errand | Question | The family's message | Opens | Fix it: the planned mistake |
|---|---|---|---|---|
| 1.1 | What does an agent do on its own? | First errand (apples only) | The list | Nothing to fix: watch the loop |
| 1.2 | What exactly is its job? | Fruit salad (the tomato) | (same) | A job description with a category word |
| 1.3 | What does it see? | Red party | How far it sees | Nothing on the list in view, and no rule for that |
| 1.4 | What does it remember? | Four things | Memory | No memory |
| 1.5 | Which brain does the job? | Grandma's allergies | Brain and effort | The biggest brain on an easy job |
| 1.6 | Does this task need an agent? | Five tasks to decide | Agent, fixed rule, or neither | The wrong tool for the task |
| 1.7 | Capstone: Who is accountable? | Yellow and green | Everything, no help | (none) |

The highest errand finished is remembered on the machine, with the stars each earned
(gold, silver or bronze, from the checks; for motivation only), and the title screen
continues from there. Any errand can be started directly from the title screen's track or
from the ladder on a briefing. Free play keeps every dial open with any message.

## How a run goes

Since the game-feel pass (`Docs/game-feel/ai-arena.md`, report in `Docs/look-audit.md`, Phase 7c)
every decision is an action on a thing in the world, and the screens carry one objective line, the
menu icon and at most three small controls besides the main action.

1. **Title.** The player's own agent in the evening square: tap it to paint it the next colour, tap
   its name tag to rename it. The errand map (seven stops and their stars) is the only ladder; Start
   or Continue. The first Start plays **the story**: four short 3D shots with one LUMA sentence each
   (the phone lights up, the note on the agent, it walks into the market, the tomato), skippable.
2. **Briefing.** A transmission: the family's house at dusk in the home camera's feed, the message
   typed in under its sender, two short lines of the errand's story, "Pack the agent".
3. **The garage: packing the agent.** The agent at a market table by its house. Drag produce into
   its basket ("Buy apples." on its note) or into the No crate ("Never buy tomatoes."); the catch and
   the rule for an empty view stay in the player's own words: tap the note to write the list, word
   for word (`Core/NoteText.cs` keeps the note and the table in step). Coins go into the battery on
   its hip, whose ring of notches shows how many turns of the errand the money buys. From 1.3 the
   parts this errand opens stand on a shelf at the back of the table, each a thing with its name
   plate on it, and are fitted with a tap: lenses on a rack ("Sight", worn on its eye), notebooks
   ("3 turns", "10 turns", "Notebook", carried in its hand), chips ("Haiku", "Sonnet", "Opus", worn
   on its head), the effort dial ("Effort"). Parts an errand does not open are not shown, and no
   plate floats: every one sits on its object. The phone holds the family's message. "Send".
   Errand 1.6 is the kitchen table instead: five notes in a pile, dropped on the agent, the rule box
   or the bin.
4. **The round.** The street behind the agent, at dusk with the lamps lit (the whole errand is at
   dusk). The clock at the top runs toward dinner, amber in the last five turns; the family's window at the top right has a face per agent;
   the battery drains; the agent's thought is a bubble (Claude's first sentence, coordinates taken
   out; the whole of it on a tap). Camera cycles the street, the agent's eye and the market from
   above; Call home is the off switch. Banners for the moments (sent home, out of tokens, all home).
5. **Debrief.** The agent home at dusk on its podium with its basket tipped out (one step for one
   agent; steps 2 and 3 when several ran), Robot Racing's ribbon medal over the step's front, LUMA's
   one line, Next, Again and Details (the checks, the agents' figures and costs, the lists, the stars
   in all, the takeaway and the question; the capstone's if-then plans). The platform is told the
   result in the curriculum's form (`1.3:complete`, `:partial` or `:incomplete`, with the checks).

The menu (Esc or the icon) holds the briefing, the file, this errand again, the story, free play,
sound, LUMA's voice, the speed, the turns, the class (one agent per student) and the way out.

## Where things are

| Part | File |
|---|---|
| Items and their families and colours | `Scripts/Core/Items.cs` |
| The six family messages | `Scripts/Core/Brief.cs` |
| The ladder: seven levels, their locked dials, warm-up questions, ready setups, checks and lines | `Scripts/Core/Lesson.cs` |
| Errand 1.6: the five tasks; the fixed rule that is a simple automation | `Scripts/Core/Decide.cs`, `Scripts/Brains/Brains.cs` (`RuleBrain`) |
| LUMA: the coach, her lines, portrait and voice | `Scripts/Ui/Luma.cs`, `Scripts/Core/Coach.cs`, `Resources/Luma`, `Resources/LumaVoice`, `Editor/LumaVoiceTools.cs` |
| What happened to an agent and why | `Scripts/Core/Verdicts.cs` |
| What a student sets | `Scripts/Core/AgentConfig.cs` (models, memory modes, effort) |
| The grid, spawning, home cells | `Scripts/Core/Arena.cs` |
| The turn loop and move resolution | `Scripts/Core/Simulation.cs` |
| What the agent is told | `Scripts/Brains/Observation.cs` |
| Reading the agent's answer | `Scripts/Brains/Decision.cs` |
| Claude brain and stand-in brain | `Scripts/Brains/Brains.cs` |
| Reaching Claude: Claude Code on the desktop, the platform's key keeper in the browser | `Scripts/Claude/*.cs`, the kit in `../../kit` |
| Look and feel, from the Maaloomatiia brand guidelines | `Scripts/Ui/Theme.cs`, `UiKit.cs`, `Sfx.cs` (sounds drawn at start), `Resources/Brand` |
| Screens | `Scripts/Ui/TitleScreen.cs`, `StoryScreen.cs`, `LessonScreen.cs` (the briefing), `SetupScreen.cs` (the garage), `DecideScreen.cs` and `DecideDebrief.cs` (errand 1.6), `ArenaScreen.cs` (the round), `ResultsScreen.cs` (the debrief), `GameMenu.cs`, `Hud.cs` (the objective, the clock, the window, the moments), `FileSheet.cs` |
| The garage, the round's and the debrief's things in the world: the table, the produce, the parts, the battery, the clock, the podium, the dark plates | `Scripts/World/Market.Garage.cs`, `Market.Round.cs`, `Market.Props.cs` |
| The market in three dimensions: the square, the stalls, the houses, the robots, a turn's moves, the robot's-eye view | `Scripts/World/Market.cs`, `RobotModel.cs`, `ItemModels.cs` |
| The camera on the market: overview, robot's eye, the cover's slow turn | `Scripts/World/MarketCamera.cs` |
| Shapes and materials, made in code; the three shaders | `Scripts/World/MeshKit.cs`, `Look.cs`, `Resources/Shaders` |
| Sprites, drawn from shapes (the story, the catalogue, the debrief) | `Editor/SpriteBaker.cs` (menu: AI Arena > Bake Sprites) |

Notes:

- **Everything is built in code.** The scene holds a camera and one `App` object;
  `App` builds the canvas and the screens at runtime. There are no prefabs to keep in sync.
- **The look follows the brand guidelines.** White pages with the teal-tinted neutral
  ramp, Inter, the `OO` emblem bottom right of every light screen; dark (Ink) only
  on the two cover screens, the title and the story, which carry the wordmark. The
  brand pictures in `Resources/Brand` and LUMA's portrait must import as single
  sprites; `ProjectSetup.FixPictures` makes sure of it before every build.
- **The market is three-dimensional, and also built in code.** The scene's camera
  renders the market (`Scripts/World`) behind the canvas, and the title and arena
  screens make the page see-through where it shows. Every shape is a box, a surface
  of revolution or a bent tube from `MeshKit.cs`; the colours and the three small
  shaders (`Resources/Shaders`, lit, see-through, sky) are in `Look.cs`. No model
  files, no materials to keep, and no Standard shader to slow the web build down.
  The camera keeps the market in a given part of the stage (the frame) by shifting
  its projection, so the cards sit next to it; "Robot's eye" follows the chosen
  robot from behind and turns every stall beyond its sight into a grey box.
- **Since the realism pass the market is a real square** (see `Docs/realism-report.md` and
  `Docs/credits.md`): **AI Arena > Build market kit** (`Editor/MarketKit*.cs`) makes the
  prefabs in `Resources/Market` from the CC0 assets in `Assets/Environment` and the
  Higgsfield models in `Assets/Models/Hero` (the square and its houses, the stalls, every
  item's goods, the rigged agent), all URP Lit; `Market` places them from the arena and
  falls back to the code-built boxes above when the kit is missing. Run it again after
  changing a model or a texture; **AI Arena > Preview market kit / Preview hero models /
  Preview agent clips** render checks into `Builds/look-audit/realism`, and **Check market
  kit materials** is the machine check (every material URP Lit with a base and a normal map).
  The screenshot walk logs `[Coverage]`, `[UiCheck]` and `[WorldCheck]` lines to the player log.
- **Sprites are generated.** Each item, the robot and the two UI shapes are drawn
  from circles, ellipses and rounded boxes in `SpriteBaker.cs`; they are used on the
  story, the catalogue and the debrief. Change a shape, run AI Arena > Bake Sprites.
  Adding an item: one line in `Items.cs`, one entry in the baker's catalogue, one
  case in `ItemModels.cs`.
- **The family at the door.** A model at `Assets/Resources/Market/Family.prefab` (origin at its feet,
  facing +Z, about 1.6 m tall) is stood on the doormat in front of the house behind the agent's
  home, on the briefing, the story and the debriefs (`Market.FamilySpot`). Until it exists the
  doormat marks the spot.
- **Any errand can be started directly** from the title screen's strip or by clicking
  another step of the ladder on a lesson screen: for a class on one idea, or to test
  one without playing the five before it.
- **Fonts.** Inter (OFL licence, `Assets/Resources/Fonts`). TextMesh Pro font assets
  are created at runtime from the TTFs, so nothing needs re-baking when text changes.
- **LUMA's voice.** After changing any line LUMA says (the level lines in `Lesson.cs`,
  the rest in `Luma.cs`), run **AI Arena > Generate LUMA Voice**. On a Mac it records
  with the built-in Samantha voice (`say`), on Windows with Windows text-to-speech
  (`Tools/LumaVoice/generate.ps1`); the two sound different, so record every line on
  the same system. Apple licenses Samantha for personal use, so a published trailer
  needs a licensed voice. Lines built on the fly (with an agent's name in them) are
  text only. A "Voice on/off" button sits on her card in the arena. Her portrait,
  `Resources/Luma/luma.png`, is cut from Claude Bomb's footage of the same character.
- **Claude Code details.** One `claude -p` call per agent per turn with the agent's
  model and `--effort`, no tools, no saved session and none of the user's Claude Code
  settings; the remembered turns go in as a transcript. Token counts shown in the
  game are Claude Code's own `usage` numbers (estimated when it reports none).
- **Agents' settings persist** in PlayerPrefs on the machine, so a class's prompts
  survive a restart.

## Checking it without opening the Editor

Three menu items, all usable from the command line too:

- **AI Arena > Smoke Test** runs the logic without the screens (briefs, board, prompts,
  answer parsing, the stand-in brain, and one real move through Claude Code if it is
  installed).
- **AI Arena > Build macOS app** builds `Builds/mac/AI Arena.app`.
- The built app, started with `-screenshots <folder>` (and `-levels 1,5` for some errands),
  walks through every screen on the stand-in brain: the title, the four story shots, every
  errand's briefing and garage, and errands 1.1, 1.2, 1.5 and 1.6 played to the debrief. It saves
  a picture of each and `counts.jsonl` with the game-feel pass's counts (words, controls, flags,
  LUMA's lines), then quits.

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.3.9f1/Unity.app/Contents/MacOS/Unity
$UNITY -batchmode -nographics -projectPath . -executeMethod Builder.VerifyAndBuild -logFile build.log
open -W -n -a "Builds/mac/AI Arena.app" --args -screenshots /tmp/shots -screen-width 1600 -screen-height 900 -screen-fullscreen 0
```

The layout is designed on a 1920 by 1080 stage that is centred and never cropped, so
16:10 and 4:3 projectors get a margin rather than a cut-off edge.

## Web version

The browser version lives on the Maaloomatiia Games platform, this repository's site:
the menu at the root, this game at `/games/ai-arena/`, and the platform's key keeper
answering the game's Claude calls at `/api/claude` on the same address (see the README
at the repository root). The shared kit (`../../kit`, referenced from
`Packages/manifest.json`) gives the game the way back to the menu, the trip to sign in
again when the keeper refuses a session, and the Relay that reaches Claude. The page the
game loads in is the kit's (`kit/WebGLTemplates~/Maaloomatiia`), copied into
`Assets/WebGLTemplates` by every web build: the platform's cover while the game downloads,
a guard for screens too small to play on, and the way back to the menu.

**AI Arena > Build web** builds into `../../site/games/ai-arena` (gzip with Unity's
decompression fallback, so the files load from any host). From the repository root:

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.3.9f1/Unity.app/Contents/MacOS/Unity
"$UNITY" -batchmode -nographics -quit -projectPath "$PWD/games/ai-arena" -buildTarget WebGL \
  -executeMethod WebBuild.BuildFromCommandLine -logFile games/ai-arena/Logs/web-build.log
node server/local.mjs             # then open http://localhost:8080 and type the code: local
```

In a classroom the platform's local server answers the Claude calls with the Claude Code
sign-in of the computer running it; published on Netlify, the keeper holds the key.
Without Claude the game runs with the stand-in brain only. The agents' settings persist
in the browser's storage.
