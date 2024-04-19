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
            EditorBuildSettings.sceneListChanged += Rebuild;
            SceneDataManager.OnSceneDataChanged += OnSceneDataChanged;
        }

        private void OnDisable()
        {
            EditorBuildSettings.sceneListChanged -= Rebuild;
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

        private readonly Color _activeColor = new (1, 1, 1, 1f);
        private readonly Color _inactiveColor = new (1, 1, 1, .2f);
        
        private void DrawSortButtons()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(25));
            GUI.backgroundColor = _sortType == SceneListSorts.Type.Path ? _activeColor : _inactiveColor;
            if (GUILayout.Button("Path", GUILayout.ExpandHeight(true))) _sortType = SceneListSorts.Type.Path;
            GUI.backgroundColor = _sortType == SceneListSorts.Type.SceneName ? _activeColor : _inactiveColor;
            if (GUILayout.Button("Scene Name", GUILayout.ExpandHeight(true))) _sortType = SceneListSorts.Type.SceneName;
            GUI.backgroundColor = _sortType == SceneListSorts.Type.BuildIndex ? _activeColor : _inactiveColor;
            if (GUILayout.Button("Build Index", GUILayout.ExpandHeight(true))) _sortType = SceneListSorts.Type.BuildIndex;
            GUI.backgroundColor = Color.white;
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
            GUILayout.Label("Not included in build:");
            bool drewOtherLabel = false;
            foreach(SceneData sceneData in _database.Scenes)
            {
                if (!sceneData) continue;
                bool inBuild = sceneData.BuildIndex >= 0;
                if (inBuild && !drewOtherLabel)
                {
                    GUILayout.Label("Included in build:");
                    drewOtherLabel = true;
                }
                bool modified = DrawScene(sceneData, sceneData.SceneName, true, inBuild);
                if (modified) return;
            }
        }

        private bool DrawScene(SceneData sceneData, string label, bool drawBuildControls, bool inBuild)
        {
            if (!sceneData) return false;
            GUILayout.BeginHorizontal(GUILayout.Height(30));
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
            
            if (drawBuildControls && inBuild)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(30));
                bool first = sceneData.BuildIndex == 0;
                bool last = sceneData.BuildIndex == EditorBuildSettings.scenes.Length - 1;
                if (!first && GUILayout.Button("▲", GUILayout.ExpandWidth(true), GUILayout.Height(last ? 30 : 15)))
                {
                    MoveUp(sceneData);
                    return true;
                }
                if (!last && GUILayout.Button("▼", GUILayout.ExpandWidth(true), GUILayout.Height(first ? 30 : 15)))
                {
                    MoveDown(sceneData);
                    return true;
                }
                EditorGUILayout.EndVertical();
            }
            
            if (GUILayout.Button(label, GUILayout.ExpandHeight(true))) EditorGUIUtility.PingObject(sceneData);
            
            if (GUILayout.Button("Load", GUILayout.ExpandHeight(true), GUILayout.Width(50)))
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
                if (!inBuild && GUILayout.Button("＋", GUILayout.Width(30), GUILayout.ExpandHeight(true)))
                {
                    Append(sceneData);
                    return true;
                }
                if (inBuild && GUILayout.Button("-", GUILayout.Width(30), GUILayout.ExpandHeight(true)))
                {
                    Remove(sceneData);
                    return true;
                }
            }
            GUILayout.EndHorizontal();
            
            if (_nestedEditors.ContainsKey(sceneData) && _nestedEditors[sceneData])
            {
                EditorGUI.indentLevel += 2;
                _nestedEditors[sceneData].OnInspectorGUI();
                EditorGUI.indentLevel -= 2;
            }

            return false;
        }
        
        private void MoveUp(SceneData sceneData)
        {
            if (!sceneData) return;
            int sceneIndex = -1;
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            for (int index = 0; index < EditorBuildSettings.scenes.Length; index++)
            {
                EditorBuildSettingsScene entry = EditorBuildSettings.scenes[index];
                scenes.Add(entry);
                if (entry.path == sceneData.Path) sceneIndex = index;
            }
            if (sceneIndex <= 0) return;
            EditorBuildSettingsScene temp = scenes[sceneIndex - 1];
            scenes[sceneIndex - 1] = scenes[sceneIndex];
            scenes[sceneIndex] = temp;
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        
        private void MoveDown(SceneData sceneData)
        {
            if (!sceneData) return;
            int sceneIndex = -1;
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            for (int index = 0; index < EditorBuildSettings.scenes.Length; index++)
            {
                EditorBuildSettingsScene entry = EditorBuildSettings.scenes[index];
                scenes.Add(entry);
                if (entry.path == sceneData.Path) sceneIndex = index;
            }
            if (sceneIndex < 0 || sceneIndex >= scenes.Count) return;
            EditorBuildSettingsScene temp = scenes[sceneIndex + 1];
            scenes[sceneIndex + 1] = scenes[sceneIndex];
            scenes[sceneIndex] = temp;
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        
        private void Append(SceneData sceneData)
        {
            if (!sceneData) return;
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes)
            {
                scenes.Add(entry);
            }
            scenes.Add(new EditorBuildSettingsScene(sceneData.Path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        
        private void Remove(SceneData sceneData)
        {
            if (!sceneData) return;
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes)
            {
                if (entry.path == sceneData.Path) continue;
                scenes.Add(entry);
            }
            EditorBuildSettings.scenes = scenes.ToArray();
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