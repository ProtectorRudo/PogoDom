using System.Collections;
using System.Collections.Generic;
using PogoDom.Cosmetics;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Converts eligible presentation meshes to the PogoDom toon shader while
    /// pooling equivalent materials. Base board tiles and other dynamic-color
    /// visuals are deliberately excluded so independent state coloring remains safe.
    /// </summary>
    [DefaultExecutionOrder(1180)]
    [DisallowMultipleComponent]
    public sealed class PogoDomToonUpgrade : MonoBehaviour
    {
        private readonly Dictionary<ToonMaterialKey, Material> _materialPool = new Dictionary<ToonMaterialKey, Material>();
        private Shader _toonShader;
        private float _rescan;

        public int UniqueToonMaterialCount => _materialPool.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
                if (prototypes[i].GetComponent<PogoDomToonUpgrade>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomToonUpgrade>();
        }

        private IEnumerator Start()
        {
            // Avatar/pogo/attachment layers assemble over the first few frames.
            yield return null;
            yield return null;
            yield return null;
            ApplyToonMaterials();
        }

        private void Update()
        {
            _rescan -= Time.unscaledDeltaTime;
            if (_rescan > 0f) return;
            _rescan = 0.85f;
            ApplyToonMaterials();
        }

        public void ApplyToonMaterials()
        {
            if (_toonShader == null) _toonShader = Shader.Find("PogoDom/ToonLit");
            if (_toonShader == null) return;

            var renderers = GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer is LineRenderer || renderer is TrailRenderer || renderer is ParticleSystemRenderer) continue;
                var isAvatar = IsAvatar(renderer.transform);
                if (!VisualRuntimeBudgetPolicy.IsShareableToonCandidate(renderer.name, isAvatar)) continue;

                var source = renderer.sharedMaterial;
                if (source == null) continue;

                var color = ReadColor(source, Color.white);
                var emission = source.HasProperty("_EmissionColor") ? source.GetColor("_EmissionColor") : Color.black;
                var rimStrength = isAvatar ? 0.22f : 0.13f;
                var outlineWidth = OutlineWidth(renderer.transform);
                var key = ToonMaterialKey.From(color, emission, rimStrength, outlineWidth);

                Material pooled;
                if (!_materialPool.TryGetValue(key, out pooled))
                {
                    pooled = BuildMaterial(color, emission, rimStrength, outlineWidth);
                    pooled.name = "PogoDomToon_Pooled_" + _materialPool.Count;
                    _materialPool.Add(key, pooled);
                }

                if (renderer.sharedMaterial != pooled)
                    renderer.sharedMaterial = pooled;
            }
        }

        private Material BuildMaterial(Color color, Color emission, float rimStrength, float outlineWidth)
        {
            var replacement = new Material(_toonShader);
            replacement.SetColor("_BaseColor", color);
            replacement.SetColor("_ShadowColor", Color.Lerp(color, new Color(0.10f, 0.12f, 0.19f), 0.62f));
            replacement.SetColor("_RimColor", Color.Lerp(color, Color.white, 0.42f));
            replacement.SetColor("_EmissionColor", emission);
            replacement.SetFloat("_ShadowThreshold", 0.32f);
            replacement.SetFloat("_ShadowSoftness", 0.075f);
            replacement.SetFloat("_RimPower", 4.2f);
            replacement.SetFloat("_RimStrength", rimStrength);
            replacement.SetFloat("_OutlineWidth", outlineWidth);
            return replacement;
        }

        private static bool IsAvatar(Transform target)
        {
            var current = target;
            while (current != null)
            {
                if (current.GetComponent<PogoAvatarVisualRig>() != null || current.name == "AvatarVisual") return true;
                current = current.parent;
            }
            return false;
        }

        private static float OutlineWidth(Transform target)
        {
            if (IsAvatar(target)) return 0.014f;
            if (target.name == "PickupHalo") return 0.004f;
            return 0.010f;
        }

        private static Color ReadColor(Material material, Color fallback)
        {
            if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
            if (material.HasProperty("_Color")) return material.GetColor("_Color");
            return fallback;
        }

        private void OnDestroy()
        {
            foreach (var pair in _materialPool)
                if (pair.Value != null) Destroy(pair.Value);
            _materialPool.Clear();
        }

        private readonly struct ToonMaterialKey
        {
            private readonly int _baseR;
            private readonly int _baseG;
            private readonly int _baseB;
            private readonly int _baseA;
            private readonly int _emissionR;
            private readonly int _emissionG;
            private readonly int _emissionB;
            private readonly int _rim;
            private readonly int _outline;

            private ToonMaterialKey(Color color, Color emission, float rimStrength, float outlineWidth)
            {
                _baseR = QuantizeColor(color.r);
                _baseG = QuantizeColor(color.g);
                _baseB = QuantizeColor(color.b);
                _baseA = QuantizeColor(color.a);
                _emissionR = QuantizeColor(emission.r);
                _emissionG = QuantizeColor(emission.g);
                _emissionB = QuantizeColor(emission.b);
                _rim = Mathf.RoundToInt(rimStrength * 1000f);
                _outline = Mathf.RoundToInt(outlineWidth * 10000f);
            }

            public static ToonMaterialKey From(Color color, Color emission, float rimStrength, float outlineWidth)
            {
                return new ToonMaterialKey(color, emission, rimStrength, outlineWidth);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ToonMaterialKey)) return false;
                var other = (ToonMaterialKey)obj;
                return _baseR == other._baseR && _baseG == other._baseG && _baseB == other._baseB && _baseA == other._baseA &&
                       _emissionR == other._emissionR && _emissionG == other._emissionG && _emissionB == other._emissionB &&
                       _rim == other._rim && _outline == other._outline;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = 17;
                    hash = hash * 31 + _baseR;
                    hash = hash * 31 + _baseG;
                    hash = hash * 31 + _baseB;
                    hash = hash * 31 + _baseA;
                    hash = hash * 31 + _emissionR;
                    hash = hash * 31 + _emissionG;
                    hash = hash * 31 + _emissionB;
                    hash = hash * 31 + _rim;
                    hash = hash * 31 + _outline;
                    return hash;
                }
            }

            private static int QuantizeColor(float value)
            {
                return Mathf.RoundToInt(Mathf.Clamp01(value) * 255f);
            }
        }
    }
}
