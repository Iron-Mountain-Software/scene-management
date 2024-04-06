using System;
using UnityEngine;

namespace IronMountain.SceneManagement.UI
{
    public class BasicSceneChangeButton : SceneChangeButton
    {
        public event Action OnSceneDataChanged;
        
        [Header("Settings")]
        [SerializeField] private SceneData sceneData;

        public SceneData SceneData
        {
            get => sceneData;
            set
            {
                if (sceneData == value) return;
                sceneData = value;
                OnSceneDataChanged?.Invoke();
            }
        }

        protected override void OnClick() => LoadScene(sceneData);
    }
}