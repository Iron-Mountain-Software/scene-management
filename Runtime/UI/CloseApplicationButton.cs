using UnityEngine;
using UnityEngine.UI;

namespace SpellBoundAR.SceneManagement.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class CloseApplicationButton : MonoBehaviour
    {
        [SerializeField] private string webGLUrlRedirect = "about:blank";
        
        [Header("Cache")]
        private Button _button;

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

        private void OnClick()
        {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
            Debug.Log(name + " : " + GetType() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name); 
#endif
#if (UNITY_EDITOR)
            UnityEditor.EditorApplication.isPlaying = false;
#elif (UNITY_STANDALONE) 
            Application.Quit();
#elif (UNITY_WEBGL)
            Application.OpenURL(webGLRedirect);
#endif
        }
    }
}