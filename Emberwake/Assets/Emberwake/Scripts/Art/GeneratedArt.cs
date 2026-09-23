using System;
using System.Collections.Generic;
using UnityEngine;

namespace Emberwake
{
    /// <summary>Mass procedural art pack — icons, portraits, enemies, buttons, app icon.</summary>
    public static class GeneratedArt
    {
        static readonly Dictionary<string, Sprite> Cache = new();

        public static Sprite AppIcon512() => PaintedArt.Load("app_icon") ?? AppIconProcedural();

        static Sprite AppIconProcedural() => Get("app", () => Paint(512, (px, s) =>
        {
            float cx = s * 0.5f, cy = s * 0.52f;
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = (x - cx) / s, dy = (y - cy) / s;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                Color c = new Color(0.08f, 0.06f, 0.1f, 1f);
                // warm radial glow
                float glow = Mathf.Exp(-d * 4.2f);
                c = Color.Lerp(c, new Color(0.55f, 0.22f, 0.05f), glow * 0.85f);
                // wick flame oval
                float fx = (x - cx) / (s * 0.12f);
                float fy = (y - cy - s * 0.02f) / (s * 0.22f);
                float flame = fx * fx + fy * fy;
                if (flame < 1f)
                {
                    float t = 1f - flame;
                    c = Color.Lerp(new Color(0.95f, 0.35f, 0.05f), new Color(1f, 0.92f, 0.45f), t);
                    c.a = 1f;
                }
                // lantern ring
                float ring = Mathf.Abs(d - 0.28f);
                if (ring < 0.035f) c = Color.Lerp(c, new Color(1f, 0.85f, 0.4f), 1f - ring / 0.035f);
                px[y * s + x] = c;
            }
        }, FilterMode.Bilinear));

