using SpellBoundAR.AssetManagement;
using UnityEngine;

namespace SpellBoundAR.SceneManagement
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Core/Scene Management/Database")]
    public class Database : SingletonDatabase<Database>
    {
        [Header("References:")]
        [SerializeField] private SceneData loginScene;
        [SerializeField] private SceneData firstGameScene;
        public SceneData LoginScene => loginScene;
        public SceneData FirstGameScene => firstGameScene;
        
        [SerializeField] private DatabaseTable<SceneData> scenes = new ();
        
        public DatabaseTable<SceneData> Scenes => scenes; 
        
        public SceneData FindSceneByName(string sceneName)
        {
            return Scenes.list.Find(test=> test.name == sceneName);
        }
    }
}