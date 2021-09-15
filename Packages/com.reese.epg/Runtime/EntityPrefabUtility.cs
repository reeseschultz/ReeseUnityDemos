using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reese.EntityPrefabGroups
{
    internal static class EntityPrefabUtility
    {
        internal static readonly string PACKAGE_DIRECTORY_NAME = "~EntityPrefabGroups";

        internal static List<EntityPrefabGroup> GetEntityPrefabGroups()
        {
            var gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));

            var groups = new List<EntityPrefabGroup>();
            foreach (GameObject go in gameObjects)
            {
                var group = go.GetComponent<EntityPrefabGroup>();

                if (group == null) continue;

                groups.Add(group);
            }

            return groups;
        }

        internal static HashSet<string> GetGroupNames(List<EntityPrefabGroup> groups = default)
        {
            var groupNames = new HashSet<string>();

            if (groups != null)
            {
                foreach (var group in groups)
                    groupNames.Add(EntityPrefabUtility.Clean(group.name));

                return groupNames;
            }

            var gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));

            foreach (GameObject go in gameObjects)
            {
                var group = go.GetComponent<EntityPrefabGroup>();

                if (group == null) continue;

                groupNames.Add(EntityPrefabUtility.Clean(group.name));
            }

            return groupNames;
        }

        internal static void RegenerateClasses(List<string> groupNames, List<EntityPrefabGroup> groups)
        {
            if (Application.isPlaying) return;
            EntityPrefabUtility.DeleteOrphanedFiles(groupNames);
            EntityPrefabUtility.GenerateClasses(groups);
        }

        internal static void GenerateClasses(List<EntityPrefabGroup> groups)
        {
            foreach (var group in groups)
            {
                if (group == null || group.Prefabs == null || group.Prefabs.Count < 1) continue;

                var prefabNames = new List<string>();
                foreach (var prefab in group.Prefabs) prefabNames.Add(prefab.name);

                GenerateClass(group.name, prefabNames);
            }
        }

        internal static void GenerateClass(string groupName, List<string> prefabNames)
        {
            var cleanSceneName = EntityPrefabUtility.Clean(SceneManager.GetActiveScene().name);
            if (cleanSceneName == null || cleanSceneName.Length < 1) return;

            if (prefabNames.Count < 1) return;

            var cleanGroupName = Clean(groupName);
            if (cleanGroupName == null || cleanGroupName.Length < 1) return;

            var cleanPrefabNames = new List<string>();
            foreach (var name in prefabNames)
            {
                var cleanPrefabName = Clean(name);

                if (cleanPrefabName == null || cleanPrefabName.Length < 1) continue;

                cleanPrefabNames.Add(cleanPrefabName);
            }

            if (cleanPrefabNames.Count < 1) return;

            var path = Path.Combine(Application.dataPath, PACKAGE_DIRECTORY_NAME);
            path = Path.Combine(path, cleanSceneName);

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var className = cleanGroupName;

            DeleteFile(path, className);

            path = Path.Combine(path, $"{className}.cs");

            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                streamWriter.WriteLine("// Do NOT modify this file; it's code-generated.\n");

                streamWriter.WriteLine("using Unity.Collections;\n");

                streamWriter.WriteLine($"namespace {cleanSceneName}");

                streamWriter.WriteLine("{");

                streamWriter.WriteLine($"\tpublic static class {className}");

                streamWriter.WriteLine("\t{");

                for (var i = 0; i < cleanPrefabNames.Count; ++i) streamWriter.WriteLine($"\t\tpublic static readonly FixedString512 {cleanPrefabNames[i]} = \"{cleanPrefabNames[i]}\";");

                streamWriter.WriteLine("\t}");

                streamWriter.WriteLine("}");
            }

            AssetDatabase.Refresh();
        }

        internal static void DeleteOrphanedFiles(List<string> groupNames)
        {
            var path = Path.Combine(Application.dataPath, PACKAGE_DIRECTORY_NAME);
            path = Path.Combine(path, EntityPrefabUtility.Clean(SceneManager.GetActiveScene().name));

            if (!Directory.Exists(path)) return;

            var di = new DirectoryInfo(path);

            foreach (var file in di.EnumerateFiles())
            {
                var orphan = true;
                foreach (var groupName in groupNames)
                {
                    var cleanedGroupName = Clean(groupName);

                    if (file.Name.Equals($"{cleanedGroupName}.cs"))
                    {
                        orphan = false;
                        break;
                    }
                }

                if (orphan) file.Delete();
            }
        }

        internal static void DeleteFile(string path, params string[] names)
        {
            var di = new DirectoryInfo(path);

            foreach (var file in di.EnumerateFiles())
                for (var i = 0; i < names.Length; ++i)
                    if (file.Name.Equals(names[i]))
                        file.Delete();
        }

        internal static void ClearFilesInDirectory(string path)
        {
            var di = new DirectoryInfo(path);

            foreach (var file in di.EnumerateFiles()) file.Delete();
            foreach (var dir in di.EnumerateDirectories()) dir.Delete(true);

            AssetDatabase.Refresh();
        }

        internal static string Clean(string str)
            => str.Length > 0 && !char.IsDigit(str[0]) ? Regex.Replace(str, "[^a-zA-Z0-9]+", "", RegexOptions.Compiled) : null;

        internal static string GetPrefabNameFromPath(string path)
            => path.Split('/').Last().Split('.').First();
    }
}
