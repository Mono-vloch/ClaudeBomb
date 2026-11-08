# LUMA, the finished model

LUMA as drawn in the brand file `Looma-Final.ai` (repository root): a navy head and
egg-shaped body, two interlocking teal rings as glasses, mint pill eyes and a mint
smile that glow, dark tube arms with teal collars at the shoulders and teal cuffs at the
wrists, navy mitten hands. The model is the file's third drawing, arms up; `LumaRig`
lowers the arms from there for her moods.

The first LUMA (`Assets/Assets/LUMA.fbx`, `Assets/Models/LUMA_25k.asset`) had the right
shape but no paint: one white material, no textures. It stays in the project as the
size and frame the installer matches; the scene no longer draws it.

## Where it comes from (Higgsfield, 7 November 2026)

1. The drawing's arms-up pose, cut from `Looma-Final.ai`, went to GPT Image 2.6
   (`gpt_image_2_6`, high quality, 2K) with the whole sheet as an identity reference,
   asking for the same character as a front-view 3D render on a plain background.
   Job `ce0ae817-e104-45fb-af4e-deda58bd851b`.
2. That render went to Meshy 7 image-to-3D (`meshy_v7_image_to_3d`: textured, PBR,
   40k triangles, ultra mode, symmetry off). Job `7cb374da-2199-48f7-bd1a-648eabf820e8`.
   `LUMA.glb` is that file, unchanged.
3. `Tools/LumaModel/clean_luma.py LUMA.glb Textures` makes the URP textures: the colour
   map with the specks of teal Meshy baked into the navy at its island edges painted
   over, the normal map, metallic + smoothness, and an emission map so the eyes and
   smile glow mint and the teal is bright, as in the drawing.
4. **Claude Bomb > Install LUMA Model** (`Assets/Editor/LumaModelTools.cs`) bakes the
   GLB's mesh into `LUMA.asset` in the first model's frame and size (Z up, facing -Y),
   builds `LUMA.mat` (URP Lit) and puts both on the scene's LUMA. It also sets
   `LumaRig`'s shoulders at her collars and marks her glasses in the vertex colours,
   so the rig keeps the glasses on the head while the wrist cuffs right beside them
   follow the arms (without the mark, the cuffs tear into spikes when she moves).
5. **Claude Bomb > Render LUMA Portrait** renders the 2D LUMA the other games show
   (`games/*/Assets/Resources/Luma/luma.png`).

To redo it from a new Meshy file: copy it here as `LUMA.glb`, run the script, measure
her collars for `LumaModelTools.Shoulder` (fractions of the model's size: x out from the
middle of the hand-to-hand width, y up from the base, z toward the face), run Install,
and watch her moods in Play.
