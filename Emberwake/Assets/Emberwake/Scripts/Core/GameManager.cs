using UnityEngine;

namespace Emberwake
{
    /// <summary>
    /// Bootstrap singleton. Keep one in the first loaded scene (DontDestroyOnLoad).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] PlayerStats playerStats;
        [SerializeField] InventorySystem inventory;
        [SerializeField] StoryProgress story;
        [SerializeField] WickRankSystem wickRank;

        public PlayerStats Stats => playerStats;
        public InventorySystem Inventory => inventory;
        public StoryProgress Story => story;
        public WickRankSystem WickRank => wickRank;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (playerStats == null) playerStats = GetComponent<PlayerStats>();
            if (inventory == null) inventory = GetComponent<InventorySystem>();
            if (story == null) story = GetComponent<StoryProgress>();
            if (wickRank == null) wickRank = GetComponent<WickRankSystem>();
        }

        public void PauseGameplay(bool paused)
        {
            Time.timeScale = paused ? 0f : 1f;
        }
    }
}
