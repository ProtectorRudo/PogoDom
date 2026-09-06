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

            var requiredSockets = AvatarPrefabContract.RequiredSockets(capabilities);
            var missing = AvatarPrefabContract.MissingSockets(names, capabilities);
            for (var i = 0; i < missing.Count; i++)
                errors.Add("Missing required socket: " + missing[i]);
            for (var i = 0; i < requiredSockets.Count; i++)
            {
                var count = CountName(names, requiredSockets[i]);
                if (count > 1) errors.Add("Required socket appears more than once: " + requiredSockets[i] + " (" + count + ")");
            }

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
            var cloth = root.GetComponentsInChildren<Cloth>(true);
            var audioSources = root.GetComponentsInChildren<AudioSource>(true);
            var particles = root.GetComponentsInChildren<ParticleSystem>(true);
            var animators = root.GetComponentsInChildren<Animator>(true);
            var skinned = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            var budget = AvatarAssetBudgetPolicy.MobileLaunch;
            var triangles = CountTriangles(root);
            var materialSlots = CountMaterialSlots(renderers);
            var bones = CountUniqueBones(skinned);

            if (triangles > budget.MaxTriangles)
                errors.Add("Avatar exceeds triangle budget: " + triangles + " / " + budget.MaxTriangles + ".");
            if (skinned.Length > budget.MaxSkinnedMeshRenderers)
                errors.Add("Avatar exceeds skinned renderer budget: " + skinned.Length + " / " + budget.MaxSkinnedMeshRenderers + ".");
            if (materialSlots > budget.MaxMaterialSlots)
                errors.Add("Avatar exceeds material-slot budget: " + materialSlots + " / " + budget.MaxMaterialSlots + ".");
            if (bones > budget.MaxBones)
                errors.Add("Avatar exceeds unique bone budget: " + bones + " / " + budget.MaxBones + ".");
            if (particles.Length > budget.MaxEmbeddedParticleSystems)
                errors.Add("Avatar embeds too many ParticleSystems: " + particles.Length + " / " + budget.MaxEmbeddedParticleSystems + ". Put skill/VFX in sockets instead.");
            if (animators.Length > budget.MaxAnimators)
                errors.Add("Avatar must have at most one Animator: " + animators.Length + " found.");
            if (lights.Length > budget.MaxLights)
                errors.Add("Avatar prefab contains Light components. Use emissive materials/VFX instead.");
            if (cloth.Length > budget.MaxClothComponents)
                errors.Add("Avatar prefab contains Cloth. Launch avatars must use authored/bone motion instead of runtime Cloth.");
            if (audioSources.Length > budget.MaxAudioSources)
                errors.Add("Avatar prefab contains AudioSource. Audio belongs to the event/audio presentation layer, not the cosmetic prefab.");

            if ((capabilities & AvatarRigCapability.HumanoidRetargeting) != 0 ||
                (capabilities & AvatarRigCapability.Hands) != 0)
            {
                if (animators.Length == 0)
                {
                    errors.Add("Humanoid-capable avatar requires an Animator.");
                }
                else
                {
                    var animator = animators[0];
                    if (!animator.isHuman)
                        errors.Add("Animator is not configured as Humanoid but this profile declares humanoid retargeting/hands.");
                    if (animator.applyRootMotion)
                        errors.Add("Animator.applyRootMotion must be OFF. Gameplay movement owns the root transform.");

                    if (animator.isHuman && (capabilities & AvatarRigCapability.Hands) != 0)
                    {
                        if (animator.GetBoneTransform(HumanBodyBones.LeftHand) == null)
                            errors.Add("Humanoid avatar is missing LeftHand bone mapping.");
                        if (animator.GetBoneTransform(HumanBodyBones.RightHand) == null)
                            errors.Add("Humanoid avatar is missing RightHand bone mapping.");
                    }
                }
            }
            else if (animators.Length > 0 && animators[0].applyRootMotion)
            {
                errors.Add("Animator.applyRootMotion must be OFF for every PogoDom avatar profile.");
            }

            if (root.transform.localPosition.sqrMagnitude > 0.000001f)
                warnings.Add("Avatar prefab root localPosition is not zero. Prefer neutral authored roots and use wrapper positioning.");
            if (Quaternion.Angle(root.transform.localRotation, Quaternion.identity) > 0.05f)
                warnings.Add("Avatar prefab root localRotation is not identity. Prefer neutral authored roots and use wrapper orientation.");
            if (Mathf.Abs(root.transform.localScale.x - root.transform.localScale.y) > 0.001f ||
                Mathf.Abs(root.transform.localScale.x - root.transform.localScale.z) > 0.001f)
                warnings.Add("Avatar root has non-uniform scale. Keep gameplay/cosmetic scale separation predictable.");

            var report = "Triangles=" + triangles +
                         " | Skinned=" + skinned.Length +
                         " | Materials=" + materialSlots +
                         " | Bones=" + bones +
                         " | Particles=" + particles.Length;

            if (errors.Count == 0)
            {
                Debug.Log("PogoDom avatar validation PASSED — " + profileName + " — " + root.name +
                          "\n" + report +
                          (warnings.Count == 0 ? "" : "\nWarnings:\n- " + string.Join("\n- ", warnings)));
                return;
            }

            Debug.LogError("PogoDom avatar validation FAILED — " + profileName + " — " + root.name +
                           "\n" + report +
                           "\nErrors:\n- " + string.Join("\n- ", errors) +
                           (warnings.Count == 0 ? "" : "\nWarnings:\n- " + string.Join("\n- ", warnings)));
        }

        private static int CountTriangles(GameObject root)
        {
            var total = 0;
            var skinned = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (var i = 0; i < skinned.Length; i++)
            {
                var mesh = skinned[i].sharedMesh;
                if (mesh != null) total += TriangleCount(mesh);
            }

            var filters = root.GetComponentsInChildren<MeshFilter>(true);
            for (var i = 0; i < filters.Length; i++)
            {
                var mesh = filters[i].sharedMesh;
                if (mesh != null) total += TriangleCount(mesh);
            }
            return total;
        }

        private static int TriangleCount(Mesh mesh)
        {
            var total = 0;
            for (var sub = 0; sub < mesh.subMeshCount; sub++)
                total += (int)(mesh.GetIndexCount(sub) / 3u);
            return total;
        }

        private static int CountMaterialSlots(Renderer[] renderers)
        {
            var total = 0;
            for (var i = 0; i < renderers.Length; i++)
                total += renderers[i].sharedMaterials == null ? 0 : renderers[i].sharedMaterials.Length;
            return total;
        }

        private static int CountUniqueBones(SkinnedMeshRenderer[] renderers)
        {
            var bones = new HashSet<Transform>();
            for (var i = 0; i < renderers.Length; i++)
            {
                var list = renderers[i].bones;
                if (list == null) continue;
                for (var b = 0; b < list.Length; b++)
                    if (list[b] != null) bones.Add(list[b]);
            }
            return bones.Count;
        }

        private static int CountName(List<string> names, string target)
        {
            var count = 0;
            for (var i = 0; i < names.Count; i++)
                if (names[i] == target) count++;
            return count;
        }

        private static void CollectNames(Transform root, List<string> names)
        {
            names.Add(root.name);
            for (var i = 0; i < root.childCount; i++)
                CollectNames(root.GetChild(i), names);
        }
    }
}
