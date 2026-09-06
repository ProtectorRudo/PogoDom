using System;
using System.Collections.Generic;

namespace PogoDom.Cosmetics
{
    public static class AvatarPrefabContract
    {
        public const string PogoSocket = "PogoSocket";
        public const string SkillSocket = "SkillSocket";
        public const string TrailSocket = "TrailSocket";
        public const string LandingFxSocket = "LandingFxSocket";
        public const string EmoteRoot = "EmoteRoot";
        public const string HeadwearSocket = "HeadwearSocket";
        public const string BackAccessorySocket = "BackAccessorySocket";
        public const string AuraSocket = "AuraSocket";
        public const string LeftGripTarget = "PogoGripTargetL";
        public const string RightGripTarget = "PogoGripTargetR";

        public static IReadOnlyList<string> RequiredSockets(AvatarRigCapability capabilities)
        {
            var result = new List<string>
            {
                PogoSocket,
                SkillSocket,
                TrailSocket,
                LandingFxSocket,
                EmoteRoot
            };

            if ((capabilities & AvatarRigCapability.HeadwearSocket) != 0)
                result.Add(HeadwearSocket);
            if ((capabilities & AvatarRigCapability.BackAccessorySocket) != 0)
                result.Add(BackAccessorySocket);
            if ((capabilities & AvatarRigCapability.AuraSocket) != 0)
                result.Add(AuraSocket);
            if ((capabilities & AvatarRigCapability.PogoHandleIk) != 0)
            {
                result.Add(LeftGripTarget);
                result.Add(RightGripTarget);
            }

            return result;
        }

        public static IReadOnlyList<string> MissingSockets(IEnumerable<string> existingNames, AvatarRigCapability capabilities)
        {
            if (existingNames == null) throw new ArgumentNullException(nameof(existingNames));
            var existing = new HashSet<string>(existingNames, StringComparer.Ordinal);
            var required = RequiredSockets(capabilities);
            var missing = new List<string>();
            for (var i = 0; i < required.Count; i++)
                if (!existing.Contains(required[i])) missing.Add(required[i]);
            return missing;
        }
    }
}
