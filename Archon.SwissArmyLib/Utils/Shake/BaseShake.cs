using UnityEngine;

namespace Archon.SwissArmyLib.Utils.Shake
{
    /// <summary>
    /// Represents an object that can shake a value of the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of value to be shaked.</typeparam>
    public abstract class BaseShake<T>
    {
        /// <summary>
        /// Gets or sets whether the shake should use <see cref="Time.time"/> or <see cref="Time.unscaledTime"/>.
        /// </summary>
        public bool UnscaledTime { get; set; }

        /// <summary>
        /// Gets how long the shake last.
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// Gets the frequency of the shake.
        /// </summary>
        public int Frequency { get; private set; }

        /// <summary>
        /// Gets the amplitude of the shake.
        /// </summary>
        public float Amplitude { get; private set; }

        /// <summary>
        /// Gets the shake's current progress in the range 0 to 1.
        /// </summary>
        public float NormalizedTime
        {
            get
            {
                return Mathf.InverseLerp(StartTime, StartTime + Duration, CurrentTime);
            }
        }

        /// <summary>
        /// Gets whether the shake is done.
        /// </summary>
        public bool IsDone
        {
            get { return NormalizedTime >= 1f; }
        }

        /// <summary>
        /// Gets or sets the time that the shake started.
        /// </summary>
        protected float StartTime { get; set; }

        /// <summary>
        /// Gets the current scaled or unscaled time.
        /// </summary>
        protected float CurrentTime
        {
            get { return UnscaledTime ? BetterTime.UnscaledTime : BetterTime.Time; }
        }

        /// <summary>
        /// Starts (or restarts) the shake with new parameters.
        /// </summary>
        /// <param name="amplitude">Amplitude of the new shake.</param>
        /// <param name="frequency">Frequency of the new shake.</param>
        /// <param name="duration">Duration of the new shake.</param>
        public virtual void Start(float amplitude, int frequency, float duration)
        {
            Amplitude = amplitude;
            Duration = duration;
            Frequency = frequency;

            StartTime = CurrentTime;
        }

        /// <summary>
        /// Gets the amplitude at the current time.
        /// </summary>
        /// <returns>The current amplitude.</returns>
        public T GetAmplitude()
        {
            return GetAmplitude(NormalizedTime);
        }

        /// <summary>
        /// Gets the amplitude at a specific (normalized) time.
        /// </summary>
        /// <param name="t">The normalized time to get the amplitude for (0-1)</param>
        /// <returns>The amplitude at the specific time.</returns>
        public abstract T GetAmplitude(float t);
    }
}