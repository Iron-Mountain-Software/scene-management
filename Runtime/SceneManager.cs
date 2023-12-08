using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace IronMountain.SceneManagement
{
    [ExecuteAlways]
    public class SceneManager : MonoBehaviour
    {
        public enum State
        {
            None,
            FadingOutToLoad,
            Loading,
            Activating,
            FadingInToNew
        }

        public static SceneManager Instance { get; private set; }
        
        public static event Action OnStateChanged;
        
        private static readonly string TemporaryScene = "Temporary Scene: ";

        [SerializeField] private Database sceneDatabase;
        [SerializeField] private float gameSceneFadeOutSeconds;
        [SerializeField] private float loadingSceneFadeInSeconds;
        [SerializeField] private float loadingSceneFadeOutSeconds;
        [SerializeField] private float gameSceneFadeInSeconds;

        [Header("Cache")]
        private State _state = State.None;
        private Scene _awakeScene;
        private SceneData _awakeSceneData;
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
        
        public SceneData GetSceneByName(string sceneName)
        {
            return sceneDatabase ? sceneDatabase.GetSceneByName(sceneName) : null;
        }
        
        public SceneData GetSceneByID(string id)
        {
            return sceneDatabase ? sceneDatabase.GetSceneByID(id) : null;
        }
        
        public SceneData GetRandomScene()
        {
            return sceneDatabase ? sceneDatabase.GetRandomScene() : null;
        }
        
        public void LoadLoginScene()
        {
            if (!sceneDatabase || !sceneDatabase.LoginScene) return;
            LoadScene(sceneDatabase.LoginScene);
        }

        public void LoadSceneByName(string sceneName, float delay = 0)
        {
            SceneData sceneData = sceneDatabase ? sceneDatabase.GetSceneByName(sceneName) : null;
            if (sceneData == null) sceneData = sceneDatabase ? sceneDatabase.FirstGameScene : null;
            LoadScene(sceneData);
        }
        
        public void LoadSceneByID(string id, float delay = 0)
        {
            SceneData sceneData = sceneDatabase ? sceneDatabase.GetSceneByID(id) : null;
            if (sceneData == null) sceneData = sceneDatabase ? sceneDatabase.FirstGameScene : null;
            LoadScene(sceneData);
        }
        
        public void LoadScene(SceneData scene, float delay = 0)
        {
            if (!scene || CurrentState is State.Loading or State.Activating) return;
            StopAllCoroutines();
            StartCoroutine(LoadSceneRunner(scene, delay));
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying) Destroy(gameObject);
            }
            else
            {
                Instance = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
                UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
#if UNITY_EDITOR
                EditorSceneManager.sceneOpened += OnSceneOpenedInEditMode;
#endif
                _awakeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                _awakeSceneData = sceneDatabase.GetSceneByName(_awakeScene.name);
                if (_awakeSceneData)
                {
                    if (Application.isPlaying) _awakeSceneData.ActivateSettings();
                    LoadDependencies(_awakeSceneData);
                }
            }
        }

        private void LoadDependencies(SceneData sceneData)
        {
            if (!sceneData) return;
            
            List<string> loadedScenes = new();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                loadedScenes.Add(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name);
            }

            foreach (string dependency in sceneData.Dependencies)
            {
                if (loadedScenes.Contains(dependency)) continue;
                if (Application.isPlaying)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(dependency, LoadSceneMode.Additive);
                }
#if UNITY_EDITOR
                else
                {
                    SceneData dependencyData = SceneDatabase.GetSceneByName(dependency);
                    if (dependencyData) EditorSceneManager.OpenScene(dependencyData.Path, OpenSceneMode.Additive);
                }
#endif
            }
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
#if UNITY_EDITOR
            EditorSceneManager.sceneOpened -= OnSceneOpenedInEditMode;
#endif
        }

#if UNITY_EDITOR
        private void OnSceneOpenedInEditMode(Scene loadedScene, OpenSceneMode openSceneMode)
        {
            if (_awakeScene.isLoaded) EditorSceneManager.SetActiveScene(_awakeScene);
        }
#endif

        private void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            SceneData loadedSceneData = sceneDatabase.GetSceneByName(loadedScene.name);
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
            SceneData loadedSceneData = sceneDatabase.GetSceneByName(unloadedScene.name);
            if (!loadedSceneData) return;
            loadedSceneData.OnThisSceneUnloaded();
        }

        private IEnumerator LoadSceneRunner(SceneData sceneDataToLoad, float delay)
        {
            if (delay > 0) yield return new WaitForSecondsRealtime(delay);
            
            CurrentState = State.FadingOutToLoad;
            
            if (gameSceneFadeOutSeconds > 0) yield return new WaitForSecondsRealtime(gameSceneFadeOutSeconds);

            CurrentState = State.Loading;

            _destinationScene = sceneDataToLoad;
            _destinationScene.ActivateSettings();
            
            Screen.orientation = ScreenOrientation.Portrait;
            
            bool destinationSceneAlreadyLoaded = false;
            List<string> scenesKeptLoaded = new ();
            List<string> scenesToUnload = new ();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (_destinationScene.name.Equals(scene.name))
                {
                    destinationSceneAlreadyLoaded = true;
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
                    UnloadAllTemporaryScenes();
                }
                else if (_destinationScene.DependsOn(scene))
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
                if (string.IsNullOrWhiteSpace(sceneToUnload)) continue;
                AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneToUnload);
                sceneUnloadingOperations.Add(operation);
            }

            if (!destinationSceneAlreadyLoaded)
            {
                AsyncOperation mainOperation =
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_destinationScene.name, LoadSceneMode.Additive);
                mainOperation.allowSceneActivation = false;
                sceneLoadingOperations.Add(mainOperation);
            }

            foreach (string dependency in _destinationScene.Dependencies)
            {
                if (string.IsNullOrWhiteSpace(dependency)) continue;
                if (dependency == _destinationScene.Name) continue;
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
                int processes = 0;
                for (int i = 0; i < sceneUnloadingOperations.Count; i++)
                {
                    if (sceneUnloadingOperations[i] == null) continue;
                    progressCounter += sceneUnloadingOperations[i].progress;
                    processes++;
                }
                for (int i = 0; i < sceneLoadingOperations.Count; i++)
                {
                    if (sceneLoadingOperations[i] == null) continue;
                    progressCounter += sceneLoadingOperations[i].progress;
                    processes++;
                }
                float unscaledProgress = progressCounter / processes;
                Progress = Mathf.Clamp01(unscaledProgress / .9f);
                yield return null;
            }
            
            CurrentState = State.Activating;
            
            if (loadingSceneFadeOutSeconds > 0) yield return new WaitForSecondsRealtime(loadingSceneFadeOutSeconds);

            Screen.orientation = _destinationScene.ScreenOrientation;
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