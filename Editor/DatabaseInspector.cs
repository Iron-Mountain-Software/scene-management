using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IronMountain.SceneManagement.Editor
{
    [CustomEditor(typeof(Database), true)]
    public class DatabaseInspector : UnityEditor.Editor
    {
        private static SceneListSorts.Type _sortType = SceneListSorts.Type.Path;

        private Database _database;
        private readonly Dictionary<string, bool> _pathViewDirectories = new ();
        private readonly Dictionary<SceneData, UnityEditor.Editor> _nestedEditors = new ();

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

            DrawSortButtons();
            
            switch (_sortType)
            {
                case SceneListSorts.Type.Path:
                    DrawPathView();
                    break;
                case SceneListSorts.Type.SceneName:
                    DrawAlphabeticalView();
                    break;
                case SceneListSorts.Type.BuildIndex:
                    DrawBuildListView();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawSortButtons()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sort:", GUILayout.Width(50), GUILayout.Height(25));
            EditorGUI.BeginDisabledGroup(_sortType == SceneListSorts.Type.Path);
            if (GUILayout.Button("Path", GUILayout.ExpandHeight(true))) _sortType = SceneListSorts.Type.Path;
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(_sortType == SceneListSorts.Type.SceneName);
            if (GUILayout.Button("Scene Name", GUILayout.ExpandHeight(true))) _sortType = SceneListSorts.Type.SceneName;
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(_sortType == SceneListSorts.Type.BuildIndex);
            if (GUILayout.Button("Build Index", GUILayout.ExpandHeight(true))) _sortType = SceneListSorts.Type.BuildIndex;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        private void DrawPathView()
        {
            _database.Scenes.Sort(SceneListSorts.ComparePath);
            string lastDirectory = string.Empty;
            foreach(SceneData sceneData in _database.Scenes)
            {
                if (!sceneData) continue;
                string directory = sceneData.Directory;
                bool draw = _pathViewDirectories.ContainsKey(directory) && _pathViewDirectories[directory];
                if (directory != lastDirectory)
                {
                    if (!_pathViewDirectories.ContainsKey(directory)) _pathViewDirectories.Add(directory, true);
                    _pathViewDirectories[directory] = EditorGUILayout.Foldout(_pathViewDirectories[directory], directory);
                    draw = _pathViewDirectories[directory];
                    lastDirectory = directory;
                }
                if (draw) DrawScene(sceneData, sceneData.SceneName, false, true);
            }
        }
        
        private void DrawAlphabeticalView()
        {           
            _database.Scenes.Sort(SceneListSorts.CompareSceneName);
            foreach(SceneData sceneData in _database.Scenes)
            {
                if (!sceneData) continue;
                DrawScene(sceneData, sceneData.SceneName, false, true);
            }
        }
        
        private void DrawBuildListView()
        {
            _database.Scenes.Sort(SceneListSorts.CompareBuildIndex);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Not In Build:");
            bool drewOtherLabel = false;
            foreach(SceneData sceneData in _database.Scenes)
            {
                if (!sceneData) continue;
                bool inBuild = IncludedInBuild(sceneData);
                if (inBuild && !drewOtherLabel)
                {
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical(GUILayout.Width(10));
                    EditorGUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("In Build:");
                    drewOtherLabel = true;
                }
                DrawScene(sceneData, sceneData.SceneName, true, inBuild);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawScene(SceneData sceneData, string label, bool drawBuildControls, bool inBuild)
        {
            if (!sceneData) return;
            GUILayout.BeginHorizontal();
            if (!_nestedEditors.ContainsKey(sceneData)) _nestedEditors.Add(sceneData, null);
            bool open = _nestedEditors[sceneData];
            open = EditorGUILayout.Toggle("", open, GUILayout.Width(20));
            if (open && !_nestedEditors[sceneData])
            {
                UnityEditor.Editor cachedEditor = null;
                CreateCachedEditor(sceneData, null, ref cachedEditor);
                _nestedEditors[sceneData] = cachedEditor;
            }
            else if (!open && _nestedEditors[sceneData])
            {
                _nestedEditors[sceneData] = null;
            }
            if (GUILayout.Button(label, GUILayout.Height(25))) EditorGUIUtility.PingObject(sceneData);
            if (GUILayout.Button("Load", GUILayout.Width(60), GUILayout.Height(25)))
            {
                if (Application.isPlaying)
                {
                    if (SceneManager.Instance) SceneManager.Instance.LoadScene(sceneData);
                    else UnityEngine.SceneManagement.SceneManager.LoadScene(sceneData.SceneName);
                }
                else EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneData.scene));
                Selection.activeObject = FindObjectOfType<SceneManager>();
            }

            if (drawBuildControls)
            {
                if (!inBuild && GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(25)))
                {
                    Append(sceneData);
                } 
                else if (inBuild && GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash"), GUILayout.Width(25), GUILayout.Height(25)))
                {
                    Remove(sceneData);
                }
            }
            GUILayout.EndHorizontal();
            
            if (_nestedEditors.ContainsKey(sceneData) && _nestedEditors[sceneData])
            {
                EditorGUI.indentLevel += 2;
                _nestedEditors[sceneData].OnInspectorGUI();
                EditorGUI.indentLevel -= 2;
            }
        }
        
        private static void Append(SceneData sceneData)
        {
            if (!sceneData) return;
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(entry.path, entry.enabled));
            }
            editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(sceneData.Path, true));
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }
        
        private static void Remove(SceneData sceneData)
        {
            if (!sceneData) return;
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes)
            {
                if (entry.path == sceneData.Path) continue;
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(entry.path, entry.enabled));
            }
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }

        private static bool IncludedInBuild(SceneData sceneData)
        {
            if (!sceneData) return false;
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                if (EditorBuildSettings.scenes[i].path == sceneData.Path) return true;
            }
            return false;
        }

        private void Rebuild()
        {
            _nestedEditors.Clear();
            _database.Scenes.Clear();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(SceneData)}");
            for( int i = 0; i < guids.Length; i++ )
            {
                string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
                SceneData sceneData = AssetDatabase.LoadAssetAtPath<SceneData>( assetPath );
                if (sceneData)
                {
                    sceneData.OnValidate();
                    _database.Scenes.Add(sceneData);
                    EditorUtility.SetDirty(sceneData);
                }
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