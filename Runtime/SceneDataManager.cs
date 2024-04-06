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
            SceneData.Sort(SceneListSorts.CompareSceneName);
            OnSceneDataChanged?.Invoke();
        }
        
        public static void UnregisterSceneData(SceneData sceneData)
        {
            if (!sceneData || !SceneData.Contains(sceneData)) return;
            SceneData.Remove(sceneData);
            OnSceneDataChanged?.Invoke();
        }
    }
}
