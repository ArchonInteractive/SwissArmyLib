namespace Archon.SwissArmyLib.Events
{
    /// <summary>
    ///     Contains the event ids used by SwissArmyLib.
    /// </summary>
    public static class BuiltinEventIds
    {
        /// <summary>
        ///     ManagedUpdate
        /// </summary>
        public const int
            Update = -1000,
            LateUpdate = -1001,
            FixedUpdate = -1002;

        /// <summary>
        ///     ResourcePool
        /// </summary>
        public const int
            PreChange = -8000,
            Change = -8001,
            Empty = -8002,
            Full = -8003,
            Renew = -8004;
    }
}
