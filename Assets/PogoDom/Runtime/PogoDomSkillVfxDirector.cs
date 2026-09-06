using System.Collections.Generic;
using PogoDom.Cosmetics;
using PogoDom.Core;
using UnityEngine;

namespace PogoDom.Runtime
{
    [DefaultExecutionOrder(1300)]
    [DisallowMultipleComponent]
    public sealed class PogoDomSkillVfxDirector : MonoBehaviour
    {
        private static readonly string[] PlayerNames = { "YOU", "BOT A", "BOT B", "BOT C" };
        private static readonly Color[] PlayerColors =
        {
            new Color(0.98f, 0.20f, 0.24f),
            new Color(0.15f, 0.88f, 0.38f),
            new Color(0.18f, 0.48f, 1.00f),
            new Color(1.00f, 0.78f, 0.10f)
        };

        private readonly List<PogoSkillBurstFx> _pool = new List<PogoSkillBurstFx>();
        private PogoDomPrototypeBootstrap _bootstrap;
        private PogoDomRuntimeSkillLoadouts _loadouts;
        private PogoDomVisualQualityScaler _quality;
        private PogoDomCameraJuice _cameraJuice;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
            {
                var root = prototypes[i].gameObject;
                if (root.GetComponent<PogoDomRuntimeSkillLoadouts>() == null)
                    root.AddComponent<PogoDomRuntimeSkillLoadouts>();
                if (root.GetComponent<PogoDomSkillVfxDirector>() == null)
                    root.AddComponent<PogoDomSkillVfxDirector>();
            }
        }

        private void Awake()
        {
            _bootstrap = GetComponent<PogoDomPrototypeBootstrap>();
            _loadouts = GetComponent<PogoDomRuntimeSkillLoadouts>();
            _quality = GetComponent<PogoDomVisualQualityScaler>();
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
            if (_loadouts == null) _loadouts = GetComponent<PogoDomRuntimeSkillLoadouts>();
            if (_quality == null) _quality = GetComponent<PogoDomVisualQualityScaler>();
            if (_cameraJuice == null && Camera.main != null)
                _cameraJuice = Camera.main.GetComponent<PogoDomCameraJuice>();

            for (var i = 0; i < events.Count; i++)
            {
                var e = events[i];
                VisualEventCue cue;
                if (!VisualEventPolicy.TryDescribe(e, out cue)) continue;

                // Normal tile ownership already has M0.31 pop/glow. Duplicating a
                // particle burst for hundreds of steals would add noise, not juice.
                if (cue.Impact == VisualImpactTier.Ambient) continue;

                PogoAvatarVisualRig rig;
                var anchor = ResolveAnchor(e, out rig);
                if (anchor == null) continue;

                SkillVisualDefinition visual = null;
                if (_loadouts != null && e.PlayerId >= 0)
                    visual = _loadouts.Resolve(e.PlayerId, e);
                if (rig != null && visual != null)
                    rig.ApplySkillVisual(visual);

                var rarity = visual == null ? CosmeticRarity.Common : visual.Rarity;
                var color = EventColor(e, cue.Trigger);
                var burst = Acquire(cue.Impact);
                if (burst == null) continue;

                var particleBudget = _quality == null
                    ? cue.ParticleBudget
                    : _quality.ScaleParticleBudget(cue.ParticleBudget);
                burst.Play(anchor.position, cue, rarity, color, particleBudget);

                if (_cameraJuice != null && cue.CameraImpulse > 0f)
                    _cameraJuice.Impulse(cue.CameraImpulse);
            }
        }

        private Transform ResolveAnchor(MatchEvent e, out PogoAvatarVisualRig rig)
        {
            rig = null;
            if (e.PlayerId >= 0 && e.PlayerId < PlayerNames.Length)
            {
                var player = FindDirectChild(PlayerNames[e.PlayerId]);
                if (player != null)
                {
                    rig = player.GetComponent<PogoAvatarVisualRig>();
                    if (rig != null && rig.SkillSocket != null) return rig.SkillSocket;
                    return player;
                }
            }

            // World events (for example TNT) can be anchored to the deterministic
            // tile encoded by MatchEvent.Position without reading hidden Core state.
            var tile = FindDirectChild("Tile_" + e.Position.X + "_" + e.Position.Y);
            return tile;
        }

        private PogoSkillBurstFx Acquire(VisualImpactTier incoming)
        {
            for (var i = 0; i < _pool.Count; i++)
                if (!_pool[i].IsPlaying) return _pool[i];

            var budget = _quality == null
                ? VisualQualityPolicy.Get(VisualQualityTier.Balanced)
                : _quality.ActiveBudget;

            if (_pool.Count < budget.MaxConcurrentSpectacleBursts)
            {
                var go = new GameObject("SkillBurst_" + _pool.Count);
                go.transform.SetParent(transform, true);
                var burst = go.AddComponent<PogoSkillBurstFx>();
                _pool.Add(burst);
                return burst;
            }

            if (incoming != VisualImpactTier.Spectacle) return null;

            // A major moment may replace the least-important currently playing
            // burst, but routine Readable feedback never evicts a spectacle.
            PogoSkillBurstFx candidate = null;
            for (var i = 0; i < _pool.Count; i++)
            {
                if (_pool[i].Impact == VisualImpactTier.Spectacle) continue;
                if (candidate == null || _pool[i].RemainingSeconds < candidate.RemainingSeconds)
                    candidate = _pool[i];
            }
            return candidate;
        }

