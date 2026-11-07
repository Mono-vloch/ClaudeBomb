# AI Arena: the realism pass

7 November 2026, branch `realism-pass`, from `Docs/realism-pass-prompt.md` (the AI Arena
section) and the target measured in `Docs/claude-bomb-recipe.md`. Tech Escape's facility kit
was the pattern. What the game teaches is unchanged: LUMA's lines and voice files, the
curriculum text, the exact words the agent reads, the level logic, the scoring and the
platform reports. This pass changed what the market is made of, how it is framed and how
the interface sits on it.

## What changed

1. **A market kit made in the Editor** (`Assets/Editor/MarketKit.cs`,
   `MarketKit.Prefabs.cs`, menu AI Arena > Build market kit), the way Tech Escape's
   `FacilityKit` is made: URP Lit materials from CC0 maps (Poly Haven ARM maps split into
   metallic-smoothness where a set has metal, one smoothness where it has none), boxes with
   UVs in metres for the paving, the Higgsfield models normalised (metres, pivot at the base
   centre, facing +Z, checked from four sides in `Builds/look-audit/realism/models`), crunched
   textures, and prefabs in `Assets/Resources/Market`:
   - **the square** (`Square`): cobbles (`cobblestone_floor_08`, tinted to the art bible's
     warm limestone) with sandstone bands laid between the arena's cells (the grid the
     lesson needs, in stone), a sandstone kerb, a cobbled street, a ring of 24 houses (the two
     Higgsfield houses alternating, every other pair mirrored, the corners left open as
     streets), the stone fountain as the landmark in the north-west corner, eight street
     lamps, planters, potted plants, benches, stacked crates and a bucket;
   - **two stalls** (`StallA`, `StallB`), each with its counter measured from the mesh and a
     lantern under the awning (lit at dusk);
   - **the goods of every item** (`Goods/<item>`): the Higgsfield crate heaped with the item
     in three layers, tipped toward the buyer, and a pile on the counter beside it, the pieces
     merged into one mesh per material (and compressed) so a stall of apples is two
     renderers, not twenty; and one piece per item (`Pieces/<item>`) for the throw into the
     basket;
   - **what the agent cannot see** (`Unknown`): two crates, grey, under the old question mark;
   - **the home stone** (`HomeStone`): a sandstone slab Market tints in the agent's colour;
   - **the agent** (`Agent`): the rigged robot, four rigged copies (walk, look, pick up, sent
     home) with one Animator and one-state controller each, sharing one material, a graphite
     antenna with a glowing bulb, a wicker basket on its back, a notebook at its side;
   - **the sky** (Unity's procedural sky, by day; the kit also makes a dusk sky, unused: the
     evening square has an ink haze and no sky).
