using System;
using SpellBoundAR.SceneManagement.Launch;
using UnityEditor;
using UnityEngine;

namespace SpellBoundAR.SceneManagement.Editor
{
    [CustomEditor(typeof(SceneLaunchManager), true)]
    public class SceneLaunchManagerInspector : UnityEditor.Editor
    {
        private SceneLaunchManager _sceneLaunchManager;
        
        private void OnEnable()
        {
            _sceneLaunchManager = (SceneLaunchManager) target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUI.BeginDisabledGroup(_sceneLaunchManager && _sceneLaunchManager.Launching);
            if (GUILayout.Button("Launch", GUILayout.MinHeight(30))) _sceneLaunchManager.Launch();
            EditorGUI.EndDisabledGroup();
        }
    }
}
