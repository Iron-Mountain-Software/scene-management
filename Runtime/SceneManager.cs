using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpellBoundAR.SceneManagement
{
    public class SceneManager : MonoBehaviour
    {
        public enum State
        {
            None,
            FadingOutToLoad,
            Loading,
            FadingOutToNew,
            FadingInToNew
        }

        public static SceneManager Instance { get; private set; }
        
        public static event Action OnStateChanged;
        
        [Header("Static Settings")]
        private static readonly string TemporaryScene = "Temporary Scene: ";
        public static readonly float SceneFadeInSeconds = .5f;
        public static readonly float SceneFadeOutSeconds = .5f;

        private static readonly WaitForSecondsRealtime FadeIn = new (SceneFadeInSeconds);
        private static readonly WaitForSecondsRealtime FadeOut = new (SceneFadeOutSeconds);
        
        [Header("References")]
        [SerializeField] private Database sceneDatabase;
        
        [Header("Cache")]
        private State _state = State.None;
        private readonly List<string> _dependenciesKeptLoaded = new ();
        private readonly List<AsyncOperation> _sceneLoadingOperations = new ();
        private readonly List<AsyncOperation> _sceneUnloadingOperations = new ();

        public Database SceneDatabase => sceneDatabase;
        public float Progress { get; private set; }

        public State CurrentState
        {
            get => _state;
            private set
            {
                if (_state == value) return;
                _state = value;
                OnStateChanged?.Invoke();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
                UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
                Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                SceneData currentSceneData = sceneDatabase.FindSceneByName(activeScene.name);
                if (currentSceneData)
                {
                    currentSceneData.ActivateSettings();
                    foreach (string dependency in currentSceneData.Dependencies)
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(dependency, LoadSceneMode.Additive);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            SceneData loadedSceneData = sceneDatabase.FindSceneByName(loadedScene.name);
            if (!loadedSceneData) return;
            loadedSceneData.OnThisSceneLoaded();
            Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (loadedScene != activeScene)
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(loadedScene);
                UnloadAllTemporaryScenes();
            }
        }
        
        private void OnSceneUnloaded(Scene unloadedScene)
        {
            SceneData loadedSceneData = sceneDatabase.FindSceneByName(unloadedScene.name);
            if (!loadedSceneData) return;
            loadedSceneData.OnThisSceneUnloaded();
        }

        public void LoadLoginScene()
        {
            StopAllCoroutines();
            StartCoroutine(LoadSceneRunner(sceneDatabase.LoginScene));
        }

        public void LoadScene(SceneData scene)
        {
            if (!scene) return;
            StopAllCoroutines();
            StartCoroutine(LoadSceneRunner(scene));
        }

        public void LoadSceneByName(string sceneName)
        {
            StopAllCoroutines();
            SceneData sceneData = sceneDatabase.FindSceneByName(sceneName);
            StartCoroutine(LoadSceneRunner(sceneData ? sceneData : sceneDatabase.FirstGameScene));
        }
        
        public void LoadSceneByID(string id)
        {
            StopAllCoroutines();
            SceneData sceneData = sceneDatabase.Scenes.GetElementByID(id);
            StartCoroutine(LoadSceneRunner(sceneData ? sceneData : sceneDatabase.FirstGameScene));
        }

        private IEnumerator LoadSceneRunner(SceneData sceneDataToLoad)
        {
            CurrentState = State.FadingOutToLoad;
            yield return FadeOut;
            Screen.orientation = ScreenOrientation.Portrait;
            sceneDataToLoad.ActivateSettings();
            CurrentState = State.Loading;
            CreateEmptyTemporaryScene();
            _dependenciesKeptLoaded.Clear();
            _sceneLoadingOperations.Clear();
            _sceneUnloadingOperations.Clear();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                bool isDependency = sceneDataToLoad.Dependencies.Find(test => test == scene.name) != null;
                if (isDependency)
                {
                    _dependenciesKeptLoaded.Add(scene.name);
                }
                else if (!scene.name.Contains(TemporaryScene))
                {
                    AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene.name);
                    _sceneUnloadingOperations.Add(operation);
                }
            }
            AsyncOperation mainOperation =
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneDataToLoad.name, LoadSceneMode.Additive);
            mainOperation.allowSceneActivation = false;
            _sceneLoadingOperations.Add(mainOperation);
            foreach (string dependency in sceneDataToLoad.Dependencies)
            {
                if (_dependenciesKeptLoaded.Contains(dependency)) continue;
                AsyncOperation operation =
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(dependency, LoadSceneMode.Additive);
                operation.allowSceneActivation = false;
                _sceneLoadingOperations.Add(operation);
            }
            Progress = 0;
            while (Progress < 1)
            {
                float progressCounter = 0f;
                for (int i = 0; i < _sceneUnloadingOperations.Count; i++) progressCounter += _sceneUnloadingOperations[i].progress;
                for (int i = 0; i < _sceneLoadingOperations.Count; i++) progressCounter += _sceneLoadingOperations[i].progress;
                float unscaledProgress = progressCounter / (_sceneUnloadingOperations.Count + _sceneLoadingOperations.Count);
                Progress = Mathf.Clamp01(unscaledProgress / .9f);
                yield return null;
            }
            CurrentState = State.FadingOutToNew;
            yield return FadeOut;
            Screen.orientation = sceneDataToLoad.ScreenOrientation;
            foreach (AsyncOperation operation in _sceneLoadingOperations)
            {
                operation.allowSceneActivation = true;
            }
            CurrentState = State.FadingInToNew;
            yield return FadeIn;
            CurrentState = State.None;
        }

        private void CreateEmptyTemporaryScene()
        {
            Scene temporaryScene = UnityEngine.SceneManagement.SceneManager.CreateScene(TemporaryScene + Time.time);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(temporaryScene);
        }

        private void UnloadAllTemporaryScenes()
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene testScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (testScene.name.Contains(TemporaryScene))
                {
                    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(testScene.name);
                }
            }
        }

#if UNITY_EDITOR

        private void Reset()
        {
            RefreshSceneDatabase();
        }

        private void OnValidate()
        {
            if (!sceneDatabase) RefreshSceneDatabase();
        }

        [ContextMenu("Refresh Scene Database")]
        private void RefreshSceneDatabase()
        {
            Database[] databases = Resources.LoadAll<Database>("");
            if (databases == null || databases.Length == 0)
            {
                throw new Exception("Could not find any Scene Database Scriptable Objects in the Resources.");
            }
            if (databases.Length > 1)
            {
                Debug.LogWarning("Multiple instances of Scene Database exist in the Resources.");
            }
            sceneDatabase = databases[0];
        }

#endif
        
    }
}