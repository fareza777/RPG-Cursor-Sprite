using UnityEngine;

namespace Emberwake
{
    /// <summary>Procedural music + SFX (no external audio assets required).</summary>
    public class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }

        AudioSource music;
        AudioSource sfx;
        AudioClip clipSlash, clipHit, clipUi, clipStart, clipStep;
        AudioClip clipQuest, clipLevelUp, clipDeath, clipMenu, clipPickup, clipBoss;
        AudioClip musicTitle, musicExplore, musicBattle, musicBoss;
        AudioClip musicLoop;
        float stepCooldown;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake = false;
            music.volume = 0.28f * GameSettings.MusicVolume;
            music.spatialBlend = 0f;

            sfx = gameObject.AddComponent<AudioSource>();
            sfx.loop = false;
            sfx.playOnAwake = false;
            sfx.volume = GameSettings.SfxVolume;
            sfx.spatialBlend = 0f;

            clipSlash = Tone(0.09f, 520f, 0.55f, true);
            clipHit = NoiseBurst(0.07f, 0.5f);
            clipUi = Tone(0.05f, 880f, 0.35f, false);
            clipStart = Chord(0.35f, new[] { 220f, 277f, 330f }, 0.4f);
            clipStep = Tone(0.03f, 140f, 0.15f, true);
            clipQuest = Chord(0.4f, new[] { 330f, 392f, 494f }, 0.45f);
            clipLevelUp = Chord(0.55f, new[] { 262f, 330f, 392f, 523f }, 0.5f);
            clipDeath = Chord(0.5f, new[] { 196f, 185f, 147f }, 0.45f);
            clipMenu = Tone(0.08f, 660f, 0.3f, false);
            clipPickup = Tone(0.12f, 740f, 0.4f, true);
            clipBoss = NoiseBurst(0.2f, 0.65f);
            musicTitle = MakeMusicLoop(5.5f, new[] { 98f, 146.8f, 196f }, new[] { 196f, 220f, 246.9f, 261.6f });
            musicExplore = MakeMusicLoop(6.4f, new[] { 110f, 164.8f, 196f }, new[] { 220f, 246.9f, 261.6f, 293.7f, 329.6f, 293.7f, 261.6f, 246.9f });
            musicBattle = MakeMusicLoop(4.8f, new[] { 130.8f, 155.6f, 196f }, new[] { 261.6f, 311.1f, 349.2f, 392f, 349.2f, 311.1f });
            musicBoss = MakeMusicLoop(5.2f, new[] { 82.4f, 123.5f, 164.8f }, new[] { 164.8f, 196f, 207.7f, 246.9f, 207.7f, 196f });
            musicLoop = musicExplore;
        }

        public void SetVolumes(float music01, float sfx01)
        {
            if (music != null) music.volume = 0.4f * Mathf.Clamp01(music01);
            if (sfx != null) sfx.volume = Mathf.Clamp01(sfx01);
        }

        public void PlayMusic()
        {
            if (music.clip == null) music.clip = musicLoop;
            if (!music.isPlaying) music.Play();
        }

        public void StopMusic() => music.Stop();

        public void SetMusicMood(string mood)
        {
            AudioClip next = mood switch
            {
                "title" => musicTitle,
                "battle" => musicBattle,
                "boss" => musicBoss,
                _ => musicExplore
            };
            if (next == null) return;
            if (music.clip == next && music.isPlaying) return;
            music.clip = next;
            musicLoop = next;
            music.Play();
        }

        public void PlaySlash() => PlayOne(clipSlash, 0.7f);
        public void PlayHit() => PlayOne(clipHit, 0.65f);
        public void PlayUi() => PlayOne(clipUi, 0.45f);
        public void PlayStart() => PlayOne(clipStart, 0.55f);
        public void PlayQuest() => PlayOne(clipQuest, 0.55f);
        public void PlayLevelUp() => PlayOne(clipLevelUp, 0.6f);
        public void PlayDeath() => PlayOne(clipDeath, 0.6f);
        public void PlayMenuOpen() => PlayOne(clipMenu, 0.45f);
        public void PlayPickup() => PlayOne(clipPickup, 0.5f);
        public void PlayBoss() => PlayOne(clipBoss, 0.7f);

        public void PlayStep()
        {
            if (Time.unscaledTime < stepCooldown) return;
            stepCooldown = Time.unscaledTime + 0.22f;
            PlayOne(clipStep, 0.2f);
        }

        void PlayOne(AudioClip clip, float vol)
        {
            if (clip == null || sfx == null) return;
            sfx.PlayOneShot(clip, vol);
        }

        static AudioClip Tone(float seconds, float freq, float amp, bool fall)
        {
            int rate = 22050;
            int n = Mathf.CeilToInt(seconds * rate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float env = fall ? (1f - t / seconds) : Mathf.Min(1f, t * 40f) * Mathf.Min(1f, (seconds - t) * 20f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * amp * env;
            }
            var clip = AudioClip.Create("tone", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip NoiseBurst(float seconds, float amp)
        {
            int rate = 22050;
            int n = Mathf.CeilToInt(seconds * rate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float env = 1f - t / seconds;
                data[i] = (Random.value * 2f - 1f) * amp * env * env;
            }
            var clip = AudioClip.Create("noise", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Chord(float seconds, float[] freqs, float amp)
        {
            int rate = 22050;
            int n = Mathf.CeilToInt(seconds * rate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Min(1f, t * 8f) * Mathf.Min(1f, (seconds - t) * 3f);
                float s = 0f;
                for (int f = 0; f < freqs.Length; f++)
                    s += Mathf.Sin(2f * Mathf.PI * freqs[f] * t);
                data[i] = (s / freqs.Length) * amp * env;
            }
            var clip = AudioClip.Create("chord", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip MakeMusicLoop(float seconds, float[] pad, float[] melody)
        {
            int rate = 22050;
            int n = Mathf.CeilToInt(seconds * rate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float s = 0f;
                for (int p = 0; p < pad.Length; p++)
                    s += Mathf.Sin(2f * Mathf.PI * pad[p] * t) * 0.12f;
                int beat = Mathf.FloorToInt(t * 2f) % melody.Length;
                float melEnv = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f);
                s += Mathf.Sin(2f * Mathf.PI * melody[beat] * t) * 0.08f * melEnv;
                s += (Mathf.PerlinNoise(t * 3f, 0.5f) - 0.5f) * 0.04f;
                data[i] = Mathf.Clamp(s, -1f, 1f);
            }
            var clip = AudioClip.Create("music", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip MakeMusicLoop(float seconds)
        {
            return MakeMusicLoop(seconds,
                new[] { 110f, 164.8f, 196f },
                new[] { 220f, 246.9f, 261.6f, 293.7f, 329.6f, 293.7f, 261.6f, 246.9f });
        }
    }
}
