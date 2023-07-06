using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class ChangingMutationPatches
    {
        public static bool Prefix(ref Disease __instance, ref Technology __result)
        {
            if (!Main.ACTIVE) return true;

            if (Main.Settings.DisableAutomaticMutation)
            {
                __result = null;
                return false; // Skip the original method, thereby disabling automatic mutations
            }

            if (Main.CAN_EVOLVE_TRANSMISSIONS)
                __instance.transmissionRandomMutations = true;

            if (Main.CAN_EVOLVE_ABILITIES)
                __instance.abilityRandomMutations = true;

            return true;
        }
    }

    [HarmonyPatch(typeof(SPDisease))]
    public class CustomInfSevLeth
    {
        [HarmonyPatch(nameof(Disease.globalInfectiousnessMax), MethodType.Getter)]
        [HarmonyPostfix]
        public static void InfectiousnessMax(ref float __result)
        {
            if (!Main.ACTIVE || !Main.Settings.UseCustomInfectivity) return;

            __result = Main.Settings.CustomInfectivity;
        }

        [HarmonyPatch(nameof(Disease.globalSeverityMax), MethodType.Getter)]
        [HarmonyPostfix]
        public static void SeverityMax(ref float __result)
        {
            if (!Main.ACTIVE || !Main.Settings.UseCustomSeverity) return;

            __result = Main.Settings.CustomSeverity;
        }

        [HarmonyPatch(nameof(Disease.globalLethalityMax), MethodType.Getter)]
        [HarmonyPostfix]
        public static void LethalityMax(ref float __result)
        {
            if (!Main.ACTIVE || !Main.Settings.UseCustomLethality) return;

            __result = Main.Settings.CustomLethality;
        }
    }

    [HarmonyPatch(typeof(SPDisease))]
    public class CustomTransmissionValueMassPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return Data.InternalCustomTransmissionNames.Select(name => AccessTools.PropertyGetter(typeof(SPDisease), name));
        }

        public static void Postfix(ref float __result, MethodBase __originalMethod)
        {
            var name = __originalMethod.Name.Replace("get_", "");
            var index = Array.IndexOf(Data.InternalCustomTransmissionNames, name);
            if (index == -1 || !Main.ACTIVE || !Main.Settings.OtherCustomTransmissions[index]) return;

            __result = Main.Settings.OtherCustomTransmissionValues[index];
        }
    }
}
