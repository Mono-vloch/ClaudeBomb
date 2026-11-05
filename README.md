# Maaloomatiia Games

One address for the games of Maaloomatiia Academy. A student enters the class code once,
sees the menu, and picks a game. Every game is its own Unity project, built into a folder
of this site; one key keeper serves them all, so a game never holds a key or a code.

| Game | Topic | Where |
|---|---|---|
| 01 Claude Bomb | Working with Claude | `games/claude-bomb/`, served at `/games/claude-bomb/` |
| 02 AI Arena | Building an AI agent | `games/ai-arena/`, served at `/games/ai-arena/` |
| 03 Robot Racing | How a machine learns from reward | `games/robot-racing/`, served at `/games/robot-racing/` |
| 04 Tech Escape | Defending a real system | `games/tech-escape/`, served at `/games/tech-escape/` |

## Playing it on this computer

```bash
node server/local.mjs        # then open http://localhost:8080 and type the code: local
```

That server does everything the published site does: the menu (`site/`), the built games
(`site/games/<id>/`), the sign-in and the Claude calls, which it answers through
[Claude Code](https://claude.com/claude-code) with the Claude subscription it is signed in
with (`claude`, then `/login`). No key is needed anywhere. `HOST=0.0.0.0 node server/local.mjs`
lets other computers in the room open it.

To check the pages without opening a browser:
`node build/snap.mjs http://localhost:8080 /tmp/shots local claude-bomb` walks through the
door, the menu and a game in a headless Chrome and saves a picture of each step.

## How it is put together

```
site/        the menu: index.html, styles.css, app.js, games.json (the catalogue), covers/, brand/
site/games/  one folder per built game (not committed; built in place, see below)
server/      the key keeper: Netlify edge functions (session, me, claude, progress) and
             local.mjs, the classroom twin; lib/ is what both share
kit/         a Unity package every game uses: Platform (where the menu is, how to go back,
             how to sign in again), Relay (Claude through the keeper) and the web page every
             game loads in (WebGLTemplates~/Maaloomatiia, copied in by each web build)
games/       the Unity projects, one folder each
build/       helper scripts
```

**Sign-in.** The menu posts the code to `/api/session`. A good code gets a signed cookie
(`server/lib/session.js`, 12 hours). Because every game is served from the same site, the
browser sends that cookie with the game's calls to `/api/claude` by itself; a game never
handles codes. If the keeper answers 401, the game sends the player back to the menu with
`?signin=1&return=/games/<id>/`, and after the code the menu returns them to the game.

Today there is one class code (`ACCESS_CODE`). Per-student codes are the next step: the
subject in the cookie becomes the learner's id, and nothing in the games changes. The plan is
to reuse Academy OS's learner codes (`MA-XXXXX-XXXXX`), which `session.js` already reads
the same way (upper case, O to 0, I to 1).

**The keeper.** `/api/claude` checks the cookie, allow-lists the model and effort a game may
ask for (`server/lib/validate.js`), adds the Anthropic key and forwards the conversation.
Claude Bomb sends `{system, messages}` and gets the defaults; AI Arena sends a model and an
effort per agent. The answer goes back in the Messages API's shape.

**The kit** (`kit/`) is referenced from each game's `Packages/manifest.json` as
`"com.maaloomatiia.games-kit": "file:../../../kit"`. `Platform.GoToMenu()` leaves the game
in the same tab; `Platform.SignInAgain()` is what a 401 leads to; `Relay.Send(...)` is the
Claude call; `Platform.ReportProgress(...)` tells the platform how a run ended. The kit's
`WebTemplate.Install()`, called by each game's build script, copies the shared web page
into `Assets/WebGLTemplates/Maaloomatiia`: the platform's cover while the game downloads,
a guard for phones and upright tablets (every game is a 16:9 stage), and the way back to
the menu. Edit the page in the kit, never in a game.

## Building a game into the site

Each game builds itself into `site/games/<id>/` with its own menu item or from the command
line. Claude Bomb:

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.3.9f1/Unity.app/Contents/MacOS/Unity
"$UNITY" -batchmode -nographics -quit -projectPath "$PWD/games/claude-bomb" -buildTarget WebGL \
  -executeMethod WebBuild.BuildFromCommandLine -logFile games/claude-bomb/Logs/web-build.log
```

(or, in the Editor, **Claude Bomb > Build Web Version**). AI Arena builds the same way, with
`-projectPath "$PWD/games/ai-arena"` and its log in `games/ai-arena/Logs/` (or, in the Editor,
**AI Arena > Build web**). Builds take 10 to 40 minutes. Without Unity installed,
`docker compose run --rm unity build` does the same in a container, `GAME=ai-arena` for the
other game (see `compose.yaml`).

## AI Arena

Wired to the platform the same way as Claude Bomb: the kit in `Packages/manifest.json`,
`Scripts/Claude/ClaudeProxy.cs` through the kit's Relay (a 401 from the keeper sends the
player to sign in again, and the menu brings them back), a "Games menu" button on the title
and debrief screens, a progress report when the robots are home, and `Editor/Builder.cs`
building into `site/games/ai-arena`. Its old `Web/serve-local.mjs` is gone: `server/local.mjs`
answers the same `{model, effort, system, messages}` request with the same reply shape.

## Robot Racing and Tech Escape

Two more games on the same kit, each its own Unity project (`games/robot-racing`,
`games/tech-escape`) with its own README. Neither calls Claude: the robot learns on the
machine and the attacks are written into the rooms, so they need no key keeper, only the
sign-in, the "Games menu" button and the progress report (`Platform.ReportProgress`, in the
curriculum's form: `2.3:complete`, `:partial` or `:incomplete`). Their `Editor/Builder.cs`
builds into `site/games/robot-racing` and `site/games/tech-escape`
(`-executeMethod Builder.BuildWeb -buildTarget WebGL`, or **Robot Racing > Build web** and
**Tech Escape > Build web** in the Editor). The three Unity games share their world and UI
code by copy for now (`MeshKit`, `Look`, `SceneCamera`, `UiKit`, `Theme`, `Luma`); moving
that into the kit is the next step once the games settle.

## Publishing

The site, the built games and the keeper go to one Netlify site, from the repository root:

```bash
npx netlify-cli deploy          # a preview link to try first
npx netlify-cli deploy --prod   # the live address
```

Three settings live in the Netlify dashboard (Site configuration > Environment variables),
never in these files: `ANTHROPIC_API_KEY` (the key the keeper uses), `ACCESS_CODE` (the
class code) and `SESSION_SECRET` (a long random string that signs the cookies, for example
`openssl rand -hex 32`). The deploy uploads only files that changed, so re-publishing after
one game's rebuild is quick. Claude Bomb's old address on GitHub Pages
(`mono-vloch.github.io/ClaudeBomb`) should get a one-line page that redirects to the new
site once the platform is live.

## What comes next

- Mini-games: one light Unity project (`games/mini`), a scene per mini-game, opened from the
  menu with `?game=<id>`.
- Student codes: an export of Academy OS learner codes into the keeper, a per-student usage
  log and daily cap, and progress on the menu (private to the student, no leaderboard).
- Robot Racing and Tech Escape, started from the kit.
