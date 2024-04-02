using System;
using System.Collections.Generic;

namespace IronMountain.SceneManagement
{
    public static class SceneListsManager
    {
        public static event Action OnSceneListsChanged;
        
        public static readonly List<SceneList> SceneLists = new();

        public static void RegisterSceneList(SceneList sceneList)
        {
            if (!sceneList || SceneLists.Contains(sceneList)) return;
            SceneLists.Add(sceneList);
            SceneLists.Sort(Compare);
            OnSceneListsChanged?.Invoke();
        }
        
        public static void UnregisterSceneList(SceneList sceneList)
        {
            if (!sceneList || !SceneLists.Contains(sceneList)) return;
            SceneLists.Remove(sceneList);
            OnSceneListsChanged?.Invoke();
        }
        
        private static int Compare(SceneList a, SceneList b)
        {
            if (!a) return -1;
            if (!b) return 1;
            return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
        }
    }
}