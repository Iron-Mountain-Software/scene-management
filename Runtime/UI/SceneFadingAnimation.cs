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
            gameObject.SetActive(
                SceneManager.Instance.CurrentState == SceneManager.State.FadingOutToLoad ||
                SceneManager.Instance.CurrentState == SceneManager.State.Loading ||
                SceneManager.Instance.CurrentState == SceneManager.State.FadingOutToNew ||
                SceneManager.Instance.CurrentState == SceneManager.State.FadingInToNew
            );
            switch (SceneManager.Instance.CurrentState)
            {
                case SceneManager.State.FadingOutToLoad:
                    _canvasGroup.alpha = 0f;
                    StopAllCoroutines();
                    StartCoroutine(FadeBlack(0, 1, SceneManager.SceneFadeOutSeconds));
                    break;
                case SceneManager.State.Loading:
                    StopAllCoroutines();
                    StartCoroutine(FadeBlack(1, 0, SceneManager.SceneFadeInSeconds));
                    break;
                case SceneManager.State.FadingOutToNew:
                    StopAllCoroutines();
                    StartCoroutine(FadeBlack(0, 1, SceneManager.SceneFadeOutSeconds));
                    break;
                case SceneManager.State.FadingInToNew:
                    _canvasGroup.alpha = 1f;
                    StopAllCoroutines();
                    StartCoroutine(FadeBlack(1, 0, SceneManager.SceneFadeInSeconds));
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