using System;
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
            SceneDataManager.OnSceneDataChanged += OnSceneDataChanged;
        }

        private void OnDisable()
        {
            SceneDataManager.OnSceneDataChanged -= OnSceneDataChanged;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loginScene"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("firstGameScene"));
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create", GUILayout.Height(30)))
            {
                NewSceneWindow.Open();
            }
            if (GUILayout.Button("Rebuild", GUILayout.Height(30)))
            {
                Rebuild();
            }
            if (GUILayout.Button("Log & Copy Data", GUILayout.Height(30)))
            {
                string data = _database.ToString();
                EditorGUIUtility.systemCopyBuffer = data;
                Debug.Log(data);
            }
            EditorGUILayout.EndHorizontal();

            foreach (SceneData sceneData in _database.Scenes)
            {
                DrawScene(sceneData);
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScene(SceneData sceneData)
        {
            if (!sceneData) return;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(sceneData.name, GUILayout.Height(25))) EditorGUIUtility.PingObject(sceneData);
            if (GUILayout.Button("Load", GUILayout.Width(50), GUILayout.Height(25)))
            {
                if (Application.isPlaying)
                {
                    if (SceneManager.Instance) SceneManager.Instance.LoadScene(sceneData);
                    else UnityEngine.SceneManagement.SceneManager.LoadScene(sceneData.SceneName);
                }
                else EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneData.scene));
                Selection.activeObject = FindObjectOfType<SceneManager>();
            }
            GUILayout.EndHorizontal();
        }

        private void Rebuild()
        {
            _database.Scenes.Clear();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(SceneData)}");
            for( int i = 0; i < guids.Length; i++ )
            {
                string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
                SceneData asset = AssetDatabase.LoadAssetAtPath<SceneData>( assetPath );
                if (asset) _database.Scenes.Add(asset);
            }
            _database.SortList();
            _database.RebuildDictionary();
            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void OnSceneDataChanged()
        {
            bool dirty = false;
            foreach (SceneData sceneData in SceneDataManager.SceneData)
            {
                if (!sceneData || _database.Scenes.Contains(sceneData)) continue;
                _database.Scenes.Add(sceneData);
                dirty = true;
            }
            if (dirty)
            {
                _database.SortList();
                _database.RebuildDictionary();
                EditorUtility.SetDirty(_database);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}