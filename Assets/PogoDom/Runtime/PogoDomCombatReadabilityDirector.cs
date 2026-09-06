using System.Collections.Generic;
using PogoDom.Cosmetics;
using PogoDom.Core;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Essential competitive feedback layered on top of the cosmetic spectacle
    /// system. It makes attacker -> target cause/effect readable without slowing,
    /// pausing or otherwise mutating deterministic battle simulation.
    /// </summary>
    [DefaultExecutionOrder(1310)]
    [DisallowMultipleComponent]
    public sealed class PogoDomCombatReadabilityDirector : MonoBehaviour
    {
        private static readonly string[] PlayerNames = { "YOU", "BOT A", "BOT B", "BOT C" };
        private readonly List<PogoCombatTraceFx> _tracePool = new List<PogoCombatTraceFx>();
        private PogoDomPrototypeBootstrap _bootstrap;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].GetComponent<PogoDomCombatReadabilityDirector>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomCombatReadabilityDirector>();
            }
        }

        private void Awake()
        {
            _bootstrap = GetComponent<PogoDomPrototypeBootstrap>();
        }

        private void OnEnable()
        {
            if (_bootstrap == null) _bootstrap = GetComponent<PogoDomPrototypeBootstrap>();
            if (_bootstrap != null) _bootstrap.PresentationEvents += PresentEvents;
        }

        private void OnDisable()
        {
            if (_bootstrap != null) _bootstrap.PresentationEvents -= PresentEvents;
        }

        private void PresentEvents(IReadOnlyList<MatchEvent> events)
        {
            if (events == null) return;

            for (var i = 0; i < events.Count; i++)
            {
                CombatFeedbackCue cue;
                if (!CombatFeedbackPolicy.TryDescribe(events[i], out cue)) continue;

                var source = ResolvePlayerAnchor(cue.SourcePlayerId);
                var target = ResolvePlayerAnchor(cue.TargetPlayerId);
                if (target == null) continue;

                var color = CueColor(cue.Kind);
                if (cue.Kind == CombatFeedbackKind.MissileTrace && source != null)
                {
                    var trace = AcquireTrace();
                    trace.Play(
                        source.position + Vector3.up * 0.18f,
                        target.position + Vector3.up * 0.24f,
                        cue,
                        color);
                }

                var pulse = target.GetComponent<PogoCombatTargetPulse>();
                if (pulse == null) pulse = target.gameObject.AddComponent<PogoCombatTargetPulse>();
                pulse.Trigger(cue, color);
            }
        }

        private PogoCombatTraceFx AcquireTrace()
        {
            for (var i = 0; i < _tracePool.Count; i++)
                if (!_tracePool[i].IsPlaying) return _tracePool[i];

            // Missile traces are short and essential. Four concurrent traces cover
            // the maximum four-player arena without unbounded allocations.
            if (_tracePool.Count < 4)
            {
                var go = new GameObject("CombatTrace_" + _tracePool.Count);
                go.transform.SetParent(transform, false);
                var trace = go.AddComponent<PogoCombatTraceFx>();
                _tracePool.Add(trace);
                return trace;
            }

            // Reuse the trace that is closest to expiry. This is presentation-only
            // and does not change missile targeting or stun state.
            var best = _tracePool[0];
            for (var i = 1; i < _tracePool.Count; i++)
                if (_tracePool[i].RemainingSeconds < best.RemainingSeconds) best = _tracePool[i];
            return best;
        }

        private Transform ResolvePlayerAnchor(int playerId)
        {
            if (playerId < 0 || playerId >= PlayerNames.Length) return null;
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name != PlayerNames[playerId]) continue;
                var rig = child.GetComponent<PogoAvatarVisualRig>();
                if (rig != null && rig.SkillSocket != null) return rig.SkillSocket;
                return child;
            }
            return null;
        }

        private static Color CueColor(CombatFeedbackKind kind)
        {
            switch (kind)
            {
                case CombatFeedbackKind.ShieldBlock: return new Color(0.15f, 0.88f, 1.00f);
                case CombatFeedbackKind.StunImpact: return new Color(1.00f, 0.34f, 0.08f);
                default: return new Color(1.00f, 0.16f, 0.08f);
            }
        }
    }

    [DisallowMultipleComponent]
    public sealed class PogoCombatTraceFx : MonoBehaviour
    {
        private const int PointCount = 9;
        private LineRenderer _line;
        private float _remaining;
        private float _duration;
        private Color _color;

        public bool IsPlaying => _remaining > 0f;
        public float RemainingSeconds => _remaining;

        private void Awake()
        {
            _line = gameObject.AddComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.positionCount = PointCount;
            _line.numCornerVertices = 3;
            _line.numCapVertices = 3;
            _line.alignment = LineAlignment.View;
            _line.textureMode = LineTextureMode.Stretch;
            _line.material = CreateLineMaterial(Color.white);
            _line.enabled = false;
        }

        public void Play(Vector3 from, Vector3 to, CombatFeedbackCue cue, Color color)
        {
            _duration = Mathf.Max(0.08f, cue.LifetimeSeconds);
            _remaining = _duration;
            _color = color;
            _line.startWidth = Mathf.Max(0.025f, cue.WidthScale);
            _line.endWidth = Mathf.Max(0.012f, cue.WidthScale * 0.42f);

            var delta = to - from;
            var horizontal = new Vector3(delta.x, 0f, delta.z);
            var side = horizontal.sqrMagnitude < 0.001f
                ? Vector3.right
                : Vector3.Cross(Vector3.up, horizontal.normalized);
            var arcHeight = Mathf.Clamp(delta.magnitude * 0.12f, 0.22f, 0.72f);

            for (var i = 0; i < PointCount; i++)
            {
                var t = i / (float)(PointCount - 1);
                var p = Vector3.Lerp(from, to, t);
                p.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
                p += side * Mathf.Sin(t * Mathf.PI * 2f) * 0.055f;
                _line.SetPosition(i, p);
            }

            ApplyColor(1f);
            _line.enabled = true;
        }

        private void Update()
        {
            if (_remaining <= 0f) return;
            _remaining = Mathf.Max(0f, _remaining - Time.deltaTime);
            var alpha = Mathf.Clamp01(_remaining / Mathf.Max(0.001f, _duration));
            ApplyColor(alpha);
            _line.widthMultiplier = 0.72f + Mathf.Sin(alpha * Mathf.PI) * 0.28f;
            if (_remaining <= 0f) _line.enabled = false;
        }

        private void ApplyColor(float alpha)
        {
            if (_line == null) return;
            var start = _color;
            var end = PogoVisualMaterialFactory.Lighten(_color, 0.35f);
            start.a = alpha;
            end.a = alpha * 0.18f;
            _line.startColor = start;
            _line.endColor = end;
            if (_line.material != null)
            {
                if (_line.material.HasProperty("_Color")) _line.material.SetColor("_Color", start);
                if (_line.material.HasProperty("_BaseColor")) _line.material.SetColor("_BaseColor", start);
            }
        }

        private static Material CreateLineMaterial(Color color)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            return material;
        }
    }

    [DisallowMultipleComponent]
    public sealed class PogoCombatTargetPulse : MonoBehaviour
    {
        private const int RingPoints = 33;
        private LineRenderer _ring;
        private float _remaining;
        private float _duration;
        private float _strength;
        private Color _color;

        private void Awake()
        {
            var go = new GameObject("CombatReadabilityRing");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            _ring = go.AddComponent<LineRenderer>();
            _ring.useWorldSpace = false;
            _ring.loop = true;
            _ring.positionCount = RingPoints;
            _ring.numCornerVertices = 2;
            _ring.numCapVertices = 2;
            _ring.startWidth = 0.045f;
            _ring.endWidth = 0.045f;
            _ring.material = CreateRingMaterial(Color.white);
            BuildUnitRing();
            _ring.enabled = false;
        }

        public void Trigger(CombatFeedbackCue cue, Color color)
        {
            _duration = Mathf.Max(0.10f, cue.LifetimeSeconds);
            _remaining = _duration;
            _strength = Mathf.Clamp01(cue.PulseStrength);
            _color = color;
            _ring.enabled = true;
            ApplyColor(1f);
        }

        private void Update()
        {
            if (_remaining <= 0f) return;
            _remaining = Mathf.Max(0f, _remaining - Time.deltaTime);
            var normalized = 1f - _remaining / Mathf.Max(0.001f, _duration);
            var envelope = Mathf.Sin(normalized * Mathf.PI);
            var radius = Mathf.Lerp(0.42f, 0.95f + _strength * 0.20f, normalized);
            _ring.transform.localScale = new Vector3(radius, 1f, radius);
            _ring.startWidth = _ring.endWidth = 0.035f + envelope * 0.045f * _strength;
            ApplyColor((1f - normalized) * (0.55f + _strength * 0.45f));
            if (_remaining <= 0f) _ring.enabled = false;
        }

        private void BuildUnitRing()
        {
            for (var i = 0; i < RingPoints; i++)
            {
                var t = i / (float)(RingPoints - 1);
                var a = t * Mathf.PI * 2f;
                _ring.SetPosition(i, new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)));
            }
        }

        private void ApplyColor(float alpha)
        {
            var color = _color;
            color.a = Mathf.Clamp01(alpha);
            _ring.startColor = color;
            _ring.endColor = color;
            if (_ring.material != null)
            {
                if (_ring.material.HasProperty("_Color")) _ring.material.SetColor("_Color", color);
                if (_ring.material.HasProperty("_BaseColor")) _ring.material.SetColor("_BaseColor", color);
            }
        }

        private static Material CreateRingMaterial(Color color)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            return material;
        }
    }
}
