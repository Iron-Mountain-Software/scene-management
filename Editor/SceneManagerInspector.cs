using IronMountain.SceneManagement.Packages.Scene_Management.Runtime;
using UnityEditor;

namespace IronMountain.SceneManagement.Packages.Scene_Management.Editor
{
    [CustomEditor(typeof(SceneManager), true)]
    public class SceneManagerInspector : UnityEditor.Editor
    {
        private SceneManager _sceneManager;
        private UnityEditor.Editor _sceneManagerInspector;

        private void OnEnable()
        {
            _sceneManager = (SceneManager) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (_sceneManager && _sceneManager.SceneDatabase)
            {
                CreateCachedEditor(_sceneManager.SceneDatabase, null, ref _sceneManagerInspector);
                _sceneManagerInspector.OnInspectorGUI();
            }
            else _sceneManagerInspector = null;
        }
    }
}