using System;
using UnityEngine;
namespace TacticalSlide.NetworkSync
{
    public sealed class RemoteSlideVisualState
    {
        public readonly RemoteSlideTimeline Timeline = new RemoteSlideTimeline();
        public Vector3? Direction { get; private set; }
        public uint LastSequence { get; private set; }
        public bool Apply(SlideEventKind kind, uint sequence, Vector3 direction, double receivedAt, double durationMs)
        {
            if (durationMs < 0 || durationMs > 10000 || direction.sqrMagnitude > 1.0001f) return false;
            if (!Timeline.Accept((byte)kind, sequence, receivedAt, durationMs / 1000.0)) return false;
            LastSequence = sequence;
            if (kind == SlideEventKind.StartSlide) Direction = direction.normalized;
            return true;
        }
        public void Tick(double now, double delaySeconds) { Timeline.Sample(now, delaySeconds); }
        public void Reset() { Timeline.Reset(); Direction = null; LastSequence = 0; }
    }
}

