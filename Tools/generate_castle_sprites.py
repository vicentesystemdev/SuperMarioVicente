"""Generates the original pixel art sprites used by the castle level (8-4)
and the victory screen.

All sprites are new, drawn procedurally for this project (no external or
Nintendo assets). The final boss "Magmor" is an original character that
plays the role of Bowser. Output: Assets/Sprites/Castle/*.png, 16 px/unit.

Usage (from the repository root):
    python Tools/generate_castle_sprites.py
"""

import math
import os
import random

from PIL import Image

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Sprites", "Castle")

CLEAR = (0, 0, 0, 0)
OUTLINE = (20, 14, 24, 255)


def hexc(value, alpha=255):
    value = value.lstrip("#")
    return tuple(int(value[i:i + 2], 16) for i in (0, 2, 4)) + (alpha,)


def save(image, name):
    os.makedirs(OUT, exist_ok=True)
    image.save(os.path.join(OUT, name + ".png"))
    print("written", name)


def put(image, x, y, color):
    if 0 <= x < image.width and 0 <= y < image.height:
        image.putpixel((x, y), color)


def outline(image, color=OUTLINE):
    source = image.copy()
    for y in range(image.height):
        for x in range(image.width):
            if source.getpixel((x, y))[3] != 0:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < image.width and 0 <= ny < image.height and source.getpixel((nx, ny))[3] == 255:
                    image.putpixel((x, y), color)
                    break


# ---------------------------------------------------------------- tiles

def stone():
    image = Image.new("RGBA", (16, 16), hexc("#7c7c8c"))
    rng = random.Random(3)
    for i in range(16):
        put(image, i, 0, hexc("#a8a8b8"))
        put(image, 0, i, hexc("#a8a8b8"))
        put(image, i, 15, hexc("#4a4a58"))
        put(image, 15, i, hexc("#4a4a58"))
    for _ in range(12):
        put(image, rng.randint(2, 13), rng.randint(2, 13), hexc("#66667a"))
    save(image, "Castle_Stone")


def bricks(name, base, mortar, light):
    image = Image.new("RGBA", (16, 16), hexc(base))
    for y in (0, 8):
        for x in range(16):
            put(image, x, y, hexc(mortar))
            put(image, x, y + 1, hexc(light))
    for y in range(16):
        x = 7 if y < 8 else 15
        put(image, x, y, hexc(mortar))
    save(image, name)


def lava(frame):
    image = Image.new("RGBA", (16, 16), hexc("#e04a00"))
    for x in range(16):
        crest = 2 + round(math.sin((x / 16) * 2 * math.pi + frame * math.pi))
        for y in range(crest):
            put(image, x, y, CLEAR)
        put(image, x, crest, hexc("#ffd040"))
        put(image, x, crest + 1, hexc("#ff9a10"))
        for y in range(11, 16):
            put(image, x, y, hexc("#b02800"))
    for (x, y) in (((3 + frame * 6) % 16, 7), ((11 + frame * 5) % 16, 10), ((7 + frame * 3) % 16, 13)):
        put(image, x, y, hexc("#ffd040"))
        put(image, x + 1, y, hexc("#ff9a10"))
    save(image, f"Lava{frame + 1}")


def crumble():
    image = Image.new("RGBA", (16, 16), hexc("#8a7660"))
    for i in range(16):
        put(image, i, 0, hexc("#b09a80"))
        put(image, i, 15, hexc("#4f4234"))
        put(image, 0, i, hexc("#b09a80"))
        put(image, 15, i, hexc("#4f4234"))
    for (x, y) in ((4, 2), (5, 3), (5, 4), (6, 5), (6, 6), (10, 8), (11, 9), (11, 10), (12, 11), (3, 11), (4, 12)):
        put(image, x, y, hexc("#3a3026"))
    save(image, "Castle_Crumble")


def platform():
    image = Image.new("RGBA", (16, 16), CLEAR)
    for y in range(4, 12):
        for x in range(16):
            put(image, x, y, hexc("#5d6d7e"))
    for x in range(16):
        put(image, x, 4, hexc("#9fb0c0"))
        put(image, x, 11, hexc("#33404c"))
    for x in (3, 12):
        put(image, x, 7, hexc("#d8e0e8"))
        put(image, x, 8, hexc("#8090a0"))
    save(image, "Castle_Platform")


