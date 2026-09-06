using System.Collections;
using PogoDom.Cosmetics;
using UnityEngine;

namespace PogoDom.Runtime
{
    public enum PogoVisualQualityMode
    {
        Auto = 0,
        Lite = 1,
        Balanced = 2,
        Showcase = 3
    }

    /// <summary>
    /// Scales decoration after the viral visual layer has built itself. It never
    /// disables board tiles, competitors, pickups or hazard telegraphs.
    /// </summary>
    [DefaultExecutionOrder(1200)]
    [DisallowMultipleComponent]
    public sealed class PogoDomVisualQualityScaler : MonoBehaviour
    {
        [SerializeField] private PogoVisualQualityMode mode = PogoVisualQualityMode.Auto;

        public VisualQualityTier ActiveTier { get; private set; } = VisualQualityTier.Balanced;
        public VisualQualityBudget ActiveBudget => VisualQualityPolicy.Get(ActiveTier);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].GetComponent<PogoDomVisualQualityScaler>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomVisualQualityScaler>();
            }
        }

        private IEnumerator Start()
        {
            // M0.31 visual objects are created after the greybox. Waiting two
            // frames makes quality scaling independent from component ordering.
            yield return null;
            yield return null;
            ApplyNow();
        }

        public void ApplyNow()
        {
            ActiveTier = ResolveTier();
            var budget = VisualQualityPolicy.Get(ActiveTier);

            var trails = GetComponentsInChildren<TrailRenderer>(true);
            for (var i = 0; i < trails.Length; i++)
                trails[i].time = budget.TrailSeconds;

            var renderers = GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
                ScaleOutline(renderers[i], budget.OutlineScale);

            ApplyDecoration(transform, budget);
        }

        public int ScaleParticleBudget(int requested)
        {
            return ActiveBudget.ScaleParticles(requested);
        }

        private VisualQualityTier ResolveTier()
        {
            switch (mode)
            {
                case PogoVisualQualityMode.Lite: return VisualQualityTier.Lite;
                case PogoVisualQualityMode.Balanced: return VisualQualityTier.Balanced;
                case PogoVisualQualityMode.Showcase: return VisualQualityTier.Showcase;
                default:
                    return VisualQualityPolicy.SelectAuto(
                        SystemInfo.graphicsMemorySize,
                        SystemInfo.systemMemorySize,
                        SystemInfo.graphicsShaderLevel);
            }
        }

        private static void ApplyDecoration(Transform root, VisualQualityBudget budget)
        {
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                var objectName = child.name;

                // These objects communicate battle state and are never removed.
                if (!VisualQualityPolicy.IsEssentialVisualName(objectName))
                {
                    if (objectName.StartsWith("CityBlock_"))
                    {
                        var index = NumericSuffix(objectName);
                        var bucket = Mathf.Abs(index * 37 + 11) % 100;
                        child.gameObject.SetActive(bucket < Mathf.RoundToInt(budget.EnvironmentDensity * 100f));
                    }
                    else if (objectName.StartsWith("CityLight_"))
                    {
                        child.gameObject.SetActive(budget.DecorativeCityLights);
                    }
                    else if (objectName == "PickupHalo")
                    {
                        // Lite keeps the pickup itself but removes the second
                        // translucent/emissive shell to save overdraw.
                        child.gameObject.SetActive(budget.Tier != VisualQualityTier.Lite);
                    }
                    else if (objectName.StartsWith("SkillOrb_"))
                    {
                        var index = NumericSuffix(objectName);
                        child.gameObject.SetActive(index < budget.MaxSkillOrbiters);
                    }
                }

                // Only recurse into active branches; disabled city decoration
                // should not incur more presentation work.
                if (child.gameObject.activeSelf)
                    ApplyDecoration(child, budget);
            }
        }

        private static void ScaleOutline(Renderer renderer, float scale)
        {
            if (renderer == null) return;
            var materials = renderer.materials;
            for (var i = 0; i < materials.Length; i++)
            {
                var material = materials[i];
                if (material == null || !material.HasProperty("_OutlineWidth")) continue;
                var width = material.GetFloat("_OutlineWidth");
                material.SetFloat("_OutlineWidth", width * Mathf.Clamp01(scale));
            }
        }

        private static int NumericSuffix(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            var underscore = value.LastIndexOf('_');
            if (underscore < 0 || underscore + 1 >= value.Length) return 0;
            int parsed;
            return int.TryParse(value.Substring(underscore + 1), out parsed) ? parsed : 0;
        }
    }
}