2. **The agent** (`Assets/Scripts/World/AgentRig.cs`, `RobotModel.cs`): a friendly humanoid
   robot from Higgsfield (Meshy image to 3D, rigged at 1.2 m, 15,559 triangles, 2K maps, the
   eyes emissive). Its coral panels were turned into a mask (`agent_team.png`) and grey in
   the base map, so URP Lit's detail albedo paints them in each team's colour (the violet,
   coral, blue agents in the frames). It walks between cells with its walk clip, looks
   around on the LOOK of every turn, stops at a stall and reaches for the goods (the piece
   flies into its basket and stays there), and holds its head when what it bought ruins
   dinner before it is called home. The brain dial is the bulb's size, the effort its glow,
   the memory the glass cards stacked beside its head or the notebook at its side. Because each rigging job makes its
   own skeleton, every clip keeps its own copy (Tech Escape's `AttackerRig`), and AgentRig
   measures each copy's facing on its first frames (the hips against their bind pose) and
   turns it back when a clip is off. Meshy's Idle clip was dropped: it swings the hips from
   81 degrees one way to 12 the other (read from both GLBs), so the robot never faced where
   it walked; the look clip played slowly is the idle.
3. **To scale**: a cell is 2.4 m (`Market.Pitch`), the stalls 2 m wide, the agent 1.2 m, the
   houses 7 m. A real walk takes real time: a step lasts 1.4 s at the fastest speed, and the
   simulation waits for the screen before the next turn (`Simulation.Holding`, pacing only:
   no rule, turn or score changed).
4. **The camera**: the main view of a round is the **street**, behind and to the right of
   the chosen agent at its shoulder (1.1 m back, 1.05 m up, 52 degrees), composed by aiming
   so the agent stands left of the middle of the world's part of the screen and the market
   opens beyond it; the camera pulls in where a stall stands behind the agent. The **agent's
   eye** is over its head, what it cannot see is grey crates, and it is framed as
   Claude Bomb frames its camera feed (corner brackets, a blinking REC dot, "AGENT CAM /
   name", its sight). **The market** from above is the third button, only when asked for.
   The title shows the agent close among the lit stalls of the evening square, the way
   Claude Bomb's title shows its device (the other agents stroll behind it); the story keeps
   the slow turn round the square. The garage shows the agent from the front at its height; the briefing and the decide
   screen turn slowly round the square at roof height; the countdown flies in over the
   houses; ALL HOME turns round the agent at arm's length.
5. **Light and post, Claude Bomb's**: one warm key with soft shadows, a cool sky ambient,
   exponential-squared haze that thickens at street level (0.013) and thins from above;
   the dusk of the title, the story and the cover is an evening market: a cool moon key,
   ink haze, the lanterns and street lamps lit. Neutral tonemapping, bloom 0.25 at
   threshold 1, a black vignette at 0.2, no grading (`Atmosphere.cs`, was ACES with
   saturation and contrast).
6. **The interface on dark glass** (`Theme.cs`, `UiKit.cs`): the white page ramp became the
   brand's ink glass (panels at 93 %, the HUD at 86 %, sheets such as the file opaque,
   teal-tinted cards, teal and chalk hairlines, chalk text); no text under 16 px on the 1080 canvas (`Ui.Text` clamps it; the small caps
   labels are 16 with lighter tracking).
   - **The run** (`ArenaScreen`): the world fills the stage; the mission and the agents'
     cards are a dark drawer of 600 of 1920 units (31 %) on the right that folds to a tab;
     "What it knows" opens inside the drawer; LUMA's line sits at its foot; the HUD and the
     moments are dark glass over the world, the moments in its upper third so the agent
     stays in view.
   - **The garage** (`SetupScreen`): the list editor, the wallet and DEPLOY are a dark drawer
     of 640 units (33 %) that folds to a tab; the agent stands big in the rest with its dials
     on it; the errand's title and buttons are a glass bar over the world, the family's
     message a glass card.
   - **The decide errand** (`DecideScreen`, 1.6): was a white sheet over 96 % of the screen;
     now the five tasks are a drawer of 660 units (34 %) beside the square.
   - The briefing, the warm-up, the debrief, the file and the settings are dark glass too;
     the debrief's market is dimmed behind it.
7. **Fallback**: without the kit (`Resources/Market` missing) every one of these paths falls
   back to the boxes the game was made of before (a 1-unit cell, the code-built robot, the
   old camera and the overview as the main view).

The build scripts are unchanged: `Builder.BuildMac` (AI Arena > Build macOS app) and
`WebBuild.BuildFromCommandLine` (into `site/games/ai-arena`). `com.unity.cloud.gltfast`
6.0.1 was added to `Packages/manifest.json` to import the GLB files.

## Assets

Every asset with its source and licence, and every Higgsfield job, is in
`games/ai-arena/Docs/credits.md`. In short: Poly Haven (CC0) for the cobbles, the
sandstone, the street, the basket, the lanterns, the street lamps, the planters, the
benches, the crates, the bucket and the potatoes; Kenney's Food Kit (CC0) for the other
fourteen items; Higgsfield for the agent (and its clips), the two stalls, the two houses,
the fountain and the produce crate. None of Claude Bomb's Poly Haven props fit a market.

## Higgsfield credits (this game's ledger)

| # | Job | Id | Credits |
|---|---|---|---|
| 1 | design image, the robot | `b8b4d9bf-10af-4bf7-be3e-d24bed13391a` | 0.25 |
| 2 | design image, stall A | `43bbb808-667e-4861-8ba8-d717db4749e4` | 0.25 |
| 3 | design image, stall B | `26244372-9bb6-4f79-bfe8-5697244bd17e` | 0.25 |
| 4 | design image, house A | `a5c628a8-46a8-4cbf-8130-86b14b2680dd` | 0.25 |
| 5 | design image, house B | `fef875db-db20-4eec-ae46-9005fed78194` | 0.25 |
| 6 | the robot, Meshy image to 3D, rigged, idle | `d107ea6d-0557-4853-a7e7-a70b6acb6326` | 38 |
| 7 | stall A, Tripo detailed | `5bc3fc1e-b066-4b7f-aa74-597ce3523d94` | 18 |
| 8 | stall B, Tripo detailed | `07a70951-d0ab-4cb1-944e-769cdf73458e` | 18 |
| 9 | design image, fountain | `70ee41bd-4078-4afb-8910-5392895a1a49` | 0.25 |
| 10 | design image, crate | `62fe3231-93d0-419e-b72a-e2c1a3a92d37` | 0.25 |
| 11 | house A, Tripo detailed | `5b3db482-52c7-4beb-a1f1-b73e69aed549` | 18 |
| 12 | house B, Tripo detailed | `f30384e5-356d-488e-a471-7d33e346e2af` | 18 |
| 13 | fountain, Tripo detailed | `5463d592-05b0-4cc8-8a60-1084a734a71d` | 18 |
| 14 | crate, Tripo standard geometry | `24760041-1012-4e81-ae56-ab5c0ba0c5fb` | 12 |
| 15 | clip, walk (613 Casual_Walk_inplace) | `022c7aff-8c94-4531-928b-23c33ddf0cd2` | 8 |
| 16 | clip, look (338 Short_Breathe_and_Look_Around) | `a1d9b430-5d14-4e01-89cc-2453ba6e9fcc` | 8 |
| 17 | clip, pick up (278 Female_Stand_Pick_Fruit_Basket) | `51b499fe-aa81-4008-a376-510e1734a2ca` | 8 |
| 18 | clip, sent home (391 Head_Hold_in_Pain) | `a1a97df8-2521-4e46-8277-5b1644b914c9` | 8 |
| 19 | clip, idle again (the one retry; still unusable, see above) | `60c61125-9c96-43b1-93f5-fcf2197d87a6` | 8 |
| | **total** | | **181.75** (planned 175.5, cap 220) |

## Machine checks

| Check | How | Result |
|---|---|---|
| Kit materials are URP Lit with a base map and a normal map | AI Arena > Check market kit materials (`MarketKit.CheckMaterials`): every renderer of every prefab in `Resources/Market` | **met**: 183 renderers, 29 materials, 0 failing (the kit has no effects; the sight square, the loop's ring and the memory's cards are made at run time) |
| No Flat shader and no MeshKit mesh on the world | the walk's `WorldCheck` on the run screen: every renderer in the scene by shader, MeshKit's meshes by name | **met**: on the run screen 209 renderers, 221 material slots URP Lit, 4 on the project's Glass shader (the sight square and the memory's cards, effects), Flat 0; one MeshKit mesh, the ring that flashes under the agent on LOOK (an effect) |
| The agent fills at least 25 % of the frame on the main play screen | the walk's `Coverage`: the agent's skinned meshes' box, eight corners projected through the camera | **met**: the street (the main play screen) 40 % × 77 % = **31 %** of the frame, the agent 1.8 m from the camera (32 % in the walk before) (Claude Bomb's device: 40 %); the garage 43 % × 70 % = 30 %; the agent's eye 9 to 17 % (it looks past the agent, by design); the title frames the agent at about the same size as the garage |
| No light-theme panel on a play screen; the world at least 65 %; sheets at most 35 %; text at least 16 px at 1080p | the walk's `UiCheck` on the garage and the run: any fill over 75 % luminance and half opaque bigger than 200 by 120, the drawer's width, the smallest font | **met**: 0 light panels on the garage and the run; the run's drawer 600 of 1920 units (31 %, world 69 %), the garage's 640 (33 %, world 67 %), the decide errand's 660 (34 %); smallest text 16 on the 1080 canvas on both screens (`Ui.Text` clamps every text to 16) |
| Under 45 MB transferred, loads and runs on the platform | `loadtime.mjs` (headless Chrome, software GL) on the local platform, port 8085 | **met**: 32.8 MB transferred, loaded in 2.9 to 3.3 s (three runs; 3.1 s on the final build), the title runs on the platform (`realism/platform-ai-arena.png`) |

