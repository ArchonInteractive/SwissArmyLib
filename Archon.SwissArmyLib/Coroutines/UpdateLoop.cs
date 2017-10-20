namespace Archon.SwissArmyLib.Coroutines
{
    /// <summary>
    /// Unity's update loops.
    /// </summary>
    public enum UpdateLoop
    {
#pragma warning disable 1591
        Update,
        LateUpdate,
        FixedUpdate,
        FrameIntervalUpdate,
        TimeIntervalUpdate
#pragma warning restore 1591
    }
}
