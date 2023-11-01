using UnityEngine;

namespace IronMountain.SceneManagement.Packages.Scene_Management.Runtime.UI
{
    public class BasicSceneChangeButton : SceneChangeButton
    {
        [Header("Settings")]
        [SerializeField] private SceneData sceneData;

        public SceneData SceneData => sceneData;

        protected override void OnClick() => LoadScene(sceneData);
    }
}