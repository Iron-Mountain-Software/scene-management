using System.Collections;
using UnityEngine;

namespace IronMountain.SceneManagement.Packages.Scene_Management.Runtime.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class SceneFadingAnimation : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool showLoadingScreen = true;
        
        private void Awake()
        {
            if (!canvas) canvas = GetComponent<Canvas>();
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnValidate()
        {
            if (!canvas) canvas = GetComponent<Canvas>();
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            SceneManager.OnStateChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            SceneManager.OnStateChanged -= Refresh;
        }

        private void Refresh()
        {
            StopAllCoroutines();
            SceneManager.State state = SceneManager.Instance ? SceneManager.Instance.CurrentState : SceneManager.State.None;
            if (canvas) canvas.enabled = state
                is SceneManager.State.FadingOutToLoad
                or SceneManager.State.Loading
                or SceneManager.State.Activating
                or SceneManager.State.FadingInToNew;
            switch (state)
            {
                case SceneManager.State.FadingOutToLoad:
                    if (canvasGroup) canvasGroup.alpha = 0f;
                    float gameOutSeconds = SceneManager.Instance ? SceneManager.Instance.GameSceneFadeOutSeconds : 0;
                    StartCoroutine(FadeBlack(0, 1, gameOutSeconds));
                    break;
                case SceneManager.State.Loading:
                    if (showLoadingScreen)
                    {
                        float loadingInSeconds = SceneManager.Instance ? SceneManager.Instance.LoadingSceneFadeInSeconds : 0;
                        StartCoroutine(FadeBlack(1, 0, loadingInSeconds)); 
                    }
                    else if (canvasGroup) canvasGroup.alpha = 1f;
                    break;
                case SceneManager.State.Activating:
                    if (showLoadingScreen)
                    {
                        float loadingOutSeconds = SceneManager.Instance ? SceneManager.Instance.LoadingSceneFadeOutSeconds : 0;
                        StartCoroutine(FadeBlack(0, 1, loadingOutSeconds));
                    }
                    else if (canvasGroup) canvasGroup.alpha = 1f;
                    break;
                case SceneManager.State.FadingInToNew:
                    if (canvasGroup) canvasGroup.alpha = 1f;
                    float gameInSeconds = SceneManager.Instance ? SceneManager.Instance.GameSceneFadeInSeconds : 0;
                    StartCoroutine(FadeBlack(1, 0, gameInSeconds));
                    break;
                case SceneManager.State.None: 
                    if (canvasGroup) canvasGroup.alpha = 0f;
                    break;
            }
        }

        private IEnumerator FadeBlack(float startAlpha, float endAlpha, float seconds)
        {
            float progress = Mathf.InverseLerp(startAlpha, endAlpha, canvasGroup.alpha);
            for (float t = seconds * progress; t < seconds; t += Time.unscaledDeltaTime)
            {
                progress = t / seconds;
                if (canvasGroup) canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
                yield return null;
            }
            if (canvasGroup) canvasGroup.alpha = endAlpha;
        }
    }
}