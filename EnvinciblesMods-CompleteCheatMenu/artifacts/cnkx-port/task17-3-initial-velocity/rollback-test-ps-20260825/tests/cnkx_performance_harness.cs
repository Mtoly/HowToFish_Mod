using System;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

internal static class CnkxPerformanceHarness
{
    private const int CandidateCount = 300;
    private const int Iterations = 10000;
    private const int MaxBones = 8;

    private sealed class Candidate
    {
        internal int Id;
        internal float Distance;
        internal float Angle;
        internal float ScreenDistance;
        internal bool Visible;
        internal bool Priority;
        internal Vector3 Point;
        internal Vector3 Velocity;
        internal Vector2[] Bones;
    }

    private struct Measurement
    {
        internal long ElapsedTicks;
        internal long AllocatedBytes;
        internal int Checksum;
    }

    private static Candidate[] _candidates;
    private static int _sink;

    private static int Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        _candidates = BuildCandidates();

        for (int i = 0; i < 1000; i++)
        {
            _sink ^= OldAcquire(_candidates);
            _sink ^= NewAcquire(_candidates, false);
            _sink ^= SelectBone(_candidates[i % CandidateCount]);
            _sink ^= PredictChecksum(_candidates[i % CandidateCount]);
        }

        Measurement oldAcquire = MeasureOldAcquire();
        Measurement newAcquire = MeasureNewAcquire();
        Measurement boneSelection = MeasureBoneSelection();
        Measurement prediction = MeasurePrediction();

        int oldBaseline = OldAcquire(_candidates);
        int newBaseline = NewAcquire(_candidates, false);
        int enhanced = NewAcquire(_candidates, true);
        int repeat = NewAcquire(_candidates, true);
        bool finitePrediction = PredictionFinite();
        bool baselineMatch = oldBaseline == newBaseline;
        bool enhancedChanged = enhanced != newBaseline;
        bool deterministic = enhanced == repeat && oldAcquire.Checksum == 17 * Iterations && newAcquire.Checksum == 211 * Iterations;

        Console.WriteLine("HARNESS=CNKX_PERFORMANCE_V1");
        Console.WriteLine("RUNTIME=" + Environment.Version);
        Console.WriteLine("OS=" + Environment.OSVersion);
        Console.WriteLine("CANDIDATES=" + CandidateCount);
        Console.WriteLine("BONES_PER_TARGET=3-8");
        Console.WriteLine("ITERATIONS=" + Iterations);
        PrintMeasurement("OLD_FIXED_MARKER_ACQUIRE", oldAcquire);
        PrintMeasurement("NEW_SCORER_ACQUIRE", newAcquire);
        PrintMeasurement("BONE_POINT_SELECTION", boneSelection);
        PrintMeasurement("BALLISTIC_PREDICTION", prediction);
        Console.WriteLine("OLD_BASELINE_TARGET=" + oldBaseline);
        Console.WriteLine("NEW_BASELINE_TARGET=" + newBaseline);
        Console.WriteLine("NEW_ENHANCED_TARGET=" + enhanced);
        Console.WriteLine("SEMANTIC_BASELINE_MATCH=" + baselineMatch);
        Console.WriteLine("ENHANCED_WEIGHTING_CHANGES_RESULT=" + enhancedChanged);
        Console.WriteLine("DETERMINISTIC_RESULT=" + deterministic);
        Console.WriteLine("PREDICTION_FINITE=" + finitePrediction);
        Console.WriteLine("SINK=" + _sink);

