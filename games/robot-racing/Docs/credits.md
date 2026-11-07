# Robot Racing: asset credits

Every model and texture in the world, with its source and licence. Added in the realism
pass (7 November 2026, branch `realism-pass`); see `Docs/claude-bomb-recipe.md` at the root
for the look they serve and `Docs/realism-report.md` here for what was built with them.

## Poly Haven (CC0)

From https://polyhaven.com, released under CC0 (public domain, free for any use, no
attribution required). Downloaded at 1k (FBX and JPG maps) into
`Assets/Environment/PolyHaven/<id>/`; the folder name is the asset's id, e.g.
https://polyhaven.com/a/old_tyre. The ones marked "from Claude Bomb" were copied from
`games/claude-bomb/Assets/Environment/PolyHaven` (same files, same licence); "from Tech
Escape" from `games/tech-escape/Assets/Environment/PolyHaven`.

| Asset | Kind | Used for |
|---|---|---|
| `asphalt_track` | texture | the circuit's asphalt (one mesh, UVs in metres); its grain under the painted lines, the chequer and the tyre belt |
| `asphalt_02` | texture | the pit lane |
| `sparse_grass` | texture | the grass (with a generated large-scale map of greener and drier patches as URP Lit's detail albedo) |
| `concrete` | texture | the jersey barriers' and the kerbs' surface (normal map, and the detail under their paint), the floodlights' footings |
| `pine_bark` | texture | the trees' trunks and branches |
| `blue_metal_plate` (from Tech Escape) | texture | painted steel: floodlight poles, flag poles, the pit wall's fence posts and rail |
| `modular_chainlink_fence` (wire maps only) | texture | the chain-link panels of the pit wall's fence |
| `grass_medium_01` (colour, alpha and normal maps) | texture | the grass clumps: its atlas's clumps on crossed cards |
| `old_tyre` | model | the tyre walls, the corner stacks, the pit box's spare tyres |
| `tire_pump` | model | the pit box |
| `metal_tool_chest` (from Claude Bomb) | model | the pit box |
| `industrial_storage_cart` (from Claude Bomb) | model | the pit box |
| `korean_fire_extinguisher_01` (from Claude Bomb) | model | the pit box |
| `Barrel_01` (from Claude Bomb) | model | the pit box |

## ambientCG (CC0)

From https://ambientcg.com, released under CC0 1.0.

| Asset | Used for |
|---|---|
| `Leaf001` (1K JPG: colour, opacity, normal) | the trees' leaves: 520 copies of the leaf, scaled, turned and tinted, composited into one leaf-cluster card (`Environment/Generated/leaf_cluster_base.png` and `_normal.png`) by the realism pass's `leafatlas.py` |

## Made for this game from the CC0 sources above

In `Assets/Environment/Generated/`, painted by the realism pass's `gentex.py` with the
grain and stains of Poly Haven's `concrete` and `asphalt_track`:

| Texture | What it is |
|---|---|
| `paint_base.png` | worn white line paint (edge lines, start line, grid slot) |
| `kerb_base.png` | the kerbs' red and white stripes, rubber marks at the road edge |
| `barrier_base.png` | the jersey barrier: concrete with a yellow and black hazard band near the top of each face |
| `chequer_base.png` | the finish line and its flags |
| `number_base.png` | the car's number roundel, "07" in Inter Bold (SIL Open Font License, already in `Resources/Fonts`) |
| `belt_base.png` | the tyre walls' red and white belt |
| `cone_base.png` | the cones' orange plastic and white band |
| `ground_variation.png` | the grass's large-scale patches (made in `TrackKit.Prefabs.cs`) |

The meshes of the barrier (a jersey profile extruded 2 m), the kerb, the cone, the trees'
trunks and crowns, the grass cards, the floodlight mast, the flag poles, the fence and the
sky dome are made in `Assets/Editor/TrackKit.Prefabs.cs` with UVs in metres.

## Higgsfield (made for this game)

Generated in the user's Higgsfield account on 7 November 2026: a design image with
GPT Image 2.6 (0.25 credits each), then image to 3D with Tripo H3.1 and PBR maps (18
credits detailed, 12 standard). The GLB files were split by `glb_maps.py` (the realism
pass's tool) into geometry-only GLBs and URP maps (`_base.jpg`, `_normal.png`, `_mask.png`
with metal in R and smoothness in A, `_emission.png` where something glows), downsized to
2K (the car) or 1K, in `Assets/Models/Hero/<Name>/`. Scale, pivot and facing (the models
came out facing -X; a +90 degree yaw turns them to +Z) are fixed in
`Assets/Editor/TrackKit.Prefabs.cs`. The design images are kept outside the project.

| Model | Design image job | 3D job (model, quality) | Cost | Triangles | Maps | Notes |
|---|---|---|---|---|---|---|
| Robot car body, empty wheel arches | `13bde82d-2e37-44a9-b287-e731a323af46` | `9d2ab66c-38c7-48d9-adca-977b945bac64` (Tripo H3.1, PBR, detailed) | 0.25 + 18 | 9,074 | 2K base, 1K normal, emission (the ice-blue eyes and the amber antenna lamp from the base map's bright pixels); one smoothness (metal mean 0.038) | 1.55 m nose to tail. The image-to-3D closed the right front arch, so the shell is mirrored to keep the side with both arches open toward the pit lane's camera |
| Wheel, rim and tyre | `ebd56b1c-2bef-4010-a57b-3d45d3e6aff4` | `8040d163-411a-47c0-a67c-0144d5d43a86` (Tripo H3.1, PBR, standard) | 0.25 + 12 | 3,813 | 1K base, 512 normal and metal-smoothness | 0.26 m across, instanced four times as separate objects: they spin, the front pair steer |
| Start gantry with its lights | `8466ab12-21a5-4334-84bc-c8cdd4e2d212` | `1455211c-6b9e-4022-8475-73c4d5c21458` (Tripo H3.1, PBR, detailed) | 0.25 + 18 | 7,385 | 1K base, 512 normal and metal-smoothness, emission (the red start lights) | 4.4 m wide, over the back of the grid |
| Covered grandstand | `293a3d63-0e89-47fa-a35f-67b36ae2ec6e` | `2eda57e1-4157-4ef1-b93d-2374c7e36c8f` (Tripo H3.1, PBR, detailed) | 0.25 + 18 | 8,817 | 1K base, 512 normal and metal-smoothness | 9 m long, behind the pit lane |
| Pit wall section | `2330d956-dfa6-41de-b4b1-a4647f12693a` | `b453c0ff-504e-45e6-be67-e899bd25a1c4` (Tripo H3.1, PBR, standard) | 0.25 + 12 | 161 kept of 3,649 | 1K base, 512 normal; one smoothness | 4 m long. Its fence came out as a mush of texture: everything above the wall is clipped off and replaced by steel posts, a rail and chain-link panels |

Total: 79.25 credits (5 design images at 0.25, three detailed 3D at 18, two standard at 12),
no retries; the cap was 100.
