# Claude Bomb: the recipe

The target for the realism pass on AI Arena, Robot Racing and Tech Escape. Every number
here was read from Claude Bomb's project files (`games/claude-bomb/Assets`) or measured on
its web build on 7 November 2026; nothing is estimated unless it says so.

Sources: `Assets/Settings/*_RPAsset.asset`, `*_Renderer.asset`, `SampleSceneProfile.asset`
(the scene's Volume), `Assets/Scenes/Main.unity` (camera, directional light, render
settings), `Assets/Editor/LabBuilder.cs` (the room, its materials, props and lights),
`Assets/Scripts/LabAtmosphere.cs` (fog, beacon, flicker, probe), `Assets/Scripts/Opening.cs`
(the camera-feed frame), the GLB files in `Assets/Assets/`, and frames captured from the web
build in headless Chrome at 1600 by 900 (`games/claude-bomb/Builds/look-audit/realism/`:
`0-title.png`, `4-play.png`).

## 1. Render pipeline

URP 17.3.0, linear colour space. Two pipeline assets; **the web build uses the Mobile one**
(`m_PerPlatformDefaultQuality: WebGL: 0`, quality level 0 is "Mobile"), the desktop uses PC.

| Setting | Mobile (web) | PC (desktop) |
|---|---|---|
| Renderer | Forward, no renderer features, native render pass on | Forward+ |
| HDR | on (default 32-bit precision) | on |
| MSAA | off | off |
| Render scale | **0.8** | 1.0 |
| Main light | per pixel, shadows on, **1024** map | per pixel, shadows on, 2048 map |
| Soft shadows | **off** | on, quality High |
| Additional lights | per pixel, **4 per object** | per pixel, 4 per object |
| Additional light shadows | **off** | on, 2048 atlas |
| Shadow distance | **3 m**, 1 cascade | 3 m, 1 cascade |
| Shadow bias (depth / normal) | 1 / 1 | 0.1 / 0.5 |
| Colour grading | LDR, LUT 32 | LDR, LUT 32 |
| Light layers | on | on |
| Reflection probes | blending on, box projection on | same |
| SRP batcher / dynamic batching | on / off | on / off |
| Fast sRGB conversion | on | off |
| Depth / opaque texture | off / off | off / off |

What this means: on the web Claude Bomb has hard shadows from one light over 3 m, renders at
80 % resolution, and gets its look from textures, emissive accents and post, not from
expensive lighting. The three games use one 2048 soft-shadow asset over 60 m today; that is
more costly than the reference and buys less.

## 2. Volume (post-processing)

One Global Volume, weight 1, profile `SampleSceneProfile`. Only these overrides are on:

| Override | Values |
|---|---|
| Tonemapping | **Neutral** (not ACES) |
| Bloom | threshold **1.0**, intensity **0.25**, scatter **0.5**, high-quality filtering on, no dirt, no tint |
| Vignette | intensity **0.2**, colour black, smoothness 0.2 (default), not rounded |
| Motion blur | present but inactive |

No colour adjustments, no white balance, no depth of field, no film grain, no lift/gamma/gain.
The camera has post-processing on, no anti-aliasing (`m_Antialiasing: 0`), HDR on. The
three games use ACES, bloom 0.35 at threshold 1.1, saturation +8, contrast +5 and SMAA:
stronger grading than the reference.

## 3. Lights

The device and the room are lit separately with **rendering layers**: the device is on layer
1 and lit by the directional light; the room is on layer 2 ("Light Layer 1") and lit only by
its own lamps. The glass table frame and the table's glow touch both.

| Light | Type | Colour | Intensity | Range / angle | Shadows | Layer |
|---|---|---|---|---|---|---|
| Directional Light | directional, rotation (50, -30, 0) | white at **5000 K** | **2.0** | | soft, strength 1, bias 0.05, normal 0.4 | device |
| LampLight (over the table) | spot, pointing down, 1.42 m under the ceiling | warm (1.00, 0.90, 0.76) | **12** | 6 m, 72° outer / 30° inner | soft 0.85 (desktop only: no additional shadows on the web) | room |
| TableGlow (under the glass) | point | brand teal (0.14, 0.88, 0.75) | 0.45 | 1.3 m | none | both |
| TubeLight × 3 (fluorescent fittings) | point, 0.35 m under the ceiling | cool (0.78, 0.87, 1.00) | 1.0 | 5 m | none | room |
| BeaconLight × 2 (back to back) | spot, turning 110°/s (300°/s in the last minute) | amber (1.00, 0.42, 0.08), red near the end | 7 | 9 m, 44° / 10° | none | room |
| ExitLight | point | green (0.10, 0.85, 0.40) | 0.9 | 2 m | none | room |

All lights are `ForcePixel`. Ambient: **Skybox** mode with the default procedural skybox,
intensity 1 (the camera never shows the sky: it clears to the fog colour). Reflections: a
**realtime reflection probe**, 128 px, box 8.4 × 3.8 × 8.4 m (the room), intensity **1.6**,
rendered once by script after the room's lights are on, so the metal reflects the lit room.

