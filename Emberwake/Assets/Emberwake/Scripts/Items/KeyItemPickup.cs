using UnityEngine;

namespace Emberwake
{
    /// <summary>
    /// Grants a dungeon key item on trigger (chest / altar / boss clear proxy).
    /// </summary>
    public class KeyItemPickup : MonoBehaviour
    {
        [SerializeField] KeyItemId item = KeyItemId.GlovesOfLift;
        [SerializeField] StoryFlag flagToSet = StoryFlag.ClearedHollowroot;
        [SerializeField] bool destroyOnPickup = true;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (GameManager.Instance == null) return;

            GameManager.Instance.Inventory.GrantKeyItem(item);
            GameManager.Instance.Story.SetFlag(flagToSet);
            GameManager.Instance.WickRank.AddEssence(40);
            Debug.Log($"[Emberwake] Obtained {item}");
            if (destroyOnPickup) Destroy(gameObject);
        }
    }
}
