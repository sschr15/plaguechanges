using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace PlagueChanges.Patches
{
    [HarmonyPatch(typeof(CMainCustomSubScreen), nameof(CMainCustomSubScreen.SetActive))]
    public class CustomSubScreenDefaultAssemblyPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns)
        {
            var field = AccessTools.Field(typeof(CMainCustomSubScreen), "currentPage");
            var mtd = SymbolExtensions.GetMethodInfo<CMainCustomSubScreen>(screen => AssemblyCalledMethods.OnSetActive(screen));
            foreach (var insn in insns)
            {
                yield return insn;
                if (insn.opcode != OpCodes.Stfld || !insn.StoresField(field)) continue;

                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, mtd);
            }
        }
    }
}
