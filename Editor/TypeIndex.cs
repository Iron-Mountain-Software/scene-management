using System;
using System.Collections.Generic;
using System.Linq;

namespace IronMountain.SceneManagement.Editor
{
    public static class TypeIndex
    {
        public static readonly List<Type> SceneDataTypes;

        public static string[] SceneDataTypeNames => SceneDataTypes.Select(t => t.Name).ToArray();
        
        static TypeIndex()
        {
            SceneDataTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && (type == typeof(SceneData) || type.IsSubclassOf(typeof(SceneData))))
                .ToList();
        }
    }
}