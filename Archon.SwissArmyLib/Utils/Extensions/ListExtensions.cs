using System;
using System.Collections.Generic;

namespace Archon.SwissArmyLib.Utils.Extensions
{
    /// <summary>
    /// Extensions for List
    /// </summary>
    public static class ListExtensions
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Shuffles the list using Fisher–Yates shuffle algorithm.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list to shuffle.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
