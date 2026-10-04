#nullable enable
using System;
namespace TacticalSlide.NetworkSync
{
    // One registration per exact manager instance. Failed attachment is retried next poll.
    public sealed class ManagerBinding<T> where T : class
    {
        private T? current;
        public T? Current { get { return current; } }
        public bool Bind(T? next, Action<T> attach, Action<T> detach)
        {
            if (Object.ReferenceEquals(current, next)) return false;
            if (current != null) { detach(current); current = null; }
            if (next != null) { attach(next); current = next; }
            return true;
        }
        public void Clear(Action<T> detach) { Bind(null, delegate(T unused) {}, detach); }
    }
}

