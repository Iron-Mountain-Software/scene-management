using UnityEngine;
using UnityEngine.UI;

namespace SpellBoundAR.SceneManagement.UI
{
    [RequireComponent(typeof(Button))]
    public abstract class SceneChangeButton : MonoBehaviour
    {
        [Header("Cache")]
        private Button _button;
        
        protected abstract void OnClick();
        
        protected void LoadScene(SceneData sceneData)
        {
            if (!sceneData) return;
            if (SceneManager.Instance) SceneManager.Instance.LoadScene(sceneData);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(sceneData.name);
        }
        
        protected virtual void Awake()
        {
            _button = GetComponent<Button>();
        }

        protected virtual void OnEnable()
        {
            if (_button) _button.onClick.AddListener(OnClick);
        }

        protected virtual void OnDisable()
        {
            if (_button) _button.onClick.RemoveListener(OnClick);
        }
    }
}