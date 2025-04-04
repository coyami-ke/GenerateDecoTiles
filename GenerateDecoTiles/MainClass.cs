using System;
using System.Collections.Generic;
using System.Linq;
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
        static string areaStrX = "50";
        static string areaStrY = "50";
        static string countStr = "25";
        static string firstAngleStr = "0";
        static string secondAngleStr = "360";
        static bool enableFloat = true;

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("Decoration Tiles Generator", new GUIStyle { fontSize = 20, fontStyle = FontStyle.Bold } );

            GUILayout.BeginVertical("box");
            GUILayout.Label("Parallax Settings");
            firstParallaxStr = CreateLabeledTextField("First Parallax", firstParallaxStr);
            secondParallaxStr = CreateLabeledTextField("Second Parallax", secondParallaxStr);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.Label("Area Settings");
            areaStrX = CreateLabeledTextField("Area X", areaStrX);
            areaStrY = CreateLabeledTextField("Area Y", areaStrY);
            enableFloat = CreateLabledCheckBox("Enable Random Single-Floated", enableFloat);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.Label("Generation Settings");
            countStr = CreateLabeledTextField("Count", countStr);
            seedStr = CreateLabeledTextField("Seed", seedStr);
            firstAngleStr = CreateLabeledTextField("First Angle", firstAngleStr);
            secondAngleStr = CreateLabeledTextField("Second Angle", secondAngleStr);
            GUILayout.EndVertical();

            if (GUILayout.Button("Generate", GUILayout.Height(40)))
            {
                GenerateObjects();
            }
        }

        static string CreateLabeledTextField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(100));
            value = GUILayout.TextField(value, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            return value;
        }
        static bool CreateLabledCheckBox(string label, bool value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(100));
            value = GUILayout.Toggle(value, GUIContent.none, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            return value;
        }

        static void GenerateObjects()
        {
            if (scnEditor.instance != null && scnEditor.instance.selectedFloors.Count > 0)
            {
                int firstParallax = SafeConvert(firstParallaxStr);
                int secondParallax = SafeConvert(secondParallaxStr);
                int seed = SafeConvert(seedStr);
                int areaX = SafeConvert(areaStrX);
                int areaY = SafeConvert(areaStrY);
                int count = SafeConvert(countStr);
                int firstAngle = SafeConvert(firstAngleStr);
                int secondAngle = SafeConvert(secondAngleStr);

                System.Random random = new System.Random(seed);

                List<LevelEvent> events = new List<LevelEvent>();

                using (new SaveStateScope(scnEditor.instance))
                {
                    for (int i = 0; i < count; i++)
                    {
                        LevelEvent levelEvent = CustomLevelHelper.CreateDecoration(LevelEventType.AddObject);
                        levelEvent["tag"] = "generated";
                        Vector2 position = new Vector2(random.Next(-areaX / 2, areaX / 2), random.Next(-areaY / 2, areaY / 2));
                        if (enableFloat)
                        {
                            float newX = position.x + (float)random.NextDouble();
                            float newY = position.y + (float)random.NextDouble();
                            position = new Vector2(newX, newY);
                        }
                        int parallax = random.Next(firstParallax, secondParallax);
                        float rotation = random.Next(-360, 360);
                        if (enableFloat) rotation += (float)random.NextDouble();
                        levelEvent.data["relativeTo"] = DecPlacementType.Tile;
                        levelEvent.data["position"] = position;
                        levelEvent.data["parallax"] = new Vector2(parallax, parallax);
                        levelEvent.data["depth"] = parallax;
                        int scale = (parallax < 0) ? Math.Abs(parallax) + 100 : 100 - parallax;
                        levelEvent.data["trackAngle"] = random.Next(firstAngle, secondAngle);
                        levelEvent.data["scale"] = new Vector2(scale, scale);
                        levelEvent.data["rotation"] = rotation;
                        levelEvent.floor = scnEditor.instance.selectedFloors[0].seqID;

                        scnEditor.instance.AddDecoration(levelEvent);

                        events.Add(levelEvent);
                    }
                }

                //scnEditor.instance.ApplyEventsToFloors();

                //scnEditor.instance.DeselectAllDecorations();

                //foreach (var decoration in events)
                //{
                //    scnEditor.instance.SelectDecoration(decoration, false, false, true, false);
                //}

                //LevelEvent lastEvent = events.Last();
                //scnEditor.instance.levelEventsPanel.ShowInspector(true, false);
                //scnEditor.instance.levelEventsPanel.ShowPanel(lastEvent.eventType, 0);
                //scnEditor.instance.propertyControlDecorationsList.RefreshScrollRectPosition(lastEvent);
                //events.Clear();

                //scnEditor.instance.DeselectAnyUIGameObject();

                int floor = scnEditor.instance.selectedFloors[0].seqID;
                string lastOpened = Persistence.GetLastOpenedLevel();

                scnEditor.instance.SaveLevel();

                scnEditor.instance.OpenLevel(lastOpened);

                scnEditor.instance.SelectFloor(scnEditor.instance.floors[floor], true);
            }
        }

        static int SafeConvert(string str)
        {
            return int.TryParse(str, out int result) ? result : 0;
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
