# Claude Bomb

A bomb-defusal game, made for Maaloomatiia Academy, that teaches people how to work
with Claude. It's the night shift at Meridian Labs, and a device has turned up in
Bay 3. The player sees the device but can't read its manual (it's in Japanese); Claude,
the bomb squad's expert on the radio, can read the manual pages the player shares but
can't see the device. LUMA, the coach in the chat, names what the player learned along
the way.

A run opens with a title screen, a radio briefing and LUMA's guided tour, which spotlights
each part of the screen in turn; the clock starts when the tour ends. It closes with a
debrief: the result, a star rating and LUMA's recap.

Live version: https://mono-vloch.github.io/ClaudeBomb/ (ask the project owner for
the access code).

## Getting started

1. Install **Unity 6000.3.9f1** through Unity Hub, with the **Web Build Support**
   module (needed only for the web version).
2. Clone this repository and open the folder in Unity Hub (Add > Add project from disk).
   The first open takes a while: Unity rebuilds its `Library` cache.
3. Open `Assets/Scenes/Main.unity` and press Play.

**Claude in the Editor.** Two ways, checked in this order:

- **An API key:** create the file `UserSettings/claude_key.txt` containing an
  Anthropic API key on one line. `UserSettings/` is gitignored - never put a key
  anywhere under `Assets/`, because in the web build every file there can be downloaded.
