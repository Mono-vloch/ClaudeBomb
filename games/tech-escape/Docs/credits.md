# Tech Escape: asset credits

Every model and texture in the world, with its source and licence. Added in the realism
pass (7 November 2026, branch `realism-pass`); see `Docs/claude-bomb-recipe.md` at the root
for the look they serve.

## Poly Haven (CC0)

From https://polyhaven.com, released under CC0 (public domain, free for any use, no
attribution required). Downloaded at 1k (FBX and JPG maps) into
`Assets/Environment/PolyHaven/<id>/`; the folder name is the asset's id, e.g.
https://polyhaven.com/a/security_camera_01. The ones marked "from Claude Bomb" were copied
from `games/claude-bomb/Assets/Environment/PolyHaven` (same files, same licence).

| Asset | Kind | Used for |
|---|---|---|
| `concrete_floor_worn_001` (from Claude Bomb) | texture | corridor floor, the ground outside |
| `plastered_wall_04` (from Claude Bomb) | texture | corridor and room walls |
| `old_linoleum_flooring_01` | texture | the post room's floor |
| `ceiling_interior` | texture | ceilings |
| `blue_metal_plate` | texture | door frames, skirting, rails, lamp housings, sign plates |
| `mounted_fluorescent_lights` (from Claude Bomb) | model | ceiling fittings |
| `metal_office_desk` (from Claude Bomb) | model | the post room's desk |
| `korean_fire_extinguisher_01` (from Claude Bomb) | model | corridor |
| `fire_alarm` (from Claude Bomb) | model | corridor, post room |
| `WetFloorSign_01` (from Claude Bomb) | model | corridor |
| `cardboard_box_01` (from Claude Bomb) | model | parcels in the post room, boxes in the corridor |
| `clipboard` (from Claude Bomb) | model | post room desk |
| `Barrel_01` (from Claude Bomb) | model | (imported, not placed yet) |
| `hanging_industrial_lamp` (from Claude Bomb) | model | the lamp over every room's prop |
| `metal_toolbox` (from Claude Bomb) | model | the lab rooms' shelves |
| `security_camera_01` | model | corridor ceilings, the post room's corner |
| `wall_clock` | model | corridor, post room |
| `metal_trash_can` | model | corridor, post room |
| `office_notepads` | model | post room desk |
| `stationery_supplies` | model | post room desk |
| `steel_frame_shelves_01` | model | post room parcel shelves |
| `power_box_01` | model | corridor walls |

## Higgsfield (made for this game)

