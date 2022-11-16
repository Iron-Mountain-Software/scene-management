using SpellBoundAR.AssetManagement.Editor;
using UnityEditor;

namespace SpellBoundAR.SceneManagement.Editor
{
    [CustomEditor(typeof(Database), true)]
    public class DatabaseInspector : SingletonDatabaseInspector
    {
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