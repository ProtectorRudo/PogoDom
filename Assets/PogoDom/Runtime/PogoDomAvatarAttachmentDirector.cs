using System.Collections;
using PogoDom.Cosmetics;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Gives each avatar a complete launch identity kit: headwear, back piece,
    /// aura, trail signature and landing signature. All geometry is presentation
    /// only and is attached below the already-neutral gameplay player root.
    /// </summary>
    [DefaultExecutionOrder(1120)]
    [DisallowMultipleComponent]
    public sealed class PogoDomAvatarAttachmentDirector : MonoBehaviour
    {
        private static readonly string[] PlayerNames = { "YOU", "BOT A", "BOT B", "BOT C" };
        private static readonly Color[] PlayerColors =
        {
            new Color(0.98f, 0.20f, 0.24f),
            new Color(0.15f, 0.88f, 0.38f),
            new Color(0.18f, 0.48f, 1.00f),
            new Color(1.00f, 0.78f, 0.10f)
        };

        private float _rescan;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
                if (prototypes[i].GetComponent<PogoDomAvatarAttachmentDirector>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomAvatarAttachmentDirector>();
        }

        private IEnumerator Start()
        {
            // ViralVisualDirector creates/initializes PogoAvatarVisualRig after the
            // greybox. Give that layer two frames before attaching cosmetic parts.
            yield return null;
            yield return null;
            DecorateNow();
        }

        private void Update()
        {
            _rescan -= Time.deltaTime;
            if (_rescan > 0f) return;
            _rescan = 0.75f;
            DecorateNow();
        }

        private void DecorateNow()
        {
            for (var i = 0; i < PlayerNames.Length; i++)
            {
                var player = FindDirectChild(PlayerNames[i]);
                if (player == null) continue;
                var rig = player.GetComponent<PogoAvatarVisualRig>();
                if (rig == null || rig.HeadwearSocket == null || rig.BackSocket == null) continue;

                var visual = player.GetComponent<PogoAvatarAttachmentVisual>();
                if (visual == null) visual = player.gameObject.AddComponent<PogoAvatarAttachmentVisual>();
                visual.Initialize(i, PlayerColors[i], rig, AvatarAttachmentVisualProfiles.ForStyle(i));
            }
        }

        private Transform FindDirectChild(string objectName)
        {
            for (var i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).name == objectName) return transform.GetChild(i);
            return null;
        }
    }

    [DisallowMultipleComponent]
    public sealed class PogoAvatarAttachmentVisual : MonoBehaviour
    {
        private bool _initialized;
        private Transform _headRoot;
        private Transform _backRoot;
        private Transform _auraRoot;

        public void Initialize(int styleIndex, Color playerColor, PogoAvatarVisualRig rig, AvatarAttachmentVisualProfile profile)
        {
            if (_initialized || rig == null || profile == null) return;
            _initialized = true;

            DisablePrototypeDecoration(rig.HeadwearSocket);
            DisablePrototypeDecoration(rig.BackSocket);

            _headRoot = Anchor("EquippedHeadwear", rig.HeadwearSocket);
            _headRoot.localScale = Vector3.one * profile.HeadwearScale;
            _backRoot = Anchor("EquippedBack", rig.BackSocket);
            _backRoot.localScale = Vector3.one * profile.BackScale;
            _auraRoot = Anchor("EquippedAura", transform);
            _auraRoot.localPosition = new Vector3(0f, 0.42f, 0f);

            BuildHeadwear(profile.Headwear, _headRoot, playerColor);
            BuildBack(profile.Back, _backRoot, playerColor);
            BuildAura(profile.Aura, _auraRoot, playerColor, profile.AuraRadius);
            ConfigureTrail(rig, profile, playerColor);
            ConfigureLanding(rig, profile, playerColor);
        }

        private static void BuildHeadwear(HeadwearVisualFamily family, Transform root, Color color)
        {
            var accent = PogoVisualMaterialFactory.Accent(color);
            var dark = new Color(0.07f, 0.08f, 0.12f);
            switch (family)
            {
                case HeadwearVisualFamily.SportCap:
                    Part("CapCrown", PrimitiveType.Cylinder, root, Vector3.zero, new Vector3(0.30f, 0.07f, 0.30f), accent, Quaternion.identity, false);
                    Part("CapBill", PrimitiveType.Cube, root, new Vector3(0f, -0.02f, -0.28f), new Vector3(0.25f, 0.035f, 0.14f), dark, Quaternion.Euler(-8f, 0f, 0f), false);
                    break;
                case HeadwearVisualFamily.SplitCrown:
                    for (var i = -1; i <= 1; i++)
                        Part("CrownSpike_" + i, PrimitiveType.Cube, root, new Vector3(i * 0.16f, 0.09f + (i == 0 ? 0.05f : 0f), 0f), new Vector3(0.09f, 0.23f, 0.10f), accent, Quaternion.Euler(0f, 0f, i * -12f), true);
                    break;
                case HeadwearVisualFamily.TechAntenna:
                    Part("TechBand", PrimitiveType.Cylinder, root, Vector3.zero, new Vector3(0.31f, 0.045f, 0.31f), dark, Quaternion.identity, false);
                    Part("TechStem", PrimitiveType.Cylinder, root, new Vector3(0f, 0.18f, 0f), new Vector3(0.025f, 0.16f, 0.025f), dark, Quaternion.identity, false);
                    Part("TechOrb", PrimitiveType.Sphere, root, new Vector3(0f, 0.36f, 0f), Vector3.one * 0.09f, accent, Quaternion.identity, true);
                    break;
                case HeadwearVisualFamily.MascotEars:
                    Part("EarL", PrimitiveType.Capsule, root, new Vector3(-0.20f, 0.12f, 0f), new Vector3(0.11f, 0.25f, 0.11f), color, Quaternion.Euler(0f, 0f, -18f), false);
                    Part("EarR", PrimitiveType.Capsule, root, new Vector3(0.20f, 0.12f, 0f), new Vector3(0.11f, 0.25f, 0.11f), color, Quaternion.Euler(0f, 0f, 18f), false);
                    break;
                case HeadwearVisualFamily.StreetBeanie:
                    Part("Beanie", PrimitiveType.Sphere, root, new Vector3(0f, 0.02f, 0f), new Vector3(0.34f, 0.20f, 0.34f), dark, Quaternion.identity, false);
                    Part("BeanieTag", PrimitiveType.Cube, root, new Vector3(0f, 0.02f, -0.30f), new Vector3(0.10f, 0.07f, 0.025f), accent, Quaternion.identity, true);
                    break;
                default:
                    Part("CaptainBand", PrimitiveType.Cylinder, root, Vector3.zero, new Vector3(0.31f, 0.045f, 0.31f), dark, Quaternion.identity, false);
                    Part("CaptainCrest", PrimitiveType.Cube, root, new Vector3(0f, 0.18f, -0.02f), new Vector3(0.09f, 0.28f, 0.16f), accent, Quaternion.Euler(0f, 0f, 8f), true);
                    break;
            }
        }

        private static void BuildBack(BackVisualFamily family, Transform root, Color color)
        {
            var accent = PogoVisualMaterialFactory.Accent(color);
            var dark = new Color(0.07f, 0.08f, 0.12f);
            switch (family)
            {
                case BackVisualFamily.SportPack:
                    Part("SportPack", PrimitiveType.Cube, root, Vector3.zero, new Vector3(0.28f, 0.34f, 0.15f), dark, Quaternion.identity, false);
                    Part("PackBadge", PrimitiveType.Sphere, root, new Vector3(0f, 0f, 0.16f), Vector3.one * 0.09f, accent, Quaternion.identity, true);
                    break;
                case BackVisualFamily.TricksterFins:
                    Part("FinL", PrimitiveType.Cube, root, new Vector3(-0.22f, 0f, 0f), new Vector3(0.12f, 0.34f, 0.07f), accent, Quaternion.Euler(0f, 0f, -28f), true);
                    Part("FinR", PrimitiveType.Cube, root, new Vector3(0.22f, 0f, 0f), new Vector3(0.12f, 0.34f, 0.07f), accent, Quaternion.Euler(0f, 0f, 28f), true);
                    break;
                case BackVisualFamily.TechBattery:
                    Part("Battery", PrimitiveType.Cube, root, Vector3.zero, new Vector3(0.25f, 0.36f, 0.14f), dark, Quaternion.identity, false);
                    Part("BatteryCore", PrimitiveType.Cube, root, new Vector3(0f, 0f, 0.15f), new Vector3(0.12f, 0.22f, 0.025f), accent, Quaternion.identity, true);
                    break;
                case BackVisualFamily.MascotTail:
                    Part("Tail", PrimitiveType.Sphere, root, new Vector3(0f, -0.12f, 0.16f), new Vector3(0.24f, 0.18f, 0.24f), accent, Quaternion.identity, false);
                    break;
                case BackVisualFamily.StreetCape:
                    Part("Cape", PrimitiveType.Cube, root, new Vector3(0f, -0.13f, 0.08f), new Vector3(0.34f, 0.48f, 0.045f), color, Quaternion.Euler(10f, 0f, 0f), false);
                    Part("CapeMark", PrimitiveType.Cube, root, new Vector3(0f, -0.10f, 0.13f), new Vector3(0.10f, 0.16f, 0.02f), accent, Quaternion.Euler(10f, 0f, 0f), true);
                    break;
                default:
                    Part("BannerPole", PrimitiveType.Cylinder, root, new Vector3(0.22f, 0.08f, 0f), new Vector3(0.025f, 0.40f, 0.025f), dark, Quaternion.identity, false);
                    Part("Banner", PrimitiveType.Cube, root, new Vector3(0.05f, 0.24f, 0f), new Vector3(0.26f, 0.18f, 0.035f), accent, Quaternion.identity, true);
                    break;
            }
        }

        private static void BuildAura(AuraVisualFamily family, Transform root, Color color, float radius)
        {
            var orbiters = family == AuraVisualFamily.HexPulse ? 6 : family == AuraVisualFamily.BubbleOrbit ? 4 : 3;
            for (var i = 0; i < orbiters; i++)
            {
                var angle = i * Mathf.PI * 2f / orbiters;
                var y = family == AuraVisualFamily.SplitOrbit && i % 2 == 0 ? 0.16f : 0f;
                var orb = Part("AuraOrb_" + i, PrimitiveType.Sphere, root,
                    new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius),
                    Vector3.one * (family == AuraVisualFamily.BubbleOrbit ? 0.075f : 0.055f),
                    PogoVisualMaterialFactory.Accent(color), Quaternion.identity, true);
                var motion = orb.AddComponent<PogoAuraOrbMotion>();
                motion.Initialize(angle, radius, family);
            }
        }

        private static void ConfigureTrail(PogoAvatarVisualRig rig, AvatarAttachmentVisualProfile profile, Color color)
        {
            var trail = rig.TrailSocket == null ? null : rig.TrailSocket.GetComponent<TrailRenderer>();
            if (trail == null) return;
            trail.startWidth = profile.TrailWidth;
            trail.endWidth = Mathf.Max(0.012f, profile.TrailWidth * 0.12f);
            trail.numCornerVertices = profile.Trail == TrailVisualFamily.Ribbon || profile.Trail == TrailVisualFamily.Royal ? 4 : 2;
            trail.material = PogoVisualMaterialFactory.Create(TrailColor(profile.Trail, color), 0f, 0.88f, true);
        }

        private static void ConfigureLanding(PogoAvatarVisualRig rig, AvatarAttachmentVisualProfile profile, Color color)
        {
            if (rig.LandingFxSocket == null) return;
            var fx = rig.LandingFxSocket.GetComponent<PogoLandingSignatureFx>();
            if (fx == null) fx = rig.LandingFxSocket.gameObject.AddComponent<PogoLandingSignatureFx>();
            fx.Initialize(profile.Landing, PogoVisualMaterialFactory.Accent(color), profile.LandingRadius);
        }

        private static Color TrailColor(TrailVisualFamily family, Color baseColor)
        {
            switch (family)
            {
                case TrailVisualFamily.Pulse: return Color.Lerp(baseColor, Color.cyan, 0.48f);
                case TrailVisualFamily.Bubbles: return PogoVisualMaterialFactory.Lighten(baseColor, 0.46f);
                case TrailVisualFamily.Royal: return Color.Lerp(baseColor, new Color(1f, 0.82f, 0.18f), 0.38f);
                default: return PogoVisualMaterialFactory.Accent(baseColor);
            }
        }

        private static Transform Anchor(string objectName, Transform parent)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject Part(string name, PrimitiveType type, Transform root, Vector3 pos, Vector3 scale, Color color, Quaternion rotation, bool emission)
        {
            var go = PogoAvatarVisualRig.Primitive(name, type, root, pos, scale, color, rotation, 0.76f);
            if (emission)
                go.GetComponent<Renderer>().material = PogoVisualMaterialFactory.Create(color, 0f, 0.86f, true);
            return go;
        }

        private static void DisablePrototypeDecoration(Transform root)
        {
            if (root == null) return;
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name.StartsWith("Prototype")) child.gameObject.SetActive(false);
            }
        }
    }

    public sealed class PogoAuraOrbMotion : MonoBehaviour
    {
        private float _phase;
        private float _radius;
        private AuraVisualFamily _family;

        public void Initialize(float phase, float radius, AuraVisualFamily family)
        {
            _phase = phase;
            _radius = radius;
            _family = family;
        }

        private void Update()
        {
            var speed = _family == AuraVisualFamily.GraffitiOrbit ? 2.7f : 1.8f;
            var t = Time.time * speed + _phase;
            var vertical = _family == AuraVisualFamily.BubbleOrbit ? Mathf.Sin(t * 1.7f) * 0.12f : Mathf.Sin(t * 2f) * 0.045f;
            var radial = _radius * (1f + Mathf.Sin(t * 0.7f) * 0.045f);
            transform.localPosition = new Vector3(Mathf.Cos(t) * radial, vertical, Mathf.Sin(t) * radial);
        }
    }

    public sealed class PogoLandingSignatureFx : MonoBehaviour
    {
        private const int Points = 33;
        private LineRenderer _ring;
        private LandingVisualFamily _family;
        private Color _color;
        private float _targetRadius;
        private float _pulse;
        private float _previousHeight;
        private bool _initialized;

        public void Initialize(LandingVisualFamily family, Color color, float targetRadius)
        {
            if (_initialized) return;
            _initialized = true;
            _family = family;
            _color = color;
            _targetRadius = targetRadius;
            _previousHeight = transform.position.y;

            var go = new GameObject("LandingSignatureRing");
            go.transform.SetParent(transform, false);
            _ring = go.AddComponent<LineRenderer>();
            _ring.useWorldSpace = false;
            _ring.loop = true;
            _ring.positionCount = Points;
            _ring.startWidth = _ring.endWidth = 0.045f;
            _ring.material = PogoVisualMaterialFactory.Create(color, 0f, 0.86f, true);
            for (var i = 0; i < Points; i++)
            {
                var a = (i / (float)(Points - 1)) * Mathf.PI * 2f;
                var wobble = _family == LandingVisualFamily.StreetStamp && i % 2 == 0 ? 0.86f : 1f;
                _ring.SetPosition(i, new Vector3(Mathf.Cos(a) * wobble, 0f, Mathf.Sin(a) * wobble));
            }
            _ring.enabled = false;
        }

        private void Update()
        {
            if (!_initialized) return;
            var height = transform.position.y;
            var descendingAcrossGround = _previousHeight > 0.16f && height <= 0.13f;
            if (descendingAcrossGround) _pulse = 1f;
            _previousHeight = height;

            if (_pulse <= 0f || _ring == null) return;
            _pulse = Mathf.Max(0f, _pulse - Time.deltaTime * 4.8f);
            var t = 1f - _pulse;
            var scale = Mathf.Lerp(0.18f, _targetRadius, t);
            _ring.transform.localScale = new Vector3(scale, 1f, scale);
            var c = _color;
            c.a = _pulse;
            _ring.startColor = _ring.endColor = c;
            _ring.enabled = _pulse > 0f;
        }
    }
}
