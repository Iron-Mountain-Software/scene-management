using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IronMountain.SceneManagement
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Scene Management/Scene Data")]
    public class SceneData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private ScreenOrientation screenOrientation = ScreenOrientation.Portrait;
        [SerializeField] private float startTimeScale = 1f;
        [SerializeField] private List<SceneList> dependencyLists;
        
        public string ID => id;
        public virtual string Name => name;
        public ScreenOrientation ScreenOrientation => screenOrientation;
        public float StartTimeScale => startTimeScale;

        public List<string> Dependencies
        {
            get
            {
                List<string> dependencies = new List<string>();
                foreach (SceneList sceneList in dependencyLists)
                {
                    if (!sceneList) continue;
                    foreach (string sceneName in sceneList.SceneNames)
                    {
                        if (string.IsNullOrWhiteSpace(sceneName) 
                            || dependencies.Contains(sceneName)) continue;
                        dependencies.Add(sceneName);
                    }
                }
                return dependencies;
            }
        }

        public bool DependsOn(Scene scene)
        {
            foreach (SceneList sceneList in dependencyLists)
            {
                if (sceneList && sceneList.SceneNames.Contains(scene.name)) return true;
            }
            return false;
        }

        public virtual void ActivateSettings()
        {
            Time.timeScale = StartTimeScale;
        }

        public virtual void OnThisSceneLoaded() { }
        
        public virtual void OnThisSceneUnloaded() { }

#if UNITY_EDITOR
        
        public virtual void Reset()
        {
            GenerateNewID();
        }
        
        protected virtual void OnValidate()
        {
            PruneDependencies();
        }
        
        [ContextMenu("Generate New ID")]
        private void GenerateNewID()
        {
            id = UnityEditor.GUID.Generate().ToString();
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