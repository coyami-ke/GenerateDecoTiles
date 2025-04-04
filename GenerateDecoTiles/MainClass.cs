using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ADOFAI;
using DG.Tweening.Plugins.Options;
using GenerateDecoTiles.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI.Extensions;
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

        private static readonly MethodInfo LevelEditorCreateDecorationMethod =
            AccessTools.Method(typeof(scnEditor), "CreateDecoration");

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
                StartMod(modEntry);
            else 
                StopMod(modEntry);

            return true;
        }

        #region Settings Field
        private static float minParallax = 20;
        private static float maxParallax = 50;
        private static int seed = 0;
        private static float areaX = 50;
        private static float areaY = 50;
        private static int count = 25;
        private static float minAngle = 0;
        private static float maxAngle = 360;

        private static int minTag = 0;
        private static int maxTag = 5;
        private static string tag = "generated";

        private static bool enableFloat = true;
        #endregion

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("Decoration Tiles Generator", new GUIStyle { fontSize = 20, fontStyle = FontStyle.Bold } );

            GUILayout.BeginVertical("box");
            GUILayout.Label("Parallax Settings (Parallax minimum & maximum values for decorations)");
            minParallax = Convert.ToSingle(GUILayoutUtils.CreateLabeledTextField("Min. Parallax Value", minParallax.ToString()));
            maxParallax = Convert.ToSingle(GUILayoutUtils.CreateLabeledTextField("Max. Parallax Value", maxParallax.ToString())); 
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.Label("Area Settings (Total area of decoration generation area including negative space)");
            areaX = Convert.ToSingle(GUILayoutUtils.CreateLabeledTextField("Area X", areaX.ToString()));
            areaY = Convert.ToSingle(GUILayoutUtils.CreateLabeledTextField("Area Y", areaY.ToString()));
            enableFloat = GUILayoutUtils.CreateLabledCheckBox("Enable Random Single-Floated", enableFloat);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.Label("Generation Settings");
            count = Convert.ToInt32(GUILayoutUtils.CreateLabeledTextField("Count", count.ToString())); 
            seed = Convert.ToInt32(GUILayoutUtils.CreateLabeledTextField("Seed", seed.ToString()));
            minAngle = Convert.ToSingle(GUILayoutUtils.CreateLabeledTextField("Min. Track Angle", minAngle.ToString()));
            maxAngle = Convert.ToSingle(GUILayoutUtils.CreateLabeledTextField("Max. Track Angle", maxAngle.ToString()));
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.Label("Generation Tag");
            tag = GUILayoutUtils.CreateLabeledTextField("Tag", tag);
            minTag = Convert.ToInt32(GUILayoutUtils.CreateLabeledTextField("Minimum. Tag", minTag.ToString()));
            maxTag = Convert.ToInt32(GUILayoutUtils.CreateLabeledTextField("Maximum. Tag", maxTag.ToString()));

            if (GUILayout.Button("Generate", GUILayout.Height(40), GUILayout.Width(300)))
            {
                GenerateObjects();
            }
        }

        private static void GenerateObjects()
        {
            var editor = scnEditor.instance;

            if (editor == null) return;
            if (editor.selectedFloors.Count <= 0) return;

            var random = new System.Random(seed);
            var events = new List<LevelEvent>();

            var areaXHalf = areaX / 2;
            var areaYHalf = areaY / 2;

            var decorationsLastIndex = editor.decorations.Count - 1;

            using (new SaveStateScope(editor))
            {
                for (var i = 0; i < count; i++)
                {
                    var levelEvent = (LevelEvent)LevelEditorCreateDecorationMethod.Invoke(editor, new object[] { LevelEventType.AddObject });//CustomLevelHelper.CreateDecoration(LevelEventType.AddObject);
                    levelEvent["tag"] = $"{tag}{random.Next(minTag, maxTag)}";

                    var position = new Vector2((float)random.NextDouble() * areaX - areaXHalf, (float)random.NextDouble() * areaY - areaYHalf);
                    if (enableFloat)
                    {
                        float newX = position.x + (float)random.NextDouble();
                        float newY = position.y + (float)random.NextDouble();
                        position = new Vector2(newX, newY);
                    }

                    var parallax = (float)random.NextDouble() * (maxParallax - minParallax) + minParallax;

                    float rotation = random.Next(-360, 360);
                    if (enableFloat) rotation += (float)random.NextDouble();

                    levelEvent.data["relativeTo"] = DecPlacementType.Tile;
                    levelEvent.data["position"] = position;
                    levelEvent.data["parallax"] = new Vector2(parallax, parallax);
                    levelEvent.data["depth"] = (int)parallax; // fixed
                    levelEvent.data["trackAngle"] = (float)random.NextDouble() * (maxAngle - minAngle) + minAngle;

                    float scale;
                    if (parallax < 0) scale = Mathf.Abs(parallax) + 100;
                    else scale = 100 - parallax;
                    levelEvent.data["scale"] = new Vector2(scale, scale);

                    levelEvent.data["rotation"] = rotation;
                    levelEvent.floor = editor.selectedFloors[0].seqID;

                    editor.AddDecoration(levelEvent);
                }
            }

            editor.ApplyEventsToFloors();
            
            editor.UpdateDecorationObjects();

            //editor.DeselectAllDecorations();

            //foreach (var decoration in events)
            //{
            //    editor.SelectDecoration(decoration, false, false, true, false);
            //}

            //LevelEvent lastEvent = events.Last();
            //editor.levelEventsPanel.ShowInspector(true, false);
            //editor.levelEventsPanel.ShowPanel(lastEvent.eventType, 0);
            //editor.propertyControlDecorationsList.RefreshScrollRectPosition(lastEvent);
            //events.Clear();

            //editor.DeselectAnyUIGameObject();

            //int floor = editor.selectedFloors[0].seqID;
            //string lastOpened = Persistence.GetLastOpenedLevel();

            //editor.SaveLevel();

            //editor.OpenLevel(lastOpened);

            //editor.SelectFloor(editor.floors[floor], true);
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
