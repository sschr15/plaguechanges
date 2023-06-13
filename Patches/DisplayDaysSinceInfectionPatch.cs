using System;
using HarmonyLib;

namespace PlagueChanges.Patches
{
    [HarmonyPatch(typeof(CHUDScreen))]
    public class DisplayDaysSinceInfectionPatch
    {
        [HarmonyPatch(nameof(CHUDScreen.SetStartDate))]
        [HarmonyPostfix]
        public static void SetStartDatePostfix(ref UILabel ___mpClockText)
        {
            if (!Main.ACTIVE || !Main.Settings.ShowDaysSinceInfection) return;

            ___mpClockText.text = "T+0d";
        }

        [HarmonyPatch(nameof(CHUDScreen.SetDay))]
        [HarmonyPostfix]
        public static void SetDayPostfix(ref UILabel ___mpClockText, ref DateTime ___startDate)
        {
            if (!Main.ACTIVE || !Main.Settings.ShowDaysSinceInfection) return;

            var daysSinceInfection = (CGameManager.currentGameDate - ___startDate).Days;
            if (daysSinceInfection >= 365)
            {
                var years = daysSinceInfection / 365;
                var days = daysSinceInfection % 365;
                ___mpClockText.text = $"T+{years}y {days}d";
            }
            else
            {
                ___mpClockText.text = $"T+{daysSinceInfection}d";
            }
        }
    }
}
