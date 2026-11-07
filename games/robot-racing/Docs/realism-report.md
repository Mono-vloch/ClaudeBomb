# Robot Racing: the realism pass

7 November 2026, branch `realism-pass`, from `Docs/realism-pass-prompt.md` (the Robot Racing
section) and the measured target in `Docs/claude-bomb-recipe.md`. Tech Escape's facility kit
(`games/tech-escape/Assets/Editor/FacilityKit*.cs`) is the pattern copied. What the game
teaches did not move: LUMA's lines and voice files, the curriculum text, the words the robot
reads, the level logic, the scoring and the platform reports are the strings and the code they
were; this pass changed what the world is made of, how it is framed and how the UI sits on it.

## What changed

1. **A track kit made in the Editor** (`Assets/Editor/TrackKit.cs`, `TrackKit.Prefabs.cs`,
   menu Robot Racing > Build track kit), the way Claude Bomb's `LabBuilder` and Tech Escape's
   `FacilityKit` work: URP Lit materials from CC0 texture sets (Poly Haven, ambientCG) and from
   textures painted with their grain (`Environment/Generated`), every texture crunched (quality
   50), metal-smoothness maps dropped where a model has no metal, shapes made as meshes with
   UVs in metres, and prefabs in `Resources/Track` (25 of them). The kit is in metres; a cell of
   the grid is 2 m, so the world places it at half scale.
