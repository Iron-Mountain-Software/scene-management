# Scene Management
Version: 1.4.0
A system for loading scenes, tracking scene dependencies, and storing scene metadata.

## Use Cases:
* Manage scene metadata, such as id, initial screen orientation, and initial Time.scale.
* Load scenes with fade effects (fade old scene out, fade loading scene in, fade loading scene out, fade new scene in) 
* Build loading screens with progress bars and percentage counters.  
* Manage scene dependencies and automatically load them when they're needed. 
* Manage lists of scenes, and apply a list to the build settings.
## Directions for Use:
SceneData
Scriptable Object that stores metadata about a scene.
Must be named exactly the same as the scene it represents.
Create > Scriptable Objects > Scene Management > SceneData
SceneDatabase
Stores all SceneData instances, and is used to rapidly switch between scenes.
Create > Scriptable Objects > Scene Management > Database
SceneList
A customizable list of SceneDatas.
Create > Scriptable Objects > Scene Management > SceneList
SceneManager (singleton)
Attach to a GameObject.
Use this singleton to load scenes.
## Package Mirrors:
[<img src='https://img.itch.zone/aW1nLzEzNzQ2ODg3LnBuZw==/original/npRUfq.png'>](https://github.com/Iron-Mountain-Software/scene-management)[<img src='https://img.itch.zone/aW1nLzEzNzQ2ODkyLnBuZw==/original/Fq0ORM.png'>](https://www.npmjs.com/package/com.iron-mountain.scene-management)[<img src='https://img.itch.zone/aW1nLzEzNzQ2ODk4LnBuZw==/original/Rv4m96.png'>](https://iron-mountain.itch.io/scene-management)
## Key Scripts & Components:
1. public class **Database** : ScriptableObject
   * Properties: 
      * public List<SceneData> ***Scenes***  { get; }
      * public SceneData ***LoginScene***  { get; }
      * public SceneData ***FirstGameScene***  { get; }
   * Methods: 
      * public SceneData ***GetSceneByName***(String sceneName)
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
      * public SceneAsset ***Scene***  { get; }
      * public String ***ID***  { get; }
      * public String ***Path***  { get; }
      * public String ***Name***  { get; }
      * public ScreenOrientation ***ScreenOrientation***  { get; }
      * public float ***StartTimeScale***  { get; }
      * public List<String> ***Dependencies***  { get; }
   * Methods: 
      * public Boolean ***DependsOn***(Scene scene)
      * public virtual void ***ActivateSettings***()
      * public virtual void ***OnThisSceneLoaded***()
      * public virtual void ***OnThisSceneUnloaded***()
      * public virtual void ***Reset***()
1. public class **SceneList** : ScriptableObject
   * Properties: 
      * public List<SceneAsset> ***Scenes***  { get; }
      * public List<String> ***SceneNames***  { get; }
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
      * public void ***LoadSceneByName***(String sceneName)
      * public void ***LoadSceneByID***(String id)
      * public void ***LoadScene***(SceneData scene)
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
   * Properties: 
      * public SceneData ***SceneData***  { get; }
1. public class **CloseApplicationButton** : MonoBehaviour
1. public abstract class **SceneChangeButton** : SceneChanger
1. public class **SceneFadingAnimation** : MonoBehaviour
1. public class **SceneLoadingProgressText** : MonoBehaviour
1. public class **SceneLoadingScreen** : MonoBehaviour
