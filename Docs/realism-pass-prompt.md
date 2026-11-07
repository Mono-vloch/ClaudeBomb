# Realism pass: bring AI Arena, Robot Racing and Tech Escape up to Claude Bomb

## Read first

- `Docs/look-audit.md` (phases 1 to 5): everything already done to these three games.
- `games/claude-bomb/`: the reference, built by a human developer. Open its `Assets/Scenes/Main.unity`, `Assets/Environment/`, `Assets/Models/`, `Assets/Textures/`, `Assets/Settings/` and its Volume profile before you write any code.

## The problem, in one sentence

The three games are made of boxes drawn in code and painted one flat colour each, so no amount of lighting, post-processing, camera work or UI polish will make them look like Claude Bomb, which is made of real modelled and textured assets. The previous passes improved lighting, post, cameras, VFX, sound and text, and they scored themselves 3.5 to 4.0 against Claude Bomb's 3.8. By eye that score is wrong: side by side, the three still look like a prototype next to a game. Do not run another lighting, post or UI-only pass. The gap is the assets, and the screen layout.

## Evidence (measured on 6 November 2026)

| | Claude Bomb | AI Arena / Robot Racing / Tech Escape |
|---|---|---|
| Model files (fbx, glb) | 26 (Poly Haven props, LUMA_25k) | 0 |
| Texture images (albedo, normal, roughness, AO) | 94 jpg + baked maps | 0 (the png files are UI sprites) |
| Material assets | 73 | 2 |
| Main scene file | 482 KB, hand dressed | 7 KB, empty; the whole world is generated at runtime by `MeshKit.cs` |
| World shader | URP materials with texture maps | one custom `Maaloomatiia/Flat` shader: a single colour, no texture, no normal map, no roughness |
| Hero object | a detailed device filling half the frame: modules, wires, LEDs, screws, labels, bevelled metal | a box with a dome (car), a box with a box head (agent), a capsule (attacker), shown small |
| Camera | close, low, one hero object, framed as a security camera feed (REC CAM 02 / BAY 3) | far and high above a board of small identical pieces |
| UI during play | a dark brand panel beside a world that fills most of the frame | a white, light-theme web-app panel over half the screen, small grey text |
| Fiction | Meridian Labs, night shift, Bay 3, hazard signs, props that belong there | a diagram of the data model: a grid of stalls, a track of square blocks, a row of cut-open rooms |

What a non-specialist sees as "cheap" or "basic", in order of how much it costs the look:

1. **No surface detail.** Every surface is one colour with sharp, unbevelled edges. Real objects have texture (wear, grain, dirt, scratches, print), bevelled edges that catch a highlight, and small parts (bolts, seams, cables, labels). That is most of the "real 3D" feeling.
2. **Primitive shapes.** Trees are a ball on a stick, barriers are cubes, stalls are boxes. The brain reads them as placeholders.
3. **Scale and distance.** Claude Bomb shows one rich object up close. The three show many simple objects from far away, so there is almost no detail per pixel.
4. **The white panel.** During play, half the screen is a light-theme dashboard. That reads as a web app or an LMS, not a game.
5. **Characters with no silhouette and no animation.** The player's robot, the agent and the attacker are the things the eye goes to, and they are the simplest objects on screen.
6. **No place.** The worlds are abstract boards. Claude Bomb is a room you could stand in.

## Goal

Each of the three games, at any moment of play, should be indistinguishable in production quality from Claude Bomb when shown side by side at the same size. Same family: semi-realistic, stylised PBR, dark brand UI, a real place with a hero object up close.

## Hard rules

- Do not change what the games teach: LUMA's lines and voice files, the curriculum text, the exact words the agents read, the level logic, the scoring and the platform reports stay as they are. This pass changes what the world is made of, how it is framed and how the UI sits on it.
- Keep the kit, the platform wiring and the build scripts working. Each game must still build to `site/games/<id>/` and run on the platform.
- Assets: CC0 only (Poly Haven, ambientCG, Kenney, Quaternius are all CC0). Record every asset with its source and licence in `games/<id>/Docs/credits.md`. Reuse Claude Bomb's own `Assets/Environment/PolyHaven` props and materials wherever they fit; they are already in the house style.
- Web size: the 25 MB budget is lifted to 45 MB transferred per game (Claude Bomb is about 60 MB). Use texture compression (ASTC/DXT via the WebGL build settings), 1K textures for props and 2K only for hero objects and the ground, and shared texture sets across props.
- Prefer prefabs and authored scenes over meshes built in code. Code may still place prefabs from level data (track layouts, stall positions, room order); what it places must be real models.
- Keep the Editor muted during automated sessions (see the engine-loop finding in the audit).

## What to build, per game

### Tech Escape (do this one first, as the pilot)

It is the closest to Claude Bomb (a facility at night), so it proves the pipeline fastest.
- The rooms become real office and facility interiors: textured concrete and vinyl floors, painted walls with skirting and trim, ceiling panels with light fittings, real desks, office chairs, monitors whose screens are emissive and show content, server racks with blinking LED strips and cable runs, filing cabinets, doors with frames, handles, windows and signage. Reuse Claude Bomb's door, extinguisher, hazard barrel, wet floor sign, boxes, clipboard, posters and hazard tape.
- Each room's misconfiguration prop (the mail sorter, the safe, the badge gate, the tape shelves and so on) becomes a real model, close enough to read.
- Camera: the main room view goes inside the room at shoulder height, framed like Claude Bomb's CAM 02 shot (camera-feed corners, room label, clock). Keep the cut-open overview only for the attack map, and make that overview a real building section (wall thickness, floor texture, ceiling lights seen from above).
- The attacker becomes a rigged humanoid (hooded figure) with idle, walk and a "force the door" animation, plus the scarlet accents already in the art bible.

