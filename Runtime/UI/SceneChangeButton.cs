using UnityEngine;
using UnityEngine.UI;

namespace SpellBoundAR.SceneManagement.UI
{
    [RequireComponent(typeof(Button))]
    public class SceneChangeButton : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private SceneData sceneData;

        [Header("Cache")]
        private Button _button;

        public SceneData SceneData => sceneData;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (_button) _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            if (_button) _button.onClick.RemoveListener(OnClick);
        }

        protected virtual void OnClick() 
        {
            if (!sceneData) return;
            if (SceneManager.Instance) SceneManager.Instance.LoadScene(sceneData);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(sceneData.name);
        }
    }
}