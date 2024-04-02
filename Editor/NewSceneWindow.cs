using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IronMountain.SceneManagement.Editor
{
    public class NewSceneWindow : EditorWindow
    {
        private static int _sceneDataTypeIndex = 0;

        private string _folder = "Assets";
        private string _name = "New Scene";

        public static void Open()
        {
            NewSceneWindow window = GetWindow(typeof(NewSceneWindow), false, "Create Scene", true) as NewSceneWindow;
            window.minSize = new Vector2(520, 225);
            window.maxSize = new Vector2(520, 225);
            window.wantsMouseMove = true;
        }

        protected void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            _folder = EditorGUILayout.TextField("Folder: ", _folder);
            if (GUILayout.Button("Current", GUILayout.Width(60))) _folder = GetCurrentFolder();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            _name = EditorGUILayout.TextField("Name", _name);
            if (GUILayout.Button("Current", GUILayout.Width(60))) _name = GetCurrentName();
            EditorGUILayout.EndHorizontal();

            _sceneDataTypeIndex = EditorGUILayout.Popup("Type", _sceneDataTypeIndex, TypeIndex.SceneDataTypeNames);
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Create", GUILayout.Height(35))) Create();
        }

        private string GetCurrentFolder()
        {
            Type projectWindowUtilType = typeof(ProjectWindowUtil);
            MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            return getActiveFolderPath is not null 
                ? getActiveFolderPath.Invoke(null, new object[0]).ToString()
                : string.Empty;
        }
        
        private string GetCurrentName()
        {
            string[] subfolders = _folder.Split(Path.DirectorySeparatorChar);
            return subfolders.Length > 0 ? subfolders[^1] : string.Empty;
        }

        private void Create()
        {
            CreateFolders();
            string scenePath = Path.Combine(_folder, _name + ".unity");

            SceneManager sceneManagerPrefab = null;
            if (SceneManager.Instance)
            {
                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(SceneManager.Instance);
                sceneManagerPrefab = AssetDatabase.LoadAssetAtPath<SceneManager>(path);
            }
            
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = _name;
            if (sceneManagerPrefab)
            {
                GameObject sceneManager = PrefabUtility.InstantiatePrefab(sceneManagerPrefab) as GameObject;
                if (sceneManager)
                {
                    sceneManager.transform.SetSiblingIndex(0);
                    Selection.activeObject = sceneManager;
                }
            }
            EditorSceneManager.SaveScene(scene, scenePath);

            Type sceneDataType = TypeIndex.SceneDataTypes[_sceneDataTypeIndex];
            SceneData sceneData = CreateInstance(sceneDataType) as SceneData;
            if (!sceneData) return;
            sceneData.scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            sceneData.OnValidate();
            string sceneDataPath = Path.Combine(_folder, _name + ".asset");

            AssetDatabase.CreateAsset(sceneData, sceneDataPath);
            EditorUtility.SetDirty(sceneData);
            AssetDatabase.SaveAssetIfDirty(sceneData);
            AssetDatabase.Refresh();

            Close();
        }

        private void CreateFolders()
        {
            string[] subfolders = _folder.Split(Path.DirectorySeparatorChar);
            if (subfolders.Length == 0) return;
            string parent = subfolders[0];
            for (int index = 1; index < subfolders.Length; index++)
            {
                var subfolder = subfolders[index];
                string child = Path.Join(parent, subfolder);
                if (!AssetDatabase.IsValidFolder(child)) AssetDatabase.CreateFolder(parent, subfolder);
                parent = child;
            }
        }
    }
}