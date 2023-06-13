using HarmonyLib;

namespace PlagueChanges.Patches
{
    [HarmonyPatch(typeof (ExportData), nameof(ExportData.DeserialiseScenario))]
    public class ExportData_DeserializeScenario
    {
        [HarmonyPostfix]
        public static void Postfix(ref ExportData __result)
        {
            __result.visibility = VisibilityStatus.Public;
        }
    }
}
