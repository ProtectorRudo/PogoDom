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

        private Vector2 _start;
        private bool _tracking;

        public Direction CurrentDirection { get; private set; } = Direction.Up;

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
#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    _start = touch.position.ReadValue();
                    _tracking = true;
                }
                else if (_tracking && touch.press.wasReleasedThisFrame)
                {
                    CompleteSwipe(touch.position.ReadValue());
                }
                return;
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    _start = mouse.position.ReadValue();
                    _tracking = true;
                }
                else if (_tracking && mouse.leftButton.wasReleasedThisFrame)
                {
                    CompleteSwipe(mouse.position.ReadValue());
                }
            }
#else
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    _start = touch.position;
                    _tracking = true;
                }
                else if (_tracking && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
                {
                    CompleteSwipe(touch.position);
                }
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                _start = Input.mousePosition;
                _tracking = true;
            }
            else if (_tracking && Input.GetMouseButtonUp(0))
            {
                CompleteSwipe(Input.mousePosition);
            }
#endif
        }

        private void CompleteSwipe(Vector2 end)
        {
            _tracking = false;
            var delta = end - _start;
            if (delta.magnitude < minimumSwipePixels)
                return;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                CurrentDirection = delta.x >= 0f ? Direction.Right : Direction.Left;
            else
                CurrentDirection = delta.y >= 0f ? Direction.Up : Direction.Down;
        }
    }
}
