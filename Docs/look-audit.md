# Look audit: AI Arena, Robot Racing, Tech Escape against Claude Bomb

Phase 1 of the look-and-feel pass, 5 November 2026, branch `look-and-feel`. Nothing in the
games was changed for this audit. Every screen of the three games was captured by the built
Mac app's own walk (`-screenshots`, 1600 by 900) into `games/<id>/Builds/look-audit/before/`;
Claude Bomb, the reference, was captured from its web build on the platform in headless Chrome
into `games/claude-bomb/Builds/look-audit/before/` (it has no screenshot walk and no Mac build).
These folders are ignored by git.

## How the screens were graded

The six axes of the Unity craft scorecard (`craft-game-floors/visual-scorecard`), 1 to 5 each:

| Axis | The question |
|---|---|
| Palette | Does every colour trace to an art bible? (instant fail: default grey material, sky or UI) |
| Style | One style across character, world, sky and UI? |
| Lighting | Warm/cool separation, shadow sides alive, depth? (flat white directional = 1) |
| Silhouette | Is the character identifiable filled black, from the gameplay camera? |
| UI | HUD on the palette, aligned, styled text, no default uGUI look? |
| Feedback | Does the busy frame show the game answering the player? |

Calibration: 5 is a promo still from a stylised indie, 3 is a polished jam entry (the floor any
build should ship at), 1 is untextured primitives under one white light with default-grey UI.
A screen that hides the world entirely (a white page with cards) scores 1 on Lighting and
Silhouette: there is no light and no character in the frame, which is exactly the problem.

Machine checks ran first, so the palette axis does not rest on eyesight: every material in the
three games is made in code from `Theme.cs` (the Maaloomatiia brand palette) and `Look.cs`
through one custom `Maaloomatiia/Flat` Lambert shader; there is no `Default-Material`, no
Standard shader and no default skybox. The gap is elsewhere: the world's colours are the brand's
UI neutrals (`#E9EDF0`, `#F5F7F8`, `#9AA5B1`) applied to boxes, so the 3D world reads as default
grey even though it is not. `Color.white`-style literals outside `Theme`/`Look`: 7 in Robot
Racing, 13 in AI Arena, 7 in Tech Escape (mostly white text and clear).

## What the three games have today (identical structure)

- Built-in Render Pipeline, one directional light (soft shadows, strength 0.5 to 0.55), trilight
  ambient and linear fog set in code, a gradient-sky shader. No post-processing of any kind: no
  tonemapping, bloom, vignette, colour grading or depth of field. No reflection probes.
- A hand-rolled camera (`SceneCamera.cs` / `MarketCamera.cs`: SmoothDamp position, FOV lerp,
  off-centre projection so cards can sit next to the world). No Cinemachine, no shake, no kicks.
- No particles, trails or line effects anywhere.
- Sound: four synthesised tones (tick, buy, ruin, whoosh) and LUMA's recorded voice. No engine,
  ambient bed, music, stings or impact sounds.
- uGUI + TextMeshPro, font assets created at runtime from Inter TTFs with default SDF settings
  (readable, not tuned), a 1920 by 1080 reference canvas that scales with the screen. Buttons use
  the baked 12 px rounded sprite; medals ("BRONZE", "GOLD") are pills; level nodes are circles.
- Claude Bomb, for comparison: URP 17.3.0 with HDR, a global Volume (tonemapping, bloom,
  vignette, colour adjustments), Cinemachine 2.10.5 cameras and scripted glides, particles (dust
  in the beam, the explosion), a lab built from Poly Haven models with a reflection probe, a
  recorded sound set, and the device as a real prop with lamps.

## Scores

### Claude Bomb (the reference)

Six frames of the title screen's camera orbit over 120 s (`t003` to `t120.png`); the headless
click did not reach "Begin", so the play screens (HUD, chat, tour) are not in this set and the
platform screenshot `Docs/screenshots/4-claude-bomb-on-the-platform.png` stands in for them.
The title alone is enough to calibrate against:

| Screen | Palette | Style | Lighting | Silhouette | UI | Feedback | Mean |
|---|---|---|---|---|---|---|---|
| title, orbit (6 frames) | 4 | 4 | 4 | 4 | 4 | 3 | 3.8 |

