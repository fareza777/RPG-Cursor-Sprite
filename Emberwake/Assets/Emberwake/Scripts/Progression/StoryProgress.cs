using System;
using System.Collections.Generic;
using UnityEngine;

namespace Emberwake
{
    [Serializable]
    public class StoryProgress : MonoBehaviour
    {
        [SerializeField] List<StoryFlag> flags = new();
        [SerializeField] List<KeyItemId> memoryShards = new(); // reuse list as shard region markers via ints later
        [SerializeField] int memoryShardCount;
        [SerializeField] RegionId currentRegion = RegionId.Millbrook;

        public RegionId CurrentRegion => currentRegion;
        public int MemoryShardCount => memoryShardCount;

        public event Action OnChanged;

        public bool HasFlag(StoryFlag flag) => flags.Contains(flag);

        public void SetFlag(StoryFlag flag)
        {
            if (flags.Contains(flag)) return;
            flags.Add(flag);
            OnChanged?.Invoke();
        }

        public void SetRegion(RegionId region)
        {
            currentRegion = region;
            OnChanged?.Invoke();
        }

        public void AddMemoryShard()
        {
            memoryShardCount = Mathf.Min(6, memoryShardCount + 1);
            OnChanged?.Invoke();
        }

        public bool CanTrueEnding => memoryShardCount >= 6 && HasFlag(StoryFlag.ClearedThrone);
    }
}
