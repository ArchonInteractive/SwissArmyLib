using UnityEngine;

namespace Archon.SwissArmyLib.Utils
{
    public static class ColorUtils
    {
        public static string ToHex(this Color color)
        {
            Color32 c = color;
            return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", c.r, c.g, c.b, c.a);
        }

        public static string RichTextColor(string text, Color color)
        {
            return string.Format("<color=#{0}>{1}</color>", color.ToHex(), text);
        }

        public static Color LerpAlpha(Color color, float targetAlpha, float t)
        {
            var targetColor = color;
            targetColor.a = targetAlpha;
            return Color.Lerp(color, targetColor, t);
        }
    }
}
