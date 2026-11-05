using UnityEngine;

namespace EuroHaul.Audio
{
    /// <summary>Procedural placeholder horn tone. Replace with properly licensed truck horn recordings.</summary>
    public static class PrototypeHornAudio
    {
        public static AudioClip CreateClip(string clipName = "EURO HAUL Prototype Horn")
        {
            const int sampleRate = 44100;
            const float duration = 0.65f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float fade = Mathf.Min(1f, time * 18f) * Mathf.Min(1f, (duration - time) * 18f);
                samples[i] = fade * 0.22f *
                    (Mathf.Sin(2f * Mathf.PI * 370f * time) + Mathf.Sin(2f * Mathf.PI * 440f * time));
            }
            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}