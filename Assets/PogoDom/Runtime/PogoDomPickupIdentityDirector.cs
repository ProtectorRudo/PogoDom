using PogoDom.Cosmetics;
using PogoDom.Core;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Adds a compact silhouette language to live pickup GameObjects. It scans
    /// only direct prototype children and decorates newly spawned items once.
    /// </summary>
    [DefaultExecutionOrder(1190)]
    [DisallowMultipleComponent]
    public sealed class PogoDomPickupIdentityDirector : MonoBehaviour
    {
        private float _scanCooldown;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].GetComponent<PogoDomPickupIdentityDirector>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomPickupIdentityDirector>();
            }
        }

        private void Awake()
        {
            ScanNow();
        }

        private void Update()
        {
            _scanCooldown -= Time.deltaTime;
            if (_scanCooldown > 0f) return;
            _scanCooldown = 0.15f;
            ScanNow();
        }

        private void ScanNow()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                PowerUpKind kind;
                if (!TryKind(child.name, out kind)) continue;
                if (child.GetComponent<PogoPickupIdentityVisual>() != null) continue;

                var visual = child.gameObject.AddComponent<PogoPickupIdentityVisual>();
                visual.Initialize(kind);
            }
        }

        private static bool TryKind(string objectName, out PowerUpKind kind)
        {
            if (objectName.StartsWith("BankCrate_")) { kind = PowerUpKind.BankCrate; return true; }
            if (objectName.StartsWith("MysteryCrate_")) { kind = PowerUpKind.MysteryCrate; return true; }
            if (objectName.StartsWith("Arrow_")) { kind = PowerUpKind.Arrow; return true; }
            if (objectName.StartsWith("Speed_")) { kind = PowerUpKind.Speed; return true; }
            if (objectName.StartsWith("Missile_")) { kind = PowerUpKind.Missile; return true; }
            if (objectName.StartsWith("Padlock_")) { kind = PowerUpKind.Padlock; return true; }
            kind = PowerUpKind.None;
            return false;
        }
    }

    [DisallowMultipleComponent]
    public sealed class PogoPickupIdentityVisual : MonoBehaviour
    {
        private PickupVisualIdentity _identity;
        private Transform _identityRoot;
        private Vector3 _baseRootPosition;
        private Vector3 _baseRootScale;
        private float _phase;
        private bool _initialized;

        public void Initialize(PowerUpKind kind)
        {
            if (_initialized) return;
            _identity = PickupVisualIdentityPolicy.Get(kind);
            _initialized = true;
            _phase = ((name.GetHashCode() & 1023) / 1023f) * Mathf.PI * 2f;

            var root = new GameObject("PickupIdentity");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localScale = InverseScale(transform.localScale);
            _identityRoot = root.transform;
            _baseRootPosition = _identityRoot.localPosition;
            _baseRootScale = _identityRoot.localScale;

            BuildIdentity(kind);
        }

        private void Update()
        {
            if (!_initialized || _identityRoot == null) return;

            var wave = Mathf.Sin(Time.time * 4.8f + _phase);
            var local = _baseRootPosition;
            local.y += wave * _identity.BobAmplitude;
            _identityRoot.localPosition = local;

            if (_identity.SpinDegreesPerSecond > 0f)
                _identityRoot.Rotate(0f, _identity.SpinDegreesPerSecond * Time.deltaTime, 0f, Space.Self);

            var pulse = 1f + wave * _identity.PulseScale;
            _identityRoot.localScale = _baseRootScale * pulse;
        }

        private void BuildIdentity(PowerUpKind kind)
        {
            var accent = AccentFor(kind);
            var dark = new Color(0.07f, 0.08f, 0.12f);
            var light = PogoVisualMaterialFactory.Lighten(accent, 0.34f);

            switch (_identity.Archetype)
            {
                case PickupVisualArchetype.BankVault:
                    Part("VaultBandA", PrimitiveType.Cube, new Vector3(0f, 0f, -0.27f), new Vector3(0.46f, 0.07f, 0.035f), light, Quaternion.identity, false);
                    Part("VaultBandB", PrimitiveType.Cube, new Vector3(0f, 0f, 0.27f), new Vector3(0.46f, 0.07f, 0.035f), light, Quaternion.identity, false);
                    Part("VaultCoin", PrimitiveType.Cylinder, new Vector3(0f, 0.32f, 0f), new Vector3(0.15f, 0.035f, 0.15f), accent, Quaternion.Euler(90f, 0f, 0f), true);
                    Part("VaultCore", PrimitiveType.Sphere, new Vector3(0f, 0.01f, -0.30f), Vector3.one * 0.095f, accent, Quaternion.identity, true);
                    break;

                case PickupVisualArchetype.MysteryRelic:
                    Part("RelicDiamond", PrimitiveType.Cube, new Vector3(0f, 0.34f, 0f), Vector3.one * 0.18f, light, Quaternion.Euler(35f, 45f, 20f), true);
                    Part("RelicOrbL", PrimitiveType.Sphere, new Vector3(-0.25f, 0f, 0f), Vector3.one * 0.075f, accent, Quaternion.identity, true);
                    Part("RelicOrbR", PrimitiveType.Sphere, new Vector3(0.25f, 0f, 0f), Vector3.one * 0.075f, accent, Quaternion.identity, true);
                    Part("RelicOrbF", PrimitiveType.Sphere, new Vector3(0f, 0f, 0.25f), Vector3.one * 0.065f, light, Quaternion.identity, true);
                    break;

                case PickupVisualArchetype.DirectionArrow:
                    Part("ArrowShaft", PrimitiveType.Cube, new Vector3(0f, 0.08f, 0.02f), new Vector3(0.09f, 0.055f, 0.38f), light, Quaternion.identity, true);
                    Part("ArrowHeadL", PrimitiveType.Cube, new Vector3(-0.10f, 0.08f, 0.23f), new Vector3(0.08f, 0.055f, 0.25f), accent, Quaternion.Euler(0f, -42f, 0f), true);
                    Part("ArrowHeadR", PrimitiveType.Cube, new Vector3(0.10f, 0.08f, 0.23f), new Vector3(0.08f, 0.055f, 0.25f), accent, Quaternion.Euler(0f, 42f, 0f), true);
                    break;

                case PickupVisualArchetype.SpeedBurst:
                    Part("SpeedCore", PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.18f, light, Quaternion.identity, true);
                    Part("SpeedFinL", PrimitiveType.Cube, new Vector3(-0.19f, 0.02f, 0f), new Vector3(0.08f, 0.25f, 0.07f), accent, Quaternion.Euler(0f, 0f, -35f), true);
                    Part("SpeedFinR", PrimitiveType.Cube, new Vector3(0.19f, 0.02f, 0f), new Vector3(0.08f, 0.25f, 0.07f), accent, Quaternion.Euler(0f, 0f, 35f), true);
                    Part("SpeedSpike", PrimitiveType.Cube, new Vector3(0f, 0.26f, 0f), new Vector3(0.07f, 0.22f, 0.07f), light, Quaternion.Euler(0f, 0f, 18f), true);
                    break;

                case PickupVisualArchetype.MissileRocket:
                    Part("RocketNose", PrimitiveType.Sphere, new Vector3(0f, 0.31f, 0f), new Vector3(0.14f, 0.19f, 0.14f), light, Quaternion.identity, true);
                    Part("RocketFinL", PrimitiveType.Cube, new Vector3(-0.18f, -0.18f, 0f), new Vector3(0.16f, 0.11f, 0.06f), accent, Quaternion.Euler(0f, 0f, -25f), false);
                    Part("RocketFinR", PrimitiveType.Cube, new Vector3(0.18f, -0.18f, 0f), new Vector3(0.16f, 0.11f, 0.06f), accent, Quaternion.Euler(0f, 0f, 25f), false);
                    Part("RocketFlame", PrimitiveType.Sphere, new Vector3(0f, -0.34f, 0f), new Vector3(0.09f, 0.15f, 0.09f), new Color(1f, 0.65f, 0.05f), Quaternion.identity, true);
                    break;

                case PickupVisualArchetype.PadlockShield:
                    Part("LockBody", PrimitiveType.Cube, new Vector3(0f, -0.08f, 0f), new Vector3(0.34f, 0.28f, 0.12f), accent, Quaternion.identity, true);
                    Part("LockPostL", PrimitiveType.Cylinder, new Vector3(-0.13f, 0.17f, 0f), new Vector3(0.045f, 0.15f, 0.045f), light, Quaternion.identity, false);
                    Part("LockPostR", PrimitiveType.Cylinder, new Vector3(0.13f, 0.17f, 0f), new Vector3(0.045f, 0.15f, 0.045f), light, Quaternion.identity, false);
                    Part("LockTop", PrimitiveType.Cylinder, new Vector3(0f, 0.31f, 0f), new Vector3(0.045f, 0.17f, 0.045f), light, Quaternion.Euler(0f, 0f, 90f), false);
                    break;
            }
        }

        private GameObject Part(
            string objectName,
            PrimitiveType type,
            Vector3 localPosition,
            Vector3 localScale,
            Color color,
            Quaternion rotation,
            bool emission)
        {
            var go = PogoAvatarVisualRig.Primitive(objectName, type, _identityRoot, localPosition, localScale, color, rotation, 0.78f);
            if (emission)
                go.GetComponent<Renderer>().material = PogoVisualMaterialFactory.Create(color, 0f, 0.82f, true);
            return go;
        }

        private static Color AccentFor(PowerUpKind kind)
        {
            switch (kind)
            {
                case PowerUpKind.BankCrate: return new Color(0.72f, 0.22f, 0.95f);
                case PowerUpKind.MysteryCrate: return new Color(1.00f, 0.72f, 0.05f);
                case PowerUpKind.Arrow: return new Color(1.00f, 0.42f, 0.04f);
                case PowerUpKind.Speed: return new Color(0.05f, 0.90f, 1.00f);
                case PowerUpKind.Missile: return new Color(1.00f, 0.15f, 0.10f);
                case PowerUpKind.Padlock: return new Color(0.18f, 0.86f, 1.00f);
                default: return Color.white;
            }
        }

        private static Vector3 InverseScale(Vector3 scale)
        {
            return new Vector3(
                Mathf.Abs(scale.x) < 0.0001f ? 1f : 1f / scale.x,
                Mathf.Abs(scale.y) < 0.0001f ? 1f : 1f / scale.y,
                Mathf.Abs(scale.z) < 0.0001f ? 1f : 1f / scale.z);
        }
    }
}
