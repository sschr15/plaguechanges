using System;
using System.Reflection;
using Harmony;
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
        public byte DefaultCustomScenarioTab;
        public bool DisableAutomaticMutation;

        public bool UseCustomInfectivity;
        public float CustomInfectivity;
        public bool UseCustomSeverity;
        public float CustomSeverity;
        public bool UseCustomLethality;
        public float CustomLethality;

        public bool UseCustomTransmissionValues;
        // Note - these transmissions are applied via a roundabout "multi-method" manner
        // to hopefully deduplicate most of the code.
        public static readonly string[] OtherCustomTransmissionNames =
        {
            "Global Air",
            "Global Sea",
            "Global Land",
            "Wealthy",
            "Poor",
            "Urban",
            "Rural",
            "Hot",
            "Cold",
            "Humid",
            "Arid",
            "Land",
            "Sea",
            "Air",
            "Corpse",
        };

        public static readonly string[] InternalCustomTransmissionNames =
        {
            "globalAirRate",
            "globalSeaRate",
            "globalLandRate",
            "wealthy",
            "poverty",
            "urban",
            "rural",
            "hot",
            "cold",
            "humid",
            "arid",
            "landTransmission",
            "seaTransmission",
            "airTransmission",
            "corpseTransmission",
        };
        public bool[] OtherCustomTransmissions = new bool[OtherCustomTransmissionNames.Length];
        public float[] OtherCustomTransmissionValues = new float[OtherCustomTransmissionNames.Length];

        public bool CustomFps;
        public int Fps = 60;
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

        private static Vector2 _scrollPosition = Vector2.zero;
        private static bool _showCustomTransmissionValues = true;
        private static bool _fpsSettingsChanged = false;

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
            entry.OnSaveGUI = self =>
            {
                UnityModManager.ModSettings.Save(Settings, self);
                if (ACTIVE && Settings.CustomFps)
                {
                    Application.targetFrameRate = Settings.Fps;
                }
                else
                {
                    Application.targetFrameRate = (int) COptionsManager.instance.videoSettings.targetFps;
                }

                _fpsSettingsChanged = false;
            };
            new HarmonyLib.Harmony(entry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        private static void OnGui(UnityModManager.ModEntry entry)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            GUILayout.BeginVertical();

            GUILayout.Label("All settings update immediately unless otherwise noted. Many visual changes require a screen refresh to visibly take effect, but the underlying values are changed immediately.", new GUIStyle(GUI.skin.label) { wordWrap = true });
            GUILayout.Space(30);

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

            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Default custom scenario tab");
                var text = new[] { "Featured", "All", "New", "Subscribed", "Local" };
                Settings.DefaultCustomScenarioTab = (byte)GUILayout.SelectionGrid(Settings.DefaultCustomScenarioTab, text, 5);
                GUILayout.EndHorizontal();
            }

            Settings.DisableAutomaticMutation = GUILayout.Toggle(Settings.DisableAutomaticMutation, "Disable automatic mutations");

            {
                GUILayout.BeginHorizontal();
                Settings.UseCustomInfectivity = GUILayout.Toggle(Settings.UseCustomInfectivity, "Set infectivity", GUILayout.Width(150));
                if (Settings.UseCustomInfectivity)
                {
                    GUILayout.Label($"{Settings.CustomInfectivity:F2}", GUILayout.Width(50));
                    Settings.CustomInfectivity = GUILayout.HorizontalSlider(Settings.CustomInfectivity, 0f, 1000f);
                }
                GUILayout.EndHorizontal();
            }

            {
                GUILayout.BeginHorizontal();
                Settings.UseCustomSeverity = GUILayout.Toggle(Settings.UseCustomSeverity, "Set severity", GUILayout.Width(150));
                if (Settings.UseCustomSeverity)
                {
                    GUILayout.Label($"{Settings.CustomSeverity:F2}", GUILayout.Width(50));
                    Settings.CustomSeverity = GUILayout.HorizontalSlider(Settings.CustomSeverity, 0f, 1000f);
                }
                GUILayout.EndHorizontal();
            }

            {
                GUILayout.BeginHorizontal();
                Settings.UseCustomLethality = GUILayout.Toggle(Settings.UseCustomLethality, "Set lethality", GUILayout.Width(150));
                if (Settings.UseCustomLethality)
                {
                    GUILayout.Label($"{Settings.CustomLethality:F2}", GUILayout.Width(50));
                    Settings.CustomLethality = GUILayout.HorizontalSlider(Settings.CustomLethality, 0f, 1000f);
                }
                GUILayout.EndHorizontal();
            }

            {
                GUILayout.BeginHorizontal();
                Settings.UseCustomTransmissionValues = GUILayout.Toggle(Settings.UseCustomTransmissionValues, "Set transmission values", GUILayout.Width(200));
                if (Settings.UseCustomTransmissionValues)
                {
                    _showCustomTransmissionValues = GUILayout.Toggle(_showCustomTransmissionValues, "Show", GUILayout.Width(50));
                }
                GUILayout.EndHorizontal();

                if (Settings.UseCustomTransmissionValues && _showCustomTransmissionValues)
                {
                    GUIStyle line = new GUIStyle(GUI.skin.label)
                    {
                        normal = new GUIStyleState { background = Texture2D.whiteTexture, textColor = Color.white },
                        margin = new RectOffset(0, 0, 4, 4),
                        fixedHeight = 1
                    };

                    GUILayout.Box(GUIContent.none, line);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.BeginVertical();
                    for (int i = 0; i < Data.OtherCustomTransmissionNames.Length; i++)
                    {
                        var name = Data.OtherCustomTransmissionNames[i];
                        var value = Settings.OtherCustomTransmissionValues[i];
                        var b = Settings.OtherCustomTransmissions[i];
                        GUILayout.BeginHorizontal();
                        b = GUILayout.Toggle(b, name, GUILayout.Width(150));
                        if (b)
                        {
                            GUILayout.Label($"{value:F2}", GUILayout.Width(50));
                            value = GUILayout.HorizontalSlider(value, 0f, 1000f);
                        }
                        GUILayout.EndHorizontal();
                        Settings.OtherCustomTransmissions[i] = b;
                        Settings.OtherCustomTransmissionValues[i] = value;
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    GUILayout.Box(GUIContent.none, line);
                }
            }

            {
                var customFps = Settings.CustomFps;
                GUILayout.BeginHorizontal();
                Settings.CustomFps = GUILayout.Toggle(Settings.CustomFps, "Set FPS", GUILayout.Width(150));

                if (customFps != Settings.CustomFps)
                {
                    _fpsSettingsChanged = true;
                }

                if (Settings.CustomFps)
                {
                    var fps = Settings.Fps;
                    var text = Settings.Fps == -1 ? "Unlimited" : Settings.Fps.ToString();
                    GUILayout.Label(text, GUILayout.Width(60));
                    Settings.Fps = (int) GUILayout.HorizontalSlider(Settings.Fps, -1, 512);
                    Settings.Fps = Settings.Fps == 0 ? -1 : Settings.Fps;
                    if (fps != Settings.Fps)
                    {
                        _fpsSettingsChanged = true;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Label(_fpsSettingsChanged ? "Note: Click the \"Save\" button below to apply the FPS setting" : " ");
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }

    public class AssemblyCalledMethods
    {
        public static void OnSetActive(CMainCustomSubScreen self)
        {
            if (!Main.ACTIVE) return;

            var inst = Traverse.Create(self);
            var showFeatured = inst.Field<bool>("showFeatured");
            var showAll = inst.Field<bool>("showAll");
            var showNew = inst.Field<bool>("showNew");
            var showSubscribed = inst.Field<bool>("showSubscribed");
            var showLocal = inst.Field<bool>("showLocal");

            switch (Main.Settings.DefaultCustomScenarioTab)
            {
                case 0:
                    showFeatured.Value = true;
                    showAll.Value = false;
                    showNew.Value = false;
                    showSubscribed.Value = false;
                    showLocal.Value = false;
                    break;
                case 1:
                    showFeatured.Value = false;
                    showAll.Value = true;
                    showNew.Value = false;
                    showSubscribed.Value = false;
                    showLocal.Value = false;
                    break;
                case 2:
                    showFeatured.Value = false;
                    showAll.Value = false;
                    showNew.Value = true;
                    showSubscribed.Value = false;
                    showLocal.Value = false;
                    break;
                case 3:
                    showFeatured.Value = false;
                    showAll.Value = false;
                    showNew.Value = false;
                    showSubscribed.Value = true;
                    showLocal.Value = false;
                    break;
                case 4:
                    showFeatured.Value = false;
                    showAll.Value = false;
                    showNew.Value = false;
                    showSubscribed.Value = false;
                    showLocal.Value = true;
                    break;
            }
        }
        
        public static int SetFps(int fps)
        {
            if (!Main.ACTIVE) return fps;
            return Main.Settings.Fps;
        }
    }
}
