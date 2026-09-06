using System.Collections.Generic;
using PogoDom.Cosmetics;
using UnityEditor;
using UnityEngine;

namespace PogoDom.Editor
{
    public static class PogoAvatarPrefabValidator
    {
        [MenuItem("PogoDom/Art/Validate Selected Avatar/Standard Humanoid")]
        public static void ValidateSelectedHumanoid()
        {
            ValidateSelection(AvatarRigCapabilities.StandardHumanoid, "Standard Humanoid");
        }

        [MenuItem("PogoDom/Art/Validate Selected Avatar/Mascot")]
        public static void ValidateSelectedMascot()
        {
            ValidateSelection(AvatarRigCapabilities.Mascot, "Mascot");
        }

        private static void ValidateSelection(AvatarRigCapability capabilities, string profileName)
        {
            var root = Selection.activeGameObject;
            if (root == null)
            {
                Debug.LogError("PogoDom avatar validation: select an avatar prefab root or scene instance first.");
                return;
            }

            var errors = new List<string>();
            var warnings = new List<string>();
            var names = new List<string>();
            CollectNames(root.transform, names);

            var missing = AvatarPrefabContract.MissingSockets(names, capabilities);
            for (var i = 0; i < missing.Count; i++)
                errors.Add("Missing required socket: " + missing[i]);

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                errors.Add("Avatar has no Renderer/SkinnedMeshRenderer; it cannot be visible.");

            var colliders = root.GetComponentsInChildren<Collider>(true);
            if (colliders.Length > 0)
                errors.Add("Visual avatar contains Collider components. Gameplay collision must stay outside cosmetic prefabs.");

            var rigidbodies = root.GetComponentsInChildren<Rigidbody>(true);
            if (rigidbodies.Length > 0)
                errors.Add("Visual avatar contains Rigidbody components. Cosmetics must not create gameplay physics.");

            var cameras = root.GetComponentsInChildren<Camera>(true);
            if (cameras.Length > 0)
                errors.Add("Avatar prefab contains a Camera.");

            var lights = root.GetComponentsInChildren<Light>(true);
            if (lights.Length > 0)
                warnings.Add("Avatar prefab contains Light components; prefer emissive materials/VFX for mobile cost and deterministic presentation.");

            if ((capabilities & AvatarRigCapability.HumanoidRetargeting) != 0 ||
                (capabilities & AvatarRigCapability.Hands) != 0)
            {
                var animator = root.GetComponentInChildren<Animator>(true);
                if (animator == null)
                {
                    errors.Add("Humanoid-capable avatar requires an Animator.");
                }
                else if (!animator.isHuman)
                {
                    errors.Add("Animator is not configured as Humanoid but this profile declares humanoid retargeting/hands.");
                }
                else if ((capabilities & AvatarRigCapability.Hands) != 0)
                {
                    if (animator.GetBoneTransform(HumanBodyBones.LeftHand) == null)
                        errors.Add("Humanoid avatar is missing LeftHand bone mapping.");
                    if (animator.GetBoneTransform(HumanBodyBones.RightHand) == null)
                        errors.Add("Humanoid avatar is missing RightHand bone mapping.");
                }
            }

            if (Mathf.Abs(root.transform.localScale.x - root.transform.localScale.y) > 0.001f ||
                Mathf.Abs(root.transform.localScale.x - root.transform.localScale.z) > 0.001f)
                warnings.Add("Avatar root has non-uniform scale. Keep gameplay/cosmetic scale separation predictable.");

            if (errors.Count == 0)
            {
                Debug.Log("PogoDom avatar validation PASSED — " + profileName + " — " + root.name +
                          (warnings.Count == 0 ? "" : "\nWarnings:\n- " + string.Join("\n- ", warnings)));
                return;
            }

            Debug.LogError("PogoDom avatar validation FAILED — " + profileName + " — " + root.name +
                           "\nErrors:\n- " + string.Join("\n- ", errors) +
                           (warnings.Count == 0 ? "" : "\nWarnings:\n- " + string.Join("\n- ", warnings)));
        }

        private static void CollectNames(Transform root, List<string> names)
        {
            names.Add(root.name);
            for (var i = 0; i < root.childCount; i++)
                CollectNames(root.GetChild(i), names);
        }
    }
}
