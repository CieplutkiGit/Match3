using UnityEngine;

namespace Match3.View
{
    public static class ProceduralAudio
    {
        private const int SampleRate = 44100;

        public static AudioClip Build(string name, SoundSpec spec)
        {
            int count = Mathf.Max(1, (int)(SampleRate * spec.duration));
            var data = new float[count];
            var rng = new System.Random(7);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SampleRate;
                float progress = Mathf.Clamp01(t / spec.duration);
                float freq = Mathf.Lerp(spec.freqStart, spec.freqEnd, progress);
                float env = Mathf.Exp(-t * spec.decay);
                float wave = Oscillator(spec.waveform, freq, t);
                wave += spec.harmonic * Oscillator(spec.waveform, freq * 2f, t);
                float n = spec.noise > 0f
                    ? (float)(rng.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * spec.noiseDecay)
                    : 0f;
                data[i] = Mathf.Clamp((wave + spec.noise * n) * env * spec.volume, -1f, 1f);
            }
            return ToClip(name, data);
        }

        public static AudioClip BuildMelody(string name, MelodySpec spec)
        {
            int noteCount = spec.notes != null ? spec.notes.Length : 0;
            if (noteCount == 0) return ToClip(name, new float[1]);

            float total = spec.step * noteCount + spec.tail;
            int count = Mathf.Max(1, (int)(SampleRate * total));
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SampleRate;
                int idx = Mathf.Clamp((int)(t / spec.step), 0, noteCount - 1);
                float local = t - idx * spec.step;
                float env = Mathf.Exp(-local * spec.decay);
                float freq = spec.notes[idx];
                float tone = Oscillator(spec.waveform, freq, t);
                float saw = Saw(freq, t);
                data[i] = Mathf.Clamp((tone * (1f - spec.sawMix) + saw * spec.sawMix) * env * spec.volume, -1f, 1f);
            }
            return ToClip(name, data);
        }

        private static float Oscillator(Waveform wave, float freq, float t)
        {
            return wave == Waveform.Saw ? Saw(freq, t) : Mathf.Sin(2f * Mathf.PI * freq * t);
        }

        private static float Saw(float freq, float t)
        {
            return 2f * (freq * t - Mathf.Floor(freq * t + 0.5f));
        }

        private static AudioClip ToClip(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
