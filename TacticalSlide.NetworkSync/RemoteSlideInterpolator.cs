using UnityEngine;

namespace TacticalSlide.NetworkSync
{
    public sealed class RemoteSlideInterpolator
    {
        private Vector3 _fromPosition;
        private Vector3 _toPosition;
        private Quaternion _fromRotation = Quaternion.identity;
        private Quaternion _toRotation = Quaternion.identity;
        private float _sampleTime;
        private float _sampleDuration = 0.1f;
        private SlideNetworkState _state;
        public bool IsSliding { get { return _state.IsSliding; } }
        public float SlideProgress { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        public void PushTransform(Vector3 position, Quaternion rotation, float now, float duration)
        {
            _fromPosition = Position == default(Vector3) ? position : Position;
            _toPosition = position; _fromRotation = Rotation; _toRotation = rotation;
            _sampleTime = now; _sampleDuration = Mathf.Max(0.001f, duration);
        }
        public bool ApplyState(SlideNetworkState state, uint currentTick, float tickRate)
        {
            _state = state;
            if (state.Event != SlideNetworkEvent.StartSlide) _state.IsSliding = false;
            UpdateProgress(currentTick, tickRate); return true;
        }
        public void Render(float now, uint currentTick, float tickRate)
        {
            float t = Mathf.Clamp01((now - _sampleTime) / _sampleDuration);
            Position = Vector3.Lerp(_fromPosition, _toPosition, t);
            Rotation = Quaternion.Slerp(_fromRotation, _toRotation, t);
            UpdateProgress(currentTick, tickRate);
        }
        private void UpdateProgress(uint currentTick, float tickRate)
        {
            if (!_state.IsSliding || tickRate <= 0f || _state.DurationTicks == 0) { SlideProgress = 0f; return; }
            uint elapsed = currentTick >= _state.StartTick ? currentTick - _state.StartTick : 0u;
            SlideProgress = Mathf.Clamp01(elapsed / (float)_state.DurationTicks);
        }
    }
}