### Robot Racing

- The circuit becomes a real small track: textured asphalt with painted lines, red and white kerbs, tyre walls and barriers with hazard stripes, a start gantry with lights, a pit wall, a small grandstand, cones, real trees and grass with variation. Kenney's Racing Kit and Poly Haven textures are a good start.
- The robot car becomes a hero model: bevelled body panels, real wheels with rims and tyres, a visible sensor pod (the things it senses), emissive eyes and antenna, a number decal. Wheels turn and steer; the body leans in bends.
- Camera: lower and closer by default; the overview is the exception, not the rule.

### AI Arena

- The market becomes a real market square: cobblestone or tiled PBR ground, wooden stalls with cloth awnings that have texture and sag, crates, baskets, real fruit and vegetable models on the stalls, hanging lamps, a fountain or well as a landmark, the houses as real small buildings.
- The agent becomes a hero robot model with idle, walk, look, pick up and "sent home" animations, in the team colours.
- Camera: follow the agent at street level as the main view; the board overview only when the player asks for it.

## Hero models from Higgsfield

The free libraries cover the environment (tracks, stalls, desks, walls, trees, props). The few hero objects that no library has in our style are made with Higgsfield (image to 3D, with PBR textures and, for characters, rigging and animation clips) and dropped as GLB files into `games/<id>/Assets/Models/Hero/`:

| Game | Hero models |
|---|---|
| Robot Racing | the robot car |
| AI Arena | the shopping agent robot |
| Tech Escape | the hooded attacker; the misconfiguration props that no library has (for example the mail sorter, the safe, the badge gate) |

If Higgsfield is connected in this session, generate them yourself: a design image first with `generate_image` (3/4 view, plain background, matching `Docs/art-bible.md` and Claude Bomb's semi-realistic stylised look), then `generate_3d` with PBR on, and `3d_rigging` with idle and walk clips for the attacker and the agent. Preflight the credit cost with `get_cost` and show it to me before you generate. If Higgsfield is not connected, use the GLB files already in `Assets/Models/Hero/` and ask me for any that are missing; do not fall back to boxes.

AI-made meshes need cleaning up before they go in the game. Do all of this for each one:
- Decimate to a web budget: about 15k triangles for a hero, about 5k for a prop. Keep the silhouette.
- Re-bake or downsize the textures to 2K for heroes and 1K for props, and set up a URP material with base map, normal map, mask map and emission (the eyes, the antenna, the LEDs).
- Fix the scale (1 unit = 1 metre), the pivot (at the base, centred) and the forward axis (+Z).
- Remove lighting or shadows baked into the albedo if they are visible.
- Parts that must move are separate objects. The car's four wheels must spin and the front two steer. If the mesh comes in one piece, cut the wheels out in Blender (scripted) or replace them with library wheels.
- Characters use an Animator with idle, walk and the game's reaction clips (the agent: look, pick up, sent home; the attacker: force the door).
- Make a prefab per hero, and put a side-by-side of the hero next to Claude Bomb's device, at the same size, in the realism folder for review.

Record each Higgsfield model in `credits.md` with its job id and the source image.

## UI during play (all three)

- Remove the white, light-theme panels from play screens. Every panel uses the dark brand glass already in use for the HUD.
- The world gets at least 65 percent of the screen at all times. Sheets and forms (the Tech Escape room form, the Robot Racing rule book, the AI Arena agent column) collapse to a side drawer the player opens, and are never wider than 35 percent.
- Minimum text size 16 px at 1080p for anything the player must read. No grey-on-white small print.
- Borrow Claude Bomb's diegetic framing where it fits (camera corners, location label, clock, REC dot).

## How to work

1. **Study Claude Bomb first.** Write `Docs/claude-bomb-recipe.md`: its render pipeline asset settings, Volume overrides and values, light setup (types, colours, intensities, shadows, reflection probe), camera FOV and distance to the hero object, material setup (which maps, which shader), texture sizes, and how big the hero object is on screen. Measure, do not guess. This recipe is the target for all three games.
2. **Pilot Tech Escape only.** Switch the world materials from `Flat` to URP Lit (or a lean custom PBR shader that supports base map, normal map, mask map and emission) and strip unused variants. Bring in the assets, build the prefabs, rebuild one room, the corridor and the attacker. Show me that before doing the rest of the game.
3. **Stop for approval** after the pilot room, with the comparison images below. Then finish Tech Escape, then Robot Racing, then AI Arena, stopping for approval after each game.

## How we judge it (no self-grading)

- For every key screen, save a side-by-side image: Claude Bomb's matching frame on the left, the game on the right, same resolution, in `games/<id>/Builds/look-audit/realism/`.
- The judgement is made by a separate agent that has not seen the work being produced. Give it only the side-by-side images and this question: "Do these two look like they come from the same studio, at the same production quality? If not, list what on the right still looks cheaper, most visible first." Fix what it lists and ask again. A screen passes only when it answers yes.
- Machine checks that must pass: no visible world object uses the `Flat` shader or a mesh from `MeshKit` (UI and VFX excepted); every world material has at least a base map and a normal map; the hero object (car, agent, attacker or the room's prop) fills at least 25 percent of the frame on the main play screen; no light-theme panel on any play screen; build under 45 MB transferred and the game loads and runs on the platform.
- Append a "Phase 6: realism pass" section to `Docs/look-audit.md` with what changed, the asset list, the before and after sizes and load times, and the side-by-side images.