        public static Sprite MenuBg() => Get("menubg", () => Paint(64, (px, s) =>
        {
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
                px[y * s + x] = Color.Lerp(new Color(0.06f, 0.07f, 0.1f), new Color(0.12f, 0.08f, 0.06f), n * 0.5f);
            }
        }, FilterMode.Bilinear));

        public static Sprite TitleBanner() => Get("banner", () => Rounded(192, 48, 14, new Color(0.12f, 0.1f, 0.08f, 0.95f), new Color(1f, 0.78f, 0.3f, 0.9f)));
        public static Sprite SoftButton() => Get("btn", () => Rounded(96, 40, 12, new Color(0.85f, 0.4f, 0.12f, 1f), new Color(1f, 0.88f, 0.45f, 1f)));
        public static Sprite SoftButtonDark() => Get("btnd", () => Rounded(96, 40, 12, new Color(0.16f, 0.17f, 0.22f, 1f), new Color(0.55f, 0.52f, 0.45f, 0.9f)));
        public static Sprite IconFrame() => Get("iframe", () => Rounded(64, 64, 10, new Color(0.1f, 0.1f, 0.12f, 0.95f), new Color(1f, 0.8f, 0.35f, 0.75f)));

        public static Sprite IconHeart() => Glyph("i_heart", (nx, ny) =>
        {
            float a = nx * nx + (ny - Mathf.Sqrt(Mathf.Abs(nx))) * (ny - Mathf.Sqrt(Mathf.Abs(nx))) - 0.5f;
            return a < 0 ? new Color(0.95f, 0.25f, 0.3f) : Color.clear;
        });
        public static Sprite IconSword() => Glyph("i_sword", (nx, ny) =>
        {
            bool blade = Mathf.Abs(nx) < 0.12f && ny > -0.55f && ny < 0.65f;
            bool guard = Mathf.Abs(ny + 0.15f) < 0.08f && Mathf.Abs(nx) < 0.35f;
            bool hilt = Mathf.Abs(nx) < 0.08f && ny < -0.15f && ny > -0.7f;
            return (blade || guard || hilt) ? new Color(0.95f, 0.9f, 0.55f) : Color.clear;
        });
        public static Sprite IconShield() => Glyph("i_shield", (nx, ny) =>
        {
            bool top = Mathf.Abs(nx) < 0.55f && ny < 0.45f && ny > -0.2f - (0.55f - Mathf.Abs(nx)) * 0.9f;
            return top ? new Color(0.35f, 0.55f, 0.95f) : Color.clear;
        });
        public static Sprite IconQuest() => Glyph("i_quest", (nx, ny) =>
        {
            float d = Vector2.Distance(new Vector2(nx, ny), Vector2.zero);
            bool ring = d > 0.35f && d < 0.55f;
            bool mark = Mathf.Abs(nx) < 0.1f && ny > -0.1f && ny < 0.35f;
            bool dot = (nx * nx + (ny + 0.35f) * (ny + 0.35f)) < 0.02f;
            return (ring || mark || dot) ? new Color(1f, 0.85f, 0.3f) : Color.clear;
        });
        public static Sprite IconBook() => Glyph("i_book", (nx, ny) =>
        {
            // Closed tome: red cover, cream page fore-edge on the right, dark spine
            // on the left, and a gold title band — reads clearly as a book.
            bool cover = Mathf.Abs(nx) < 0.44f && Mathf.Abs(ny) < 0.54f;
            if (!cover) return Color.clear;
            if (nx > 0.30f) return new Color(0.93f, 0.89f, 0.76f);              // pages (fore-edge)
            if (nx > 0.24f) return new Color(0.55f, 0.4f, 0.22f);              // edge shadow
            if (nx < -0.34f) return new Color(0.36f, 0.1f, 0.09f);            // spine
            if (Mathf.Abs(ny) < 0.07f && nx < 0.2f) return new Color(0.96f, 0.8f, 0.34f); // title band
            return new Color(0.62f, 0.2f, 0.17f);                             // cover
        });
        public static Sprite IconSave() => Glyph("i_save", (nx, ny) =>
        {
            bool body = Mathf.Abs(nx) < 0.45f && Mathf.Abs(ny) < 0.45f;
            bool slot = Mathf.Abs(nx) < 0.2f && ny > 0.05f && ny < 0.35f;
            return body ? (slot ? new Color(0.2f, 0.55f, 0.95f) : new Color(0.75f, 0.75f, 0.8f)) : Color.clear;
        });
        public static Sprite IconStar() => Glyph("i_star", (nx, ny) =>
        {
            float ang = Mathf.Atan2(ny, nx);
            float r = Mathf.Sqrt(nx * nx + ny * ny);
            float star = 0.35f + 0.2f * Mathf.Cos(ang * 5f);
            return r < star ? new Color(1f, 0.88f, 0.25f) : Color.clear;
        });
        public static Sprite IconSkull() => Glyph("i_skull", (nx, ny) =>
        {
            bool head = (nx * nx) / 0.35f + ((ny - 0.05f) * (ny - 0.05f)) / 0.4f < 1f;
            bool eyeL = ((nx + 0.15f) * (nx + 0.15f) + (ny - 0.1f) * (ny - 0.1f)) < 0.025f;
            bool eyeR = ((nx - 0.15f) * (nx - 0.15f) + (ny - 0.1f) * (ny - 0.1f)) < 0.025f;
            if (eyeL || eyeR) return new Color(0.1f, 0.1f, 0.12f);
            return head ? new Color(0.92f, 0.9f, 0.85f) : Color.clear;
        });
        public static Sprite IconMap() => Glyph("i_map", (nx, ny) =>
        {
            bool paper = Mathf.Abs(nx) < 0.5f && Mathf.Abs(ny) < 0.4f;
            bool path = paper && Mathf.Abs(ny - nx * 0.3f) < 0.06f;
            return paper ? (path ? new Color(0.75f, 0.35f, 0.15f) : new Color(0.85f, 0.78f, 0.55f)) : Color.clear;
        });
        public static Sprite IconCoin() => Glyph("i_coin", (nx, ny) =>
        {
            float d = Mathf.Sqrt(nx * nx + ny * ny);
            if (d > 0.5f) return Color.clear;
            if (d > 0.38f) return new Color(0.75f, 0.55f, 0.1f);
            return new Color(1f, 0.85f, 0.25f);
        });
        public static Sprite IconWick() => Glyph("i_wick", (nx, ny) =>
        {
            float fx = nx / 0.22f, fy = (ny - 0.05f) / 0.45f;
            if (fx * fx + fy * fy < 1f) return Color.Lerp(new Color(1f, 0.4f, 0.05f), new Color(1f, 0.95f, 0.5f), 1f - (fx * fx + fy * fy));
            if (Mathf.Abs(nx) < 0.06f && ny < -0.15f && ny > -0.65f) return new Color(0.35f, 0.25f, 0.15f);
            return Color.clear;
        });
        public static Sprite IconBag() => Glyph("i_bag", (nx, ny) =>
        {
            bool bag = Mathf.Abs(nx) < 0.4f && ny > -0.45f && ny < 0.25f;
            bool strap = Mathf.Abs(nx) < 0.25f && ny > 0.2f && ny < 0.45f && Mathf.Abs(nx) > 0.12f;
            return (bag || strap) ? new Color(0.55f, 0.35f, 0.18f) : Color.clear;
        });
        public static Sprite IconMenu() => Glyph("i_menu", (nx, ny) =>
        {
            bool b1 = Mathf.Abs(ny - 0.25f) < 0.07f && Mathf.Abs(nx) < 0.4f;
            bool b2 = Mathf.Abs(ny) < 0.07f && Mathf.Abs(nx) < 0.4f;
            bool b3 = Mathf.Abs(ny + 0.25f) < 0.07f && Mathf.Abs(nx) < 0.4f;
            return (b1 || b2 || b3) ? new Color(1f, 0.92f, 0.75f) : Color.clear;
        });

        public static Sprite PortraitKael() => PaintedArt.Load("portrait_kael") ?? PortraitStyled("p_kael", 0);
        public static Sprite PortraitMara() => PaintedArt.Load("portrait_mara") ?? PortraitStyled("p_mara", 1);
        public static Sprite PortraitLira() => PaintedArt.Load("portrait_lira") ?? PortraitStyled("p_lira", 2);
        public static Sprite PortraitBram() => PaintedArt.Load("portrait_bram") ?? PortraitStyled("p_bram", 3);
        public static Sprite PortraitSera() => PaintedArt.Load("portrait_sera") ?? PortraitStyled("p_sera", 4);
        public static Sprite PortraitWick() => PaintedArt.Load("portrait_wick") ?? PortraitStyled("p_wick", 5);

        public static Sprite PortraitFor(string speaker) => speaker switch
        {
            "Mara" => PortraitMara(),
            "Lira" => PortraitLira(),
            "Bram" => PortraitBram(),
            "Sera" => PortraitSera(),
            "Wick" => PortraitWick(),
            _ => PortraitKael()
        };

        public static Sprite EnemyArt(string id) => id switch
        {
            "slime" => EnemyBlob("e_slime", new Color(0.25f, 0.85f, 0.35f)),
            "ash_wisp" => EnemyWisp("e_wisp", new Color(0.85f, 0.55f, 0.2f)),
            "root_crawler" => EnemyCrawler("e_root", new Color(0.45f, 0.55f, 0.25f)),
            "ember_moth" => EnemyMoth("e_moth", new Color(1f, 0.7f, 0.25f)),
            "hollow_knight" => EnemyKnight("e_hk", new Color(0.35f, 0.4f, 0.55f)),
            "barkling" => EnemyBoss("e_bark", new Color(0.4f, 0.7f, 0.3f)),
            _ => EnemyBlob("e_unk", new Color(0.6f, 0.6f, 0.6f))
        };

        public static Sprite VillageNight() => Get("village_night", () => Paint(160, (px, s) =>
        {
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float t = y / (float)s;
                Color sky = Color.Lerp(new Color(0.05f, 0.06f, 0.12f), new Color(0.18f, 0.1f, 0.08f), t);
                if (y < s * 0.28f)
                    sky = Color.Lerp(new Color(0.08f, 0.12f, 0.07f), new Color(0.16f, 0.12f, 0.07f), x / (float)s);
                // path
                if (y < s * 0.28f && Mathf.Abs(x - s * 0.5f) < 10f + (s * 0.28f - y) * 0.15f)
                    sky = new Color(0.28f, 0.2f, 0.12f);
                px[y * s + x] = sky;
            }
            void House(int hx, int hy, int w, int h, Color wall)
            {
                for (int y = hy; y < hy + h && y < s; y++)
                for (int x = hx; x < hx + w && x < s; x++)
                {
                    if (x < 0 || y < 0) continue;
                    Color c = wall;
                    if (y > hy + h - 14) c = new Color(0.25f, 0.12f, 0.08f);
                    if (Mathf.Abs(x - (hx + w / 2)) < 3 && y > hy + 6 && y < hy + h / 2)
                        c = new Color(1f, 0.72f, 0.25f);
                    px[y * s + x] = c;
                }
            }
            House(12, 28, 36, 48, new Color(0.32f, 0.22f, 0.16f));
            House(108, 32, 40, 52, new Color(0.22f, 0.18f, 0.2f));
            // lantern glows
            void Glow(int gx, int gy)
            {
                for (int y = gy - 8; y <= gy + 8; y++)
                for (int x = gx - 8; x <= gx + 8; x++)
                {
                    if (x < 0 || y < 0 || x >= s || y >= s) continue;
                    float d = Mathf.Sqrt((x - gx) * (x - gx) + (y - gy) * (y - gy)) / 8f;
                    if (d < 1f)
                        px[y * s + x] = Color.Lerp(px[y * s + x], new Color(1f, 0.72f, 0.25f), (1f - d) * 0.85f);
                }
            }
            Glow(30, 48);
            Glow(128, 54);
            Glow(80, 22);
        }, FilterMode.Bilinear));

        /// <summary>0 Kael, 1 Mara, 2 Lira, 3 Bram, 4 Sera, 5 Wick — readable busts.</summary>
        static Sprite PortraitStyled(string key, int style) => Get(key, () => Paint(96, (px, s) =>
        {
            Color skin = style switch
            {
                1 => new Color(0.78f, 0.58f, 0.42f),
                2 => new Color(1f, 0.84f, 0.7f),
                3 => new Color(0.7f, 0.56f, 0.42f),
                4 => new Color(0.42f, 0.28f, 0.2f),
                5 => new Color(1f, 0.62f, 0.18f),
                _ => new Color(0.95f, 0.76f, 0.58f)
            };
            Color hair = style switch
            {
                1 => new Color(0.42f, 0.14f, 0.55f),
                2 => new Color(0.9f, 0.48f, 0.16f),
                3 => new Color(0.9f, 0.9f, 0.86f),
                4 => new Color(0.08f, 0.08f, 0.1f),
                5 => new Color(1f, 0.92f, 0.4f),
                _ => new Color(0.2f, 0.38f, 0.78f)
            };
            Color cloth = style switch
            {
                1 => new Color(0.45f, 0.18f, 0.5f),
                2 => new Color(0.85f, 0.4f, 0.22f),
                3 => new Color(0.32f, 0.24f, 0.14f),
                4 => new Color(0.1f, 0.1f, 0.12f),
                5 => new Color(0.55f, 0.16f, 0.05f),
                _ => new Color(0.16f, 0.32f, 0.62f)
            };
            Color iris = style switch
            {
                1 => new Color(0.35f, 0.72f, 0.28f),
                2 => new Color(0.2f, 0.4f, 0.9f),
                3 => new Color(0.3f, 0.2f, 0.12f),
                4 => new Color(0.9f, 0.2f, 0.18f),
                5 => new Color(1f, 1f, 0.75f),
                _ => new Color(0.12f, 0.35f, 0.85f)
            };
            float cx = s * 0.5f;
            float cy = s * 0.5f;
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float bx = (x - cx) / (s * 0.48f);
                float by = (y - cy) / (s * 0.48f);
                Color c = bx * bx + by * by < 1f
                    ? new Color(0.1f, 0.08f, 0.12f, 1f)
                    : Color.clear;

                // shoulders
                if (y < 30 && Mathf.Abs(x - cx) < 38f - (30 - y) * 0.35f)
                    c = cloth;
                // neck
                if (y >= 22 && y < 40 && Mathf.Abs(x - cx) < 9f)
                    c = skin;

                float fdx = (x - cx) / 22f;
                float fdy = (y - 50f) / 26f;
                bool face = fdx * fdx + fdy * fdy < 1f;
                if (face) c = skin;

                // hair stays above the brow so eyes stay readable
                bool aboveBrow = y > 58;
                if (style != 5 && face && aboveBrow) c = hair;
                if (style == 0 && face && y > 54 && Mathf.Abs(x - cx) < 20f) c = hair;
                if (style == 1 && Mathf.Abs(x - cx) > 14f && Mathf.Abs(x - cx) < 24f && y > 28 && y < 70) c = hair;
                if (style == 2 && face && y > 52 && y < 72) c = hair;
                if (style == 3 && face && y > 68) c = hair;
                if (style == 3 && face && y < 40 && y > 28 && Mathf.Abs(x - cx) < 16f) c = hair;
                if (style == 4 && face && y > 60) c = hair;
                if (style == 5 && face)
                    c = Color.Lerp(new Color(0.85f, 0.25f, 0.05f), new Color(1f, 0.95f, 0.55f), (y - 30f) / 40f);

                // eyes — drawn last among features so hair cannot cover them
                if (style != 5)
                {
                    DrawEye(ref c, x, y, cx - 9f, 54f, iris);
                    DrawEye(ref c, x, y, cx + 9f, 54f, iris);
                    if (Mathf.Abs(x - cx) < 2f && y > 44 && y < 50) c = new Color(skin.r * 0.75f, skin.g * 0.6f, skin.b * 0.55f);
                    if (Mathf.Abs(x - cx) < 6f && y > 38 && y < 41) c = new Color(0.55f, 0.22f, 0.22f);
                }
                else
                {
                    DrawEye(ref c, x, y, cx - 9f, 54f, iris);
                    DrawEye(ref c, x, y, cx + 9f, 54f, iris);
                }

                if (style == 0 && Mathf.Abs(x - (cx + 12f)) < 1.5f && y > 42 && y < 56)
                    c = new Color(0.72f, 0.22f, 0.18f);

                px[y * s + x] = c;
            }
        }, FilterMode.Point));

        static void DrawEye(ref Color c, int x, int y, float ex, float ey, Color iris)
        {
            float dx = x - ex, dy = y - ey;
            float d = dx * dx + dy * dy;
            if (d < 22f) c = Color.white;
            if (d < 8f) c = iris;
            if (d < 2.2f) c = new Color(0.05f, 0.05f, 0.08f);
        }

        static Sprite EnemyBlob(string key, Color col) => Get(key, () => Paint(64, (px, s) =>
        {
            float cx = s * 0.5f, cy = s * 0.42f;
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = (x - cx) / (s * 0.38f), dy = (y - cy) / (s * 0.32f);
                float d = dx * dx + dy * dy;
                Color c = Color.clear;
                if (d < 1f) c = Color.Lerp(col, Color.white, (1f - d) * 0.25f);
                if ((x - cx + 10) * (x - cx + 10) + (y - cy - 4) * (y - cy - 4) < 20) c = new Color(0.05f, 0.05f, 0.08f);
                if ((x - cx - 10) * (x - cx - 10) + (y - cy - 4) * (y - cy - 4) < 20) c = new Color(0.05f, 0.05f, 0.08f);
                px[y * s + x] = c;
            }
        }, FilterMode.Point));

        static Sprite EnemyWisp(string key, Color col) => Get(key, () => Paint(64, (px, s) =>
        {
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float nx = (x / (float)s) * 2f - 1f, ny = (y / (float)s) * 2f - 1f;
                float d = Mathf.Sqrt(nx * nx + (ny + 0.1f) * (ny + 0.1f));
                float a = Mathf.Clamp01(1.1f - d * 1.6f);
                px[y * s + x] = new Color(col.r, col.g, col.b, a * a);
            }
        }, FilterMode.Bilinear));

        static Sprite EnemyCrawler(string key, Color col) => Get(key, () => Paint(64, (px, s) =>
        {
            Color dark = col * 0.55f; dark.a = 1f;
            Color lite = Color.Lerp(col, Color.white, 0.28f);
            Color eye = new Color(1f, 0.72f, 0.22f);
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float nx = (x / (float)s) * 2f - 1f, ny = (y / (float)s) * 2f - 1f;
                Color c = Color.clear;
                bool body = Mathf.Abs(nx) < 0.6f && Mathf.Abs(ny) < 0.24f;
                bool leg = Mathf.Abs(nx) < 0.6f && Mathf.Abs(ny) >= 0.24f && Mathf.Abs(ny) < 0.44f
                           && Mathf.Sin(nx * 13f) > 0.25f;
                if (body)
                {
                    c = Color.Lerp(dark, lite, (ny + 0.24f) / 0.48f);          // top-lit ridge
                    if (Mathf.Sin(nx * 11f) > 0.55f) c = Color.Lerp(c, dark, 0.5f); // segment grooves
                }
                if (leg) c = dark;
                if (nx > 0.42f && Mathf.Abs(ny - 0.02f) < 0.09f
                    && Mathf.Abs(nx - 0.5f) < 0.07f) c = eye;                  // forward eye
                px[y * s + x] = c;
            }
        }, FilterMode.Point));

        static Sprite EnemyMoth(string key, Color col) => Get(key, () => Paint(64, (px, s) =>
        {
            Color dark = col * 0.6f; dark.a = 1f;
            Color lite = Color.Lerp(col, Color.white, 0.4f);
            Color eye = new Color(1f, 0.95f, 0.6f);
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float nx = (x / (float)s) * 2f - 1f, ny = (y / (float)s) * 2f - 1f;
                Color c = Color.clear;
                float wl = (nx + 0.35f) * (nx + 0.35f) / 0.2f + ny * ny / 0.35f;
                float wr = (nx - 0.35f) * (nx - 0.35f) / 0.2f + ny * ny / 0.35f;
                bool body = Mathf.Abs(nx) < 0.11f && Mathf.Abs(ny) < 0.42f;
                if (wl < 1f) c = Color.Lerp(lite, dark, wl);                   // soft wing shading
                if (wr < 1f) c = Color.Lerp(lite, dark, wr);
                if (wl < 0.28f || wr < 0.28f) c = Color.Lerp(c, Color.white, 0.35f); // wing eyespots
                if (body) c = Color.Lerp(dark, col, (ny + 0.42f) / 0.84f);     // fuzzy body
                if (ny > 0.24f && Mathf.Abs(Mathf.Abs(nx) - 0.05f) < 0.035f) c = eye; // tiny eyes
                px[y * s + x] = c;
            }
        }, FilterMode.Point));

        static Sprite EnemyKnight(string key, Color col) => Get(key, () => Paint(64, (px, s) =>
        {
            Color dark = col * 0.5f; dark.a = 1f;
            Color lite = Color.Lerp(col, Color.white, 0.32f);
            Color eye = new Color(0.7f, 0.95f, 1f);   // pale hollow glow
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float nx = (x / (float)s) * 2f - 1f, ny = (y / (float)s) * 2f - 1f;
                Color c = Color.clear;
                bool helm = Mathf.Abs(nx) < 0.32f && ny > 0.08f && ny < 0.55f;
                bool torso = Mathf.Abs(nx) < 0.30f && ny > -0.45f && ny < 0.12f;
                bool pauldron = Mathf.Abs(nx) > 0.26f && Mathf.Abs(nx) < 0.46f && ny > -0.05f && ny < 0.16f;
                bool blade = nx > 0.24f && nx < 0.62f && Mathf.Abs(ny + 0.06f) < 0.055f;
                if (helm || torso)
                {
                    c = Color.Lerp(dark, col, (ny + 0.5f));                    // top-lit metal
                    c = Color.Lerp(c, dark, Mathf.Abs(nx) * 0.7f);            // rounded shading
                }
                if (pauldron) c = dark;
                if (helm && ny > 0.46f) c = lite;                             // helm crest
                if (ny > 0.22f && ny < 0.34f && Mathf.Abs(Mathf.Abs(nx) - 0.12f) < 0.045f) c = eye; // visor eyes
                if (blade) c = Color.Lerp(lite, Color.white, 0.4f);          // steel blade
                px[y * s + x] = c;
            }
        }, FilterMode.Point));

        // Barkling: a hunched bark/root golem — jagged wooden trunk, root spikes,
        // two glowing amber eyes and a cracked ember maw. Reads clearly as a boss.
        static Sprite EnemyBoss(string key, Color col) => Get(key, () => Paint(96, (px, s) =>
        {
            float cx = s * 0.5f;
            Color bark = Color.Lerp(new Color(0.30f, 0.34f, 0.22f), col, 0.25f);
            Color barkDark = new Color(0.15f, 0.18f, 0.11f);
            Color barkLite = new Color(0.46f, 0.52f, 0.33f);
            Color eye = new Color(1f, 0.80f, 0.28f);
            Color ember = new Color(1f, 0.44f, 0.12f);
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float nx = (x - cx) / (s * 0.5f);
                float ny = (y / (float)s) * 2f - 1f;   // -1 bottom .. +1 top
                Color c = Color.clear;

                // Hunched trunk: domed shoulders on top, broad base
                float shoulder = 0.60f + 0.10f * Mathf.Cos(nx * 3.14159f);
                float halfWidth = Mathf.Lerp(0.80f, 0.52f, Mathf.InverseLerp(-0.8f, 0.72f, ny));
                bool inBody = ny < 0.72f && ny > -0.78f && Mathf.Abs(nx) < halfWidth
                              && (ny < 0.46f || Mathf.Abs(nx) < shoulder);
                if (inBody)
                {
                    float groove = Mathf.Sin(nx * 9f + ny * 2f) * 0.5f + 0.5f;   // vertical bark grain
                    c = Color.Lerp(barkDark, bark, groove);
                    if (ny > 0.58f) c = Color.Lerp(c, barkLite, (ny - 0.58f) * 3.5f); // thin mossy crown
                    c = Color.Lerp(c, barkDark, Mathf.Abs(nx) * 0.45f);            // side shade
                }

                // Root spikes fanning out at the base
                if (ny <= -0.55f && ny > -0.98f)
                {
                    float spikes = Mathf.Abs(Mathf.Sin(nx * 6.5f));
                    float depth = Mathf.InverseLerp(-0.55f, -0.98f, ny);
                    if (spikes > depth && Mathf.Abs(nx) < 0.82f) c = barkDark;
                }

                // Sunken dark sockets, then small bright glowing eyes for a clear face
                const float ey = 0.24f;
                float eL = Mathf.Pow((nx + 0.22f) * 2.6f, 2) + Mathf.Pow((ny - ey) * 2.6f, 2);
                float eR = Mathf.Pow((nx - 0.22f) * 2.6f, 2) + Mathf.Pow((ny - ey) * 2.6f, 2);
                if (eL < 1f || eR < 1f) c = new Color(0.06f, 0.07f, 0.05f);         // socket
                float pL = Mathf.Pow((nx + 0.22f) * 5.2f, 2) + Mathf.Pow((ny - ey) * 5.2f, 2);
                float pR = Mathf.Pow((nx - 0.22f) * 5.2f, 2) + Mathf.Pow((ny - ey) * 5.2f, 2);
                if (pL < 1f || pR < 1f) c = eye;                                    // glowing pupil

                // Cracked ember maw
                if (ny < 0.04f && ny > -0.14f && Mathf.Abs(nx) < 0.34f)
                {
                    float crack = Mathf.Abs(ny + 0.05f) * 13f + Mathf.Sin(nx * 16f) * 0.16f;
                    if (crack < 0.55f) c = Color.Lerp(ember, eye, Mathf.Abs(nx) * 1.4f);
                }

                px[y * s + x] = c;
            }
        }, FilterMode.Point));

        static Sprite Glyph(string key, Func<float, float, Color> fn) => Get(key, () => Paint(64, (px, s) =>
        {
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float nx = (x + 0.5f) / s * 2f - 1f;
                float ny = -((y + 0.5f) / s * 2f - 1f);
                px[y * s + x] = fn(nx, ny);
            }
        }, FilterMode.Bilinear));

        static Sprite Rounded(int w, int h, int r, Color fill, Color border)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool outR = !InRound(x, y, w, h, r);
                bool inn = InRound(x, y, w, h, Mathf.Max(1, r - 3));
                if (outR) px[y * w + x] = Color.clear;
                else if (!inn) px[y * w + x] = border;
                else px[y * w + x] = fill;
            }
            tex.SetPixels(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        }

        static bool InRound(int x, int y, int w, int h, int radius)
        {
            if (x < radius && y < radius) return Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) <= radius;
            if (x >= w - radius && y < radius) return Vector2.Distance(new Vector2(x, y), new Vector2(w - radius - 1, radius)) <= radius;
            if (x < radius && y >= h - radius) return Vector2.Distance(new Vector2(x, y), new Vector2(radius, h - radius - 1)) <= radius;
            if (x >= w - radius && y >= h - radius) return Vector2.Distance(new Vector2(x, y), new Vector2(w - radius - 1, h - radius - 1)) <= radius;
            return true;
        }

        static Sprite Paint(int size, Action<Color[], int> paint, FilterMode filter)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = filter };
            var px = new Color[size * size];
            paint(px, size);
            tex.SetPixels(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size * 0.5f);
        }

        static Sprite Get(string key, Func<Sprite> make)
        {
            if (Cache.TryGetValue(key, out var s) && s != null) return s;
            s = make();
            Cache[key] = s;
            return s;
        }
    }
}
