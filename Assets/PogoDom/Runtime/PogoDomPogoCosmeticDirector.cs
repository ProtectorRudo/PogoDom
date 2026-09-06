using System.Collections;
using PogoDom.Cosmetics;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Decorates the neutral gameplay pogo with cosmetic silhouette families.
    /// The decorator lives below AvatarVisual and never owns player movement.
    /// </summary>
    [DefaultExecutionOrder(1100)]
    [DisallowMultipleComponent]
    public sealed class PogoDomPogoCosmeticDirector : MonoBehaviour
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
            {
                if (prototypes[i].GetComponent<PogoDomPogoCosmeticDirector>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomPogoCosmeticDirector>();
            }
        }

        private IEnumerator Start()
        {
            // M0.31 creates AvatarVisual one frame after the greybox.
            yield return null;
            yield return null;
            DecoratePlayers();
        }

        private void Update()
        {
            _rescan -= Time.deltaTime;
            if (_rescan > 0f) return;
            _rescan = 1f;
            DecoratePlayers();
        }

        private void DecoratePlayers()
        {
            for (var playerId = 0; playerId < PlayerNames.Length; playerId++)
            {
                var player = FindDirectChild(PlayerNames[playerId]);
                if (player == null) continue;
                var pogo = FindDeep(player, "PogoSocket");
                if (pogo == null) continue;

                var decorator = pogo.GetComponent<PogoCosmeticSilhouette>();
                if (decorator == null) decorator = pogo.gameObject.AddComponent<PogoCosmeticSilhouette>();
                decorator.Initialize(PogoSilhouetteProfiles.ForPlayerSlot(playerId), PlayerColors[playerId]);
            }
        }

        private Transform FindDirectChild(string objectName)
        {
            for (var i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).name == objectName) return transform.GetChild(i);
            return null;
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

    [DisallowMultipleComponent]
    public sealed class PogoCosmeticSilhouette : MonoBehaviour
    {
        private bool _initialized;
        private PogoSilhouetteFamily _family;

        public void Initialize(PogoSilhouetteFamily family, Color color)
        {
            if (_initialized)
            {
                // Current battle slots keep one profile for the whole match. If a
                // future locker swaps profile live, rebuilding is safer than
                // silently stacking geometry.
                if (_family == family) return;
                ClearDecoration();
            }

            _initialized = true;
            _family = family;
            var profile = PogoSilhouetteProfiles.Get(family);
            ApplyBaseProportions(profile);
            BuildDecoration(profile, color);
        }

        private void ApplyBaseProportions(PogoSilhouetteProfile profile)
        {
            var foot = FindChild("PogoFoot");
            if (foot != null)
            {
                var s = foot.localScale;
                s.x *= profile.FootWidthScale;
                foot.localScale = s;
            }

            var spring = FindChild("PogoSpring");
            if (spring != null)
            {
                var s = spring.localScale;
                s.x *= profile.SpringWidthScale;
                s.z *= profile.SpringWidthScale;
                spring.localScale = s;
            }

            var handle = FindChild("PogoHandle");
            if (handle != null)
            {
                var s = handle.localScale;
                s.y *= profile.HandleWidthScale;
                handle.localScale = s;
            }
        }

        private void BuildDecoration(PogoSilhouetteProfile profile, Color color)
        {
            var root = new GameObject("PogoCosmeticDecoration");
            root.transform.SetParent(transform, false);
            var accent = PogoVisualMaterialFactory.Accent(color);
            var dark = new Color(0.07f, 0.08f, 0.12f);

            switch (profile.Family)
            {
                case PogoSilhouetteFamily.HeroSport:
                    Part("HeroFinL", PrimitiveType.Cube, root.transform, new Vector3(-0.18f, -0.31f, 0.03f), new Vector3(0.15f, 0.045f, 0.25f), accent, Quaternion.Euler(0f, -18f, 0f), profile.UsesEmissionAccent);
                    Part("HeroFinR", PrimitiveType.Cube, root.transform, new Vector3(0.18f, -0.31f, 0.03f), new Vector3(0.15f, 0.045f, 0.25f), accent, Quaternion.Euler(0f, 18f, 0f), profile.UsesEmissionAccent);
                    Part("HeroBadge", PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.20f, -0.07f), Vector3.one * 0.075f, accent, Quaternion.identity, true);
                    break;

                case PogoSilhouetteFamily.TricksterCoil:
                    Part("CoilRingLow", PrimitiveType.TorusFallback(), root.transform, new Vector3(0f, -0.10f, 0f), new Vector3(0.17f, 0.035f, 0.17f), accent, Quaternion.identity, false);
                    Part("CoilRingHigh", PrimitiveType.TorusFallback(), root.transform, new Vector3(0f, 0.07f, 0f), new Vector3(0.15f, 0.035f, 0.15f), accent, Quaternion.identity, false);
                    Part("TrickBell", PrimitiveType.Sphere, root.transform, new Vector3(0f, -0.34f, 0f), new Vector3(0.14f, 0.07f, 0.14f), color, Quaternion.identity, false);
                    break;

                case PogoSilhouetteFamily.TechPulse:
                    Part("PulseCore", PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.08f, -0.07f), Vector3.one * 0.10f, accent, Quaternion.identity, true);
                    Part("PulseRailL", PrimitiveType.Cube, root.transform, new Vector3(-0.11f, 0.12f, 0f), new Vector3(0.025f, 0.30f, 0.025f), accent, Quaternion.identity, true);
                    Part("PulseRailR", PrimitiveType.Cube, root.transform, new Vector3(0.11f, 0.12f, 0f), new Vector3(0.025f, 0.30f, 0.025f), accent, Quaternion.identity, true);
                    Part("TechNode", PrimitiveType.Cube, root.transform, new Vector3(0f, -0.31f, -0.08f), new Vector3(0.18f, 0.045f, 0.12f), dark, Quaternion.Euler(0f, 45f, 0f), false);
                    break;

                case PogoSilhouetteFamily.MascotBubble:
                    Part("BubbleCore", PrimitiveType.Sphere, root.transform, new Vector3(0f, -0.08f, 0f), Vector3.one * 0.17f, PogoVisualMaterialFactory.Lighten(color, 0.28f), Quaternion.identity, true);
                    Part("BubbleGripL", PrimitiveType.Sphere, root.transform, new Vector3(-0.34f, 0.40f, 0f), Vector3.one * 0.13f, accent, Quaternion.identity, false);
                    Part("BubbleGripR", PrimitiveType.Sphere, root.transform, new Vector3(0.34f, 0.40f, 0f), Vector3.one * 0.13f, accent, Quaternion.identity, false);
                    Part("BubbleFoot", PrimitiveType.Sphere, root.transform, new Vector3(0f, -0.34f, 0f), new Vector3(0.30f, 0.055f, 0.20f), color, Quaternion.identity, false);
                    break;

                case PogoSilhouetteFamily.StreetDeck:
                    Part("Deck", PrimitiveType.Cube, root.transform, new Vector3(0f, -0.33f, 0f), new Vector3(0.55f, 0.045f, 0.16f), dark, Quaternion.identity, false);
                    Part("DeckTipL", PrimitiveType.Sphere, root.transform, new Vector3(-0.50f, -0.32f, 0f), new Vector3(0.12f, 0.05f, 0.16f), accent, Quaternion.identity, false);
                    Part("DeckTipR", PrimitiveType.Sphere, root.transform, new Vector3(0.50f, -0.32f, 0f), new Vector3(0.12f, 0.05f, 0.16f), accent, Quaternion.identity, false);
                    Part("StreetBadge", PrimitiveType.Cube, root.transform, new Vector3(0f, 0.24f, -0.06f), new Vector3(0.10f, 0.08f, 0.03f), accent, Quaternion.Euler(0f, 0f, 22f), false);
                    break;

                case PogoSilhouetteFamily.CaptainCrest:
                    Part("GuardL", PrimitiveType.Cube, root.transform, new Vector3(-0.18f, 0.34f, 0f), new Vector3(0.06f, 0.15f, 0.04f), accent, Quaternion.Euler(0f, 0f, -18f), true);
                    Part("GuardR", PrimitiveType.Cube, root.transform, new Vector3(0.18f, 0.34f, 0f), new Vector3(0.06f, 0.15f, 0.04f), accent, Quaternion.Euler(0f, 0f, 18f), true);
                    Part("CaptainCrest", PrimitiveType.Cube, root.transform, new Vector3(0f, -0.33f, -0.08f), new Vector3(0.16f, 0.10f, 0.05f), accent, Quaternion.Euler(0f, 0f, 45f), true);
                    Part("CaptainCore", PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.10f, 0f), Vector3.one * 0.075f, color, Quaternion.identity, true);
                    break;
            }
        }

        private static void Part(string objectName, PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Color color, Quaternion rotation, bool emission)
        {
            var go = PogoAvatarVisualRig.Primitive(objectName, type, parent, localPosition, localScale, color, rotation, 0.78f);
            if (emission)
                go.GetComponent<Renderer>().material = PogoVisualMaterialFactory.Create(color, 0f, 0.84f, true);
        }

        private Transform FindChild(string childName)
        {
            for (var i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).name == childName) return transform.GetChild(i);
            return null;
        }

        private void ClearDecoration()
        {
            var decoration = FindChild("PogoCosmeticDecoration");
            if (decoration != null) Destroy(decoration.gameObject);
        }
    }

    internal static class PogoPrimitiveCompatibility
    {
        // Unity has no torus PrimitiveType. A thin cylinder gives a readable ring
        // proxy in the procedural prototype while authored art will use real coils.
        public static PrimitiveType TorusFallback(this PrimitiveType ignored)
        {
            return PrimitiveType.Cylinder;
        }
    }
}