Why it scores: the lab is one cool blue with warm lamp accents and yellow hazard stripes, every
colour a choice; the device, the table, the props and the "REC CAM 02 / BAY 3" frame are one
style; a cool key with warm lamps, fog and dust in the beam, red module lamps that glow, shadow
sides that stay blue; the device reads as a device from any angle (handle, modules, yellow
corners); the cover panel is the brand's dark page with the wordmark. Feedback is a 3 on a
title: the orbit, the running clock and the blinking lamps are the only life. The two buttons
are pills, which the house rule for the three games forbids; Claude Bomb itself is not touched.

### Robot Racing (8 screens, mean 2.0)

| Screen | Palette | Style | Lighting | Silhouette | UI | Feedback | Mean |
|---|---|---|---|---|---|---|---|
| 1-title | 3 | 3 | 2 | 2 | 4 | 2 | 2.7 |
| 2-briefing | 3 | 2 | 1 | 1 | 3 | 1 | 1.8 |
| 2b-warm-up | 3 | 2 | 1 | 1 | 3 | 1 | 1.8 |
| 3-pit-lane | 2 | 2 | 2 | 2 | 3 | 2 | 2.2 |
| 4-training-slow | 3 | 2 | 2 | 2 | 2 | 2 | 2.2 |
| 5-race | 3 | 2 | 2 | 2 | 2 | 3 | 2.3 |
| 6-debrief-questions | 3 | 1 | 1 | 1 | 2 | 1 | 1.5 |
| 7-debrief | 3 | 2 | 1 | 1 | 2 | 2 | 1.8 |

The five worst, `games/robot-racing/Builds/look-audit/before/`:

1. **6-debrief-questions.png (1.5)**: seven rows of five identical outlined buttons on a white
   page. No world, no robot, no result on screen; the quiz reads as a worksheet.
2. **2-briefing.png (1.8)**: a white page with a heading, LUMA's card, body text and a card
   list. The track is a 7-cell pixel diagram in a box. The 3D circuit built for this very level
   is nowhere in the frame.
3. **2b-warm-up.png (1.8)**: a modal dialogue of three outlined buttons over the dimmed page.
4. **7-debrief.png (1.8)**: a chart, a checklist, a "BRONZE" pill and four buttons on white;
   two thirds of the page is empty. The medal is the level's reward and it is an 11 px pill.
5. **3-pit-lane.png (2.2)**: the one screen with the robot big in frame, and it is a teal box
   with a black lid on grey blocks under flat daylight; the settings panel is a white form over
   60 % of the world. The circuit's blocks are mid-grey on mid-grey.

Also: 4-training and 5-race put the world in a top-down quarter of the screen with the rest
text panels; the crash, the first finish and the moment it settles (the three things the level
is about) leave no visible trace; "THE RACE" is a white box with text.

### AI Arena (12 screens, mean 2.4)

| Screen | Palette | Style | Lighting | Silhouette | UI | Feedback | Mean |
|---|---|---|---|---|---|---|---|
| 1-title | 4 | 3 | 3 | 3 | 4 | 2 | 3.2 |
| 2-story | 4 | 3 | 1 | 3 | 3 | 2 | 2.7 |
| 2b-story-market | 4 | 3 | 1 | 3 | 3 | 3 | 2.8 |
| 2c-story-tomato | 4 | 3 | 1 | 2 | 2 | 1 | 2.2 |
| 3-lesson | 3 | 2 | 1 | 1 | 3 | 1 | 1.8 |
| 3b-warm-up | 3 | 2 | 1 | 1 | 3 | 1 | 1.8 |
| 4-tour | 3 | 2 | 2 | 3 | 3 | 2 | 2.5 |
| 4b-decide | 3 | 1 | 1 | 1 | 2 | 1 | 1.5 |
| 5-setup | 3 | 3 | 2 | 3 | 3 | 2 | 2.7 |
| 6-market | 3 | 3 | 2 | 2 | 3 | 4 | 2.8 |
| 6b-agents-eye | 3 | 3 | 2 | 3 | 3 | 4 | 3.0 |
| 7-debrief | 3 | 2 | 1 | 1 | 3 | 2 | 2.0 |

The five worst, `games/ai-arena/Builds/look-audit/before/`:

1. **4b-decide.png (1.5)**: errand 1.6 is five rows of three outlined buttons on a white page,
   with the title's button the only colour. No market, no agent.
2. **3-lesson.png (1.8)**: the mission briefing is a white page: heading, LUMA card, body text,
   the message in a box, a row of item sprites, the dials as cards. The market is hidden.
