using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

internal static class PerformanceHarness
{
    private sealed class Candidate
    {
        internal Vector3 Position;
        internal string Name;
        internal bool Dead;
        internal bool Attached;
        internal bool Albatross;
        internal bool Seagull;
    }

    private static int SelectBaseline(List<Candidate> source, Vector3 camera, Vector3 forward)
    {
        Candidate[] allocatedSnapshot = source.ToArray();
        int best = -1;
        float bestScore = float.MaxValue;
        for (int i = 0; i < allocatedSnapshot.Length; i++)
        {
            Candidate candidate = allocatedSnapshot[i];
            if (candidate.Dead || candidate.Attached) continue;
            string lowerName = candidate.Name.ToLower();
            if (lowerName.Contains("seagull")) continue;
            Vector3 delta = candidate.Position - camera;
            float distance = delta.Length();
            Vector3 direction = distance > 0.00001f ? delta / distance : Vector3.Zero;
            float dot = Math.Max(-1f, Math.Min(1f, Vector3.Dot(Vector3.Normalize(forward), direction)));
            float angle = (float)(Math.Acos(dot) * 57.29577951308232);
            if (angle > 90f) continue;
            float score = lowerName.Contains("albatross") ? angle * 0.2f : distance + angle * 0.5f;
            if (score < bestScore) { bestScore = score; best = i; }
        }
        return best;
    }

    private static int SelectOptimized(List<Candidate> targets, Vector3 camera, Vector3 forward)
    {
        int best = -1;
        float bestScore = float.MaxValue;
        for (int i = 0; i < targets.Count; i++)
        {
            Candidate candidate = targets[i];
            if (candidate.Dead || candidate.Attached || candidate.Seagull) continue;
            Vector3 delta = candidate.Position - camera;
            float distance = delta.Length();
            Vector3 direction = distance > 0.00001f ? delta / distance : Vector3.Zero;
            float dot = Math.Max(-1f, Math.Min(1f, Vector3.Dot(Vector3.Normalize(forward), direction)));
            float angle = (float)(Math.Acos(dot) * 57.29577951308232);
            if (angle > 90f) continue;
            float score = candidate.Albatross ? angle * 0.2f : distance + angle * 0.5f;
            if (score < bestScore) { bestScore = score; best = i; }
        }
        return best;
    }

    private static void Main()
    {
        const int targetCount = 300;
        const int frames = 20000;
        var random = new Random(935);
        var targets = new List<Candidate>(targetCount);
        for (int i = 0; i < targetCount; i++)
        {
            string name = i % 41 == 0 ? "Albatross" : (i % 53 == 0 ? "Seagull" : "Fish");
            targets.Add(new Candidate {
                Position = new Vector3(random.Next(-500, 501), random.Next(-50, 101), random.Next(-500, 501)),
                Name = name,
                Dead = i % 47 == 0,
                Attached = i % 59 == 0,
                Albatross = name.IndexOf("albatross", StringComparison.OrdinalIgnoreCase) >= 0,
                Seagull = name.IndexOf("seagull", StringComparison.OrdinalIgnoreCase) >= 0
            });
        }

        var camera = new Vector3(2f, 5f, -7f);
        for (int i = 0; i < 720; i++)
        {
            float radians = (float)(i * Math.PI / 360.0);
            var forward = new Vector3((float)Math.Sin(radians), 0f, (float)Math.Cos(radians));
            int baseline = SelectBaseline(targets, camera, forward);
            int optimized = SelectOptimized(targets, camera, forward);
            if (baseline != optimized)
            {
                Console.WriteLine("SEMANTIC_EQUIVALENCE=FAIL frame={0} baseline={1} optimized={2}", i, baseline, optimized);
                Environment.Exit(1);
            }
        }
        Console.WriteLine("SEMANTIC_EQUIVALENCE=PASS cases=720");

        Vector3 fixedForward = Vector3.UnitZ;
        GC.Collect();
        long beforeBaseline = GC.GetAllocatedBytesForCurrentThread();
        var timer = Stopwatch.StartNew();
        for (int i = 0; i < frames; i++) SelectBaseline(targets, camera, fixedForward);
        timer.Stop();
        long baselineMs = timer.ElapsedMilliseconds;
        long afterBaseline = GC.GetAllocatedBytesForCurrentThread();

        GC.Collect();
        long beforeOptimized = GC.GetAllocatedBytesForCurrentThread();
        timer.Restart();
        for (int i = 0; i < frames; i++) SelectOptimized(targets, camera, fixedForward);
        timer.Stop();
        long optimizedMs = timer.ElapsedMilliseconds;
        long afterOptimized = GC.GetAllocatedBytesForCurrentThread();

        Console.WriteLine("BASELINE elapsed_ms={0} allocated_bytes={1}", baselineMs, afterBaseline - beforeBaseline);
        Console.WriteLine("MODIFIED elapsed_ms={0} allocated_bytes={1}", optimizedMs, afterOptimized - beforeOptimized);
        Console.WriteLine("SPEEDUP={0:F2}x", (double)baselineMs / Math.Max(1L, optimizedMs));
    }
}