def spikes(down):
    image = Image.new("RGBA", (16, 16), CLEAR)
    for i in range(4):
        cx = 2 + i * 4
        for row in range(10):
            half = min(2, row // 3)
            for x in range(cx - half, cx + half + 1):
                color = hexc("#e0e0ec") if x <= cx else hexc("#8c8ca0")
                put(image, x, 4 + row, color)
    for x in range(16):
        for y in (14, 15):
            put(image, x, y, hexc("#3a3a48"))
    outline(image)
    if down:
        image = image.transpose(Image.FLIP_TOP_BOTTOM)
    save(image, "Spikes_Down" if down else "Spikes")


def torch(frame):
    image = Image.new("RGBA", (16, 32), CLEAR)
    for y in range(18, 30):
        put(image, 7, y, hexc("#6a4420"))
        put(image, 8, y, hexc("#4a2c10"))
    for x in range(5, 11):
        put(image, x, 17, hexc("#8a8a9a"))
        put(image, x, 18, hexc("#5a5a6a"))
    sway = 1 if frame else -1
    for y in range(6, 17):
        width = max(0, int((y - 5) * 0.45))
        cx = 8 + (sway if y < 11 else 0)
        for x in range(cx - width, cx + width + 1):
            color = hexc("#ffe070") if abs(x - cx) < width * 0.5 else hexc("#ff7a10")
            put(image, x, y, color)
    put(image, 8 + sway, 4, hexc("#ffb030"))
    put(image, 8 + sway, 5, hexc("#ffb030"))
    save(image, f"Torch{frame + 1}")


def fireball(frame):
    image = Image.new("RGBA", (8, 8), CLEAR)
    for y in range(8):
        for x in range(8):
            d = math.hypot(x - 3.5, y - 3.5)
            if d <= 3.4:
                put(image, x, y, hexc("#ffe070") if d < 1.6 else hexc("#ff7a10"))
    for (x, y) in (((0, 0), (7, 7)) if frame == 0 else ((7, 0), (0, 7))):
        put(image, x, y, hexc("#ffb030"))
    save(image, f"Fireball{frame + 1}")


def flame(frame):
    image = Image.new("RGBA", (16, 48), CLEAR)
    for y in range(48):
        progress = y / 47
        width = 2 + progress * 5 + math.sin(y * 0.6 + frame * 2) * 1.2
        cx = 7.5 + math.sin(y * 0.3 + frame * math.pi) * (1 - progress) * 1.5
        for x in range(16):
            dist = abs(x - cx)
            if dist <= width:
                color = hexc("#fff0a0") if dist < width * 0.35 else hexc("#ffb030") if dist < width * 0.7 else hexc("#ff5a10")
                put(image, x, y, color)
    save(image, f"Flame{frame + 1}")


def cannon():
    image = Image.new("RGBA", (16, 16), hexc("#3c3c4c"))
    for i in range(16):
        put(image, i, 0, hexc("#6c6c80"))
        put(image, i, 15, hexc("#1c1c28"))
    for y in range(5, 11):
        for x in range(0, 6):
            put(image, x, y, hexc("#101018"))
    for y in range(4, 12):
        put(image, 6, y, hexc("#c8a040"))
    put(image, 11, 7, hexc("#ff5030"))
    put(image, 12, 7, hexc("#ff5030"))
    save(image, "Cannon")


def door():
    image = Image.new("RGBA", (32, 48), CLEAR)
    for y in range(48):
        for x in range(32):
            arch = y >= 16 or math.hypot(x - 15.5, y - 16) <= 15.5
            if arch:
                put(image, x, y, hexc("#5a3418"))
    for x in range(2, 30, 6):
        for y in range(4, 48):
            if image.getpixel((x, y))[3]:
                put(image, x, y, hexc("#3a200c"))
    for y in (20, 36):
        for x in range(32):
            if image.getpixel((x, y))[3]:
                put(image, x, y, hexc("#8a8a9a"))
    put(image, 22, 28, hexc("#e0c040"))
    outline(image)
    save(image, "Door")


def gate():
    image = Image.new("RGBA", (16, 16), CLEAR)
    for x in (2, 7, 12):
        for y in range(16):
            put(image, x, y, hexc("#9a9aac"))
            put(image, x + 1, y, hexc("#5a5a6c"))
    for x in range(16):
        put(image, x, 7, hexc("#6c6c80"))
        put(image, x, 8, hexc("#3c3c4c"))
    save(image, "Gate")


def checkpoint(active):
    image = Image.new("RGBA", (16, 32), CLEAR)
    for y in range(2, 32):
        put(image, 3, y, hexc("#d0d0dc"))
        put(image, 4, y, hexc("#8a8a9a"))
    cloth = hexc("#40c060") if active else hexc("#7a7a88")
    shade = hexc("#20803a") if active else hexc("#55556a")
    for y in range(3, 13):
        for x in range(5, 15 - abs(y - 8) // 2):
            put(image, x, y, cloth if (x + y) % 5 else shade)
    if active:
        for (x, y) in ((9, 7), (10, 8), (9, 8), (10, 7)):
            put(image, x, y, hexc("#ffe070"))
    put(image, 3, 1, hexc("#e0c040"))
    put(image, 4, 1, hexc("#e0c040"))
    outline(image)
    save(image, "Checkpoint_On" if active else "Checkpoint_Off")


def trophy():
    """Golden cup crowned with a star: the castle treasure."""
    image = Image.new("RGBA", (16, 16), CLEAR)
    gold, light, dark = hexc("#f0c030"), hexc("#fff0a0"), hexc("#b07810")
    for y in range(5, 11):
        half = 5 - (y - 5) // 2
        for x in range(8 - half, 8 + half):
            put(image, x, y, light if x < 7 else gold)
    for y in range(11, 13):
        put(image, 7, y, dark)
        put(image, 8, y, gold)
    for x in range(4, 12):
        put(image, x, 13, gold)
        put(image, x, 14, dark)
    for (x, y) in ((2, 6), (2, 7), (3, 8), (13, 6), (13, 7), (12, 8)):
        put(image, x, y, gold)
    for (x, y) in ((7, 0), (8, 0), (6, 1), (7, 1), (8, 1), (9, 1), (7, 2), (8, 2), (6, 3), (9, 3)):
        put(image, x, y, hexc("#ffe070"))
    outline(image)
    save(image, "Trophy")


# ---------------------------------------------------------------- boss

def boss(pose):
    """Magmor, the lava king: original horned rock golem-dragon, 32x32,
    facing left. Poses: idle1, idle2, attack (mouth open), defeated."""
    image = Image.new("RGBA", (32, 32), CLEAR)
    rock = hexc("#4a3c48") if pose != "defeated" else hexc("#5a5a60")
    rock_light = hexc("#6e5a6a") if pose != "defeated" else hexc("#7a7a80")
    belly = hexc("#c8743a") if pose != "defeated" else hexc("#8a7a6a")
    crack = hexc("#ff7a10") if pose != "defeated" else hexc("#6a5040")
    glow = hexc("#ffe070")
    dy = 1 if pose == "idle2" else 0

    # legs
    for x0 in (11, 21):
        for y in range(26, 31):
            for x in range(x0, x0 + 5):
                put(image, x, y, rock)
        for x in range(x0 - 1, x0 + 6):
            put(image, x, 31, rock_light)

    # body
    for y in range(32):
        for x in range(32):
            if ((x - 18) / 11) ** 2 + ((y - 19 - dy) / 9) ** 2 <= 1:
                put(image, x, y, rock)
            if ((x - 15) / 6) ** 2 + ((y - 21 - dy) / 6) ** 2 <= 1:
                put(image, x, y, belly)

    # back spikes
    for i, sx in enumerate((20, 24, 28)):
        for row in range(4):
            for x in range(sx - (3 - row) // 2, sx + (3 - row) // 2 + 1):
                put(image, x, 9 + row - (1 if i == 1 else 0) + dy, rock_light)

    # lava cracks on the body
    for (x, y) in ((22, 15), (23, 16), (23, 17), (24, 18), (26, 20), (27, 21), (25, 24), (24, 25)):
        put(image, x, y + dy, crack)

    # head
    for y in range(32):
        for x in range(32):
            if math.hypot(x - 9, y - 11 - dy) <= 6.5:
                put(image, x, y, rock)
    # horns
    for row in range(6):
        put(image, 6 - row // 2, 4 - row + 3 + dy, hexc("#e8e0c8"))
        put(image, 13 + row // 2, 4 - row + 3 + dy, hexc("#e8e0c8"))
        put(image, 7 - row // 2, 4 - row + 3 + dy, hexc("#b8b098"))
        put(image, 12 + row // 2, 4 - row + 3 + dy, hexc("#b8b098"))

    # eyes
    if pose == "defeated":
        for (x, y) in ((5, 8), (7, 10), (7, 8), (5, 10), (10, 8), (12, 10), (12, 8), (10, 10)):
            put(image, x, y, hexc("#101018"))
    else:
        for x in (5, 6, 10, 11):
            put(image, x, 9 + dy, glow)
        put(image, 5, 10 + dy, hexc("#ff3010"))
        put(image, 10, 10 + dy, hexc("#ff3010"))

    # jaw
    if pose in ("attack", "defeated"):
        for y in range(13, 18):
            for x in range(2, 11):
                if y - 13 <= (x - 2) * 0.8:
                    put(image, x, y + dy, hexc("#2a0a08"))
        for x in range(3, 10):
            put(image, x, 15 + dy, crack)
        for x in (3, 6, 9):
            put(image, x, 13 + dy, hexc("#f0f0e0"))
    else:
        for x in range(3, 12):
            put(image, x, 14 + dy, hexc("#2a1a20"))
        for x in (4, 7, 10):
            put(image, x, 13 + dy, hexc("#f0f0e0"))

    # arm
    for y in range(18, 24):
        for x in range(7, 11):
            put(image, x, y + dy, rock_light)
    for x in range(6, 11):
        put(image, x, 24 + dy, hexc("#e8e0c8"))

    outline(image)
    save(image, "Boss_" + pose.capitalize())


def boss_fireball(frame):
    image = Image.new("RGBA", (16, 8), CLEAR)
    for y in range(8):
        for x in range(16):
            # head on the left, tail on the right
            radius = 3.6 - x * 0.18 + (0.6 if (x + frame) % 3 == 0 else 0)
            if abs(y - 3.5) <= radius:
                dist = abs(y - 3.5) / max(radius, 0.1)
                put(image, x, y, hexc("#fff0a0") if dist < 0.35 and x < 8 else hexc("#ffb030") if dist < 0.7 else hexc("#ff5010"))
    save(image, f"Boss_Fireball{frame + 1}")


def shockwave(frame):
    image = Image.new("RGBA", (16, 16), CLEAR)
    for x in range(16):
        height = 6 + int(4 * abs(math.sin(x * 0.7 + frame * 1.5)))
        for y in range(16 - height, 16):
            dist = (y - (16 - height)) / height
            put(image, x, y, hexc("#ffe070") if dist > 0.6 else hexc("#ff9a10") if dist > 0.3 else hexc("#ff4a10"))
    save(image, f"Shockwave{frame + 1}")


if __name__ == "__main__":
    stone()
    bricks("Castle_Brick", "#4b3f4f", "#241c2a", "#6a5a70")
    bricks("Castle_BrickBack", "#2e2733", "#1a1520", "#3a3240")
    lava(0)
    lava(1)
    crumble()
    platform()
    spikes(False)
    spikes(True)
    torch(0)
    torch(1)
    fireball(0)
    fireball(1)
    flame(0)
    flame(1)
    cannon()
    door()
    gate()
    checkpoint(False)
    checkpoint(True)
    trophy()
    for pose in ("idle1", "idle2", "attack", "defeated"):
        boss(pose)
    boss_fireball(0)
    boss_fireball(1)
    shockwave(0)
    shockwave(1)
