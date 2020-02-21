using System;
using UnityEngine;

namespace Assets
{
    [Serializable]
    public struct Limits
    {
        public float Min;
        public float Max;

        public float Diff => Max - Min;

        public float Lerp(float step)
        {
            return Mathf.Lerp(Min, Max, step);
        }
    }

    public static class Extensions
    {
        public static float Clamp(this float value, Limits limits)
        {
            return Mathf.Clamp(value, limits.Min, limits.Max);
        }
    }
}
