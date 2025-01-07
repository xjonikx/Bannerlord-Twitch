using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BLTAdoptAHero.Util
{
    public static class CompanionPatch
    {
        [HarmonyPatch(typeof(CompanionsCampaignBehavior))]
        public class CompanionsCampaignBehaviorPatch
        {
            [HarmonyPatch("TryKillCompanion")]
            [HarmonyPrefix]
            public static bool SkipTryKillCompanion()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(DefaultClanTierModel), "GetCompanionLimit")]
        internal class CompanionLimitPatch
        {
            public static void Postfix(Clan clan, ref int __result)
            {
                bool flag = clan != null && clan == Clan.PlayerClan;
                if (flag)
                {
                    __result += BLTAdoptAHeroModule.CommonConfig.CustomCompanionLimit;
                }
            }
        }
    }
}
