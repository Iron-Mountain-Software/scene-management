using System;
using System.Collections.Generic;

namespace IronMountain.SceneManagement
{
    public static class SceneDataManager
    {
        public static event Action OnSceneDataChanged;
        
        public static readonly List<SceneData> SceneData = new();

        public static void RegisterSceneData(SceneData sceneData)
        {
            if (!sceneData || SceneData.Contains(sceneData)) return;
            SceneData.Add(sceneData);
            SceneData.Sort(Compare);
            OnSceneDataChanged?.Invoke();
        }
        
        public static void UnregisterSceneData(SceneData sceneData)
        {
            if (!sceneData || !SceneData.Contains(sceneData)) return;
            SceneData.Remove(sceneData);
            OnSceneDataChanged?.Invoke();
        }

        private static int Compare(SceneData a, SceneData b)
        {
            if (!a) return -1;
            if (!b) return 1;
            return string.Compare(a.SceneName, b.SceneName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
