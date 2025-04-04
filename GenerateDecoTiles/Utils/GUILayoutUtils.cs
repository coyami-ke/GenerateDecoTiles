using UnityEngine;

namespace GenerateDecoTiles.Utils
{
    public static class GUILayoutUtils 
    {
        public static string CreateLabeledTextField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(100));
            value = GUILayout.TextField(value, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            return value;
        }

        public static bool CreateLabledCheckBox(string label, bool value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(100));
            value = GUILayout.Toggle(value, GUIContent.none, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            return value;
        }
    }
}