Generated in the user's Higgsfield account on 7 November 2026: a design image with
GPT Image 2.6, then image to 3D with PBR maps. The GLB files were split by
`glb_maps.py` (the realism pass's tool) into geometry-only GLBs and URP maps
(`_base.jpg`, `_normal.png`, `_mask.png` with metal in R and smoothness in A,
`_emission.png` where something glows), downsized to 2K (heroes) or 1K (props), in
`Assets/Models/Hero/<Name>/`. Scale, pivot and facing are fixed in
`Assets/Editor/FacilityKit.Prefabs.cs` (`BuildHeroes`).

| Model | Design image job | 3D job (model) | Triangles | Maps | Notes |
|---|---|---|---|---|---|
| Hooded attacker, idle | `204d92f7-5728-4448-8d82-43259b102907` | `fd56bd7f-6915-4b3b-bd36-d6d6d98a08af` (Meshy image to 3D, PBR, rigged, clip Idle) | 15,599 | 2K | 1.75 m, 24 bones; the emission map is the scarlet of the visor, stripes and straps |
| Hooded attacker, walk | (same model) | `7c39c9e6-9802-4c97-9e28-c034f551e1bc` (Meshy rigging, clip 671 Sneaky_Walk_inplace) | 15,599 | shares the idle's | its own rig: the three rigs do not share bone orientations, so each clip keeps its copy |
| Hooded attacker, force the door | (same model) | `cba97054-9b12-4128-9265-25a12de7b732` (Meshy rigging, clip 259 Step_Forward_and_Push) | 15,599 | shares the idle's | |
| Mail sorter (Suspicious Inbox's prop) | `961de330-0df7-40f6-aa03-4e701c1aad76` | `02fce3b9-27b3-466f-8f58-3164b301f96c` (Tripo H3.1, PBR, detailed) | 12,756 | 2K | scaled to 1.22 m wide |
| Server rack | `9710b2ac-e969-4203-a97f-53fe61684f85` | `386b3dbc-c0e8-4329-8f20-e95d591b882a` (Tripo H3.1, PBR, detailed) | 7,008 | 1K | LED emission from the green and amber pixels |
| Office monitor | `91702fc1-fd70-4758-92e4-bfd017fcb348` | `46d84356-7c80-4aba-9dbf-620553bfc19e` (Tripo H3.1, PBR) | 3,150 | 1K | the screen's content is a separate emissive panel |
| Security door leaf | `c2bdbfb2-8673-46db-8c28-3d02a5fb3b75` | `1ee8b503-55a5-4405-9b78-1761dfb719df` (Tripo H3.1, PBR) | 2,593 after cleanup (3,020) | 1K | the door closer arm came out as a rod through the slab and is clipped |
| Office task chair | `89060cd7-e741-4c90-a1fe-fd3720b40f37` | `e0407e8f-d59f-4e4a-90c9-87c461cf88e6` (Tripo H3.1, PBR) | 5,534 | 1K | |

The rooms' props and the dressing every room shares (second approved list, 7 November 2026,
Tripo H3.1 with PBR and detailed texture; detailed geometry for the ten props):

| Model | Room | Design image job | 3D job | Triangles | Notes |
|---|---|---|---|---|---|
| Rolling board of risk cards | Risk Room | `c3650906-0d54-496e-8ba5-3b28da742d4a` | `5ae737f8-aea1-4951-bd49-26e5c15647a1` | 8,296 | 1.5 m wide |
| Floor safe with a teal keypad | Password Vault | `c507dba2-419b-466a-96b4-7636c842a108` | `d7106ef9-3aa0-4459-95a7-a3e0c57ea086` | 8,501 | keypad glows (emission from its teal pixels) |
| Glass isolation chamber, red sample | Malware Containment | `07fb65b0-2644-4a12-bed7-b284c02bfc05` | `1e828e49-f7ca-42a0-bf84-7ed07264893b` | 8,376 | the sample glows red |
| Badge turnstile | Identity Chamber | `26ba3223-4da6-4fc0-81f9-a36d78393a75` | `3c51e631-4e7e-48c6-b750-0ef6b126cb9e` | 8,368 | reader ring and lights glow |
| Workbench, three small servers | Patch Laboratory | `d58e9c23-a993-4986-84a6-328f9e8ae192` | `45964f61-c125-4403-8279-7afa0f2f9b8f` | 8,986 | the amber warning light glows |
| Wall network cabinet, tangled cables | Network Maze | `52eee852-e8b1-4060-99ec-eb2b40a1db9f` | `49293fb1-cbdd-47ef-87ea-a34f020e5d02` | 9,209 | link lights glow |
| Console of toggle switches | Firewall Control | `370cab45-f761-4d8e-8723-3d13650d9c1e` | `f3a9f0cd-6e0f-47ad-9a0b-8737ea50e1e4` | 8,065 | teal lamps and one red |
| Steel lockbox, padlock, key | Encryption Chamber | `2ce13874-1c8c-42d5-a408-2abb2c36eb5e` | `ec7f4d8b-1856-4e25-846d-a1ff49afa71d` | 8,489 | on a desk against the back wall |
| Pedestal with a glass cloud lamp | Cloud Chamber | `84df52ea-f523-4252-8c0d-72fea29ea723` | `33d12ea7-b0cb-4a80-9291-ac334931d7ff` | 9,204 | a cool point light inside the glass |
| Tape library cabinet | Backup Vault | `9c09eeab-b157-43b1-9440-6b2deb3bda2a` | `31a00198-6077-4568-9620-4b6824fdeb4d` | 7,920 | status screen glows |
| Steel four-drawer filing cabinet | office rooms | `924a3d23-b442-4079-bd99-ee6e04e92fff` | `e8516b67-7812-4389-96b0-6492a7f0c2a1` | 3,036 | standard geometry; normal and mask maps at 512 |
| Cork notice board | every room but the post room | `dcb21a40-18a8-479e-a393-f56c572f40f6` | `3c49daea-1141-4c25-aa08-8b76435da6d8` | 2,563 | standard geometry; normal and mask maps at 512 |

The Operations Center's wall of screens is six of the monitor above on a steel rail, each
showing a board of charts made in the project.

Credits spent on the second list: 207.75 of the 260 approved (15 design images 3.75, ten
detailed models 180, two standard 24).

Credits spent on the first list: 128.75 of the 185 approved (11 design images 2.75, Meshy 38, Tripo
72, two rigging clips 16), from the account's transaction list of 7 November 2026.

## Higgsfield, the game-feel pass (8 November 2026)

Made in the user's Higgsfield account for the game-feel pass's round 4 and handed over as
files. The patch panel went through the same cleanup as the heroes above:
`Tools/glb_maps.py` (a rewrite of the realism pass's tool, which was lost with its session's
scratch folder) split it into a geometry-only GLB and URP maps at 1K in
`Assets/Models/Hero/PatchPanel/`; `Tech Escape > Build patch panel` (in
`Assets/Editor/FacilityKit.Prefabs.cs`) fixes its scale, pivot and facing.

| Model | Design image job | 3D job | Triangles | Maps | Notes |
|---|---|---|---|---|---|
| Patch panel with cables (2.4's firewall rack) | `248f8cde-c338-4fa0-b73b-2cfa4414bdfe` | `7ba8b068-856e-4e0c-8990-f0885dec79c2` (Tripo H3.1, PBR, real-world size) | 8,998 | 1K base; normal and mask at 512 in the build | 1.1 m wide as a prefab; the rig mounts it under the five rule modules (0.74 m wide there, to fit above the floor). No emission map: the green LEDs are too dim in the base map to pick out (28 pixels), and the amber ones match the yellow cables |

| Images | Jobs | Used |
|---|---|---|
| Two printed sheets of six staff badges (Sami the contractor, gone in June; Dana; Omar; Lina; Nour; the intern) | sheet A `339dc1e0-bf86-4ce3-b179-1516ee2dbfab`, sheet B `653dbd79-8bd3-47fe-bf71-75af2533c409` | Both are spelled right ("HARBOR PERMITS OFFICE", every name and role). Sheet B is used: its Sami badge says LEFT JUNE as the game does, and its large names and coloured role bands read from the room camera. Each badge cropped into `Assets/Resources/Badges/<key>.jpg` (240 x 340) and put on a thin card on 2.1's turnstile accounts and 3.1's roles |

Credits: 18 for the patch panel's 3D job, 1 for the badge sheets (as given by the user).

## Made in the project

- The monitor's inbox screen, the post room's poster, a flat normal map, and the
  metal-smoothness / occlusion maps split from Poly Haven's ARM maps: made by
  `Assets/Editor/FacilityKit.cs` into `Assets/Environment/Generated/`.
- Fonts: Inter (SIL Open Font Licence, `Assets/Resources/Fonts/OFL.txt`).