Fog (set at runtime by `LabAtmosphere`): **exponential squared, density 0.085, colour
#091021** (0.035, 0.06, 0.13); the camera's background is the same colour. At 1 m the fog
is invisible, at 5 m the room is 17 % fogged, at 8 m 37 %: the walls fall into blue-black.

Life in the light: one fluorescent tube stutters in bursts of 3 to 8 flickers every 3 to 9 s;
on a strike all of them stutter; the beacon turns; a fire-alarm LED blinks; a faint additive
light cone (alpha 0.06 at the lamp, 0 at the floor) and 160 warm dust motes (5 to 14 mm,
16 per second, 10 s life) hang in the lamp's beam; a mains hum loops under everything.

## 4. Camera

| | Value |
|---|---|
| Projection | perspective, **60° vertical** FOV, near 0.3, far 1000 |
| Start pose | (0.58, 0.16, -0.82), pitch **8.6° down**, yaw -20.6° |
| Distance to the device's centre | **1.02 m** at the start, clamped to **0.7 to 1.2 m** |
| Height | **0.16 m above the device's centre**, 1.21 m above the floor (the device's centre is at 1.05 m: a 0.76 m table plus half its height), so the camera looks almost level at it |
| Control | orbit around the device, scroll / W-S for distance, A-D pan up to 1 m, arrows step 45° / 30° |
| Framing | lens shift: the device is centred in the part of the screen left of the chat panel (play) or in the left 60 % (title) |
| Moves | scripted glides for the tour, a slow orbit on the title, a small shake on a strike |

## 5. The hero object on screen

The device is **1.00 × 0.58 × 0.33 m** (pivot at its centre), on a glass table 0.76 m high.

| Screen | Device bounding box | Share of the 1600 × 900 frame |
|---|---|---|
| Play (`4-play.png`, camera at its start pose) | 805 × 720 px, x 270 to 1075, y 110 to 830 | **50 % of the width, 80 % of the height, 40 % of the frame** |
| Projected from the scene's start camera at 16:9 (8 corners) | | 54 % × 74 %, 40 % of the frame (agrees) |
| Title (`0-title.png`, orbit, framed in the left 60 %) | 535 × 440 px | 33 % × 49 %, 16 % of the frame, 28 % of the camera view |

So the hero fills **40 % of the frame on the main play screen** and stays above 25 % of the
camera view even on the title's wide orbit. The rest of the frame is one table, one wall, a
desk at the edge and a few props falling off into fog: **one rich object, close, and a room
that recedes**, not a board of objects.

## 6. Models

| Group | Files | Triangles | Textures |
|---|---|---|---|
| The device (`Assets/Assets/*.glb`) | 10 GLB, one mesh each, normalised to 1 m on the longest side, scaled in the scene | 3,268 to 16,007 each; **Bomb.glb 14,018** | three **2048²** JPG per model: base colour, metallic-roughness, normal (30 files, 66 MB on disk) |
| The room's props (`Environment/PolyHaven/*`) | 15 Poly Haven models at **1k** (FBX) | Poly Haven's own | Poly Haven 1k sets: diffuse, normal (GL), ARM |
| LUMA | `LUMA.fbx` (+ `Models/LUMA_25k.asset`) | 25k | |

The ten device GLBs have the signature of image-to-3D output (one remeshed mesh per part,
about 15k triangles, three generated 2K maps named `image_0/1/2`), loaded with **glTFast
6.0.1** onto its PBR metallic-roughness shader graph. That is exactly the pipeline the
realism prompt proposes for our heroes.

The fifteen Poly Haven props: `hanging_industrial_lamp`, `mounted_fluorescent_lights` (× 3),
`metal_tool_chest`, `korean_fire_extinguisher_01`, `fire_alarm`, `metal_office_desk`,
`industrial_microscope`, `medical_box`, `clipboard`, `industrial_storage_cart`,
`metal_toolbox`, `cardboard_box_01` (× 3), `Barrel_01`, `old_military_crate`,
`WetFloorSign_01`. Everything else in the room is Unity primitives (quads and cubes) wearing
textured or plain Lit materials: the floor, walls, ceiling, skirting, pipes, door and frame,
push bar, exit sign, table, beacon, poster.

## 7. Materials

73 material assets. All world materials are **URP Lit** (or glTFast's PBR graph for the
device); none is a custom flat shader.

`LabBuilder` makes them from Poly Haven's maps:

- `_BaseMap` from `*_diff_1k.jpg` (sRGB); with an alpha/opacity map packed into its alpha
  for cut-outs (alpha clip 0.5) or glass (transparent).
- `_BumpMap` from `*_nor_gl_1k.jpg` (normal map type), `_NORMALMAP` on.
- Poly Haven's packed ARM (R = AO, G = roughness, B = metal) is **split** into URP's
  `_MetallicGlossMap` (R = metal, A = 1 - roughness, `_Smoothness` 1) and `_OcclusionMap`
  (R = AO). `_METALLICSPECGLOSSMAP` and `_OCCLUSIONMAP` on.
