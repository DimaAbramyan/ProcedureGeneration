using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColourComparison
{
    static float extentColor = 0.2f;
    public static bool ColourCheck(Color color)
    {
        return (color.r > extentColor &&
            color.g > extentColor &&
            color.b > extentColor);
    }
    public static bool ColourCheck(Color color, float ExtentColour)
    {
        return (color.r > ExtentColour &&
            color.g > ExtentColour &&
            color.b > ExtentColour);
    }
    public static bool ColourCheck(Color color, Func<float, bool> pred)
    {
        return (pred(color.r) &&
            pred(color.g) &&
            pred(color.b));
    }
    public static bool ColourCheck(Color color,  float ExtentColour, Func<float, float, bool> pred)
    {
        return (pred(color.r, extentColor) &&
            pred(color.g, extentColor) &&
            pred(color.b, extentColor));
    }
}
