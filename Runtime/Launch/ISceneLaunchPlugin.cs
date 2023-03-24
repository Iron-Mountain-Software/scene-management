using System;

namespace SpellBoundAR.SceneManagement.Launch
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