The walk's lines are in the player log of the last walk (`Builds/look-audit/realism/walk9`).
The Flat shader and MeshKit remain in the code for the effects (the sight square, the ring
that flashes on LOOK, the glass cards of the agent's memory) and as the fallback when the
kit is missing.

## Web, before and after

Measured with `loadtime.mjs` (headless Chrome, software GL, the local platform on port 8085,
cache off): the bytes transferred and the time until the loader hides.

| | Before (the build on the platform this morning) | After (this pass) |
|---|---|---|
| Transferred | 20.1 MB (the text pass recorded 19.2 MB) | **32.8 MB** (budget 45) |
| Load | 5.4 s (measured while a Unity build ran; the text pass recorded 3.7 s) | **3.1 s** on the final build (2.9 and 3.3 s on the build before it; 4.3 s with a Unity build alongside) |
| data / wasm (gzip, on disk) | 10.4 / 9.3 MB | 23.1 / 9.4 MB (32 MB in all) |

Where the after build's bytes are (Unity's build report, uncompressed): textures 20.1 MB (every
kit texture crunched at quality 50; 2K only for the agent's colour map, 1K for the surfaces
and the heroes' colour maps, 512 for normal and mask maps and the Poly Haven props; mask maps
dropped where a model has almost no metal), meshes 8.5 MB (the agent's four rigged copies are
5.6 MB of it), sounds 3.4 MB, animations 1.4 MB, shaders 1.2 MB. The game runs on the
platform: `Builds/look-audit/realism/platform-ai-arena.png` is the title captured 8 s after
the load (`platform-ai-arena-before.png` the build that was there before).

