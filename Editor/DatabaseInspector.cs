using SpellBoundAR.AssetManagement.Editor;
using UnityEditor;
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
                    if (SceneManager.Instance) SceneManager.Instance.LoadScene(sceneData);
                    else UnityEngine.SceneManagement.SceneManager.LoadScene(sceneData.name);
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