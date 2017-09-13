namespace Archon.SwissArmyLib.Coroutines
{
    internal sealed class WaitForSecondsLite
    {
        internal static readonly WaitForSecondsLite Instance = new WaitForSecondsLite();

        public float Duration;
        public bool Unscaled;

        private WaitForSecondsLite() { }
    }
}