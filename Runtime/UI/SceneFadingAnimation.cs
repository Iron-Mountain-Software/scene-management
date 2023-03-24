using System.Collections;
using UnityEngine;

namespace SpellBoundAR.SceneManagement.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SceneFadingAnimation : MonoBehaviour
    {

        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            SceneManager.OnStateChanged += RefreshState;
        }

        private void OnDestroy()
        {
            SceneManager.OnStateChanged -= RefreshState;
        }

        private void Start()
        {
            RefreshState();
        }

        private void RefreshState()
        {
            bool active = SceneManager.Instance
                          && SceneManager.Instance.CurrentState
                              is SceneManager.State.FadingOutToLoad
                              or SceneManager.State.Loading
                              or SceneManager.State.FadingOutToNew
                              or SceneManager.State.FadingInToNew;
            gameObject.SetActive(active);
            if (!active) return;
            switch (SceneManager.Instance.CurrentState)
            {
                case SceneManager.State.FadingOutToLoad:
                    _canvasGroup.alpha = 0f;
                    StopAllCoroutines();
                    StartCoroutine(FadeBlack(0, 1, SceneManager.Instance.SceneFadeOutSeconds));
                    break;
                case SceneManager.State.Loading:
                    StopAllCoroutines();
                    StartCoroutine(FadeBlack(1, 0, SceneManager.Instance.SceneFadeInSeconds));
                    break;
                case SceneManager.State.FadingOutToNew:
                    StopAllCoroutines();
                    StartCoroutine(FadeBlack(0, 1, SceneManager.Instance.SceneFadeOutSeconds));
                    break;
                case SceneManager.State.FadingInToNew:
                    _canvasGroup.alpha = 1f;
                    StopAllCoroutines();
                    StartCoroutine(FadeBlack(1, 0, SceneManager.Instance.SceneFadeInSeconds));
                    break;
            }
        }

        private IEnumerator FadeBlack(float startAlpha, float endAlpha, float seconds)
        {
            float progress = Mathf.InverseLerp(startAlpha, endAlpha, _canvasGroup.alpha);
            for (float t = seconds * progress; t < seconds; t += Time.unscaledDeltaTime)
            {
                progress = t / seconds;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
                yield return null;
            }
            _canvasGroup.alpha = endAlpha;
        }
    }
}