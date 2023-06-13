using System;
using HarmonyLib;

namespace PlagueChanges.Patches
{
    [HarmonyPatch(typeof(SPDisease))]
    public class SPDiseasePatches
    {
        [HarmonyPatch(nameof(SPDisease.GetEvolveCost))]
        [HarmonyPostfix]
        public static void EvolveCost(ref int __result)
        {
            if (!Main.ACTIVE) return;

            __result = Util.AwayFromZero(__result * Main.EVOLVE_COST_MULTIPLIER);
        }

        [HarmonyPatch(nameof(SPDisease.GetDeEvolveCost))]
        [HarmonyPostfix]
        public static void DeEvolveCost(ref int __result)
        {
            if (!Main.ACTIVE) return;

            __result = Util.AwayFromZero(__result * Main.EVOLVE_COST_MULTIPLIER);
        }
    }

    [HarmonyPatch(typeof(Disease))]
    public class EvolutionPatches
    {
        [HarmonyPatch(nameof(Disease.CanDeEvolve))]
        [HarmonyPostfix]
        public static void DevolveOverride(ref bool __result, ref Disease __instance)
        {
            if (!Main.ACTIVE || Main.UPGRADE_STATE == 2) return;

            foreach (var technology in __instance.technologies)
            {
                switch (Main.UPGRADE_STATE)
                {
                    case 0:
                        technology.cantDevolve = false;
                        break;
                    case 1:
                        technology.cantDevolve = true;
                        break;
                }
            }

            switch (Main.UPGRADE_STATE)
            {
                case 0:
                    __result = true;
                    break;
                case 1:
                    __result = false;
                    break;
            }
        }
        
        [HarmonyPatch(nameof(Disease.CanEvolve))]
        [HarmonyPostfix]
        public static void EvolveOverride(Technology tech, ref bool __result)
        {
            if (!Main.ACTIVE) return;

            if (tech.isPreEvolved || Main.ALL_UPGRADES_AVAILABLE)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Disease), nameof(Disease.EvolveRandomTech), typeof(string[]))]
    public class EvolveOtherStuffPatch
    {
        public static void Prefix(ref Disease __instance)
        {
            if (!Main.ACTIVE) return;

            if (Main.CAN_EVOLVE_TRANSMISSIONS)
                __instance.transmissionRandomMutations = true;

            if (Main.CAN_EVOLVE_ABILITIES)
                __instance.abilityRandomMutations = true;
        }
    }
}
