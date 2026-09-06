using System.Collections;
using System.Collections.Generic;
using PogoDom.Cosmetics;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Development-time visual budget audit. It never disables an essential
    /// object and never changes gameplay; it only reports presentation bloat.
    /// </summary>
    [DefaultExecutionOrder(1500)]
    [DisallowMultipleComponent]
    public sealed class PogoDomVisualBudgetAuditor : MonoBehaviour
    {
        [SerializeField] private bool logHealthyAudit = false;
        private PogoDomVisualQualityScaler _quality;
        private PogoDomToonUpgrade _toon;
        private float _nextAudit;
        private bool _hasWarned;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
                if (prototypes[i].GetComponent<PogoDomVisualBudgetAuditor>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomVisualBudgetAuditor>();
        }

        private IEnumerator Start()
        {
            _quality = GetComponent<PogoDomVisualQualityScaler>();
            _toon = GetComponent<PogoDomToonUpgrade>();
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            AuditNow();
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextAudit) return;
            _nextAudit = Time.unscaledTime + 5f;
            AuditNow();
        }

        public void AuditNow()
        {
            var tier = _quality == null ? VisualQualityTier.Balanced : _quality.ActiveTier;
            var budget = VisualRuntimeBudgetPolicy.Get(tier);
            var renderers = GetComponentsInChildren<Renderer>(true);
            var materials = new HashSet<int>();
            var mesh = 0;
            var lines = 0;
            var trails = 0;
            var particles = 0;

            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null) continue;
                if (renderer is TrailRenderer) trails++;
                else if (renderer is LineRenderer) lines++;
                else if (renderer is ParticleSystemRenderer) particles++;
                else mesh++;

                var shared = renderer.sharedMaterials;
                for (var m = 0; m < shared.Length; m++)
                    if (shared[m] != null) materials.Add(shared[m].GetInstanceID());
            }

            var uniqueToon = _toon == null ? 0 : _toon.UniqueToonMaterialCount;
            var over = uniqueToon > budget.MaxUniqueToonMaterials ||
                       mesh > budget.MaxMeshRenderers ||
                       lines > budget.MaxLineRenderers ||
                       trails > budget.MaxTrailRenderers ||
                       particles > budget.MaxParticleSystems;

            if (over)
            {
                if (!_hasWarned)
                {
                    _hasWarned = true;
                    Debug.LogWarning(
                        "[PogoDom Visual Budget] " + tier +
                        " | toon " + uniqueToon + "/" + budget.MaxUniqueToonMaterials +
                        " | mesh " + mesh + "/" + budget.MaxMeshRenderers +
                        " | lines " + lines + "/" + budget.MaxLineRenderers +
                        " | trails " + trails + "/" + budget.MaxTrailRenderers +
                        " | particles " + particles + "/" + budget.MaxParticleSystems +
                        " | total shared materials " + materials.Count + ".");
                }
            }
            else
            {
                _hasWarned = false;
                if (logHealthyAudit)
                    Debug.Log("[PogoDom Visual Budget] healthy " + tier + " | toon=" + uniqueToon + " mesh=" + mesh + " materials=" + materials.Count);
            }
        }
    }
}
