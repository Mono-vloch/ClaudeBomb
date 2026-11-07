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
should be, read only; run the attack and watch it hold), **Fix the broken room** (the room
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
checks; for motivation only). Any level can be started from the title screen's track or
from the ladder on a briefing, so a tester or a class can go straight to one room.

## How a run goes

1. **Briefing.** LUMA opens the level: the idea in plain words,
   what the student fixes, and the planned mistake if the level has one. The facility is
   listed room by room on the right with what the record says about each: held, a gap, or
   not yet.
2. **The room.** The room in three dimensions on the left, its door open; on the right the
   system as it was handed over, row by row (assets, accounts, messages, rules, steps), each
   with its decisions as chips that cycle through their options. Rows that must be put in
   order have arrows. A thing nobody listed can be added. A record that must be signed has a
   signature line, with the date. Then: run the attack.
3. **The attack.** The corridor from above, with the attacker walking to every door the
   attack tries; line by line on the right, what it tried and whether the door held or gave.
   Banners mark each door. On the last level the exit opens when nothing got through.
4. **Debrief.** What got through and what held, the level's checks, a medal, the facility's
   record so far (the spine the exit asks for), LUMA's takeaway and the question to take back
   to work, the next level's unlock, and on a capstone the two if-then plans. The platform is
   told the result in the curriculum's form (`2.2:complete`, `:partial` or `:incomplete`, with
   the checks).

## Where things are

| Part | File |
|---|---|
| The case: the service, the people, the twelve rooms; a room's state, an attack, a session | `Scripts/Core/Facility.cs` |
| The nineteen levels as data: items, decisions, the ready and broken rooms, warm-ups, the attack each room runs, the checks, the lines | `Scripts/Core/Rooms.cs` |
| LUMA: the coach, her lines, portrait and voice | `Scripts/Ui/Luma.cs`, `Resources/Luma`, `Resources/LumaVoice`, `Editor/LumaVoiceTools.cs` |
| The campaign, the session on disk, the step (watch, fix, your run), the record | `Scripts/Ui/App.cs` |
| Screens | `Scripts/Ui/TitleScreen.cs`, `LessonScreen.cs` (the briefing; its warm-up question is turned off), `RoomScreen.cs` (the room's decisions), `AttackScreen.cs` (the attack), `ResultsScreen.cs` (the debrief) |
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
  Voice** (macOS `say`; a published trailer needs a licensed voice). The walk below is
  silent.
- **Rooms and progress persist** in PlayerPrefs on the machine.

## Checking it without opening the Editor

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.3.9f1/Unity.app/Contents/MacOS/Unity
$UNITY -batchmode -nographics -projectPath . -executeMethod Builder.BuildMac -logFile Logs/build.log
"Builds/mac/Tech Escape.app/Contents/MacOS/Tech Escape" -screenshots /tmp/shots -screen-width 1600 -screen-height 900 -screen-fullscreen 0
"Builds/mac/Tech Escape.app/Contents/MacOS/Tech Escape" -cover /tmp/cover.png -screen-width 1600 -screen-height 1000 -screen-fullscreen 0
```

The built app, started with `-screenshots <folder>`, walks through every screen (title,
briefing, a room, the attack, the debrief, a room with rows to order) and saves a
picture of each, then quits; `-cover` takes the platform's cover picture. The current set is in
`Docs/screenshots`. Grep the build log for `error CS` and `Build:`.

## Web version

**Tech Escape > Build web** (or `-executeMethod Builder.BuildWeb -buildTarget WebGL` from the
command line) builds into `site/games/tech-escape` at the root of the repository, where the
Maaloomatiia Games menu launches it; the platform's page comes from the kit. See the root README.
