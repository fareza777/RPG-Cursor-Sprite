using UnityEngine;

namespace Emberwake
{
    /// <summary>
    /// Loads Super Retro Collection sprites from Assets/Resources (real copies, not Gif junction).
    /// </summary>
    public static class SliceArt
    {
        static Sprite[] groundTiles;
        static Sprite[] dirtTiles;
        static Sprite[] grassFill;
        static Sprite[] pathFill;
        static Sprite[] stoneFill;
        static Sprite[] pathEdge;
        static bool logged;

        public static Sprite HeroIdle()
        {
            var s = First("Hero/hero/color_1/walk/hero_walk_DOWN");
            return s != null ? s : Pixel(new Color(0.25f, 0.7f, 1f));
        }

        public static Sprite Slime()
        {
            var s = Resources.Load<Sprite>("Battlers/SlimeA");
            if (s == null)
            {
                var all = Resources.LoadAll<Sprite>("Battlers/SlimeA");
                if (all != null && all.Length > 0) s = all[0];
            }
            return s != null ? s : Pixel(new Color(0.3f, 0.9f, 0.35f));
        }

        public static Sprite Boss()
        {
            var all = Resources.LoadAll<Sprite>("Characters/Monsters/Monsters_01_0");
            if (all != null && all.Length > 0) return all[Mathf.Min(2, all.Length - 1)];
            return Pixel(new Color(0.45f, 0.7f, 0.3f));
        }

        public static Sprite Ghost()
        {
            var s = Resources.Load<Sprite>("Battlers/GhostA");
            if (s == null)
            {
                var all = Resources.LoadAll<Sprite>("Battlers/GhostA");
                if (all != null && all.Length > 0) s = all[0];
            }
            return s != null ? s : Slime();
        }

        public static Sprite FloorBg(string roomKey)
        {
            string path = roomKey switch
            {
                "hub" => "Backgrounds/PlainA",
                "root" => "Backgrounds/ForestA",
                "pressure" => "Backgrounds/DungeonA",
                "reliquary" => "Backgrounds/DungeonB",
                "boss" => "Backgrounds/DungeonC",
                "altar" => "Backgrounds/DungeonD",
                _ => "Backgrounds/ForestB"
            };
            var s = Resources.Load<Sprite>(path);
            if (s == null)
            {
                var all = Resources.LoadAll<Sprite>(path);
                if (all != null && all.Length > 0) s = all[0];
            }
            return s != null ? s : GroundTile(0);
        }

        public static Sprite GroundTile(int index)
        {
            EnsureFillTiles();
            return grassFill[Mathf.Abs(index) % grassFill.Length];
        }

        public static Sprite PathTile(int index)
        {
            EnsureFillTiles();
            return pathFill[Mathf.Abs(index) % pathFill.Length];
        }

        public static Sprite GrassTile(int index)
        {
            EnsureFillTiles();
            return grassFill[Mathf.Abs(index) % grassFill.Length];
        }

        public static Sprite StoneTile(int index)
        {
            EnsureFillTiles();
            return stoneFill[Mathf.Abs(index) % stoneFill.Length];
        }

        public static Sprite PathEdgeTile(int index, bool rightSide)
        {
            EnsureFillTiles();
            if (pathEdge == null || pathEdge.Length == 0)
                return PathTile(index);
            var s = pathEdge[Mathf.Abs(index) % pathEdge.Length];
            // Caller flips via scale; rightSide reserved for future mirrored bake
            return s;
        }

        public static Sprite PixelColor(Color c) => Pixel(c);

        public static Sprite Rock(int i = 0)
        {
            return PrefabSprite("Prefabs/Rocks/Sprites/rock_0" + ((i % 5) + 1), new Color(0.45f, 0.4f, 0.35f));
        }

        public static Sprite Crate(int i = 0)
        {
            return PrefabSprite("Prefabs/Crates/Sprites/crate_0" + ((i % 5) + 1), new Color(0.75f, 0.55f, 0.25f));
        }

        public static Sprite Barrel(int i = 0)
        {
            return PrefabSprite("Prefabs/Barrels/Sprites/barrel_0" + ((i % 5) + 1), new Color(0.55f, 0.35f, 0.2f));
        }

        public static Sprite Pot(int i = 0)
        {
            string[] names = { "pot_01", "pot_02", "pot_10", "pot_11", "pot_12" };
            return PrefabSprite("Prefabs/Pots/Sprites/" + names[Mathf.Abs(i) % names.Length], new Color(0.7f, 0.45f, 0.3f));
        }

        public static Sprite Torch(int i = 0)
        {
            return PrefabSprite("Prefabs/Torches/Sprites/Torch_0" + ((i % 5) + 1), new Color(1f, 0.7f, 0.2f));
        }

        public static Sprite Column(int i = 0)
        {
            return PrefabSprite("Prefabs/Columns/Sprites/column_0" + ((i % 5) + 1), new Color(0.55f, 0.55f, 0.6f));
        }

