using HarmonyLib;

namespace PlagueChanges.Patches
{
    [HarmonyPatch(typeof(CGSDifficultySubScreen), nameof(CGSDifficultySubScreen.SetActive))]
    public class ForceAllowCheats
    {
        public static void Postfix(ref CGSDifficultySubScreen __instance, bool isActive)
        {
            if (!Main.ACTIVE || !Main.ALWAYS_ALLOW_CHEATS || !isActive)
                return;
            
            __instance.cheatButton.isEnabled = true;
        }
    }

    [HarmonyPatch(typeof(CGSGeneSubScreen), nameof(CGSGeneSubScreen.CheckScreenLocked))]
    public class ForceAllowGenes
    {
        public static void Prefix()
        {
            CGameManager.game.SetupParameters.lockAllGenes &= !Main.ACTIVE || !Main.ALWAYS_ALLOW_GENES;
        }
    }
    
    [HarmonyPatch(typeof(CGSDifficultySubScreen), nameof(CGSDifficultySubScreen.CheckScreenLocked))]
    public class ForceAllowDifficulties
    {
        public static void Postfix(ref CGSDifficultySubScreen __instance, ref bool __result)
        {
            if (!Main.ACTIVE)
                return;
            
            if (!Main.Settings.AlwaysAllowCheats && !Main.Settings.AlwaysAllowAnyDifficulty)
                return;

            if (Main.Settings.AlwaysAllowAnyDifficulty)
            {
                foreach (var button in __instance.buttons)
                {
                    button.SetEnabled(true);
                }
            }
            else if (!CGameManager.game.SetupParameters.difficulty.Contains(","))
            {
                // some might call this awkward... (that we need to show the screen because cheats are forced on)
                var difficulty = CGameManager.game.SetupParameters.difficulty.Replace(" ", "");
                for (var i = 0; i < __instance.buttons.Length; i++)
                {
                    var button = __instance.buttons[i];
                    button.SetEnabled(CGameManager.Difficulties[i] == difficulty);
                }
            }

            __result = false;
        }
    }
}