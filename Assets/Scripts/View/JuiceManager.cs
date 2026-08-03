using System;
using System.Collections;
using UnityEngine;

namespace Match3.View
{
    public sealed class JuiceManager : MonoBehaviour
    {
        public static JuiceManager Instance { get; private set; }

        public event Action<int> DisplayScoreChanged;
        public Func<Vector3> ScoreWorldTarget;

        private readonly SandBar _bar = new SandBar();
        private readonly AnimatedScore _score = new AnimatedScore();
        private bool _configured;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                enabled = false;
                return;
            }

            Instance = this;
            _score.Changed += HandleScoreChanged;
        }

        private void OnEnable()
        {
            if (_configured)
                StartCoroutine(AmbientRoutine());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            _score.Changed -= HandleScoreChanged;
            if (Instance == this)
                Instance = null;
        }

        public void Configure(Vector3 center, float height, float width)
        {
            if (_configured)
                return;

            _bar.Build(transform, center, height, width);
            _configured = true;
            StartCoroutine(AmbientRoutine());
        }

        public void InitializeDisplay(int score, float progress)
        {
            _bar.SetImmediate(progress);
            _score.SetImmediate(score);
        }

        public void SetTargets(int score, float progress)
        {
            _bar.SetTarget(progress);
            _score.SetTarget(score);
        }

        public void Collect(Vector3 from, Color color)
        {
            SpawnToBar(from, color);
            SpawnToScore(from, color);
            ParticleSpawner.Debris(from, color);
        }

        private void HandleScoreChanged(int score)
        {
            DisplayScoreChanged?.Invoke(score);
        }

        private void SpawnToBar(Vector3 from, Color color)
        {
            float topY = _bar.TargetTopY;
            for (int i = 0; i < 5; i++)
            {
                _bar.RegisterParticle();
                Vector3 to = new Vector3(_bar.X + UnityEngine.Random.Range(-_bar.Width, _bar.Width) * 0.35f, topY, 0f);
                ParticleSpawner.Fly(from, to, color, i * 0.05f, _bar.OnParticleArrived);
            }
        }

        private void SpawnToScore(Vector3 from, Color color)
        {
            Vector3 target = ScoreWorldTarget != null ? ScoreWorldTarget() : DefaultScoreTarget();
            for (int i = 0; i < 3; i++)
            {
                _score.RegisterParticle();
                Vector3 to = target + (Vector3)UnityEngine.Random.insideUnitCircle * 0.3f;
                ParticleSpawner.Fly(from, to, color, i * 0.06f, _score.OnParticleArrived);
            }
        }

        private static Vector3 DefaultScoreTarget()
        {
            if (Camera.main == null) return Vector3.zero;
            float depth = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 p = Camera.main.ViewportToWorldPoint(new Vector3(0.85f, 0.92f, depth));
            p.z = 0f;
            return p;
        }

        private IEnumerator AmbientRoutine()
        {
            while (true)
            {
                ParticleSpawner.BackgroundFall();
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.12f, 0.35f));
            }
        }
    }
}
