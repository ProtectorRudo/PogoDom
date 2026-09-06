using System.Collections.Generic;
using PogoDom.Cosmetics;
using UnityEngine;

namespace PogoDom.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PogoAvatarVisualRig : MonoBehaviour
    {
        private Transform _visualRoot;
        private Transform _pogoSpring;
        private Transform _headwearSocket;
        private Transform _backSocket;
        private Transform _skillSocket;
        private Transform _trailSocket;
        private Transform _landingFxSocket;
        private Transform _emoteRoot;
        private Transform _shadow;
        private PogoProceduralSkillSocket _proceduralSkill;
        private Vector3 _lastWorldPosition;
        private float _landingSquash;
        private bool _initialized;
        private Color _playerColor;
        private int _styleIndex;

        public Transform HeadwearSocket => _headwearSocket;
        public Transform BackSocket => _backSocket;
        public Transform SkillSocket => _skillSocket;
        public Transform TrailSocket => _trailSocket;
        public Transform LandingFxSocket => _landingFxSocket;
        public Transform EmoteRoot => _emoteRoot;

        public void Initialize(int styleIndex, Color playerColor)
        {
            if (_initialized) return;
            _initialized = true;
            _styleIndex = styleIndex;
            _playerColor = playerColor;
            _lastWorldPosition = transform.position;

            var legacy = GetComponent<Renderer>();
            if (legacy != null) legacy.enabled = false;

            _visualRoot = NewAnchor("AvatarVisual", transform, Vector3.zero);
            BuildPogo(_visualRoot, playerColor);
            BuildCharacter(_visualRoot, styleIndex, playerColor);
            BuildSockets(_visualRoot);
            BuildTrail(playerColor);
            BuildShadow();

            _proceduralSkill = _skillSocket.gameObject.AddComponent<PogoProceduralSkillSocket>();
            _proceduralSkill.Initialize(styleIndex, PogoVisualMaterialFactory.Accent(playerColor));
        }

        public void ApplySkillVisual(SkillVisualDefinition definition)
        {
            if (!_initialized || _proceduralSkill == null || definition == null) return;
            _proceduralSkill.Apply(definition);
        }

        private void Update()
        {
            if (!_initialized || _visualRoot == null) return;

            var dt = Mathf.Max(Time.deltaTime, 0.0001f);
            var now = transform.position;
            var velocity = (now - _lastWorldPosition) / dt;
            var altitude = Mathf.Max(0f, now.y - 0.65f);

            if (altitude < 0.07f && velocity.y < -0.12f)
                _landingSquash = 1f;
            _landingSquash = Mathf.MoveTowards(_landingSquash, 0f, dt * 6.5f);

            var travelStretch = Mathf.Clamp01(Mathf.Abs(velocity.y) / 4f) * 0.07f;
            var squash = _landingSquash * 0.14f;
            var targetScale = new Vector3(
                1f + squash - travelStretch * 0.45f,
                1f - squash + travelStretch,
                1f + squash - travelStretch * 0.45f);
            _visualRoot.localScale = Vector3.Lerp(_visualRoot.localScale, targetScale, 1f - Mathf.Exp(-18f * dt));

            var pitch = Mathf.Clamp(velocity.z * 2.1f, -11f, 11f);
            var roll = Mathf.Clamp(-velocity.x * 2.1f, -11f, 11f);
            var targetRotation = Quaternion.Euler(pitch, 0f, roll);
            _visualRoot.localRotation = Quaternion.Slerp(_visualRoot.localRotation, targetRotation, 1f - Mathf.Exp(-12f * dt));

            if (_pogoSpring != null)
            {
                var compression = Mathf.Clamp01(1f - altitude / 0.28f);
                var springScale = _pogoSpring.localScale;
                springScale.y = Mathf.Lerp(1f, 0.72f, compression * compression);
                _pogoSpring.localScale = springScale;
            }

            if (_shadow != null)
            {
                _shadow.position = new Vector3(now.x, 0.08f, now.z);
                var shadowScale = Mathf.Lerp(0.56f, 0.28f, Mathf.Clamp01(altitude / 0.9f));
                _shadow.localScale = new Vector3(shadowScale, 0.018f, shadowScale);
            }

            _lastWorldPosition = now;
        }

        private void OnDestroy()
        {
            if (_shadow != null) Destroy(_shadow.gameObject);
        }

        private void BuildPogo(Transform parent, Color color)
        {
            var pogoRoot = NewAnchor("PogoSocket", parent, new Vector3(0f, -0.28f, 0f));
            Primitive("PogoFoot", PrimitiveType.Cube, pogoRoot, new Vector3(0f, -0.28f, 0f), new Vector3(0.38f, 0.08f, 0.22f), color, Quaternion.identity, 0.65f);
            _pogoSpring = Primitive("PogoSpring", PrimitiveType.Cylinder, pogoRoot, new Vector3(0f, -0.05f, 0f), new Vector3(0.065f, 0.24f, 0.065f), new Color(0.15f, 0.17f, 0.22f), Quaternion.identity, 0.82f).transform;
            Primitive("PogoStem", PrimitiveType.Cylinder, pogoRoot, new Vector3(0f, 0.20f, 0f), new Vector3(0.052f, 0.20f, 0.052f), PogoVisualMaterialFactory.Lighten(color, 0.18f), Quaternion.identity, 0.76f);
            Primitive("PogoHandle", PrimitiveType.Cylinder, pogoRoot, new Vector3(0f, 0.40f, 0f), new Vector3(0.045f, 0.25f, 0.045f), new Color(0.12f, 0.13f, 0.18f), Quaternion.Euler(0f, 0f, 90f), 0.70f);
            Primitive("GripL", PrimitiveType.Sphere, pogoRoot, new Vector3(-0.28f, 0.40f, 0f), Vector3.one * 0.10f, color, Quaternion.identity, 0.72f);
            Primitive("GripR", PrimitiveType.Sphere, pogoRoot, new Vector3(0.28f, 0.40f, 0f), Vector3.one * 0.10f, color, Quaternion.identity, 0.72f);
        }

        private void BuildCharacter(Transform parent, int styleIndex, Color color)
        {
            var skin = new Color(0.93f, 0.72f, 0.58f);
            if (styleIndex == 2) skin = new Color(0.70f, 0.78f, 0.88f);
            if (styleIndex == 3) skin = new Color(0.90f, 0.68f, 0.45f);

            Primitive("Body", PrimitiveType.Capsule, parent, new Vector3(0f, 0.18f, 0f), new Vector3(0.33f, 0.34f, 0.33f), color, Quaternion.identity, 0.62f);
            Primitive("Belly", PrimitiveType.Sphere, parent, new Vector3(0f, 0.09f, -0.19f), new Vector3(0.22f, 0.28f, 0.11f), PogoVisualMaterialFactory.Lighten(color, 0.22f), Quaternion.identity, 0.55f);
            Primitive("Head", PrimitiveType.Sphere, parent, new Vector3(0f, 0.66f, 0f), new Vector3(0.42f, 0.39f, 0.39f), skin, Quaternion.identity, 0.58f);

            Primitive("EyeL", PrimitiveType.Sphere, parent, new Vector3(-0.13f, 0.72f, -0.34f), new Vector3(0.085f, 0.10f, 0.055f), Color.white, Quaternion.identity, 0.34f);
            Primitive("EyeR", PrimitiveType.Sphere, parent, new Vector3(0.13f, 0.72f, -0.34f), new Vector3(0.085f, 0.10f, 0.055f), Color.white, Quaternion.identity, 0.34f);
            Primitive("PupilL", PrimitiveType.Sphere, parent, new Vector3(-0.13f, 0.72f, -0.385f), Vector3.one * 0.038f, new Color(0.06f, 0.07f, 0.10f), Quaternion.identity, 0.30f);
            Primitive("PupilR", PrimitiveType.Sphere, parent, new Vector3(0.13f, 0.72f, -0.385f), Vector3.one * 0.038f, new Color(0.06f, 0.07f, 0.10f), Quaternion.identity, 0.30f);

            Primitive("ArmL", PrimitiveType.Capsule, parent, new Vector3(-0.34f, 0.28f, -0.02f), new Vector3(0.11f, 0.25f, 0.11f), color, Quaternion.Euler(0f, 0f, -48f), 0.58f);
            Primitive("ArmR", PrimitiveType.Capsule, parent, new Vector3(0.34f, 0.28f, -0.02f), new Vector3(0.11f, 0.25f, 0.11f), color, Quaternion.Euler(0f, 0f, 48f), 0.58f);
            Primitive("HandL", PrimitiveType.Sphere, parent, new Vector3(-0.49f, 0.08f, -0.02f), Vector3.one * 0.12f, skin, Quaternion.identity, 0.46f);
            Primitive("HandR", PrimitiveType.Sphere, parent, new Vector3(0.49f, 0.08f, -0.02f), Vector3.one * 0.12f, skin, Quaternion.identity, 0.46f);

            BuildSilhouetteAccessory(parent, styleIndex, color);
        }

        private static void BuildSilhouetteAccessory(Transform parent, int styleIndex, Color color)
        {
            var dark = new Color(0.08f, 0.09f, 0.14f);
            switch (styleIndex % 4)
            {
                case 0:
                    Primitive("HeroVisor", PrimitiveType.Cube, parent, new Vector3(0f, 0.83f, -0.31f), new Vector3(0.33f, 0.08f, 0.08f), dark, Quaternion.Euler(-8f, 0f, 0f), 0.78f);
                    Primitive("HeroCrest", PrimitiveType.Cube, parent, new Vector3(0f, 1.02f, 0f), new Vector3(0.10f, 0.22f, 0.22f), PogoVisualMaterialFactory.Lighten(color, 0.18f), Quaternion.Euler(0f, 0f, 12f), 0.65f);
                    break;
                case 1:
                    Primitive("SideFinL", PrimitiveType.Cube, parent, new Vector3(-0.40f, 0.74f, 0f), new Vector3(0.12f, 0.28f, 0.08f), dark, Quaternion.Euler(0f, 0f, -24f), 0.72f);
                    Primitive("SideFinR", PrimitiveType.Cube, parent, new Vector3(0.40f, 0.74f, 0f), new Vector3(0.12f, 0.28f, 0.08f), dark, Quaternion.Euler(0f, 0f, 24f), 0.72f);
                    break;
                case 2:
                    Primitive("AntennaStem", PrimitiveType.Cylinder, parent, new Vector3(0f, 1.03f, 0f), new Vector3(0.025f, 0.16f, 0.025f), dark, Quaternion.identity, 0.72f);
                    Primitive("AntennaOrb", PrimitiveType.Sphere, parent, new Vector3(0f, 1.19f, 0f), Vector3.one * 0.09f, PogoVisualMaterialFactory.Accent(color), Quaternion.identity, 0.82f);
                    break;
                default:
                    Primitive("EarL", PrimitiveType.Capsule, parent, new Vector3(-0.23f, 1.02f, 0.02f), new Vector3(0.13f, 0.27f, 0.13f), color, Quaternion.Euler(0f, 0f, -18f), 0.55f);
                    Primitive("EarR", PrimitiveType.Capsule, parent, new Vector3(0.23f, 1.02f, 0.02f), new Vector3(0.13f, 0.27f, 0.13f), color, Quaternion.Euler(0f, 0f, 18f), 0.55f);
                    break;
            }
        }

        private void BuildSockets(Transform parent)
        {
            _headwearSocket = NewAnchor("HeadwearSocket", parent, new Vector3(0f, 1.04f, 0f));
            _backSocket = NewAnchor("BackAccessorySocket", parent, new Vector3(0f, 0.45f, 0.30f));
            _skillSocket = NewAnchor("SkillSocket", parent, new Vector3(0f, 0.46f, 0f));
            _trailSocket = NewAnchor("TrailSocket", parent, new Vector3(0f, -0.20f, 0.18f));
            _landingFxSocket = NewAnchor("LandingFxSocket", parent, new Vector3(0f, -0.58f, 0f));
            _emoteRoot = NewAnchor("EmoteRoot", parent, Vector3.zero);

            if (_styleIndex == 1)
                Primitive("PrototypeBackpack", PrimitiveType.Cube, _backSocket, Vector3.zero, new Vector3(0.28f, 0.34f, 0.15f), PogoVisualMaterialFactory.Lighten(_playerColor, 0.12f), Quaternion.identity, 0.48f);
            if (_styleIndex == 0)
                Primitive("PrototypeCap", PrimitiveType.Cylinder, _headwearSocket, new Vector3(0f, 0.02f, 0f), new Vector3(0.28f, 0.05f, 0.28f), PogoVisualMaterialFactory.Accent(_playerColor), Quaternion.identity, 0.62f);
        }

        private void BuildTrail(Color color)
        {
            var trail = _trailSocket.gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.18f;
            trail.startWidth = 0.12f;
            trail.endWidth = 0.015f;
            trail.minVertexDistance = 0.05f;
            trail.numCornerVertices = 2;
            trail.numCapVertices = 2;
            trail.material = PogoVisualMaterialFactory.Create(PogoVisualMaterialFactory.Accent(color), 0.1f, 0.85f, true);
        }

        private void BuildShadow()
        {
            var shadowGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shadowGo.name = name + "_ContactShadow";
            if (transform.parent != null) shadowGo.transform.SetParent(transform.parent, true);
            RemoveCollider(shadowGo);
            shadowGo.transform.position = new Vector3(transform.position.x, 0.08f, transform.position.z);
            shadowGo.transform.localScale = new Vector3(0.52f, 0.018f, 0.52f);
            var renderer = shadowGo.GetComponent<Renderer>();
            renderer.material = PogoVisualMaterialFactory.Create(new Color(0.04f, 0.05f, 0.08f), 0f, 0.15f, false);
            _shadow = shadowGo.transform;
        }

        private static Transform NewAnchor(string objectName, Transform parent, Vector3 localPosition)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go.transform;
        }

        internal static GameObject Primitive(string objectName, PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Color color, Quaternion localRotation, float smoothness)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = objectName;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            go.transform.localScale = localScale;
            RemoveCollider(go);
            var renderer = go.GetComponent<Renderer>();
            renderer.material = PogoVisualMaterialFactory.Create(color, 0f, smoothness, false);
            return go;
        }

        internal static void RemoveCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }
    }

    internal static class PogoVisualMaterialFactory
    {
        public static Material Create(Color color, float metallic, float smoothness, bool emission)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var material = new Material(shader);

            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            if (emission)
            {
                material.EnableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", color * 1.8f);
            }
            return material;
        }

        public static Color Lighten(Color color, float amount)
        {
            return Color.Lerp(color, Color.white, Mathf.Clamp01(amount));
        }

        public static Color Accent(Color color)
        {
            var max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            if (max < 0.01f) return new Color(0.2f, 0.9f, 1f);
            return Lighten(color, 0.34f);
        }
    }

    [DisallowMultipleComponent]
    public sealed class PogoProceduralSkillSocket : MonoBehaviour
    {
        private readonly List<Transform> _orbiters = new List<Transform>();
        private Transform _core;
        private Color _color;
        private int _style;
        private float _speed = 120f;
        private float _radius = 0.56f;
        private float _vertical = 0.16f;

        public void Initialize(int style, Color color)
        {
            _style = style % 4;
            _color = color;
            Rebuild();
        }

        public void Apply(SkillVisualDefinition definition)
        {
            if (definition == null) return;
            _style = ((int)definition.Trigger + (int)definition.Rarity) % 4;
            _speed = 95f + (int)definition.Rarity * 22f;
            _radius = 0.48f + (int)definition.Rarity * 0.035f;
            Rebuild();
        }

        private void Rebuild()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            _orbiters.Clear();

            var core = PogoAvatarVisualRig.Primitive("SkillCore", PrimitiveType.Sphere, transform, Vector3.zero, Vector3.one * 0.11f, _color, Quaternion.identity, 0.9f);
            core.GetComponent<Renderer>().material = PogoVisualMaterialFactory.Create(_color, 0f, 0.92f, true);
            _core = core.transform;

            var count = _style == 2 ? 4 : 3;
            for (var i = 0; i < count; i++)
            {
                var orb = PogoAvatarVisualRig.Primitive("SkillOrb_" + i, PrimitiveType.Sphere, transform, Vector3.zero, Vector3.one * (_style == 1 ? 0.075f : 0.06f), _color, Quaternion.identity, 0.86f);
                orb.GetComponent<Renderer>().material = PogoVisualMaterialFactory.Create(_color, 0f, 0.9f, true);
                _orbiters.Add(orb.transform);
            }
        }

        private void Update()
        {
            if (_core == null) return;
            var t = Time.time * _speed * Mathf.Deg2Rad;
            _core.localScale = Vector3.one * (0.10f + Mathf.Sin(t * 1.7f) * 0.018f);

            for (var i = 0; i < _orbiters.Count; i++)
            {
                var phase = t + i * (Mathf.PI * 2f / _orbiters.Count);
                var y = Mathf.Sin(phase * (_style == 3 ? 2f : 1f)) * _vertical;
                var radius = _radius * (1f + Mathf.Sin(phase * 0.5f) * 0.06f);
                _orbiters[i].localPosition = new Vector3(Mathf.Cos(phase) * radius, y, Mathf.Sin(phase) * radius);
            }
        }
    }
}
