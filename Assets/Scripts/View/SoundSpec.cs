using System;
using UnityEngine;

namespace Match3.View
{
    public enum Waveform
    {
        Sine,
        Saw
    }

    [Serializable]
    public class SoundSpec
    {
        public float duration = 0.16f;
        public float freqStart = 520f;
        public float freqEnd = 880f;
        public float decay = 22f;
        public Waveform waveform = Waveform.Sine;
        [Range(0f, 1f)] public float harmonic = 0.3f;
        [Range(0f, 1f)] public float noise = 0f;
        public float noiseDecay = 16f;
        [Range(0f, 1f)] public float volume = 0.5f;
    }

    [Serializable]
    public class MelodySpec
    {
        public float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
        public float step = 0.12f;
        public float tail = 0.25f;
        public float decay = 7f;
        public Waveform waveform = Waveform.Sine;
        [Range(0f, 1f)] public float sawMix = 0f;
        [Range(0f, 1f)] public float volume = 0.4f;
    }
}
