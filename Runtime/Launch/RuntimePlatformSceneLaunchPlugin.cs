using System;
using UnityEngine;

namespace IronMountain.SceneManagement.Launch
{
    public class RuntimePlatformSceneLaunchPlugin : MonoBehaviour, ISceneLaunchPlugin
    {
        public event Action OnStatusMessageChanged;
        
        [SerializeField] private int priority;
        [SerializeField] private SceneData sceneToLaunchAndroid;
        [SerializeField] private SceneData sceneToLaunchIPhonePlayer;
        [SerializeField] private SceneData sceneToLaunchWebGLPlayer;
        [SerializeField] private SceneData sceneToLaunchDefault;
        [SerializeField] private string statusMessage;

        public int Priority => priority;
        public bool IsReady => true;
        public string StatusMessage => statusMessage;
        public SceneData SceneToLaunch
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        return sceneToLaunchAndroid;
                    case RuntimePlatform.IPhonePlayer:
                        return sceneToLaunchIPhonePlayer;
                    case RuntimePlatform.WebGLPlayer:
                        return sceneToLaunchWebGLPlayer;
                    default:
                        return sceneToLaunchDefault;
                }
            }
        }
    }
}