using PogoDom.Cosmetics;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Protects recording/play frame pacing in Auto mode. The governor only
    /// downgrades presentation during a live session; it never oscillates upward.
    /// </summary>
    [DefaultExecutionOrder(1510)]
    [DisallowMultipleComponent]
    public sealed class PogoDomAdaptiveVisualQualityGovernor : MonoBehaviour
    {
        private PogoDomVisualQualityScaler _quality;
        private float _smoothedFrameMs = 16.7f;
        private float _pressureSeconds;
        private float _warmupSeconds = 2.0f;
        private float _cooldownAfterDowngrade;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
                if (prototypes[i].GetComponent<PogoDomAdaptiveVisualQualityGovernor>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomAdaptiveVisualQualityGovernor>();
        }

        private void Awake()
        {
            _quality = GetComponent<PogoDomVisualQualityScaler>();
        }

        private void Update()
        {
            if (_quality == null || !_quality.IsAutoMode) return;

            var dt = Mathf.Clamp(Time.unscaledDeltaTime, 0f, 0.25f);
            if (dt <= 0f) return;

            if (_warmupSeconds > 0f)
            {
                _warmupSeconds -= dt;
                return;
            }

            if (_cooldownAfterDowngrade > 0f)
            {
                _cooldownAfterDowngrade -= dt;
                return;
            }

            var frameMs = dt * 1000f;
            // Roughly a 1.5s moving horizon at 60fps, resilient to one-frame VFX spikes.
            var alpha = 1f - Mathf.Exp(-dt / 1.5f);
            _smoothedFrameMs = Mathf.Lerp(_smoothedFrameMs, frameMs, alpha);
            _pressureSeconds = AdaptiveVisualQualityPolicy.UpdatePressureSeconds(_pressureSeconds, _smoothedFrameMs, dt);

            var sample = new FramePressureSample(_smoothedFrameMs, _pressureSeconds);
            if (!AdaptiveVisualQualityPolicy.ShouldDowngrade(sample)) return;

            if (_quality.TryAdaptiveDowngrade())
            {
                Debug.Log("[PogoDom Visual Quality] Adaptive downgrade to " + _quality.ActiveTier +
                          " after sustained frame pressure (" + _smoothedFrameMs.ToString("0.0") + " ms). Gameplay unchanged.");
                _pressureSeconds = 0f;
                _cooldownAfterDowngrade = 4f;
            }
        }
    }
}
