using System;

namespace PogoDom.Core
{
    /// <summary>
    /// Engine-independent one-thumb swipe recognizer. A gesture commits exactly
    /// once, as soon as it crosses the distance threshold, so the next 0.5 s
    /// battle tick can consume the new direction without waiting for finger-up.
    /// </summary>
    public sealed class SwipeGestureTracker
    {
        private readonly float _minimumDistanceSquared;
        private float _startX;
        private float _startY;
        private bool _tracking;
        private bool _committed;

        public bool IsTracking => _tracking;
        public bool HasCommitted => _committed;

        public SwipeGestureTracker(float minimumDistance)
        {
            if (minimumDistance <= 0f)
                throw new ArgumentOutOfRangeException(nameof(minimumDistance));
            _minimumDistanceSquared = minimumDistance * minimumDistance;
        }

        public void Begin(float x, float y)
        {
            _startX = x;
            _startY = y;
            _tracking = true;
            _committed = false;
        }

        public bool TryUpdate(float x, float y, out Direction direction)
        {
            direction = Direction.None;
            if (!_tracking || _committed)
                return false;

            var dx = x - _startX;
            var dy = y - _startY;
            if (dx * dx + dy * dy < _minimumDistanceSquared)
                return false;

            direction = ResolveCardinal(dx, dy);
            _committed = true;
            return true;
        }

        public void End()
        {
            _tracking = false;
            _committed = false;
        }

        public static Direction ResolveCardinal(float dx, float dy)
        {
            if (dx == 0f && dy == 0f)
                return Direction.None;

            // Horizontal wins an exact 45-degree tie, matching the audited
            // Cocos implementation where <= PI/4 resolves to Right/Left.
            if (Math.Abs(dx) >= Math.Abs(dy))
                return dx >= 0f ? Direction.Right : Direction.Left;
            return dy >= 0f ? Direction.Up : Direction.Down;
        }
    }
}