        public static Sprite ExitMarker()
        {
            return Torch(0);
        }

        public static Sprite Plate()
        {
            return GroundTile(3);
        }

        public static Sprite Gloves()
        {
            return PrefabSprite("Prefabs/Pots/Sprites/pot_11", new Color(0.9f, 0.75f, 0.2f));
        }

        public static Sprite Gate()
        {
            return Column(1);
        }

        public static Sprite Altar()
        {
            return Column(0);
        }

        public static void LogStatusOnce()
        {
            if (logged) return;
            logged = true;
            var hero = First("Hero/hero/color_1/walk/hero_walk_DOWN");
            var slime = Slime();
            var floor = Resources.Load<Sprite>("Backgrounds/PlainA");
            Debug.Log($"[Emberwake] SliceArt hero={(hero != null)} slime={(slime != null && slime.name != "Pixel")} floor={(floor != null)}");
        }

        static Sprite PrefabSprite(string path, Color fallback)
        {
            var s = Resources.Load<Sprite>(path);
            if (s == null)
            {
                var all = Resources.LoadAll<Sprite>(path);
                if (all != null && all.Length > 0) s = all[0];
            }
            return s != null ? s : Pixel(fallback);
        }

        static Sprite First(string path)
        {
            var all = Resources.LoadAll<Sprite>(path);
            if (all != null && all.Length > 0) return all[0];
            return Resources.Load<Sprite>(path);
        }

        static void EnsureGround()
        {
            // Kept for any legacy callers — now redirects to fill tiles
            EnsureFillTiles();
            groundTiles = grassFill;
            dirtTiles = pathFill;
        }

        static void EnsureFillTiles()
        {
            if (grassFill != null) return;

            // Always ship hand-crafted pixel fills (atlas wang sheets are 64x64 and look broken if used whole)
            var grassList = new System.Collections.Generic.List<Sprite>
            {
                MakeGrassTile(0), MakeGrassTile(1), MakeGrassTile(2), MakeGrassTile(3)
            };
            var pathList = new System.Collections.Generic.List<Sprite>
            {
                MakeDirtTile(0), MakeDirtTile(1), MakeDirtTile(2), MakeDirtTile(3)
            };
            var stoneList = new System.Collections.Generic.List<Sprite>
            {
                MakeStoneTile(0), MakeStoneTile(1), MakeStoneTile(2)
            };
            var edgeList = new System.Collections.Generic.List<Sprite>
            {
                MakePathEdgeTile(0), MakePathEdgeTile(1), MakePathEdgeTile(2)
            };

            grassFill = grassList.ToArray();
            pathFill = pathList.ToArray();
            stoneFill = stoneList.ToArray();
            pathEdge = edgeList.ToArray();
            groundTiles = grassFill;
            dirtTiles = pathFill;
            Debug.Log($"[Emberwake] SliceArt fill grass={grassFill.Length} path={pathFill.Length} stone={stoneFill.Length}");
        }

        static Sprite MakeGrassTile(int seed)
        {
            // Soft plains grass — low contrast so tiling is less noisy
            Color baseCol = new Color(0.34f, 0.62f, 0.28f);
            Color dark = new Color(0.28f, 0.54f, 0.22f);
            Color light = new Color(0.38f, 0.68f, 0.32f);
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int h = Hash(x, y, seed);
                float n = (h & 0xFF) / 255f;
                Color c = n < 0.08f ? dark : (n > 0.94f ? light : baseCol);
                px[y * size + x] = c;
            }
            for (int t = 0; t < 5; t++)
            {
                int h = Hash(t * 3, seed, seed + 7);
                int tx = Mathf.Abs(h) % (size - 2);
                int ty = Mathf.Abs(h >> 8) % (size - 2);
                px[ty * size + tx] = dark;
            }
            tex.SetPixels(px);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static Sprite MakeDirtTile(int seed)
        {
            Color baseCol = new Color(0.66f, 0.52f, 0.34f);
            Color dark = new Color(0.52f, 0.38f, 0.24f);
            Color light = new Color(0.74f, 0.60f, 0.42f);
            Color pebble = new Color(0.55f, 0.52f, 0.48f);
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int h = Hash(x, y, seed + 40);
                float n = (h & 0xFF) / 255f;
                Color c = n < 0.15f ? dark : (n > 0.88f ? light : baseCol);
                if (((h >> 8) & 0xFF) > 250) c = pebble;
                if ((y % 5) == 0 && ((h >> 4) & 3) == 0)
                    c = Color.Lerp(c, dark, 0.2f);
                px[y * size + x] = c;
            }
            tex.SetPixels(px);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static Sprite MakePathEdgeTile(int seed)
        {
            // Left half dirt-ish, right half grass — jagged seam
            Color dirt = new Color(0.64f, 0.5f, 0.32f);
            Color dirtD = new Color(0.5f, 0.36f, 0.22f);
            Color grass = new Color(0.36f, 0.66f, 0.30f);
            Color grassD = new Color(0.26f, 0.52f, 0.20f);
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                int h = Hash(y, seed, seed + 3);
                int seam = 14 + ((h & 7) - 3); // jagged around mid
                for (int x = 0; x < size; x++)
                {
                    int hn = Hash(x, y, seed + 11);
                    bool isDirt = x < seam;
                    Color baseCol = isDirt ? dirt : grass;
                    Color dark = isDirt ? dirtD : grassD;
                    float n = (hn & 0xFF) / 255f;
                    Color c = n < 0.15f ? dark : baseCol;
                    // dark fringe on grass side of seam
                    if (!isDirt && x <= seam + 1)
                        c = grassD;
                    px[y * size + x] = c;
                }
            }
            tex.SetPixels(px);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static Sprite MakeStoneTile(int seed)
        {
            Color baseCol = new Color(0.4f, 0.4f, 0.46f);
            Color dark = new Color(0.28f, 0.28f, 0.34f);
            Color light = new Color(0.55f, 0.55f, 0.6f);
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int h = Hash(x, y, seed + 90);
                bool seam = (x % 8) == 0 || (y % 8) == 0;
                float n = (h & 0xFF) / 255f;
                Color c = seam ? dark : (n < 0.3f ? dark : (n > 0.75f ? light : baseCol));
                px[y * size + x] = c;
            }
            tex.SetPixels(px);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static Sprite[] tufts;
        static Sprite[] flowers;

