using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpellBoundAR.SceneManagement
{
    public class SceneLaunchManager : SceneChanger
    {
        [Header("References")]
        [SerializeField] private SceneData defaultScene;
        [SerializeField] private float pluginTimeout = 15;

        [Header("Cache")]
        private float _startTime;

        private bool HasTimedOut => Time.unscaledTime - _startTime > pluginTimeout;
        
        IEnumerator Start()
        {
            _startTime = Time.unscaledTime;
            List<ISceneLaunchPlugin> plugins = GetComponentsInChildren<ISceneLaunchPlugin>().ToList();
            plugins.Sort((pluginA, pluginB) => pluginA.Priority.CompareTo(pluginB.Priority));
            foreach (ISceneLaunchPlugin plugin in plugins)
            {
                if (plugin == null) continue;
                do
                {
                    if (plugin == null) break;
                    if (plugin.IsReady)
                    {
                        if (!plugin.SceneToLaunch) break;
                        LoadScene(plugin.SceneToLaunch);
                        yield break;
                    }
                    yield return null;
                } while (!HasTimedOut);
            }
            LoadScene(defaultScene);
        }
    }
}
