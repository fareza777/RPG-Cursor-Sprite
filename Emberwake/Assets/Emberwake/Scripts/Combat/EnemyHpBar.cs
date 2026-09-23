using UnityEngine;

namespace Emberwake
{
    /// <summary>World-space HP bar above enemies — always visible.</summary>
    public class EnemyHpBar : MonoBehaviour
    {
        Health health;
        Transform root;
        Transform fill;
        SpriteRenderer fillSr;
        static Sprite white;

        public void Init(Health h)
        {
            health = h;
            if (white == null)
            {
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;
                var px = new Color[16];
                for (int i = 0; i < 16; i++) px[i] = Color.white;
                tex.SetPixels(px);
                tex.Apply();
                white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }

            var rootGo = new GameObject("HpBar");
            root = rootGo.transform;
            root.SetParent(transform, false);
            root.localPosition = new Vector3(0f, 1.05f, 0f);

            var bg = new GameObject("Bg");
            bg.transform.SetParent(root, false);
            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.sprite = white;
            bgSr.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);
            bgSr.sortingOrder = 80;
            bg.transform.localScale = new Vector3(1.35f, 0.18f, 1f);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(root, false);
            fill = fillGo.transform;
            fillSr = fillGo.AddComponent<SpriteRenderer>();
            fillSr.sprite = white;
            fillSr.color = new Color(0.35f, 0.9f, 0.35f, 1f);
            fillSr.sortingOrder = 81;
            fill.localScale = new Vector3(1.28f, 0.12f, 1f);

            if (health != null)
            {
                health.OnDamaged += Refresh;
                health.OnDied += Hide;
            }
            Refresh();
        }

        void Hide()
        {
            if (root != null) root.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (health == null || root == null) return;
            root.rotation = Quaternion.identity;
            float sx = transform.lossyScale.x;
            float sy = transform.lossyScale.y;
            if (Mathf.Abs(sx) > 0.01f && Mathf.Abs(sy) > 0.01f)
                root.localScale = new Vector3(1f / sx, 1f / Mathf.Abs(sy), 1f);
            // Keep bar above sprite regardless of enemy scale
            root.localPosition = new Vector3(0f, 0.95f + 0.25f * Mathf.Abs(sy), 0f);
        }

        void Refresh()
        {
            if (health == null || fill == null) return;
            float t = health.MaxHp > 0 ? Mathf.Clamp01(health.Hp / health.MaxHp) : 0f;
            fill.localScale = new Vector3(1.28f * t, 0.12f, 1f);
            fill.localPosition = new Vector3((-1.28f + 1.28f * t) * 0.5f, 0f, 0f);
            if (t > 0.55f) fillSr.color = new Color(0.35f, 0.9f, 0.35f, 1f);
            else if (t > 0.3f) fillSr.color = new Color(0.95f, 0.75f, 0.2f, 1f);
            else fillSr.color = new Color(0.95f, 0.25f, 0.28f, 1f);
        }

        void OnDestroy()
        {
            if (health != null)
            {
                health.OnDamaged -= Refresh;
                health.OnDied -= Hide;
            }
        }
    }
}
