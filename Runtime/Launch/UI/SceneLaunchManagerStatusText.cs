using UnityEngine;
using UnityEngine.UI;

namespace IronMountain.SceneManagement.Packages.Scene_Management.Runtime.Launch.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Text))]
    public class SceneLaunchManagerStatusText : MonoBehaviour
    {
        [SerializeField] private SceneLaunchManager launchManager;

        [Header("Cache")]
        private Text _text;
        private ISceneLaunchPlugin _currentPlugin;

        private ISceneLaunchPlugin CurrentPlugin
        {
            get => _currentPlugin;
            set
            {
                if (_currentPlugin == value) return;
                if (_currentPlugin != null) _currentPlugin.OnStatusMessageChanged -= RefreshText;
                _currentPlugin = value;
                if (_currentPlugin != null) _currentPlugin.OnStatusMessageChanged += RefreshText;
                RefreshText();
            }
        }

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        private void OnEnable()
        {
            if (!launchManager) launchManager = FindObjectOfType<SceneLaunchManager>();
            if (launchManager) launchManager.OnCurrentPluginChanged += OnLaunchManagerPluginChanged;
            OnLaunchManagerPluginChanged();
        }
        
        private void OnLaunchManagerPluginChanged()
        {
            CurrentPlugin = launchManager ? launchManager.CurrentPlugin : null;
        }
        
        private void OnDisable()
        {
            if (launchManager) launchManager.OnCurrentPluginChanged -= OnLaunchManagerPluginChanged;
            CurrentPlugin = null;
        }

        private void RefreshText()
        {
            if (!_text) return;
            _text.text = _currentPlugin != null ? _currentPlugin.StatusMessage : string.Empty;
        }
    }
}