## The side-by-sides

Claude Bomb's frame on the left (`games/claude-bomb/Builds/look-audit/realism/`), AI Arena's
on the right, both 1600 × 900, a 16 px gutter (3216 × 900, Tech Escape's format), in
`games/ai-arena/Builds/look-audit/realism/final/`:

| File | Claude Bomb | AI Arena |
|---|---|---|
| `side-by-side-1-title.png` | its title (the device, the camera feed) | the title: the agent close in the lantern-lit evening square |
| `side-by-side-2-story.png` | the incoming transmission | the story (LUMA's line, the loop in four words) over the evening square |
| `side-by-side-3-briefing.png` | the briefing's camera feed | the briefing over the square turning at roof height |
| `side-by-side-4-garage.png` | ready to play | the garage: the agent with its dials, the list in the drawer |
| `side-by-side-5-market-run.png` | its play screen | the run, the main play screen: the street behind the agent |
| `side-by-side-6-agents-eye.png` | its play screen, later | the agent's eye, framed as a camera feed |
| `side-by-side-7-decide.png` | its play screen, clicked | the decide errand (1.6): the tasks in the drawer beside the square |
| `side-by-side-8-debrief.png` | the tour escaped | the debrief on glass over the dimmed square |

The frames come from the Mac build's screenshot walk (`Builds/look-audit/realism/walk9`, all
fourteen screens, plus `6c-overview.png`, the market from above). The steps on the way are in
`walk1` to `walk8`, the kit's renders in `realism/kit` and the models from four sides and the
agent's clips in `realism/models`. `realism/before-after/` has the text pass's frames (left)
against this pass's (right) for the title, the garage, the run, the agent's eye, the decide
errand and the debrief.

## What I could not do, or did not

- **No reviewer was run**, as asked: the side-by-sides are for you to judge.
- **The fallback without the kit was not run** in this pass: the old code paths are kept
  behind `Market.Real` and compile, but every build here had the kit.
- **Meshy's idle clip** (two jobs, 16 credits of the ledger) turned out unusable: its hips
  swing through 90 degrees, so the robot never faced where it walked. The look clip, slowed,
  is the idle; the agent has walk, look, pick up and sent home, not a separate idle.
- **Clip copies cost space**: each rigged copy is 1.4 MB in the build (four copies, 5.6 MB of
  the 8.5 MB of meshes). They are kept, as Tech Escape keeps its attacker's, because the
  rigging jobs' skeletons do not share bone orientations.
- **Fruit and vegetables are Kenney's low-poly pieces** (smoothed, palette-textured). Poly
  Haven's scanned apple, lemon and pears are imported but not used: a heaped crate of them would be some 70k
  triangles, and 26 stalls of them would not run on a school laptop. Only the potatoes are
  Poly Haven's. Up close the Kenney pieces read stylised next to the semi-realistic stalls.
- **The overview is still a board**: from above, the grid of stalls is what the lesson needs
  (where the agent can see, who is where). It is now the third view, shown only when asked
  for, and a real square with houses round it, but it is the screen that most resembles the
  old game.
- **The story's pictures** (the house, the little robot, the stalls) stay the flat 2D sprites
  the art bible keeps on purpose; their board is now ink instead of white.
- **Trees**: the old park's trees went with the park. No CC0 tree light enough for the web
  fitted (Poly Haven's are millions of triangles); the square is closed by houses, with
  planters and potted plants for green.
- **`Docs/look-audit.md` was not touched**, as instructed (the prompt asks for a Phase 6
  section there; this report is that section's material).
- **The decide screen's LUMA line** is cut by the screenshot mid-typing ("Mark each task: an
  agen..."): it is typed as she says it, as on every screen; the walk captures it early.
