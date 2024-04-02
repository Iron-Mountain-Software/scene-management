using UnityEditor;
using UnityEngine;

namespace IronMountain.SceneManagement.Editor
{
    [CustomEditor(typeof(SceneManager), true)]
    public class SceneManagerInspector : UnityEditor.Editor
    {
        private SceneManager _sceneManager;
        private UnityEditor.Editor _activeSceneInspector;
        private UnityEditor.Editor _databaseInspector;

        private void OnEnable()
        {
            _sceneManager = (SceneManager) target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneDatabase"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loadingSceneOrientation"));
            EditorGUILayout.Space();
            DrawTransitionSettings();
            EditorGUILayout.Space();
            
            SceneData activeSceneData = _sceneManager.GetActiveSceneData();
            
            if (_sceneManager && activeSceneData)
            {
                CreateCachedEditor(activeSceneData, null, ref _activeSceneInspector);
                _activeSceneInspector.OnInspectorGUI();
            }
            else _activeSceneInspector = null;
            
            if (_sceneManager && _sceneManager.SceneDatabase)
            {
                CreateCachedEditor(_sceneManager.SceneDatabase, null, ref _databaseInspector);
                _databaseInspector.OnInspectorGUI();
            }
            else _databaseInspector = null;
        }

        private void DrawTransitionSettings()
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.BeginVertical();
            EditorGUILayout.Space(5);
            GUILayout.EndVertical(); 
            GUILayout.BeginVertical(GUILayout.Width(85));
            EditorGUILayout.LabelField("Game Out", GUILayout.Width(85));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gameSceneFadeOutSeconds"), GUIContent.none, GUILayout.Width(55));
            GUILayout.EndVertical(); 
            GUILayout.BeginVertical(GUILayout.Width(85));
            EditorGUILayout.LabelField("Loading In", GUILayout.Width(85));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loadingSceneFadeInSeconds"), GUIContent.none, GUILayout.Width(60));;
            GUILayout.EndVertical(); 
            GUILayout.BeginVertical(GUILayout.Width(85));
            EditorGUILayout.LabelField("Loading Out", GUILayout.Width(85));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loadingSceneFadeOutSeconds"), GUIContent.none, GUILayout.Width(60));
            GUILayout.EndVertical(); 
            GUILayout.BeginVertical(GUILayout.Width(85));
            EditorGUILayout.LabelField("Game In", GUILayout.Width(85));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gameSceneFadeInSeconds"), GUIContent.none, GUILayout.Width(50));
            GUILayout.EndVertical(); 
            GUILayout.BeginVertical();
            EditorGUILayout.Space(5);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}