using UnityEngine;

namespace IronMountain.SceneManagement.Packages.Scene_Management.Runtime
{
    public class LoadSceneAfterSeconds : SceneChanger
    {
        [SerializeField] private SceneData scene;
        [SerializeField] private float seconds;

        [Header("Cache")]
        private float _startTime = float.MaxValue;
        
        private void OnEnable()
        {
            _startTime = Time.time;
        }

        private void Update()
        {
            if (Time.time - _startTime > seconds)
            {
                LoadScene(scene);
                enabled = false;
            }
        }

        private void OnDisable()
        {
            _startTime = float.MaxValue;
        }
    }
}