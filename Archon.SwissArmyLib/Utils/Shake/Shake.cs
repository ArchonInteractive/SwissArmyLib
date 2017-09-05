using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Archon.SwissArmyLib.Utils.Shake
{
    /// <summary>
    /// An object that can shake a float over time.
    /// </summary>
    public class Shake : BaseShake<float>
    {
        private readonly List<float> _samples = new List<float>();

        /// <inheritdoc />
        public override void Start(float amplitude, int frequency, float duration)
        {
            base.Start(amplitude, frequency, duration);

            _samples.Clear();

            var samples = duration*frequency;

            for (var i = 0; i < samples; i++)
                _samples.Add(Random.value * 2 - 1);
        }

        /// <inheritdoc />
        public override float GetAmplitude(float t)
        {
            var sampleIndex = Mathf.FloorToInt(_samples.Count*t);
            var firstSample = GetSample(sampleIndex);
            var secondSample = GetSample(sampleIndex+1);

            var innerT = _samples.Count*t - sampleIndex;

            return Mathf.Lerp(firstSample, secondSample, innerT) * (1 - t) * Amplitude;
        }

        private float GetSample(int index)
        {
            if (index < 0 || index >= _samples.Count)
                return 0;

            return _samples[index];
        }
    }
}