- **Your Claude subscription:** with no key file, the game uses
  [Claude Code](https://claude.com/claude-code) if it is installed and signed in
  (`claude`, then `/login`). It finds the command-line install or the copy inside the
  VS Code extension. Each reply takes a few seconds longer than through the API.

With neither, the game asks for a key on start.

The Console prints every module's answer at the start of each round. That
cheat sheet exists only in the Editor; builds never contain it.

## How the game is put together

| Area | Where |
|---|---|
| Game state, timer, strikes | `Scripts/GameManager.cs` (also the module plate names), `DeviceState.cs` (random bomb each round) |
| Look and feel | `Scripts/Ui.cs` (brand colours, shapes, builders), `UiTheme.cs` + `Resources/UiTheme.asset` (fonts), `ButtonFeel.cs` |
| Opening: title, briefing | `Scripts/Opening.cs`; all new screens are added to the scene by `GameUi.cs` |
| LUMA's guided tour | `Scripts/Tour.cs` (also from the help screen: Take the tour again) |
| HUD: clock, strikes, modules | `Scripts/Hud.cs` |
| Game menu (Esc): pause, restart, new device, settings | `Scripts/GameMenu.cs`, `Settings.cs` (sound, LUMA's voice, the clock: 10, 15 minutes or none) |
| End of a run: explosion, debrief | `Scripts/LoseSequence.cs` |
| Module rules | `Scripts/Rules.cs` - must match the manual pages exactly |
| The five modules | `Scripts/ModuleA_Wires.cs` ... `ModuleE_Switches.cs` |
| Manual pages (Japanese shown, English sent to Claude) | `Resources/Papers/papers.json`, `Scripts/Paper*.cs`, `PaperDesk.cs` |
| Claude connection | `Scripts/ClaudeClient.cs`; Claude's instructions are the System Prompt on the `ClaudeClient` object in the scene |
| Chat | `Scripts/ChatPanel.cs` - player white, Claude green, LUMA blue |
| LUMA (coach) | `Scripts/LoomaCoach.cs` (what LUMA says and when), `LumaRig.cs` (3D animation), voice files in `Resources/LumaVoice` |
| Help screen | `Scripts/HelpPanel.cs` (gear icon) |
| Camera: player control, scripted glides | `Scripts/BombCameraController.cs` |
| Bay 3, the room around the device | `Resources/Lab.prefab`, built by `Editor/LabBuilder.cs` (**Claude Bomb > Build Lab**); kept alive by `Scripts/LabAtmosphere.cs` (fog, beacon, flicker, hum, reactions to the run) |

Things that are easy to break:

- **Rules and pages go together.** Change a rule in `Rules.cs` and the matching text
  in `papers.json` in the same commit, or Claude will give wrong answers.
- **Claude tags its replies.** Each reply starts with a hidden `[ACT]`, `[ASK]`,
  `[PAGE]` or `[INFO]` tag that `ChatPanel` strips and LUMA reacts to. Keep that part
  of the System Prompt if you edit it.
- **The Japanese font is pre-drawn.** `Fonts/NotoSansJP/NotoSansJP SDF.asset` only
  contains the characters the pages use today. If you add Japanese text, add its
  characters to the font asset (Font Asset Creator, or `TryAddCharacters`) or they
  show as empty boxes.
- **LUMA's voice.** After changing any line in `LoomaCoach.Lines` (the tour's lines are
  there too), run **Claude Bomb > Generate LUMA Voice**. On a Mac it records with the
  built-in Samantha voice (`say`), on Windows with Windows text-to-speech
  (`Tools/LumaVoice/generate.ps1`). The two sound different, so record every line on
  the same system; the current files were made on a Mac. To use a better voice,
  replace the `.wav` files with new ones of the same names.
- **The room is built, then saved.** `Lab.prefab` is made by **Claude Bomb > Build Lab**
  from the Poly Haven models and textures in `Environment/PolyHaven` (CC0, see the
  licence file there); the materials and maps it makes are in `Environment/Generated`.
  Change the layout in `LabBuilder.cs` and run it again, or open the prefab and move
  things by hand (a rebuild then overwrites those changes). The table top is placed at
  the device's bottom, so if the device changes size, rebuild the lab.
- **Two sets of lights.** The device is lit as before, by the scene's directional light,
  on the default rendering layer. The room is on "Light Layer 1" and lit only by its
  own lights, so the room's mood never makes the device harder to read. A new room
  object must be on that layer too (the builder does this).
- **The interface is built in code.** Screens build their own UI at runtime from
  `Ui.cs`; the chat panel, access-code panel and HUD readouts laid out in the scene
  are restyled by their scripts (`ChatPanel.Restyle`, `KeyPanel.Restyle`). Change the
  look in `Ui.cs`, not in the scene.
- **The Maaloomatiia brand.** The interface follows the Maaloomatiia brand guidelines:
  the title, briefing and debrief are dark cover and closing screens with the full
  wordmark; everything used during play is light, with the OO emblem bottom-right of
  the chat. Colours are the brand palette in `Ui.cs`; the one colour outside it is the
  red for strikes and the explosion. The logo files in `Resources/Brand` are the
  official artwork - never recolour, stretch or rebuild them. The fonts are Inter
  (`Fonts/Inter`, open font licence); the brand's SF Pro Display isn't used because
  its licence doesn't allow embedding it in a game published on the web.
- **LUMA's model is a placeholder.** `Assets/Assets/LUMA.fbx` (from Meshy) is shown
  through the reduced mesh `Assets/Models/LUMA_25k.asset`; the final textured model
  is still to come. `LumaRig` builds the skeleton in code, so a re-export of the
  same character should work after updating the mesh reference.

## The web version

The game runs in the browser (Mac and Windows). Two parts:

- **The game itself** is hosted on GitHub Pages (the `gh-pages` branch, which only
  ever holds the latest build).
- **The key keeper** is a small Netlify function (`Web/netlify/edge-functions/claude.js`)
  that holds the Anthropic key on the server and checks the players' access code.
  Its settings live in the Netlify dashboard, never in this repository:
  `ANTHROPIC_API_KEY` and `ACCESS_CODE`. Changing the key keeper needs access to the
  Netlify project (ask the owner), then `npx netlify-cli deploy --prod --dir public`
  from the `Web` folder.

To try the web version on your own computer before publishing: **Claude Bomb > Build
Web Version**, then `node Web/serve-local.mjs` and open http://localhost:8080 (access
code `local`). The local server answers through Claude Code with your subscription,
so it needs no key; the published game always uses the key keeper on Netlify.

To publish an update of the game:

1. Save the scene (Ctrl+S) - an unsaved scene makes Unity show a dialog that blocks the build.
2. **Claude Bomb > Build Web Version** (writes `Web/public`, about 10-40 minutes).
3. From Git Bash: `bash Web/publish-github-pages.sh` (needs push access to this repository).

The link updates within a minute or two; players get the new version on their next visit.
