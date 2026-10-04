using System;
namespace TacticalSlide.NetworkSync
{
    // Pure presentation state: no Unity, network, input or Rigidbody dependencies.
    // One instance per remote player. Reset on disconnect or object replacement.
    public sealed class RemoteSlideTimeline
    {
        private uint sequence;
        private bool hasSequence;
        private double start, end, lastSample;
        private bool started, sampled;
        private float weight;
        public float Weight { get { return weight; } }
        public float Progress { get; private set; }
        public bool Active { get; private set; }
        public bool Accept(byte kind, uint seq, double receivedAt, double durationSeconds)
        {
            if (kind < 1 || kind > 3 || !Finite(receivedAt) || !Finite(durationSeconds)) return false;
            if (kind == 1 && (durationSeconds <= 0 || durationSeconds > 10)) return false;
            if (hasSequence && unchecked((int)(seq - sequence)) <= 0) return false;
            sequence = seq; hasSequence = true;
            if (kind == 1) { start = receivedAt; end = receivedAt + durationSeconds; started = true; }
            else if (started) end = Math.Min(end, receivedAt);
            return true;
        }
        public void Sample(double now, double renderDelaySeconds)
        {
            if (!Finite(now) || !Finite(renderDelaySeconds)) return;
            double time = now - Math.Max(0, renderDelaySeconds);
            double dt = sampled ? Math.Max(0, time - lastSample) : 0;
            if (sampled && time < lastSample) return;
            lastSample = time; sampled = true;
            Active = started && time >= start && time < end;
            Progress = !started || time < start ? 0 : (float)Math.Min(1, (time - start) / Math.Max(0.0001, end - start));
            // Time-based exponential blend: same result at 30/60/144 FPS for a held state.
            float target = Active ? 1f : 0f;
            weight = target + (weight - target) * (float)Math.Exp(-dt / 0.07);
        }
        public void Reset()
        {
            sequence = 0; hasSequence = false; started = sampled = Active = false;
            start = end = lastSample = 0; weight = Progress = 0;
        }
        private static bool Finite(double v) { return !double.IsNaN(v) && !double.IsInfinity(v); }
    }
}
