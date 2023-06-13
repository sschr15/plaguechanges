using HarmonyLib;
using UnityEngine;

namespace PlagueChanges.Patches
{
    [HarmonyPatch(typeof(TechHex), "RefreshForCoreDisease")]
    public class InvertntPatch
    {
        private static readonly int MaskInvert = Shader.PropertyToID("_MaskInvert");

        public static void Postfix(ref TechHex __instance)
        {
            if (!Main.ACTIVE || !Main.DONT_INVERT_ICONS || __instance.customIcon == null) return;

            __instance.customIcon.material.SetFloat(MaskInvert, 0f);
        }
    }
}