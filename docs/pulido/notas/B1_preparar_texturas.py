"""Prepara las texturas de B1 con sus nombres finales: color y normal a 1024 en JPG,
calcomanías de humedad con alfa y calcomanías propias dibujadas aquí."""
import os, sys
from PIL import Image, ImageDraw, ImageFont, ImageFilter

DL = sys.argv[1]
OUT = sys.argv[2]
TEX = os.path.join(OUT, "Kit")
DEC = os.path.join(OUT, "Kit", "Calcomanias")
os.makedirs(DEC, exist_ok=True)

# nombre final: (carpeta descargada, archivo color, archivo normal)
MATERIALS = {
    "AzulejoBlanco": ("Tiles107", None, None),
    "AzulejoPiscina": ("Tiles132B", None, None),
    "Alfombra": ("Carpet016", None, None),
    "PapelTapiz": ("Wallpaper002C", None, None),
    "TechoPlacas": ("OfficeCeiling003", None, None),
    "Concreto": ("Concrete034", None, None),
    "MetalVerde": ("PaintedMetal006", None, None),
    "MetalAmarillo": ("PaintedMetal016", None, None),
    "PisoTienda": ("Tiles139", None, None),
    "Yeso": ("Plaster007", None, None),
    "Asfalto": ("Asphalt031", None, None),
    "MaderaPintada": ("PaintedWood007B", None, None),
    "Tierra": ("Ground109", None, None),
    "PastoSeco": ("withered_grass", "withered_grass_diff_1k.png", "withered_grass_nor_gl_1k.png"),
}

def fit(im, size=1024):
    if max(im.size) > size:
        im = im.resize((size, size), Image.LANCZOS)
    return im

for name, (folder, color, normal) in MATERIALS.items():
    src = os.path.join(DL, folder)
    color = color or f"{folder}_1K-PNG_Color.png"
    normal = normal or f"{folder}_1K-PNG_NormalGL.png"
    d = os.path.join(TEX, name)
    os.makedirs(d, exist_ok=True)
    fit(Image.open(os.path.join(src, color)).convert("RGB")).save(os.path.join(d, f"Kit_{name}_Color.jpg"), quality=90)
    fit(Image.open(os.path.join(src, normal)).convert("RGB")).save(os.path.join(d, f"Kit_{name}_Normal.jpg"), quality=95)
    print("ok", name)

# Manchas de humedad: la forma está en el mapa de desplazamiento de ambientCG (el color es casi blanco)
for i, folder in enumerate(["Leaking012A", "Leaking013A", "Leaking017B"], 1):
    disp = Image.open(os.path.join(DL, folder, f"{folder}_1K-PNG_Displacement.png"))
    top = max(1, disp.getextrema()[1])
    alpha = disp.point(lambda v: v * (255.0 * 1.6 / top), "F").convert("L")
    alpha = alpha.point(lambda v: min(255, v))
    stain = Image.new("RGBA", alpha.size, (78, 66, 42, 255))
    stain.putalpha(alpha)
    stain.resize((512, 512), Image.LANCZOS).save(os.path.join(DEC, f"Calco_Humedad{i}.png"))
    print("ok humedad", i)

def font(size, bold=True):
    for f in (["arialbd.ttf", "Arial Bold.ttf"] if bold else ["arial.ttf"]):
        try:
            return ImageFont.truetype(f, size)
        except OSError:
            pass
    return ImageFont.load_default()

def centered(draw, box, text, f, fill):
    l, t, r, b = draw.textbbox((0, 0), text, font=f)
    x = box[0] + (box[2] - box[0] - (r - l)) / 2 - l
    y = box[1] + (box[3] - box[1] - (b - t)) / 2 - t
    draw.text((x, y), text, font=f, fill=fill)

def aged(im, amount=18):
    # Un poco de suciedad pareja para que no se vea recién impreso
    noise = Image.effect_noise(im.size, amount).convert("L")
    dirt = Image.new("RGBA", im.size, (60, 50, 30, 0))
    dirt.putalpha(noise.point(lambda v: max(0, v - 150)))
    out = Image.alpha_composite(im, dirt)
    a = im.getchannel("A")
    out.putalpha(a)
    return out

def sign(name, size, bg, fg, lines, border=None, radius=24):
    im = Image.new("RGBA", size, (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((4, 4, size[0] - 5, size[1] - 5), radius=radius, fill=bg, outline=border, width=10 if border else 0)
    n = len(lines)
    for k, (text, fsize) in enumerate(lines):
        top = 20 + (size[1] - 40) * k / n
        centered(d, (20, top, size[0] - 20, top + (size[1] - 40) / n), text, font(fsize), fg)
    aged(im).save(os.path.join(DEC, f"Calco_{name}.png"))
    print("ok", name)

# Señal de salida verde, la clásica de pasillo
sign("Salida", (1024, 384), (24, 140, 70, 255), (240, 255, 240, 255), [("EXIT  →", 190)], border=(240, 255, 240, 255))
# Números de pasillo: el 7 vuelve a aparecer donde no debería
for num in ["7", "13", "7B"]:
    sign(f"Pasillo{num}", (512, 640), (230, 200, 60, 255), (25, 25, 25, 255), [("AISLE", 110), (num, 300)], border=(25, 25, 25, 255), radius=16)
sign("PisoMojado", (512, 640), (250, 205, 30, 255), (20, 20, 20, 255), [("CAUTION", 90), ("WET", 150), ("FLOOR", 150)], border=(20, 20, 20, 255), radius=10)
sign("NoCorrer", (768, 512), (245, 245, 240, 255), (180, 30, 30, 255), [("NO RUNNING", 120), ("BY THE POOL", 80)], border=(180, 30, 30, 255), radius=8)
sign("SoloEmpleados", (768, 256), (40, 40, 45, 255), (235, 235, 235, 255), [("EMPLOYEES ONLY", 100)], radius=8)

# Afiche de la promo de papas: el producto que nunca aparece
im = Image.new("RGBA", (640, 896), (0, 0, 0, 0))
d = ImageDraw.Draw(im)
d.rectangle((0, 0, 639, 895), fill=(210, 40, 35, 255))
d.ellipse((90, 300, 550, 700), fill=(245, 200, 60, 255))
centered(d, (0, 30, 640, 170), "POTATO", font(130), (255, 235, 120, 255))
centered(d, (0, 150, 640, 280), "CHIPS!", font(130), (255, 235, 120, 255))
centered(d, (90, 420, 550, 580), "NEW!", font(120), (180, 30, 30, 255))
centered(d, (0, 740, 640, 830), "AISLE 7", font(80), (255, 255, 255, 255))
aged(im, 24).save(os.path.join(DEC, "Calco_AfichePapas.png"))
print("ok afiche")
