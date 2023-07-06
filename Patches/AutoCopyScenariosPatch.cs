using System.IO;
using System.Linq;
using HarmonyLib;

namespace PlagueChanges.Patches
{
    [HarmonyPatch(typeof(ExportData), nameof(ExportData.DeserialiseScenario))]
    public class AutoCopyScenariosPatch
    {
        const string DISALLOWED_CHARS = "\\/:*?\"<>|";

        public static void Postfix(ref ExportData __result)
        {
            if (!Main.ACTIVE) return;

            var name = __result.GetScenarioInformation().scenName;
            name = DISALLOWED_CHARS.Aggregate(name, (current, c) => current.Replace(c.ToString(), ""));

            if (Directory.Exists(name))
            {
                var diffNum = 1;
                while (Directory.Exists(name + $" ({diffNum})"))
                {
                    diffNum++;
                }
                name += $" ({diffNum})";
            }

            CSLocalUGCHandler.LocalSaveExport(name, __result);
        }
    }
}
