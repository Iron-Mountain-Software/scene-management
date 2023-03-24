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

        [Header("References")]
        [SerializeField] private Database sceneDatabase;
        [SerializeField] private float gameSceneFadeOutSeconds;
        [SerializeField] private float loadingSceneFadeInSeconds;
        [SerializeField] private float loadingSceneFadeOutSeconds;
        [SerializeField] private float gameSceneFadeInSeconds;

        [Header("Cache")]
        private State _state = State.None;
        private SceneData _destinationScene;

        public Database SceneDatabase => sceneDatabase;
        public float Progress { get; private set; }
        public float GameSceneFadeOutSeconds => gameSceneFadeOutSeconds;
        public float LoadingSceneFadeInSeconds => loadingSceneFadeInSeconds;
        public float LoadingSceneFadeOutSeconds => loadingSceneFadeOutSeconds;
        public float GameSceneFadeInSeconds => gameSceneFadeInSeconds;

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
            if (_destinationScene && _destinationScene.Name == loadedScene.name)
            {
                _destinationScene = null;
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
            LoadScene(sceneDatabase.LoginScene);
        }
        
        public void LoadSceneByName(string sceneName)
        {
            SceneData sceneData = sceneDatabase.FindSceneByName(sceneName);
            LoadScene(sceneData ? sceneData : sceneDatabase.FirstGameScene);
        }
        
        public void LoadSceneByID(string id)
        {
            SceneData sceneData = sceneDatabase.Scenes.GetElementByID(id);
            LoadScene(sceneData ? sceneData : sceneDatabase.FirstGameScene);
        }
        
        public void LoadScene(SceneData scene)
        {
            if (CurrentState != State.None) return;
            if (!scene) return;
            StopAllCoroutines();
            StartCoroutine(LoadSceneRunner(scene));
        }
        
        private IEnumerator LoadSceneRunner(SceneData sceneDataToLoad)
        {
            float startTime = Time.unscaledTime;
            _destinationScene = sceneDataToLoad;
            CurrentState = State.FadingOutToLoad;
            
            if (gameSceneFadeOutSeconds > 0) yield return new WaitForSecondsRealtime(gameSceneFadeOutSeconds);
            
            Screen.orientation = ScreenOrientation.Portrait;
            sceneDataToLoad.ActivateSettings();
            CurrentState = State.Loading;

            bool destinationSceneAlreadyLoaded = false;
            List<string> scenesKeptLoaded = new ();
            List<string> scenesToUnload = new ();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (sceneDataToLoad.name.Equals(scene.name))
                {
                    destinationSceneAlreadyLoaded = true;
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
                    UnloadAllTemporaryScenes();
                }
                else if (sceneDataToLoad.DependsOn(scene))
                {
                    scenesKeptLoaded.Add(scene.name);
                }
                else scenesToUnload.Add(scene.name);
            }
            
            List<AsyncOperation> sceneLoadingOperations = new ();

            if (!destinationSceneAlreadyLoaded)
            {
                CreateEmptyTemporaryScene();
            }
            
            List<AsyncOperation> sceneUnloadingOperations = new ();
            foreach (string sceneToUnload in scenesToUnload)
            {
                AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneToUnload);
                sceneUnloadingOperations.Add(operation);
            }

            if (!destinationSceneAlreadyLoaded)
            {
                AsyncOperation mainOperation =
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneDataToLoad.name, LoadSceneMode.Additive);
                mainOperation.allowSceneActivation = false;
                sceneLoadingOperations.Add(mainOperation);
            }

            foreach (string dependency in sceneDataToLoad.Dependencies)
            {
                if (dependency == sceneDataToLoad.Name) continue;
                if (scenesKeptLoaded.Contains(dependency)) continue;
                AsyncOperation operation =
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(dependency, LoadSceneMode.Additive);
                operation.allowSceneActivation = false;
                sceneLoadingOperations.Add(operation);
            }
            
            Progress = 0;
            while (Progress < 1)
            {
                float progressCounter = 0f;
                for (int i = 0; i < sceneUnloadingOperations.Count; i++) progressCounter += sceneUnloadingOperations[i].progress;
                for (int i = 0; i < sceneLoadingOperations.Count; i++) progressCounter += sceneLoadingOperations[i].progress;
                float unscaledProgress = progressCounter / (sceneUnloadingOperations.Count + sceneLoadingOperations.Count);
                Progress = Mathf.Clamp01(unscaledProgress / .9f);
                yield return null;
            }
            
            CurrentState = State.FadingOutToNew;
            
            if (loadingSceneFadeOutSeconds > 0) yield return new WaitForSecondsRealtime(loadingSceneFadeOutSeconds);

            Screen.orientation = sceneDataToLoad.ScreenOrientation;
            foreach (AsyncOperation operation in sceneLoadingOperations)
            {
                operation.allowSceneActivation = true;
            }
            
            CurrentState = State.FadingInToNew;
            
            if (gameSceneFadeInSeconds > 0) yield return new WaitForSecondsRealtime(gameSceneFadeInSeconds);
            
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
            if (gameSceneFadeOutSeconds < 0) gameSceneFadeOutSeconds = 0;
            if (loadingSceneFadeInSeconds < 0) loadingSceneFadeInSeconds = 0;
            if (loadingSceneFadeOutSeconds < 0) loadingSceneFadeOutSeconds = 0;
            if (gameSceneFadeInSeconds < 0) gameSceneFadeInSeconds = 0;
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