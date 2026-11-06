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
