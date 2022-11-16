using UnityEngine;
using UnityEngine.UI;

namespace SpellBoundAR.SceneManagement.UI
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
            _text.text = Mathf.Round(SceneManager.Instance.Progress * 100f) + "%";
        }
    }
}