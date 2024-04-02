using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IronMountain.SceneManagement
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Scene Management/Scene List")]
    public class SceneList : ScriptableObject
    {
    
#if UNITY_EDITOR
        
        [SerializeField] private List<SceneAsset> scenes;
        public List<SceneAsset> Scenes => scenes;
        
#endif
        
        [SerializeField] private List<string> sceneNames = new ();

        public List<string> SceneNames => sceneNames;

        private void OnEnable()
        {
            SceneListsManager.RegisterSceneList(this);
        }

        private void OnDisable()
        {
            SceneListsManager.UnregisterSceneList(this);
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            PruneScenes();
        }

        [ContextMenu("Prune Dependencies")]
        private void PruneScenes()
        {
            scenes = scenes.Distinct().ToList();
            sceneNames.Clear();
            foreach (SceneAsset sceneAsset in scenes) sceneNames.Add(sceneAsset.name);
        }

#endif

    }
}