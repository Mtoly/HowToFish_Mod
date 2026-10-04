using System;
using System.IO;
using UnityEngine;

namespace TacticalSlide.NetworkSync
{
    public enum SlideNetworkEvent : byte { StartSlide = 1, SlideHop = 2, EndSlide = 3 }

    public struct SlideNetworkState : IEquatable<SlideNetworkState>
    {
        public const byte CurrentVersion = 1;
        public SlideNetworkEvent Event;
        public bool IsSliding;
        public Vector3 Direction;
        public uint StartTick;
        public ushort DurationTicks;
        public bool IsSlideHop;
        public ushort Sequence;

        public static SlideNetworkState Start(uint tick, ushort durationTicks, Vector3 direction, ushort sequence)
        {
            return new SlideNetworkState { Event = SlideNetworkEvent.StartSlide, IsSliding = true, Direction = Normalize(direction), StartTick = tick, DurationTicks = durationTicks, Sequence = sequence };
        }
        public static SlideNetworkState Hop(uint tick, Vector3 direction, ushort sequence)
        {
            return new SlideNetworkState { Event = SlideNetworkEvent.SlideHop, Direction = Normalize(direction), StartTick = tick, IsSlideHop = true, Sequence = sequence };
        }
        public static SlideNetworkState End(uint tick, ushort sequence)
        {
            return new SlideNetworkState { Event = SlideNetworkEvent.EndSlide, Direction = Vector3.forward, StartTick = tick, Sequence = sequence };
        }
        public bool IsNewerThan(ushort lastSequence)
        {
            return Sequence != lastSequence && (ushort)(Sequence - lastSequence) < 32768;
        }
        public byte[] Serialize()
        {
            using (var stream = new MemoryStream(24))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(CurrentVersion); writer.Write((byte)Event); writer.Write(IsSliding);
                writer.Write(Direction.x); writer.Write(Direction.y); writer.Write(Direction.z);
                writer.Write(StartTick); writer.Write(DurationTicks); writer.Write(IsSlideHop); writer.Write(Sequence);
                return stream.ToArray();
            }
        }
        public static bool TryDeserialize(byte[] payload, out SlideNetworkState state)
        {
            state = default(SlideNetworkState);
            if (payload == null || payload.Length != 24) return false;
            try
            {
                using (var stream = new MemoryStream(payload, false))
                using (var reader = new BinaryReader(stream))
                {
                    if (reader.ReadByte() != CurrentVersion) return false;
                    state.Event = (SlideNetworkEvent)reader.ReadByte();
                    if (!Enum.IsDefined(typeof(SlideNetworkEvent), state.Event)) return false;
                    state.IsSliding = reader.ReadBoolean();
                    state.Direction = Normalize(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                    state.StartTick = reader.ReadUInt32(); state.DurationTicks = reader.ReadUInt16();
                    state.IsSlideHop = reader.ReadBoolean(); state.Sequence = reader.ReadUInt16();
                    return stream.Position == stream.Length;
                }
            }
            catch { return false; }
        }
        public bool Equals(SlideNetworkState other)
        {
            return Event == other.Event && IsSliding == other.IsSliding && Direction == other.Direction && StartTick == other.StartTick && DurationTicks == other.DurationTicks && IsSlideHop == other.IsSlideHop && Sequence == other.Sequence;
        }
        public override bool Equals(object obj) { return obj is SlideNetworkState && Equals((SlideNetworkState)obj); }
        public override int GetHashCode() { return (int)Event ^ Sequence ^ (int)StartTick; }
        private static Vector3 Normalize(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }
    }

    public sealed class SlideNetworkStateTracker
    {
        private ushort _lastSequence;
        private bool _initialized;
        public SlideNetworkState Current { get; private set; }
        public bool TryApply(SlideNetworkState incoming)
        {
            if (_initialized && !incoming.IsNewerThan(_lastSequence)) return false;
            _initialized = true; _lastSequence = incoming.Sequence; Current = incoming; return true;
        }
    }
}
