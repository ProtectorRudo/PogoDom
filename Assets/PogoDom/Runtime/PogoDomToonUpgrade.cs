using System.Collections;
using UnityEngine;

namespace PogoDom.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PogoDomToonUpgrade : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].GetComponent<PogoDomToonUpgrade>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomToonUpgrade>();
            }
        }

        private IEnumerator Start()
        {
            // The viral visual director creates presentation-only geometry after
            // the greybox. Wait long enough for that hierarchy to exist.
            yield return null;
            yield return null;
            ApplyToonMaterials();
        }

        public void ApplyToonMaterials()
        {
            var shader = Shader.Find("PogoDom/ToonLit");
            if (shader == null) return;

            var renderers = GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer.material == null) continue;

                var source = renderer.material;
                var color = ReadColor(source, Color.white);
                var emission = source.HasProperty("_EmissionColor") ? source.GetColor("_EmissionColor") : Color.black;
                var replacement = new Material(shader);
                replacement.SetColor("_BaseColor", color);
                replacement.SetColor("_ShadowColor", Color.Lerp(color, new Color(0.10f, 0.12f, 0.19f), 0.62f));
                replacement.SetColor("_RimColor", Color.Lerp(color, Color.white, 0.42f));
                replacement.SetColor("_EmissionColor", emission);
                replacement.SetFloat("_ShadowThreshold", 0.32f);
                replacement.SetFloat("_ShadowSoftness", 0.075f);
                replacement.SetFloat("_RimPower", 4.2f);
                replacement.SetFloat("_RimStrength", IsAvatar(renderer.transform) ? 0.22f : 0.10f);
                replacement.SetFloat("_OutlineWidth", OutlineWidth(renderer.transform));
                renderer.material = replacement;
            }
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
            var n = target.name;
            if (n.StartsWith("MysteryCrate_") || n.StartsWith("BankCrate_") || n.StartsWith("Missile_") || n.StartsWith("Speed_") || n.StartsWith("Padlock_"))
                return 0.010f;
            if (n.StartsWith("BeaconCap_") || n.StartsWith("Skill")) return 0.007f;
            return 0f;
        }

        private static Color ReadColor(Material material, Color fallback)
        {
            if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
            if (material.HasProperty("_Color")) return material.GetColor("_Color");
            return fallback;
        }
    }
}
