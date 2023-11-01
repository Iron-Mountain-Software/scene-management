using UnityEngine;
using UnityEngine.UI;

namespace IronMountain.SceneManagement.Packages.Scene_Management.Runtime.UI
{
    [RequireComponent(typeof(Button))]
    public abstract class SceneChangeButton : SceneChanger
    {
        [Header("Cache")]
        private Button _button;
        
        protected abstract void OnClick();
        
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