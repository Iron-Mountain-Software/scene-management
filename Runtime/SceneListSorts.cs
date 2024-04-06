using System;
using UnityEngine;

namespace IronMountain.SceneManagement
{
    public static class SceneListSorts
    {
        public enum Type
        {
            None,
            Path,
            SceneDataName,
            SceneName,
            BuildIndex
        }

        public static Comparison<SceneData> GetComparison(Type type)
        {
            switch (type)
            {
               case Type.None:
                   return CompareNone;
               case Type.Path:
                   return ComparePath;
               case Type.SceneName:
                   return CompareSceneName;
               case Type.SceneDataName:
                   return CompareSceneDataName;
               case Type.BuildIndex:
                   return CompareBuildIndex;
               default:
                   return CompareSceneName;
            }
        }

        public static int CompareNone(SceneData a, SceneData b) => 0;

        public static int ComparePath(SceneData a, SceneData b)
        {
            if (!a) return -1;
            if (!b) return 1;

            for (int i = 0; i < Mathf.Min(a.Directory.Length, b.Directory.Length); i++)
            {
                int compare = string.CompareOrdinal(a.Directory[i].ToString(), b.Directory[i].ToString());
                if (compare != 0) return compare;
            }

            if (a.Directory.Length > b.Directory.Length) return 1; 
            if (b.Directory.Length > a.Directory.Length) return -1;
            return string.Compare(a.Directory, b.Directory, StringComparison.OrdinalIgnoreCase);
        }
        
        public static int CompareSceneName(SceneData a, SceneData b)
        {
            if (!a) return -1;
            if (!b) return 1;
            return string.Compare(a.SceneName, b.SceneName, StringComparison.OrdinalIgnoreCase);
        }
        
        public static int CompareSceneDataName(SceneData a, SceneData b)
        {
            if (!a) return -1;
            if (!b) return 1;
            return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
        }
        
        public static int CompareBuildIndex(SceneData a, SceneData b)
        {
            if (!a) return -1;
            if (!b) return 1;
            return a.BuildIndex.CompareTo(b.BuildIndex);
        }
    }
}