using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IronMountain.SceneManagement
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Scene Management/Scene Data")]
    public class SceneData : ScriptableObject
    {
        
#if UNITY_EDITOR
        [SerializeField] public SceneAsset scene;
#endif
        
        [SerializeField] private string id;
        [SerializeField] private string path;
        [SerializeField] private string directory;
        [SerializeField] private string sceneName;
        [SerializeField] private int buildIndex;
        [SerializeField] private ScreenOrientation screenOrientation = ScreenOrientation.Portrait;
        [SerializeField] private bool setTimeScale = true;
        [SerializeField] private float startTimeScale = 1f;
        [SerializeField] private List<SceneList> dependencyLists = new ();
        
#if UNITY_EDITOR
        [SerializeField] private List<SceneAsset> dependencyScenes = new ();
#endif
        
        [SerializeField] private List<string> dependencies = new ();

        public string ID => id;
        public string Path => path;
        public string Directory => directory;
        public virtual string Name => name;
        public virtual string SceneName => sceneName;
        public virtual int BuildIndex => buildIndex;
        public ScreenOrientation ScreenOrientation => screenOrientation;
        public List<SceneList> DependencyLists => dependencyLists;
#if UNITY_EDITOR
        public List<SceneAsset> DependencyScenes => dependencyScenes;
#endif
        public List<string> Dependencies => dependencies;

        public bool DependsOn(Scene scene)
        {
            return dependencies != null && dependencies.Contains(scene.name);
        }

        public void Load(float delay)
        {
            if (SceneManager.Instance) SceneManager.Instance.LoadScene(this, delay);
        }
        
        [ContextMenu("Load This Scene")]
        public void Load()
        {
            if (SceneManager.Instance) SceneManager.Instance.LoadScene(this);
        }

        public virtual void ActivateSettings()
        {
            if (setTimeScale) Time.timeScale = startTimeScale;
        }

        public virtual void OnThisSceneLoaded() { }
        
        public virtual void OnThisSceneUnloaded() { }

        private void OnEnable()
        {
            SceneDataManager.RegisterSceneData(this);
        }

        private void OnDisable()
        {
            SceneDataManager.UnregisterSceneData(this);
        }

#if UNITY_EDITOR
        
        public virtual void Reset()
        {
            GenerateNewID();
        }
        
        public virtual void OnValidate()
        {
            RefreshDependencies();
            path = scene ? AssetDatabase.GetAssetPath(scene) : string.Empty;
            directory = !string.IsNullOrWhiteSpace(path) ? System.IO.Path.GetDirectoryName(path) : string.Empty;
            sceneName = scene ? scene.name : string.Empty;
            buildIndex = -1;
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                if (EditorBuildSettings.scenes[i].path == path) buildIndex = i;
            }
        }

        [ContextMenu("Generate New ID")]
        private void GenerateNewID()
        {
            id = GUID.Generate().ToString();
        }

        [ContextMenu("Refresh Dependencies")]
        private void RefreshDependencies()
        {
            dependencyLists = dependencyLists.Distinct().ToList();
            dependencyScenes = dependencyScenes.Distinct().ToList();
            dependencies.Clear();
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
            foreach (SceneAsset sceneAsset in dependencyScenes)
            {
                if (string.IsNullOrWhiteSpace(sceneAsset.name) 
                    || dependencies.Contains(sceneAsset.name)) continue;
                dependencies.Add(sceneAsset.name);
            }
        }

#endif
        
    }
}