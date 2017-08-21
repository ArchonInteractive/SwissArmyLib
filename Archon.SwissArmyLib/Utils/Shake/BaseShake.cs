using UnityEngine;

namespace Archon.SwissArmyLib.Utils.Shake
{
    public abstract class BaseShake<T>
    {
        public float Duration { get; private set; }
        public int Frequency { get; private set; }
        public float Amplitude { get; private set; }
        protected float StartTime;

        public float NormalizedTime
        {
            get
            {
                return Mathf.InverseLerp(StartTime, StartTime + Duration, Time.time);
            }
        }

        public bool IsDone
        {
            get { return NormalizedTime >= 1f; }
        }

        public virtual void Start(float amplitude, int frequency, float duration)
        {
            Amplitude = amplitude;
            Duration = duration;
            Frequency = frequency;

            StartTime = Time.time;
        }

        public T GetAmplitude()
        {
            return GetAmplitude(NormalizedTime);
        }

        public abstract T GetAmplitude(float t);
    }
}