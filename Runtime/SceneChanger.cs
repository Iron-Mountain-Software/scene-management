using UnityEngine;

namespace IronMountain.SceneManagement
{
    public class SceneChanger : MonoBehaviour
    {
        public void LoadScene(SceneData sceneData)
        {
            if (!sceneData) return;
            if (SceneManager.Instance) SceneManager.Instance.LoadScene(sceneData);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(sceneData.SceneName);
        }
    }
}
