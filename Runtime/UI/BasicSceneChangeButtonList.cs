using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IronMountain.SceneManagement.UI
{
    [ExecuteAlways]
    public class BasicSceneChangeButtonList : MonoBehaviour
    {
        [SerializeField] private bool refreshOnEnable;
        [SerializeField] private SceneListSorts.Type sort;
        [SerializeField] private BasicSceneChangeButton prefab;
        [SerializeField] private Transform parent;
        [SerializeField] private Database database;
        [SerializeField] private List<SceneData> list = new ();

        private void OnValidate()
        {
            if (!parent) parent = transform;
            list ??= new List<SceneData>();
            if (database)
            {
                list.Clear();
                foreach (SceneData sceneData in database.Scenes)
                {
                    list.Add(sceneData);   
                }
            }
            list.Sort(SceneListSorts.GetComparison(sort));
        }
        
        private void OnEnable()
        {
            if (refreshOnEnable) Refresh();
        }

        private void Refresh()
        {
            ClearParent();
            list.Sort(SceneListSorts.GetComparison(sort));
            InstantiateButtons();
        }

        private void ClearParent()
        {
            if (!parent) return;
            if (Application.isPlaying)
            {
                foreach (Transform child in parent)
                {
                    if (!child || !child.gameObject) continue;
                    Destroy(child.gameObject);
                }
            }
#if UNITY_EDITOR
            else
            {
                while (parent.childCount > 0)
                {
                    Transform child = parent.GetChild(0);
                    if (!child || !child.gameObject) continue;
                    DestroyImmediate(child.gameObject);
                }
            }
#endif
        }

        private void InstantiateButtons()
        {
            if (!prefab || !parent || list == null) return;
            foreach (var sceneData in list)
            {
                if (!sceneData) continue;
                if (Application.isPlaying) Instantiate(prefab, parent).SceneData = sceneData;
#if UNITY_EDITOR
                else
                {
                    Object instantiated = PrefabUtility.InstantiatePrefab(prefab, parent);
                    if (instantiated is not BasicSceneChangeButton basicSceneChangeButton) continue;
                    basicSceneChangeButton.SceneData = sceneData;
                }
#endif
            }
        }
    }
}
