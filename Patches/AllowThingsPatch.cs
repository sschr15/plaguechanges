using HarmonyLib;

namespace PlagueChanges.Patches
{
    [HarmonyPatch(typeof(IGame), nameof(IGame.CreateGame))]
    public class AllowThingsPatch
    {
        public static void Postfix(ref IGame __instance)
        {
            if (!Main.ACTIVE) return;

            var param = __instance.SetupParameters;

            if (Main.ALWAYS_ALLOW_CHEATS)
            {
                param.lockAllCheats = false;
            }
            
            if (Main.ALWAYS_ALLOW_GENES)
            {
                param.lockAllGenes = false;
            }
            
            if (Main.ANY_STARTING_COUNTRY)
            {
                param.fixedStartCountry = false;
                param.startCountryID = string.Empty;
            }
            
            if (Main.ALWAYS_ALLOW_ANY_DIFFICULTY)
            {
                param.lockDifficulty = false;
                param.difficulty = string.Empty;
            }
        }
    }
}