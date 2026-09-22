import os
import struct
import io
from PIL import Image, ImageFilter

ASSETS_DIR = r"d:\OneDrive\Storage\General\Programming\PDFaNoter\PDFaNoter\Assets"
MASTER_IMAGE = os.path.join(ASSETS_DIR, "PDFaNote_ikon.png")

assert os.path.exists(MASTER_IMAGE), f"Master image not found: {MASTER_IMAGE}"
master_img = Image.open(MASTER_IMAGE)
art = master_img.crop((248, 228, 1192, 1212)) # (944, 984)

def create_square_icon(N):
    if N <= 20:
        th = N - 2
    elif N <= 32:
        th = N - 3 if N % 2 == 1 else N - 4
    elif N <= 48:
        th = N - 4
    elif N <= 72:
        th = N - 6
    else:
        th = round(N * (984.0 / 1072.0))
    tw = round(th * (944.0 / 984.0))
    res = art.resize((tw, th), Image.Resampling.LANCZOS)
    if N <= 24:
        res = res.filter(ImageFilter.UnsharpMask(radius=0.5, percent=50, threshold=1))
    elif N <= 48:
        res = res.filter(ImageFilter.UnsharpMask(radius=0.7, percent=40, threshold=1))
    canvas = Image.new("RGBA", (N, N), (0, 0, 0, 0))
    ox = (N - tw) // 2
    oy = (N - th) // 2
    canvas.paste(res, (ox, oy), res)
    return canvas

def create_wide_icon(w, h):
    # Wide tile: square icon centered vertically and horizontally
    sq = create_square_icon(h)
    canvas = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    ox = (w - h) // 2
    canvas.paste(sq, (ox, 0), sq)
    return canvas

