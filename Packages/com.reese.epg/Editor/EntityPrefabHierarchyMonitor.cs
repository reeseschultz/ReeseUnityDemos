using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Reese.EntityPrefabGroups
{
    [InitializeOnLoadAttribute]
    public static class EntityPrefabHierarchyMonitor
    {
        static HashSet<string> lastGroupNames = new HashSet<string>();

        static EntityPrefabHierarchyMonitor()
        {
            var groups = EntityPrefabUtility.GetEntityPrefabGroups();
            lastGroupNames = EntityPrefabUtility.GetGroupNames(true, groups);
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        static void OnHierarchyChanged()
        {
            var groups = EntityPrefabUtility.GetEntityPrefabGroups();
            var currentGroupNames = EntityPrefabUtility.GetGroupNames(true, groups);
            var currentGroupNamesAsList = currentGroupNames.ToList();

            foreach (var name in lastGroupNames)
            {
                if (!currentGroupNames.Contains(name))
                {
                    EntityPrefabUtility.RegeneratePrefabHelperClasses(currentGroupNamesAsList, groups);
                    break;
                }
            }

            lastGroupNames = currentGroupNames;
        }
    }
}
