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

---

# Phase 3a: AI Arena

5 November 2026, branch `look-and-feel`, after the Robot Racing pilot was approved. The same
floors, adapted to the market: every step checked in the live Editor (muted) before the next,
the Mac build's own walk made the "after" set in `games/ai-arena/Builds/look-audit/after/`, and
the web build was measured on the platform. LUMA's lines, the curriculum text and the exact
words the agent reads are unchanged.

## What changed

1. **Art bible**: `games/ai-arena/Docs/art-bible.md`. A toy-market diorama under a late-morning
   sun: warm limestone plaza and tiles, meadow and moss, oak and linen stalls under aqua
   awnings, the agents in their team colours with white heads; the dusk variant with every
   stall's lamp lit for the title, the story and the cover.
2. **URP 17.3.0 and Cinemachine 2.10.5**, the same `RenderSetup` as Robot Racing (`Assets/
   Settings/AIArena_RPAsset` + `Renderer`, all quality levels, linear colour), the same four
   URP shaders (`Flat` with its sheen, `Glass`, `Sky`, `Spark`), `Look.cs` extended with gloss
   and the sky globals. No material went pink or grey.
3. **Lighting** (`Market.BuildLight`): the warm key at 42 degrees at 1.15 with full soft
   shadows, gradient ambient, fog from 30 to 120, a mossy verge under the square; at dusk every
   lamp glows and a third of them light the stalls around them.
4. **Post-processing**: `Atmosphere` on the camera (ACES, bloom, vignette, grading, SMAA,
   depth of field behind the briefing, the decide screen and the debrief, the red pulse and
   hit-stop when an agent is sent home).
5. **Cameras** (`CameraRig`): the agent's eye is now a Cinemachine chase over the chosen
   agent's shoulder (the stalls it can see in colour, every stall beyond its sight a grey
   question-mark crate, its sight square on the ground); the countdown flies in from high and
   wide behind the first agent and back to the overview as they set out; an impulse shake when
   an agent is sent home; ALL HOME turns slowly around the first agent home in slow motion.
6. **The loop, visible.** The HUD carries LOOK · DECIDE · MOVE and lights them one after the
   other every turn: LOOK flashes a ring in the agent's colour under the chosen agent as its
   observation is written, DECIDE holds while the agents think (the antennas pulse), MOVE while
   they walk. SENT HOME (red, sparks, shake, alarm, the item that ruined dinner underneath), OUT
   OF TOKENS (amber), ALL HOME (teal, fanfare, confetti over every house, the slow turn),
   NOBODY CAME HOME and n OF m HOME are moments in dark glass; 3 · 2 · 1 · AGENTS OUT opens
   every errand. A buy chimes with a puff of teal confetti; dust rises under a walking agent.
7. **Sound**: Robot Racing's layer without the engine: chime, thud, beeps, GO, fanfare, alarm,
   a murmur over the market while it shows, a sound switch on the title, ducking under LUMA.
8. **Screens**: baked Inter atlases, 12 px corners everywhere (the errand nodes and the stars
   are rounded squares, the status dot too), the briefing rebuilt around a window on the
   errand's market with the ladder in glass over it and the agent's parts in two columns, the
   decide screen's five tasks on a sheet over the soft market, the debrief with the market
   turning in a window, the medal badge revealed with stars, confetti and a fanfare, the
   unlock card beside LUMA's takeaway, the run's mission HUD in dark glass (errand and step,
   turn, home count, baskets, tokens left) with the speed controls beside the view toggle.
   `Ui.Group` fixed for the Editor; the walk and the cover are silent.
9. **Web release settings** in the build script (strip engine code, IL2CPP for size, no debug
   symbols, DiskSize+LTO); the frame rate left to the browser.

## Scores after

Same walk (twelve screens), same six axes, same reference. Frames in
`games/ai-arena/Builds/look-audit/after/`; the "before" set is still in `before/`.

| Screen | Palette | Style | Lighting | Silhouette | UI | Feedback | Mean | Before |
|---|---|---|---|---|---|---|---|---|
| 1-title | 4 | 4 | 4 | 3 | 4 | 3 | 3.7 | 3.2 |
| 2-story | 4 | 4 | 4 | 3 | 4 | 2 | 3.5 | 2.7 |
| 2b-story-market | 4 | 3 | 4 | 3 | 4 | 3 | 3.5 | 2.8 |
| 2c-story-tomato | 4 | 4 | 4 | 3 | 4 | 2 | 3.5 | 2.2 |
| 3-lesson | 4 | 4 | 3 | 2 | 4 | 2 | 3.2 | 1.8 |
| 3b-warm-up | 4 | 4 | 3 | 2 | 4 | 2 | 3.2 | 1.8 |
| 4-tour | 4 | 4 | 4 | 3 | 4 | 3 | 3.7 | 2.5 |
| 4b-decide | 4 | 4 | 3 | 1 | 4 | 2 | 3.0 | 1.5 |
| 5-setup | 4 | 4 | 4 | 3 | 4 | 3 | 3.7 | 2.7 |
| 6-market | 4 | 4 | 4 | 3 | 4 | 5 | 4.0 | 2.8 |
| 6b-agents-eye | 4 | 4 | 4 | 3 | 4 | 5 | 4.0 | 3.0 |
| 7-debrief | 4 | 4 | 3 | 3 | 4 | 4 | 3.7 | 2.0 |
| **mean** | | | | | | | **3.6** | **2.4** |

The board for the game:

- palette: 4/5 — the bible's warm stone, meadow, oak and linen on every box, the team colours on
  the agents, teal only where the lesson points; no default material, sky or UI.
- style: 4/5 — one diorama from the stalls to the houses to the trees, the glass HUD and the
  moments part of it; the story's flat sprites are the one other register, kept on purpose.
- lighting: 4/5 — the warm key with full soft shadows by day, every lamp lit at dusk with
  bloom, fog, a vignette; 3 behind the card screens where the depth of field is doing its job.
- silhouette: 3/5 — the agent is still a box with a box head, now with a sheen, bright eyes
  and a glowing antenna; it reads as a robot over its shoulder and as a coloured dot from the
  overview. The character pass stays the axis at the floor, as in Robot Racing.
- UI: 4/5 — 12 px corners everywhere, the errand nodes and the stars rounded squares, the
  mission HUD and the moments in dark glass, the medal badge, baked Inter.
- feedback: 4/5 — the loop's three words light every turn with a ring under the agent, SENT
  HOME with sparks, shake, pulse and alarm, a chime and confetti on a buy, 3 · 2 · 1 · AGENTS
  OUT, ALL HOME with the slow turn, the medal reveal; 2 on the briefing, the warm-up and the
  decide sheet, which show the world but do not answer the player.

## Web after

| | Before | After |
|---|---|---|
| Build on disk | 17.6 MB | 19.5 MB |
| Transferred | 17.3 MB | 19.2 MB |
| Load time | 2.3 s | 3.8 s |
| data file | 7.8 MB | 10.5 MB |
| wasm | 9.8 MB | 9.3 MB |

The same shape as Robot Racing: the data file grew (the baked Inter atlases and URP's
resources), the code shrank with the release settings. Well under the 25 MB budget, and the
game loads and runs on the platform (`games/ai-arena/Builds/look-audit/platform/`).

---

# Phase 3b: Tech Escape

5 November 2026, branch `look-and-feel`, after AI Arena was approved. The same floors, adapted
to the facility: every step checked in the live Editor (muted) before the next, the Mac build's
own walk made the "after" set in `games/tech-escape/Builds/look-audit/after/`, and the web build
was measured on the platform. LUMA's lines, the curriculum text and the exact words the agent
reads are unchanged.

## What changed

1. **Art bible**: `games/tech-escape/Docs/art-bible.md`. A permit office in lockdown, cut open
   from above: concrete in lockdown blue, the four door colours the only saturated colours on the
   set (red locked, amber current, teal held, scarlet breached), the light coming from the
   facility itself (the strips, the screens, the LEDs, the exit sign) under a cool moon. Lockdown
   on every screen, not only the title.
2. **URP 17.3.0 and Cinemachine 2.10.5**, the same `RenderSetup` as the other two (`Assets/
   Settings/TechEscape_RPAsset` + `Renderer`, all quality levels, linear colour), the same four
   URP shaders, `Look.cs` rewritten for the bible with gloss and the sky globals. No material went
   pink or grey.
3. **Lockdown lighting** (`Building.BuildLight`): one cool key at 50 degrees, full soft shadows,
   gradient ambient in blues, fog from 50 to 160; every door's strip is a real point light on the
   corridor (the floor cut into one piece per door so each piece takes its own strips); the
   room on screen has its own cool ceiling lamp and a warm one on the desk; the door frames and
   the floor lines glow faintly; bloom on the strips, the screens and the visor. The web's limit
   of visible additional lights (16) is respected: 12 strips, the exit, the attacker's light.
4. **Each room's misconfiguration readable**: every room has the one prop that says what it is
   (a register board and a filing cabinet, a mail sorter with one red envelope, a safe with a
   teal dial, a glass cell with something red in it, a badge gate, a workbench of small servers
   with one amber light, patch panels and cables, a wall of switches with one red, a lockbox with
   a padlock and the key beside it, a cloud over a pedestal, a wall of screens, shelves of tapes),
   and a board of lamps on its back wall, one per row of the system, teal when every decision in
   that row is made and amber while one is still "?"; the sheet's rows carry the same lamp. The
   room screen is the world full-screen with the HUD, the runs, YOU FIX and LUMA in dark glass.
5. **Post-processing**: `Atmosphere` on the camera (ACES, bloom, vignette, grading, SMAA, depth
   of field behind the briefing, the warm-up and the debrief with its distances set per window so
   the subject stays sharp, the red pulse and the hit-stop when a door gives way).
6. **The attack replay** (`CameraRig`, `Building.Visit`, `AttackScreen`): THE ATTACK BEGINS with
   a siren, a red pulse and the alarm (every locked door's strip strobes scarlet), the camera
   flying in high over the entrance and blending into a chase behind the attacker down the
   corridor (world-space, following a point kept to the corridor's middle band, so it never
   leaves the corridor for a room when the attacker turns to a door; the corridor itself was
   extended five units before the first door to give the camera room, and the old solid end
   wall became an open entrance); the attacker, lighter and glossy with a scarlet band on its
   shoulders and a scarlet ring on the floor under it, leaves a scarlet trail and dust under
   its feet; a door that gives way is
   sparks, an impulse shake, the hit-stop, the crash and a big THROUGH in scarlet with the gap
   under it; one that holds is a teal flash of motes, a clack and HELD; the HUD carries the
   twelve doors as lamps and THROUGH · HELD counts, the lines fill a glass column; the end is a
   slow turn in slow motion around the last door (ONE DOOR GAVE WAY / n DOORS GAVE WAY, NOTHING
   GOT THROUGH) or around the exit as it opens (THE EXIT OPENS, fanfare, confetti), then back to
   the overview, the breached door still scarlet and the trail still on the floor.
7. **Sound**: the shared layer (chime, buzz, beeps, fanfare, alarm) plus the siren, the clack of
   a door that held, the crash of one that gave, and the low hum of a facility in lockdown while
   the world shows; a sound switch on the title; ducking under LUMA. Plain waves only.
8. **Screens**: baked Inter atlases, 12 px corners everywhere (the title's level stops and stars
   are rounded squares), the briefing rebuilt around a window on this level's door lit amber with
   the ladder in glass over it and the facility room by room under it, the debrief over the
   facility turning slowly with the breached door scarlet, the medal badge revealed with its
   stars, confetti and a fanfare, the unlock card beside the checks; the warm-up over the soft
   facility. `Ui.Group` fixed for the Editor; the walk and the cover are silent.
9. **Web release settings** in the build script (strip engine code, IL2CPP for size, no debug
   symbols, DiskSize+LTO); the frame rate left to the browser.

## Scores after

Same walk (the seven screens, plus the end of the attack as an eighth frame), same six axes,
same reference. Frames in `games/tech-escape/Builds/look-audit/after/`; the "before" set is still
in `before/`.

| Screen | Palette | Style | Lighting | Silhouette | UI | Feedback | Mean | Before |
|---|---|---|---|---|---|---|---|---|
| 1-title | 4 | 4 | 4 | 3 | 4 | 3 | 3.7 | 3.0 |
| 2-briefing | 4 | 4 | 4 | 2 | 4 | 2 | 3.3 | 1.8 |
| 2b-warm-up | 4 | 4 | 3 | 2 | 4 | 2 | 3.2 | 1.8 |
| 3-room | 4 | 4 | 4 | 3 | 4 | 3 | 3.7 | 1.8 |
| 4-attack | 4 | 4 | 4 | 3 | 4 | 5 | 4.0 | 1.8 |
| 4b-attack-end | 4 | 4 | 4 | 3 | 4 | 4 | 3.8 | – |
| 5-debrief | 4 | 4 | 3 | 3 | 4 | 4 | 3.7 | 1.7 |
| 6-room-ordered | 4 | 4 | 4 | 3 | 4 | 3 | 3.7 | 1.8 |
| **mean** | | | | | | | **3.6** | **1.9** |

The board for the game:

- palette: 4/5 — the bible's lockdown blues on every surface, the four door colours the only
  saturated ones and each meaning one thing, scarlet for the attacker and the alarm, teal where
  the lesson points; no default material, sky or UI.
- style: 4/5 — one cut-open facility from the entrance to the exit, every room furnished for its
  job, the glass HUD and the moments part of it; the white cards of the briefing and the debrief
  are the one other register, kept on purpose and always with the facility behind them.
- lighting: 4/5 — the strips light the corridor, the room on screen has its own lamps, the moon
  catches the wall tops, bloom on everything that glows, fog, a vignette; 3 behind the warm-up
  and the debrief, where the depth of field and the scrim do their job.
- silhouette: 3/5 — the attacker is a capsule under a hood, now with a sheen, a scarlet visor and
  band and a ring on the floor, so it reads over its shoulder and from above; the rooms' props
  read at a glance. The character pass stays the axis at the floor, as in the other two.
- UI: 4/5 — 12 px corners everywhere, the level stops rounded squares, the HUD, the ladder, the
  lines and the moments in dark glass, the medal badge, baked Inter.
- feedback: 4/5 — the siren and the alarm strobe, the chase, the trail, sparks, shake, pulse and
  hit-stop on a breach, the teal flash and clack on a hold, THROUGH and HELD at every door, the
  door lamps and counts in the HUD, the slow turn at the end, the medal reveal; 2 on the briefing
  and the warm-up, which show the world but do not answer the player.

## Web after

| | Before | After |
|---|---|---|
| Build on disk | 19.9 MB | 21 MB |
| Transferred | 19.5 MB | 21.4 MB |
| Load time | 2.6 s | 3.7 s |
| data file | – | 12.8 MB |
| wasm | – | 9.3 MB |

The same shape as the other two: the data file carries the baked Inter atlases and URP's
resources, the code shrank with the release settings. Under the 25 MB budget, and the game loads
and runs on the platform (`games/tech-escape/Builds/look-audit/platform/6-game-tech-escape.png`).

---

# Phase 4a: AI Arena, the text pass

5 November 2026, branch `look-and-feel`, after the three look-and-feel passes. The brief: a
game is not a page. Claude Bomb's manuals are in Japanese, the story is four lines over a
camera feed, LUMA speaks one line at a time and it goes; our three games had paragraphs on the
briefing, lists on the debrief and a caption under everything. AI Arena is the pilot.

## The rule

On any screen, at most one sentence of English is on screen, it is LUMA's, it is spoken, typed
out as she says it, and it fades (`Luma.Line`). Everything else is the world, numbers, icons,
and **the file**: a sheet (`FileSheet`, the button "The file" on every screen that had the
text) with everything the course says about the errand: the question and the headline, the
idea, the message and its catch, the training runs' reasons, the takeaway, the question for
the agent's manager, how the garage works, and whatever the screen adds (the debrief's full
stories and checks). Nothing was deleted from the game; it stopped being the screen. LUMA's
recorded lines, the curriculum text in `Lesson.cs` and the exact words the agent reads are
unchanged.

## What left each screen

| Screen | Was | Is |
|---|---|---|
| Title | a 40-word blurb, LUMA's card, the errands' names, a footer | title, tagline, LUMA's one line, the errands as numbered stops with their stars; the voice and sound switches |
| Story | LUMA's card, a "best with sound on" hint | her line in a smaller card; the pictures and captions as they were |
| Briefing | kicker, LUMA, the 80-word idea, the message with its catch and the whole catalogue, six part cards with descriptions, a caption | the question, the headline, LUMA's intro spoken, the message, six part chips (this errand's lit), the ladder; the rest in the file |
| Warm-up | "Right. That is from errand 1.1." | "Right." / "Not quite." with the answer lit |
| Garage | a run note, the shelf hint, a dial blurb under each dial, "opens at … until then every agent gets the same…", the 40-word pocket-money arithmetic | labels only (THE LIST, IF NOTHING IS IN VIEW, YOUR OWN WORDS, POCKET MONEY, AGENTS); the training run's reason spoken by LUMA once over the robot; "Opens at errand 1.x"; "About 773 tokens a turn · 30,000 tokens buys the whole errand" |
| Run | LUMA's card with three-sentence commentary | her lead sentence typed at the foot of the column, gone when she is done; the mission line, the cards and the loop as they were |
| Debrief | the message and turn count, LUMA's takeaway + the five ways + the ask, three-line stories per agent, Done/Missed words with the full check labels, an unlock sentence | the takeaway as her line, one line per agent (and the part that failed, on the capstone), lamps with short labels, the unlock as a name; the full stories and checks in the file |
| Decide | a 50-word instruction card, the reasons on the debrief rows | "Mark each task: an agent, a simple automation, or neither."; the reasons in the file |

## The short forms

The debrief's lamps carry these; the full label stays in the file and in the platform's report.

| Full label | On the lamp |
|---|---|
| Said what to buy: apples | Named apples |
| Said what to leave at the market | Said what to leave |
| Came home with dinner | Came home |
| Named the items, not just "fruit" | Named the items |
| Covered the catch: the tomato | Covered the tomato |
| Said what to do when nothing on the list is in view | A rule for nothing in view |
| Found something on the list | Found a listed item |
| Told it what to do when nothing is in view | A rule for nothing in view |
| Bought something on the list / Bought at least one thing on the list | Bought a listed item |
| No notebook to write in | No notebook |
| Nothing in the basket, so no cost per item | Empty basket |
| Named the items and the exceptions | Named items and exceptions |
| Said what to do when the list does not fit | A rule for nothing in view |
| Cheapest item of the round: $x | Cheapest item: $x |
| Chose a simple automation where one did the job | A rule where a rule did |
| Chose an agent where the task needed judgment | An agent where it needed one |
| Marked the task for a person as neither | Neither, for the person's call |

The one-line stories: "Sent home on turn 2: bought a carrot", "Out of tokens with 1 in the
basket", "Came home with 2 wanted items", "Came home with nothing"; on the capstone the part
that failed in two or three words ("The job description", "Its brain", "Its budget", "What it
saw", "What it remembered"). The unlock names: "How far it sees", "Memory", "Brain and effort",
"Agent, rule, or neither", "Everything open: the capstone".

Frames: `games/ai-arena/Builds/look-audit/text/` (the same walk as `after/`, plus `3c-file.png`
with the file open over the briefing).

## Web after the text pass

19.2 MB transferred, 3.7 s to load (was 19.2 MB / 3.8 s after the look pass): the words moved,
they did not leave the build. Loads and runs on the platform.

---

# Phase 4b: Robot Racing, the text pass

6 November 2026, the same rule as AI Arena's (Phase 4a), applied to the circuit. `Luma.Line`,
`FileSheet` and `Labels` carried over; the file's standing section here is "How the pit lane
works" (the sensors, the moves, the points table, new moves, the knobs, the tracks, the measure,
the attempt budget, what a locked setting means).

## What left each screen

