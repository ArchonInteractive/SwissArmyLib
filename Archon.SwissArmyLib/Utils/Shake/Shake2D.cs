using UnityEngine;

namespace Archon.SwissArmyLib.Utils.Shake
{
    /// <summary>
    /// An object that can shake a <see cref="Vector2"/> over time.
    /// </summary>
    public class Shake2D : BaseShake<Vector2>
    {
        /// <summary>
        /// The shake used for shaking the <see cref="Vector2.x"/> value of the <see cref="Vector2"/>.
        /// </summary>
        public readonly Shake Horizontal = new Shake();

        /// <summary>
        /// The shake used for shaking the <see cref="Vector2.y"/> value of the <see cref="Vector2"/>.
        /// </summary>
        public readonly Shake Vertical = new Shake();

        /// <inheritdoc />
        public override void Start(float amplitude, int frequency, float duration)
        {
            base.Start(amplitude, frequency, duration);

            Horizontal.Start(amplitude, frequency, duration);
            Vertical.Start(amplitude, frequency, duration);
        }

        /// <inheritdoc />
        public override Vector2 GetAmplitude(float t)
        {
            return new Vector2(Horizontal.GetAmplitude(t), Vertical.GetAmplitude(t));
        }
    }
}