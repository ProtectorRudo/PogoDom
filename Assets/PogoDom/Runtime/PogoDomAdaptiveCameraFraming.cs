using System.Collections;
using PogoDom.Session;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Fits the entire readable arena before M0.31 installs its camera-juice
    /// component. Portrait devices therefore start from a valid 9:16-safe base
    /// pose instead of inheriting the old fixed landscape greybox camera.
    /// </summary>
    [DefaultExecutionOrder(850)]
    [DisallowMultipleComponent]
    public sealed class PogoDomAdaptiveCameraFraming : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].GetComponent<PogoDomAdaptiveCameraFraming>() == null)
                    prototypes[i].gameObject.AddComponent<PogoDomAdaptiveCameraFraming>();
            }
        }

        private IEnumerator Start()
        {
            // Bootstrap builds tiles and the camera in Awake. Fit immediately if
            // available; retry once next frame for unusual scene construction.
            if (!FitNow())
            {
                yield return null;
                FitNow();
            }
        }

        public bool FitNow()
        {
            var camera = Camera.main;
            if (camera == null) return false;

            Bounds bounds;
            if (!TryGetBoardBounds(out bounds)) return false;

            var aspect = Screen.height <= 0 ? camera.aspect : Screen.width / (float)Screen.height;
            if (aspect <= 0.01f) aspect = camera.aspect > 0.01f ? camera.aspect : 9f / 16f;

            // Include the tile top surface in width/depth; the policy adds extra
            // world padding for outlines, beacons and short trails.
            var frame = ViralCameraFramingPolicy.Fit(
                Mathf.Max(0.1f, bounds.size.x),
                Mathf.Max(0.1f, bounds.size.z),
                aspect,
                visualHeight: 2.35f,
                worldPadding: aspect < 1f ? 1.05f : 0.90f);

            var pitch = frame.PitchDegrees * Mathf.Deg2Rad;
            var target = new Vector3(bounds.center.x, 0.24f, bounds.center.z);
            var offset = new Vector3(
                0f,
                Mathf.Sin(pitch) * frame.Distance,
                -Mathf.Cos(pitch) * frame.Distance);

            camera.fieldOfView = frame.VerticalFovDegrees;
            camera.transform.position = target + offset;
            camera.transform.rotation = Quaternion.LookRotation(target - camera.transform.position, Vector3.up);
            camera.nearClipPlane = Mathf.Min(camera.nearClipPlane, 0.15f);
            camera.farClipPlane = Mathf.Max(camera.farClipPlane, 60f);
            return true;
        }

        private bool TryGetBoardBounds(out Bounds bounds)
        {
            var found = false;
            bounds = default;

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (!child.name.StartsWith("Tile_")) continue;

                var renderer = child.GetComponent<Renderer>();
                var pointBounds = renderer != null
                    ? renderer.bounds
                    : new Bounds(child.position, new Vector3(0.95f, 0.15f, 0.95f));

                if (!found)
                {
                    bounds = pointBounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(pointBounds);
                }
            }
            return found;
        }
    }
}
