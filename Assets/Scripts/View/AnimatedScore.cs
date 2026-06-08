using System;
using UnityEngine;

namespace Match3.View
{
    public class AnimatedScore
    {
        private int _target;
        private int _shown;
        private int _inFlight;

        public event Action<int> Changed;

        public void SetImmediate(int value)
        {
            _target = _shown = value;
            Changed?.Invoke(_shown);
        }

        public void SetTarget(int value)
        {
            _target = value;
        }

        public void RegisterParticle()
        {
            _inFlight++;
        }

        public void OnParticleArrived()
        {
            _inFlight = Mathf.Max(0, _inFlight - 1);
            int n = _inFlight + 1;
            _shown += Mathf.CeilToInt((_target - _shown) / (float)n);
            if (_inFlight == 0) _shown = _target;
            Changed?.Invoke(_shown);
        }
    }
}
