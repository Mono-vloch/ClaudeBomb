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
