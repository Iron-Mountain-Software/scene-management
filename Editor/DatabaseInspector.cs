using System.Diagnostics.Eventing.Reader;
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
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            foreach (SceneData sceneData in ((Database)target).Scenes.list)
            {
                if (!sceneData) continue;
                if (GUILayout.Button("Switch to " + sceneData.Name, GUILayout.MinHeight(30)))
                {
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
                }
            }
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