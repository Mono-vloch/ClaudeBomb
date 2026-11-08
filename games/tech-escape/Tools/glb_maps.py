#!/usr/bin/env python3
"""glb_maps.py: split an image-to-3D GLB (Tripo, Meshy) into what the facility kit builds heroes from.

    python3 -I Tools/glb_maps.py <in.glb> <out folder> <file stem> [--size 1024] [--emission green|green+amber|none]

Writes, in <out folder>:
  <stem>.glb           the geometry only (positions, normals, UVs, indices; no images, no textures)
  <stem>_base.jpg      the base colour, downsized to --size (2048 for heroes, 1024 for props)
  <stem>_normal.png    the normal map, same size
  <stem>_mask.png      URP's metallic-smoothness map: metal in R (glTF's B), smoothness in A (1 - glTF's G)
  <stem>_emission.png  where something glows, taken from the base colour's lamp pixels (--emission)

The realism pass's tool of the same name (7 November 2026) lived in a session's scratch folder
and was lost; this is a rewrite to the same outputs (Docs/credits.md, "Higgsfield"). Images go
through ImageMagick (`magick`); nothing else beyond the standard library.
"""
import json, struct, subprocess, sys, os, tempfile, colorsys


def read_glb(path):
    d = open(path, "rb").read()
    magic, version, length = struct.unpack("<4sII", d[:12])
    if magic != b"glTF" or version != 2:
        sys.exit(f"{path}: not a glTF 2 binary")
    jl, jt = struct.unpack("<II", d[12:20])
    j = json.loads(d[20:20 + jl])
    b0 = 20 + jl
    bl, bt = struct.unpack("<II", d[b0:b0 + 8])
    return j, d[b0 + 8:b0 + 8 + bl]


def image_bytes(j, b, index):
    img = j["images"][index]
    bv = j["bufferViews"][img["bufferView"]]
    o = bv.get("byteOffset", 0)
    return b[o:o + bv["byteLength"]], img.get("mimeType", "image/png")


def texture_image(j, ref):
    if ref is None:
        return None
    return j["textures"][ref["index"]]["source"]


def write_geometry(j, b, out):
    # Keep only the buffer views the accessors use, packed again on 4-byte boundaries.
    used = sorted({a["bufferView"] for a in j["accessors"] if "bufferView" in a})
    blob = bytearray()
    remap = {}
    views = []
    for i in used:
        bv = dict(j["bufferViews"][i])
        o = bv.get("byteOffset", 0)
        while len(blob) % 4:
            blob.append(0)
        bv["byteOffset"] = len(blob)
        bv["buffer"] = 0
        blob += b[o:o + bv["byteLength"]]
        remap[i] = len(views)
        views.append(bv)
    while len(blob) % 4:
        blob.append(0)
    g = dict(j)
    g["bufferViews"] = views
    g["accessors"] = [dict(a, bufferView=remap[a["bufferView"]]) if "bufferView" in a else a for a in j["accessors"]]
    g["buffers"] = [{"byteLength": len(blob)}]
    for k in ("images", "textures", "samplers"):
        g.pop(k, None)
    mats = []
    for m in j.get("materials", []):
        pbr = dict(m.get("pbrMetallicRoughness", {}))
        for k in ("baseColorTexture", "metallicRoughnessTexture"):
            pbr.pop(k, None)
        mats.append({"name": m.get("name", "material"), "pbrMetallicRoughness": pbr})
    g["materials"] = mats
    js = json.dumps(g, separators=(",", ":")).encode()
    while len(js) % 4:
        js += b" "
    total = 12 + 8 + len(js) + 8 + len(blob)
    with open(out, "wb") as f:
        f.write(struct.pack("<4sII", b"glTF", 2, total))
        f.write(struct.pack("<II", len(js), 0x4E4F534A)); f.write(js)
        f.write(struct.pack("<II", len(blob), 0x004E4942)); f.write(blob)


def magick(*args):
    subprocess.run(["magick", *args], check=True)


def main():
    a = sys.argv[1:]
    if len(a) < 3:
        sys.exit(__doc__)
    src, out_dir, stem = a[0], a[1], a[2]
    size = int(a[a.index("--size") + 1]) if "--size" in a else 1024
    emission = a[a.index("--emission") + 1] if "--emission" in a else "none"
    os.makedirs(out_dir, exist_ok=True)
    j, b = read_glb(src)
    mat = j["materials"][0]
    pbr = mat.get("pbrMetallicRoughness", {})
    tmp = tempfile.mkdtemp(prefix="glb_maps_")

    def dump(index, name):
        data, mime = image_bytes(j, b, index)
        p = os.path.join(tmp, name + (".jpg" if "jpeg" in mime else ".png"))
        open(p, "wb").write(data)
        return p

    base = dump(texture_image(j, pbr.get("baseColorTexture")), "base")
    magick(base, "-resize", f"{size}x{size}", "-quality", "90", os.path.join(out_dir, stem + "_base.jpg"))
    if mat.get("normalTexture"):
        normal = dump(texture_image(j, mat["normalTexture"]), "normal")
        magick(normal, "-resize", f"{size}x{size}", "-alpha", "off", "PNG24:" + os.path.join(out_dir, stem + "_normal.png"))
    if pbr.get("metallicRoughnessTexture"):
        mr = dump(texture_image(j, pbr["metallicRoughnessTexture"]), "mr")
        mask = os.path.join(out_dir, stem + "_mask.png")
        magick(mr, "-resize", f"{size}x{size}", "-write", "mpr:mr", "+delete",
               "(", "mpr:mr", "-channel", "B", "-separate", "+channel", ")",
               "(", "mpr:mr", "-channel", "G", "-separate", "+channel", "-fill", "white", "-colorize", "100", ")",
               "(", "mpr:mr", "-channel", "G", "-separate", "+channel", "-fill", "black", "-colorize", "100", ")",
               "(", "mpr:mr", "-channel", "G", "-separate", "+channel", "-negate", ")",
               "-channel", "RGBA", "-combine", "PNG32:" + mask)
    if emission != "none":
        # The lamps: small, bright, saturated pixels of the given hues in the base colour.
        raw = subprocess.run(["magick", base, "-resize", f"{size}x{size}", "-depth", "8", "rgb:-"], check=True, capture_output=True).stdout
        n = len(raw) // 3
        side = int(round(n ** 0.5))
        outb = bytearray(len(raw))
        lit = 0
        for i in range(n):
            r, g, bb = raw[3 * i], raw[3 * i + 1], raw[3 * i + 2]
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, bb / 255)
            h *= 360
            hit = s > 0.45 and v > 0.55 and (85 <= h <= 160 or ("amber" in emission and 30 <= h <= 48 and v > 0.85))
            if hit:
                outb[3 * i:3 * i + 3] = bytes((r, g, bb)); lit += 1
        subprocess.run(["magick", "-size", f"{side}x{side}", "-depth", "8", "rgb:-", "PNG24:" + os.path.join(out_dir, stem + "_emission.png")], input=bytes(outb), check=True)
        print(f"emission: {lit} of {n} pixels lit ({100.0 * lit / n:.2f} %)")
    write_geometry(j, b, os.path.join(out_dir, stem + ".glb"))
    tris = sum(j["accessors"][p["indices"]]["count"] // 3 for m in j["meshes"] for p in m["primitives"] if "indices" in p)
    print(f"{stem}: {tris} triangles, maps at {size}, written to {out_dir}")


if __name__ == "__main__":
    main()