def make_ico(images_dict, output_path):
    entries = []
    data_blobs = []
    for (w, h), img in sorted(images_dict.items(), key=lambda x: x[0][0]):
        img = img.convert("RGBA")
        if w == 256:
            buf = io.BytesIO()
            img.save(buf, format="PNG")
            blob = buf.getvalue()
        else:
            bih = struct.pack("<IIIHHIIIIII",
                40, w, h * 2, 1, 32, 0, w * h * 4, 0, 0, 0, 0
            )
            raw = img.tobytes("raw", "BGRA")
            row_size = w * 4
            pixels = bytearray()
            for y in range(h - 1, -1, -1):
                pixels.extend(raw[y*row_size:(y+1)*row_size])
            row_bytes = ((w + 31) // 32) * 4
            and_mask = bytearray(row_bytes * h)
            blob = bih + bytes(pixels) + bytes(and_mask)
            
        b_w = 0 if w >= 256 else w
        b_h = 0 if h >= 256 else h
        entries.append({
            "w": b_w, "h": b_h, "colors": 0, "res": 0,
            "planes": 1, "bpp": 32, "size": len(blob)
        })
        data_blobs.append(blob)
        
    offset = 6 + len(entries) * 16
    with open(output_path, "wb") as f:
        f.write(struct.pack("<HHH", 0, 1, len(entries)))
        for entry in entries:
            f.write(struct.pack("<BBBBHHII",
                entry["w"], entry["h"], entry["colors"], entry["res"],
                entry["planes"], entry["bpp"], entry["size"], offset
            ))
            offset += entry["size"]
        for blob in data_blobs:
            f.write(blob)

def main():
    generated_files = []

    # 1. Target sizes for Square44x44Logo
    target_sizes = [16, 20, 24, 30, 32, 36, 40, 44, 48, 60, 64, 72, 80, 96, 256]
    for s in target_sizes:
        img = create_square_icon(s)
        for suffix in [
            f"targetsize-{s}_altform-unplated.png",
            f"targetsize-{s}_altform-lightunplated.png",
            f"targetsize-{s}.png"
        ]:
            fn = f"Square44x44Logo.{suffix}"
            p = os.path.join(ASSETS_DIR, fn)
            img.save(p, "PNG")
            generated_files.append(fn)

    # 2. Scale factors for Square44x44Logo
    scales_44 = {
        "scale-100": 44,
        "scale-125": 55,
        "scale-150": 66,
        "scale-200": 88,
        "scale-400": 176
    }
    for tag, sz in scales_44.items():
        fn = f"Square44x44Logo.{tag}.png"
        img = create_square_icon(sz)
        p = os.path.join(ASSETS_DIR, fn)
        img.save(p, "PNG")
        generated_files.append(fn)

    # 3. Scale factors for Square150x150Logo
    scales_150 = {
        "scale-100": 150,
        "scale-125": 188,
        "scale-150": 225,
        "scale-200": 300,
        "scale-400": 600
    }
    for tag, sz in scales_150.items():
        fn = f"Square150x150Logo.{tag}.png"
        img = create_square_icon(sz)
        p = os.path.join(ASSETS_DIR, fn)
        img.save(p, "PNG")
        generated_files.append(fn)

    # 4. Scale factors for Wide310x150Logo
    scales_310 = {
        "scale-100": (310, 150),
        "scale-125": (388, 188),
        "scale-150": (465, 225),
        "scale-200": (620, 300),
        "scale-400": (1240, 600)
    }
    for tag, (w, h) in scales_310.items():
        fn = f"Wide310x150Logo.{tag}.png"
        img = create_wide_icon(w, h)
        p = os.path.join(ASSETS_DIR, fn)
        img.save(p, "PNG")
        generated_files.append(fn)

    # 5. Scale factors for SplashScreen
    scales_splash = {
        "scale-100": (620, 300),
        "scale-125": (775, 375),
        "scale-150": (930, 450),
        "scale-200": (1240, 600),
        "scale-400": (2480, 1200)
    }
    for tag, (w, h) in scales_splash.items():
        fn = f"SplashScreen.{tag}.png"
        img = create_wide_icon(w, h)
        p = os.path.join(ASSETS_DIR, fn)
        img.save(p, "PNG")
        generated_files.append(fn)

    # 6. Scale factors for LockScreenLogo
    scales_lock = {
        "scale-100": 24,
        "scale-125": 30,
        "scale-150": 36,
        "scale-200": 48,
        "scale-400": 96
    }
    for tag, sz in scales_lock.items():
        fn = f"LockScreenLogo.{tag}.png"
        img = create_square_icon(sz)
        p = os.path.join(ASSETS_DIR, fn)
        img.save(p, "PNG")
        generated_files.append(fn)

    # 7. StoreLogo
    store_scales = {
        "StoreLogo.png": 50,
        "StoreLogo.scale-100.png": 50,
        "StoreLogo.scale-125.png": 63,
        "StoreLogo.scale-150.png": 75,
        "StoreLogo.scale-200.png": 100,
        "StoreLogo.scale-400.png": 200
    }
    for fn, sz in store_scales.items():
        img = create_square_icon(sz)
        p = os.path.join(ASSETS_DIR, fn)
        img.save(p, "PNG")
        generated_files.append(fn)

    # Base un-qualified fallback files
    base_fallbacks = {
        "Square44x44Logo.png": create_square_icon(44),
        "Square150x150Logo.png": create_square_icon(150),
        "Wide310x150Logo.png": create_wide_icon(310, 150),
        "SplashScreen.png": create_wide_icon(620, 300),
        "LockScreenLogo.png": create_square_icon(24),
    }
    for fn, img in base_fallbacks.items():
        p = os.path.join(ASSETS_DIR, fn)
        img.save(p, "PNG")
        generated_files.append(fn)

    # 8. AppIcon.ico with all standard sizes
    ico_sizes = [16, 20, 24, 30, 32, 36, 40, 44, 48, 60, 64, 72, 80, 96, 128, 256]
    ico_dict = {
        (s, s): create_square_icon(s)
        for s in ico_sizes
    }
    ico_path = os.path.join(ASSETS_DIR, "AppIcon.ico")
    make_ico(ico_dict, ico_path)
    generated_files.append("AppIcon.ico")

    print(f"Generated {len(generated_files)} assets successfully.")
    return sorted(set(generated_files))

if __name__ == "__main__":
    main()
