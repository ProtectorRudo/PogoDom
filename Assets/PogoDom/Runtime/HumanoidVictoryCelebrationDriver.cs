using System;
using PogoDom.Cosmetics;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Presentation-only adapter for Unity Humanoid avatars. It layers a
    /// deterministic PogoDom celebration pose over a captured baseline by using
    /// Mecanim's HumanPose muscles, so one choreography can retarget across many
    /// characters without a bespoke AnimationClip per skin.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HumanoidVictoryCelebrationDriver : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [Tooltip("Optional visual child root. Leave empty to avoid moving the gameplay root.")]
        [SerializeField] private Transform presentationRoot;
        [SerializeField, Range(0.25f, 1.5f)] private float intensity = 1f;
        [SerializeField, Range(0f, 0.25f)] private float visualBobMeters = 0.08f;
        [SerializeField, Range(0f, 15f)] private float visualYawDegrees = 7f;
        [SerializeField, Range(0f, 12f)] private float visualLeanDegrees = 6f;

        private HumanPoseHandler _handler;
        private HumanPose _pose;
        private float[] _baselineMuscles;
        private VictoryCelebrationDefinition _active;
        private float _startedAt;
        private Vector3 _rootBasePosition;
        private Quaternion _rootBaseRotation;
        private bool _playing;

        private int _leftShoulderDownUp = -1;
        private int _rightShoulderDownUp = -1;
        private int _leftArmDownUp = -1;
        private int _rightArmDownUp = -1;
        private int _leftForearmStretch = -1;
        private int _rightForearmStretch = -1;
        private int _leftForearmTwist = -1;
        private int _rightForearmTwist = -1;
        private int _leftHandDownUp = -1;
        private int _rightHandDownUp = -1;
        private int _leftHandInOut = -1;
        private int _rightHandInOut = -1;

        public bool IsPlaying => _playing;
        public CosmeticId ActiveCelebrationId => _active == null ? default(CosmeticId) : _active.Id;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        public bool PlayAura67()
        {
            return Play(VictoryCelebrationLibrary.Aura67());
        }

        public bool Play(VictoryCelebrationDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (!EnsureHumanoid()) return false;

            CaptureBaseline();
            _active = definition;
            _startedAt = Time.unscaledTime;
            _playing = true;
            if (presentationRoot != null)
            {
                _rootBasePosition = presentationRoot.localPosition;
                _rootBaseRotation = presentationRoot.localRotation;
            }
            return true;
        }

        public void Stop()
        {
            if (!_playing) return;
            RestoreBaseline();
            _playing = false;
            _active = null;
        }

        private void LateUpdate()
        {
            if (!_playing || _active == null) return;
            var elapsed = Time.unscaledTime - _startedAt;
            if (elapsed >= _active.DurationSeconds)
            {
                Stop();
                return;
            }

            var sample = VictoryCelebrationSampler.Sample(_active, elapsed);
            ApplyMuscles(sample);
            ApplyVisualRoot(sample);
        }

        private bool EnsureHumanoid()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                return false;

            if (_handler == null)
            {
                _handler = new HumanPoseHandler(animator.avatar, animator.transform);
                ResolveMuscles();
            }
            return true;
        }

        private void CaptureBaseline()
        {
            _pose = new HumanPose();
            _handler.GetHumanPose(ref _pose);
            _baselineMuscles = new float[_pose.muscles.Length];
            Array.Copy(_pose.muscles, _baselineMuscles, _baselineMuscles.Length);
        }

        private void RestoreBaseline()
        {
            if (_handler != null && _baselineMuscles != null)
            {
                _handler.GetHumanPose(ref _pose);
                Array.Copy(_baselineMuscles, _pose.muscles, Math.Min(_baselineMuscles.Length, _pose.muscles.Length));
                _handler.SetHumanPose(ref _pose);
            }
            if (presentationRoot != null)
            {
                presentationRoot.localPosition = _rootBasePosition;
                presentationRoot.localRotation = _rootBaseRotation;
            }
        }

        private void ApplyMuscles(VictoryCelebrationPose sample)
        {
            _handler.GetHumanPose(ref _pose);
            Array.Copy(_baselineMuscles, _pose.muscles, Math.Min(_baselineMuscles.Length, _pose.muscles.Length));

            var k = intensity;
            Add(_leftShoulderDownUp, sample.LeftArmLift * 0.16f * k);
            Add(_rightShoulderDownUp, sample.RightArmLift * 0.16f * k);
            Add(_leftArmDownUp, sample.LeftArmLift * 0.58f * k);
            Add(_rightArmDownUp, sample.RightArmLift * 0.58f * k);
            Add(_leftForearmStretch, -sample.LeftForearmOpen * 0.34f * k);
            Add(_rightForearmStretch, -sample.RightForearmOpen * 0.34f * k);

            // Mirrored forearm twist presents both palms upward instead of
            // rotating both wrists in the same anatomical direction.
            Add(_leftForearmTwist, sample.LeftPalmUp * 0.62f * k);
            Add(_rightForearmTwist, -sample.RightPalmUp * 0.62f * k);
            Add(_leftHandDownUp, sample.LeftPalmUp * 0.08f * k);
            Add(_rightHandDownUp, sample.RightPalmUp * 0.08f * k);
            Add(_leftHandInOut, sample.BodyLean * 0.08f * k);
            Add(_rightHandInOut, -sample.BodyLean * 0.08f * k);

            _handler.SetHumanPose(ref _pose);
        }

        private void ApplyVisualRoot(VictoryCelebrationPose sample)
        {
            if (presentationRoot == null) return;
            presentationRoot.localPosition = _rootBasePosition + Vector3.up * (sample.BodyBob * visualBobMeters * intensity);
            presentationRoot.localRotation = _rootBaseRotation * Quaternion.Euler(
                0f,
                sample.BodyYaw * visualYawDegrees * intensity,
                sample.BodyLean * visualLeanDegrees * intensity);
        }

        private void Add(int muscleIndex, float delta)
        {
            if (muscleIndex < 0 || muscleIndex >= _pose.muscles.Length) return;
            _pose.muscles[muscleIndex] = Mathf.Clamp(_pose.muscles[muscleIndex] + delta, -1f, 1f);
        }

        private void ResolveMuscles()
        {
            _leftShoulderDownUp = FindMuscle("Left Shoulder Down-Up");
            _rightShoulderDownUp = FindMuscle("Right Shoulder Down-Up");
            _leftArmDownUp = FindMuscle("Left Arm Down-Up");
            _rightArmDownUp = FindMuscle("Right Arm Down-Up");
            _leftForearmStretch = FindMuscle("Left Forearm Stretch");
            _rightForearmStretch = FindMuscle("Right Forearm Stretch");
            _leftForearmTwist = FindMuscle("Left Forearm Twist In-Out");
            _rightForearmTwist = FindMuscle("Right Forearm Twist In-Out");
            _leftHandDownUp = FindMuscle("Left Hand Down-Up");
            _rightHandDownUp = FindMuscle("Right Hand Down-Up");
            _leftHandInOut = FindMuscle("Left Hand In-Out");
            _rightHandInOut = FindMuscle("Right Hand In-Out");
        }

        private static int FindMuscle(string name)
        {
            var names = HumanTrait.MuscleName;
            for (var i = 0; i < names.Length; i++)
                if (string.Equals(names[i], name, StringComparison.Ordinal)) return i;
            return -1;
        }

        private void OnDisable()
        {
            if (_playing) Stop();
        }

        private void OnDestroy()
        {
            if (_handler != null)
            {
                _handler.Dispose();
                _handler = null;
            }
        }
    }
}
