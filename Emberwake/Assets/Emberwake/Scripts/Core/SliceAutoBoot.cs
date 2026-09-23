using UnityEngine;

namespace Emberwake
{
    /// <summary>
    /// Guarantees the vertical slice boots even if the scene script reference breaks.
    /// </summary>
    public static class SliceAutoBoot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            Debug.Log("[Emberwake] SliceAutoBoot AfterSceneLoad");
            if (Object.FindFirstObjectByType<VerticalSliceDirector>() != null)
            {
                Debug.Log("[Emberwake] VerticalSliceDirector already present");
                return;
            }

            var go = new GameObject("VerticalSlice_Auto");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<VerticalSliceDirector>();
            Debug.Log("[Emberwake] VerticalSliceDirector spawned by auto-boot");
        }
    }
}
