using HarmonyLib;

namespace PlagueChanges.Patches
{
    // The name is misleading, it doesn't pop "choose starting country" bubbles
    [HarmonyPatch(typeof(BonusObject), nameof(BonusObject.Initialise))]
    public class AutoPopAllPatch
    {
        public static void Postfix(ref BonusObject __instance)
        {
            if (!Main.ACTIVE || !Main.AUTO_POP_ALL) return;

            var autoPop = Traverse.Create(__instance).Field<bool>("autoPop");
            if (__instance.type.ToString().StartsWith("COUNTRY_SELECT")) return;

            autoPop.Value = true;
        }
    }
}