using UnityEngine;
using UnityEngine.UI;

namespace IronMountain.SceneManagement.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(Text))]
    public class BasicSceneChangeButtonSceneNameLabel : MonoBehaviour
    {
        [SerializeField] private BasicSceneChangeButton basicSceneChangeButton;
        [SerializeField] private Text text;

        private void OnValidate()
        {
            if (!text) text = GetComponent<Text>();
            if (!basicSceneChangeButton) basicSceneChangeButton = GetComponentInParent<BasicSceneChangeButton>();
        }
        
        private void Awake() => OnValidate();

        private void OnEnable()
        {
            if (basicSceneChangeButton) basicSceneChangeButton.OnSceneDataChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (basicSceneChangeButton) basicSceneChangeButton.OnSceneDataChanged -= Refresh;
        }

        private void Refresh()
        {
            if (!text) return;
            text.text = basicSceneChangeButton && basicSceneChangeButton.SceneData
                ? basicSceneChangeButton.SceneData.SceneName
                : string.Empty;
        }
    }
}