        private Transform FindDirectChild(string objectName)
        {
            for (var i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).name == objectName) return transform.GetChild(i);
            return null;
        }

        private static Color EventColor(MatchEvent e, SkillVisualTrigger trigger)
        {
            var baseColor = e.PlayerId >= 0 && e.PlayerId < PlayerColors.Length
                ? PlayerColors[e.PlayerId]
                : new Color(1.00f, 0.28f, 0.10f);

            switch (trigger)
            {
                case SkillVisualTrigger.Speed: return Color.Lerp(baseColor, Color.cyan, 0.48f);
                case SkillVisualTrigger.Missile: return Color.Lerp(baseColor, new Color(1f, 0.12f, 0.05f), 0.52f);
                case SkillVisualTrigger.Padlock:
                case SkillVisualTrigger.ShieldBlock: return Color.Lerp(baseColor, new Color(0.20f, 0.90f, 1f), 0.55f);
                case SkillVisualTrigger.Bank: return Color.Lerp(baseColor, new Color(0.82f, 0.38f, 1f), 0.35f);
                case SkillVisualTrigger.HazardBlast: return new Color(1f, 0.25f, 0.04f);
                default: return PogoVisualMaterialFactory.Lighten(baseColor, 0.18f);
            }
        }
    }

    [DisallowMultipleComponent]
    public sealed class PogoSkillBurstFx : MonoBehaviour
    {
        private ParticleSystem _particles;
        private ParticleSystemRenderer _particleRenderer;
        private Transform _core;
        private Renderer _coreRenderer;
        private float _remaining;
        private float _duration;
        private float _radius;
        private Color _color;

        public bool IsPlaying => _remaining > 0f;
        public float RemainingSeconds => _remaining;
        public VisualImpactTier Impact { get; private set; }

        private void Awake()
        {
            _particles = gameObject.AddComponent<ParticleSystem>();
            var main = _particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 64;
            var emission = _particles.emission;
            emission.enabled = false;
            var shape = _particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.10f;

            _particleRenderer = _particles.GetComponent<ParticleSystemRenderer>();
            _particleRenderer.material = CreateParticleMaterial(Color.white);

            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "BurstCore";
            core.transform.SetParent(transform, false);
            PogoAvatarVisualRig.RemoveCollider(core);
            _core = core.transform;
            _coreRenderer = core.GetComponent<Renderer>();
            _coreRenderer.material = PogoVisualMaterialFactory.Create(Color.white, 0f, 0.92f, true);
            core.SetActive(false);
        }

        public void Play(Vector3 position, VisualEventCue cue, CosmeticRarity rarity, Color color, int particleBudget)
        {
            transform.position = position;
            Impact = cue.Impact;
            _duration = Mathf.Max(0.10f, cue.LifetimeSeconds);
            _remaining = _duration;
            _radius = Mathf.Max(0.2f, cue.RadiusTiles);
            _color = color;

            var main = _particles.main;
            main.startLifetime = Mathf.Max(0.16f, _duration * 0.72f);
            main.startSpeed = 0.75f + _radius * 1.65f;
            main.startSize = 0.055f + (int)rarity * 0.012f;
            main.startColor = color;
            var shape = _particles.shape;
            shape.radius = 0.08f + _radius * 0.045f;

            ApplyParticleColor(color);
            ApplyCoreColor(color);
            _particles.Clear(true);
            _particles.Play(true);
            if (particleBudget > 0) _particles.Emit(Mathf.Min(64, particleBudget));

            _core.gameObject.SetActive(true);
            _core.localScale = Vector3.one * 0.06f;
        }

        private void Update()
        {
            if (_remaining <= 0f) return;
            _remaining = Mathf.Max(0f, _remaining - Time.deltaTime);
            var t = 1f - _remaining / Mathf.Max(0.001f, _duration);
            if (_core != null)
            {
                var envelope = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);
                var size = 0.06f + envelope * _radius * 0.34f;
                _core.localScale = Vector3.one * size;
                _core.Rotate(25f * Time.deltaTime, 70f * Time.deltaTime, 15f * Time.deltaTime, Space.Self);
            }

            if (_remaining <= 0f && _core != null)
                _core.gameObject.SetActive(false);
        }

        private void ApplyParticleColor(Color color)
        {
            if (_particleRenderer == null || _particleRenderer.material == null) return;
            var material = _particleRenderer.material;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", color * 1.8f);
        }

        private void ApplyCoreColor(Color color)
        {
            if (_coreRenderer == null || _coreRenderer.material == null) return;
            var bright = PogoVisualMaterialFactory.Lighten(color, 0.18f);
            var material = _coreRenderer.material;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", bright);
            if (material.HasProperty("_Color")) material.SetColor("_Color", bright);
            if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", bright * 1.8f);
        }

        private static Material CreateParticleMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", color * 1.8f);
            return material;
        }
    }
}
