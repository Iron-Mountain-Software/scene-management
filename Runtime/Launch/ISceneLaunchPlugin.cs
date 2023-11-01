using System;

namespace IronMountain.SceneManagement.Packages.Scene_Management.Runtime.Launch
{
    public interface ISceneLaunchPlugin
    {
        public event Action OnStatusMessageChanged;
        public int Priority { get; }
        public bool IsReady { get; }
        public string StatusMessage { get; }
        public SceneData SceneToLaunch { get; }
    }
}