3. **3b-warm-up.png (1.8)**: the modal quiz over the dimmed page.
4. **7-debrief.png (2.0)**: a ranking as four white cards with sprite baskets and "GOLD" pills,
   the lower third empty. The ranking is the game's payoff and it reads as a table.
5. **2c-story-tomato.png (2.2)**: the story beat's cards draw their heading over their body text
   (a layout bug: "What does an agent do on its own?" and "It shops alone. You cannot step in."
   overlap), and LUMA's line is cut mid-word at capture time.

Also: the title and the market are the two best screens of all three games (the dusk market
with its stall lamps, the "SENT HOME · Lina" banner with the agent's sight square), but the
banner is a white box, the agents are small boxes at the gameplay camera, and the world is lit
flat with weak shadows.

### Tech Escape (7 screens, mean 1.9)

| Screen | Palette | Style | Lighting | Silhouette | UI | Feedback | Mean |
|---|---|---|---|---|---|---|---|
| 1-title | 4 | 3 | 3 | 2 | 4 | 2 | 3.0 |
| 2-briefing | 3 | 2 | 1 | 1 | 3 | 1 | 1.8 |
| 2b-warm-up | 3 | 2 | 1 | 1 | 3 | 1 | 1.8 |
| 3-room | 2 | 2 | 2 | 1 | 3 | 1 | 1.8 |
| 4-attack | 2 | 2 | 2 | 1 | 2 | 2 | 1.8 |
| 5-debrief | 3 | 2 | 1 | 1 | 2 | 1 | 1.7 |
| 6-room-ordered | 2 | 2 | 2 | 1 | 3 | 1 | 1.8 |

The five worst, `games/tech-escape/Builds/look-audit/before/`:

1. **5-debrief.png (1.7)**: four white cards on a white page, the lower half empty. What got
   through (the attack's one line) is a red word in a list.
2. **4-attack.png (1.8)**: the facility seen from above on a flat grey-blue background, the
   attacker a 20 px purple stick; the right half is a near-empty panel with one line. This is
   the game's climax ("the facility runs the attack") and nothing happens in the frame: no alarm,
   no lights, no door giving way.
3. **3-room.png (1.8)**: the room's misconfiguration is a list of chips in a white form over a
   grey world; the room itself (desk, rack, monitor) is grey boxes under flat daylight with no
   character and nothing that says "inbox".
4. **2-briefing.png (1.8)**: the white page again, with the facility as a list of twelve rows.
5. **2b-warm-up.png (1.8)**: the modal quiz.

Also: the title is good (the night facility with red door lights and blue ambient) and then the
game abandons that mood: every later screen is daylight grey. "Lockdown" is only on the title.

## The fixes, ranked by impact

1. **Keep the world on screen on every screen.** The briefing, warm-up, decide and debrief screens
   are the eleven worst frames across the three games (all at 1.5 to 2.0), and they are worst
   because they hide the world. Put the briefing over the live scene with depth of field, the
   warm-up as a card in the world, the debrief as a podium / medal moment in the scene with the
   result in big type. This one change moves eleven screens from "worksheet" to "game" and is
   what Claude Bomb does with its dark cover screens over the lab.
2. **Move to URP with post-processing and an art bible per game.** Tonemapping, a little bloom on
   the accents, a vignette, colour grading and depth of field behind menus: +1 to +2 on Lighting
   and Palette on every 3D frame. The bible gives each world its own palette instead of the UI's
   neutral ramp on boxes (Robot Racing: a circuit at golden hour; AI Arena: the dusk market the
   title already has; Tech Escape: the lockdown night the title already has).
3. **Light the scenes.** A warm key at 35 to 50 degrees with real shadows (strength 0.8 and up,
   not 0.55), a cool sky in the ambient, fog tuned to the camera distance, a reflection probe on
   the metal and glass. Today every scene is a flat 0.7-intensity light with half-strength
   shadows, which is why the boxes look like boxes.
4. **Make the big moments moments.** A banner is flash, scale-in with overshoot, a sting, confetti
   or sparks, then a hold: the first finish, the crash, "THE RACE", the medal, "SENT HOME",
   "ALL HOME", "THROUGH" / "HELD" per door, the exit opening. Today a banner is a white box.
5. **Cameras.** Cinemachine (2.10.5, as in Claude Bomb): a chase camera on the agent's car with
   damping and a speed FOV kick, impulse shake on crashes, a slow-motion finish; in AI Arena a
   camera that follows the chosen agent stall by stall; in Tech Escape the attacker's walk door
   to door and the room from inside. Today the camera is a SmoothDamp to fixed overview points.
6. **Give the characters a silhouette.** The robot car is a box with a dome, the agent a box with
   a box head, the attacker a stick: exaggerate the identifying parts 30 %, taper and tilt,
   two tones plus dark accents, an idle bob, a glow on the antenna / visor. Score the silhouette
   from the gameplay camera, where the agents are 30 px tall today.
7. **A mission HUD instead of text panels.** Objective, attempt count, reward score, best time,
   level and stars in the corners, in big type on dark glass; the rule book and the sensors as a
   collapsible panel, not the default view. Baked Inter SDF atlases (crisp small text on the
   web), 12 px rounded rectangles everywhere (the medal pills and the circle level nodes go),
   phone and desktop scaling checked.
8. **VFX.** Sparks and a short impact pause on a crash, tyre trails at speed, dust under the
   robot, confetti on a medal, ghost lines of earlier attempts on the track (the learning made
   visible), the attacker's trail and an alarm strobe in Tech Escape, a glow on what an agent
   can see in AI Arena.
9. **Sound.** Engine pitch following speed, crash, reward chime, finish sting, an ambient bed per
   world, an alarm, a door, a UI click set; duck LUMA's voice under it.
10. **Web budget.** Keep the gzip builds under about 25 MB and the load under what they are
    today; URP and the Volume add shader variants, so strip the unused ones (`optimize-web`).

## Web baseline (before)

Download size is the gzip build on disk under `site/games/<id>/Build`; load time is from
navigation to the loader hiding, in headless Chrome with software rendering on this Mac, cache
off (slower than a laptop with a GPU, but the same yardstick before and after).

| Game | Build on disk | Transferred | Load time |
|---|---|---|---|
| Robot Racing | 17.9 MB (gzip) | 17.6 MB | 3.1 s |
| AI Arena | 17.6 MB (gzip) | 17.3 MB | 2.3 s |
| Tech Escape | 19.9 MB (gzip) | 19.5 MB | 2.6 s |
| Claude Bomb (reference) | 60.2 MB (brotli) | 58.9 MB | 6.2 s |

Each of the three is one 8 MB data file and one 10 MB wasm file. Claude Bomb's size is its
Poly Haven models and textures; the three games have no model files at all, so URP, the Volume
and Cinemachine should add a few MB of code and shaders, not tens.

## Claude Bomb anchor frames

`games/claude-bomb/Builds/look-audit/before/`: `0-loaded.png` (the page as the loader hides),
`t003` to `t120.png` (the title's orbit at 3, 8, 15, 25, 40, 55, 75, 95 and 120 s),
`click-centre-a/b.png` (after a click in the middle of the device: unchanged). The frames to
hold the three games against: `t003.png` (the device face-on with the bay sign and the desk),
`t075.png` (the warm lamp and the crate behind the device), `click-centre-b.png` (the door,
the extinguisher, the hazard drum: the room has props scaled to the device).

---

# Phase 2: the Robot Racing pilot

5 November 2026, branch `look-and-feel`. Every step was checked in a live Editor (Play mode
captures through the Unity CLI) before the next one, then the Mac build's own walk made the
"after" set in `games/robot-racing/Builds/look-audit/after/` and the web build was measured on
the platform. The teaching did not move: LUMA's lines, the curriculum text and the exact words
the robot reads are the strings they were.

## What changed, step by step

1. **Art bible**: `games/robot-racing/Docs/art-bible.md`. A toy-circuit diorama under a late-
   afternoon sun; the world's palette (slate asphalt, chalk lines, meadow and moss, warm concrete
   barriers, limestone stand, the brand's aqua for the agent); the dusk variant for the title.
2. **URP 17.3.0**, the version Claude Bomb uses. `Assets/Editor/RenderSetup.cs` makes and
   assigns `Assets/Settings/RobotRacing_RPAsset` + `Renderer` (HDR on, no MSAA, SMAA on the
   camera, soft shadows from a 2048 map in two cascades over 60 units, four lamps per object,
   HDR colour grading) in Graphics and in all six quality levels, and switches the project to
   linear colour. The three shaders (`Flat`, `Glass`, `Sky` in `Resources/Shaders`) were
   rewritten in URP HLSL with the same names and properties, so `Look.cs` kept working: no
   material went pink or grey. `Flat` gained a sheen (sun highlight plus a reflection of the
   gradient sky, fed as globals) for the car's paint, eyes and kerbs. A fourth shader, `Spark`,
   draws the particles, so the web build carries four small shaders instead of URP's sets.
3. **Lighting** (`Circuit.BuildLight`): a warm key at 42 degrees from the camera's right at 1.15
   with full-strength soft shadows, gradient ambient (cool sky, warm ground), fog from 30 to 120
   units, a darker verge under the track, lamp posts on the stand and a lit arch at dusk.
4. **Post-processing** (`World/Atmosphere.cs`, built in code on the camera): ACES, bloom 0.35 at
   threshold 1.1, an ink vignette, saturation +8 / contrast +5, SMAA, a Gaussian depth of field
   that tweens in behind the briefing and the debrief and off for play, a vignette pulse for
   crashes (red) and finishes (teal), and the hit-stop.
5. **Cameras** (`World/CameraRig.cs`, Cinemachine 2.10.5): the scene's own `SceneCamera` keeps
   the overview, the portrait and the title's orbit (it fits the circuit into a screen rect by
   shifting the projection, which no rig does); the rig takes over for the chase behind the
   car (transposer with damping, composer aimed at the frame's centre, a field-of-view kick
   with speed), the countdown's fly-in from high and wide, the impulse shake on a crash and the
   slow-motion orbit of the last finish, then hands the camera back where it is.
6. **Feel and VFX**: pooled particle systems (`World/Effects.cs`: amber sparks on a crash,
   confetti on a finish and the medal, dust under the car at speed), teal tyre trails, ghost
   lines of the last fourteen attempts on the track (teal to a finish, red to a crash, fading
   with age: the learning, visible), a 70 ms hit-stop on a watched crash, and the banners
   rebuilt as moments: dark glass, the words in the moment's colour, an overshoot in, a flash
   of the frame and a line underneath for the big ones. "3 · 2 · 1 · GO" opens every race;
   "REWARD HACKED · points, no lap" is the biggest moment of the level that teaches it; "FIRST
   FINISH", "THE CURVE SETTLED", "RACE OVER" and the lap moments are the rest.
7. **Sound** (`Ui/Sfx.cs`, all synthesised at start): an engine loop whose pitch follows the
   car's speed (heard at 1x and 10x), a crash thud, a chime on a finish, beeps and a GO for the
   countdown, a fanfare for the race and the medal, an alarm for the wrong reward, a wind bed
   while the world shows, one gain layer, a sound switch on the title remembered on the
   machine, and everything ducks under LUMA's voice.
8. **HUD and screens**: static Inter SDF atlases baked by `Assets/Editor/FontBaker.cs` (the
   runtime fonts stay as the fallback); every rounded box on the exact 12 px corner sprite,
   the level nodes as rounded squares, the medal as a badge, no pill anywhere; the briefing
   and the debrief rebuilt around a window on the world (the level's track on its grid; the
   race track turning slowly) with the cards beside it; the debrief's medal revealed with
   stars, confetti and a fanfare; the quiz beside a tall window; the run's HUD in dark glass
   (level and step, attempt, reward score, best, lap) with the speed controls beside the view
   toggle. The `Ui.Group` helper used `??` on `GetComponent`, which only works in builds; it is
   `TryGetComponent` now, so the game runs in the Editor's Play mode as well.

## Scores after

Same walk, same six axes, same reference (Claude Bomb's title at 3.8). Frames in
`games/robot-racing/Builds/look-audit/after/`; the "before" set is still in `before/`.

| Screen | Palette | Style | Lighting | Silhouette | UI | Feedback | Mean | Before |
|---|---|---|---|---|---|---|---|---|
| 1-title | 4 | 4 | 4 | 3 | 4 | 3 | 3.7 | 2.7 |
| 2-briefing | 4 | 4 | 3 | 2 | 4 | 2 | 3.2 | 1.8 |
| 2b-warm-up | 4 | 4 | 3 | 2 | 4 | 2 | 3.2 | 1.8 |
| 3-pit-lane | 4 | 4 | 4 | 3 | 4 | 2 | 3.5 | 2.2 |
| 4-training-slow | 4 | 4 | 4 | 3 | 4 | 4 | 3.8 | 2.2 |
| 5-race | 4 | 4 | 4 | 3 | 4 | 5 | 4.0 | 2.3 |
| 6-debrief-questions | 4 | 4 | 3 | 2 | 4 | 2 | 3.2 | 1.5 |
| 7-debrief | 4 | 4 | 3 | 3 | 4 | 4 | 3.7 | 1.8 |
| **mean** | | | | | | | **3.5** | **2.0** |

The board, in the scorecard's grammar, for the game as a whole:

- palette: 4/5 — every colour traces to the bible or the brand; warm concrete and limestone
  instead of the interface's cool neutrals on the boxes; no default material, sky or UI.
- style: 4/5 — one flat-shaded toy-diorama style from the car to the stand to the sky, and
  the dark glass of the HUD and the moments belongs to it; the one seam is the car itself.
- lighting: 4/5 — a warm key with full soft shadows, cool shadow sides, fog, bloom on the
  lamps and the antenna, a vignette; 3 on the card screens where the depth of field softens
  the world on purpose.
- silhouette: 3/5 — the agent is still a box with a dome, now with a lit antenna, bright
  eyes and sheen; it reads as a car from the chase camera and as a dot from the overview. The
  character pass (fix 6 of the audit) was not part of this pilot and is the axis left at the
  floor.
- UI: 4/5 — 12 px corners everywhere, no pill or circle button, a dark-glass mission HUD,
  moments instead of white boxes, a medal badge with stars, baked Inter atlases; the quiz is
  still a column of buttons, now beside the world.
- feedback: 4/5 — sparks, shake, hit-stop and a red pulse on a crash; confetti and a chime on
  a finish; ghost lines of the attempts on the track; "FIRST FINISH", "THE CURVE SETTLED",
  "REWARD HACKED", "3 · 2 · 1 · GO", the slow-motion finish and the medal reveal; 2 on the
  briefing, the warm-up and the quiz, which show the world but do not answer the player.

All six axes are at or above the floor of 3 for the game; the two screens that stay at 2 on
single axes are the card screens on silhouette and feedback, where the world is visible but
the player is reading.

## Web after

Same yardstick as the baseline: the gzip build on disk under `site/games/robot-racing/Build`,
and the load from navigation to the loader hiding in headless Chrome with software rendering,
cache off, median of three runs.

| | Before | After |
|---|---|---|
| Build on disk | 17.9 MB | 19.8 MB |
| Transferred | 17.6 MB | 19.5 MB |
| Load time | 3.1 s | 4.0 s |
| data file | 8.0 MB | 10.8 MB |
| wasm | 10.2 MB | 9.3 MB |

The data file grew 2.8 MB: the two baked Inter atlases (2.2 MB each uncompressed, the
runtime fonts are still there as the fallback) and URP's resources. The code shrank 0.9 MB
from the release settings now in `Builder.BuildWeb` (strip engine code, IL2CPP optimised for
size, no debug symbols, DiskSize+LTO linking, which lives in `Library/` and so is set by the
build script on every machine). Shader stripping in Graphics settings was not changed: the
Editor API for it did not resolve in this version and the four custom shaders are small.
Still well under the 25 MB budget. On the platform: the door, the menu, the game's sheet and
the game itself load and run (`games/robot-racing/Builds/look-audit/platform/`), the title
rendering with URP and the post-processing in the browser. Frame rate on the web was not
measured (headless Chrome renders in software); the Mac build runs the walk at 60 fps.

## Two findings worth keeping

- **An audible engine loop quits the Editor on this Mac.** With the engine's loop playing
  through the speakers, the Robot Racing Editor shut itself down 10 to 35 s into Play mode,
  cleanly, with nothing in any log; bisected live to that one AudioSource (not the camera,
  post, effects, UI, pitch, loop length or the runner), and never once with the Editor muted
  or the engine silent. The engine is now plain waves (the layer of noise it had is the one
  ingredient never shown safe aloud), the Editor is muted for every automated session, and the
  screenshot walk zeroes the listener. The audible build has not been retested on this Mac:
  play the Mac build with sound once to confirm.
- **The web build grew 2 MB** (fonts and URP resources in the data file) while the code
  shrank (strip engine code, IL2CPP size, DiskSize+LTO linking): see the table above.
