using PogoDom.Core;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PogoDom.Runtime
{
    public sealed class SwipeDirectionInput : MonoBehaviour
    {
        [SerializeField] private float minimumSwipePixels = 45f;

        private SwipeGestureTracker _gesture;

        public Direction CurrentDirection { get; private set; } = Direction.Up;

        private void Awake()
        {
            RebuildGestureTracker();
        }

        private void OnValidate()
        {
            if (minimumSwipePixels < 1f) minimumSwipePixels = 1f;
            if (Application.isPlaying) RebuildGestureTracker();
        }

        public void ResetDirection(Direction direction = Direction.Up)
        {
            CurrentDirection = direction;
            if (_gesture == null) RebuildGestureTracker();
            else _gesture.End();
        }

        private void RebuildGestureTracker()
        {
            _gesture = new SwipeGestureTracker(Mathf.Max(1f, minimumSwipePixels));
        }

        private void Update()
        {
            ReadKeyboard();
            ReadPointer();
        }

        private void ReadKeyboard()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) CurrentDirection = Direction.Up;
            else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) CurrentDirection = Direction.Right;
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) CurrentDirection = Direction.Down;
            else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) CurrentDirection = Direction.Left;
#else
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) CurrentDirection = Direction.Up;
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) CurrentDirection = Direction.Right;
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) CurrentDirection = Direction.Down;
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) CurrentDirection = Direction.Left;
#endif
        }

        private void ReadPointer()
        {
            if (_gesture == null) RebuildGestureTracker();

#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                var position = touch.position.ReadValue();
                if (touch.press.wasPressedThisFrame)
                    _gesture.Begin(position.x, position.y);

                if (_gesture.IsTracking && touch.press.isPressed)
                    CommitIfReady(position);

                if (_gesture.IsTracking && touch.press.wasReleasedThisFrame)
                {
                    CommitIfReady(position);
                    _gesture.End();
                }
                return;
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                var position = mouse.position.ReadValue();
                if (mouse.leftButton.wasPressedThisFrame)
                    _gesture.Begin(position.x, position.y);

                if (_gesture.IsTracking && mouse.leftButton.isPressed)
                    CommitIfReady(position);

                if (_gesture.IsTracking && mouse.leftButton.wasReleasedThisFrame)
                {
                    CommitIfReady(position);
                    _gesture.End();
                }
            }
#else
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                    _gesture.Begin(touch.position.x, touch.position.y);

                if (_gesture.IsTracking && (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary))
                    CommitIfReady(touch.position);

                if (_gesture.IsTracking && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
                {
                    CommitIfReady(touch.position);
                    _gesture.End();
                }
                return;
            }

            var mousePosition = (Vector2)Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
                _gesture.Begin(mousePosition.x, mousePosition.y);

            if (_gesture.IsTracking && Input.GetMouseButton(0))
                CommitIfReady(mousePosition);

            if (_gesture.IsTracking && Input.GetMouseButtonUp(0))
            {
                CommitIfReady(mousePosition);
                _gesture.End();
            }
#endif
        }

        private void CommitIfReady(Vector2 position)
        {
            Direction direction;
            if (_gesture.TryUpdate(position.x, position.y, out direction))
                CurrentDirection = direction;
        }
    }
}
