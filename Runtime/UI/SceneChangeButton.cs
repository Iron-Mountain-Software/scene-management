using UnityEngine;

namespace SpellBoundAR.SceneManagement.UI
{
    public class SceneChangeButton : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private SceneData _sceneData;

        public void OnClick() 
        {
            if (!_sceneData) return;
            if (SceneManager.Instance) SceneManager.Instance.LoadScene(_sceneData);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(_sceneData.name);
        }
    }
}