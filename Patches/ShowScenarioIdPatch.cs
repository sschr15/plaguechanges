using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PlagueChanges.Patches
{
    [HarmonyPatch(typeof(UIScenarioDetails), nameof(UIScenarioDetails.SetScenarioDetails))]
    public class ShowScenarioIdPatch
    {
        [HarmonyPatch(new[]{ typeof(string), typeof(string), typeof(string), typeof(string), typeof(Texture), typeof(bool), typeof(bool), typeof(IList<string>) })]
        [HarmonyPostfix]
        public static void ManyArgs(ref UIScenarioDetails __instance)
        {
            RunAction(__instance);
        }

        [HarmonyPatch(new[]{ typeof(CustomScenarioMetadata), typeof(Texture) })]
        [HarmonyPostfix]
        public static void FewArgs(ref UIScenarioDetails __instance)
        {
            RunAction(__instance);
        }

        public static void RunAction(UIScenarioDetails inst)
        {
            if (!Main.ACTIVE) return;

            var title = Traverse.Create(inst).Field<string>("scenarioTitle");
            var id = inst.publishID;
            var toAdd = $" ({id})";
            
            if (!title.Value.EndsWith(toAdd))
            {
                title.Value += toAdd;
            }

            inst.scenarioNameTitleLabel.text = title.Value;
        }
    }

    [HarmonyPatch(typeof(UICustomScenarioTableElement), nameof(UICustomScenarioTableElement.PopulateWithData))]
    public class ShowIdInListPatch
    {
        [HarmonyPatch(new[]{ typeof(CustomScenarioMetadata) })]
        [HarmonyPostfix]
        public static void ForMetadata(CustomScenarioMetadata metadata, ref UICustomScenarioTableElement __instance)
        {
            if (!Main.ACTIVE) return;

            var id = metadata.Id;
            var toAdd = $" ({id})";

            __instance.scenarioNameLabel.text += toAdd;
        }
        
        [HarmonyPatch(new[]
        {
            typeof(ulong), typeof(string), typeof(int),
            typeof(bool), typeof(bool), typeof(bool),
            typeof(string), typeof(ScenarioInformation)
        })]
        [HarmonyPostfix]
        public static void ForScenario(ulong publishedFileID, ref UICustomScenarioTableElement __instance)
        {
            if (!Main.ACTIVE) return;

            var toAdd = $" ({publishedFileID})";

            __instance.scenarioNameLabel.text += toAdd;
        }
    }
}