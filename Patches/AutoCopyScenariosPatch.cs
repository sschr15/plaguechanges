using HarmonyLib;
using Random = System.Random;

namespace PlagueChanges.Patches
{
    [HarmonyPatch(typeof(ExportData), nameof(ExportData.DeserialiseScenario))]
    public class AutoCopyScenariosPatch
    {
        public static void Postfix(ref ExportData __result)
        {
            if (!Main.ACTIVE) return;

            CSLocalUGCHandler.LocalSaveExport(__result.publishedID ?? new Random().Next().ToString(), __result);
        }
    }
}