using UnityEngine;

namespace IronMountain.SceneManagement.Packages.Scene_Management.Runtime
{
    public class SceneChanger : MonoBehaviour
    {
        public void LoadScene(SceneData sceneData)
        {
            if (!sceneData) return;
            if (SceneManager.Instance) SceneManager.Instance.LoadScene(sceneData);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(sceneData.name);
        }
    }
}
