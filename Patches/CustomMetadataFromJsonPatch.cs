using HarmonyLib;

namespace PlagueChanges.Patches
{
    [HarmonyPatch(typeof(CustomScenarioMetadata), nameof(CustomScenarioMetadata.FromJson))]
    public class CustomMetadata_FromJson
    {
        [HarmonyPostfix]
        public static void Postfix(ref CustomScenarioMetadata __result)
        {
            __result.Visibility = CustomScenarioVisibility.Public;
        }
    }
}
