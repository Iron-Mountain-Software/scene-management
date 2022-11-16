using UnityEngine;

namespace SpellBoundAR.SceneManagement.UI
{
    public class SceneLoadingScreen : MonoBehaviour
    {
        private void Awake() => SceneManager.OnStateChanged += RefreshState;
        private void OnDestroy() => SceneManager.OnStateChanged -= RefreshState;

        private void Start() => RefreshState();

        private void RefreshState()
        {
            gameObject.SetActive(
                SceneManager.Instance.CurrentState is SceneManager.State.Loading or SceneManager.State.FadingOutToNew
            );
        }
    }
}