using UnityEngine;

namespace Archon.SwissArmyLib.Utils.Shake
{
    public class Shake2D : BaseShake<Vector2>
    {
        public readonly Shake Horizontal = new Shake(), 
            Vertical = new Shake();

        public override void Start(float amplitude, int frequency, float duration)
        {
            base.Start(amplitude, frequency, duration);

            Horizontal.Start(amplitude, frequency, duration);
            Vertical.Start(amplitude, frequency, duration);
        }

        public override Vector2 GetAmplitude(float t)
        {
            return new Vector2(Horizontal.GetAmplitude(t), Vertical.GetAmplitude(t));
        }
    }
}