using System.Collections;
using PogoDom.Cosmetics;
using UnityEngine;

namespace PogoDom.Runtime
{
    /// <summary>
    /// Runtime safety net for the procedural M0 avatars. Final authored FBX
    /// prefabs must already satisfy AvatarPrefabContract and are validated in
    /// the Editor; this component only keeps the current generated fallback
    /// aligned with the same socket names.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PogoAvatarSocketRegistry : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var prototypes = Object.FindObjectsOfType<PogoDomPrototypeBootstrap>();
            for (var i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].GetComponent<PogoAvatarSocketRegistry>() == null)
                    prototypes[i].gameObject.AddComponent<PogoAvatarSocketRegistry>();
            }
        }

        private IEnumerator Start()
        {
            // M0.31 builds procedural avatar children one frame after the core
            // greybox. Waiting two frames avoids coupling to execution order.
            yield return null;
            yield return null;
            EnsureCurrentAvatarSockets();
        }

        public void EnsureCurrentAvatarSockets()
        {
            var rigs = Object.FindObjectsOfType<PogoAvatarVisualRig>();
            for (var i = 0; i < rigs.Length; i++)
                EnsureFallbackRig(rigs[i].transform);
        }

        public static Transform FindSocket(Transform root, string socketName)
        {
            if (root == null || string.IsNullOrEmpty(socketName)) return null;
            if (root.name == socketName) return root;
            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindSocket(root.GetChild(i), socketName);
                if (found != null) return found;
            }
            return null;
        }

        private static void EnsureFallbackRig(Transform root)
        {
            if (root == null) return;
            var visual = FindSocket(root, "AvatarVisual") ?? root;

            EnsureAnchor(visual, AvatarPrefabContract.AuraSocket, new Vector3(0f, 0.38f, 0f));

            var pogo = FindSocket(root, AvatarPrefabContract.PogoSocket);
            if (pogo != null)
            {
                EnsureAnchor(pogo, AvatarPrefabContract.LeftGripTarget, new Vector3(-0.28f, 0.40f, 0f));
                EnsureAnchor(pogo, AvatarPrefabContract.RightGripTarget, new Vector3(0.28f, 0.40f, 0f));
            }
        }

        private static Transform EnsureAnchor(Transform parent, string objectName, Vector3 localPosition)
        {
            var existing = FindSocket(parent, objectName);
            if (existing != null) return existing;
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.transform;
        }
    }
}
