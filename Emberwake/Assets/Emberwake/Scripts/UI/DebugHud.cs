using UnityEngine;

namespace Emberwake
{
    /// <summary>Minimal debug HUD until real UI art exists.</summary>
    public class DebugHud : MonoBehaviour
    {
        void OnGUI()
        {
            if (GameManager.Instance == null || GameManager.Instance.Stats == null) return;
            var s = GameManager.Instance.Stats;
            var w = GameManager.Instance.WickRank;
            var inv = GameManager.Instance.Inventory;

            GUI.Box(new Rect(12, 12, 260, 92),
                $"EMBERWAKE\nHearts {s.Hearts}/{s.MaxHearts}  Stam {s.Stamina:0}/{s.MaxStamina:0}\n" +
                $"Wick Rank {w.Rank}  Essence {w.Essence}\nGold {inv.Gold}  SwordT{inv.SwordTier} BowT{inv.BowTier}");
        }
    }
}
