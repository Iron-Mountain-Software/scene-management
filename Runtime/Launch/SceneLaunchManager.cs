using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IronMountain.SceneManagement.Packages.Scene_Management.Runtime.Launch
{
    public class SceneLaunchManager : SceneChanger
    {
        public event Action OnCurrentPluginChanged;
        
        [SerializeField] private bool launchOnStart = true;
        [SerializeField] private SceneData defaultScene;
        [SerializeField] private float pluginTimeout = 15;

        [Header("Cache")]
        private bool _launching;
        private float _startTime;
        private ISceneLaunchPlugin _currentPlugin;

        private bool HasTimedOut => Time.unscaledTime - _startTime > pluginTimeout;

        public ISceneLaunchPlugin CurrentPlugin
        {
            get => _currentPlugin;
            private set
            {
                if (_currentPlugin == value) return;
                _currentPlugin = value;
                OnCurrentPluginChanged?.Invoke();
            }
        }

        public bool Launching => _launching;

        private void Start()
        {
            if (launchOnStart) Launch();
        }

        public void Launch()
        {
            if (_launching) return;
            StartCoroutine(LaunchRunner());
        }

        private void OnDisable()
        {
            _launching = false;
        }

        private IEnumerator LaunchRunner()
        {
            _launching = true;
            _startTime = Time.unscaledTime;
            List<ISceneLaunchPlugin> plugins = GetComponentsInChildren<ISceneLaunchPlugin>().ToList();
            plugins.Sort((pluginA, pluginB) => pluginA.Priority.CompareTo(pluginB.Priority));
            foreach (ISceneLaunchPlugin plugin in plugins)
            {
                if (plugin == null) continue;
                CurrentPlugin = plugin;
                do
                {
                    if (CurrentPlugin == null) break;
                    if (CurrentPlugin.IsReady)
                    {
                        if (!CurrentPlugin.SceneToLaunch) break;
                        LoadScene(CurrentPlugin.SceneToLaunch);
                        _launching = false;
                        yield break;
                    }
                    yield return null;
                } while (!HasTimedOut);
                CurrentPlugin = null;
            }
            LoadScene(defaultScene);
            _launching = false;
        }
    }
}
