using System;
using TacticalSlide.NetworkSync;
using UnityEngine;

internal static class SlideNetworkStateSelfTest
{
    private static int Main()
    {
        var start = SlideNetworkState.Start(100u, (ushort)Math.Round(0.85 * 64), new Vector3(2f, 3f, 0f), 7);
        if (start.DurationTicks != 54 || !start.IsSliding || start.Direction != Vector3.right) return Fail("start conversion");
        byte[] bytes = start.Serialize();
        if (!SlideNetworkState.TryDeserialize(bytes, out var roundTrip) || !roundTrip.Equals(start)) return Fail("roundtrip");
        var tracker = new SlideNetworkStateTracker();
        if (!tracker.TryApply(start)) return Fail("first apply");
        if (tracker.TryApply(start)) return Fail("duplicate");
        if (tracker.TryApply(SlideNetworkState.End(101u, 6))) return Fail("older sequence");
        if (!tracker.TryApply(SlideNetworkState.End(101u, 8))) return Fail("newer sequence");
        var interp = new RemoteSlideInterpolator();
        interp.ApplyState(start, 127u, 64f);
        if (Math.Abs(interp.SlideProgress - 0.5f) > 0.001f) return Fail("progress");
        Console.WriteLine("TASK3_OK");
        Console.WriteLine("DurationTicks=54");
        Console.WriteLine("RoundTrip=PASS");
        Console.WriteLine("DuplicateAndOrdering=PASS");
        Console.WriteLine("Progress=0.5");
        return 0;
    }
    private static int Fail(string reason) { Console.WriteLine("TASK3_FAIL=" + reason); return 1; }
}
