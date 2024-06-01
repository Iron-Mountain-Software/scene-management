# Scene Management
*Version: 1.7.1*
## Description: 
A system for loading scenes, tracking scene dependencies, and storing scene metadata.
## Use Cases: 
* Manage scene metadata, such as id, initial screen orientation, and initial Time.scale.
* Load scenes with fade effects (fade old scene out, fade loading scene in, fade loading scene out, fade new scene in) 
* Build loading screens with progress bars and percentage counters.  
* Manage scene dependencies and automatically load them when they're needed. 
* Manage lists of scenes, and apply a list to the build settings.
## Directions for Use: 
1. SceneData
   1. Scriptable Object that stores metadata about a scene.
   1. Must be named exactly the same as the scene it represents.
   1. Create > Scriptable Objects > Scene Management > SceneData
1. SceneDatabase
   1. Stores all SceneData instances, and is used to rapidly switch between scenes.
   1. Create > Scriptable Objects > Scene Management > Database
1. SceneManager (singleton)
   1. Attach to a GameObject.
   1. Use this singleton to load scenes.
1. SceneList
   1. A customizable list of SceneDatas.
   1. Create > Scriptable Objects > Scene Management > SceneList
## Package Mirrors: 
[<img src='https://img.itch.zone/aW1nLzEzNzQ2ODg3LnBuZw==/original/npRUfq.png'>](https://github.com/Iron-Mountain-Software/scene-management)[<img src='https://img.itch.zone/aW1nLzEzNzQ2ODkyLnBuZw==/original/Fq0ORM.png'>](https://www.npmjs.com/package/com.iron-mountain.scene-management)[<img src='https://img.itch.zone/aW1nLzEzNzQ2ODk4LnBuZw==/original/Rv4m96.png'>](https://iron-mountain.itch.io/scene-management)
---
## Key Scripts & Components: 
1. public class **Database** : ScriptableObject
   * Properties: 
      * public List<SceneData> ***Scenes***  { get; }
      * public SceneData ***LoginScene***  { get; }
      * public SceneData ***FirstGameScene***  { get; }
   * Methods: 
      * public SceneData ***GetSceneByName***(String sceneName)
      * public SceneData ***GetSceneByPath***(String scenePath)
      * public SceneData ***GetSceneByID***(String id)
      * public SceneData ***GetRandomScene***()
      * public void ***SortList***()
      * public void ***RebuildDictionary***()
      * public override String ***ToString***()
1. public class **LoadSceneAfterSeconds** : SceneChanger
1. public class **SceneChanger** : MonoBehaviour
   * Methods: 
      * public void ***LoadScene***(SceneData sceneData)
1. public class **SceneData** : ScriptableObject
   * Properties: 
      * public String ***ID***  { get; }
      * public String ***Path***  { get; }
      * public String ***Directory***  { get; }
      * public String ***Name***  { get; }
      * public String ***SceneName***  { get; }
      * public Int32 ***BuildIndex***  { get; }
      * public Boolean ***BuildEnabled***  { get; }
      * public ScreenOrientation ***ScreenOrientation***  { get; }
      * public List<SceneList> ***DependencyLists***  { get; }
      * public List<SceneAsset> ***DependencyScenes***  { get; }
      * public List<String> ***Dependencies***  { get; }
   * Methods: 
      * public void ***CacheBuildDetails***(Int32 index, Boolean enabled)
      * public Boolean ***DependsOn***(Scene scene)
      * public void ***Load***(float delay)
      * public void ***Load***()
      * public virtual void ***ActivateSettings***()
      * public virtual void ***OnThisSceneLoaded***()
      * public virtual void ***OnThisSceneUnloaded***()
      * public virtual void ***Reset***()
      * public virtual void ***OnValidate***()
1. public static class **SceneDataManager**
1. public class **SceneList** : ScriptableObject
   * Properties: 
      * public List<SceneAsset> ***Scenes***  { get; }
      * public List<String> ***SceneNames***  { get; }
1. public static class **SceneListSorts**
1. public static class **SceneListsManager**
1. public class **SceneManager** : MonoBehaviour
   * Properties: 
      * public Database ***SceneDatabase***  { get; }
      * public float ***Progress***  { get; }
      * public float ***GameSceneFadeOutSeconds***  { get; }
      * public float ***LoadingSceneFadeInSeconds***  { get; }
      * public float ***LoadingSceneFadeOutSeconds***  { get; }
      * public float ***GameSceneFadeInSeconds***  { get; }
      * public State ***CurrentState***  { get; }
   * Methods: 
      * public SceneData ***GetSceneByName***(String sceneName)
      * public SceneData ***GetSceneByID***(String id)
      * public SceneData ***GetRandomScene***()
      * public void ***LoadLoginScene***()
      * public void ***LoadSceneByName***(String sceneName, float delay)
      * public void ***LoadSceneByID***(String id, float delay)
      * public void ***LoadScene***(SceneData scene, float delay)
      * public SceneData ***GetActiveSceneData***()
### Launch
1. public interface **ISceneLaunchPlugin**
   * Actions: 
      * public event Action ***OnStatusMessageChanged*** 
   * Properties: 
      * public Int32 ***Priority***  { get; }
      * public Boolean ***IsReady***  { get; }
      * public String ***StatusMessage***  { get; }
      * public SceneData ***SceneToLaunch***  { get; }
1. public class **RuntimePlatformSceneLaunchPlugin** : MonoBehaviour
   * Actions: 
      * public event Action ***OnStatusMessageChanged*** 
   * Properties: 
      * public Int32 ***Priority***  { get; }
      * public Boolean ***IsReady***  { get; }
      * public String ***StatusMessage***  { get; }
      * public SceneData ***SceneToLaunch***  { get; }
1. public class **SceneLaunchManager** : SceneChanger
   * Actions: 
      * public event Action ***OnCurrentPluginChanged*** 
   * Properties: 
      * public ISceneLaunchPlugin ***CurrentPlugin***  { get; }
      * public Boolean ***Launching***  { get; }
   * Methods: 
      * public void ***Launch***()
1. public class **SceneLaunchPlugin** : MonoBehaviour
   * Actions: 
      * public event Action ***OnStatusMessageChanged*** 
   * Properties: 
      * public Int32 ***Priority***  { get; }
      * public Boolean ***IsReady***  { get; }
      * public String ***StatusMessage***  { get; }
      * public SceneData ***SceneToLaunch***  { get; }
### Launch. U I
1. public class **SceneLaunchManagerStatusText** : MonoBehaviour
### U I
1. public class **BasicSceneChangeButton** : SceneChangeButton
   * Actions: 
      * public event Action ***OnSceneDataChanged*** 
   * Properties: 
      * public SceneData ***SceneData***  { get; set; }
1. public class **BasicSceneChangeButtonList** : MonoBehaviour
1. public class **BasicSceneChangeButtonSceneNameLabel** : MonoBehaviour
1. public class **CloseApplicationButton** : MonoBehaviour
1. public abstract class **SceneChangeButton** : SceneChanger
1. public class **SceneFadingAnimation** : MonoBehaviour
1. public class **SceneLoadingProgressText** : MonoBehaviour
1. public class **SceneLoadingScreen** : MonoBehaviour
