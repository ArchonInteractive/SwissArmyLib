using UnityEngine;

namespace Archon.SwissArmyLib.Gravity
{
    /// <summary>
    /// Represents a point with a gravitational pull on entities.
    /// </summary>
    public interface IGravitationalPoint
    {
        /// <summary>
        /// Calculates how much gravitational pull is at a specific location caused by this point.
        /// </summary>
        /// <param name="location">The location to test.</param>
        /// <returns>A vector representing the force at <paramref name="location"/>.</returns>
        Vector3 GetForceAt(Vector3 location);
    }
}