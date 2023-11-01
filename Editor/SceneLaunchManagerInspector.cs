using IronMountain.SceneManagement.Packages.Scene_Management.Runtime.Launch;
using UnityEditor;
using UnityEngine;

namespace IronMountain.SceneManagement.Packages.Scene_Management.Editor
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
            if (Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(_sceneLaunchManager && _sceneLaunchManager.Launching);
                if (GUILayout.Button("Launch", GUILayout.MinHeight(30))) _sceneLaunchManager.Launch();
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}
