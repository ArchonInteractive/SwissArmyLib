using UnityEngine;

namespace Archon.SwissArmyLib.Utils
{
    /// <summary>
    /// Utility methods for <see cref="Color"/>.
    /// </summary>
    public static class ColorUtils
    {
        /// <summary>
        /// Converts the given color to its equivalent hex color in the form of #RRGGBBAA (eg. #000000FF for opaque black).
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The hex representation of <paramref name="color"/>.</returns>
        public static string ToHex(this Color color)
        {
            Color32 c = color;
            return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", c.r, c.g, c.b, c.a);
        }

        /// <summary>
        /// Wraps the supplied string in rich text color tags.
        /// </summary>
        /// <param name="text">The text to be colored.</param>
        /// <param name="color">The color to make the text.</param>
        /// <returns><paramref name="text"/> wrapped in color tags.</returns>
        public static string RichTextColor(string text, Color color)
        {
            return string.Format("<color=#{0}>{1}</color>", color.ToHex(), text);
        }
    }
}