| Screen | Was | Is |
|---|---|---|
| Title | a 40-word blurb, LUMA's card, every level's name beside its tier, a footer | title, tagline, LUMA's one line, three tiers of numbered stops with stars; the voice and sound switches |
| Briefing | kicker, LUMA, the 70-word idea, the track with its note and map, seven part cards with descriptions and "fixed … opens at …", a caption | in dark glass over the sharp circuit: the question, the headline, LUMA's intro spoken, the track's name, line and map, seven part chips (this level's lit), the ladder; the rest in the file |
| Warm-up | "Right. That is from level 1.1." | "Right." / "Not quite." with the answer lit |
| Pit lane | a run note, "150 attempts to train, then 5 laps to race on the test track.", long section titles, "LOCKED · OPENS AT 3.1", a hint on fixed settings, the 25-word attempt-budget sentence, the training run's reason in a caption | "150 attempts · 5 laps on the test track"; THE TRACKS, WHAT IT SENSES, WHAT IT MAY DO, THE POINTS TABLE, NEW MOVES, THE KNOBS, THE MEASURE; "OPENS AT 3.1"; the training run's reason spoken by LUMA once over the car; "Watch the pro: a ready setup" / "Fix it: no points at all" |
| Run | LUMA's card with three-sentence commentary; "This attempt: 0 points · best attempt: -100 · last move earned -100.2"; "Chance of a new move right now: 0.87 · the moves that paid come back more often; the rest it tries at this chance."; "No finish yet. Random moves, and the points sorting them."; the rule book's long title | her lead sentence at the foot of the column, gone when she is done; "This attempt 0 · best −100 · last move −100.2"; "New move chance 0.87"; "No finish yet"; THE RULE BOOK · THE SITUATION, THE MOVE THAT PAID; "The race on one bend: 0/5 laps" |
| Debrief | "A few questions first…" over the questions; Done/Missed with the full check labels; each run's settings in a second line; LUMA's takeaway + the three kinds of learning + the ask; unlock sentences | lamps with short labels; one line per run; the takeaway as her line; the unlock as a name; the full checks, each run's settings, the three kinds of learning and the ask in the file |

## The short forms

The debrief's lamps carry these; the full label stays in the file and in the platform's report.

| Full label | On the lamp |
|---|---|
| Said what the robot knows at the start: nothing | Knows nothing at the start |
| Trained a robot to a finish | Trained it to a finish |
| Found why the first setup never improved | Found why it never improved |
| Trained until the curve settled | Trained until it settled |
| The robot finished most of several races, not one | Finished most races |
| Read the rule book for one situation | Read the rule book |
| Explained why the robot is not supervised learning | Why it is not supervised |
| The robot got past both bends | Got past both bends |
| Explained why the first robot could not learn it | Why the first robot could not |
| The robot finished | Finished |
| A constraint was set | Set a constraint |
| The rule book uses slow down where it matters | Slows down where it matters |
| Set finish, crash and on-the-way points | Set the three points |
| Learned within the attempt budget | Learned within the budget |
| The crash costs more than any single move can earn | A crash costs more than a move |
| Predicted what the robot would do before running | Predicted first |
| Explained why a curve can rise while the robot never finishes | Why the curve lies |
| Set a high chance that falls | A high chance that falls |
| The robot's line improved after its first finish | The line improved |
| Finished within the attempt budget | Finished within the budget |
| No loophole exploited | No loophole |
| Compared the prediction with what happened | Compared the prediction |
| The sensors on are the ones the track needed | The right sensors on |
| The test track was never trained on | A test track never trained on |
| The robot finished the test track | Finished the test track |
| Explained what the one-track robot had learned | What the one-track robot learned |
| Picked the measure that fits the stated cost / The measure chosen matched the stated cost | The measure fits the cost |
| Explained in one line why finish rate lied | Why finish rate lied |
| Changed one knob per run | One knob per run |
| Named the knob that helped | Named the knob that helped |
| Recognized the rewrite speed set too high | Spotted the rewrite too high |
| The test track passed | Passed the test track |
| The knobs were set one at a time | Knobs one at a time |
| The real run finished, or the difference that broke it was named | The real run, or what broke it |

Unchanged: Named the five parts, Sorted the six problems, Read the curve, The fixed robot
finished, The curve settled, Kept the seed, Trained within the budget. The unlock names: "The
attempts", "The sensors", "The moves and a constraint", "The points table", "New moves", "The
tracks", "The measure", "The knobs and the seed", "Everything open: the capstone".

Frames: `games/robot-racing/Builds/look-audit/text/` (the same walk as `after/`, plus
`2c-file.png` with the file open over the briefing).

## Web after the text pass

loaded in 4.2 s, 20.5 MB transferred (was 19.5 MB / 4.0 s after the look pass). Loads and runs on the platform.

---

# Phase 4c: Tech Escape, the text pass

6 November 2026, the same rule, applied to the facility. The file's standing section here is
"How a room works" (the rows and their chips, the lamps on the rows and on the room's wall,
adding what is not listed, signing a record, the three runs, and what the attack does with it);
the file also carries the facility's record, which used to be a card on the debrief.

## What left each screen

| Screen | Was | Is |
|---|---|---|
| Title | a 45-word blurb, LUMA's card, each tier's room names, a footer | title, tagline, LUMA's one line, three tiers of numbered stops with stars; the voice and sound switches |
| Briefing | kicker, LUMA, the 70-word idea, YOU FIX, FIX IT with its why, the twelve rooms as rows with "a gap: …" texts, a caption | in dark glass over the sharp corridor with this level's door lit amber: the question, the headline, LUMA's intro spoken, YOU FIX in one line, the mistake's name, the twelve rooms as lamps (held, a gap, this one, not yet), the ladder; the idea, the why and the record in the file |
| Warm-up | "Right. That is from level 1.1." | "Right." / "Not quite." with the answer lit |
| Room | "Fix it: an asset nobody listed. The ready register misses the working copy…" in the HUD; LUMA's card | "Fix it: An asset nobody listed"; her line, gone when she is done; the rows, their lamps and YOU FIX as they were |
| Attack | each line's full sentence in the column; LUMA's card | HELD · room, or THROUGH · room · the gap; her line; the moments as they were |
| Debrief | Held/Through with every sentence, Done/Missed with the full check labels, the record card, LUMA's takeaway + the standard + the ask, | lamps: Held · room / Through · room · gap, lamps with short labels, the takeaway as her line; the full lines, the checks, the record, the standard and the ask in the file |

## The short forms

The debrief's lamps carry these; the full label stays in the file and in the platform's report.

| Full label | On the lamp |
|---|---|
| Administration unreachable from the public side | Admin closed to the public side |
| An alert that fired on the intrusion and not on the noise | An alert on the intrusion, not the noise |
| An incident log with a time for each step | A timed incident log |
| At least one cause is a control gap from tier 2 | A cause from a tier 2 gap |
| Backups unreachable from the zones the attack ran in | Backups out of the attack's reach |
| Blocks are logged, by the last rule | Blocks logged by the last rule |
| Both forms have the facts a responder needs | Both forms carry the facts |
| Each control picked for the risk it lowers, with its Function | Each control fits its risk |
| Each crossing is listed and justified | Each crossing listed and justified |
| Each has an impact in the three terms | An impact in the three terms |
| Each link names its room and its gap | Each link names room and gap |
| Each report has the facts a responder needs | Each report carries the facts |
| Each service's rights limited to its job | Each service limited to its job |
| Every action has an owner and a due date | Every action owned and dated |
| Every asset in the facility is on the register | Every asset on the register |
| Every cause has an action with a done-when | Every cause has a done-when |
| Every control has evidence that would satisfy an auditor | Evidence for every control |
| Every high risk has a treatment decision | Every high risk treated |
| Every link in the record is in the chain, in order | The chain complete, in order |
| Every privileged account has MFA | MFA on every privileged account |
| Every suspicious message caught, with a reason that is not "it looked wrong" | Every suspicious message caught, with a reason |
| Every ticket has an owner and a date | Every ticket owned and dated |
| Evidence kept before anything is removed | Evidence kept first |
| Evidence of each, dated and signed | Evidence, dated and signed |
| No account for a job that no longer exists | No account for a gone job |
| No account with more than its job needs | No account above its job |
| No high risk without a control | No high risk uncontrolled |
| No key on the same system as its data | No key beside its data |
| No private data in the logs | No private data in logs |
| No public storage of sensitive assets | Sensitive storage not public |
| Nothing exploitable today rated below High | Exploitable today rated High |
| Nothing on the form is a guess presented as a fact | No guess presented as fact |
| Nothing reachable from outside on a high-value asset left open | No reachable high-value gap left |
| Ordered by severity, exposure and asset value, not score alone | Ordered by severity, exposure, value |
| Rated likelihood and impact for every risk | Every risk rated |
| Rotate, invalidate, revoke, in that order | Rotate, invalidate, revoke, in order |
| Severity declared within the time limit and justified by the facts | Severity declared in time, justified |
| Sign-in, access and network logs kept for the sensitive assets | Sign-in, access, network logs kept |
| The commander made no technical change | The commander made no change |
| The decision, event or incident, is right and gives a reason | Event or incident: right, with a reason |
| The evidence says what was checked and when | Evidence says what and when |
| The internal address unreachable from services that do not need it | Internal address closed to the rest |
| The priority actions trace back to the tier 1 register | Priorities trace to the register |
| The public zone cannot reach the data zone | Public zone cannot reach data |
| The record names who approved each account kept | Each kept account has an approver |
| The runbook has no "consider" | No "consider" in the runbook |
| The sensitive assets are encrypted at rest | Sensitive assets encrypted at rest |
| The three actions each tie to a high risk and its control | Three actions tied to high risks |
| The trace is complete from the first sign-in to the data touched | The trace complete, sign-in to data |
| The trigger and the root causes are distinct | Trigger and root causes distinct |
| The trigger is marked, and is not called the cause | The trigger marked, not the cause |
| The whole-facility attack got through nowhere | The attack got through nowhere |
| Their own access, not the administrator's | Own access, not the admin's |
| Traffic across the public line is encrypted | Public-line traffic encrypted |
| Whatever got through is in the record with the room and the gap | What got through is in the record |

The thirty other labels were already five words or fewer and are shown as they are.

Frames: `games/tech-escape/Builds/look-audit/text/` (the same walk as `after/`, plus
`2c-file.png` with the file open over the briefing).

## Web after the text pass

22.5 MB transferred, 2.4 s to load (was 21.4 MB / 3.7 s after the look pass; the two load
times are the same yardstick, headless Chrome with software rendering, and swing with the
machine's load). Loads and runs on the platform.

---

# Phase 5a: AI Arena, the garage

6 November 2026. Charbel, testing the web build: "I don't feel here I'm in a game, too much
details." The garage was a settings sheet with an agent standing next to it. What changed:

- **The agents run by themselves; the player writes and watches.** The three runs of an errand
  (Watch the pro, Fix the broken bot, Your run) are gone as a control. A level opens with its
  list already in the box: the broken one on a fix-it level ("Get fruit."), the ready one on
  1.1, nothing on the capstone. LUMA says once, over the agent, why it works or what is wrong
  with it. One run per level; it always counts.
- **One agent.** The game was built for a class, one agent per student; for a player that is
  noise. The roster now defaults to one, the player's (named, coloured), and a class adds more
  in Settings. The agents' chips show only when there is more than one.
- **The dials live on the agent.** The four upgrade cards are gone; four tags hang off the
  agent's parts: BRAIN and EFFORT at the antenna, SIGHT at the eyes, MEMORY at its back. Tap
  one to turn it: the agent pops, the antenna ball swaps size, the memory cards or the notebook
  appear on it. A dial the errand has not opened wears OPENS AT 1.x instead.
- **Pocket money is a wallet**: ten coins of 10k, tap to take that many; the gauge under it
  says how far they go.
- **Speed is always the fastest.** The 1x/2x/4x control left the run; it is in Settings with
  the turns an errand lasts, LUMA's voice, the sound and the roster.
- The editor keeps the one thing the player writes: the list, with "Build it from parts" for
  those who want the shelf.

Frames: `games/ai-arena/Builds/look-audit/garage/` (1.1 with the ready list, 1.2 with the
broken one, Settings, the capstone with every tag open, a brain switched, seven coins, the run
and the debrief) and the walk in `text/`.

---

# Phase 5b: Robot Racing, the pit lane

6 November 2026, the garage's cuts applied to the circuit.

- **The robot runs by itself.** The three runs (Watch the pro, Fix the broken bot, Your run)
  are gone as a control. A level opens with its setup already in the pit lane: the broken one
  on a fix-it level (no points at all on 1.1, the sensor missing on 2.1, the loophole on 2.4),
  the ready one where there is only that, the fixed defaults on the capstone. LUMA says once,
  under the car, why it works or what is wrong with it. On the three levels whose mistake needs
  two robots (never again / forever, the scraper / the careful one, run A / run B) the second
  robot still trains after the player's, unchanged, so the two curves can be compared.
- **The settings live on the car.** The seven sections are tags in two columns beside the car,
  each with a line to the part of the car it is (senses at the nose, moves at the front wheel,
  points on the roof light, knobs at the back, new moves at the rear wheel, tracks and the
  measure on the ground); a setting the level has not opened reads OPENS AT 2.1 instead. The
  sheet on the right holds only what this level opens: nothing on 1.1, the points table on
  2.3, everything on the capstone, and the prediction when the level asks for one. The car
  pops when a setting moves. The portrait camera came in closer and lower for the car.
- **Speed is always the fastest.** The 1x/10x/100x/Max control left the run; it is in Settings
  with LUMA's voice and the sound. The race still drops to 10x so it can be watched.
- The run notes, the dial blurbs, the hint on fixed settings and the settings summary under
  the car are gone with the cards they sat on.

Frames: `games/robot-racing/Builds/look-audit/pit/` (1.1 with nothing to set, 2.3 with the
points table, the capstone with everything, Settings, a run, the debrief) and the walk in
`text/`.

---

# Phase 5c: Tech Escape, the room

6 November 2026, the same cuts applied to the facility.

- **The facility runs the attack by itself.** The three runs (Watch the pro, Fix the broken
  room, Your run) are gone as a control. A level opens with the room as it was handed over,
  the planned mistake in it (fix it, or run the attack and watch it get through); a room with
  only a ready setup opens with that; a room the player already ran comes back as they left
  it. LUMA says the mistake and why it fails once, under the room. One run per level; it
  always counts, and it goes to the record.
- The HUD names the mistake in four words ("Fix it: An asset nobody listed") and nothing more;
  the attack's and the debrief's headers lost the run's name.
- The room keeps what is the game here: the system's rows and their chips, the lamps on the
  rows and on the wall. There is no speed to hide and one robot's worth of settings, so no
  Settings sheet.

Frames: `games/tech-escape/Builds/look-audit/room/` (1.3 as handed over, 1.1, the attack's
end, the debrief) and the walk in `text/`.

---

# Phase 6: realism pass (Tech Escape pilot)

7 November 2026, branch `realism-pass`, from `Docs/realism-pass-prompt.md`. The previous
passes lit, graded and framed boxes; this one changes what the world is made of. Step 1,
Claude Bomb measured, is `Docs/claude-bomb-recipe.md`: URP Lit everywhere, textured surfaces
tinted to the palette, one detailed hero at about a metre filling about 40 % of the frame, a
real room that recedes into exponential fog, a warm key over the hero and the facility's own
lamps for the rest, neutral tonemapping with bloom 0.25 and a 0.2 vignette, a camera-feed
frame. Step 2, the pilot: one room (the Suspicious Inbox, level 1.3), the corridor and the
attacker. The other eleven rooms keep their old interiors until the pilot is approved.

## What changed

1. **A facility kit made in the Editor** (`Assets/Editor/FacilityKit.cs`,
   `FacilityKit.Prefabs.cs`, menu Tech Escape > Build facility kit), the way Claude Bomb's
   `LabBuilder` makes Bay 3: URP Lit materials from CC0 texture sets (Poly Haven ARM maps split
   into metallic-smoothness and occlusion), architecture as boxes with UVs in metres (a 4.2 m
   bay tiles a 2.1 m texture seamlessly), and prefabs in `Resources/Facility`: three corridor
   bays with their props, the entrance, the exit wall with a pair of doors and a lit EXIT
   sign, the outside ground, a door (the hero leaf on a hinge, a painted steel frame, the
   status lamp, an enamel sign plate, a badge reader), a lamp for the rooms' boards, the post
   room, and the attacker. `Building` places them from the level data; nothing it places in
   the corridor, the doors, the exit, the post room or the attacker is MeshKit or Flat.
2. **Six hero models from Higgsfield**, approved list, 128.75 credits: the hooded attacker
   (rigged, with idle, a sneaking walk and a step-and-push for forcing a door, one Animator
   per clip because the three rigs do not share bone orientations), the mail sorter (the
   room's misconfiguration prop), a server rack with LEDs and cables, a monitor, a security
   door leaf (its stray closer arm clipped) and an office chair. Each was split into a
   geometry-only GLB and URP maps (base, normal, metal-smoothness mask, emission where it
   glows), downsized to 2K (attacker, sorter) or 1K, scaled in metres, pivoted at its base and
   turned to face +Z. Credits, job ids and sources: `games/tech-escape/Docs/credits.md`.
3. **The post room**, 3.6 × 3.4 m and 2.8 m high, closed: linoleum floor, painted plaster
   walls with skirting and cornice, a ceiling with a fluorescent fitting, the mail sorter
   against the back wall under a warm key spot, "POST ROOM" stencilled beside it (Claude
   Bomb's "BAY 3"), the board of decision lamps under the stencil, the rack in the corner,
   the desk with the monitor showing the inbox, the chair, parcels on steel shelves, a bin,
   the clock, the fire alarm, a security camera, a poster. `RoomLife` makes the rack's LEDs
   wink, the screen's glow waver and the tube stutter.
4. **The room is seen from inside**, at shoulder height (1.5 m, 48° field of view, a degree
   of drift), framed as a security camera's feed: corner brackets, a blinking REC dot,
   "CAM 02 / SUSPICIOUS INBOX", a running clock from 23:47, "PERMIT OFFICE / LOCKDOWN".
5. **The corridor is a corridor**: ceilings (hidden only in the overview, which becomes a
   building section with wall thickness and the fittings seen from above), concrete floor,
   plaster walls with skirting and a dado rail, the twelve doors with frames, lamps, signs and
   readers, extinguishers, alarms, bins, boxes, a wet-floor sign, power boxes, clocks,
   security cameras. The chase camera stays under the ceiling, the opening shot looks in from
   outside the entrance, the end moment sweeps an arc on the corridor side of the door.
6. **Light and air per view** (`Building.Mood`): in the room, an overhead key with shadows
   over 12 m, the warm spot, the cool tube, the screen's glow, exponential fog 0.05 in the
   night colour; in the corridor, a dim cool key, the door lamps, fog 0.04; from above, the
   moon and the old linear fog. The door lamps go off in the room (the web's light budget).
7. **Post as Claude Bomb's**: neutral tonemapping (was ACES), bloom 0.25 at threshold 1.0,
   a black vignette at 0.2, no extra saturation or contrast (`Atmosphere`).
8. **The play screens' panels are dark**: the room's sheet is a dark drawer of 640 px (33 %
   of the width) that folds to a 72 px tab with the rows' lamps, the world keeps the rest;
   the level, the mistake and the buttons moved into the drawer's head; every text there and
   on the attack screen is 16 px or more; the attack's line cards are dark with a coloured
   keyline (they were light salmon and teal fills).
9. **Web budget**: every kit texture is crunched DXT (quality 50), the maps' sizes as the prompt
   asks (2K heroes, 1K surfaces, 512 props). Without it the first web build was 73.6 MB.

## Machine checks

| Check | Result |
|---|---|
| World materials on the kit (229 renderers, 43 materials) are URP Lit with a base map and a normal map | 42 of 43; the one left is the lamp's beam, an additive light effect (VFX, exempt) |
| No Flat shader or MeshKit mesh on the pilot's world | met in the corridor, doors, exit, post room and attacker; not yet in the other 11 rooms' interiors, which the overview still shows; the attacker's floor ring, the trail and the breach puffs are MeshKit/Glass effects |
| The hero fills at least 25 % of the main play screen | the mail sorter's box 36 % × 77 % = 28 % of the room screen (Claude Bomb's device: 40 %); the attacker in the chase 14 % (a moving shot, not the main play screen) |
| No light-theme panel on a play screen | the room and the attack: met (dark drawer 33 % of the width, dark line cards, text 16 px and up); the briefing and the debrief still have white cards (outside the pilot) |
| Under 45 MB transferred, loads and runs on the platform | 46.6 MB transferred (0.6 MB over), loads in 3.6 s and runs (`Builds/look-audit/realism/platform-tech-escape.png`) |

## Web, before and after

| | Text pass (Phase 4c) | Realism pilot |
|---|---|---|
| Transferred | 22.5 MB | 46.6 MB |
| Load (headless Chrome, software GL) | 2.4 s | 3.6 s |
| data / wasm on disk | – / 9.3 MB | 37.0 MB / 9.3 MB |
| Textures in the build (uncompressed) | – | 29.0 MB (88.8 MB before crunch) |
| Meshes in the build | – | 11.8 MB (the attacker's three rigged copies are 6 MB of it) |

## The side-by-sides and the reviewer

`games/tech-escape/Builds/look-audit/realism/side-by-side-*.png`: Claude Bomb on the left
(its play screen, its title shot, its wide orbit), Tech Escape on the right, 1600 × 900
each. Judged by a separate agent given only the images and the prompt's question.

Round 1 (`realism/round1/`): NO on all three. Fixed after it: a visible light source over the
sorter (Claude Bomb's hanging lamp, a beam and dust), conduit and a breaker box on the back
wall, round lit indicator lamps on the board, YOU FIX moved into the drawer so nothing covers
the sorter, long rows and chips wrap instead of being cut, the shot taken after LUMA's line,
the corridor's tubes off, softer door lamps, a stronger key, polished concrete, the attacker
caught walking rather than mid-lunge, its floor ring only from above.

Round 2 (the current images): still NO on all three. What it says still looks cheaper:

- **Room**: the drawer reads as a web form (stacked cards of outlined fields, crowded text,
  the next card cut at the bottom); the back wall is still plain and the sorter is the only
  dressed object; the lamp row reads as a light strip on a bar; the beam is a hard-edged
  cone; the boxes are muddy; the linoleum's pattern repeats in a grid; cropped clutter at the
  left edge; the palette is grey with one red envelope against Claude Bomb's saturated,
  glowing modules.
- **Corridor and attacker**: the WHAT IT TRIED column is a large, mostly empty glass box;
  the ceiling and its fittings look smeared; the light is a flat wash with no contact
  shadows; the floor still reads untextured; the attacker's pose reads stiff; identical
  doors and signs; props float.
- **Attack map**: the other rooms are the old boxes, the facility floats in a void, the
  attacker's path reads as a debug line, the list repeats one room.

Frames of every step: `realism/walk1` to `walk9`, the kit previews in `realism/kit`, the
heroes from four sides in `realism/models`, before and after in `realism/before-after-*.png`.

---

# Phase 6b: Tech Escape, finished

7 November 2026, after the pilot: "finish tech escape". The whole facility on the kit, the
remaining white screens dark, the pilot reviewer's world items fixed, the web build back
toward the budget.

## What changed

1. **Twelve real rooms** (`Assets/Editor/FacilityKit.Rooms.cs`): one builder for every room,
   the same kind of place and a different room each. The shell (concrete floor, painted
   plaster, skirting, cornice, conduit, a ceiling with its fitting), Claude Bomb's hanging lamp
   over the room's prop with its beam and dust, the prop against the back wall, the room's name
   stencilled beside it, the board of decision lamps, an amber lockdown beacon turning in the
   corner (Claude Bomb's beacon, its two beams), and furniture by kind: offices keep a filing
   cabinet and a notice board, labs a rack, shelves and a toolbox; every room a desk with a lit
   screen, a chair, a bin, a clock, an extinguisher, a fire alarm, a security camera. The
   Operations Center's prop is six of the pilot's monitors on a rail, each with its board.
2. **Twelve more Higgsfield models** (the second approved list, 207.75 credits): the rooms'
   props (a rolling board of risk cards, a floor safe, a glass isolation chamber, a badge
   turnstile, a workbench of small servers, a wall network cabinet of tangled cables, a console
   of toggle switches, a lockbox with its key, a cloud lamp, a tape library) and a filing
   cabinet and a notice board for every room. `games/tech-escape/Docs/credits.md`.
3. **Each room framed by measurement**: the kit projects the prop's box through a camera at
   shoulder height and keeps the farthest distance where it covers about 30 % of the frame
   without spilling out of the feed; the stencil and the board go only where that camera sees
   them. In the game every room measures 30 to 31 %.
4. **The other screens**: the debrief on dark glass (it was white cards), the warm-up question
   on dark glass, every small-caps label and the ladder at 16 px, the attack's column as tall as
   its lines, the corridor's tubes replaced by recessed panels that belong to the ceiling (not
   seen from above), reflection probes for the corridor and the room on screen rendered once
   the lamps are on.
5. **Budget**: kit textures crunched; metal-smoothness maps dropped where a model has almost no
   metal (one smoothness instead); the rooms' props' normal and mask maps at 512 (their colour
   at 1K); the Poly Haven meshes compressed.
6. **The pilot reviewer's world items**: the octagon linoleum replaced by the polished concrete
   (it read blurry and oversized); the board of lamps never jammed against the top of the feed
   (it goes on a side wall when the view has no room for it beside the prop); the corridor
   brighter (a stronger cool key, a lighter ambient, the ceiling panels faintly lit).

## Machine checks

| Check | Result |
|---|---|
| Kit materials (1,144 renderers, 62 materials): URP Lit with a base map and a normal map | 61 of 62; the one left is the lamps' beam, an additive light effect (exempt as VFX) |
| No Flat shader or MeshKit mesh on the world | met for the corridor, every door, the exit, all twelve rooms and the attacker; the Flat/MeshKit code stays only for effects (the trail, the attacker's ring from above, the breach puffs) and as the fallback when the kit is missing |
| The hero fills at least 25 % of the main play screen | every room 30 to 31 % (the mail sorter 38 % × 82 %; the cloud lamp and the tape library 31 % × 100 %, cropped slightly); Claude Bomb's device: 40 % |
| No light-theme panel on a play screen | met: room, attack, briefing, warm-up and debrief are dark glass; every small-caps label at 16 px |
| Under 45 MB transferred, loads and runs on the platform | 44.5 MB transferred, 3.7 s to load, runs (`realism/platform-tech-escape-full.png`) |

## Web, before and after

| | Text pass (Phase 4c) | Pilot | Tech Escape finished |
|---|---|---|---|
| Transferred | 22.5 MB | 46.6 MB | 44.5 MB |
| Load (headless Chrome, software GL) | 2.4 s | 3.6 s | 3.7 s |
| data / wasm on disk | – / 9.3 MB | 37.0 / 9.3 MB | 34.8 / 9.3 MB |

The first full build was 53.9 MB; the metal-smoothness maps (9 MB in the build) were the
biggest single cost, most of them on surfaces with no metal in them.

## The side-by-sides and the reviewer

`games/tech-escape/Builds/look-audit/realism/final2/`: seven screens, Claude Bomb on the left
(its title, its briefing transmission, its play screen twice, its room view, its wide orbit,
its title panel against our debrief). Two rounds with a fresh reviewer each (`final1`, then
`final2` after the fixes above). **Round 2: NO on all seven.** What it still says looks
cheaper, grouped:

- **The interface, on every screen**: the room's drawer reads as a form or a spreadsheet
  (Verdict / The reason / The report rows), labels cut with ellipses, the list running off the
  bottom; the briefing is a dashboard of blocks (the 12 room chips, the 19-chip ladder); the
  title's level map reads as a progress tracker; the debrief's panels are half empty and the
  medal is three grey circles without art; LUMA's line is sometimes caught mid-word; mixed
  button cases; translucent panels let the scene show through and look muddy.
- **The world**: the lamp's beam is a hard-edged wedge; the ceiling panels light nothing; the
  floor and walls read plain next to Claude Bomb's device; the dust reads as specks; the
  attacker's walk is caught in a stiff pose; the wire bins render as solid jars; door signs
  are soft at a distance; the attack map is a small diorama in a dark void with a flat red
  route.
- **What already holds**: the rooms' props (the mail sorter, the safe and the others) stand
  next to Claude Bomb's device.

The interface items go beyond what the world is made of: they are the screens' design, which
the prompt keeps (forms collapse to a side drawer, never wider than 35 %), and they are the
reason no screen passes. Frames: `realism/game4` (the walk with every room), the kit's renders
in `realism/kit`.

---

# Phase 6c: Robot Racing

7 November 2026, run in parallel with AI Arena by its own agent ("can u run all remaining
games in parallel?"), on the approved list (cap 100 credits). Full report:
`games/robot-racing/Docs/realism-report.md`; assets and jobs: `games/robot-racing/Docs/credits.md`.

## What changed

1. **A track kit made in the Editor** (`Assets/Editor/TrackKit.cs`, `TrackKit.Prefabs.cs`),
   on Tech Escape's FacilityKit pattern: 25 prefabs in `Resources/Track`, URP Lit from CC0
   sets (Poly Haven, ambientCG) and textures painted in the project, crunched. `Circuit.Kit.cs`
   places them from each track's grid; without the kit the old blocks are built as before.
2. **A real small circuit**: asphalt with painted lines, red and white kerbs at every bend,
   jersey barriers with a hazard band, tyre walls with a belt, the start gantry with its
   lights, the pit wall with a chain-link fence, the pit lane and a pit box (Claude Bomb's tool
   chest, cart, extinguisher, barrel), cones, the grandstand, a chequered finish with flags,
   floodlight masts, trees of bark and leaf cards, grass with patches, a sky dome.
3. **The car is a hero model**: a Higgsfield body with four separate Higgsfield wheels, the
   sensor pod, glowing eyes and antenna, the "07" roundel. `CarRig.cs` spins the wheels with the
   distance, steers the front pair, leans the body out of a bend and squats it at speed.
4. **Evening practice under floodlights**, fog in the dusk colour, a reflection probe; post as
   Claude Bomb's (neutral tonemapping, bloom 0.25 from 1, vignette 0.2).
5. **The camera low and close by default** (the pit lane in front of the car, the run's chase
   just behind it), the overview as the exception.
6. **Dark play screens**: the run and the pit lane in a 640 px dark drawer (33 %) that folds
   to a tab; the debrief, its questions, the warm-up and the settings on dark glass; every label
   16 px or more; the title in a trackside camera's frame (LIVE, CAM 3).

## Machine checks

| Check | Result |
|---|---|
| Kit materials (87 renderers, 33 materials): URP Lit with a base map and a normal map | 33 of 33; the sky dome's gradient is exempt like a skybox |
| No Flat shader or MeshKit mesh on the world | 0 on the title, briefing, pit lane, run, race and debrief (sensor beams, trails and ghost lines are effects) |
| The hero fills at least 25 % of the main play screen | pit lane 30 % (2.3) and 33 % (1.1), training run 31 %, race 31 %; Claude Bomb's device: 40 % |
| No light-theme panel on a play screen | 0 |
| Text at least 16 px at 1080p | 0 under 16 px on all six screens |
| Under 45 MB transferred, loads and runs on the platform | 28.5 MB, 5.0 s, runs (`realism/platform-robot-racing.png`) |

## Web, before and after

| | Text pass (6 November) | Realism pass |
|---|---|---|
| Transferred | 20.5 MB | 28.5 MB |
| Load (headless Chrome, software GL) | 3.3 s | 5.0 s (median of three) |
| data / wasm on disk | 10.8 / 9.3 MB | 18.8 / 9.3 MB |

## Credits

79.25 of the 100 cap (5 design images at 0.25, three detailed 3D models at 18, two standard
at 12), no retries; matches the account's transactions of 7 November 2026.

## The side-by-sides and the reviewer

`games/robot-racing/Builds/look-audit/realism/final/`: seven screens (title, briefing, pit
lane at 2.3 and 1.1, training run, race, debrief), Claude Bomb on the left. One fresh reviewer,
given only the images and the question. **NO on all seven.** What it says looks cheaper,
grouped:

- **The interface, on every screen**: the drawers read as an inspector (the points table as a
  6 x 4 grid of number buttons, the rule book's rows, sensor bars, "seed 7"); the briefing is
  nested cards (LUMA card, track card, seven part chips, the 13-chip ladder); the title's level
  map reads as a web stepper; the debrief is six boxes like an analytics dashboard with an empty
  chart as its largest element and the medal as three grey dots; two button styles in one row;
  the pit lane's eight callouts with leader lines read as debug gizmos (and show the parts that
  open at later levels on 1.1); about 120 words on the pit lane against Claude Bomb's 40;
  translucent panels muddy with the scene behind them.
- **Text cut or caught mid-line**: LUMA's line on the title and the pit lane, "lap time 0 m..."
  on the race HUD, the rule book running off the bottom.
- **Empty states as the hero**: the run's chart with nothing plotted, "REWARD 0 / BEST 0 /
  LAP -", the debrief's flat line at 0.
- **The camera**: the training chase so low and close that the car's rear deck fills half the
  view and its soft texture shows; the race frame pressed into the car facing a barrier, no
  track ahead.
- **The world**: the red and white barrier texture stretched and blurry (the most visible
  material failure); flat grey walls and jersey barriers; the headlight pools and the teal
  floor stripe clipping to white; the wheel arches black voids from the front; flat dusk sky,
  low-detail trees.
- **What already holds**: the car model and the dusk circuit in the pit lane's hero shots.

Some of the strings it names are what the game teaches and stay as they are (the rule book's
"front wall, left wall, right wall, speed stopped" are the words the robot reads); others are
display problems to fix (the cut HUD line; the debrief's run line "settled {n} of {attempts}",
which reads "settled never of 120" when the curve never settled, wording older than this pass;
the score line shown twice on the debrief). As with Tech Escape, the interface is why no
screen passes.

---

# Phase 6d: AI Arena

7 November 2026, run in parallel with Robot Racing by its own agent, on the approved list (cap
220 credits). Full report: `games/ai-arena/Docs/realism-report.md`; assets and jobs:
`games/ai-arena/Docs/credits.md`.

## What changed

1. **A market kit made in the Editor** (`Assets/Editor/MarketKit.cs`, `MarketKit.Prefabs.cs`),
   on Tech Escape's FacilityKit pattern, prefabs in `Resources/Market`: the square of cobbles
   with sandstone bands laid between the arena's cells (the grid the lesson needs, in stone),
   a ring of 24 houses (the two Higgsfield houses alternating and mirrored), the Higgsfield
   fountain in one corner, street lamps, planters, benches, crates; two Higgsfield stall styles
   with a lantern under each awning; every item's goods heaped in the Higgsfield crate and piled
   on the counter (Kenney's Food Kit for 14 of the 15 items, Poly Haven's potatoes). Without the
   kit every path falls back to the old boxes.
2. **The agent is a rigged robot** (Meshy, 1.2 m, 15,559 triangles, 2K maps, emissive eyes;
   `World/AgentRig.cs`): its coral panels are a mask so each team's colour paints them; it walks,
   looks around on every LOOK, reaches into the stall to buy (the piece flies into its basket),
   holds its head when it is sent home. One rigged copy per clip, as Tech Escape's attacker.
   Meshy's idle clip swung the hips through about 90 degrees (unusable on the one retry too); the
   look clip, slowed, is the idle.
3. **To scale**: a cell is 2.4 m, a step 1.4 s at the fastest speed, and the next turn waits for
   the walk (pacing only: no rule, turn or score changed).
4. **The camera**: the main view of a round is the street, behind the agent at its shoulder; a
   second view is the agent's eye, framed like Claude Bomb's camera feed (brackets, REC); the
   market from above is a third button, only when asked for. The title shows the agent close in
   the lantern-lit evening square.
5. **Light and post, Claude Bomb's**: neutral tonemapping (was ACES with grading), bloom 0.25
   from 1, vignette 0.2, haze; dusk on the title and the story, day on the rounds.
6. **Dark play screens**: the brand's ink glass everywhere; the run's drawer 31 %, the garage's
   33 %, the decide errand's 34 % (it was a white sheet over 96 % of the screen); the run's and
   the garage's fold to a tab; every text clamped to 16 px at 1080p.

## Machine checks

| Check | Result |
|---|---|
| Kit materials (183 renderers, 29 materials): URP Lit with a base map and a normal map | 29 of 29 |
| No Flat shader or MeshKit mesh on the world | Flat 0; one MeshKit mesh, the ring that flashes under the agent on LOOK (an effect); the sight square and the memory's cards are on the Glass shader (effects) |
| The hero fills at least 25 % of the main play screen | the street 31 %, the garage 30 %; the agent's eye 9 to 17 % (it looks past the agent, by design); Claude Bomb's device: 40 % |
| No light-theme panel; the world at least 65 %; text at least 16 px at 1080p | 0 light panels; the world 69 % on the run, 67 % in the garage; smallest text 16 |
| Under 45 MB transferred, loads and runs on the platform | 32.8 MB, 3.1 s, runs (`realism/platform-ai-arena.png`) |

## Web, before and after

| | Before (the platform's build, 7 November) | Realism pass |
|---|---|---|
| Transferred | 20.1 MB (the text pass recorded 19.2) | 32.8 MB |
| Load (headless Chrome, software GL) | 5.4 s with a Unity build alongside (the text pass recorded 3.7) | 3.1 s |
| data / wasm on disk | 10.4 / 9.3 MB | 23.1 / 9.4 MB |

The four rigged copies of the agent are 5.6 MB of the build's 8.5 MB of meshes.

## Credits

181.75 of the 220 cap (7 design images at 0.25, Meshy image to 3D rigged 38, five detailed
Tripo models at 18, one standard at 12, five rigging clips at 8, one of them the idle retry);
matches the account's transactions of 7 November 2026. Robot Racing and AI Arena together:
261 of the 320 approved.

## The side-by-sides and the reviewer

`games/ai-arena/Builds/look-audit/realism/final/`: eight screens (title, story, briefing,
garage, market run, the agent's eye, decide, debrief), Claude Bomb on the left. One fresh
reviewer, given only the images and the question. **NO on all eight.** What it says looks
cheaper, grouped:

- **The interface, on every screen**: the garage's drawer reads as an order form (12 swatches,
  a name field, 15 items marked BUY or NEVER, the wallet's ten coin buttons, DEPLOY); the
  decide errand as a quiz of 15 identical buttons with an empty gap; the debrief as a
  spreadsheet of rank, score, ticks, tokens, dollars and model, with the medal as the word GOLD
  and three dots; the run's agent cards are a muddy maroon with coordinates, token counts and
  bars; three groups of controls on the run; the title crowded with the level strip, three
  buttons and two toggles; the story's centre a flat 2D slide with web-tab steps and carousel
  dots; the briefing's message box and grey part buttons read as empty and disabled fields;
  tab styles that do not match; panels see-through enough to muddy.
- **What it reads as glitches**: the see-through pink squares beside the agent's head (the
  memory dial's glass cards), the coloured discs and faint "?" shapes in the agent's eye (most
  likely its sight and the unknown markers), green blobs at its feet (most likely the LOOK ring), the agent's own antenna
  poking into its eye view, a name tag with no agent under it, the corn cob in the basket
  passing through the shoulder.
- **The world**: the rounds lit flat like noon against the studio's navy (only the title and
  the story are at dusk); the stalls empty on the briefing and the decide screen (the goods come
  with a round); the cobbles blurry and repeating close to the camera, the sandstone grid lines
  visible; no people; no garage set (the garage is the square); the agent lit flat on the title,
  with no rim and no glow from its eyes.
- **Text**: LUMA's line caught mid-typing on the title and the decide screen; the title's setup
  lines ("Claude not connected: agents will use the built-in stand-in brain", the install line).

Some of what it names is what the game teaches and stays: the memory cards, the sight square and the
unknown markers carry the lesson (their look can change, their meaning cannot); the agent's
thought ("Stand-in brain: heading for the banana at (3, 2)") is the brain's own line, and it
reads as the stand-in's because the screenshot machine has no Claude sign-in, which is also
why the title shows the setup lines; the tokens, the cost and the model on the debrief are the
lesson about the brain and the effort dials. The reviewer's "12 to 13 px" is the 16 px text in
frames scaled from 1080 to 900 px tall. The fixable ones are the corn's scale and placement,
the antenna in the eye view, the orphan name tag, the noon light and the empty stalls on the
screens before a round.

---

# Phase 6, where the three games stand

All three games are on Editor-made kits of CC0 and Higgsfield models, with every machine check
met and every web build under 45 MB (Tech Escape 44.5, Robot Racing 28.5, AI Arena 32.8).
Higgsfield for the pass: 597.5 credits (Tech Escape 336.5, Robot Racing 79.25, AI Arena
181.75). The reviewer says NO on every screen of every game, and the first reason is the same
in all three: the screens' design (drawers as forms and inspectors, briefings and debriefs as
dashboards of boxes, chip strips, empty states as the hero, two button styles), which the
realism prompt keeps as it was and only moves into drawers. The world's remaining items are
second. Closing the gap needs an interface pass across the three games, which is a decision
for the owner, not this pass.

---

# Phase 7: game-feel pass

7 November 2026, from `Docs/game-feel-pass-prompt.md`, after the look was approved. Design
first (`Docs/game-feel/`, approved with the owner's answers), then the Robot Racing pilot:
levels 1.1 and 2.3 rebuilt end to end on the new branch `game-feel-pass`. The other levels and
the other two games wait for the pilot's approval.

## What changed in Robot Racing (the pilot)

1. **Verbs, not forms.** The rewards are markers placed on the track (`World/Markers.cs`): the
   chequered flag on the finish, three cones against the wall (a crash), teal chevrons along the
   way (each step toward the finish), a red one pointing back (each step away), a clock post
   (each moment), an orange sign (the speed bonus). Each shows its number on a plate; drag one
   from the shelf on the pit lane onto its place, tap it to turn the number, drag it back for
   zero. On 1.1 the flag alone, which turns the points on as a whole (the rest return to the
   ready robot's values); from 2.3 the shelf. No physics: picked by projection, dragged along
   the pointer's ray on the ground.
2. **The world shows the state.** Every attempt leaves its ghost line; each move's points float
   up from the car; the attempt budget is a fuel gauge with a needle; the lap is on a board by
   the grid and at the top; the start lights under the gantry run the countdown; a crash is
   sparks and a shake; the race ends with the camera on the car at the line.
3. **The montage.** The training is computed first (`Core/Replay.cs`) and played back: every
   attempt at race speed, a thirty-second montage that slows for the first attempt, the first
   finish, the settle and the last, or straight to the end; "Skip ahead" is the run's main
   action, the pace is in the menu.
4. **The briefing is a transmission**: the car on the grid in the camera feed, three lines
   from race control typed in, one button.
5. **One line, one menu.** An objective of at most eight words at the top left; Esc or one icon
   opens the menu (`Ui/GameMenu.cs`: the briefing, the file, this level again, the title, the
   games menu, sound, voice, pace). Briefing, The file, Settings, Title screen, Follow/The
   track, Pause, Stop race now, Hide left the screens.
6. **LUMA says one short thing when something happens**: "Nobody is steering." on attempt 1,
   "One finish proves nothing: watch whether the curve stays up." at the first finish, "That is
   learning, not luck." at the settle, "The race, with the rule book as it is." at the start
   lights, "Every move earns zero." after a run with no finish, "Turn the finish points on and run
   again." back in the pit lane, the first sentence of the takeaway on the debrief. Shown whole,
   never typed mid-sentence, 15 words or fewer, recorded like her other lines. Her full lines
   and the curriculum are in the file.
7. **The questions are taps on the world** (`Ui/QuizScreen.cs`): the car, the track, the sensors'
   rays, the wheels, the points floating over the track (lit teal while the question is up), or
   three signs beside the grid. Stored as the chips stored them.
8. **The debrief is the car at the line**, the lap board, the medal with its stars, LUMA's one
   line, Next and Again; the curve, the checks, the runs' numbers, the stars in all and the rank
   behind Details.

## The four bugs

| Bug | Fix |
|---|---|
| 1.1 could not be won (its points were locked at zero and the one control was missing) | The flag; on the finish it turns the points on as a whole. The walk's 1.1: first finish at attempt 18, race 5 of 5, GOLD. |
| Training invisible at the default speed (over in one frame) | The montage, about 30 s whatever the budget; Max stays in the menu. |
| Tech Escape 2.4's allow-all rule below the drawer's fold | The drawer head lost the "You fix" paragraph and the step note (both are in the file and in LUMA's line); two decisions on a row sit side by side. All five rules fit. |
| Tech Escape's attack banner gone before LUMA finished her line | The banner waits until she has finished typing and speaking (`Luma.Busy`). |

Also fixed on the way: the warm-up question is off in the three games; the "Reason" label no
longer clips.

## Words and controls, before and after

Counted the same way before (the inventory, `Docs/game-feel/inventory/robot-racing.md`) and
after (the walk's `counts.jsonl`): every visible word with a letter or digit, every button the
player can press; Claude Bomb's yardstick counted the same way is 78 words on its briefing
and 46 to 68 on its play screen (4 control groups plus the chat).

| Screen | Before: words / controls | After: words / controls | Limit |
|---|---|---|---|
| Title | 84 / 17 | 61 / 15 (the level map is the one ladder) | – |
| Briefing 1.1 | 98 / 15 | 46 / 3 (Skip, the menu, the button) | 50 |
| Pit lane 1.1 | 125 / 5 | 6 to 8 / 2 (the menu, Train) | 30, 3 + main |
| Training 1.1 | 203 / 6 | 9 to 15 / 4 (Camera, Details, the menu, Skip ahead) | 30, 3 + main |
| Race 1.1 | 188 / 6 | 15 to 19 / 3 | 30, 3 + main |
| Race over 1.1 | 216 / 7 | 10 / 4 (Debrief is the main action) | 30, 3 + main |
| Questions 1.1 | 146 / 32 | 10 and 22 / 1 (the menu; the answers are taps) | 30 |
| Debrief 1.1 | 127 / 5 | 15 / 4 (Details, Again, the menu, Next) | 30 |
| Debrief 1.1, Details open | – | 120 / 6 | – |
| Briefing 2.3 | 113 / 15 | 47 / 3 | 50 |
| Pit lane 2.3 | 170 / 28 | 23 to 26 / 2 | 30, 3 + main |
| Training 2.3 | 190 to 235 / 6 | 14 to 25 / 4 | 30, 3 + main |
| Training 2.3, Details open | – | 234 / 5 (the seed and the chance live here) | – |
| Race 2.3 | 224 to 227 / 6 | 16 to 20 / 3 | 30, 3 + main |
| Debrief 2.3 | 147 / 5 | 16 / 4 | 30 |
| The menu | – | 41 / 15 | – |

Every LUMA line on every screen of the walk is whole and 15 words or fewer. No "OPENS AT",
no coordinates, no seed and no probability on any play or briefing screen (the seed and the
new-move chance are behind Details, where the walk flags them on purpose). The car fills 36 %
of the frame in training and 26 % in the race (the chase pulled back so the track ahead
shows); in the pit lane the view is fitted to the markers, not the car.

## The reviewers, round 1

Two fresh agents, given only the screenshots (`games/robot-racing/Builds/look-audit/game-feel/final/`).

**The player test** (level 1.1's eleven screens in order, "what is it about, what do you do on
each screen, where did it feel like a form"): it said what the game is about in one paragraph
("You do not drive... give it points, press Train, watch it crash, crash later, find the line,
then it races five laps and you get a medal") and what to do on nine of eleven screens. It
could not tell on two: the 1.1 pit lane ("nothing shows where or how to set the points; the
flag never looked touchable") and the first question ("LUMA's line is a statement, nothing to
tap"). Screens that felt like a form or instructions: the briefing (an instruction page, "the
right kind, with a Skip"), the signs question ("a quiz, nicer than radio buttons, still a
comprehension test"), and half the title (the tier grid "feels like a syllabus"). Its plain
summary: "what I actually did was tap a flag I did not know was tappable, press Train, watch
three screens, press Debrief, tap a sign, press Next"; the red-then-teal ribbons "teach the idea
without a word"; "if the flag glowed or wobbled, or the hint said Tap the flag, the one act of
play would land."

**The Claude Bomb test** (the side-by-sides): RIGHT on the pit lane, the training, the race
and the debrief ("a chase camera on a car on a track with a gauge and an objective line is a
racing game"; "the most game-like frame of the set"; "a finish-line shot between tyre walls
with a 5/5 board and a GOLD callout"), LEFT on the title ("a course outline") and the questions
("a multiple-choice question staged on billboards"), EVEN on the briefing ("the right's 3D scene
is more convincing; the left's text gives the player a role and a clock"). What still reads as an
app, most visible first: the title's tier map; LUMA's strips read as a tutor's captions ("Nobody
is steering.", "No new moves, no learning.", the takeaway); the button rows (Camera/Details,
Again/Next/Details) and the menu icon; the floating numbers with no HUD to belong to. Its three
changes: rebuild the title around the car with the level select in the world; LUMA as a radio
call with stakes, six words or fewer, and a fixed HUD with a timer or position; in-world
triggers instead of buttons, and the question asked by the race itself with a consequence.

Acted on before round 2: the flag now pulses with a teal lamp from the start and the objective
reads "Put the flag on the finish line." until it is placed; the five things a parts question
can mean glow teal while it is up; the race ends on the car at the line, not the overview; the
race cue became "The race, with the rule book as it is."; the shelf moved up the pit lane, clear
of the gantry, and the kit's own cones hide while the crash marker is there.

## The reviewers, round 2

The same two tests, fresh agents, on the frames after those fixes.

**The player test**: it said what the game is about ("A robot race car that nobody drives. I
don't steer it; I put down a reward, the car crashes into walls until it figures out the route,
then it runs a five-lap race, I answer questions, I get a medal") and what to do on ten of
eleven screens. The 1.1 pit lane now lands: "I'd drag the checkered flag up to the finish, then
press Train. This is the first thing that feels like touching the game, and it's
understandable." The race over and the debrief "feel like a game's results screen". The one
screen it still could not read is the first question ("Tap the answer" with "The one making the
decisions.", "a sentence fragment, not a question", the glowing car and the "+2" not read as
choices). Still a form or instructions by its account: the briefing ("a reading page, short,
skippable"), the signs ("the nicest-looking quiz I've seen, but a comprehension check"), and the
title's left half ("a course dashboard"). Not forms, not play either: the training and the race
("my only real act in the world is dragging one flag; the game's premise is nobody drives, and
it's honest about that, but the player's hands are idle for most of the level"). "Rule book" in
the race cue is "a phrase nobody introduced".

**The Claude Bomb test**: RIGHT on the pit lane, the training, the race, the questions (on looks:
"no chat panel, only the track from above; but it is a multiple-choice question") and the
debrief ("a medal reveal over the car in a tyre-walled pen"); EVEN on the briefing; LEFT on the
title ("the TIER 1/2/3 node grid turns the opening screen into a course map"). "The right side
feels more like a game than the reference in five of seven screens." What still reads as an app
on every right-side screen: the teal main button in the bottom-right corner (Train, Skip ahead,
Next, To the pit lane, Continue), LUMA's sentence box, the small buttons (Camera, Details, Again,
Skip), the menu icon; and the floating "-100" / "+2" and "GOLD" as text rather than objects. Its
three changes: a title with no node grid and the level select in the world; one key or a
diegetic trigger instead of the main button and the pills; the points and the medal as objects
(gates that light, a scoreboard, a podium).

## Where the pilot stands

The pilot meets the prompt's machine checks on every screen and passes the player test on all
but one screen; the Claude Bomb test prefers it on five of seven. What both reviewers still hold
against it is largely what the prompt keeps (the ladder on the title, three controls plus one
main action, LUMA's one line, questions on the world) or what the game is ("nobody drives": the
player places and watches). The owner's call, not the pass's: the level map on the title (the
brief keeps it there); the first question's prompt, which is the curriculum's wording; buttons
versus key prompts; and whether to turn the floating points and the medal into objects (a lit
gate, a podium) in the next round.

## Web

Robot Racing: 31.2 MB transferred (was 28.5; the recorded cues and the new code), 5.0 s to
load, runs on the platform. Tech Escape with its two fixes: 44.5 MB, 6.8 s. Both measured
with the realism pass's `loadtime.mjs` in headless Chrome with software rendering. The frames of
the pilot, the counts and the side-by-sides are in `games/robot-racing/Builds/look-audit/game-feel/final/`.

---

# Phase 7b: the rest of Robot Racing

7 November 2026, after the pilot's approval. The owner asked for four fixes while the other
eleven levels were built, and settled the open questions: race control's points show dim on
1.1 once the flag lands; computed-then-replayed training stays as long as the results and the
platform report are identical; one big Continue and a small Levels on the title, the level map
as a race calendar; buttons stay for the main actions; a guess on the fuel gauge before each
training, LUMA reacting to how close it was. Everything below is on branch `game-feel-pass`
(the thirteen levels as `932b511`, the second four fixes as the commit after it).

## The four fixes

| Asked | Done |
|---|---|
| Questions: the question itself as the objective line, the tappable things glow, LUMA only after the answer | The objective reads "Who makes the decisions?", "What does the robot know at the start?", "What would the robot do here?" (`QuizScreen.Short`). The answers stand on one signpost beside the grid, stacked plates with a teal halo that pulses while the question is up; a parts question lights the car, the track, a ray, a wheel and the "+2". A prompt longer than the line stands on a board. LUMA says "Right." or "Not quite." after the tap, nothing before. |
| 2.3's placed numbers never overlap each other or the props; the camera lower and closer while placing | The plates face the camera and lift clear of each other every frame (`Markers.Unstack`: the lowest on the screen settles first, each higher one lifts until clear, the size taken from the text's height in the world). The placing view is 22° down instead of 30°, fitted to the car, the shelf and the markers' places with half the margin, with headroom for the plates; 38° when the track cards are out, so the gantry does not hide them. |
| Training's floating dots must read as points from the car, or go | They rise off the car's roof in the world on a thin thread of their colour (`Circuit.PointsFloat`), scale up as they leave and fade; the confetti at a training finish went (a pulse and a chime stay; the race keeps its confetti). |
| Debrief: a medal model rising on a podium, the stars part of it | A podium beside the car where the race ended (`Circuit.Podium`): three steps numbered 1, 2, 3; the medal rises out of its step (gold on 1, silver on 2, bronze on 3) on a ribbon, its three stars in an arc over the disc, lit as earned, popping one by one; the camera stands behind the car on the podium's side. No model was bought: it is built from the kit's materials. |

## Every setting as a thing in the world

`World/Markers.cs` now holds every setting a level opens (`PitScreen.Kinds` says which):

- **The rewards** on the shelf by the pit wall, as in the pilot. On 1.1 the flag alone; once it
  is on the finish, race control's crash, toward, away and moment stand on the track at the
  ready robot's values, darkened, with grey plates, and cannot be touched (they write nothing;
  the flag turns the points on as a whole, as before). From 2.3 they are the player's.
- **The dials** on the shelf, tapped to turn: the fuel can (the attempts: 120, 250, 400, 1000,
  50) with a red cap on 1.2 that stops the training at the first finish (the level's planned
  mistake; a tap takes it off); the die (the chance of a new move: 90 %, 10 %, 50 %) with the
  "never again" cap on 2.5; the fall post (falls, never, slowly, fast, at once) and the floor
  (5 %, 0 %, 50 %); the rewrite dial and the seed cube (3.3; a tap on the seed draws a new one,
  the plate shows it); the stopwatch (3.2: Finishes, Crashes, Touches, Lap time).
- **The parts on the car**: five sensor lamps (nose, two flanks, roof) with their names on
  small plates and the beams drawn long while they are the thing to set (2.1); five move
  chevrons across the rear deck (2.2) and the limiter at the tail (top speed 2 or 1). A tap
  takes a part off (it stays as a grey ghost) or puts it back.
- **The track cards** on the pit lane beyond the shelf (3.1): ten cards, each with its circuit
  drawn on it; a tap turns a card through off, training (teal) and the test track (amber), one
  test at a time, the name on a plate while the card is in use.
- **The guess**: a notch on the fuel gauge's rim, dragged on the pit lane; at the first finish
  LUMA says "Close: you read the robot well.", "Earlier than you guessed." or "Later than you
  guessed."; after a run with no finish, "It never finished. The tank ran dry."
- **The curve in the world**: on the levels whose lesson is the curve (1.2, 2.4, 2.5, 3.3) the
  points per attempt stand as bars on a board by the grid that turns to the camera, and those
  levels train in the overview so the board stays on screen (the owner's rule for numbers that
  are the lesson).
- **The title**: one big Continue, a small Levels that opens the season's calendar (a card per
  round with its circuit and the stars it earned); the tier grid is gone.
- **Two-robot levels** (2.5, 3.2, 3.3) name the robot on the track in the objective line
  ("Never again: watch it learn."); 3.4 ends on "THE REAL RUN · finished / crashed on the oil".
- **The briefing** keeps within fifty words on every level: the lines get thirty-one, the
  track's note going first when it runs over, then the kicker's later sentences; LUMA's stays.
- **The pit lane's cue** is LUMA's first sentence, or the kicker's when hers runs long; after a
  failed run, the first and last sentences of the level's "why" and the planned mistake's thing
  pulses under a teal lamp (the cap, a sensor, a move, the speed sign, a card, the dial).

## Words and controls, all thirteen levels

The walk (`-screenshots`, all levels; `counts.jsonl` in
`games/robot-racing/Builds/look-audit/game-feel/final3/`, the rows of the second four fixes from
`final5/` and `final6/`) counted the same way as the pilot's table. Briefings are within 50 everywhere (37 to 48); every play screen is within 30 but one,
noted below; every screen has at most three controls plus the main action (the run screens'
Camera, Details, the menu icon and Skip ahead / Debrief). Every one of the 106 LUMA lines on
the walk's screens is whole and 15 words or fewer. The seed and the new-move chance appear
only behind Details (flagged there on purpose); no OPENS AT, ladder or coordinates anywhere.

| Level | Briefing | Pit lane (before / after the thing is set) | Training (first, a moment) | Race, race over | Questions | Debrief |
|---|---|---|---|---|---|---|
| 1.1 | 46 | 8 / 11 | 13, 22 | 19, 10 | 5 to 34 (seven questions; the seventh with the board) | 17 (Details: 122) |
| 1.2 | 38 | 20 / 20 (the tank knob, its cap, both named) | 15, 25 | 18, 9 | 23 (the rule book on the signpost) | 12 |
| 1.3 | 46 | 14 | – | – | – | – |
| 2.1 | 39 | 20 / 20 (five sensor lamps, named, their rays) | – | – | – | – |
| 2.2 | 42 | 26 / 26 (five moves, named, the limiter) | – | – | – | – |
| 2.3 | 47 | 23 / 26 | 14, 23 (Details: 228) | 20, 11 | – | 18 |
| 2.4 | 37 | 15 / 8 (the speed sign, the prediction field) | – | – | – | – |
| 2.5 | 47 | 29 / 29 (three knobs and the cap, all named) | 16, 27 | 19, 9 | – | 15 |
| 2.6 | 45 | 23 (everything open; part names on tap) | – | – | – | – |
| 3.1 | 48 | 28 / 30 (ten cards named on their face, the tank knob quiet) | – | – | – | – |
| 3.2 | 45 | 17 / 17 (the stopwatch) | – | – | – | – |
| 3.3 | 40 | 27 / 27 (five knobs, three lit and named) | – | – | – | – |
| 3.4 | 43 | 28 (everything: two rows of knobs, the tank lit; the prediction field) | 15, 24 | 21, 28 (THE REAL RUN) | 17 to 22 (a line of text, then the measure) | 15 (Details: 148) |

The title is 40 words and 3 controls (Continue, Levels, the menu); the calendar 84 words and
18 (thirteen cards, Close, the menu, and the title's buttons beneath); the menu 38 to 55 and
13 to 15. The one screen over 30 is 1.1's seventh question (34): its three answers are the
curriculum's own sentences, and the board above them repeated the question; in the current
build the board shows only for the rule book or a prompt longer than twelve words, which
takes that screen to 26. The pilot's before-and-after for 1.1 and 2.3 stands (above); the
other levels' before counts were not inventoried.

## The second four fixes (8 November 2026)

After the full report the owner asked for four more, all on the pit lane and the debrief, with
the rule that no label in the world floats as white text: every one is the same dark plate.

| Asked | Done |
|---|---|
| The dials on 2.5, 3.3 and 3.4 are unreadable: put the knobs on a small desk at the pit wall, close to the camera, each a real control with a short name plate and its value on its face, the level's knob lit, the rest dim; the seed number stays | A console at the pit wall behind the grid, nearest the camera and clear of the gantry (`Markers.BuildMarkers`): a dark base and a steel panel tipped 32° toward the camera, one row of knobs (two rows from five knobs, the capstone), each a round knob whose value sits on its face on a dark plate ("90%", "falls", "5%", "400", "0.3", the seed's number) and whose name lies on the panel below it ("Tank", "New moves", "Fall", "Floor", "Rewrite", "Seed", "Measure"), the names of neighbours staggered so none overlap. The knobs the level is about are teal with a halo on their plates; the rest stand grey and show their name for two seconds after a tap. The caps are red buttons on the panel above their knob with their own name ("Stop at first finish" on 1.2, "Lock after first finish" on 2.5), flat and grey when off. The placing camera stands 30° over the console and fits the whole desk. |
| The sensors on 2.1: names floating over each other; show each as a lamp on the car with its ray on the track, its name on a plate beside the lamp, the camera closer | The five lamps on the car as before, their plates moved off the barrier walls (Left and Right stood behind them), the bend sensor's ray drawn on the asphalt ahead with the other rays; the gantry hidden while parts are placed, so no post stands in front of a name; the camera at 12°, close. The moves' plates (2.2) moved the same way, with headroom so "Speed up" is not cut at the top. |
| No floating white 3D text anywhere | Every label is `Circuit.DarkPlate`: the knobs' values and names, the caps', the parts', the rewards' numbers, the quiz's "+2", the points rising off the car, the track cards' names (now a strip at the top of each card, where they stood over the cards before), the signpost's answers. |
| The medal reads as a ball on a stick: a medal on a ribbon or a trophy cup, and the car on the podium step | The podium stands on the pit lane up from the grid, its steps across the lane and its front toward the grid, seen along the pit lane from behind the grid with nothing between (a race can end against a wall: 2.5's did, and a podium beside the car put its third step into the barrier; across the track the pit wall hid the steps), the car driven onto the step it earned, the medal hanging on a ribbon from the step's front edge: a disc in the medal's metal with a star in relief and the three stars on the ribbon, lit as earned, popping one by one. Trees, grass and cones round the podium hide. No model was bought: Higgsfield was not connected in this session; a trophy cup stays an option to quote. |

Two walks under the screen lock verified them (`final5/`, six levels; `final6/`, 2.5, 3.1 and
3.4 after the last tweaks). The earlier fixes that no frame showed (the question board, the
floodlight mast, the 12° parts view, the rule book's board) are in those frames too. The count
of 3.1's set pit lane came down to 30 by keeping the unlit tank's name quiet; 3.4's pit lane is
29 with its prediction field moved to the right, over Train, where it no longer covers the
console.

## Web

Robot Racing: 33 MB transferred (was 31.2: the recorded cues, the world's new pieces), 4.9 s to
load, runs on the platform (`loadtime.mjs`, headless Chrome, software rendering). The platform
report is unchanged in form: the same `id:result` string, stars and check list, built from the
same checks on the same session data (`ResultsScreen.Build`), whatever the training's pace.

## The reviewers, round 3

Two fresh agents, given only the pictures in `final2/` (the frames before the three unverified
fixes above) and `final2/side-by-sides/`.

**The player test** (1.1's eleven screens, then eight other levels' pit lanes, four more):
what the game is about came out right ("a robot car that nobody drives: you set the points it
gets, press Train, watch it crash and eventually finish, then it races five laps on its own;
then I got quizzed on big green road signs and ended on a podium with a gold medal"), with the
honest "mostly I watched". It knew what to do on nine of the eleven 1.1 screens; the two it
could not read are the first question (the parts question: "a glowing car, no answers
anywhere") and, half, the pit lane before the flag ("could not tell how you move the flag,
drag?"). On the other levels' pit lanes it read the sensors ("tap the floating words to switch
sensors on"), the moves, the cards ("tap the small track-map tiles") and the rewards, and could
not tell what the dials mean on 3.3 ("a column of values with a cube, dice and box"), which
number is the die on 2.5, how to change the fuel on 1.2, or what to do first on 3.4 ("numbers,
a dice, a text box to type in"). Felt like a form or a worksheet: 3.4's pit lane ("a typing box
plus a list of stats, like homework"), 3.3's ("a settings column with unlabelled values"), the
briefing ("a reading panel next to a car"), the sign questions ("obvious test, though the signs
look nicer than a worksheet"). Broken or cut, as it saw it: plates cut by the gantry post or
the floodlight mast (1.1 set, 2.3 set, 3.4; the mast is the fix not yet walked), the rule book's
board behind the signpost (1.2; also fixed, not walked), the title's letters showing through
the calendar's cards (the dim is now opaque), the two teal wedges under the car in the race (the
side sensors' rays, as in the pilot), the flag "floating mid-air" on the shelf. Liked most: the
debrief ("podium, medal, stars: it feels like winning"), the title ("looks like an actual
game"), 2.1's pit lane ("words hanging over the car with the stadium behind, like fitting a
real car"). Least: 3.4's pit lane ("a wall of numbers through a pole plus a homework text box").

**The Claude Bomb test** (the side-by-sides): RIGHT on the briefing ("the car on a sunset grid
beats a dimmed title card, but the right half is still a text column with a button"), the pit
lane ("score markers standing on the track is a game idea"), the training ("chase cam on a
moving car reads as play"), the race ("LAP 4/5 and the sensor cones say watch this") and the
debrief ("podium, stars, flags: a results screen a player recognises"); EVEN on the title
("both are landing pages"); LEFT on the questions ("a three-line multiple-choice billboard is
a quiz, however far into the world you push it"). The redesign of the level select "beats a
curriculum node map, but it's still a modal card grid"; the sensors on the car are "a game
scene, though they're still floating labels, not parts" against the old "settings modal plus
OPENS AT lock list, pure admin". The tells, most visible first: the title's layout (hero line,
tagline, two pills, the menu icon); the briefing's panel ("onboarding slide"); the stacked
answers ("form validation, not a race"); LUMA's bar ("a chat toast") and the bottom-right
buttons; the objective strip ("a notification banner"); the menu icon. Its three changes: give
the player something to operate in the scene (drag the markers and the sensors as physical
things, Train as a start light or lever); one persistent HUD strip (attempts, lap, crashes,
points); delete the reading screens (the briefing as one spoken line over the car rolling out,
the quiz as three things on the track you pick). Broken, as it saw it: the Details button
under LUMA's bar on the run and debrief screens (the strip did overlap the buttons by a few
pixels; LUMA's line now sits 16 px higher and the prediction field above it, in the build on
disk, not yet walked); the mast through the plates (fixed, not walked); the plates overlapping
on 2.3's set pit lane; the labels "drifting off the car" on 2.1 (they lift clear of each other,
so the top ones stand over the barrier); the medal "an untextured yellow sphere"; the camera
feed's labels showing through the calendar; the car's rear cut on the title.

## Where Robot Racing stands

All thirteen levels run on the new screens and meet the brief's machine checks: every play
screen within 30 words and three controls plus the main action, every briefing within 50, every
LUMA line whole and 15 words or fewer, no OPENS AT, ladder, coordinates, seeds or probabilities
on a play or briefing screen (the seed's number stands on its own knob on 3.3, as the owner
asked). The second four fixes answer what the round-3 reviewers could not read: the knobs are
instruments on a console with their names and values, the sensors are named lamps with rays,
nothing floats, and the car stands on its podium step under its medal. What remains is the
owner's call beyond this pass: the parts question could show its candidates as plates on the
things themselves; the buttons and LUMA's bar are the next polish round, as agreed; a trophy
cup from Higgsfield would need a quote and a yes. Committed on `game-feel-pass`; AI Arena and
Tech Escape are built by their own sessions on their own branches.

---

# Phase 7c: AI Arena on the game-feel screens

8 November 2026, branch `game-feel-ai-arena` (a worktree from `game-feel-pass` at 932b511, merged back on 8 November), from
`Docs/game-feel-pass-prompt.md` and the approved design `Docs/game-feel/ai-arena.md`, with the
owner's answers: dropping produce writes only the plain part of the list (the catch and the rule
for an empty view stay in the player's own words, because the checks read them there); the thought
bubble shows Claude's first sentence without coordinates and the whole thought is one tap away;
the story is four 3D shots, first play only, skippable. Carried over from Robot Racing's Phase 7:
every control the player sets has a short name plate on the object; no floating white 3D text,
every world label on the same dark plate; the question is the objective line; a number that is
the lesson stays in the world; the podium and the ribbon medal.

## What changed

1. **Packing the agent (`World/Market.Garage.cs`, `Ui/SetupScreen.cs`).** The garage is the agent
   at a market table by its house. On the table: the errand's produce and up to three
   look-alikes, its basket, the "No" crate, the family's phone, the coins, and from 1.3 the
   trays of parts. Produce dragged (or tapped) into the basket writes "Buy apples." on the note
   pinned to its chest; into the crate, "Never buy tomatoes."; back on the table takes it off.
   The world shows what the note says (`Core/NoteText.cs` reads the text the way the garage
   shows it, and rewrites only the plain sentences), so a list typed by hand lays the produce
   out the same way. The note opens full size on a tap: the one text field, the lesson. The
   phone opens the message on a tap and closes again. Coins tapped into the battery on the
   agent's hip light its ring of notches as far as the money lasts at this list's cost; a tap on
   the battery takes one back. No physics: picking by projection to the screen, dragging along
   the table's plane, drop places by screen distance. "Send": the basket flies onto its back, it
   turns and walks off.
2. **The parts are fitted to the agent.** From 1.3 the parts the errand opens stand on a shelf at
   the back of the table, each a thing with its plate on it, and are fitted with a tap: lenses on a
   rack (Sight, worn on its eye, the sight square on the cobbles in the agent's-eye view), notebooks
   (3 turns, 10 turns, Notebook, carried in its hand), chips (Haiku, Sonnet, Opus, worn on its head),
   the effort dial. Locked parts are not shown. Arrivals: "NEW: SIGHT / MEMORY / BRAIN",
   "EVERYTHING OPEN" with LUMA's line, first visit only. (As built after the owner's answers; see
   below.)
3. **The round (`Ui/ArenaScreen.cs`, `World/Market.Round.cs`).** The street behind the agent
   pulled back so the market shows ahead (`MarketCamera.followBack` 1.9 m, was 1.12). One
   objective line (the message's gist, at most eight words), the clock at the top (six to seven,
   no digits, amber in the last five turns), the home window at the
   top right with one face per agent (crossed when sent home, lit when home; a tap follows that
   agent). On the agent: the battery draining, its thought in a bubble (the first sentence, the
   coordinates and the brain's name taken out; the whole thought one tap away). The loop on the
   agent: it looks on LOOK, the antenna pulses on DECIDE, it walks on MOVE; on 1.1 three small
   glyphs (an eye, a bulb, a foot) light in turn under the clock. Three controls: the menu, the
   camera (street, the agent's eye, the market from above), Call home. Moments: 3, 2, 1, AGENT
   OUT, SENT HOME with "Nadia bought a lemon.", OUT OF TOKENS; at the end the result stays until
   Debrief (HOME with "5 on the list in the basket", SENT HOME, CALLED HOME, ALL HOME for a
   class). The drawer, the cards, the turn counter, the stats and the view tabs went.
4. **The briefing is a transmission (`Ui/LessonScreen.cs`).** The house at dusk in the home
   camera's feed (the agent at the door, the window lit), and on the right the family's message
   typed in under "Mom · 6:00 PM", two short lines of the errand's story, "Pack the agent". The
   question, the kicker card, LUMA's intro, the part chips and the ladder went (to the file and
   the title).
5. **The debrief is a moment, then a door (`Ui/ResultsScreen.cs`).** Dusk at the family's door;
   the agent on its podium step with its basket on its back and what it bought tipped out in
   front of it (a red glow under anything that ruined dinner); the medal rising on its ribbon
   behind it and the stars popping; LUMA's one line (the takeaway's first sentence); Next, Again,
   Details. On 1.5 the cost per item, the lesson's number, stands on a plate beside the agent
   ("$0.0008 an item" in the walk). Details holds the checks, the agents' figures and costs, the lists, the
   stars in all and the rank, the takeaway and the question for the manager, the if-then plans
   on the capstone, and the file. The report to the platform is built from the same checks and
   the same strings as before.
6. **The story (`Ui/StoryScreen.cs`)** is four shots of the world, once: the house at dusk and
   the phone lighting up on the step ("Every evening, the family sends a message about
   dinner."); the agent at the door with its note ("That list is the agent's job
   description."); it walks into the market and looks around ("Once it leaves, there is no
   phone."); it buys a tomato and is sent home ("Say exactly what you mean."). A tap moves on,
   Skip ends it. The six flat beats, their captions and the ladder went; the full story lines
   are in the file.
7. **Errand 1.6 (`Ui/DecideScreen.cs`)** is the kitchen table: the five messages as notes in a
   pile, the top one readable on a dark plate; drag it to the agent, the box or the bin
   ("Agent", "Automation", "Neither"); a placed note comes back with a tap. "Who does each task?",
   LUMA's "Mark each task: an agent, a simple automation, or neither." (recorded), "Run them".
   The hints that gave the answer away went to the file. Its debrief has the same shape as the
   others (`Ui/DecideDebrief.cs`).
8. **The title (`Ui/TitleScreen.cs`)**: the player's own agent in the evening square; tap it to
   paint it the next colour, tap its name tag (a dark glass plate at its feet) to rename it.
   The errand map stays (the only ladder); Start or Continue; the menu icon; an amber lamp by
   the menu only when Claude is not connected, which opens the reason. Sound, the voice, free
   play, the story and the games menu moved to the menu.
9. **One menu (`Ui/GameMenu.cs`)**, Esc or the icon, as Claude Bomb's: Resume, The briefing, The
   file, This errand again, The story, Free play, Sound, LUMA's voice, Speed, Turns, The class
   (the roster, `Ui/SettingsSheet.cs`), Title screen, Games menu. The file (`Ui/FileSheet.cs`) is
   dark now and holds the message and its catch, the ready and broken lists with their reasons,
   LUMA's full lines, the takeaway, the question, how to play, and the story.
10. **LUMA** says one short thing when something happens (`Ui/Luma.cs`): the cues are sentences
    of her own lines, recorded with `say` like the rest (`LumaVoiceTools.GenerateBatch`), shown
    whole and never typed mid-sentence. The tour overlay went (its four lines are the garage's
    cues on the first visit: the phone, the note, a coin, the send).

## Words and controls, before and after

Counted the same way as Robot Racing's Phase 7: the "before" from the design doc's inventory (7
November 2026, the web build), the "after" from the final walk's `counts.jsonl`
(`games/ai-arena/Builds/look-audit/game-feel/answers/`, the walk after the owner's answers of 8
November, on the stand-in brain, twelve turns an errand). A word is a whitespace-separated token with a letter or a digit; left out, as the brief
asks, are the player's own words (the note on the agent, a field) and Claude's (the thought bubble,
the whole thought), which the walk counts apart. A control is anything that can be pressed or typed
in; a tap on a thing that only shows state (a face at the window, the bubble) is not counted, nor
is a tap anywhere to move the story on. The menu icon counts as a control on every screen.

| Screen | Before: words / controls | After: words / controls | Limit |
|---|---|---|---|
| Title | 59 / 12 | 29 / 11 | – |
| The story (six beats, now four shots) | 44 to 186 / 2 | 7 to 11 / 1 | – |
| Briefing 1.1 | 91 / 9 | 39 / 3 | 50 |
| Garage 1.1 (the list apart) | 144 / 29 | 9 / 2 | 30, 3 + main |
| Garage 1.1, the phone open | – | 26 / 3 | 30 |
| The round 1.1: agents out, the street, the agent's eye, the market, the end | 79 to 98 / 9 | 8 to 27 / 2 to 3 | 30, 3 + main |
| Debrief 1.1 | 115 / 6 | 11 / 4 | 30 |
| Debrief 1.1, Details open | – | 130 / 6 | – |
| Briefing 1.2 | – | 48 / 3 | 50 |
| Garage 1.2 (broken, then fixed) | – | 17 to 25 / 2 | 30, 3 + main |
| The round 1.2 (a moment: sent home) | – | 8 to 17 / 2 to 3 | 30, 3 + main |
| Debrief 1.2 | – | 7 / 4 | 30 |
| Briefing 1.3, garage 1.3 | – | 50 / 3 · 22 / 2 | 50 · 30 |
| Briefing 1.4, garage 1.4 | – | 47 / 3 · 20 / 2 | 50 · 30 |
| Briefing 1.5 | 94 / 9 | 45 / 3 | 50 |
| Garage 1.5 (broken, then fixed) | 147 / 33 | 28 / 2 | 30, 3 + main |
| The round 1.5 | 101 / 9 | 9 to 23 / 2 to 3 | 30, 3 + main |
| Debrief 1.5 | 126 / 6 | 19 / 4 | 30 |
| Debrief 1.5, Details open | – | 168 / 6 | – |
| Briefing 1.6 | – | 40 / 3 | 50 |
| Decide 1.6 (the pile, then all placed) | 184 / 19 | 8 to 25 / 1 to 2 | 30, 3 + main |
| A task of 1.6 (the rule against the agent) | – | 9 / 3 | 30, 3 + main |
| Debrief 1.6 | – | 9 / 4 | 30 |
| Briefing 1.7, garage 1.7 | – | 46 / 3 · 27 to 28 / 2 | 50 · 30 |
| The menu | – | 67 / 28 | – |

What the machine checks say, on every screen of the final walk:

- **Words:** every play screen within 30, every briefing within 50 (1.3's at 50 exactly), every
  debrief with Details closed within 30. The garage of 1.1 holds 9 words, its round 8 to 27; the
  garages that open parts carry their plates and stay within 30 (1.5 at 28, 1.7 at 27 to 28).
- **Controls:** 3 or fewer on every play screen plus the main action (the menu, Camera, Call home;
  Send, Debrief, Run them). The title keeps its 11 (the seven errand stops, Start, the menu, the
  agent's name tag, the Claude lamp), the menu its 28.
- **Form controls on a play screen:** none. The one text field is the note, opened full size on a
  tap: the place where the player writes the agent's list, which the brief allows.
- **Flags:** no OPENS AT, no ladder, no coordinates (the bubble's are stripped; the walk checks
  Claude's words for them too), no seeds, no probabilities on any play or briefing screen.
- **LUMA:** 32 lines on screen across the walk, every one whole and 15 words or fewer.

The garage's before counted 144 words and 29 controls (the drawer: a name field, ten swatches, the
list, the build-from-parts mode, ten coins, four tags); after, 9 words and 2 controls plus Send,
the decisions made on the table. The 1.6 decide screen went from 184 words and 19 controls to 9 to
26 and 1 to 2.

## The reviewers, round 1

Two fresh agents, given only the pictures of the first walk (`games/ai-arena/Builds/look-audit/
game-feel/run1/`, before the fixes below), as the brief asks.

**The player test** (1.1's fourteen frames, title to debrief): it said what the game is about
("You are the person at home, not the robot. You write a shopping list for an autonomous agent,
hand it a basket, and send it into a market where you can only watch through cameras and, at
most, switch it off. The lesson is that the list is all the control you get") and what to do on
every screen but the fourth story shot (no caption in that frame: the walk shot it before the
line). Form or instructions by its account: the briefing ("a text message with a button"), the
phone ("the same text again"), the title ("a menu"); and it was "told apples, nothing else five
times in three wordings" (the story's note, the message, the objective, the phone, the note).
Play: the round's five frames ("a pinned objective, a clock, camera modes with REC and LIVE
overlays, a map view, a Call home button"), and the debrief ("a proper game beat"). Liked most:
the look, the home-cam and agent-cam framing, the robot thinking in a bubble, the podium. Liked
least: the repetition, the stand-in agent "wandering" ("reads like a log line"), three stars with
no shown reason. Broken, as it saw it: the title lit errand 7 (the machine's saved progress);
a pink blob beside the robot's head on every dusk frame (the memory's glass cards); no caption
on shot 4; the robot's note hidden behind the basket; "AGENTS OUT" for one agent; the bubble
still up at the end; the face icon "turns red" (its colour); the podium's third step hidden
and the robot beside it. Its summary: "across fourteen screens I made no decision that changed
anything": on 1.1 the ready list is packed already, which is the errand's design ("nothing to
fix: watch the loop"), and the walk never dragged anything.

**The Claude Bomb test** (eight side-by-sides): RIGHT on the briefing ("that is a game briefing")
and on the round's look ("a third-person follow camera behind a robot carrying a basket, a
clock on the HUD, stalls ahead: that is a game camera"), the debrief by default; LEFT on the
title ("the stepper is an onboarding wizard"), the 1.1 garage ("a large empty brown slab with
three tiny vegetables... a sorting activity, not yet a scene"), the agent's eye (a lamp post in
the frame), 1.5's garage ("every meaning is also spelled out on a black plaque: a settings panel
rebuilt in 3D"), and the decide table ("a classification worksheet", "near-black and unlit"). The
tells, most visible first: the title's stepper and tagline; the flat teal buttons in the corners;
the word-signs on the table; LUMA's bar; rules restated on the briefing; the thought as a debug
log; the menu icon and the status dots; the "Agent" tag; the debrief as text plus buttons. Its
three changes: start in the world and stay there (no title panel, the errands as doors on the
street); replace every plaque and corner button with a prop and an action; make the round end
on a moment on the robot's body (home, the door opens, the basket tipped out, the robot on the
podium step). Broken: the glowing phone ("a lighting artefact"), the note cut behind the
basket and the Sight plaque, OPUS and Effort colliding at the antenna, the lamp post, the red
rectangle on the cobbles (the agent's home stone), the decide table's grid.

**Acted on before round 2:** the walk starts from a fresh machine (errand 1 lit, "Start") and
puts the progress back; the memory cards show only on levels about memory; the agent stands
beside the table's near end, whole, the table two metres and the produce bigger; the crate's
walls lower so what is in it shows; the phone's glow at a third; the tray plates lie low behind
their trays and the bulb's name moved off the Effort plate; the stand-in brain speaks plainly
("I see an apple. Going for it.", "Nothing on my list in view. Walking on.") and no longer walks
through forbidden stalls on its way, and reads "except" as a warning; "AGENT OUT" for one agent;
the bubble goes with the round; the face at home is framed teal; the agent stands on its podium
step turned so its basket shows, the medal rising behind it; the stalls in front of the door
cleared for the briefing, the story and the debrief; the story's note reads "Get fruit." so the
tomato lands; the fourth shot taken while it happens. Kept, as the brief keeps them: the errand
map on the title, the three small buttons plus one main action, LUMA's one line, and the name
plates on the things the player sets (Robot Racing's rule).

## The reviewers, round 2

The same two tests, fresh agents, on the third walk's frames (`run3/`, after the fixes above).

**The player test** (1.1's fourteen frames): the game came out right again ("You run a home that
sends a robot shopping agent to market... You pack the robot with a written list and supplies,
then it goes out alone while you can only watch and pull the plug. The lesson is clearly about AI
agents: they act on their instructions alone"). It knew what to do on thirteen of fourteen screens;
the fourth story shot still had no line (its caption had faded before the walk's shot). Liked
most: "the off switch as your one control", the agent cam's "I see an apple" and "Nothing on my
list in view", the town and the robot. Form or instructions: the briefing ("a web-page layout"),
the title ("landing page"), the phone ("read this closely"), the table ("a form dressed up in 3D:
every object has a printed label... Send works like Submit"), the story ("rules as a slideshow").
Play: agent out, the street, the agent cam, the overhead view ("the display looks like a real
game, the thought bubbles are charming, and 'you can only press the off switch' creates real
tension"). Least: the shopping "never appears on screen: no goods on the stalls, no apple I could
see", the round's end with no outcome, three stars not explained. Broken: the edge of the world
between houses, the light going night, day, night, Skip beside Mom's name, the feed's place name
over the agent's feet, "Batter" clipped, the message hiding the table, "I see an apple" with no
apple in view, the loop's icons unlabelled, the podium's empty steps, the medal "a torch or
lollipop".

**The Claude Bomb test** (the side-by-sides): RIGHT on seven of eight: the briefing ("the right
takes the left's security-feed look... gives you someone handing out the mission"), the setup
("a 3D packing scene... the goal in a quest bar"), the play ("a third-person camera behind the
robot walking a market street with its basket, and the world fills the frame"), the agent's eye
("the most game-like frame in the set"), the debrief ("reads straight away as a results screen"),
1.5's setup, and 1.6 ("only because it's 3D; the most lesson-like frame"). LEFT on the title only
("laid out like a landing page... seven numbered boxes"). In round 1 it preferred the right on two
or three. What still reads as an app, most visible first: the word plates on the table (Battery,
Coins, No, Effort, OPUS, Sight, Memory, Brain, Agent, Rule, Neither); 1.6 as a sorting quiz; the
navy panels of the title and the briefing; the flat buttons; the stepper; LUMA's takeaways; the
bubbles "like tooltips"; OPUS, Effort, Memory as settings words. Its three changes: physical parts
that snap in with no plates; sellers, shoppers and goods in the market; the title and the briefing
over the world. Broken: "Batter", the No plate behind the produce, the note cut, "I see an apple"
with no apple, the "?" markers (the agent's-eye view of stalls it cannot see), the pink home stone
"like a debug overlay", the robot's colour under dusk and day, the medal's pole behind its head,
"simple automation" in LUMA's line and "Rule" on the box, the bin crowding "Run them".

**Found on the way: the stalls had no goods.** Both reviewers' "empty market" was a real bug from
the realism pass, in two parts, both in the kit's merge step (`MarketKit.Merge`): the combined
goods mesh was run through `MeshUtility.Optimize`, which on Unity 6000.3 empties it, and its index
format was switched from 32 to 16 bits after combining, which drops its triangles. So every
`goods_<item>_crate_0` and `_heap_0` mesh was saved with 0 vertices and 0 triangles (the kit's
preview rendered them from memory before saving, so the previews looked right). Optimize is gone,
the index format is chosen before combining, and **AI Arena > Rebuild market goods**
(`Editor/MarketKit.Goods.cs`) rebuilt the 15 goods prefabs and their 30 meshes from the same recipe
(1,040 to 30,630 triangles each); nothing else of the kit was rebuilt, and the food material the
rebuild rewrites was restored as it was. The stalls now show their crates heaped with the item.

**Acted on after round 2:** the round's result stays up until Debrief ("HOME" with "1 on the list in
the basket", "SENT HOME", "CALLED HOME"); the debrief tips the basket out on the podium step in
front of the agent, a red glow under anything that ruined dinner; the medal stands clear of the
agent's head; the feed's place name at the bottom right; the message card under the table; the No
plate over the produce; the battery plate beside the pack; LUMA's phone cue cut to two of her own
sentences ("Read it closely. The agent will never see it.") so the screen keeps within thirty
words; the story's last line stays for its shot; the box on 1.6 reads "Automation", as LUMA says.
Kept, and left to the owner: the name plates on the things the player sets (Robot Racing's rule,
which both reviewers would remove), the errand map and the navy panel on the title (the brief keeps
the ladder there), the briefing as a transmission panel, the dusk for the briefing and the debrief
and the day for the garage and the round (as designed; the player test found the change confusing),
the agent's home stone.

## The owner's answers, 8 November, and what changed

| Asked | Done |
|---|---|
| Name plates: keep them, only on a real object; each part the errand opens is a visible thing with its plate on it; parts it does not open are not shown; no plate floats over the houses or the table | The parts stand on a shelf at the back of the table (`Market.Garage.cs`, `Market.Props.cs`): three lenses on a rack whose front reads "Sight"; three notebooks standing, "3 turns", "10 turns", "Notebook" on their covers; three chips standing in a block, "Haiku", "Sonnet", "Opus" on their faces; the effort dial in its box, "Effort" on its front. A fitted part leaves the shelf and is worn with its plate: the chip on the agent's head, the notebook in its right hand, the lens on its eye (its plate is the rack's). The battery reads "Battery" on its pack (turned toward the garage's camera), the coins sit in a box reading "Coins", the crate reads "No" on its front wall. On 1.6 "Agent" is pinned on the agent's chest, "Automation" on the box, "Neither" on the bin, and the top note's message is a card standing in a wooden holder on the pile. Every billboard plate is gone, and so are the floating "?" over the stalls the agent cannot see (the covered crates say it) and the receipt (1.5's cost per item now stands on the podium step's face). Parts an errand does not open are not on the table or on the agent; the old glass cards behind its head are gone |
| Title and briefing panels: keep | Kept |
| One light: dusk for the whole errand, lamps lit | The garage, the round, the decide table and the debrief are at dusk like the story and the briefing; the square's lamps and the stalls' lanterns are lit, a lantern stands lit on the table, the door lamp lights the agent |
| No clock tower | The clock post on the square is gone; the clock at the top of the screen stays |
| The family at the door: leave a marked spot | A doormat at the front of the house behind the agent's home (`Market.FamilySpot`, found at run time: the nearest house of the square's ring). Mom stands on it now (below) |
| The medal: Robot Racing's ribbon medal exactly; the podium's 2nd and 3rd steps only when several agents run | `Market.Podium` is Robot Racing's `Circuit.Podium` in this kit's materials: the same steps (1.1, 0.7, 0.7 wide; 0.36, 0.24, 0.15 high), the medal on its ribbon over the front of the hero's step, the disc at its end, its three stars on the ribbon, dropping into place and the stars popping. One agent: a single step, no number. Several: three steps numbered 1, 2, 3, the next two agents on 2 and 3 |

Also: LUMA's arrival line on 1.7 is "Every dial costs tokens on every turn." (a sentence of her own 1.7
intro, recorded), so the garage with every part open keeps within thirty words.

**Mom.** The owner had her made (Higgsfield, Meshy 7, job `a2c8e5b3-390e-486a-9621-4c1b9e3c673e`,
47.5 credits; recorded in `games/ai-arena/Docs/credits.md`). She was checked before use: rendered
at rest and through the wave (`games/ai-arena/Builds/look-audit/game-feel/mom/`): the face, the
glasses and the hands are whole, the waving hand has its five fingers, her arms rest at her sides at
the clip's first and last frames, she faces +Z. Split into 1K maps like the other heroes, built by
**AI Arena > Build family** (`Editor/MarketKit.Family.cs`) into `Resources/Market/Family`: 1.65 m,
pivot at her feet, one URP Lit material, lit by the dusk scene and a porch lamp over her door. She
stands at rest on the doormat while the agent is out (the briefing, the story, the round) and waves
once when it comes home (the debriefs, `World/MomRig.cs`).

## The player test, round 3

A fresh agent, on the walk after the owner's answers (`answers/`): 1.1's fourteen frames, then the
garages and debriefs of 1.5, 1.6 and 1.7 (taken before Mom was in). It said what the game is about
("You pack a small shopping robot for an errand: you give it a written list, a brain, a number of
turns, a battery, coins and things it must not buy. Then you let it go and cannot steer it. It is a
lesson about AI agents dressed as a dinner errand: write exact instructions, use the smallest tool
that does the job, and know when you don't need an agent at all") and what to do on every screen.
Play, by its account: the round ("'Agent out, nobody is steering', the clock and the see-think-act
icons create real tension; 'I see an apple' lets me watch the agent's mind work"), the medal plinths
("read as a reward"), the 1.1 garage ("closer to play than the later ones: few objects and they are
real things"). Form or instructions: the briefing ("an instruction card"), the story ("a captioned
slideshow"), the later garages ("a settings panel laid out on a table: rows of labelled cards";
"Haiku, Sonnet and Opus mean nothing to a non-Claude player"), 1.6 ("a sorting quiz"), LUMA's
debrief lines ("state the rule outright"). Liked most: the hook ("You write the list. It shops alone.
Once it leaves, nobody can help it"), the home-cam and agent-cam framing, the robot thinking out
loud. Least: the red wash over the garages (the agent's home stone, tinted in its team colour, under
the table), the label wall on the later garages, not knowing what can be touched at the table.
Broken or confusing, as it saw it: the world ending in a flat plane between the houses; the agent cam
saying "I see an apple" with no apple in view and the overview where the robot cannot be found;
"HOME" while the robot is still in the market (the round's end, before it walks home); the apples
reading orange at dusk on the podium; the lantern on the decide table "floating" (it stands on the
table's back edge, seen from in front); "Notebook" cut by the Haiku chip on 1.5; the note on the chest
cut mid-list; the lens on the agent's face read as "a flat blue disc"; 1.7's garage identical before
and after the walk's one change (a coin, which shows only on the battery's ring); "Call home" after
"no phone"; 17:58 on the feed and 6:00 PM on the message.

The table above is the state this reviewer saw, plus Mom. The owner took four of its findings into
the last round (below): the red wash under the garage's table, "HOME" in the market, the orange
apples, and the model names.

## The last round, 8 November

The owner's five points after Mom, and what changed. Frames: `home-walk/`, on the committed build, and `last/`, the walk the fourth player test saw (below).

| Asked | Done |
|---|---|
| The garage's ground is washed pink-red under the whole table: the agent's home stone should be a small mat under the agent only | Two things made the wash: the agent's sight square (twelve metres of glass in its team colour, drawn round the chosen agent) showed in the garage, and the home stone was a whole 1.9 m cell tinted in the team colour. The sight square is off while the garage is open (`Market.UpdateVision`); the home stone is a mat 0.8 by 0.57 m under the agent's feet (`Market.BuildHouse`), moved under it where it stands beside the table (`Market.BuildGarage`). The square round it keeps its own stone. At the debrief the mats give way to the podium |
| "HOME" shows while the agent is still in the market; it only shows when the agent reaches the house | When the turns run out (or the player calls them home), every agent with charge left walks home, all at once: on the grid round the stalls still standing to its home cell, then across the street to its door, in front of Mom's doormat for the house with the family (she waves as it arrives), and stops facing the door (`Market.WalkHome`, 1.85 to 3.2 m a second, faster on a long way). Only then does the round say HOME, CALLED HOME or "n OF m HOME", the agent's face in the window turns to home and the Debrief button appears (`ArenaScreen.HomeThenSay`). An agent sent home for a wrong buy keeps its "SENT HOME" moment where it happens |
| Apples look orange at dusk on the podium | Two warm lamps over-lit them: the medal's lamp reached the step's top, and the door lamp shone at full strength. The medal's lamp now lights the medal only (closer, shorter reach), the door lamp is softer at the debrief, and the apples on the step take a deeper red. They read red |
| Haiku, Sonnet and Opus keep their names, but the chips are small, medium and large, and LUMA names them in one short line the first time they appear | Each chip and its name plate grow together: Haiku at 0.7, Sonnet at 0.9, Opus at 1.2 of the old chip (`Market.ChipSizes`); worn on the agent's head they are 0.8 of that, so Opus is as wide as the head. The block is respaced so the small chip stands clear of the notebook, the effort dial a little narrower. On 1.5's first visit, after "Now you choose the agent's brain and how hard it thinks.", LUMA says "Three brains: Haiku is small, Sonnet medium, Opus large." (nine words, recorded); 1.5's garage is at thirty words with it |
| On the debrief, bring Mom closer to the camera so her wave reads; make sure the medal's ribbon is visible | Mom comes off her doorstep toward the camera by 3.4 m, turned to it, a lamp over her (`Market.FamilyCloser`); her doormat stays at the door. She stands about 1.4 times as tall in the frame, to the left behind the podium. The ribbon is red with a white stripe (Robot Racing's was navy on the navy step, so only the disc showed), a little wider, and comes over the step's edge from where the agent stands before it drops down the face to the disc; the three stars are on it |

All machine checks still hold on every screen of `last/` and `home-walk/` (no screen over its limit; 1.5's garage at 30
words, 1.7's at 27 and 28). The Mac build's smoke test passes.

## The player test, round 4

A fresh agent, the same prompt and the same twenty screens as round 3, on the walk of the five
points (`last/`). What the game is about, in its words: "You are the person who prepares a small
shopping robot for Mom's dinner errand. You write its list and choose its gear (brain size,
number of turns, how far it can see, a notebook, effort), then you send it out alone and cannot
help it; you can only watch or call it home. When it returns you get a medal based on what it
brought back. Underneath, it is teaching how AI agents work: precise instructions, the agent acts
on its own, every word and turn costs tokens, use the smallest model that does the job, and don't
use an agent when a simple automation will do." It said what to do on most screens; on the 1.1
garage it could not tell which choice was its own (the list is packed already), and on 1.7's
second garage frame it could not tell what had been set.

Play, by its account: the debrief ("the strongest game moment: the robot on a plinth with the
apples it brought, a medal, and Mom"), the round's cameras ("it looks like a game; but it is
spectating"), the 1.1 garage ("could be play, packing a basket from a table of goods"). Form or
instructions: the briefing ("a text panel with rules and a button"), the story ("a lecture over
pretty backdrops"), the phone, the later garages ("a configuration form made of 3D plates"), 1.6
("a classification worksheet"), LUMA's debrief lines and LUMA over almost every frame. Liked most:
the warm market town, the loop (prepare, send, watch it act alone, judge the haul), the agent cam
and the overhead. Least: the loadout table (crowded, plates overlapping, unclear what is chosen),
and LUMA explaining before the player discovers. "In the first level I cannot point to a single
choice I made myself."

On the five points: the garage's red wash is not mentioned; the apples on the step and the medal
read (it saw "three apples and a glowing gold medal hanging off the front"); the chips' names were
read, but not their sizes; Mom's gesture was read ("raises a hand to her head ... could mean pleased
or 'oh no'"). Two findings were about this round's changes, and both were fixed after the test
(`home-walk/`):

- "HOME" still read wrong: the agent stood on its mat at the square's edge, among the stalls, not
  at the house. The walk home now goes on across the street to the door: in front of Mom's doormat
  for the house with the family (she waves as it arrives), to its own door's step for the others,
  and it stops facing the door. The round says HOME then: in `home-walk/` the round-over frames of
  1.1 and 1.5 show the agent at Mom's door, her hand up, HOME over the door (the banner covers her
  apron; her face and her hand are above it).
- 1.5's "$0.0003 an item" on the step's face ran into the new ribbon and over the step's edge: it is
  smaller now (0.58 from 0.7) and sits clear of both.

Its other findings stand as in round 3, or are new and not acted on (the brief asks for a stop):
the world's edge beyond the houses; the battery pack "dangling" from the arm and the agent
"wearing four or five signs at once"; the lens read as "a flat pale-blue disc"; translucent circles
at the agent's feet in the round; the red-tinted sight square in the agent cam read as a bug;
LUMA's line over the overhead without its panel; "Call them home" with one agent; the note on
the chest cut ("Anything at the market is fine..."); the "Effort" plate hidden by corn spilling
from the "No" crate on 1.5 (the crate's goods vary from run to run); 1.6's single note card read as
"Mom's message rather than a task", and its "Neither" bin as a trash can; nothing on 1.6's step;
the produce on 1.7 piled up and the teal pear and cabbage in a "yellow and green only" errand; 1.7's
list plate blank, and its "set" frame the same as before; the three icons under the clock and the
window's face unexplained; every medal gold in these frames, so no tiers are seen.

## Web

AI Arena: 37.5 MB transferred (37.4 before this round; 32.8 after the realism pass), 3.9 s until
the loader hides on a quiet machine (4.8 s while a screenshot walk ran beside it), no console
errors; measured with a headless Chrome on software rendering against the platform's local server
run from the branch's worktree (the same method as the realism pass). Under the 45 MB cap. The build was
`site/games/ai-arena/` in that worktree; after the merge it is rebuilt in the main folder (Phase 7e).

## Where the pictures are

All in `games/ai-arena/Builds/look-audit/game-feel/` (moved from the worktree when it was removed; ignored by git, like Robot
Racing's): `home-walk/` the last walk, on the committed build (errands 1.1, 1.5, 1.6 and 1.7, 35
frames with `counts.jsonl`); `last/` the walk the fourth player test saw (before the walk to the
door and the smaller price tag); `last-round/` the first walk of the last round (the small chip
still behind the notebook); `answers/` the walk after the owner's answers, 51 frames (the frames
of the third player test); `mom-walk/` errands 1.1 and 1.6 with Mom at the door; `mom/` her preview
renders and `mom-sheet.png`; `final/` the walk before the answers, with `final/side-by-sides/`;
`run1/` and `run3/` the frames of the two earlier reviewer rounds, with their side-by-sides;
`goods-check/` errands 1.1 and 1.2 just after the goods fix.

## Waiting for Charbel

Nothing blocking. The five points of the last round are done (above). The fourth player test's
other findings are listed under it, not acted on, for a next round if you want one; the largest
are the loadout table read as a settings form, LUMA over almost every frame, and the world's edge
beyond the houses.

## Where AI Arena stands

All seven errands run on the new screens and meet the brief's machine checks on every screen of
the walk after the owner's answers (the table above) and of the last walk (`home-walk/`). The player test, both rounds, says what the game is about and
what to do on every screen but one story shot; the Claude Bomb test preferred the new screens on
two or three of eight in round 1 and seven of eight in round 2. What both still hold against it
is mostly what the brief and Robot Racing's rules keep (the name plates, the errand map on the
title, the transmission panel, the buttons for the main actions, LUMA's one line), and what the
game is ("nobody steers": on 1.1 the list is packed already and the player watches). The empty
market both reviewers saw was a kit bug, now fixed.

Not checked by the walk, and worth a look by hand: a live run on Claude (the walk uses the stand-in
brain; the smoke test made one real Claude call through Claude Code and read its move); the class
mode with several agents (the window's faces, picking one); free play; dragging by hand (the walk
acts through the same calls the drags make); 1.3, 1.4 and 1.7 played through (their briefings and
garages are walked). The third player test saw every screen but Mom; the fourth saw Mom and this round's changes,
except the walk to the door and the smaller price tag, which came from its findings and were checked
on the last walk's frames.

This section was the branch's report (`Docs/game-feel/ai-arena-report.md`) until the merge.

---

# Phase 7d: Tech Escape on the game-feel screens

8 November 2026, branch `game-feel-tech-escape` (a worktree off `game-feel-pass` at 932b511, merged back on 8 November),
from `Docs/game-feel-pass-prompt.md` and the design in [tech-escape.md](game-feel/tech-escape.md) with the
owner's three answers (console labels in plain words with the port after; the attacker walks
to the door and never attacks early; the first attack on an unfixed room is a free rehearsal).
All nineteen levels run on the new screens. Only `games/tech-escape/`, this report (then `Docs/game-feel/tech-escape-report.md`) and the design
doc's answers changed; the kit, the server and the other games are untouched. Frames and counts:
`games/tech-escape/Builds/look-audit/game-feel/` (not in git).

Round 5 comes first below (two last changes), then round 4: Charbel's answers to the round-3
report, the Higgsfield files, the counts, the web size and the fourth player test. Round 3,
then rounds 1 and 2, follow as they were reported; where a later round changed something, the
later section is the one that holds.

## Round 5: two last changes (8 November)

1. **The board's lamps are teal once every decision on the row is set**, amber while one is
   still a question mark; whether it was right, only the attack shows. The lamps at the top of
   the screen, the board of lamps on the wall and the lamp on the card brought close follow
   the same rule. On 2.4 the two rules that are wrong as handed over now show teal like the
   others; on 1.1 the found drive's card stays amber until its five decisions are made ("Set
   its owner", then "Rate it", then "Lift the lockdown"). The round-4 test of rightness
   (`Room.RowRight`) is gone from the code.
2. **2.9's corridor monitor clears the wall's BACKUP.** The monitor's search now counts the
   words stencilled on a room's back wall among the things to keep clear of. Where its bracket
   would still cross them (2.9, where no spot on the wall is wholly clear), it hangs from a rod
   to the ceiling instead, lifted until its foot clears the words. On 2.9 it now hangs above
   BACKUP, which reads whole.

Re-walked: 1.1, 2.4 and 2.9 only (`Builds/look-audit/game-feel/round5/`, 38 pictures). Their
counts are unchanged (1.1's room 20 and 26 words, 2.4's 30, 2.9's 23; briefings 44, 43, 39;
debriefs 18 and 13), no flags, LUMA's lines whole. The other sixteen levels were not walked
again: the lamp rule is the same code in every room, and the monitor moves only where its spot
crosses a room's lettering, which in round 4's frames was 2.9 alone (2.3 and 3.3 have COMMS on
the wall with the monitor well below it; they were not walked again). The web build was made
again with these changes (41.1 MB on disk, the same as round 4, whose transfer measured 43.2 MB).

## Round 4: the owner's answers and the Higgsfield files (8 November)

What Charbel answered after the round-3 report, and what the build does now:

1. **Values only where they are read.** From the room camera the knobs show no values; each
   card on the board has one lamp at its corner, teal when the row is set right and amber when
   not. "Right" is the room's own judgement (`Room.RowRight` in `Core/Facility.cs`): the row
   as the player has it is tried in the setup that works, and it is right when nothing more
   gets through or fails, so any answer the checks accept turns the lamp teal, not only the one
   in the setup. The board of lamps on the wall and the lamps at the top of the screen follow
   the same judgement. The card brought close shows every value ("Owner: Dana, clerk") and the
   card's lamp by its title; the knob just turned shows its value on the board until the next
   one is turned.
2. **Light knobs on the open card**: a light face and a dark pointer from the centre to the
   rim, at the end of each row.
3. **Finding what nobody listed (1.1).** The working copy of applicants' documents is a small
   network drive, labelled SHARED, on a crate on the floor of the room, out of the camera's
   view as the room opens. The player turns the security camera by dragging across the room
   or with the arrow keys (36 degrees each way, 22 down); two faint chevrons at the edges of
   the feed say it turns, and fade once it has. Anything not yet on the list glints, a small
   star and a chime, when the camera passes it. A tap flies the drive up to the board, where
   it becomes the card, while the camera turns back. The printer and the canteen menu, the
   decoys, glint and fly the same way (in the code; the walk taps only the drive). No other
   level has an unlisted thing to find.
4. **The attack pays off both ways.** A door that gives way: the attacker goes in, comes back
   out with the drive in his hand (lit teal, so it reads down the dark corridor) and runs off
   with it. A door that holds: he tries the handle twice more, the door shaking in its frame,
   stands a moment, gives up and walks away down the corridor. Then the end held on the door, as
   before. No body or role for the player.
5. **The staff badges** (Higgsfield's two printed sheets). Both sheets are spelled right,
   "HARBOR PERMITS OFFICE" and every name and role included. Sheet B is used: its Sami badge says
   LEFT JUNE as the game does, and its large names and coloured role bands read from the room
   camera. Each badge is cropped into its own texture and clipped on a thin card at the left of
   each of 2.1's six turnstile accounts and 3.1's three roles.
6. **The patch panel** (Higgsfield, Tripo). Split into a geometry-only model and 1K maps by the
   same cleanup as the other heroes (`Tools/glb_maps.py`, a rewrite of the realism pass's lost
   tool) and built as a prefab by `Tech Escape > Build patch panel`. The model is a 2U panel
   full of blue, yellow and red cables hanging in front, not five sockets; hung above the rules
   its cables would cover them, so it is the rack's bottom unit, 0.74 m wide under 2.4's five
   rule modules, cables hanging below. 8,998 triangles, no emission map.
7. **LUMA's strip** is left as it is: it is shared and is fixed once after the merge (done, Phase 7e).

Two bugs found on the way and fixed: the room camera kept the prop in the middle of the frame
by shifting its projection, so a turned camera only skewed the picture around the board; the
point it keeps in the middle now turns with the player's look. And a card found in the room
told the screen before the board was rebuilt, so the lamps at the top lagged one step.

### Words and controls, round 4

From the last walk (`Builds/look-audit/game-feel/final/counts.jsonl`, all nineteen levels,
130 pictures). With the values off the knobs the rooms are back within the brief:

| Level | Briefing | Room as handed over | Card close | Room set right | Debrief |
|---|---|---|---|---|---|
| 1.1 | 44 | 20 | 54 | 26 | 18 |
| 1.2 | 43 | 24 | 55 | 24 | – |
| 1.3 | 40 | 20 | 55 | 20 | 19 |
| 1.4 | 41 | 23 | 43 | 23 | – |
| 1.5 | 41 | 19 | 72 | 19 | – |
| 2.1 | 42 | 21 | 35 | 21 | – |
| 2.2 | 41 | 28 | 43 | 28 | – |
| 2.3 | 44 | 22 | 28 | 22 | – |
| 2.4 | 43 | 30 | 42 | 30 | 13 |
| 2.5 | 41 | 20 | 38 | 20 | – |
| 2.6 | 41 | 19 | 26 | 19 | – |
| 2.7 | 42 | 28 | 32 | 28 | – |
| 2.8 | 42 | 23 | 23 | 23 | – |
| 2.9 | 39 | 23 | 27 | 23 | – |
| 2.10 | 41 | 23 | 43 | 23 | – |
| 3.1 | 43 | 17 | 21 | 17 | – |
| 3.2 | 39 | 19 | 27 | 19 | – |
| 3.3 | 43 | 24 | 40 | 24 | – |
| 3.4 | 37 | 25 | 48 | 25 | – |

Every room within 30 words and 3 controls plus Lift the lockdown, as handed over and set
right; every briefing within 50; every debrief within 30; the eleven LUMA lines on the walk's
screens whole and 15 words or fewer; no flags. The card brought close goes over 30 on most
levels (21 to 72 words, the board around it included), as agreed: it is the reading moment.
The badges' printed words are not counted (they are pictures, not text the walk can read).

### Web, round 4

| | Round 3 | Round 4 |
|---|---|---|
| Transferred (the platform's page in headless Chrome, port 8086) | 42.4 MB | 43.2 MB |
| Load (software GL) | 4.5 s | 4.9 s |

Under the 45 MB limit with 1.8 MB in hand; the patch panel and the badges cost 0.8 MB. The
game loads on the platform (`Builds/look-audit/game-feel/platform/platform-tech-escape-round4.png`).

### The player test, round 4

A fresh agent, the same prompt, only 1.1's eighteen screens from the last walk (its answer in
full: `Builds/look-audit/game-feel/reviewers/player-round4/answer.md`). It told the level back
whole: the lockdown at the Harbor Permits Office, the board in the Risk Room, the one asset
nobody listed, the rehearsal in which the intruder steals it, the shared drive found, added,
owned and rated, the door holding, three stars.

- **What it read as play**: finding the drive ("the one real 'aha, there it is' moment"), the
  attacker running off with "something glowing" ("the best moment so far, because something
  was taken"), and the door that "visibly refuses him" before he walks away.
- **What it read as a form or instructions**: the briefing ("the mission verb is 'list'"), the
  card brought close with its question marks ("this is the form"), Details ("a report card").
  The board as handed over is "closest to play", but Lift the lockdown is lit from the start
  "like a Submit button".
- **What it could not tell**: whether the drive in mid-air was carried, dragged or a glitch
  (the flight caught in a still), and whether the hooded figure was the player or the
  intruder (the chase camera sits over his shoulder).
- **What it saw as broken, and where it stands**:
  - No values on the board's knobs. As asked: the lamps carry the state, and the values are on
    the card brought close.
  - The board's cards stick out past the whiteboard's frame, and the open card cuts the cards
    beside it. True in the frames.
  - "Take it off the board" in salmon reads like a delete button.
  - LUMA's "Watch which door it goes through" comes while the player is still in the room.
    That is the line she says once the drive is owned, before the attack.
  - Framing: door signs cut at the frame's edges, flat ceiling lights, the medal small on a
    dark step, the HUD's corner marks over props, a dark wedge under the Details panel.
- **Its changes**: slot the drive into the board by hand and set the owner by handing it to a
  person, with the ratings coming from what the rehearsal showed; make the threat real, with the
  player walking as the agent and a countdown; hide Lift the lockdown until the board is
  complete; a bigger, brighter medal. The countdown and the player's body go against your
  answers (no countdown; no body or role), so they are not taken up. Hiding Lift the lockdown
  until the board is complete would undo the rehearsal, which needs the room as handed over.

None of this was acted on after the test: the build committed is the one in the frames.

## Round 3: the owner's answers (8 November)

What Charbel answered after the round-2 report, and what the build does now:

1. **The close-up is a flat card, face-on** (`World/RoomRig.cs`). Tapping a card no longer
   tilts its plates toward the camera. A new card comes up in front of the board, square to
   the camera: the full name at the top, the file's line under it, then one row per decision,
   each reading "Owner: Omar, supervisor" with a lamp at its start and a knob at its end. All
   rows of a card share one text size, the size its longest possible line needs, so nothing
   jumps when a row is turned. The camera stands as far back as fits the whole card with a
   margin. When furniture stands between, the camera comes closer and the card shrinks with
   it: 44 % on 1.5 (the bin), 85 % on 2.2 and 2.3, full size elsewhere. While it is open, the
   loose things and the lamp's cone and dust step aside. Checked in every room of the last
   walk: nineteen card close-ups and the six console close-ups, all face-on, nothing cut off.
2. **Every control shows its value on its face**, as on Robot Racing's console: "Omar",
   "High", "Off" on the knob at rest, and "Label: Value" on each row of the close-up. The
   knobs were drawn twice their size until now (Unity's built-in cylinder mesh is two units
   across, not one) and overlapped each other; they now sit in their housings, 7.6 cm discs,
   and the housings are matte so the lamp no longer flares on them.
3. **The console is a card.** The room's one-off decisions (Sign-in rules on 1.4, The summary
   on 1.5, Crossings on 2.3, The sign-in on 2.7, Declaration on 3.1, Trigger and cause on 3.4)
   are the last card of the grid, the same size as the others, brought close the same way.
   This ends 1.5's shrunken console and the Operations Center console that stood in front of
   the monitor.
4. **The Operations Center monitor.** The corridor feed plays on one of the screen wall's own
   screens. The grid slides sideways, by as little as it can, so that one screen stays clear:
   the grid moved 25 cm on 2.7, 5 cm on 3.1 and 45 cm on 3.4, and the feed's screen is 0 %
   covered in all three (it was 41 % on 2.7). Elsewhere the monitor on the back wall is a little smaller, so it clears 2.3's
   board of lamps and stays inside 1.4's feed.
5. **No countdown.** The attacker on the monitor walks the corridor to this room's door and
   stops there. He tries the handle, with a rattle, every 3.5 to 5.5 seconds, until the player
   lifts the lockdown. He never attacks early.
6. **"Lift the lockdown"** replaces "Run the attack": the button, the objectives ("Set its
   owner. Then lift the lockdown.", "Fix the room. Then lift the lockdown.") and the briefing.
7. **The briefing is the alarm and the story only**: "Intruder alarm", the room, then three
   lines. For 1.1: "23:47. An intruder alarm at the Harbor Permits Office." / "Every door is
   sealed. Someone is in the corridor." / "Before you lift the lockdown, list what the Risk
   Room protects." Nineteen new alarms, no motto in any.
8. **The motto moved to LUMA's line on the debrief**: as many whole sentences of the level's
   takeaway as fit in 15 words ("You cannot protect what you have not listed, and nobody
   protects what nobody owns."). The eight debrief lines whose words changed were recorded
   again, and 3.4's is new (take-19).
9. **The podium has one step**, with no number, the medal rising on its front. Robot Racing's
   podium got the same change after the merge (Phase 7e).
10. **Higgsfield**: the staff ID badges and the patch panel with cables are approved and
    coming from Charbel. The current props stay until the files arrive.

The words this costs: the values on the knobs at rest count as words when the camera sees
them, so the rooms with many knobs go over the brief's 30 (table below). That is the owner's
rule against the brief's limit; see Waiting for Charbel.

### Words and controls, round 3

From round 3's walk (`Builds/look-audit/game-feel/final-round3/counts.jsonl`, all nineteen levels),
counted as in rounds 1 and 2: every visible word, the world's labels included when the camera
sees them. The values on the knob faces now count, as they are on screen.

| Level | Briefing | Room as handed over (round 2) | Card close | Room set right | Debrief |
|---|---|---|---|---|---|
| 1.1 | 44 | 47 (22) | 75 | 55 | 18 |
| 1.2 | 43 | 48 (24) | 79 | 46 | – |
| 1.3 | 40 | 47 (20) | 82 | 48 | 19 |
| 1.4 | 41 | 36 (23) | 56 | 39 | – |
| 1.5 | 41 | 51 (30) | 104 | 51 | – |
| 2.1 | 42 | 33 (21) | 44 | 33 | – |
| 2.2 | 41 | 43 (28) | 51 | 43 | – |
| 2.3 | 44 | 32 (30) | 32 | 32 | – |
| 2.4 | 43 | 44 (30) | 50 | 42 | 13 |
| 2.5 | 41 | 29 (20) | 47 | 31 | – |
| 2.6 | 41 | 24 (19) | 31 | 25 | – |
| 2.7 | 42 | 39 (30) | 38 | 39 | – |
| 2.8 | 42 | 29 (23) | 26 | 29 | – |
| 2.9 | 39 | 30 (23) | 33 | 31 | – |
| 2.10 | 41 | 41 (23) | 61 | 41 | – |
| 3.1 | 43 | 20 (18) | 24 | 26 | – |
| 3.2 | 39 | 25 (19) | 33 | 27 | – |
| 3.3 | 43 | 27 (24) | 43 | 38 | – |
| 3.4 | 37 | 68 (30) | 88 | 68 | – |

- **Within the brief**: every briefing (37 to 44 words, limit 50); every debrief (13 to 19,
  limit 30); every play screen at 3 controls plus Lift the lockdown; all eleven LUMA lines
  on the walk's screens whole and 15 words or fewer; no flags (OPENS AT, ladder, coordinates,
  seed or probability) on any play or briefing screen.
- **Over the brief's 30 words**: the room as handed over on twelve levels, and set right on
  fifteen, up to 68 on 3.4. The difference from round 2 is the knob values: 1.1 counts 25
  more words and has 25 knobs. The rooms with few knobs (2.6, 2.8, 3.1, 3.2) stay within 30.
- **Card close** counts the card and whatever of the board shows around it (1.5's 104 is
  its card plus the knob values of the cards beside it). The menu now counts 89 words (51) and the file 517
  (457): the menu itself is unchanged; the room's knob values show behind its veil, and the
  file's page on the room was rewritten for this round.

### Web, round 3

| | Round 2 | Round 3 |
|---|---|---|
| Transferred (the platform's page in headless Chrome, port 8086) | 42.0 MB | 42.4 MB |
| Load (software GL) | 4.1 to 5.4 s | 4.5 s |

Under the 45 MB limit with 2.6 MB in hand. The game loads and runs on the platform
(`Builds/look-audit/game-feel/platform/platform-tech-escape-round3.png`).

### The player test, round 3

A fresh agent, the same prompt, only 1.1's thirteen screens from the last walk (its answer in
full: `Builds/look-audit/game-feel/reviewers/player-round3/answer.md`). It told the level back
whole: the alarm at the Harbor Permits Office, the board in the Risk Room that must list what
the office protects, each item's owner and how bad a leak, a change or a loss would be, the
rehearsal where the attacker takes what nobody wrote down, the shared drive found and added,
the lockdown lifted, the door holding, three stars. "The corridor scenes look good; the parts
where you actually act look like a spreadsheet."

- **What it read as play**: the rehearsal, the real attack ("the best moment of the level"),
  the medal.
- **What it read as a form or instructions**: the briefing ("homework, not a mission"), the
  card brought close ("a database record with knobs on it"), Details ("a teacher's marking
  sheet"). The four board screens read as "a form dressed up as a prop".
- **What it could not tell**: what to click in the room as handed over and where the missing
  thing is; what to do differently after the rehearsal; how the shared drive got onto the
  board (the pin happens between two screenshots); what Camera does.
- **What it saw as broken, and where it stands**:
  - The knob values on the board are too small to read at the room camera's distance.
    True in the frames.
  - The row knobs on the open card are dark on dark. True: the notch shows, the disc barely.
  - LUMA's strip is see-through and the scene shows through it.
  - "Watch which door it goes through" stays up while the player is told to lift the
    lockdown. That is the line LUMA says once the owner is set, before the attack.
  - Smaller framing points: the dot bars at the top over the door frame and window; the
    Details panel over the medal; a door sign cut at the edge of the briefing's corridor
    picture; the close camera cutting the monitor at the left edge.
- **Its three changes**: search the room with the hands and carry the missing thing to the
  board; set the dials on the plates with labels that read, and drop the card, the briefing
  card and the Details panel; make the attack pay off both ways (the attacker running off with
  the item, or reacting when the door holds) and give the player a body or a role.

None of this was acted on after the test: the build committed is the one in the frames.

## What changed in rounds 1 and 2

1. **Verbs, not forms: every row is a thing on the room's prop** (`World/RoomRig.cs`). The
   drawer of chips is gone from all twelve rooms. An asset, an account, a message, a rule or a
   step is a token on the hero: a card on the rolling board, an envelope in the sorter's
   pigeonholes, a badge at the turnstile, a ticket on the bench, a lever module in the
   firewall's rack. Each token carries its short name on a dark plate (the same plate as Robot
   Racing's). Each decision is a switch on its token: a lever for two choices, a dial for
   more, with a lamp that is amber while the decision is open, teal once made, red for the
   choice that closes or flags (Block and log, Suspicious, Remove, Must change). Tap a token to
   bring it close: its full name and its line show on a plate under the grid, and each switch
   shows its name and its value; tap a switch to turn it. Rows that must be ordered are a
   rack: drag a module up or down and it swaps places as it passes. The room's one-off
   decisions sit on a console beside the grid with their names at rest; a record that must be
   signed has a pad under it. What is not on the list yet lies about the room (the folder of
   applicants' papers on a crate, stamped SHARED DRIVE; the lobby printer; the canteen menu on
   the wall): tap it to pin it to the board, Unpin on the card to send it back. Everything
   writes the same `RoomState` the chips wrote, so the checks and the platform report read the
   same values. No model was bought: tokens and switches are built from the kit's own
   materials (the door's steel and trim, the lamp's lens, the sign plate).
2. **The world shows the state.** The board of lamps on the wall follows the tokens; the
   levers and dials show the setup as handed over (the firewall's five levers all up, the
   allow-all's lamp lit; the risk matrix's dials all at the same notch).
3. **The corridor monitor** (`Building.Stage.cs`): a second camera at the corridor's entrance
   renders the corridor, one frame in four, onto a monitor on each room's back wall. While the
   player works the attacker walks the corridor door by door toward this room, waits outside it
   and tries the handle now and then; it never attacks before "Run the attack".
4. **The rehearsal.** The first attack on a room with a planned mistake, as handed over and
   untouched, runs as a rehearsal: the chase, the door giving way, "REHEARSAL · NOT COUNTED"
   under the door lamps, nothing recorded or reported, and "Back to the room" instead of the
   debrief. Back in the room the objective reads "Fix it, then run the attack again.", LUMA
   says what the attack took ("The attack takes whatever is not on the list.") and the thing
   the mistake lives in pulses teal. The next run counts.
5. **The briefing is a transmission**: the corridor from the ceiling camera with this level's
   door lit amber, "Night shift" and the room, three short lines typed in (one line of story,
   the level's own sentences), one button ("Enter the Risk Room"). The YOU FIX box, the twelve
   room chips and the ladder are gone.
6. **One line, one menu.** An objective of at most eight words at the top left
   (`Core/Script.cs`); Esc or the icon opens the menu (`Ui/GameMenu.cs`: the briefing, the
   file, this level again, sound, LUMA's voice, the title, the games menu). Briefing, The file,
   Hide/Open, Title screen and Games menu left the screens. The play screen keeps Camera (up to
   the prop and back) and Run the attack.
7. **LUMA says one short thing when something happens** (`Ui/Luma.cs`): the planned mistake's
   name a few seconds after the room is entered ("An asset nobody listed."), the first sentence
   of her live line after the attack's first banner, the attack's summary at its end, the
   first sentence of the takeaway on the debrief. Shown whole, never typed mid-sentence, 15
   words or fewer, recorded with the other lines (71 recordings; the old intro and live
   recordings that no screen says any more were removed). Her full lines and the curriculum
   are in the file.
8. **The attack** keeps its chase, sparks and banners; its HUD is the row of twelve door lamps,
   nothing else. The "What it tried" column moved to the debrief's Details.
9. **The debrief is a moment, then a door**: the corridor outside the room, its door shut and
   teal (or hanging open and scarlet), the attacker turning away down the corridor, a podium
   before the door with the medal rising on the step it earned and its stars popping one by
   one (Robot Racing's podium, built from the kit's steel and lens), LUMA's one line, Next and
   a quiet Again. The attack's lines, the checks, the stars in all and the rank, the question
   for work, the capstone's if-then plans and the file sit behind Details.
10. **The title**: the facility from the corridor camera, one big Start / Continue and a small
    Levels that opens the facility map (twelve room cards in the corridor's order, each level
    a step with its stars): the one ladder in the game.
11. **Two bugs from the pilot, checked**: 2.4's allow-all rule is the fifth module of the rack,
    on screen with the other four, no scrolling anywhere; the attack's first banner waits until
    LUMA has finished (`Luma.Busy`), and her line now comes after the banner, whole.

## How the checks are met

Unchanged in `Rooms.cs`; the actions write the same picks, order, added set and signature:

| Check | Met by |
|---|---|
| 1.1 every asset on the register; the unlisted one found | the folder pinned to the board (a decoy pinned fails the check, as before) |
| 1.1 an owner and an impact in the three terms | the Owner dial and the Leaked, Changed, Gone dials on every card |
| 2.4 no allow-all; administration unreachable from the public side | the Anything → Anywhere and Internet → Admin levers thrown to Block + log |
| 2.4 each allow has a reason; blocks logged by the last rule; the service still works | the Reason dials; the allow-all module dragged to the bottom; the three service modules left on Allow |
| a dated, signed record (2.1, 2.2, 2.3, 2.5, 2.6, 2.9) | the pad beside the grid |
| the room-wide decisions (1.4 guesses, 1.5 the summary, 2.3 crossings, 2.7 the sign-in, 3.1 severity, 3.4 trigger and cause) | the console card, last in the grid (round 3) |

The platform report is unchanged in form: the same `id:result` string, stars and check list
(`ResultsScreen`), from the same checks on the same state. A rehearsal reports nothing.

## Words and controls, before and after (round 2)

Counted the same way as the inventory (`inventory/tech-escape.md`) and Robot Racing's walk:
every visible word with a letter or a digit, every button the player can press; the world's
own labels count when the camera sees them (the token and switch plates), the kit's door
signs and wall stencils do not (signage, as in the inventory). Every number below is from the
round-2 walk, all nineteen levels, `Builds/look-audit/game-feel/final-round2/counts.jsonl`.

| Screen | Before: words / controls | After: words / controls | Limit |
|---|---|---|---|
| Title | 78 / 22 | 35 / 3 (Start, Levels, the menu) | – |
| The facility map (Levels) | – (it was the title) | 48 / 21 (19 levels, Close, the menu) | – |
| Briefing 1.1 | 142 / 21 | 44 / 3 (Skip, the menu, Enter) | 50 |
| Room 1.1, as handed over | 145 (+90 below the fold) / 14 (+18) | 22 / 3 (Camera, the menu, Run the attack) | 30, 3 + main |
| Room 1.1, a card brought close | – | 47 (the card's full line; while it is close) | – |
| Room 1.1, set right | – | 25 / 3 | 30, 3 + main |
| Rehearsal 1.1 (the attack, the end) | – | 11, 18 / 1, 2 | 30 |
| Attack 1.1 (early, the end) | 34, 47 / 0, 1 | 7, 9 / 1, 2 | 30 |
| Debrief 1.1 | 108 / 5 | 21 / 4 (Details, Again, Next, the menu) | 30 |
| Debrief 1.1, Details open | – | 95 / 6 | – |
| Briefing 2.4 | 137 / 21 | 36 / 3 | 50 |
| Room 2.4, as handed over | 158 (+13 below the fold) / 20 (+4) | 30 / 3 | 30, 3 + main |
| Room 2.4, a module brought close | – | 44 (its full rule) | – |
| Attack 2.4 (early, the end) | 25, 49 / 0, 1 | 0, 10 / 1, 2 | 30 |
| Debrief 2.4 | 115 / 5 | 16 / 4 | 30 |
| The menu | – | 51 / 11 | – |
| The file | ~470 | 457 / 5 | – |

All nineteen levels:

| Level | Briefing | Room as handed over | Card close | Room set right | Debrief |
|---|---|---|---|---|---|
| 1.1 | 44 | 22 | 47 | 25 | 21 |
| 1.2 | 39 | 24 | 52 | 24 | – |
| 1.3 | 44 | 20 | 52 | 20 | 12 |
| 1.4 | 45 | 23 | 40 | 23 | – |
| 1.5 | 32 | 30 | 60 | 30 | – |
| 2.1 | 45 | 21 | 34 | 21 | – |
| 2.2 | 44 | 28 | 45 | 28 | – |
| 2.3 | 43 | 30 | 37 | 30 | – |
| 2.4 | 36 | 30 | 44 | 30 | 16 |
| 2.5 | 44 | 20 | 33 | 20 | – |
| 2.6 | 44 | 19 | 25 | 19 | – |
| 2.7 | 39 | 30 | 34 | 30 | – |
| 2.8 | 39 | 23 | 26 | 23 | – |
| 2.9 | 36 | 23 | 29 | 23 | – |
| 2.10 | 46 | 23 | 39 | 23 | – |
| 3.1 | 37 | 18 | 21 | 24 | – |
| 3.2 | 39 | 19 | 26 | 19 | – |
| 3.3 | 46 | 24 | 39 | 24 | – |
| 3.4 | 36 | 30 | 49 | 30 | – |

Every play screen is within 30 words and 3 controls plus the main action; every briefing within
50; every debrief within 30 with Details closed. A card brought close shows its full name and
line on purpose (it is the file's words on the thing, for as long as it is close: 21 to 60
words). Every LUMA line on every screen is whole and 15 words or fewer (eleven different lines
on the walk's screens). No OPENS AT, ladder, coordinates, seed or probability on any play or
briefing screen; the level map (19 steps) is behind Levels on the title only. The prop fills
30 to 31 % of the frame in every room, as in the realism pass.

## Web (round 2)

| | Realism pass (7 Nov) | Game-feel pass (8 Nov) |
|---|---|---|
| Transferred | 44.5 MB | 42.0 MB |
| Load (headless Chrome, software GL, the worktree's own server on port 8086) | 3.7 s | 4.1 to 5.4 s (four measurements this session) |
| data / wasm on disk | 34.8 / 9.3 MB | 32.2 / 9.4 MB |

Under the 45 MB limit with 3 MB in hand (the thirty-eight old recordings of LUMA's intro
and live lines went; the seventy-one short cues weigh less). Loads and runs on the platform
(`Builds/look-audit/game-feel/platform/platform-tech-escape.png`). The web build's size was
measured by the platform's own page in headless Chrome, as the earlier passes measured it.

## The reviewers, round 1

Two fresh agents, given only the pictures (`Builds/look-audit/game-feel/reviewers/`).

**The player test** (level 1.1's thirteen screens in order). It read the game right: "a
security-training game: night shift at the Harbor Permits Office, every door locked; on the
board are the office's information assets, each with dials for owner, sensitivity and how
bad if changed, leaked or gone; Run the attack sends a hooded intruder through the building
that takes anything not on the board; the first run is a practice, it fails because a Shared
drive was never listed; you add it, run again, the door holds, three stars". It could say
what to do on eleven of thirteen screens. It could not on two: the room as handed over ("the
order says *Put every asset on the board* but five are already on it, and the only things to
touch are 25 dials with no labels") and the camera's close-up (it ended behind the office
chair). Felt like a form: the room (03, "a 5×5 grid of fields made of knobs"), the open card
(07, "a spreadsheet row with knobs instead of dropdowns"), the new card (08). Instructions:
the title's LUMA line and the briefing. Play: the attack ("the one payoff moment where it felt
like a game") and the medal. Broken, as it saw it: the open card's labels piled on each other;
the close-up behind the chair; the clock jumping back between screens and touching the menu
icon; a red bar floating by the monitor; eleven red dots in the attack's strip reading as
eleven failures; the hooded figure crouched at the podium "like it's stealing the medal"; the
Details panel see-through; asset, board, register, list and card used for one thing.

**The Claude Bomb test** (seven side-by-sides). RIGHT on the attack ("a hooded intruder at a
lit door in a red corridor... looks like GTA or a heist game; the best frame on either side")
and the debrief ("unmistakably a game's end screen"); LEFT on the title, the two rooms and the
open card; EVEN on the briefing. Its tells, most visible first: the room puzzles as forms and
tables (five plates of five dials, the firewall as a rule table with column headers when a
module is open); no stakes on screen (no countdown, strikes or progress); the briefing as a
lesson slide; the maxim said twice; web-style button pairs; the menu icon. What already reads
as a game: the attack, the podium, the security-camera framing, the corridor monitor with the
intruder in it, full-screen 3D with no sidebar. Broken: the plates float rather than mount;
2.4's third rule in a smaller font than the others; the crate's levers poking between the
modules; the open module's labels tiny and tilted; the debrief's door sign cut by the frame;
a dark red smear under LUMA's line reading as blood (the attacker's trail). Two of my pairs
were not like for like (Claude Bomb's title mid-fade stood in for its briefing, its briefing
for a debrief); round 2 pairs the briefing with its "Incoming transmission" and, Claude Bomb
having no debrief frame on disk, the debrief with its title.

## What changed after round 1

| Reviewer said | Done |
|---|---|
| 1.1's objective does not match the board; nothing to find | "Find what is missing from the board."; the loose things (the folder, the printer, the menu on the wall) glint now and then; after pinning: "Set its owner. Then run the attack." |
| The open card's labels pile up; tapping a card does not come closer | Tapping a card or module brings the camera to it along the room camera's own line, the card grows by half, its switch names and values stagger, its full name and line stand under it |
| The close-up ends behind the chair | The camera closes in along the room camera's line of sight, so nothing the feed does not show can come between |
| No progress or stakes on screen | A row of lamps at the top of the room, one per row on the prop and one for the console and the pad, amber until set, teal once set (the attack's door lamps, Claude Bomb's module dots) |
| The rack's third rule in a smaller font; levers poking through; plates floating | One text size for every module; the modules mounted on a dark rack panel with two rails, 2 cm apart |
| Eleven red dots read as failures | Doors not yet reached are dim; amber this room, teal held, scarlet breached |
| The clock jumps back and touches the menu icon; a red bar by the monitor | One clock for the night, whatever the screen, clear of the icon; the monitor's REC lamp was a deep box sticking out of the screen, now flat |
| The intruder crouching at the medal; the red smear; the door's sign cut; Details see-through | The intruder already walking away down the corridor; no trail on the debrief; the camera higher; Details opaque, Close clear of the menu icon |
| Medal: a flat disc, no stars | The stars had been culled (their triangles faced the wall); now three stars in an arc over the disc, lit as earned |

Not done, as beyond the pass or the owner's to decide: a countdown in the rooms (the design
says the attacker is never punishing and never attacks early); per-room physical objects in
place of the generic levers and dials (see Waiting for Charbel); the briefing's third line,
the level's maxim, approved in the design.

## The reviewers, round 2

**The player test** (a fresh agent, 1.1's thirteen screens after the fixes). It told the whole
level back: "make a whiteboard list of the office's information complete and correct before
an attacker is let loose; run a practice attack; a hooded figure walks into the Risk Room and
steals the one thing that wasn't on the board; add the Shared drive, give it an owner, run
the real attack; the door holds; gold medal". It now names what to do on ten of thirteen
screens. It could not tell on three: how to find the missing thing in the room as handed over
("nothing in the room points to it"), whether the owner was set on the new card (the
objective still said "Set its owner"), and what the close-up was for. Felt like a form: the
room (03, "a table of rows and columns dressed up as a whiteboard") and above all the open
card (07, "a five-field record with knobs instead of dropdown menus"). Instructions: the title
and the briefing. Cut-scenes rather than play: the two attacks ("good contrast between the
door left open and the door that held, but I only watch"). Broken, as it saw it: the open
card still overlapping its neighbours; the knobs unlabelled at rest; LUMA's attack line said
while the owner was being set; the overview after the attack dark and tiny, the win and the
loss looking alike; the takeaway said three times; the podium's second and third steps in a
one-player game; a feed bracket poking above Run the attack; LUMA's strip touching the
Camera button.

Acted on before the last build: the loose things in the room pulse with a small lamp of their
own (the folder, the printer, the menu: one of them is the asset); the objective moves on to
"Run the attack." once the owner is set and LUMA's "Watch which door it goes through." comes
then; the attack ends held on the door (shut and teal, or hanging open and scarlet) instead
of the overview; Details no longer repeats the takeaway and spells out "stars in all"; the
feed's lower brackets go where the buttons are; LUMA's strip sits clear of the Camera button.

**The Claude Bomb test, round 2** (a fresh agent, the side-by-sides with the briefing paired
against Claude Bomb's "Incoming transmission"). RIGHT on the attack ("a real game cutscene and
the best screen on either side") and the debrief (by default: Claude Bomb has no debrief frame
on disk, so its title stood in); LEFT on the title (narrowly), the briefing (clearly: "the left
is an incoming radio message with a threat, a deadline and a role; the right is the start
screen of a course module"), the two rooms ("the right has the better full-screen room and
less interface, but what you play is a whiteboard of labelled tiles, the clock counts up, and
nothing is at risk") and the open module (decisively). Its tells, most visible first: the
firewall box as a rule table (arrows, dots, port numbers in brackets); the close-up as a form
and a glossary; the title as a landing page; the maxim on the briefing and the debrief; the
briefing's welcome with no threat, deadline or role; the 1.1 board as an asset checklist;
"Run the attack" as a Submit button; the dots at the top as a page indicator; no pressure.
What reads as a game: the attack, the podium and medal, full-screen 3D with no sidebar, the
camera framing and the corridor monitor, the lighting and props, the firewall box's five status
lamps, the objective line. Its three changes: a countdown with consequences instead of the
Submit button (the attack comes when the clock runs out or when a lever is pulled); every table
an object (cables between labelled sockets, a key cabinet with empty hooks and name tags; the
words off the panels); open with the intruder, not the maxim (the title, the briefing as an
alarm).

Acted on before the last build: the open module's full rule reads above the rack instead of
over the next module, the module's switch names and values at the console's size; the title
smaller, clear of the camera frame; the debrief's LUMA line bottom left and narrow, clear of the
podium; the medal's lamp gone (it threw a yellow pool on the floor). Not acted on, as the
owner's call (see Waiting for Charbel): the countdown, the objects in place of rule rows and
dials, the briefing rewritten as an alarm, the maxim off the briefing.

## Where Tech Escape stands

All nineteen levels run on the new screens with Charbel's round-4 answers and the Higgsfield
files in. Every machine check of the brief holds on every screen of the last walk again: the
knob values are off the board, so every room is within 30 words. On 1.1 the unlisted drive is
found by turning the camera, glints as the camera passes it and flies to the board; the
attack ends with the attacker running off with the drive, or giving up at a door that held.
2.1 and 3.1 carry the staff badges, 2.4's rules stand on the patch panel, and the web build is
43.2 MB.

The player test, round 4, tells the level back whole and calls two of the new moments the
best in it: finding the drive, and the attacker running off with it. It still reads the card
brought close and Details as forms and the briefing as instructions, and it asks for
hands-on actions in place of the card's rows.

Round 5 then set the lamps to "every decision made" and moved 2.9's monitor off the
lettering (above).

Known in the last frames, not fixed:
- LUMA's strip is see-through: left for the shared fix after the merge, as asked (opaque since, Phase 7e).
- The open card's lamp and the board's lamps are small from the room camera.

## Waiting for Charbel

1. **The patch panel is the rack's bottom unit, not the rack.** The model came as a 2U panel
   full of cables; above the rules its cables would cover them, so it sits below. A panel with
   five labelled sockets and one cable per rule (the first spec) would let each rule be its
   own cable; that would be a new model. Placeholder: the panel under the five modules.
2. **No Claude Bomb debrief frame on disk**, and the Claude Bomb test was not run again in
   rounds 3 and 4; you asked for the player test.

Done after the merge (Phase 7e): Robot Racing's podium has the single step, and LUMA's strip
is opaque in all three games.

## Files

- Code: `games/tech-escape/Assets/Scripts/Core/Script.cs`, `World/RoomRig.cs`,
  `World/Building.Stage.cs`, `Ui/GameMenu.cs`, `Ui/Hud.cs` (new); `Core/Facility.cs`,
  `Ui/App.cs`, `AttackScreen.cs`, `CameraFeed.cs`, `DemoDriver.cs`, `FileSheet.cs`,
  `LessonScreen.cs`, `Luma.cs`, `ResultsScreen.cs`, `RoomScreen.cs`, `Sfx.cs`, `TitleScreen.cs`,
  `UiKit.cs`, `World/AttackerRig.cs`, `World/Building.cs`, `World/CameraRig.cs`,
  `World/SceneCamera.cs`, `Editor/FacilityKit.Prefabs.cs` (changed); `Resources/LumaVoice/` (72
  recordings).
- The Higgsfield files: `Assets/Models/Hero/PatchPanel/` (geometry and maps),
  `Assets/Resources/Facility/Hero/PatchPanel.prefab`, `Assets/Resources/Badges/` (six badges),
  `Tools/glb_maps.py` (the cleanup tool), recorded in `games/tech-escape/Docs/credits.md`.
- Frames (not in git), under `games/tech-escape/Builds/look-audit/game-feel/`:
  - `final/`: the last full walk, round 4, 130 pictures and `counts.jsonl`.
  - `round5/`: round 5's walk of 1.1, 2.4 and 2.9, 38 pictures.
  - `final-round3/`, `final-round2/`: the earlier rounds' walks (round 2's with `side-by-sides/`).
  - `reviewers/`: what each reviewer saw; `player-round3/answer.md` and
    `player-round4/answer.md` are the last two answers.
  - `claude-bomb/`: the reference frames.
  - `platform/`: the game on the platform, rounds 2, 3 and 4.
- Docs: this section (the branch's report until the merge); `Docs/game-feel/tech-escape.md` (the owner's answers recorded, rounds 2
  to 4); `games/tech-escape/README.md`.

---

# Phase 7e: after the merge, and the Claude Bomb test on all three games

8 November 2026, on `game-feel-pass`. `game-feel-pass` was first fast-forwarded to `main`
(25e69fc, the merge that brought Claude Bomb's own history into `games/claude-bomb`; it changes
no file), then `game-feel-ai-arena` (3d9e5df) and `game-feel-tech-escape` (05a36b9) were merged
in without conflict: each branch had changed only its own game, its report and its design doc.
The two reports are Phases 7c and 7d above; `Docs/game-feel/ai-arena-report.md` and
`tech-escape-report.md` are gone, and the links to them point here. The frames each worktree
kept out of git moved into the main folder under the same paths (Tech Escape's worktree
`walk1/` as `walk1-worktree/`, beside the older `walk1/`), and the two worktrees were removed.

## The two shared fixes

1. **LUMA's strip is opaque in all three games.** `Ui/Luma.cs` builds every line of hers
   (`Line`, which `Cue` calls too); its dark strip was the brand ink at 84 % and AI Arena's
   light one its panel colour at 93 %, so the scene showed through. Both are now solid; the
   strip still fades in and out as before. Nothing else draws her line.
2. **Robot Racing's podium is a single step** (`World/Circuit.Play.cs`, `Podium`), as in AI
   Arena and Tech Escape: one car races, so there is no second or third place. The step keeps
   the old first step's size (0.9 by 1.0 m, 0.36 m high), with no number on it; the car is
   driven onto it with any medal (with none, it stands beside it, as before), and the medal
   hangs from the middle of its front edge. `games/robot-racing/README.md` says so.

## Builds and walks

Mac and web builds of all three, no compile errors. Web on disk: Robot Racing 31 MB, AI Arena
36 MB, Tech Escape 41 MB (the transfer was not measured again; the code change is a few lines).
One walk per game under the screen lock, the levels the earlier rounds compared: Robot Racing
1.1 and 2.3, AI Arena 1.1 and 1.5, Tech Escape 1.1 and 2.4, in
`games/<id>/Builds/look-audit/game-feel/after-merge/` with `counts.jsonl`. The counts are the
last walks' on every screen but these: Robot Racing's debriefs one word fewer (the step's "1"
is gone) and 1.1's set pit lane 10 (11 in `final3/`); AI Arena's 1.1 errand 23 words (8 before:
LUMA's line was on screen when this picture was taken); the menus, which show saved progress.
The only flags are where they belong: the seed and the chance in 2.3's training Details, and the
ladder on Tech Escape's level select, reached from the title.

## The Claude Bomb test

Run once, nothing changed after it. One fresh agent per game, given only eight side-by-sides
(`after-merge/side-by-sides/`, Claude Bomb on the left, a caption under each half) and the
brief's question ("Which one feels more like a game, and why? List what on the right feels like
an app, a form or a lesson, most visible first"), with a verdict asked per picture and anything
that looks broken. The left frames are Claude Bomb's realism-pass frames: its title, its
"Incoming transmission" for every briefing, its play screen for every play screen, and its play
screen again against each debrief (there is still no Claude Bomb debrief frame on disk, and the
caption said so).

| Game | RIGHT | EVEN | LEFT | Before |
|---|---|---|---|---|
| Robot Racing | 6 (both pit lanes, training, race, question "narrowly", debrief) | title | briefing | round 3: RIGHT 5 of 7, LEFT on the questions |
| AI Arena | 6 (briefing, 1.1 garage, errand, the agent's eye, home, debrief) | | title, 1.5 garage | round 2: RIGHT 7 of 8, LEFT on the title |
| Tech Escape | 6 (briefing, both rooms, the search, attack, debrief) | title | card close | round 2: RIGHT 2 of 7 (attack, debrief) |

### Robot Racing

"During play the right feels more like a game: full-bleed 3D, no side panel, a mission objective
and a dial like a racing HUD; the left's play screen is half chat app. The right loses at the
start: its title and briefing are web pages, and its spoken lines and quiz read as a lesson."
The debrief: "the robot on a podium under a light with a hanging star medal is a real reward
moment". The briefing lost to "Incoming transmission" with "You have 10 minutes" ("stakes"),
against "Race control" explaining the loop.

What reads as an app, a form or a lesson, most visible first: the title as a landing page and the
briefing as a text column; the web-style buttons along the bottom (Train, Skip ahead, Next, Again,
Camera, Details); the question as a quiz dressed in 3D; LUMA's lines as the lesson ("The robot
learns from the points, and from nothing else"); the score tags as "a spreadsheet column" with
nothing saying what each belongs to; the menu icon; the course wording ("GAME 03", "round 13").

Broken or cut, as it saw it: the menu icon over the title's clock; the question's prompt running
past its plate, a lamp pole in front of the answer boards; the gantry post clipping "-0.2" on 1.1
and a fence post the "+" of 2.3's "+2"; teal letters behind a blank board at the debrief's right
edge; the side sensors' teal wedges at the car's rear in the race; "LAP 4/5" without a plate; the
fuel dial without numbers.

### AI Arena

"The right feels more like a game: a full-screen 3D world with a character, a mission banner, a
third-person view, an agent's-eye camera and a medal." The briefing won with Mom at her door on
the home camera ("one short order"); the walk home ("arriving at the door while Mom waves and a
HOME banner appears feels like completing a mission"). LEFT on the title ("a web landing page...
a 1-7 stepper") and on 1.5's garage ("a table covered in word cards... a settings screen built
out of props").

What reads as an app, a form or a lesson: the flat teal buttons and outlined ones on every
screen; the title's errand row as "a checkout progress bar"; the garage table as a form or a
matching quiz (Sight, 10 turns, Notebook, Sonnet, Opus, Effort, Coins, No); LUMA's lines stating
the lesson ("Three brains: Haiku is small, Sonnet medium, Opus large."); the briefing's text
column; the HOME card's "7 on the list in the basket" as a stats line; the menu icon. It also
missed stakes on screen ("no timer, score or strikes").

Broken, as it saw it: the HOME card covering Mom's face and chest as she waves; the apples
running through the agent's feet on the step and the medal "flat on the plinth's front face, as
if pasted on"; on 1.5 the "3 turns" plate over the agent's note, the note cut with "...", "Haiku"
too small; in the agent's eye the floor tinted salmon (the sight square), the grey ground at the
world's edge, the agent's head cut at the bottom; past the house a bare plane; the camera's clock
(17:58) against Mom's "6:00 PM".

### Tech Escape

"The right feels more like a game. Play happens in full-screen 3D spaces with CCTV framing, a
short objective, spoken LUMA lines, a staged attack and a medal... It falls back to website at the
start and becomes a form at the ownership card." The briefing now wins ("fewer lines, a bigger
moody corridor scene and one clear action"), as do the rooms, the search and 2.4's patch panel
("a physical patch panel with lamps, cables and a server rack"); the attack is "pure game".

What reads as an app, a form or a lesson: the card brought close ("a modal of labelled Field: ?
rows", Owner, Sensitivity, If leaked, If changed, If gone); the title and briefing as a landing
page and a text column; the teal and outlined buttons ("Lift the lockdown" on every play screen);
the objective lines as worksheet steps; the motto and LUMA's homework lines; the patch panel's
rows as a firewall rule table (ports, a square on each row); the board's labelled sections; the
menu icon and the dots at the top as an onboarding stepper.

Broken, as it saw it: the card cutting three boards in half and a yellow square in its corner;
the menu icon, clock and REC corner over busy props on the search and the room; the blown-out
lamps on the SHARED device; 2.4's rule text small and slanted, its strips past the panel's edge;
a grey mesh in the attack's dark (a ceiling camera); the dots, 12 in the attack against 5 or 6 in
the rooms; a red sign cut at the briefing's edge; the medal "stuck flat on the crate's front face
like a sticker".

## What the three runs share

Each game now wins six of eight against Claude Bomb, on the play screens, the attack, the walk
home and the debriefs, which Claude Bomb's half-screen chat cannot match. What still reads as an
app is the same in all three: the title as a landing page, the teal and outlined buttons, the
menu icon, and LUMA's line read as the lesson (none of the three agents called her strip
see-through). Two medals read "flat, like a sticker" on the step's face (AI Arena, Tech Escape);
Robot Racing's, on the new single step, read as "a real reward moment". The places that still
lose are each game's densest decision screen: Tech Escape's card, AI Arena's 1.5 garage, and
Robot Racing's briefing. Nothing was fixed after this run; these findings are for the next round.

---

# Phase 7f: the last polish (LUMA on events, the world's edge, HOME clear of Mom)

8 November 2026, on `game-feel-pass` (recreated from `main` for this pass). Each game was
changed, built (Mac and web, no compile errors) and walked in full under the screen lock; one
player test per game followed. Nothing was fixed after the player tests.

## LUMA speaks when something happens

On play screens she now speaks only at a world event; at most one line on a briefing and one
on a debrief (the takeaway, as the medal arrives). Her title lines are gone too: they described
the screen. Every cut line is kept word for word in that level's file, in a new "LUMA" section
(Robot Racing `FileSheet.LumaLines`, AI Arena `Luma.OnTheWay` as "LUMA, on the way", Tech Escape
its `enter-`, `live-` and `watch-` lines); only the title's "Welcome back. Next: ..." is not
copied, since it names the next level and nothing else. No line was recorded again.

| Game | Kept on play screens | Cut from play screens |
|---|---|---|
| Robot Racing | the first crash ("Nobody is steering.", which used to open every training and now waits for the crash); the first finish ("One finish proves nothing: watch whether the curve stays up.", with the guess line after it on levels with a guess) | every pit-lane line (arrival, `cue-dark`, the broken robot's why and fix), "That is learning, not luck.", "Every move earns zero.", "It never finished. The tank ran dry.", the race's two cues, and the answers' "Right." / "Not quite." (the tapped sign or part already flashes teal or red with a chime or a thud) |
| AI Arena | an agent sent home (the first sentence of `Coach.Eliminated`, "Nadia is coming home early: it bought a cherry, and that ruins dinner."); an agent out of tokens ("Nadia ran out of tokens on turn N."), kept because the agent stops and dims under OUT OF TOKENS | the four story shots' lines (the shots are now wordless, 3.2 s each), every garage line, 1.6's decide line, the first turn, the rounds, the off switch, the first buy ("Nadia bought an apple..."). The agents coming home had no line and have none |
| Tech Escape | the attack's end, theft or held door ("The attacker took the one thing nobody had written down.", "The register held: nothing unlisted to take.", "Blocked, and the block is in the log." ...), the rehearsal's end included | the room's `enter-` and `live-` lines, the drive's `watch-1`, the attack's opening cue ("Watch which door it goes through.") |

Briefings: none of the three had a LUMA line on the briefing, and none has now. The walks'
`counts.jsonl` confirm it on every screen: LUMA's line on a play screen only at those events,
one line on each debrief, never two at once. Robot Racing's title came down from 40 words to 29.

## The edge of the world

In all three games the fog the code already set never reached a build: Graphics settings
stripped the fog shader variants ("Automatic", and no scene saves fog). Each
`ProjectSettings/GraphicsSettings.asset` now keeps the linear fog variant (custom stripping).

- **Robot Racing**: linear fog from 30 to 75 units in the sky dome's horizon colour (the largest
  track reaches 9 units from the centre, so the play area is clear), the dome's lower half and
  the camera background in the same colour, and a ring of the field's grass out to 280 units.
  The field and its lamps are untouched. Web 33 MB on disk (31 before: the fog variants).
- **AI Arena**: linear fog in the dusk sky's navy, starting 34 m beyond the camera's distance
  from the square's centre and full 16 m later, so nothing inside the ring of houses hazes; two
  of the kit's houses close each of the four open corner streets, 34 m out. The gap past Mom's
  house went from grey to the sky's navy. Web 36 MB.
- **Tech Escape**: only the overhead shots see outside. The kit's ground outside is darkened and
  matte (the pale plane was the moon's sheen at a grazing angle) and fades into linear fog of
  the same colour (`Look.NightOutside`, also the overview's background), set each frame from
  just past the building's farthest corner (`Building.EdgeFog`), so no room is fogged. Web 41 MB.

## AI Arena: HOME clear of Mom

`ClearOfMom()` projects Mom's bounds, her wave included, and places the HOME card on the side of
her with more room, narrowed to fit (above or below if neither side fits). On 1.1 and 1.5 it
stands to her right, clear.

## Walks

`games/<id>/Builds/look-audit/game-feel/polish/` in each game: every level (Robot Racing 13, 78
files; AI Arena 7, 52 files; Tech Escape 19, 131 files). AI Arena's fog was tightened after its
full walk: 1.1 and 1.5 were walked again on the final build (`polish-fog/`). Every screen keeps
the brief's limits as before; the flags are the ones that belong (2.3's training Details,
Tech Escape's level select), and Tech Escape's close-up cards stay at 32 to 72 words as in round
4. Not seen in a frame: Tech Escape's attack opening shot (the walk does not stop there), and
AI Arena's five other errands on the final fog values.

## The player test, once per game

Fresh agents, level 1.1's frames in order, the brief's question.

**Robot Racing** (`polish/`, 16 frames): "A teal robot race car... nobody drives it. You give it
points... and watch it teach itself, crashing at first and later finishing laps... quizzed on how
a robot learns from points, and the car gets a medal on a podium." Playing: the pit lane (the
flag), the training ("Nobody is steering." under it), FIRST FINISH ("feels like a reward"), the
race, the race over, the podium. Form or instructions: the seven questions in a row ("nothing
happens in the world between them"; in 1 to 5 it could not see the choices: the glowing car and
parts were not read as answers), the briefing's text panel, and the set pit lane's numbers
("bare numbers stacked in the air"). Broken, as it saw it: the questions' prompt running past
its plate; the gantry post and a lamp post hiding the answers' first letters on question 7 and
the "-" of "-0.2"; the menu icon on the title's clock; the side sensors' teal wedges under the
car; teal letters cut behind a board at the debrief; the finish banner letting the car show
through; LUMA's "the curve" with no curve on screen.

**AI Arena** (`polish-fog/`, 14 frames): "You run a little robot errand-runner... Mom wants apples
and nothing else. You pack the robot... send it into a night market, and watch... At the end you
get a medal." Playing: the title, the story's walk into the market, the agent out, the street,
the agent's eye ("a nice touch"), the walk home ("feels like playing"), the medal ("feels like a
reward"). Form or instructions: the briefing ("a rules lecture" in a navy panel), the garage
("a checklist made of props"; the list already says "Buy apples.", so it could not tell what it
chose), Mom's message on the phone ("the third time"); from the street on, "watching a replay".
The wordless story shots 1 and 4 it could not read. Broken, as it saw it: AGENT OUT over the
thought bubble; yellow-green discs at the agent's feet; the "Buy apples." card over the battery
and the apple on the basket's rim; the salmon sight square in the agent's eye; the apples
reading as tomatoes; the agent a speck on the overhead; "4 on the list in the basket" small and
awkward. HOME over Mom is no longer among them.

**Tech Escape** (`polish/`, 18 frames): "A night-shift security agent at the Harbor Permits Office
during an intruder lockdown... make sure the Risk Room's board lists everything... A practice
break-in shows you what's missing: an unlisted shared drive. You add it to the board, the real
attack bounces off, and you earn a medal." Playing on fourteen of eighteen; the search is "the
best moment". Form or instructions: the briefing, the card brought close ("a database record,
not an object you handle"; the knob changes nothing in the room; nothing says what "Sensitivity"
or "If gone" mean), and the objective strip as step-by-step instructions. It also read the flow
as letting the lockdown lift with four fields still "?" (the lamps turn teal only once all five
are set, so the walk set the other four between those two frames). Broken, as it saw it: the
corridor monitor frame small and soft; the grey shape near the ceiling (the ceiling camera); the
clock over the cork board; the card cutting three panels and its yellow square; "Dana" too small;
panels past the board's edge; the twelve dots with only one changing; "RISK RO..." cut at the
edge; the medal flat on the crate.

## Where the three stand

LUMA now waits for something to happen, the edge of the world no longer shows, and HOME clears
Mom; no player test mentioned LUMA's lines as a problem, the horizon, or HOME over Mom. What all
three player tests still name is what earlier rounds named: the briefing as a reading panel, and
each game's densest decision screen as a form (Robot Racing's questions and point values, AI
Arena's garage table, Tech Escape's card). Those are the next round's, if there is one.
