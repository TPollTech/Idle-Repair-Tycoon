using UnityEngine;

namespace IdleRepairTycoon
{
    public sealed class IdleGameAudio : MonoBehaviour
    {
        private AudioSource sfx;
        private AudioSource amb;
        private AudioClip coin;
        private AudioClip job;
        private AudioClip upgrade;
        private AudioClip clickSfx;
        private AudioClip selectSfx;

        private void Awake()
        {
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.volume = 0.45f;

            amb = gameObject.AddComponent<AudioSource>();
            amb.loop = true;
            amb.volume = 0.10f;

            coin = GenTone(880f, 0.12f, 0.50f, 0f);
            job = GenChirp(440f, 1100f, 0.25f, 0.40f);
            upgrade = GenArpeggio(440f, 0.45f, 0.35f);
            clickSfx = GenTone(1200f, 0.04f, 0.25f, 0f);
            selectSfx = GenTone(660f, 0.08f, 0.30f, 0f);
            PlayAmbient();
        }

        private void PlayAmbient()
        {
            int rate = AudioSettings.outputSampleRate;
            int len = rate * 6;
            var clip = AudioClip.Create("Ambient", len, 1, rate, false);
            var buf = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / rate;
                float hum = Mathf.Sin(t * 55f * Mathf.PI * 2f) * 0.025f
                          + Mathf.Sin(t * 85f * Mathf.PI * 2f) * 0.015f
                          + Mathf.Sin(t * 115f * Mathf.PI * 2f) * 0.010f;
                buf[i] = hum * (0.5f + Mathf.Sin(t * 0.3f) * 0.5f);
            }
            clip.SetData(buf, 0);
            amb.clip = clip;
            amb.Play();
        }

        private static AudioClip GenTone(float freq, float dur, float vol, float endVol)
        {
            int rate = AudioSettings.outputSampleRate;
            int n = Mathf.FloorToInt(rate * dur);
            var clip = AudioClip.Create("Tone", n, 1, rate, false);
            var buf = new float[n];
            for (int i = 0; i < n; i++)
            {
                float p = (float)i / n;
                buf[i] = Mathf.Sin(p * dur * freq * Mathf.PI * 2f) * Mathf.Lerp(vol, endVol, p);
            }
            clip.SetData(buf, 0);
            return clip;
        }

        private static AudioClip GenChirp(float f0, float f1, float dur, float vol)
        {
            int rate = AudioSettings.outputSampleRate;
            int n = Mathf.FloorToInt(rate * dur);
            var clip = AudioClip.Create("Chirp", n, 1, rate, false);
            var buf = new float[n];
            for (int i = 0; i < n; i++)
            {
                float p = (float)i / n;
                float freq = Mathf.Lerp(f0, f1, p);
                buf[i] = Mathf.Sin((float)i / rate * freq * Mathf.PI * 2f) * Mathf.Clamp01(1f - p * 0.7f) * vol;
            }
            clip.SetData(buf, 0);
            return clip;
        }

        private static AudioClip GenArpeggio(float baseFreq, float dur, float vol)
        {
            int rate = AudioSettings.outputSampleRate;
            int n = Mathf.FloorToInt(rate * dur);
            var clip = AudioClip.Create("Arpeggio", n, 1, rate, false);
            var buf = new float[n];
            float[] mults = { 1f, 1.25f, 1.5f, 2f };
            int seg = n / mults.Length;
            for (int i = 0; i < n; i++)
            {
                int s = Mathf.Min(i / seg, mults.Length - 1);
                float freq = baseFreq * mults[s];
                float lp = (float)(i - s * seg) / seg;
                buf[i] = Mathf.Sin((float)i / rate * freq * Mathf.PI * 2f) * Mathf.Clamp01(1f - lp * 1.5f) * vol;
            }
            clip.SetData(buf, 0);
            return clip;
        }

        public void PlayCoin() { if (coin != null) sfx.PlayOneShot(coin); }
        public void PlayJobDone() { if (job != null) sfx.PlayOneShot(job); }
        public void PlayUpgrade() { if (upgrade != null) sfx.PlayOneShot(upgrade); }
        public void PlayClick() { if (clickSfx != null) sfx.PlayOneShot(clickSfx); }
        public void PlaySelect() { if (selectSfx != null) sfx.PlayOneShot(selectSfx); }
    }
}
