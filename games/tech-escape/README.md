# Tech Escape: Nothing Gets Through

Game 04 of the Maaloomatiia Academy series. Topic: defending a real system.

The story: the Harbor Permits Office, a public service that issues permits, is in
lockdown. Every room is a system that is genuinely misconfigured (the register with an
asset nobody listed, the inbox with the message that looked right, the leaver who kept
admin, the patch order by score alone, the backups on the same keys) and the student has
to fix it. Then the facility runs the attack against whatever they built and shows what
got through. What got through goes on the facility's record, and the exit stays locked
until the student can reconstruct the whole chain. Nineteen levels in twelve rooms, with
LUMA, the coach from Claude Bomb. The content follows `Docs/curriculum/tech-escape.md` at
the root of the repository, level by level.

## Getting started

1. Install **Unity 6000.3.9f1** through Unity Hub.
2. Open this folder in Unity Hub (Add > Add project from disk). On the first open the
   project sets itself up: it imports the TextMesh Pro resources, bakes the sprites into
   `Assets/Resources/Sprites` and creates `Assets/Scenes/Main.unity`.
3. Open `Assets/Scenes/Main.unity` and press Play.

No Claude, no key, no network: the attacks are written into the rooms.

## The ladder

Every level is a question and gives two training runs before the student's own: **Watch the pro** (the room as it
should be, read only; lift the lockdown and watch it hold), **Fix the broken room** (the room
with the level's planned mistake in it; the attack finds it) and **Your run**, the one that
counts and goes on the record.

| Level | Room | Question | You fix | Fix it: the planned mistake |
|---|---|---|---|---|
| 1.1 | Risk Room | What is worth protecting? | the asset register | An asset nobody listed |
| 1.2 | Risk Room | How likely, how bad? | the risk matrix and one control per high risk | Everything rated Medium |
| 1.3 | Suspicious Inbox | Which of these is an attack? | the inbox sort | The one that looked right |
| 1.4 | Password Vault | Who can get in? | the account check | MFA for everyone but one |
| 1.5 | Malware Containment | Capstone: Report it | the report, and three priority actions | (none) |
| 2.1 | Identity Chamber | Who should still have access? | the access list | The leaver with admin |
| 2.2 | Patch Laboratory | Which hole first? | the patch order | Severity score alone |
| 2.3 | Network Maze | What can reach what? | the network zones | One flat network |
| 2.4 | Firewall Control | What is allowed through? | the rules | Allow everything, at the bottom |
| 2.5 | Encryption Chamber | Who can read it if they take it? | what is encrypted, and where the key is | The key next to the data |
| 2.6 | Cloud Chamber | What did the defaults leave open? | the cloud settings | Public by default |
| 2.7 | Operations Center | Is this an event or an incident? | the logs and the alerts | The log that was never kept |
| 2.8 | Malware Containment | How do you stop it without losing the evidence? | the containment order | Delete first |
| 2.9 | Backup Vault | Can you get it back? | where the backups live and who can reach them | Backups on the same keys |
| 2.10 | Risk Room | Capstone: The register and the evidence | the whole facility | (none) |
| 3.1 | Operations Center | Is it an incident, and how bad? | the declaration, the roles | The commander who starts typing |
| 3.2 | Password Vault | What do you cut first? | the containment steps | Rotated, and stopped there |
| 3.3 | Network Maze | How did they get from the inbox to the vault? | the chain | The chain that stops at the person |
| 3.4 | Operations Center | Capstone: What changes, who, by when? | the review | (none) |

The highest level finished is remembered on the machine, with every room as the student
left it, the record, and the stars each level earned (gold, silver or bronze from the
checks; for motivation only). Any level can be started from the title screen's Levels map,
so a tester or a class can go straight to one room.

## How a run goes

1. **Briefing.** The intruder alarm: the corridor from the ceiling camera, this level's door
   lit amber, three short lines of what is happening tonight and what the player is there to
   do, and "Enter the room".
2. **The room.** The room through its security camera, full screen. Every row is a card on
   the room's prop with a knob per decision and a lamp, teal once every decision on it is made
   and amber while one is open; tap a card to bring it close (one row per decision with its value) and tap
   its rows to turn them. Where something nobody listed is to be found (1.1), it waits in a
   corner of the room: turn the camera (drag across the room, or the arrow keys), it glints as
   the camera passes it, and a tap flies it to the board. A record that must be signed has a
   pad. One objective line at the top, a lamp per row beside it, the attacker on the wall
   monitor trying the door. Then: lift the lockdown.
3. **The attack.** The camera flies in over the entrance and chases the attacker door to door.
   A door that gives way goes scarlet, and the attacker comes back out with the drive and runs
   off with it; one that holds flashes teal, and he tries the handle, gives up and walks away.
   The twelve door lamps at the top. The first attack on an untouched room is a rehearsal and
   goes back to the room.
4. **Debrief.** The corridor outside the room, its door shut or broken, the medal rising on a
   step, LUMA saying the level's motto; Next and Again. Behind Details: what got through and what
   held, the checks, the stars, the question to take back to work, the file, and on a capstone
   the two if-then plans. The platform is told the result in the curriculum's form
   (`2.2:complete`, `:partial` or `:incomplete`, with the checks).

## Where things are

| Part | File |
|---|---|
| The case: the service, the people, the twelve rooms; a room's state, an attack, a session | `Scripts/Core/Facility.cs` |
| The nineteen levels as data: items, decisions, the ready and broken rooms, warm-ups, the attack each room runs, the checks, the lines | `Scripts/Core/Rooms.cs` |
| LUMA: the coach, her lines, portrait and voice | `Scripts/Ui/Luma.cs`, `Resources/Luma`, `Resources/LumaVoice`, `Editor/LumaVoiceTools.cs` |
| The campaign, the session on disk, the step (watch, fix, your run), the record | `Scripts/Ui/App.cs` |
| Screens | `Scripts/Ui/TitleScreen.cs` (the cover, Start and the facility map), `LessonScreen.cs` (the briefing as the night shift's transmission), `RoomScreen.cs` (the room: its rows as things on the prop, `World/RoomRig.cs`), `AttackScreen.cs` (the attack, with the rehearsal), `ResultsScreen.cs` (the debrief: the door, the podium, Details), `GameMenu.cs` (Esc), `Hud.cs` |
| What the game-feel pass says: the objective lines, the transmissions, the short names on the plates | `Scripts/Core/Script.cs` |
| The facility in three dimensions: the corridor, twelve rooms, the door lights, the attacker, the exit | `Scripts/World/Building.cs` |
| The camera: overview, orbit, the room from the corridor side | `Scripts/World/SceneCamera.cs` |
| Shapes and materials, made in code; the three shaders | `Scripts/World/MeshKit.cs`, `Look.cs`, `Resources/Shaders` |
| Look and feel, from the Maaloomatiia brand guidelines | `Scripts/Ui/Theme.cs`, `UiKit.cs`, `Sfx.cs` |

Notes:

- **Everything is built in code.** The scene holds a camera and one `App` object;
  `App` builds the canvas and the screens at runtime. There are no prefabs.
- **A room is data.** Its rows and decisions, the ready room, the broken room and the
  warm-up are tables in `Rooms.cs`; its attack and its checks are two short functions
  next to them, so a level is read top to bottom. Adding a room is adding one entry.
- **The record is the spine.** Each tier 2 room writes one line to the record (held, or
  the gap and what the attacker reached); the exit in tier 3 asks for the chain.
- **The world is three-dimensional, and also built in code**, the way AI Arena's market
  is: shapes from `MeshKit.cs`, colours and three small shaders in `Look.cs`, a camera
  that keeps the facility in a given part of the stage so the cards sit next to it.
- **LUMA's voice.** After changing any line she says, run **Tech Escape > Generate LUMA
  Voice** (macOS `say`; a published trailer needs a licensed voice): the build stops when a
  line has no recording. Since the game-feel pass she says one short cue when something
  happens (`Luma.Lines`: her first sentence on entering a room, the attack's line, the
  takeaway); her full lines and the curriculum are in the file. The walk below is silent.
- **The game-feel pass (8 November 2026).** Every row of a room is a card on the room's prop
  (`World/RoomRig.cs`), its short name on a dark plate, a knob per decision and a lamp: teal
  once every decision on the row is made, amber while one is still a question mark; whether it
  was right, only the attack shows. The knobs show no values at rest, only the one just turned. Tap a card to bring it close: a flat card faces the
  camera, the full name and line at the top, one row per decision ("Owner: Omar") with a light
  knob; tap a row to turn it. Drag a module up or down a rack. On 1.1 the unlisted drive waits
  on a crate out of the opening view: the camera turns (`SceneCamera.lookYaw`/`lookPitch`), the
  drive glints as it passes, a tap flies it to the board. The corridor monitor on the wall
  shows the attacker coming to the door, where it tries the handle until the player lifts the
  lockdown (in the Operations Center the feed is on one of the room's own screens). The attack
  ends at the door: through it, the attacker runs off with the drive (`Building.Grab`); held,
  he gives up and walks away (`Building.GiveUp`). The first attack on an untouched room is a
  rehearsal and counts for nothing. 2.1's accounts and 3.1's roles carry the staff badges
  (`Resources/Badges/`, cropped from Higgsfield's printed sheet); 2.4's rack of rules stands on
  the patch panel (`Models/Hero/PatchPanel/`, split by `Tools/glb_maps.py`, built by
  `Tech Escape > Build patch panel`). The objective lines, the alarms before each room and the
  short names are in `Core/Script.cs`; the data in `Rooms.cs`, the checks and the report are
  as they were.

## Checking it without opening the Editor

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.3.9f1/Unity.app/Contents/MacOS/Unity
$UNITY -batchmode -nographics -projectPath . -executeMethod Builder.BuildMac -logFile Logs/build.log
"Builds/mac/Tech Escape.app/Contents/MacOS/Tech Escape" -screenshots /tmp/shots -screen-width 1600 -screen-height 900 -screen-fullscreen 0
"Builds/mac/Tech Escape.app/Contents/MacOS/Tech Escape" -cover /tmp/cover.png -screen-width 1600 -screen-height 1000 -screen-fullscreen 0
```

The built app, started with `-screenshots <folder>`, walks through every level (the briefing,
the room as handed over, the room set right; 1.1, 1.3 and 2.4 played through the rehearsal,
the attack and the debrief), saves a picture of each screen with the game-feel pass's counts
in `counts.jsonl` (words, controls, LUMA's lines), then quits; `-levels 1,9` limits it to
those levels; `-cover` takes the platform's cover picture. Grep the build log for `error CS`
and `Build:`. The latest frames are in `Builds/look-audit/game-feel/`.

## Web version

**Tech Escape > Build web** (or `-executeMethod Builder.BuildWeb -buildTarget WebGL` from the
command line) builds into `site/games/tech-escape` at the root of the repository, where the
Maaloomatiia Games menu launches it; the platform's page comes from the kit. See the root README.