2. **The circuit is a real small track** (`Assets/Scripts/World/Circuit.Kit.cs`), placed by
   `Circuit` from each track's grid: one asphalt mesh over the road cells with UVs in metres
   (Poly Haven `asphalt_track`, no seam at the cells), painted edge lines, a start line and a
   grid slot; red and white kerbs on the outside of every bend and both sides of its inside
   corner; jersey barriers with a yellow and black hazard band along the straights; tyre walls
   (Poly Haven `old_tyre`) with a red and white belt on the outside of the bends and at the end
   of the finish, a stack of tyres in each bend's outer corner; the start gantry with its red
   lights over the back of the grid; the pit wall along the first straight with a chain-link
   fence, the pit lane behind it (Poly Haven `asphalt_02`), a pit box (tool chest, cart, tyre
   pump, extinguisher, barrel, spare tyres), cones, the covered grandstand behind; the
   chequered finish line with a flag each side; three floodlight masts; trees (bark trunks and
   crowns of leaf cards, lit as one volume) in the infield and round the circuit; grass clumps
   along the barriers and the trees; a grass field with large-scale greener and drier patches
   (URP Lit's detail albedo over Poly Haven `sparse_grass`); a sky dome. When the kit is missing
   (`Resources/Track/Road` or `Car` absent) the old blocks are built as before.
3. **The robot car is a hero model** (Higgsfield body and wheel): bevelled aqua and white
   panels, a dark canopy, the lidar pod on the nose with its camera lenses, glowing ice-blue
   eyes and an amber antenna lamp (emission from the model's own bright pixels), the "07" number
   roundel on each side, and four wheels as their own objects. `Assets/Scripts/World/CarRig.cs`
   spins the wheels with the distance covered, steers the front pair with the rate of turn,
   leans the body out of a bend and squats it at speed; it reads the car's move every frame, so
   it works at every training speed. The old sensor rays now run low over the asphalt from the
   pod, translucent, like a scanner's lines.
4. **Evening practice under the floodlights**: the sun low and warm across the track (soft
   shadows), a navy and violet ambient, exponential-squared fog in the dusk colour, the
   floodlights as the warm key on the grid, a reflection probe rendered once after the lamps
   are on; the cover (title) at dusk with the sun all but gone. Post as Claude Bomb's: neutral
   tonemapping, bloom 0.25 from a threshold of 1, black vignette 0.2, no grading on top
   (`World/Atmosphere.cs`).
5. **The camera is low and close by default; the overview is the exception.** The road is one
   cell wide between barriers, so every camera stays on it: the pit lane's portrait in front of
   the car (1.05 cells ahead, 0.34 up, 42 degrees), the run's chase just behind it (Cinemachine,
   0.8 behind, 0.38 up, 42 degrees, light damping, warped with the car when a new attempt sends
   it back to the grid) as the default, "The track" button for the overview; the briefing low
   behind the car looking down the first straight; the title and the debrief a slow swing in
   front of the car; the finish's slow-motion move a swing behind it. Sparks are short streaks
   and dust is off at this distance (it bloomed in the lens).
6. **The play screens' panels are dark** (as Tech Escape): the run screen and the pit lane keep
   the world on the left two thirds and put everything else in a dark glass drawer of 640 px
   (33 % of 1920) that folds to a 72 px tab; the run's HUD and buttons sit on glass over the
   world; the chart has a dark variant; the debrief, its questions, the warm-up question and the
   settings sheet are dark glass (they were white cards). Every label is 16 px or more at 1080p
   (the eyebrow labels were 13, the rule book 13, the chart's labels 10 to 12). The title wears a
   trackside camera's frame (`Ui/CameraFeed.cs`: LIVE dot, CAM 3 / THE GRID, a running clock,
   the circuit's name), Claude Bomb's CAM 02 borrowed. The file stays a white sheet: it is the
   paper prop.
7. **The screenshot walk measures** (`Ui/DemoDriver.cs`): the car's coverage of the frame
   (its projected bounding box, as Tech Escape's), and on every key screen the machine checks
   below.

## Assets

Every asset with its source and licence, and every Higgsfield job with its cost: `Docs/credits.md`.
In short:

- **Higgsfield** (5 models): the robot car's body with empty wheel arches, one wheel (used four
  times), the start gantry, the covered grandstand, a pit wall section.
- **Poly Haven** (CC0): textures `asphalt_track`, `asphalt_02`, `sparse_grass`, `concrete`,
  `pine_bark`, `blue_metal_plate`, the `modular_chainlink_fence` wire maps and the
  `grass_medium_01` atlas; models `old_tyre`, `tire_pump`, and from Claude Bomb
  `metal_tool_chest`, `industrial_storage_cart`, `korean_fire_extinguisher_01`, `Barrel_01`.
- **ambientCG** (CC0): `Leaf001`, composited into the trees' leaf-cluster card.
- **Made here** from those: the painted textures (line paint, kerb stripes, the barrier's hazard
  band, the chequer, the number roundel, the tyre belt, the cone's band, the grass patches) and
  the meshes (barrier, kerb, cone, trees, grass cards, floodlight, poles, fence, sky dome).
- Not used: **Kenney's Racing Kit** (its flat-coloured low-poly pieces would sit beside the PBR
  ones like toys; the barriers, kerbs and cones are made in the kit instead), Poly Haven's trees
  (0.3 to 17 million triangles each), its shrubs and the gravel texture (downloaded, tried,
  removed).

## Higgsfield credits (my ledger)

| # | Job | Id | Credits | Running |
|---|---|---|---|---|
| 1 | design image: car body (GPT Image 2.6) | `13bde82d-2e37-44a9-b287-e731a323af46` | 0.25 | 0.25 |
| 2 | design image: wheel | `ebd56b1c-2bef-4010-a57b-3d45d3e6aff4` | 0.25 | 0.50 |
| 3 | design image: start gantry | `8466ab12-21a5-4334-84bc-c8cdd4e2d212` | 0.25 | 0.75 |
| 4 | design image: grandstand | `293a3d63-0e89-47fa-a35f-67b36ae2ec6e` | 0.25 | 1.00 |
| 5 | design image: pit wall | `2330d956-dfa6-41de-b4b1-a4647f12693a` | 0.25 | 1.25 |
| 6 | 3D car body, detailed (Tripo H3.1, PBR) | `9d2ab66c-38c7-48d9-adca-977b945bac64` | 18 | 19.25 |
| 7 | 3D wheel, standard | `8040d163-411a-47c0-a67c-0144d5d43a86` | 12 | 31.25 |
| 8 | 3D start gantry, detailed | `1455211c-6b9e-4022-8475-73c4d5c21458` | 18 | 49.25 |
| 9 | 3D grandstand, detailed | `2eda57e1-4157-4ef1-b93d-2374c7e36c8f` | 18 | 67.25 |
| 10 | 3D pit wall, standard | `b453c0ff-504e-45e6-be67-e899bd25a1c4` | 12 | 79.25 |

**79.25 credits** of the 80.5 planned and the 100 cap; no retries (each first result was
usable; the car's closed right front arch and the pit wall's mushy fence were fixed in the kit
instead of regenerating).

## Machine checks

| Check | Result |
|---|---|
| Kit materials are URP Lit with a base map and a normal map (`TrackKit.CheckMaterials`) | 87 renderers, 33 materials, **0 failing**; the sky dome's gradient shader is exempt like a skybox |
| No Flat shader or MeshKit mesh on a visible world object (the walk's `Checks`, every renderer in the scene except particles, trails, ghost lines and the sensor beams, which are effects) | **0 Flat, 0 MeshKit** on the title (645 renderers), the briefing, the pit lane (547), the training run, the race (645) and the debrief. The Flat and MeshKit code stays for the effects (the sensor beams, the trails, the ghost lines) and as the fallback without the kit |
| The car fills at least 25 % of the frame on the main play screen (projected bounding box, `DemoDriver.Coverage`) | **pit lane 30 %** (43 % x 69 %, level 2.3) and **33 %** (48 % x 70 %, level 1.1); **training run 31 %** (44 % x 71 %, the default chase); **race 31 %** (44 % x 71 %). Claude Bomb's device: 40 % |
| No light-theme panel on a play screen (any image brighter than 75 % luminance, more than half opaque, larger than 60,000 px² at 1080p) | **0** on the title, briefing, pit lane, training run, race and debrief |
| Text the player reads at least 16 px at 1080p (every visible TextMesh Pro label) | **0 under 16 px** on all six screens (the smallest is 16) |
| Web build under 45 MB transferred, loads and runs on the platform | **28.5 MB**, 5.0 s, runs (`Builds/look-audit/realism/platform-robot-racing.png`) |

## Web, before and after

The platform server on port 8085, headless Chrome with software GL, cache off (the realism
pass's `loadtime.mjs` on port 9336).

| | Before (text pass, 6 November) | After (realism pass) |
|---|---|---|
| Transferred | 20.5 MB | **28.5 MB** (budget 45) |
| Load to the loader hiding | 3.3 s | **5.0 s** (median of 4.8, 5.3, 5.0) |
| data / wasm on disk | 10.8 / 9.3 MB | 18.8 / 9.3 MB |

It loads and runs on the platform: `Builds/look-audit/realism/platform-robot-racing.png` (the
title, as the browser draws it).

## The side-by-sides

`games/robot-racing/Builds/look-audit/realism/final/`: Claude Bomb's frame on the left, Robot
Racing's on the right, 1600 x 900 each (3216 x 900 with a 16 px gap). Robot Racing's frames
come from the Mac build's walk (`walk12`, also kept as `game-*.png` in the same folder; the earlier walks `walk1` to `walk11` show the steps, the kit's four-sided renders are in `realism/models`).

| Screen | File | Claude Bomb's frame |
|---|---|---|
| Title | `side-by-side-1-title.png` | `0-title.png` (its title, the CAM 02 frame) |
| Briefing | `side-by-side-2-briefing.png` | `2-after-briefing.png` (the transmission beside the device) |
| Pit lane / setup (main play screen) | `side-by-side-3-pit-lane.png` (2.3, the points table open) | `4-play.png` (its play screen) |
| Pit lane, nothing to set | `side-by-side-3b-pit-lane-1.1.png` (1.1) | `6-play-clicked.png` |
| Training run | `side-by-side-4-training-run.png` | `5-play-later.png` |
| Race | `side-by-side-5-race.png` (2.6 on the grand circuit) | `6-play-clicked.png` |
| Debrief | `side-by-side-6-debrief.png` | `1-briefing.png` (its dark panel beside the device) |

They have not been judged here (no self-grading, no reviewer spawned).

## What I could not do, and what is still open

- **Trees.** Poly Haven's trees are 0.3 to 17 million triangles and its fir twig maps are a
  baked atlas, not sprites; the trees are made in the kit from bark lathes and cards of an
  ambientCG leaf. They read as trees in the fog behind the barriers; up close they would look
  like cards. No tree was on the approved Higgsfield list.
- **The car's right front arch** came out of the image-to-3D closed (its wheel would sit hidden
  inside). The shell is mirrored so the car's right has both arches open, and the pit lane's
  camera always stands on the car's right; the closed side shows from the chase camera's far
  side and on the title's and the debrief's swing.
- **The pit wall's fence** came out as a mush of texture; it is clipped off and replaced by a
  real fence (steel posts, rail, chain-link). The wall's own texture is soft up close (the
  briefing's camera passes it).
- **The drawers are still lists of controls.** They are dark, a third of the screen and fold
  away, with 16 px text, but the points table and the rule book are rows of chips and text; the
  Tech Escape reviewer called the same kind of drawer "a form".
- **Moments over the world.** At 1x the race shows a banner per lap over the car (the game's
  own moments, unchanged); the walk waits for a gap between them for its race frame.
- **Load time** grew with the assets (see the table); software GL in headless Chrome is the
  slow case, not a player's machine.
- **The art bible** (`Docs/art-bible.md`) still describes the toy diorama of the look pass; the
  realism pass's look (evening practice, PBR) is described here and in the code's comments.
- **Not checked by eye:** the old-blocks fallback without the kit (the code path is kept and
  compiles; it was not run); the tracks the walk does not visit (it shows "One bend" on 1.1,
  "The hairpin" in 2.3's pit lane, "The grand circuit" on 2.6's race and the title); the audible
  build. `Builder.BuildMac` and `Builder.BuildWeb` both build (the last Mac walk is `walk12`).
- `Docs/look-audit.md` was not touched (another session owns it); a Phase 6c section for Robot
  Racing can be made from this report.
