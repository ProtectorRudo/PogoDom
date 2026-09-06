using System;

namespace PogoDom.Cosmetics
{
    public readonly struct AvatarAssetBudget
    {
        public int MaxTriangles { get; }
        public int MaxSkinnedMeshRenderers { get; }
        public int MaxMaterialSlots { get; }
        public int MaxBones { get; }
        public int MaxEmbeddedParticleSystems { get; }
        public int MaxAnimators { get; }
        public int MaxLights { get; }
        public int MaxClothComponents { get; }
        public int MaxAudioSources { get; }

        public AvatarAssetBudget(
            int maxTriangles,
            int maxSkinnedMeshRenderers,
            int maxMaterialSlots,
            int maxBones,
            int maxEmbeddedParticleSystems,
            int maxAnimators,
            int maxLights,
            int maxClothComponents,
            int maxAudioSources)
        {
            if (maxTriangles < 1) throw new ArgumentOutOfRangeException(nameof(maxTriangles));
            if (maxSkinnedMeshRenderers < 1) throw new ArgumentOutOfRangeException(nameof(maxSkinnedMeshRenderers));
            if (maxMaterialSlots < 1) throw new ArgumentOutOfRangeException(nameof(maxMaterialSlots));
            if (maxBones < 1) throw new ArgumentOutOfRangeException(nameof(maxBones));
            if (maxEmbeddedParticleSystems < 0) throw new ArgumentOutOfRangeException(nameof(maxEmbeddedParticleSystems));
            if (maxAnimators < 1) throw new ArgumentOutOfRangeException(nameof(maxAnimators));
            if (maxLights < 0) throw new ArgumentOutOfRangeException(nameof(maxLights));
            if (maxClothComponents < 0) throw new ArgumentOutOfRangeException(nameof(maxClothComponents));
            if (maxAudioSources < 0) throw new ArgumentOutOfRangeException(nameof(maxAudioSources));

            MaxTriangles = maxTriangles;
            MaxSkinnedMeshRenderers = maxSkinnedMeshRenderers;
            MaxMaterialSlots = maxMaterialSlots;
            MaxBones = maxBones;
            MaxEmbeddedParticleSystems = maxEmbeddedParticleSystems;
            MaxAnimators = maxAnimators;
            MaxLights = maxLights;
            MaxClothComponents = maxClothComponents;
            MaxAudioSources = maxAudioSources;
        }
    }

    /// <summary>
    /// Launch-avatar import budget. Cosmetics and skill VFX are separate surfaces;
    /// the base character prefab must stay cheap enough for four simultaneous
    /// competitors plus screen recording on mid-range phones.
    /// </summary>
    public static class AvatarAssetBudgetPolicy
    {
        public static AvatarAssetBudget MobileLaunch => new AvatarAssetBudget(
            maxTriangles: 40000,
            maxSkinnedMeshRenderers: 3,
            maxMaterialSlots: 10,
            maxBones: 90,
            maxEmbeddedParticleSystems: 1,
            maxAnimators: 1,
            maxLights: 0,
            maxClothComponents: 0,
            maxAudioSources: 0);

        public static bool IsWithinBudget(
            AvatarAssetBudget budget,
            int triangles,
            int skinnedMeshRenderers,
            int materialSlots,
            int bones,
            int embeddedParticleSystems,
            int animators,
            int lights,
            int clothComponents,
            int audioSources)
        {
            if (triangles < 0 || skinnedMeshRenderers < 0 || materialSlots < 0 || bones < 0 ||
                embeddedParticleSystems < 0 || animators < 0 || lights < 0 || clothComponents < 0 || audioSources < 0)
                return false;

            return triangles <= budget.MaxTriangles &&
                   skinnedMeshRenderers <= budget.MaxSkinnedMeshRenderers &&
                   materialSlots <= budget.MaxMaterialSlots &&
                   bones <= budget.MaxBones &&
                   embeddedParticleSystems <= budget.MaxEmbeddedParticleSystems &&
                   animators <= budget.MaxAnimators &&
                   lights <= budget.MaxLights &&
                   clothComponents <= budget.MaxClothComponents &&
                   audioSources <= budget.MaxAudioSources;
        }
    }
}
