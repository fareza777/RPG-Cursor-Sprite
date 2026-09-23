using UnityEngine;

namespace Emberwake
{
    /// <summary>Marks an enemy for bestiary unlock + quest progress on death.</summary>
    public class EnemyIdentity : MonoBehaviour
    {
        [SerializeField] string bestiaryId = "slime";
        [SerializeField] int essenceXp = 8;

        Health health;
        bool reported;

        public void Init(string id, int xp = 8)
        {
            bestiaryId = id;
            essenceXp = xp;
        }

        void Awake()
        {
            health = GetComponent<Health>();
            if (health != null) health.OnDied += OnDead;
        }

        void OnDestroy()
        {
            if (health != null) health.OnDied -= OnDead;
        }

        void OnDead()
        {
            if (reported) return;
            reported = true;
            BestiarySystem.Instance?.Unlock(bestiaryId);
            LevelingSystem.Instance?.AddXp(essenceXp);
            if (bestiaryId == "slime")
                QuestSystem.Instance?.AddProgress("side_slime");
            if (bestiaryId == "barkling")
            {
                QuestSystem.Instance?.Discover("main_barkling");
                QuestSystem.Instance?.Complete("main_barkling");
            }
        }
    }
}
