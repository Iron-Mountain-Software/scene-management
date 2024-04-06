using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IronMountain.SceneManagement.Editor
{
    [CustomEditor(typeof(SceneData), true)]
    public class SceneDataInspector : UnityEditor.Editor
    {
        private SceneData _sceneData;

        private void OnEnable()
        {
            _sceneData = target ? (SceneData) target : null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scene"));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("path"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("directory"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buildIndex"));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("screenOrientation"));
            GUILayout.BeginHorizontal();
            SerializedProperty setTimescale = serializedObject.FindProperty("setTimeScale");
            EditorGUILayout.PropertyField(setTimescale);
            EditorGUI.BeginDisabledGroup(!setTimescale.boolValue);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startTimeScale"), GUIContent.none);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dependencies");
            if (GUILayout.Button("Add", GUILayout.Width(40), GUILayout.Height(25)))
            {
                ShowDependencyMenu();
            }
            GUILayout.EndHorizontal();

            foreach (SceneList sceneList in _sceneData.DependencyLists)
            {
                if (!sceneList) continue;
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                foreach (SceneAsset sceneAsset in sceneList.Scenes)
                {
                    DrawDependency(sceneAsset, false);
                }
                GUILayout.EndVertical();
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash"), GUILayout.Width(40), GUILayout.Height(26 * sceneList.Scenes.Count)))
                {
                    ToggleDependencySceneList(sceneList);
                }
                GUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space(5);

            foreach (SceneAsset sceneAsset in _sceneData.DependencyScenes)
            {
                if (!sceneAsset) continue;
                DrawDependency(sceneAsset, true);
            }
            
            EditorGUILayout.Space(5);

            DrawPropertiesExcluding(serializedObject,
                "m_Script",
                "scene",
                "id",
                "path",
                "directory",
                "sceneName",
                "buildIndex",
                "screenOrientation",
                "setTimeScale",
                "startTimeScale",
                "dependencyLists",
                "dependencyScenes",
                "dependencies");
            
            serializedObject.ApplyModifiedProperties();
        }

        private void ShowDependencyMenu()
        {
            GenericMenu menu = new GenericMenu();
                
            if (SceneListsManager.SceneLists is {Count: > 0})
            {
                menu.AddSeparator("Lists");
                foreach (SceneList sceneList in SceneListsManager.SceneLists)
                {
                    if (!sceneList) continue;
                    menu.AddItem(new GUIContent(sceneList.name), _sceneData.DependencyLists.Contains(sceneList), () =>
                    {
                        ToggleDependencySceneList(sceneList);
                    });
                }
            }

            if (SceneDataManager.SceneData is {Count: > 0})
            {
                menu.AddSeparator("Scenes");
                foreach (SceneData sceneData in SceneDataManager.SceneData)
                {
                    if (!sceneData) continue;
                    menu.AddItem(new GUIContent(sceneData.SceneName), _sceneData.Dependencies.Contains(sceneData.SceneName), () =>
                    {
                        ToggleDependencySceneAsset(sceneData.scene);
                    });
                }
            }

            menu.ShowAsContext();
        }

        private void DrawDependency(SceneAsset sceneAsset, bool removable)
        {
            if (!sceneAsset) return;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(sceneAsset.name, GUILayout.Height(25))) EditorGUIUtility.PingObject(sceneAsset);
            if (GUILayout.Button("Load", GUILayout.Width(50), GUILayout.Height(25)))
            {
                if (Application.isPlaying)
                {
                    if (SceneManager.Instance) SceneManager.Instance.LoadSceneByName(sceneAsset.name);
                    else UnityEngine.SceneManagement.SceneManager.LoadScene(sceneAsset.name);
                }
                else EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAsset));
                Selection.activeObject = FindObjectOfType<SceneManager>();
            }
            if (removable && GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash"), GUILayout.Width(40), GUILayout.Height(25)))
            {
                ToggleDependencySceneAsset(sceneAsset);
            }
            GUILayout.EndHorizontal();
        }

        private void ToggleDependencySceneList(SceneList sceneList)
        {
            if (!sceneList) return;
            int index = _sceneData.DependencyLists.IndexOf(sceneList);
            SerializedProperty listProperty = serializedObject.FindProperty("dependencyLists");
            if (index < 0)
            {
                listProperty.InsertArrayElementAtIndex(listProperty.arraySize);
                listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).objectReferenceValue = sceneList;
            }
            else listProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            _sceneData.OnValidate();
        }
        
        private void ToggleDependencySceneAsset(SceneAsset sceneAsset)
        {
            if (!sceneAsset) return;
            int index = _sceneData.DependencyScenes.IndexOf(sceneAsset);
            SerializedProperty listProperty = serializedObject.FindProperty("dependencyScenes");
            if (index < 0)
            {
                listProperty.InsertArrayElementAtIndex(listProperty.arraySize);
                listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).objectReferenceValue = sceneAsset;
            }
            else listProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            _sceneData.OnValidate();
        }
    }
}