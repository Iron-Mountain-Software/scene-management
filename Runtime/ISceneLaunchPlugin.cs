using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpellBoundAR.SceneManagement
{
    public interface ISceneLaunchPlugin
    {
        public int Priority { get; }
        public bool IsReady { get; }
        public SceneData SceneToLaunch { get; }
    }
}
