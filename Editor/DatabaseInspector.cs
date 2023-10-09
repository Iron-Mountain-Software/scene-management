using System.IO;
using SpellBoundAR.AssetManagement.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpellBoundAR.SceneManagement.Editor
{
    [CustomEditor(typeof(Database), true)]
    public class DatabaseInspector : SingletonDatabaseInspector
    {
        private Database _database;

        private void OnEnable()
        {
            _database = (Database) target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rebuild Lists"))
                RebuildLists();
            if (GUILayout.Button("Sort Lists"))
                SortLists();
            if (GUILayout.Button("Rebuild Dictionaries"))
                RebuildDictionaries();
            if (GUILayout.Button("Log & Copy Data"))
                Debug.Log(EditorGUIUtility.systemCopyBuffer = ToString());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loginScene"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("firstGameScene"));

            EditorGUILayout.BeginHorizontal();
            SerializedProperty scenes = serializedObject.FindProperty("scenes");
            EditorGUILayout.PropertyField(scenes);
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(50));
            if (scenes.isExpanded && scenes.FindPropertyRelative("list").isExpanded)
            {
                GUILayout.Space(EditorGUIUtility.singleLineHeight * 2.5f);
                foreach (SceneData sceneData in _database.Scenes.list)
                {
                    EditorGUI.BeginDisabledGroup(!sceneData);
                    if (GUILayout.Button("Load", GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)))
                    {
                        if (!sceneData) continue;
                        if (Application.isPlaying)
                        {
                            if (SceneManager.Instance) SceneManager.Instance.LoadScene(sceneData);
                            else UnityEngine.SceneManagement.SceneManager.LoadScene(sceneData.name);
                        }
                        else
                        {
                            string path = AssetDatabase.GetAssetPath(sceneData);
                            if (string.IsNullOrWhiteSpace(path)) return;
                            string directory = Path.GetDirectoryName(path);
                            string filename = Path.GetFileNameWithoutExtension(path);
                            EditorSceneManager.OpenScene(Path.Combine(directory, filename + ".unity"));
                        }
                        Selection.activeObject = FindObjectOfType<SceneManager>();
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            serializedObject.ApplyModifiedProperties();
        }

        protected override void RebuildLists()
        {
            Utilities.FillWithAssetsOfType(((Database) target).Scenes.list, target);
        }

        protected override void SortLists()
        {
            ((Database)target).Scenes.SortList();
        }

        protected override void RebuildDictionaries()
        {
            ((Database)target).Scenes.RebuildDictionary();
        }
        
        public override string ToString()
        {
            return ((Database)target).Scenes.ToString("Scenes");
        }
    }
}