        bool pass = baselineMatch && enhancedChanged && deterministic && finitePrediction;
        Console.WriteLine("HARNESS_RESULT=" + (pass ? "PASS" : "FAIL"));
        return pass ? 0 : 1;
    }

    private static Candidate[] BuildCandidates()
    {
        Random random = new Random(935);
        Candidate[] result = new Candidate[CandidateCount];
        for (int i = 0; i < result.Length; i++)
        {
            int boneCount = 3 + i % 6;
            Vector2[] bones = new Vector2[boneCount];
            for (int b = 0; b < boneCount; b++)
            {
                bones[b] = new Vector2(
                    (float)(random.NextDouble() * 700.0 - 350.0),
                    (float)(random.NextDouble() * 400.0 - 200.0));
            }
            float distance = 5f + (float)random.NextDouble() * 190f;
            float angle = 0.2f + (float)random.NextDouble() * 34f;
            float screen = 5f + (float)random.NextDouble() * 240f;
            result[i] = new Candidate
            {
                Id = i,
                Distance = distance,
                Angle = angle,
                ScreenDistance = screen,
                Visible = i % 11 != 0,
                Priority = false,
                Point = new Vector3(10f + i * 0.11f, 1f + (i % 9) * 0.07f, 15f + i * 0.05f),
                Velocity = new Vector3((i % 13) - 6f, (i % 5) * 0.15f, (i % 17) - 8f),
                Bones = bones
            };
        }

        // A deterministic baseline winner and a distinct enhanced winner.
        result[17].Angle = 0.01f;
        result[17].Distance = 90f;
        result[17].ScreenDistance = 110f;
        result[17].Visible = true;
        result[211].Angle = 0.5f;
        result[211].Distance = 8f;
        result[211].ScreenDistance = 4f;
        result[211].Visible = true;
        result[211].Priority = true;
        return result;
    }

    private static int OldAcquire(Candidate[] candidates)
    {
        int best = -1;
        float bestAngle = float.PositiveInfinity;
        for (int i = 0; i < candidates.Length; i++)
        {
            Candidate candidate = candidates[i];
            if (!candidate.Visible || candidate.Distance < 0f || candidate.Distance > 200f || candidate.Angle < 0f || candidate.Angle > 35f)
                continue;
            if (candidate.Angle < bestAngle)
            {
                bestAngle = candidate.Angle;
                best = candidate.Id;
            }
        }
        return best;
    }

    private static int NewAcquire(Candidate[] candidates, bool enhanced)
    {
        int best = -1;
        float bestScore = float.PositiveInfinity;
        for (int i = 0; i < candidates.Length; i++)
        {
            Candidate candidate = candidates[i];
            if (!candidate.Visible || candidate.Distance < 0f || candidate.Distance > 200f || candidate.Angle < 0f || candidate.Angle > 35f || candidate.ScreenDistance > 250f)
                continue;
            float score = enhanced
                ? candidate.Distance + candidate.Angle * 0.5f + candidate.ScreenDistance * 0.25f - (candidate.Priority ? 50f : 0f)
                : candidate.Angle;
            if (score < bestScore)
            {
                bestScore = score;
                best = candidate.Id;
            }
        }
        return best;
    }

    private static int SelectBone(Candidate candidate)
    {
        int best = -1;
        float bestScore = float.PositiveInfinity;
        Vector2[] bones = candidate.Bones;
        for (int i = 0; i < bones.Length; i++)
        {
            float score = bones[i].LengthSquared();
            if (score < bestScore)
            {
                bestScore = score;
                best = i;
            }
        }
        return best;
    }

    private static Vector3 Predict(Candidate candidate)
    {
        const float projectileSpeed = 300f;
        Vector3 origin = Vector3.Zero;
        Vector3 point = candidate.Point;
        Vector3 velocity = candidate.Velocity;
        float flightTime = Vector3.Distance(origin, point) / projectileSpeed;
        for (int i = 0; i < 2; i++)
        {
            Vector3 estimate = point + velocity * flightTime;
            flightTime = Vector3.Distance(origin, estimate) / projectileSpeed;
        }
        return point + velocity * flightTime;
    }

    private static int PredictChecksum(Candidate candidate)
    {
        Vector3 predicted = Predict(candidate);
        return (int)(predicted.X * 31f + predicted.Y * 17f + predicted.Z * 13f);
    }

    private static Measurement MeasureOldAcquire()
    {
        return Measure(delegate
        {
            int value = 0;
            for (int i = 0; i < Iterations; i++) value += OldAcquire(_candidates);
            return value;
        });
    }

    private static Measurement MeasureNewAcquire()
    {
        return Measure(delegate
        {
            int value = 0;
            for (int i = 0; i < Iterations; i++) value += NewAcquire(_candidates, true);
            return value;
        });
    }

    private static Measurement MeasureBoneSelection()
    {
        return Measure(delegate
        {
            int value = 0;
            for (int i = 0; i < Iterations; i++)
                for (int c = 0; c < _candidates.Length; c++) value += SelectBone(_candidates[c]);
            return value;
        });
    }

    private static Measurement MeasurePrediction()
    {
        return Measure(delegate
        {
            int value = 0;
            for (int i = 0; i < Iterations; i++)
                for (int c = 0; c < _candidates.Length; c++) value = unchecked(value + PredictChecksum(_candidates[c]));
            return value;
        });
    }

    private delegate int Work();

    private static Measurement Measure(Work work)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        Stopwatch stopwatch = Stopwatch.StartNew();
        int checksum = work();
        stopwatch.Stop();
        long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        _sink ^= checksum;
        return new Measurement
        {
            ElapsedTicks = stopwatch.ElapsedTicks,
            AllocatedBytes = allocatedAfter - allocatedBefore,
            Checksum = checksum
        };
    }

    private static bool PredictionFinite()
    {
        for (int i = 0; i < _candidates.Length; i++)
        {
            Vector3 value = Predict(_candidates[i]);
            if (float.IsNaN(value.X) || float.IsInfinity(value.X) ||
                float.IsNaN(value.Y) || float.IsInfinity(value.Y) ||
                float.IsNaN(value.Z) || float.IsInfinity(value.Z)) return false;
        }
        return true;
    }

    private static void PrintMeasurement(string name, Measurement measurement)
    {
        double milliseconds = measurement.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
        Console.WriteLine(name + "_TOTAL_MS=" + milliseconds.ToString("0.000"));
        Console.WriteLine(name + "_PER_ITER_US=" + (milliseconds * 1000.0 / Iterations).ToString("0.000000"));
        Console.WriteLine(name + "_ALLOCATED_BYTES=" + measurement.AllocatedBytes);
        Console.WriteLine(name + "_CHECKSUM=" + measurement.Checksum);
    }
}