        public static Sprite Tuft(int i)
        {
            if (tufts == null)
                tufts = new[] { MakeTuft(1), MakeTuft(2), MakeTuft(3), MakeTuft(4) };
            return tufts[Mathf.Abs(i) % tufts.Length];
        }

        public static Sprite Flower(int i)
        {
            if (flowers == null)
                flowers = new[] { MakeFlower(1), MakeFlower(2), MakeFlower(3) };
            return flowers[Mathf.Abs(i) % flowers.Length];
        }

        static Sprite MakeTuft(int seed)
        {
            const int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];
            for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
            Color[] shades =
            {
                new Color(0.24f, 0.5f, 0.2f), new Color(0.32f, 0.62f, 0.26f),
                new Color(0.4f, 0.7f, 0.3f)
            };
            int blades = 4 + (Hash(seed, 7, 3) & 3);
            for (int b = 0; b < blades; b++)
            {
                int h = Hash(b, seed, seed + 5);
                int bx = 3 + (Mathf.Abs(h) % (size - 6));
                int height = 5 + (Mathf.Abs(h >> 4) % 7);
                float lean = ((h >> 8) & 3) - 1.5f;
                var col = shades[Mathf.Abs(h >> 12) % shades.Length];
                for (int yy = 0; yy < height; yy++)
                {
                    int cx = Mathf.Clamp(bx + Mathf.RoundToInt(lean * yy / height), 0, size - 1);
                    int y = yy; // grow from bottom
                    px[y * size + cx] = col;
                    if (yy < height - 1 && cx + 1 < size) px[y * size + cx + 1] = col * 0.9f;
                }
            }
            tex.SetPixels(px);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.15f), size);
        }

        static Sprite MakeFlower(int seed)
        {
            const int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];
            for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
            Color stem = new Color(0.28f, 0.55f, 0.24f);
            Color[] petalSet =
            {
                new Color(1f, 0.85f, 0.35f), new Color(0.95f, 0.5f, 0.55f),
                new Color(0.75f, 0.7f, 1f), new Color(1f, 0.95f, 0.95f)
            };
            Color petal = petalSet[Mathf.Abs(Hash(seed, 2, 9)) % petalSet.Length];
            Color core = new Color(1f, 0.8f, 0.2f);
            int cx = size / 2, stemH = 6 + (Hash(seed, 3, 1) & 3);
            for (int y = 0; y < stemH; y++) px[y * size + cx] = stem;
            int fy = stemH + 2;
            // 4-petal bloom + core
            void Set(int x, int y, Color c) { if (x >= 0 && x < size && y >= 0 && y < size) px[y * size + x] = c; }
            Set(cx, fy, core);
            Set(cx - 1, fy, petal); Set(cx + 1, fy, petal);
            Set(cx, fy - 1, petal); Set(cx, fy + 1, petal);
            Set(cx - 1, fy - 1, petal * 0.92f); Set(cx + 1, fy + 1, petal * 0.92f);
            tex.SetPixels(px);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.15f), size);
        }

        static int Hash(int x, int y, int seed)
        {
            int h = (x * 374761393 + y * 668265263 + seed * 982451653) ^ (x * y + seed);
            h = (h ^ (h >> 13)) * 1274126177;
            return h;
        }

        static Sprite MakeNoiseTile(int seed, Color baseCol, Color dark, Color light)
        {
            return MakeGrassTile(seed);
        }

        static Sprite Pixel(Color c)
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            var cols = new Color[64];
            for (int i = 0; i < 64; i++) cols[i] = c;
            tex.SetPixels(cols);
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
        }
    }
}
