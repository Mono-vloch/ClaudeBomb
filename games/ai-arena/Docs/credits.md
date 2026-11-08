# AI Arena: asset credits

Every model and texture in the world, with its source and licence. Added in the realism
pass (7 November 2026, branch `realism-pass`); see `Docs/claude-bomb-recipe.md` at the root
for the look they serve and `Docs/realism-report.md` in this folder for what was done.

## Poly Haven (CC0)

From https://polyhaven.com, released under CC0 (public domain, free for any use, no
attribution required). Downloaded at 1k (FBX and JPG maps) into
`Assets/Environment/PolyHaven/<id>/`; the folder name is the asset's id, e.g.
https://polyhaven.com/a/wicker_basket_02. None of Claude Bomb's Poly Haven props fit a
market square (they are a lab's), so none were copied.

| Asset | Kind | Used for |
|---|---|---|
| `cobblestone_floor_08` | texture | the market square's cobbles (tinted toward the art bible's warm limestone) |
| `cobblestone_floor_04` | texture | the street round the square |
| `large_sandstone_blocks_01` | texture | the sandstone bands between the cells, the kerb, the agents' home stones (tinted per team) |
| `wicker_basket_02` | model | the basket on the agent's back |
| `wooden_lantern_01` | model | the lantern under every stall's awning (lit at dusk) |
| `street_lamp_02` | model | eight lamps round the kerb (lit at dusk) |
| `planter_box_01` | model | planters along the house fronts |
| `potted_plant_04` | model | potted plants along the house fronts |
| `painted_wooden_bench` | model | benches along the house fronts |
| `wooden_crate_02` | model | stacked crates by the houses |
| `wooden_bucket_01` | model | a bucket by the fountain |
| `sweet_potato` | model | the potatoes on the stalls |
| `food_apple_01`, `lemon`, `food_pears_asian_01` | models | imported, not placed (Kenney's pieces are used for these: at about 7k triangles an apple, a heaped crate would be some 70k) |

## Kenney (CC0)

Food Kit 2.0 by Kenney (https://kenney.nl/assets/food-kit), CC0 (see
`Assets/Environment/Kenney/Food/LICENSE.txt`). Fourteen models in GLB with the kit's palette
texture (`colormap.png`), in `Assets/Environment/Kenney/Food/`. In the kit their normals are
smoothed across faces under 60 degrees and they wear a URP Lit material with the palette as
the base map and a flat normal map, so every world material has a base and a normal map.

| Model | Item |
|---|---|
| `apple`, `banana`, `orange`, `strawberry`, `grapes`, `lemon`, `cherries`, `pear` | the fruit |
| `carrot`, `broccoli`, `mushroom`, `tomato`, `corn` | the vegetables |
| `paprika` | the chili (laid on its side and drawn out) |

## Higgsfield (made for this game)

Generated in the user's Higgsfield account on 7 November 2026: a design image with
GPT Image 2.6 (0.25 credits each), then image to 3D with PBR maps. The GLB files were split
by `glb_maps.py` (the realism pass's tool) into geometry-only GLBs and URP maps
(`_base.jpg`, `_normal.png`, `_mask.png` with metal in R and smoothness in A,
`_emission.png` where something glows), at 2K (the agent) or 1K (the rest), in
`Assets/Models/Hero/<Name>/`. Scale, pivot and facing are fixed in
`Assets/Editor/MarketKit.Prefabs.cs` (`BuildHeroes`, `BuildAgent`).

| Model | Design image job | 3D job (model) | Cost | Triangles | Notes |
|---|---|---|---|---|---|
| Shopping agent robot | `b8b4d9bf-10af-4bf7-be3e-d24bed13391a` | `d107ea6d-0557-4853-a7e7-a70b6acb6326` (Meshy image to 3D, PBR, rigged at 1.2 m, clip 0 Idle) | 0.25 + 38 | 15,559 | its maps (2K base, normal, emission; the eyes glow) are the agent's; its coral panels became the team-colour mask (`agent_team.png`, URP Lit's detail mask), so the panels take each team's colour. Its own rig and idle clip are not used (see the idle below) |
| Agent, walk | (same model) | `022c7aff-8c94-4531-928b-23c33ddf0cd2` (Meshy rigging, clip 613 Casual_Walk_inplace) | 8 | 15,559 | each clip keeps its own rigged copy: the rigs do not share bone orientations; the rigging jobs return the robot at 1.7 m, the kit scales each copy to 1.2 m |
| Agent, look | (same model) | `a1d9b430-5d14-4e01-89cc-2453ba6e9fcc` (clip 338 Short_Breathe_and_Look_Around) | 8 | 15,559 | the LOOK of the loop, and, played slowly, the agent at rest |
| Agent, pick up | (same model) | `51b499fe-aa81-4008-a376-510e1734a2ca` (clip 278 Female_Stand_Pick_Fruit_Basket) | 8 | 15,559 | a buy |
| Agent, sent home | (same model) | `a1a97df8-2521-4e46-8277-5b1644b914c9` (clip 391 Head_Hold_in_Pain) | 8 | 15,559 | a buy that ruins dinner |
| Agent, idle (not used) | (same model) | `60c61125-9c96-43b1-93f5-fcf2197d87a6` (clip 0 Idle) | 8 | 15,559 | the one retry, and still unusable: Meshy's Idle clip swings the hips from 81 degrees one way to 12 the other (measured in both GLBs; the walk stays within 6), so the robot never faces where it walks. Dropped; the look clip, slowed, is the idle. `AgentRig` also measures each copy's facing on its first frames and turns it back if a clip is off by more than 20 degrees |
| Market stall, teal and white awning | `43bbb808-667e-4861-8ba8-d717db4749e4` | `5bc3fc1e-b066-4b7f-aa74-597ce3523d94` (Tripo H3.1, PBR, detailed) | 0.25 + 18 | 8,361 | 2.0 m wide, counter at 0.78 m |
| Market stall, terracotta awning, green cupboard | `26244372-9bb6-4f79-bfe8-5697244bd17e` | `07a70951-d0ab-4cb1-944e-769cdf73458e` (Tripo H3.1, PBR, detailed) | 0.25 + 18 | 8,284 | 2.0 m wide, counter at 0.87 m |
| Town house, limestone and teal shutters | `a5c628a8-46a8-4cbf-8130-86b14b2680dd` | `5b3db482-52c7-4beb-a1f1-b73e69aed549` (Tripo H3.1, PBR, detailed) | 0.25 + 18 | 8,870 | 7.2 m tall; the ring round the square alternates the two houses, every other pair mirrored |
| Half-timbered cottage | `fef875db-db20-4eec-ae46-9005fed78194` | `f30384e5-356d-488e-a471-7d33e346e2af` (Tripo H3.1, PBR, detailed) | 0.25 + 18 | 8,965 | 6.8 m tall |
| Stone fountain | `70ee41bd-4078-4afb-8910-5392895a1a49` | `5463d592-05b0-4cc8-8a60-1084a734a71d` (Tripo H3.1, PBR, detailed) | 0.25 + 18 | 9,187 | 3.4 m across, the landmark in the north-west corner |
| Wooden produce crate | `62fe3231-93d0-419e-b72a-e2c1a3a92d37` | `24760041-1012-4e81-ae56-ab5c0ba0c5fb` (Tripo H3.1, PBR, standard geometry) | 0.25 + 12 | 4,866 | 0.6 m; heaped with the item on every counter; tinted grey, two of them are what a stall becomes when the agent cannot see it |
| Mom, the family at the door (added 8 November 2026, the game-feel pass) | `f7c6567a-e4ec-4dbb-b8c4-58132fe2f23f` | `a2c8e5b3-390e-486a-9621-4c1b9e3c673e` (Meshy 7, textured, PBR, rigged at 1.65 m, one clip: Wave_One_Hand, 4.1 s) | 47.5 | 15,558 | made in the owner's account; split by `glb_maps.py` into `Assets/Models/Hero/Mom/` (1K base and normal maps; no metal in her, so one smoothness and no mask), built by **AI Arena > Build family** (`Editor/MarketKit.Family.cs`) into `Resources/Market/Family`: 1.65 m, pivot at her feet, facing +Z, one URP Lit material. She stands on the doormat at the door of the house behind the agent's home (`Market.FamilySpot`), at rest on the clip's first frame while the agent is out, and waves once when it comes home (`MomRig`, on the debriefs); a porch lamp over the door lights her at dusk |

**Higgsfield spend for this game: 181.75 credits** (planned 175.5, cap 220): seven design
images (1.75), the robot (38), five clips at 8 (40, one of them the idle's retry), five
props with detailed geometry at 18 (90: two stalls, two houses, the fountain) and the crate
with standard geometry (12). The job-by-job ledger is in `Docs/realism-report.md`.

**Added in the game-feel pass: 47.5 credits** for Mom (the 3D job above, as recorded by the
owner; her design image is listed by its job id). **In all: 229.25 credits.**

## Made in the project

The antenna's mast and bulb, the lantern's flame and the agent's notebook are Unity's
primitive meshes wearing URP Lit materials made from 4 × 4 swatch textures and a flat
normal map (`Assets/Environment/Generated`). The squares, bands, kerbs and slabs are boxes
with UVs in metres, made by the kit. The skies are Unity's procedural sky.
