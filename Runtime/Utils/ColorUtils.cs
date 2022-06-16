using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ColorUtils
{
    // closed match in RGB space
    public static int ClosestColorRgb(List<Color> colors, Color target)
    {
        var colorDiffs = colors.Select(n => ColorDiff(n, target)).Min(n => n);
        MyLogger.Log($"colorDiffs = {string.Join(",", colorDiffs)}");
        return colors.FindIndex(n => ColorDiff(n, target) == colorDiffs);
    }

    // distance in RGB space
    public static float ColorDiff(Color c1, Color c2)
    {
        return Mathf.Sqrt((c1.r - c2.r) * (c1.r - c2.r)
                               + (c1.g - c2.g) * (c1.g - c2.g)
                               + (c1.b - c2.b) * (c1.b - c2.b));
    }

    public static Color GetColor(XrColor xrColor)
    {
        return new Color(xrColor.R, xrColor.G, xrColor.B, xrColor.A);
    }

    //// closed match for hues only:
    //public static int ClosestColorHue(List<Color> colors, Color target)
    //{
    //    var hue1 = target.GetHue();
    //    var diffs = colors.Select(n => getHueDistance(n.GetHue(), hue1));
    //    var diffMin = diffs.Min(n => n);
    //    return diffs.ToList().FindIndex(n => n == diffMin);
    //}

    //// weighed distance using hue, saturation and brightness
    //public int ClosestColorHsb(List<Color> colors, Color target)
    //{
    //    float hue1 = target.GetHue();
    //    var num1 = ColorNum(target);
    //    var diffs = colors.Select(n => Math.Abs(ColorNum(n) - num1) +
    //                                   getHueDistance(n.GetHue(), hue1));
    //    var diffMin = diffs.Min(x => x);
    //    return diffs.ToList().FindIndex(n => n == diffMin);
    //}


    //// color brightness as perceived:
    //public static float GetBrightness(Color c)
    //{ return (c.R * 0.299f + c.g * 0.587f + c.b * 0.114f) / 256f; }

    //// distance between two hues:
    //public static float GetHueDistance(float hue1, float hue2)
    //{
    //    float d = Math.Abs(hue1 - hue2); return d > 180 ? 360 - d : d;
    //}

    ////  weighed only by saturation and brightness (from my trackbars)
    //public static float ColorNum(Color c)
    //{
    //    return c.GetSaturation() * factorSat +
    //                getBrightness(c) * factorBri;
    //}


}
