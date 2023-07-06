using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace PlagueChanges.Patches
{
    [HarmonyPatch]
    public class AnyFpsLimitPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(COptionsManager), nameof(COptionsManager.SavePrefs));
            yield return AccessTools.Method(typeof(COptionsManager), nameof(COptionsManager.LoadPrefs));
            yield return AccessTools.Method(typeof(CMainOptionsSubScreen), "DoApplyVideo");
            yield return AccessTools.Method(typeof(CMainOptionsSubScreen), "DoCancelApply");
        }
    }
}
