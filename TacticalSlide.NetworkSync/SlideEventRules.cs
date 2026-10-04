using System;
namespace TacticalSlide.NetworkSync
{
    public static class SlideEventRules
    {
        public static bool IsTransition(byte kind, bool local, bool before, bool after)
        {
            return local && (kind == 1 ? !before && after : (kind == 2 || kind == 3) && before && !after);
        }
        public static ushort DurationMs(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0 || seconds > 10) return 0;
            return (ushort)Math.Max(1, Math.Ceiling(seconds * 1000));
        }
        public static bool Valid(byte kind, ushort durationMs, float x, float y, float z)
        {
            return kind >= 1 && kind <= 3 && (kind != 1 || durationMs > 0) && durationMs <= 10000
                && Finite(x) && Finite(y) && Finite(z) && Math.Abs(x)<=1.01 && Math.Abs(y)<=1.01 && Math.Abs(z)<=1.01;
        }
        private static bool Finite(float v) { return !float.IsNaN(v) && !float.IsInfinity(v); }
    }
}
