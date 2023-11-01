using UnityEngine;
using UnityEngine.UI;

namespace IronMountain.SceneManagement.UI
{
    [RequireComponent(typeof(Text))]
    public class SceneLoadingProgressText : MonoBehaviour
    {
        private Text _text;
    
        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        private void Update()
        {
            if (!_text) return;
            _text.text = SceneManager.Instance 
                ? Mathf.Round(SceneManager.Instance.Progress * 100f) + "%"
                : "-1%";
        }
    }
}