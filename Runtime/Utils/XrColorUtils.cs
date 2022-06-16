using UnityEngine;

public static class XrColorUtils
{
    public static Color GetColor(string hex)
    {
        Color color;

        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }

        return Color.white;
    }
}
