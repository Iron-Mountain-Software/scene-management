using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IronMountain.SceneManagement.Editor
{
    [CustomEditor(typeof(Database), true)]
    public class DatabaseInspector : UnityEditor.Editor
    {
        private Database _database;

        private void OnEnable()
        {
            _database = (Database) target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loginScene"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("firstGameScene"));
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rebuild Scene List"))
            {
                RebuildSceneList();
            }
            if (GUILayout.Button("Sort Scene List"))
            {
                _database.SortList();
            }
            if (GUILayout.Button("Rebuild Dictionary"))
            {
                _database.RebuildDictionary();
            }
            if (GUILayout.Button("Log & Copy Data"))
            {
                string data = _database.ToString();
                EditorGUIUtility.systemCopyBuffer = data;
                Debug.Log(data);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            SerializedProperty scenes = serializedObject.FindProperty("scenes");
            EditorGUILayout.PropertyField(scenes);
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(50));
            if (scenes.isExpanded)
            {
                GUILayout.Space(EditorGUIUtility.singleLineHeight * 1.5f);
                foreach (SceneData sceneData in _database.Scenes)
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

        private void RebuildSceneList()
        {
            _database.Scenes.Clear();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(SceneData)}");
            for( int i = 0; i < guids.Length; i++ )
            {
                string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
                SceneData asset = AssetDatabase.LoadAssetAtPath<SceneData>( assetPath );
                if (asset) _database.Scenes.Add(asset);
            }
            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}