using UnityEngine;

namespace Emberwake
{
    /// <summary>Music / SFX prefs shared by menu and AudioDirector.</summary>
    public static class GameSettings
    {
        const string MusicKey = "ew_music";
        const string SfxKey = "ew_sfx";

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicKey, 0.7f);
            set { PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); Apply(); }
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(SfxKey, 0.85f);
            set { PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); Apply(); }
        }

        public static void Apply()
        {
            if (AudioDirector.Instance == null) return;
            AudioDirector.Instance.SetVolumes(MusicVolume, SfxVolume);
        }

        public static void ShareGame()
        {
            const string url = "https://play.google.com/store/apps/details?id=com.emberwakestudio.emberwake";
            const string text = "Emberwake — ketika lentera terakhir padam.\n" + url;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var intentClass = new AndroidJavaClass("android.content.Intent");
                using var intent = new AndroidJavaObject("android.content.Intent");
                intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
                intent.Call<AndroidJavaObject>("setType", "text/plain");
                intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text);
                using var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unity.GetStatic<AndroidJavaObject>("currentActivity");
                using var chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, "Bagikan Emberwake");
                activity.Call("startActivity", chooser);
                return;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[Emberwake] Share failed: " + ex.Message);
            }
#endif
            GUIUtility.systemCopyBuffer = text;
            PortraitMobileHud.Instance?.ShowToast("Tautan disalin");
        }

        public static void RateOnStore()
        {
            const string market = "market://details?id=com.emberwakestudio.emberwake";
            const string web = "https://play.google.com/store/apps/details?id=com.emberwakestudio.emberwake";
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                Application.OpenURL(market);
                return;
            }
            catch { }
#endif
            Application.OpenURL(web);
        }
    }
}
