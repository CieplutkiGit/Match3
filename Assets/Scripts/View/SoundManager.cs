using UnityEngine;

namespace Match3.View
{
    public sealed class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Voices")]
        [SerializeField] private int _voiceCount = 8;
        [Range(0, 14)]
        [SerializeField] private int _maxComboSteps = 14;

        [Header("Match Pop")]
        [SerializeField] private SoundSpec _pop = new SoundSpec
        {
            duration = 0.16f,
            freqStart = 520f,
            freqEnd = 880f,
            decay = 22f,
            waveform = Waveform.Sine,
            harmonic = 0.3f,
            noise = 0f,
            volume = 0.5f
        };

        [Header("Swap")]
        [SerializeField] private SoundSpec _swap = new SoundSpec
        {
            duration = 0.12f,
            freqStart = 300f,
            freqEnd = 620f,
            decay = 30f,
            waveform = Waveform.Sine,
            harmonic = 0f,
            noise = 0f,
            volume = 0.4f
        };

        [Header("Invalid Move")]
        [SerializeField] private SoundSpec _invalid = new SoundSpec
        {
            duration = 0.22f,
            freqStart = 220f,
            freqEnd = 130f,
            decay = 10f,
            waveform = Waveform.Saw,
            harmonic = 0f,
            noise = 0f,
            volume = 0.35f
        };

        [Header("Special Blast")]
        [SerializeField] private SoundSpec _blast = new SoundSpec
        {
            duration = 0.5f,
            freqStart = 170f,
            freqEnd = 55f,
            decay = 6f,
            waveform = Waveform.Sine,
            harmonic = 0f,
            noise = 0.6f,
            noiseDecay = 16f,
            volume = 0.9f
        };

        [Header("Win Jingle")]
        [SerializeField] private MelodySpec _win = new MelodySpec
        {
            notes = new[] { 523.25f, 659.25f, 783.99f, 1046.5f },
            step = 0.12f,
            decay = 7f,
            waveform = Waveform.Sine,
            sawMix = 0f,
            volume = 0.4f
        };

        [Header("Lose Jingle")]
        [SerializeField] private MelodySpec _lose = new MelodySpec
        {
            notes = new[] { 392f, 329.63f, 261.63f, 196f },
            step = 0.16f,
            decay = 5f,
            waveform = Waveform.Sine,
            sawMix = 0.3f,
            volume = 0.4f
        };

        private AudioClip _popClip;
        private AudioClip _swapClip;
        private AudioClip _invalidClip;
        private AudioClip _blastClip;
        private AudioClip _winClip;
        private AudioClip _loseClip;

        private AudioSource[] _sources;
        private int _next;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                enabled = false;
                return;
            }

            Instance = this;

            _popClip = ProceduralAudio.Build("pop", _pop);
            _swapClip = ProceduralAudio.Build("swap", _swap);
            _invalidClip = ProceduralAudio.Build("invalid", _invalid);
            _blastClip = ProceduralAudio.Build("blast", _blast);
            _winClip = ProceduralAudio.BuildMelody("win", _win);
            _loseClip = ProceduralAudio.BuildMelody("lose", _lose);

            _sources = new AudioSource[Mathf.Max(1, _voiceCount)];
            for (int i = 0; i < _sources.Length; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sources[i] = source;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void PlaySwap()
        {
            Play(_swapClip, 1f);
        }

        public void PlayInvalid()
        {
            Play(_invalidClip, 1f);
        }

        public void PlayBlast()
        {
            Play(_blastClip, 1f);
        }

        public void PlayWin()
        {
            Play(_winClip, 1f);
        }

        public void PlayLose()
        {
            Play(_loseClip, 1f);
        }

        public void PlayPop(int combo)
        {
            int steps = Mathf.Clamp(combo, 0, _maxComboSteps);
            float pitch = Mathf.Pow(2f, steps / 12f);
            Play(_popClip, pitch);
        }

        private void Play(AudioClip clip, float pitch)
        {
            if (clip == null) return;
            var source = _sources[_next];
            _next = (_next + 1) % _sources.Length;
            source.clip = clip;
            source.pitch = pitch;
            source.Play();
        }
    }
}
