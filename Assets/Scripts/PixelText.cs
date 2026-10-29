using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Draws uppercase text with a built-in 5x7 pixel font into a point filtered
// texture, keeping the NES look without depending on external font assets.
[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
public class PixelText : MonoBehaviour
{
    private const int GlyphWidth = 5;
    private const int GlyphHeight = 7;

    public string text = "TEXT";
    public Color color = Color.white;
    public bool shadow = true;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.6f);
    public float pixelSize = 8f;

    private RawImage image;
    private Texture2D texture;
    private bool dirty;

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDisable()
    {
        if (image != null) {
            image.texture = null;
        }

        if (texture != null)
        {
            if (Application.isPlaying) {
                Destroy(texture);
            } else {
                DestroyImmediate(texture);
            }

            texture = null;
        }
    }

    private void OnValidate()
    {
        dirty = true;
    }

    private void Update()
    {
        if (dirty) {
            Refresh();
        }
    }

    public void SetText(string value)
    {
        text = value;
        Refresh();
    }

    public void Refresh()
    {
        dirty = false;

        if (image == null) {
            image = GetComponent<RawImage>();
        }

        string value = string.IsNullOrEmpty(text) ? " " : text.ToUpperInvariant();
        int offset = shadow ? 1 : 0;
        int width = value.Length * (GlyphWidth + 1) - 1 + offset;
        int height = GlyphHeight + offset;

        if (texture == null || texture.width != width || texture.height != height)
        {
            OnDisable();

            texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "PixelText";
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.hideFlags = HideFlags.HideAndDontSave;
        }

        Color32[] pixels = new Color32[width * height];

        if (shadow) {
            Draw(value, pixels, width, height, 1, -1, shadowColor);
        }

        Draw(value, pixels, width, height, 0, 0, color);

        texture.SetPixels32(pixels);
        texture.Apply();

        image.texture = texture;
        image.uvRect = new Rect(0f, 0f, 1f, 1f);
        image.rectTransform.sizeDelta = new Vector2(width, height) * pixelSize;
    }

    private static void Draw(string value, Color32[] pixels, int width, int height, int dx, int dy, Color32 color)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (!Glyphs.TryGetValue(value[i], out string[] glyph)) {
                continue;
            }

            for (int row = 0; row < GlyphHeight; row++)
            {
                for (int col = 0; col < GlyphWidth; col++)
                {
                    if (glyph[row][col] != '#') {
                        continue;
                    }

                    int x = i * (GlyphWidth + 1) + col + dx;
                    int y = height - 1 - row + dy;
                    pixels[y * width + x] = color;
                }
            }
        }
    }

    private static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
    {
        ['A'] = new[] { ".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" },
        ['B'] = new[] { "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####." },
        ['C'] = new[] { ".###.", "#...#", "#....", "#....", "#....", "#...#", ".###." },
        ['D'] = new[] { "####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####." },
        ['E'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#####" },
        ['F'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#...." },
        ['G'] = new[] { ".###.", "#...#", "#....", "#.###", "#...#", "#...#", ".####" },
        ['H'] = new[] { "#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" },
        ['I'] = new[] { ".###.", "..#..", "..#..", "..#..", "..#..", "..#..", ".###." },
        ['J'] = new[] { "..###", "...#.", "...#.", "...#.", "#..#.", "#..#.", ".##.." },
        ['K'] = new[] { "#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#" },
        ['L'] = new[] { "#....", "#....", "#....", "#....", "#....", "#....", "#####" },
        ['M'] = new[] { "#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#" },
        ['N'] = new[] { "#...#", "#...#", "##..#", "#.#.#", "#..##", "#...#", "#...#" },
        ['O'] = new[] { ".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
        ['P'] = new[] { "####.", "#...#", "#...#", "####.", "#....", "#....", "#...." },
        ['Q'] = new[] { ".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#" },
        ['R'] = new[] { "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#" },
        ['S'] = new[] { ".####", "#....", "#....", ".###.", "....#", "....#", "####." },
        ['T'] = new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.." },
        ['U'] = new[] { "#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
        ['V'] = new[] { "#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#.." },
        ['W'] = new[] { "#...#", "#...#", "#...#", "#.#.#", "#.#.#", "#.#.#", ".#.#." },
        ['X'] = new[] { "#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#" },
        ['Y'] = new[] { "#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.." },
        ['Z'] = new[] { "#####", "....#", "...#.", "..#..", ".#...", "#....", "#####" },
        ['0'] = new[] { ".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###." },
        ['1'] = new[] { "..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###." },
        ['2'] = new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####" },
        ['3'] = new[] { "#####", "...#.", "..#..", "...#.", "....#", "#...#", ".###." },
        ['4'] = new[] { "...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#." },
        ['5'] = new[] { "#####", "#....", "####.", "....#", "....#", "#...#", ".###." },
        ['6'] = new[] { "..##.", ".#...", "#....", "####.", "#...#", "#...#", ".###." },
        ['7'] = new[] { "#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#..." },
        ['8'] = new[] { ".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###." },
        ['9'] = new[] { ".###.", "#...#", "#...#", ".####", "....#", "...#.", ".##.." },
        ['-'] = new[] { ".....", ".....", ".....", "#####", ".....", ".....", "....." },
        ['×'] = new[] { ".....", ".....", "#...#", ".#.#.", "..#..", ".#.#.", "#...#" },
        ['!'] = new[] { "..#..", "..#..", "..#..", "..#..", "..#..", ".....", "..#.." },
    };

}
