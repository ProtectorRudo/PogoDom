using System.Collections.Generic;
using PogoDom.Cosmetics;
using PogoDom.Core;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Makes avatars react to important battle moments without touching the
    /// deterministic simulation. Procedural faces are a fallback for the current
    /// prototype; authored characters can later replace this with blendshapes.
    /// </summary>
    [DefaultExecutionOrder(1320)]
    [DisallowMultipleComponent]
    public sealed class PogoDomAvatarExpressionDirector : MonoBehaviour
    {
        private static readonly string[] PlayerNames = { "YOU", "BOT A", "BOT B", "BOT C" };
        private PogoDomPrototypeBootstrap _bootstrap;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
                if (prototypes[i].GetComponent<PogoDomAvatarExpressionDirector>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomAvatarExpressionDirector>();
        }

        private void Awake()
        {
            _bootstrap = GetComponent<PogoDomPrototypeBootstrap>();
        }

        private void OnEnable()
        {
            if (_bootstrap == null) _bootstrap = GetComponent<PogoDomPrototypeBootstrap>();
            if (_bootstrap != null) _bootstrap.PresentationEvents += Present;
        }

        private void OnDisable()
        {
            if (_bootstrap != null) _bootstrap.PresentationEvents -= Present;
        }

        private void Present(IReadOnlyList<MatchEvent> events)
        {
            if (events == null) return;
            for (var i = 0; i < events.Count; i++)
            {
                AvatarExpressionCue cue;
                if (!AvatarExpressionPolicy.TryDescribe(events[i], out cue)) continue;
                var player = FindDirectChild(cue.PlayerId);
                if (player == null) continue;
                var face = player.GetComponent<PogoProceduralFaceController>();
                if (face == null) face = player.gameObject.AddComponent<PogoProceduralFaceController>();
                face.Play(cue);
            }
        }

        private Transform FindDirectChild(int playerId)
        {
            if (playerId < 0 || playerId >= PlayerNames.Length) return null;
            for (var i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).name == PlayerNames[playerId]) return transform.GetChild(i);
            return null;
        }
    }

    [DisallowMultipleComponent]
    public sealed class PogoProceduralFaceController : MonoBehaviour
    {
        private Transform _eyeL;
        private Transform _eyeR;
        private Transform _pupilL;
        private Transform _pupilR;
        private Transform _browL;
        private Transform _browR;
        private Transform _mouth;
        private Vector3 _eyeLScale;
        private Vector3 _eyeRScale;
        private Vector3 _pupilLScale;
        private Vector3 _pupilRScale;
        private Vector3 _browLPosition;
        private Vector3 _browRPosition;
        private Quaternion _browLRotation;
        private Quaternion _browRRotation;
        private Vector3 _mouthScale;
        private float _remaining;
        private float _duration;
        private AvatarExpressionCue _active;
        private bool _ready;

        public void Play(AvatarExpressionCue cue)
        {
            EnsureFace();
            if (!_ready) return;
            _active = cue;
            _duration = cue.DurationSeconds;
            _remaining = _duration;
        }

        private void EnsureFace()
        {
            if (_ready) return;
            _eyeL = FindDeep(transform, "EyeL");
            _eyeR = FindDeep(transform, "EyeR");
            _pupilL = FindDeep(transform, "PupilL");
            _pupilR = FindDeep(transform, "PupilR");
            var head = FindDeep(transform, "Head");
            if (_eyeL == null || _eyeR == null || _pupilL == null || _pupilR == null || head == null) return;

            _eyeLScale = _eyeL.localScale;
            _eyeRScale = _eyeR.localScale;
            _pupilLScale = _pupilL.localScale;
            _pupilRScale = _pupilR.localScale;

            var parent = head.parent;
            var dark = new Color(0.05f, 0.055f, 0.08f);
            _browL = PogoAvatarVisualRig.Primitive("ExpressionBrowL", PrimitiveType.Cube, parent,
                new Vector3(-0.13f, 0.86f, -0.36f), new Vector3(0.11f, 0.025f, 0.025f), dark, Quaternion.Euler(0f, 0f, -5f), 0.30f).transform;
            _browR = PogoAvatarVisualRig.Primitive("ExpressionBrowR", PrimitiveType.Cube, parent,
                new Vector3(0.13f, 0.86f, -0.36f), new Vector3(0.11f, 0.025f, 0.025f), dark, Quaternion.Euler(0f, 0f, 5f), 0.30f).transform;
            _mouth = PogoAvatarVisualRig.Primitive("ExpressionMouth", PrimitiveType.Sphere, parent,
                new Vector3(0f, 0.54f, -0.365f), new Vector3(0.11f, 0.025f, 0.035f), dark, Quaternion.identity, 0.28f).transform;

            _browLPosition = _browL.localPosition;
            _browRPosition = _browR.localPosition;
            _browLRotation = _browL.localRotation;
            _browRRotation = _browR.localRotation;
            _mouthScale = _mouth.localScale;
            _ready = true;
            RestoreNeutral();
        }

        private void Update()
        {
            if (!_ready || _remaining <= 0f) return;
            _remaining = Mathf.Max(0f, _remaining - Time.unscaledDeltaTime);
            var progress = 1f - _remaining / Mathf.Max(0.001f, _duration);
            var envelope = Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI) * _active.Strength;
            Apply(_active.Kind, envelope);
            if (_remaining <= 0f) RestoreNeutral();
        }

        private void Apply(AvatarExpressionKind kind, float k)
        {
            RestoreNeutral();
            switch (kind)
            {
                case AvatarExpressionKind.Excited:
                    ScaleEyes(1f + 0.18f * k, 1f + 0.12f * k);
                    _mouth.localScale = new Vector3(_mouthScale.x * (1f + 0.45f * k), _mouthScale.y * (1f + 0.45f * k), _mouthScale.z);
                    MoveBrows(0.025f * k);
                    break;

                case AvatarExpressionKind.Proud:
                    ScaleEyes(1f + 0.08f * k, 1f - 0.16f * k);
                    RotateBrows(-8f * k, 8f * k);
                    _mouth.localScale = new Vector3(_mouthScale.x * (1f + 0.55f * k), _mouthScale.y * (1f + 0.20f * k), _mouthScale.z);
                    break;

                case AvatarExpressionKind.Attack:
                    ScaleEyes(1f + 0.04f * k, 1f - 0.26f * k);
                    RotateBrows(18f * k, -18f * k);
                    _mouth.localScale = new Vector3(_mouthScale.x * (1f - 0.25f * k), _mouthScale.y, _mouthScale.z);
                    break;

                case AvatarExpressionKind.Hurt:
                    ScaleEyes(1f + 0.10f * k, 1f - 0.48f * k);
                    RotateBrows(-22f * k, 22f * k);
                    _pupilL.localScale = _pupilLScale * (1f - 0.25f * k);
                    _pupilR.localScale = _pupilRScale * (1f - 0.25f * k);
                    _mouth.localScale = new Vector3(_mouthScale.x * (1f + 0.30f * k), _mouthScale.y * (1f + 0.80f * k), _mouthScale.z);
                    break;

                case AvatarExpressionKind.Shielded:
                    ScaleEyes(1f + 0.03f * k, 1f - 0.12f * k);
                    RotateBrows(10f * k, -10f * k);
                    MoveBrows(0.018f * k);
                    break;

                case AvatarExpressionKind.Surprised:
                    ScaleEyes(1f + 0.30f * k, 1f + 0.34f * k);
                    _pupilL.localScale = _pupilLScale * (1f + 0.20f * k);
                    _pupilR.localScale = _pupilRScale * (1f + 0.20f * k);
                    _mouth.localScale = new Vector3(_mouthScale.x * (1f - 0.38f * k), _mouthScale.y * (1f + 1.20f * k), _mouthScale.z);
                    MoveBrows(0.035f * k);
                    break;
            }
        }

        private void ScaleEyes(float x, float y)
        {
            _eyeL.localScale = new Vector3(_eyeLScale.x * x, _eyeLScale.y * y, _eyeLScale.z);
            _eyeR.localScale = new Vector3(_eyeRScale.x * x, _eyeRScale.y * y, _eyeRScale.z);
        }

        private void MoveBrows(float y)
        {
            _browL.localPosition = _browLPosition + Vector3.up * y;
            _browR.localPosition = _browRPosition + Vector3.up * y;
        }

        private void RotateBrows(float leftZ, float rightZ)
        {
            _browL.localRotation = _browLRotation * Quaternion.Euler(0f, 0f, leftZ);
            _browR.localRotation = _browRRotation * Quaternion.Euler(0f, 0f, rightZ);
        }

        private void RestoreNeutral()
        {
            if (!_ready) return;
            _eyeL.localScale = _eyeLScale;
            _eyeR.localScale = _eyeRScale;
            _pupilL.localScale = _pupilLScale;
            _pupilR.localScale = _pupilRScale;
            _browL.localPosition = _browLPosition;
            _browR.localPosition = _browRPosition;
            _browL.localRotation = _browLRotation;
            _browR.localRotation = _browRRotation;
            _mouth.localScale = _mouthScale;
        }

        private static Transform FindDeep(Transform root, string objectName)
        {
            if (root.name == objectName) return root;
            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindDeep(root.GetChild(i), objectName);
                if (found != null) return found;
            }
            return null;
        }
    }
}
