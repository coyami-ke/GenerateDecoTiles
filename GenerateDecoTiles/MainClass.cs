using System;
using System.Reflection;
using ADOFAI;
using DG.Tweening.Plugins.Options;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace GenerateDecoTiles
{
    public static class MainClass
    {
        public static bool IsEnabled { get; private set; }
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static Settings Settings { get; private set; } = new Settings();

        private static Harmony harmony;
        internal static void Setup(UnityModManager.ModEntry modEntry) 
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;

            Settings = ModSettings.Load<Settings>(modEntry);
        }
        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) 
        {
            IsEnabled = value;
            if (value)
            {
                StartMod(modEntry);
            }
            else 
            {
                StopMod(modEntry);
            }
            return true;
        }

        static string firstParallaxStr = "20";
        static string secondParallaxStr = "50";
        static string seedStr = "0";
        static string areaStr = "50";
        static string countStr = "25";
        static string firstAngleStr = "0";
        static string secondAngleStr = "360";
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            // Define static variables so that their values persist between frames

            int firstParallax = 0;
            int secondParallax = 0;
            int seed = 0;
            int area = 0;
            int count = 0;
            int firstAngle = 0;
            int secondAngle = 0;

            // Add labels and corresponding input fields
            GUILayout.Label("First Parallax");
            firstParallaxStr = GUILayout.TextField(firstParallaxStr);

            GUILayout.Label("Second Parallax");
            secondParallaxStr = GUILayout.TextField(secondParallaxStr);

            GUILayout.Label("Seed");
            seedStr = GUILayout.TextField(seedStr);

            GUILayout.Label("Area");
            areaStr = GUILayout.TextField(areaStr);

            GUILayout.Label("Count");
            countStr = GUILayout.TextField(countStr);

            GUILayout.Label("First Angle");
            firstAngleStr = GUILayout.TextField(firstAngleStr);

            GUILayout.Label("Second Angle");
            secondAngleStr = GUILayout.TextField(secondAngleStr);

            // Parse the input values from strings to integers
            try { firstParallax = Convert.ToInt32(firstParallaxStr); }
            catch { firstParallax = 0; }

            try { secondParallax = Convert.ToInt32(secondParallaxStr); }
            catch { secondParallax = 0; }

            try { seed = Convert.ToInt32(seedStr); }
            catch { seed = 0; }

            try { area = Convert.ToInt32(areaStr); }
            catch { area = 0; }

            try { count = Convert.ToInt32(countStr); }
            catch { count = 0; }

            try { firstAngle = Convert.ToInt32(firstAngleStr); }
            catch { firstAngle = 0; }

            try { secondAngle = Convert.ToInt32(secondAngleStr); }
            catch { secondAngle = 0; }

            if (GUILayout.Button("Generate"))
            {
                if (scnEditor.instance != null && scnEditor.instance.selectedFloors.Count > 0)
                {
                    System.Random random = new System.Random(seed);
                    using (new SaveStateScope(scnEditor.instance))
                    {
                        for (int i = 0; i < count; i++)
                        {
                            LevelEvent levelEvent = CustomLevelHelper.CreateDecoration(LevelEventType.AddObject);
                            levelEvent["tag"] = "generated";
                            Vector2 position = new Vector2
                            {
                                x = random.Next(-area / 2, area / 2),
                                y = random.Next(-area / 2, area / 2),
                            };
                            int parallax = random.Next(firstParallax, secondParallax);

                            int rotation = random.Next(-360, 360);

                            levelEvent.data["relativeTo"] = DecPlacementType.Tile;
                            levelEvent.data["position"] = position;
                            levelEvent.data["parallax"] = new Vector2(parallax, parallax);
                            levelEvent.data["depth"] = parallax;

                            int scale;
                            if (parallax < 0) scale = Math.Abs(parallax) + 100;
                            else scale = 100 - parallax;

                            int trackAngle = random.Next(firstAngle, secondAngle);

                            levelEvent.data["trackAngle"] = trackAngle;

                            levelEvent.data["scale"] = new Vector2(scale, scale);
                            levelEvent.data["rotation"] = rotation;

                            levelEvent.floor = scnEditor.instance.selectedFloors[0].seqID;

                            CustomLevelHelper.AddDecoration(levelEvent);
                        }
                    }
                    scnEditor.instance.RemakePath();
                }
            }
        }
        private static void StartMod(UnityModManager.ModEntry modEntry) 
        {
            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        private static void StopMod(UnityModManager.ModEntry modEntry) 
        {
            harmony.UnpatchAll(modEntry.Info.Id);
        }
    }
}
