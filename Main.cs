using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace PlagueChanges
{
    public class Data : UnityModManager.ModSettings
    {
        // Old settings, requiring getters in Main
        public int UpgradeState = 2;
        public bool AlwaysAllowGenes;
        public bool AlwaysAllowCheats;
        public bool AlwaysAllowAnyDifficulty;
        public bool AnyStartingCountry;
        public bool AllUpgradesAvailable;
        public bool AutoPopAll;
        public bool DontInvertIcons;
        public bool CanEvolveTransmissions;
        public bool CanEvolveAbilities;
        public float EvolveCostMultiplier = 1f;
        
        // New settings, with the express intent of calling directly
        public bool ShowDaysSinceInfection;
    }
    public class Main
    {
        public static bool ACTIVE = true;
        public static Data Settings;

        public static int UPGRADE_STATE => Settings.UpgradeState;
        public static bool ALWAYS_ALLOW_GENES => Settings.AlwaysAllowGenes;
        public static bool ALWAYS_ALLOW_CHEATS => Settings.AlwaysAllowCheats;
        public static bool ALWAYS_ALLOW_ANY_DIFFICULTY => Settings.AlwaysAllowAnyDifficulty;
        public static bool ANY_STARTING_COUNTRY => Settings.AnyStartingCountry;
        public static bool ALL_UPGRADES_AVAILABLE => Settings.AllUpgradesAvailable;
        public static bool AUTO_POP_ALL => Settings.AutoPopAll;
        public static bool DONT_INVERT_ICONS => Settings.DontInvertIcons;
        public static bool CAN_EVOLVE_TRANSMISSIONS => Settings.CanEvolveTransmissions;
        public static bool CAN_EVOLVE_ABILITIES => Settings.CanEvolveAbilities;
        public static float EVOLVE_COST_MULTIPLIER => Settings.EvolveCostMultiplier;

        public static bool Load(UnityModManager.ModEntry entry)
        {
            Settings = UnityModManager.ModSettings.Load<Data>(entry);
            entry.OnToggle = (self, b) =>
            {
                ACTIVE = b;
                if (b)
                {
                    // Reload settings (maybe they changed on disk)
                    Settings = UnityModManager.ModSettings.Load<Data>(self);
                }
                return true;
            };
            entry.OnGUI = OnGui;
            entry.OnSaveGUI = self => UnityModManager.ModSettings.Save(Settings, self);
            new HarmonyLib.Harmony(entry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        private static void OnGui(UnityModManager.ModEntry entry)
        {
            GUILayout.BeginVertical();

            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("All upgrades can...");
                var text = new[] { "be devolved", "not be devolved", "remain the same" };
                Settings.UpgradeState = GUILayout.SelectionGrid(Settings.UpgradeState, text, 3);
                GUILayout.EndHorizontal();
            }

            Settings.AlwaysAllowGenes = GUILayout.Toggle(Settings.AlwaysAllowGenes, "Always allow genes");
            Settings.AlwaysAllowCheats = GUILayout.Toggle(Settings.AlwaysAllowCheats, "Always allow cheats");
            Settings.AlwaysAllowAnyDifficulty = GUILayout.Toggle(Settings.AlwaysAllowAnyDifficulty, "Always allow any difficulty (Untested)");
            Settings.AnyStartingCountry = GUILayout.Toggle(Settings.AnyStartingCountry, "Allow any starting country");
            Settings.AllUpgradesAvailable = GUILayout.Toggle(Settings.AllUpgradesAvailable, "All traits can be evolved");
            Settings.AutoPopAll = GUILayout.Toggle(Settings.AutoPopAll, "Auto-pop (almost) all bubbles");
            Settings.DontInvertIcons = GUILayout.Toggle(Settings.DontInvertIcons, "Don't invert icons on evolved traits");
            Settings.CanEvolveTransmissions = GUILayout.Toggle(Settings.CanEvolveTransmissions, "Transmissions can auto-evolve");
            // CAN_EVOLVE_SYMPTOMS = GUILayout.Toggle(CAN_EVOLVE_SYMPTOMS, "Symptoms can auto-evolve");
            Settings.CanEvolveAbilities = GUILayout.Toggle(Settings.CanEvolveAbilities, "Abilities can auto-evolve");
            Settings.ShowDaysSinceInfection = GUILayout.Toggle(Settings.ShowDaysSinceInfection, "Show days since infection (instead of the current in-game date)");

            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Evolve cost multiplier (default 1, current {Settings.EvolveCostMultiplier:F2})", GUILayout.Width(300));
                Settings.EvolveCostMultiplier = GUILayout.HorizontalSlider(Settings.EvolveCostMultiplier, -1f, 10f);
                var reset = GUILayout.Button("Reset", GUILayout.Width(100));
                if (reset)
                {
                    Settings.EvolveCostMultiplier = 1f;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
    }
}
