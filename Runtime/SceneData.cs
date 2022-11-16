using System.Collections.Generic;
using System.Linq;
using SpellBoundAR.AssetManagement;
using UnityEngine;

namespace SpellBoundAR.SceneManagement
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Core/Scene Management/Scene Data")]
    public class SceneData : IdentifiableScriptableObject
    {
        [Header("Settings")]
        [SerializeField] private ScreenOrientation screenOrientation = ScreenOrientation.Portrait;
        [SerializeField] private float startTimeScale = 1f;
        [SerializeField] private List<SceneList> dependencyLists;
        
        public ScreenOrientation ScreenOrientation => screenOrientation;
        public float StartTimeScale => startTimeScale;

        public List<string> Dependencies
        {
            get
            {
                List<string> dependencies = new List<string>();
                foreach (SceneList sceneList in dependencyLists)
                {
                    foreach (string sceneName in sceneList.SceneNames)
                    {
                        if (!dependencies.Contains(sceneName)) dependencies.Add(sceneName);
                    }
                }
                return dependencies;
            }
        }

        public virtual void ActivateSettings()
        {
            Time.timeScale = StartTimeScale;
        }

        public virtual void OnThisSceneLoaded() { }
        
        public virtual void OnThisSceneUnloaded() { }

#if UNITY_EDITOR

        protected virtual void OnValidate()
        {
            PruneDependencies();
        }

        [ContextMenu("Prune Dependencies")]
        private void PruneDependencies()
        {
            dependencyLists = dependencyLists.Distinct().ToList();
            dependencyLists.RemoveAll(dependencySceneAsset => !dependencySceneAsset);
        }

#endif
        
    }
}