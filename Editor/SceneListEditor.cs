using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpellBoundAR.SceneManagement.Editor
{
    [CustomEditor(typeof(SceneList), true)]
    public class SceneListEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            if (GUILayout.Button("Use This SceneList"))
            {
                UseSceneList((SceneList)target);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            DrawDefaultInspector();
        }

        private static void UseSceneList(SceneList sceneList)
        {
            if (!sceneList) return;
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
            foreach (SceneAsset sceneAsset in sceneList.Scenes)
            {
                string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                if (!string.IsNullOrEmpty(scenePath))
                    editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }
    }
}
