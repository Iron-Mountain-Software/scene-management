using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IronMountain.SceneManagement
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Scene Management/Database")]
    public class Database : ScriptableObject
    {
        [SerializeField] private SceneData loginScene;
        [SerializeField] private SceneData firstGameScene;
        [SerializeField] private List<SceneData> scenes = new ();

        private Dictionary<string, SceneData> _dictionary;

        public List<SceneData> Scenes => scenes; 
        public SceneData LoginScene => loginScene;
        public SceneData FirstGameScene => firstGameScene;

        public SceneData GetSceneByName(string sceneName)
        {
            return scenes.Find(test=> test && test.SceneName == sceneName);
        }
        
        public SceneData GetSceneByPath(string scenePath)
        {
            return scenes.Find(test=> test && test.Path == scenePath);
        }
        
        public SceneData GetSceneByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            if (_dictionary == null) RebuildDictionary();
            return _dictionary.ContainsKey(id) ? _dictionary[id] : null;
        }

        public SceneData GetRandomScene()
        {
            if (_dictionary == null) RebuildDictionary();
            return _dictionary.Count > 0
                ? _dictionary.ElementAt(Random.Range(0, _dictionary.Count)).Value
                : null;
        }

        public void SortList()
        {
            scenes.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));   
        }

        public void RebuildDictionary()
        {
            if (_dictionary != null) _dictionary.Clear();
            else _dictionary = new Dictionary<string, SceneData>();
            int failures = 0;
            foreach (SceneData scene in scenes)
            {
                try  
                {
                    if (!scene) throw new Exception("Null Scene");
                    if (string.IsNullOrWhiteSpace(scene.ID)) throw new Exception("Scene Data with empty key: " + scene.name);
                    if (_dictionary.ContainsKey(scene.ID)) throw new Exception("Scene Data with duplicate keys: " + scene.name + ", " + _dictionary[scene.ID].name);
                    _dictionary.Add(scene.ID, scene);
                }  
                catch (Exception exception)  
                {  
                    failures++;
                    if (scene) Debug.LogError(exception.Message, scene);
                    else Debug.LogError(exception.Message);
                }
            }
            Debug.Log("Rebuilt Dictionary: " + _dictionary.Count + " Successes, " + failures + " Failures");
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("<SCENES>\n");
            foreach (SceneData scene in scenes)
            {
                result.Append(scene.ID);
                result.Append("\t");
                result.Append(scene.name);
                result.Append("\n");
            }
            result.Append("</SCENES>\n");
            return result.ToString();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            scenes.RemoveAll(test => !test);
            scenes = scenes.Distinct().ToList();
        }

#endif
        
    }
}