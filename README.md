# Scene Management

A system for loading scenes, tracking scene dependencies, and storing scene metadata.

EASY SET UP!

---

### Use this package to:

* Manage scene metadata, such as id, initial screen orientation, and initial Time.scale.
* Load scenes with fade effects (fade old scene out, fade loading scene in, fade loading scene out, fade new scene in)
* Manage scene dependencies and automatically load them when they're needed.
* Build loading screens with progress bars and percentage counters.
* Manage lists of scenes, and apply a list to the build settings.

---

### Key components:

1. SceneData
	* Scriptable Object that stores metadata about a scene.
	* Must be named exactly the same as the scene it represents.
	* Create > Scriptable Objects > Scene Management > SceneData
2. SceneDatabase
	* Stores all SceneData instances, and is used to rapidly switch between scenes.
	* Create > Scriptable Objects > Scene Management > Database
3. SceneList
	* A customizable list of SceneDatas.
	* Create > Scriptable Objects > Scene Management > SceneList
4. SceneManager (singleton)
	* Attach to a GameObject.
	* Use this singleton to load scenes.