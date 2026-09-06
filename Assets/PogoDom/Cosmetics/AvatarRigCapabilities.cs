using System;

namespace PogoDom.Cosmetics
{
    [Flags]
    public enum AvatarRigCapability
    {
        None = 0,
        HeadwearSocket = 1 << 0,
        BackAccessorySocket = 1 << 1,
        AuraSocket = 1 << 2,
        Hands = 1 << 3,
        PogoHandleIk = 1 << 4,
        FullBodyEmote = 1 << 5,
        FaceExpression = 1 << 6,
        HumanoidRetargeting = 1 << 7
    }

    public static class AvatarRigCapabilities
    {
        public const AvatarRigCapability StandardHumanoid =
            AvatarRigCapability.HeadwearSocket |
            AvatarRigCapability.BackAccessorySocket |
            AvatarRigCapability.AuraSocket |
            AvatarRigCapability.Hands |
            AvatarRigCapability.PogoHandleIk |
            AvatarRigCapability.FullBodyEmote |
            AvatarRigCapability.FaceExpression |
            AvatarRigCapability.HumanoidRetargeting;

        public const AvatarRigCapability Mascot =
            AvatarRigCapability.HeadwearSocket |
            AvatarRigCapability.BackAccessorySocket |
            AvatarRigCapability.AuraSocket |
            AvatarRigCapability.FullBodyEmote;

        public static bool Supports(AvatarRigCapability available, AvatarRigCapability required)
        {
            return (available & required) == required;
        }
    }
}
