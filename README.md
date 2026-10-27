# Claude Bomb

A bomb-defusal game that teaches people how to work with Claude. The player sees
the bomb but can't read the manual (it's in Japanese); Claude, in the chat, can read
the manual pages the player shares but can't see the bomb. LUMA, the coach in the
chat, names what the player learned along the way.

Live version: https://mono-vloch.github.io/ClaudeBomb/ (ask the project owner for
the access code).

## Getting started

1. Install **Unity 6000.3.9f1** through Unity Hub, with the **Web Build Support**
   module (needed only for the web version).
2. Clone this repository and open the folder in Unity Hub (Add > Add project from disk).
   The first open takes a while: Unity rebuilds its `Library` cache.
3. Open `Assets/Scenes/Main.unity` and press Play.

**Claude API key (Editor only).** Create the file `UserSettings/claude_key.txt`
containing an Anthropic API key on one line. `UserSettings/` is gitignored - never
put a key anywhere under `Assets/`, because in the web build every file there can
be downloaded. Without the file, the game asks for a key on start.

The Console prints every module's answer at the start of each round. That
cheat sheet exists only in the Editor; builds never contain it.

## How the game is put together

| Area | Where |
|---|---|
| Game state, timer, strikes | `Scripts/GameManager.cs`, `DeviceState.cs` (random bomb each round) |
| Module rules | `Scripts/Rules.cs` - must match the manual pages exactly |
| The five modules | `Scripts/ModuleA_Wires.cs` ... `ModuleE_Switches.cs` |
| Manual pages (Japanese shown, English sent to Claude) | `Resources/Papers/papers.json`, `Scripts/Paper*.cs`, `PaperDesk.cs` |
| Claude connection | `Scripts/ClaudeClient.cs`; Claude's instructions are the System Prompt on the `ClaudeClient` object in the scene |
| Chat | `Scripts/ChatPanel.cs` - player white, Claude green, LUMA blue |
| LUMA (coach) | `Scripts/LoomaCoach.cs` (what LUMA says and when), `LumaRig.cs` (3D animation), voice files in `Resources/LumaVoice` |
| Help screen | `Scripts/HelpPanel.cs` (gear icon) |

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
- **LUMA's voice.** After changing any line in `LoomaCoach.Lines`, run
  **Claude Bomb > Generate LUMA Voice** (Windows only: it uses Windows text-to-speech
  via `Tools/LumaVoice/generate.ps1`). To use a better voice, replace the `.wav`
  files with new ones of the same names.
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

To publish an update of the game:

1. Save the scene (Ctrl+S) - an unsaved scene makes Unity show a dialog that blocks the build.
2. **Claude Bomb > Build Web Version** (writes `Web/public`, about 10-40 minutes).
3. From Git Bash: `bash Web/publish-github-pages.sh` (needs push access to this repository).

The link updates within a minute or two; players get the new version on their next visit.
