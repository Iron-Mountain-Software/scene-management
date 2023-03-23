using UnityEngine;

namespace SpellBoundAR.SceneManagement
{
    public class SceneLaunchPlugin : MonoBehaviour, ISceneLaunchPlugin
    {
        [SerializeField] private SceneData sceneToLaunch;
        [SerializeField] private int priority;

        public int Priority => priority;
        public bool IsReady => true;
        public SceneData SceneToLaunch => sceneToLaunch;
    }
}