- `_EmissionMap` where the asset has one, at 2.5×; the tube glass emissive at 2.2×.
- Room surfaces are tinted through `_BaseColor`: floor `concrete_floor_worn_001` × (0.50,
  0.51, 0.54); walls `plastered_wall_04` × (0.40, 0.45, 0.54), a cool blue-grey.
- Plain Lit (no maps) only for small or dark parts: ceiling (#292B33, smoothness 0.1), trim
  (#141A24, metallic 0.3), pipes (metallic 0.85, smoothness 0.55), brushed steel (metallic
  0.9, smoothness 0.62), the door (#17263F navy, metallic 0.45), emissive lamps and signs.
- One generated texture: the hazard stripes (128 × 32, yellow #ECBA18 and #161618 at 45°).

## 8. Texture sizes

| Use | Source | Imported at |
|---|---|---|
| The device's maps | 2048² JPG | 2048, normal compression on WebGL |
| Floor and walls | Poly Haven 1k | **1024** |
| Props | Poly Haven 1k | **512** |
| Generated ARM splits | PNG from the 1k ARM | 512 (props), 1024 (surfaces) |

## 9. The room

Bay 3 is a closed box **8.4 × 8.4 m, 3.8 m high**, the device in the middle. Floor and ceiling
in 4 × 4 tiles of 2.1 m and each wall in 4 × 2 tiles, **each tile its own object**, so the
four-lights-per-object limit picks the nearest lamps for every tile. A dark skirting 12 cm
high runs round the walls; pipes run along the top of the back wall and down a corner;
hazard tape is laid on the floor in a 3.1 m square around the table. Signs: "BAY 3" stencilled
6.5 high in worn paint (35 % alpha) on the back wall, "HAZARD CONTAINMENT / AUTHORISED STAFF
ONLY" under it, a door with frame, push bar and dark window, a lit EXIT sign, the academy's
safety poster on the left wall. All colliders are stripped from the room (clicks go only to
the device).

## 10. The camera-feed frame (diegetic UI)

From `Opening.cs`, drawn over the left 60 % of the screen on the title and the briefing:

- inset 48 px from the edges of that area;
- four corner brackets, each two strokes 44 × 2 px, in the faint brand grey at 50 % alpha;
- top left: a **red REC dot** 14 px, blinking (0.7 s on in a 1.2 s cycle), then "REC   CAM 02
  /  BAY 3" in 13 px letter-spaced caps, white;
- top right: the clock "23:47:09" in the display font at 18 px, monospaced digits, counting up
  from 23:47:00;
- bottom left: "MERIDIAN LABS  /  NIGHT SHIFT", 12 px caps, faint.

During play the frame goes; the HUD (time, strikes, modules) sits top centre on a white
rounded card, the chat on a **white panel 392 px wide on the right (24.5 % of the width)**, and
the world takes the other **75.5 %**. So Claude Bomb's own play screen does have a light panel,
but it is narrow, it is a chat, and the world behind is two thirds device.

## 11. Size and load

| | Claude Bomb |
|---|---|
| Web build | 60.2 MB on disk (brotli), **58.9 MB transferred** |
| Load (headless Chrome, software GL) | 6.2 s |
| Where the bytes are | the 30 device maps at 2K (most of it), the Poly Haven sets, two Japanese and Inter font atlases, LUMA |

## 12. The recipe, as rules for the three games

1. **URP Lit (or glTFast's PBR graph) on every world material**, with at least a base map
   and a normal map; ARM split into metallic-smoothness and occlusion. No flat colour shader
   on anything in the world.
2. **Textured surfaces, tinted.** Poly Haven / ambientCG 1k sets: 1024 for floors and walls,
   512 for props; tint the base colour toward the scene's palette instead of repainting.
3. **One hero, close.** A detailed model (about 15k triangles, 2K base / metal-rough / normal)
   at about 1 m from a 60° camera, its bounding box **about 40 % of the frame**, the camera
   near the hero's own height, looking almost level.
4. **A real room that recedes.** A closed box of the right proportions (for a facility: about
   3 to 4 m high), dark skirting, pipes or trim on the walls, 10 to 20 real props scaled to
   the hero, signs that belong to the place, and **exponential-squared fog (about 0.085)** so
   everything past 5 m sinks into the night colour.
5. **Light like a place at night.** One warm key lamp over the hero (spot, intensity about
   12), cool fluorescent fill (points, about 1.0), one moving coloured light (beacon), small
   emissive accents (LEDs, exit sign, table edge). Light layers to keep the hero's light off
   the room. A realtime reflection probe the size of the room, rendered once after the lights
   are on, at 1.6.
6. **Post, lightly.** Neutral tonemapping, bloom 0.25 at threshold 1.0, vignette 0.2. Nothing
   else.
7. **Web-cheap pipeline.** Hard shadows from one light over a few metres, 1024 map, render
   scale 0.8, 4 lights per object, no additional-light shadows.
8. **Life.** Flicker, dust in the beam, a turning beacon, a blinking LED, a hum.
9. **Frame it as a camera feed** where it fits: corner brackets, REC dot, location label,
   running clock. Keep the world at **75 % or more** of the screen during play; any panel is
   one narrow column.
