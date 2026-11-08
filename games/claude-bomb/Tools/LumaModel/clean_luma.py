"""Prepare the Meshy LUMA's textures for the game.

    python3 -I clean_luma.py <meshy.glb> <textures dir>

Writes the model's textures to <textures dir>, ready for URP (copy the GLB itself to
Assets/Models/LUMA/LUMA.glb as it is):
   LUMA_BaseColor.png   the colour map, with the stray specks of teal that Meshy baked
                        into the navy (and navy into the teal) at its island edges
                        painted over with the colour around them;
   LUMA_Normal.png      the normal map as it came (OpenGL convention, as Unity's);
   LUMA_MetalSmooth.png R = metallic (none), A = smoothness (1 - glTF roughness);
   LUMA_Emission.png    the mint eyes and smile in full, the teal at about half, else black.

Needs ImageMagick 7 (`magick`) on the PATH for the image work.
"""
import json
import os
import struct
import subprocess
import sys
import tempfile

src, tex_dir = sys.argv[1], sys.argv[2]
data = open(src, "rb").read()
off, gltf, bin_off = 12, None, None
while off < len(data):
    clen, ctype = struct.unpack_from("<I4s", data, off)
    if ctype == b"JSON":
        gltf = json.loads(data[off + 8: off + 8 + clen])
    else:
        bin_off = off + 8
    off += 8 + clen


def image_bytes(i):
    img = gltf["images"][i]
    bv = gltf["bufferViews"][img["bufferView"]]
    s = bin_off + bv.get("byteOffset", 0)
    return bytes(data[s: s + bv["byteLength"]]), img.get("mimeType")


def magick(*args):
    subprocess.run(["magick", *args], check=True)


# ---- Textures ----
mat = gltf["materials"][0]
pbr = mat["pbrMetallicRoughness"]
tmp = tempfile.mkdtemp()


def extract(tex_index, name):
    raw, mime = image_bytes(gltf["textures"][tex_index]["source"])
    path = os.path.join(tmp, name + (".png" if mime == "image/png" else ".jpg"))
    open(path, "wb").write(raw)
    return path


base = extract(pbr["baseColorTexture"]["index"], "base")
normal = extract(mat["normalTexture"]["index"], "normal")
orm = extract(pbr["metallicRoughnessTexture"]["index"], "orm")
os.makedirs(tex_dir, exist_ok=True)

# Specks: a pixel whose colour class (navy / teal / mint) differs from the one most of
# its 7x7 neighbourhood has is repainted with the neighbourhood's median colour. The
# islands are large flat patches, so only specks and one-pixel slivers change.
med = os.path.join(tmp, "median.png")
magick(base, "-statistic", "Median", "7x7", med)
cls = os.path.join(tmp, "class.png")
cls_med = os.path.join(tmp, "class_median.png")
# class as a grey level: navy 0, teal 0.5, mint 1
fx_class = "(r>0.42 && g>0.68) ? 1.0 : ((g>0.5) ? 0.5 : 0)"
magick(base, "-fx", fx_class, "-colorspace", "Gray", cls)
magick(med, "-fx", fx_class, "-colorspace", "Gray", cls_med)
odd = os.path.join(tmp, "odd.png")
magick(cls, cls_med, "-compose", "Difference", "-composite", "-threshold", "10%", odd)
base_out = os.path.join(tex_dir, "LUMA_BaseColor.png")
magick(base, med, odd, "-compose", "Over", "-composite", base_out)

magick(normal, os.path.join(tex_dir, "LUMA_Normal.png"))
metal = os.path.join(tmp, "metal.png")
smooth = os.path.join(tmp, "smooth.png")
magick(orm, "-channel", "B", "-separate", "+channel", metal)
magick(orm, "-channel", "G", "-separate", "+channel", "-negate", smooth)
magick(metal, metal, metal, smooth, "-channel", "RGBA", "-combine", "-define", "png:color-type=6",
       os.path.join(tex_dir, "LUMA_MetalSmooth.png"))
glow = os.path.join(tmp, "glow.png")
magick(base_out, "-fx", "(r>0.42 && g>0.68) ? 1.0 : ((r<0.38 && g>0.55) ? 0.45 : 0)",
       "-colorspace", "Gray", "-morphology", "Dilate", "Disk:1", "-blur", "0x0.8", glow)
magick(base_out, glow, "-compose", "Multiply", "-composite", os.path.join(tex_dir, "LUMA_Emission.png"))
print("textures written to", tex_dir)
