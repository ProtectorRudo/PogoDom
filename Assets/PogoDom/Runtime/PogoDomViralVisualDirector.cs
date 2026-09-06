using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PogoDom.Runtime
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    public sealed class PogoDomViralVisualDirector : MonoBehaviour
    {
        private static readonly string[] PlayerNames = { "YOU", "BOT A", "BOT B", "BOT C" };
        private static readonly Color[] PlayerColors =
        {
            new Color(0.98f, 0.20f, 0.24f),
            new Color(0.15f, 0.88f, 0.38f),
            new Color(0.18f, 0.48f, 1.00f),
            new Color(1.00f, 0.78f, 0.10f)
        };

        private readonly List<PogoDomTileJuice> _tiles = new List<PogoDomTileJuice>();
        private Transform _visualEnvironment;
        private PogoDomCameraJuice _cameraJuice;
        private float _rescanTimer;
        private bool _built;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].GetComponent<PogoDomViralVisualDirector>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomViralVisualDirector>();
            }
        }

        private IEnumerator Start()
        {
            // PogoDomPrototypeBootstrap creates the greybox in Awake. Waiting a
            // frame keeps the visual layer independent from battle setup order.
            yield return null;
            BuildOnce();
        }

        private void Update()
        {
            if (!_built) return;

            var changedTiles = 0;
            for (var i = 0; i < _tiles.Count; i++)
                if (_tiles[i] != null && _tiles[i].ConsumeChangeFlag()) changedTiles++;

            if (_cameraJuice != null && changedTiles >= 4)
                _cameraJuice.Impulse(Mathf.Clamp01(changedTiles / 18f));

            _rescanTimer -= Time.deltaTime;
            if (_rescanTimer <= 0f)
            {
                _rescanTimer = 0.75f;
                DecoratePlayers();
                DecoratePickups();
            }
        }

        private void BuildOnce()
        {
            if (_built) return;
            _built = true;

            ConfigureCameraAndLighting();
            DecorateTiles();
            BuildArenaFrame();
            DecoratePlayers();
            DecoratePickups();
        }

        private void ConfigureCameraAndLighting()
        {
            var camera = Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.035f, 0.045f, 0.075f);
                camera.fieldOfView = 39f;
                _cameraJuice = camera.GetComponent<PogoDomCameraJuice>();
                if (_cameraJuice == null) _cameraJuice = camera.gameObject.AddComponent<PogoDomCameraJuice>();
            }

            RenderSettings.ambientLight = new Color(0.36f, 0.40f, 0.52f);
            var lights = Object.FindObjectsOfType<Light>();
            for (var i = 0; i < lights.Length; i++)
            {
                if (lights[i].type != LightType.Directional) continue;
                lights[i].intensity = 1.15f;
                lights[i].color = new Color(1.00f, 0.94f, 0.86f);
                lights[i].shadows = LightShadows.Soft;
                break;
            }
        }

        private void DecorateTiles()
        {
            _tiles.Clear();
            var originalChildCount = transform.childCount;
            for (var i = 0; i < originalChildCount; i++)
            {
                var child = transform.GetChild(i);
                if (!IsBaseTileName(child.name)) continue;
                var juice = child.GetComponent<PogoDomTileJuice>();
                if (juice == null) juice = child.gameObject.AddComponent<PogoDomTileJuice>();
                juice.Initialize();
                _tiles.Add(juice);
            }
        }

        private void DecoratePlayers()
        {
            for (var p = 0; p < PlayerNames.Length; p++)
            {
                var player = FindDirectChild(PlayerNames[p]);
                if (player == null) continue;
                var rig = player.GetComponent<PogoAvatarVisualRig>();
                if (rig == null) rig = player.gameObject.AddComponent<PogoAvatarVisualRig>();

                var renderer = player.GetComponent<Renderer>();
                var color = renderer != null ? ReadColor(renderer.material, PlayerColors[p]) : PlayerColors[p];
                if (ColorDistance(color, new Color(0.72f, 0.72f, 0.76f)) < 0.02f) color = PlayerColors[p];
                rig.Initialize(p, color);
            }
        }

        private void DecoratePickups()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (!IsPickupName(child.name)) continue;
                if (child.GetComponent<PogoPickupJuice>() == null)
                    child.gameObject.AddComponent<PogoPickupJuice>();
            }
        }

        private void BuildArenaFrame()
        {
            if (_visualEnvironment != null) return;
            if (_tiles.Count == 0) return;

            var minX = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var minZ = float.PositiveInfinity;
            var maxZ = float.NegativeInfinity;
            for (var i = 0; i < _tiles.Count; i++)
            {
                var p = _tiles[i].transform.position;
                minX = Mathf.Min(minX, p.x);
                maxX = Mathf.Max(maxX, p.x);
                minZ = Mathf.Min(minZ, p.z);
                maxZ = Mathf.Max(maxZ, p.z);
            }

            var environment = new GameObject("ViralArenaEnvironment");
            environment.transform.SetParent(transform, false);
            _visualEnvironment = environment.transform;

            var center = new Vector3((minX + maxX) * 0.5f, -0.18f, (minZ + maxZ) * 0.5f);
            var width = maxX - minX + 2.15f;
            var depth = maxZ - minZ + 2.15f;
            CreateEnvironmentPrimitive("ArenaPlinth", PrimitiveType.Cube, center, new Vector3(width, 0.26f, depth), new Color(0.075f, 0.085f, 0.13f), 0.46f);
            CreateEnvironmentPrimitive("ArenaInset", PrimitiveType.Cube, center + Vector3.up * 0.15f, new Vector3(width - 0.52f, 0.055f, depth - 0.52f), new Color(0.12f, 0.135f, 0.19f), 0.70f);

            var rail = new Color(0.17f, 0.19f, 0.27f);
            CreateEnvironmentPrimitive("RailNorth", PrimitiveType.Cube, new Vector3(center.x, 0.14f, maxZ + 0.83f), new Vector3(width, 0.32f, 0.22f), rail, 0.72f);
            CreateEnvironmentPrimitive("RailSouth", PrimitiveType.Cube, new Vector3(center.x, 0.14f, minZ - 0.83f), new Vector3(width, 0.32f, 0.22f), rail, 0.72f);
            CreateEnvironmentPrimitive("RailEast", PrimitiveType.Cube, new Vector3(maxX + 0.83f, 0.14f, center.z), new Vector3(0.22f, 0.32f, depth), rail, 0.72f);
            CreateEnvironmentPrimitive("RailWest", PrimitiveType.Cube, new Vector3(minX - 0.83f, 0.14f, center.z), new Vector3(0.22f, 0.32f, depth), rail, 0.72f);

            BuildCornerBeacons(minX, maxX, minZ, maxZ);
            BuildBackdropCity(minX, maxX, maxZ);
        }

        private void BuildCornerBeacons(float minX, float maxX, float minZ, float maxZ)
        {
            var positions = new[]
            {
                new Vector3(minX - 0.75f, 0.44f, minZ - 0.75f),
                new Vector3(minX - 0.75f, 0.44f, maxZ + 0.75f),
                new Vector3(maxX + 0.75f, 0.44f, maxZ + 0.75f),
                new Vector3(maxX + 0.75f, 0.44f, minZ - 0.75f)
            };

            for (var i = 0; i < positions.Length; i++)
            {
                CreateEnvironmentPrimitive("BeaconBody_" + i, PrimitiveType.Cylinder, positions[i], new Vector3(0.18f, 0.42f, 0.18f), new Color(0.09f, 0.10f, 0.16f), 0.68f);
                var cap = CreateEnvironmentPrimitive("BeaconCap_" + i, PrimitiveType.Sphere, positions[i] + Vector3.up * 0.48f, Vector3.one * 0.18f, PlayerColors[i], 0.90f);
                cap.GetComponent<Renderer>().material = PogoVisualMaterialFactory.Create(PlayerColors[i], 0f, 0.9f, true);
            }
        }

        private void BuildBackdropCity(float minX, float maxX, float maxZ)
        {
            var dark = new Color(0.065f, 0.075f, 0.12f);
            const int count = 14;
            for (var i = 0; i < count; i++)
            {
                var t = count == 1 ? 0f : i / (float)(count - 1);
                var x = Mathf.Lerp(minX - 1.4f, maxX + 1.4f, t);
                var height = 0.65f + ((i * 37) % 7) * 0.14f;
                var z = maxZ + 1.75f + ((i % 3) * 0.13f);
                var building = CreateEnvironmentPrimitive("CityBlock_" + i, PrimitiveType.Cube, new Vector3(x, height * 0.5f - 0.05f, z), new Vector3(0.42f, height, 0.48f), dark, 0.38f);
                if (i % 3 == 0)
                {
                    var light = CreateEnvironmentPrimitive("CityLight_" + i, PrimitiveType.Cube, building.transform.position + new Vector3(0f, height * 0.20f, -0.255f), new Vector3(0.16f, 0.08f, 0.02f), PlayerColors[i % 4], 0.82f);
                    light.GetComponent<Renderer>().material = PogoVisualMaterialFactory.Create(PlayerColors[i % 4], 0f, 0.82f, true);
                }
            }
        }

        private GameObject CreateEnvironmentPrimitive(string objectName, PrimitiveType type, Vector3 worldPosition, Vector3 scale, Color color, float smoothness)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = objectName;
            go.transform.SetParent(_visualEnvironment, true);
            go.transform.position = worldPosition;
            go.transform.localScale = scale;
            PogoAvatarVisualRig.RemoveCollider(go);
            go.GetComponent<Renderer>().material = PogoVisualMaterialFactory.Create(color, 0f, smoothness, false);
            return go;
        }

        private Transform FindDirectChild(string childName)
        {
            for (var i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).name == childName) return transform.GetChild(i);
            return null;
        }

        private static bool IsBaseTileName(string objectName)
        {
            return objectName.StartsWith("Tile_") &&
                   !objectName.EndsWith("_Rim") &&
                   !objectName.EndsWith("_Glow");
        }

        private static bool IsPickupName(string objectName)
        {
            return objectName.StartsWith("BankCrate_") ||
                   objectName.StartsWith("MysteryCrate_") ||
                   objectName.StartsWith("Arrow_") ||
                   objectName.StartsWith("Speed_") ||
                   objectName.StartsWith("Missile_") ||
                   objectName.StartsWith("Padlock_");
        }

        internal static Color ReadColor(Material material, Color fallback)
        {
            if (material == null) return fallback;
            if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
            if (material.HasProperty("_Color")) return material.GetColor("_Color");
            return fallback;
        }

        private static float ColorDistance(Color a, Color b)
        {
            var d = new Vector3(a.r - b.r, a.g - b.g, a.b - b.b);
            return d.sqrMagnitude;
        }
    }

    [DisallowMultipleComponent]
    public sealed class PogoDomTileJuice : MonoBehaviour
    {
        private Renderer _topRenderer;
        private Renderer _glowRenderer;
        private Transform _glowTransform;
        private Vector3 _baseScale;
        private Color _lastColor;
        private float _pulse;
        private bool _changed;
        private bool _initialized;

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            _topRenderer = GetComponent<Renderer>();
            if (_topRenderer == null) return;

            _baseScale = new Vector3(0.86f, 0.12f, 0.86f);
            transform.localScale = _baseScale;
            _lastColor = PogoDomViralVisualDirector.ReadColor(_topRenderer.material, new Color(0.72f, 0.72f, 0.76f));

            var parent = transform.parent;
            var underlay = GameObject.CreatePrimitive(PrimitiveType.Cube);
            underlay.name = name + "_Rim";
            underlay.transform.SetParent(parent, true);
            underlay.transform.position = transform.position + Vector3.down * 0.045f;
            underlay.transform.localScale = new Vector3(0.98f, 0.075f, 0.98f);
            PogoAvatarVisualRig.RemoveCollider(underlay);
            underlay.GetComponent<Renderer>().material = PogoVisualMaterialFactory.Create(new Color(0.055f, 0.06f, 0.095f), 0f, 0.48f, false);

            var glow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glow.name = name + "_Glow";
            glow.transform.SetParent(parent, true);
            glow.transform.position = transform.position + Vector3.down * 0.005f;
            glow.transform.localScale = new Vector3(0.91f, 0.025f, 0.91f);
            PogoAvatarVisualRig.RemoveCollider(glow);
            _glowRenderer = glow.GetComponent<Renderer>();
            _glowRenderer.material = PogoVisualMaterialFactory.Create(_lastColor, 0f, 0.9f, true);
            _glowTransform = glow.transform;
        }

        private void Update()
        {
            if (_topRenderer == null) return;
            var current = PogoDomViralVisualDirector.ReadColor(_topRenderer.material, _lastColor);
            var delta = new Vector3(current.r - _lastColor.r, current.g - _lastColor.g, current.b - _lastColor.b);
            if (delta.sqrMagnitude > 0.0025f)
            {
                _lastColor = current;
                _pulse = 1f;
                _changed = true;
                ApplyGlowColor(current);
            }

            _pulse = Mathf.MoveTowards(_pulse, 0f, Time.deltaTime * 5.5f);
            var pop = 1f + Mathf.Sin(_pulse * Mathf.PI) * 0.075f;
            transform.localScale = new Vector3(_baseScale.x * pop, _baseScale.y * (1f + _pulse * 0.28f), _baseScale.z * pop);
            if (_glowTransform != null)
            {
                var glowPop = 1f + _pulse * 0.13f;
                _glowTransform.localScale = new Vector3(0.91f * glowPop, 0.025f, 0.91f * glowPop);
            }
        }

        public bool ConsumeChangeFlag()
        {
            if (!_changed) return false;
            _changed = false;
            return true;
        }

        private void ApplyGlowColor(Color color)
        {
            if (_glowRenderer == null) return;
            var bright = PogoVisualMaterialFactory.Lighten(color, 0.22f);
            var material = _glowRenderer.material;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", bright);
            if (material.HasProperty("_Color")) material.SetColor("_Color", bright);
            if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", bright * 1.8f);
        }
    }

    [DisallowMultipleComponent]
    public sealed class PogoPickupJuice : MonoBehaviour
    {
        private Vector3 _baseScale;
        private float _phase;
        private Transform _halo;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _phase = (name.GetHashCode() & 255) * 0.024f;

            var halo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            halo.name = "PickupHalo";
            halo.transform.SetParent(transform, false);
            halo.transform.localPosition = Vector3.zero;
            halo.transform.localScale = Vector3.one * 1.22f;
            PogoAvatarVisualRig.RemoveCollider(halo);
            var renderer = halo.GetComponent<Renderer>();
            var source = GetComponent<Renderer>();
            var color = source == null ? new Color(0.25f, 0.9f, 1f) : PogoDomViralVisualDirector.ReadColor(source.material, Color.white);
            renderer.material = PogoVisualMaterialFactory.Create(PogoVisualMaterialFactory.Lighten(color, 0.25f), 0f, 0.82f, true);
            _halo = halo.transform;
        }

        private void Update()
        {
            var wave = Mathf.Sin(Time.time * 4.2f + _phase);
            transform.localScale = _baseScale * (1f + wave * 0.035f);
            if (_halo != null)
            {
                var haloScale = 1.17f + Mathf.Sin(Time.time * 5.4f + _phase) * 0.07f;
                _halo.localScale = Vector3.one * haloScale;
                _halo.Rotate(0f, 55f * Time.deltaTime, 0f, Space.Self);
            }
        }
    }

    [DisallowMultipleComponent]
    public sealed class PogoDomCameraJuice : MonoBehaviour
    {
        private Vector3 _basePosition;
        private Quaternion _baseRotation;
        private Camera _camera;
        private float _baseFov;
        private float _impulse;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _basePosition = transform.localPosition;
            _baseRotation = transform.localRotation;
            _baseFov = _camera == null ? 39f : _camera.fieldOfView;
        }

        public void Impulse(float strength)
        {
            _impulse = Mathf.Max(_impulse, Mathf.Clamp01(strength));
        }

        private void LateUpdate()
        {
            _impulse = Mathf.MoveTowards(_impulse, 0f, Time.deltaTime * 4.5f);
            if (_impulse <= 0.001f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, _basePosition, 1f - Mathf.Exp(-14f * Time.deltaTime));
                transform.localRotation = Quaternion.Slerp(transform.localRotation, _baseRotation, 1f - Mathf.Exp(-14f * Time.deltaTime));
                if (_camera != null) _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _baseFov, 1f - Mathf.Exp(-12f * Time.deltaTime));
                return;
            }

            var noiseX = Mathf.Sin(Time.time * 71f) * 0.035f * _impulse;
            var noiseY = Mathf.Sin(Time.time * 93f + 1.7f) * 0.022f * _impulse;
            transform.localPosition = _basePosition + new Vector3(noiseX, noiseY, 0f);
            transform.localRotation = _baseRotation * Quaternion.Euler(noiseY * 18f, noiseX * 10f, -noiseX * 25f);
            if (_camera != null) _camera.fieldOfView = _baseFov - _impulse * 0.65f;
        }
    }
}
