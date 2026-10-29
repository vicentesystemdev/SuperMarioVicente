"""Generates the original pixel art sprites used by the water level (2-2).

All sprites are new, drawn procedurally for this project (no external or
Nintendo assets). Output: Assets/Sprites/Water/*.png at 16 pixels per unit.

Usage (from the repository root):
    python Tools/generate_water_sprites.py
"""

import math
import os
import random

from PIL import Image

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Sprites", "Water")

CLEAR = (0, 0, 0, 0)
OUTLINE = (27, 27, 58, 255)


def hexc(value, alpha=255):
    value = value.lstrip("#")
    return tuple(int(value[i:i + 2], 16) for i in (0, 2, 4)) + (alpha,)


def save(image, name):
    os.makedirs(OUT, exist_ok=True)
    image.save(os.path.join(OUT, name + ".png"))
    print("written", name)


def outline(image, color=OUTLINE):
    """Adds a 1px outline around every opaque pixel (4-neighbourhood)."""
    width, height = image.size
    source = image.copy()
    for y in range(height):
        for x in range(width):
            if source.getpixel((x, y))[3] != 0:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < width and 0 <= ny < height and source.getpixel((nx, ny))[3] == 255:
                    image.putpixel((x, y), color)
                    break


def block(name, base, light, dark, speck, seed):
    """Tileable 16x16 stone block with bevel and speckles."""
    rng = random.Random(seed)
    image = Image.new("RGBA", (16, 16), hexc(base))
    for x in range(16):
        image.putpixel((x, 0), hexc(light))
        image.putpixel((x, 15), hexc(dark))
    for y in range(16):
        image.putpixel((0, y), hexc(light))
        image.putpixel((15, y), hexc(dark))
    # inner crack line and speckles
    for x in range(3, 9):
        image.putpixel((x, 6 + (x % 2)), hexc(dark))
    for _ in range(14):
        image.putpixel((rng.randint(2, 13), rng.randint(2, 13)), hexc(speck))
    save(image, name)


def coral():
    """Vertically tileable coral column with side knobs."""
    image = Image.new("RGBA", (16, 16), CLEAR)
    body, light, dark = hexc("#e8607a"), hexc("#ffa6b6"), hexc("#a02848")
    for y in range(16):
        for x in range(3, 13):
            image.putpixel((x, y), body)
        image.putpixel((3, y), dark)
        image.putpixel((12, y), dark)
        image.putpixel((5, y), light)
    for (x, y) in ((1, 3), (2, 3), (1, 4), (2, 4), (13, 10), (14, 10), (13, 11), (14, 11)):
        image.putpixel((x, y), body)
    for (x, y) in ((8, 2), (9, 7), (7, 11), (10, 13)):
        image.putpixel((x, y), dark)
    save(image, "Water_Coral")


def surface():
    """Horizontally tileable wave crest for the water surface."""
    image = Image.new("RGBA", (16, 16), CLEAR)
    for x in range(16):
        crest = 3 + round(1.5 * math.sin(x / 16 * 2 * math.pi))
        for y in range(crest, 16):
            alpha = max(0, 150 - (y - crest) * 14)
            image.putpixel((x, y), hexc("#8fd8ff", alpha))
        image.putpixel((x, crest), hexc("#f0fbff"))
        if crest + 1 < 16:
            image.putpixel((x, crest + 1), hexc("#c8eeff"))
    save(image, "Water_Surface")


def seaweed(frame):
    image = Image.new("RGBA", (16, 32), CLEAR)
    light, dark = hexc("#4cc35a"), hexc("#1e6b2a")
    for strand, (base_x, phase) in enumerate(((5, 0.0), (10, 1.7))):
        for y in range(4 + strand * 3, 32):
            sway = math.sin((y / 32) * math.pi * 2 + phase + frame * math.pi) * (32 - y) / 14
            x = int(round(base_x + sway))
            image.putpixel((x, y), light)
            image.putpixel((x + 1, y), dark)
    save(image, f"Water_Seaweed{frame + 1}")


def fish(frame):
    """Slim striped fish facing left, two tail positions."""
    image = Image.new("RGBA", (16, 16), CLEAR)
    body, belly, stripe = hexc("#f0c040"), hexc("#fff0a0"), hexc("#2a9d8f")
    for y in range(16):
        for x in range(16):
            inside = ((x - 7) / 5.5) ** 2 + ((y - 8) / 3.4) ** 2 <= 1
            if inside:
                color = belly if y >= 9 else body
                if x in (7, 10) and y <= 9:
                    color = stripe
                image.putpixel((x, y), color)
    spread = 3 if frame == 0 else 2
    for x in range(12, 16):
        reach = x - 11
        for y in range(8 - min(reach, spread), 8 + min(reach, spread) + 1):
            image.putpixel((x, y), stripe)
    fin_x = (7, 8, 9) if frame == 0 else (8, 9, 10)
    for x in fin_x:
        image.putpixel((x, 4), stripe)
    image.putpixel((3, 7), hexc("#ffffff"))
    image.putpixel((3, 8), hexc("#101020"))
    outline(image)
    save(image, f"Fish_Swim{frame + 1}")


def jellyfish(extended):
    """Violet jellyfish, contracted (sinking) or extended (pulse)."""
    image = Image.new("RGBA", (16, 24), CLEAR)
    dome, light, tentacle = hexc("#c070e0"), hexc("#ecb8ff"), hexc("#9048c0")
    rx, ry, top = (5.5, 6.0, 2) if extended else (6.5, 5.0, 6)
    cy = top + ry
    for y in range(24):
        for x in range(16):
            if y <= cy and ((x - 7.5) / rx) ** 2 + ((y - cy) / ry) ** 2 <= 1:
                image.putpixel((x, y), dome)
    for (x, y) in ((5, top + 2), (6, top + 2), (5, top + 3)):
        image.putpixel((x, y), light)
    eye_y = int(cy) - 2
    image.putpixel((5, eye_y), hexc("#101020"))
    image.putpixel((10, eye_y), hexc("#101020"))
    length = 13 if extended else 6
    for i, base_x in enumerate((4, 7, 9, 12) if extended else (3, 6, 9, 12)):
        for step in range(length):
            y = int(cy) + 1 + step
            wobble = int(round(math.sin(step * 0.9 + i) * (1 if extended else 0.6)))
            if y < 24:
                image.putpixel((min(15, max(0, base_x + wobble)), y), tentacle)
    outline(image)
    save(image, "Jellyfish_Extended" if extended else "Jellyfish_Contracted")


def bubble():
    image = Image.new("RGBA", (4, 4), CLEAR)
    ring = hexc("#d8f6ff")
    for (x, y) in ((1, 0), (2, 0), (0, 1), (3, 1), (0, 2), (3, 2), (1, 3), (2, 3)):
        image.putpixel((x, y), ring)
    image.putpixel((1, 1), hexc("#ffffff"))
    save(image, "Water_Bubble")


if __name__ == "__main__":
    block("Water_Seafloor", "#3f8f7f", "#6cc3a8", "#1f5a50", "#2d7366", seed=7)
    block("Water_Rock", "#6a5a8a", "#9888b8", "#3a2f55", "#55477a", seed=11)
    coral()
    surface()
    seaweed(0)
    seaweed(1)
    fish(0)
    fish(1)
    jellyfish(False)
    jellyfish(True)
    bubble()
