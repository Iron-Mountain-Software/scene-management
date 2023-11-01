using System;
using UnityEngine;

namespace IronMountain.SceneManagement.Packages.Scene_Management.Runtime.Launch
{
    public class SceneLaunchPlugin : MonoBehaviour, ISceneLaunchPlugin
    {
        public event Action OnStatusMessageChanged;

        [SerializeField] private int priority;
        [SerializeField] private SceneData sceneToLaunch;
        [SerializeField] private string message;

        public int Priority => priority;
        public bool IsReady => true;
        public string StatusMessage => message;
        public SceneData SceneToLaunch => sceneToLaunch;
    }
}