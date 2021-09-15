using System.Collections.Generic;
using System.Linq;
using Malee.List;
using UnityEditor;
using UnityEngine;

namespace Reese.EntityPrefabGroups
{
    [CustomEditor(typeof(EntityPrefabGroup))]
    public class EntityPrefabGroupEditor : Editor
    {
        EntityPrefabGroup obj = default;
        SerializedObject serializedObj = default;
        SerializedProperty settingsProperty = default;
        ReorderableList reorderableSettings = default;

        [SerializeField]
        int lastPrefabCount = 0;

        public void OnEnable()
        {
            obj = (EntityPrefabGroup)target;
            serializedObj = new SerializedObject(obj);
            settingsProperty = serializedObj.FindProperty(nameof(EntityPrefabGroup.Prefabs));
            reorderableSettings = new ReorderableList(settingsProperty);

            if (obj == null || obj.name == null || obj.name.Length < 1) return;

            var data = EditorPrefs.GetString(obj.name, JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);
        }

        public void OnDisable()
        {
            if (obj == null || obj.name == null || obj.name.Length < 1) return;

            var data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString(obj.name, data);
        }

        public override void OnInspectorGUI()
        {
            serializedObj.Update();

            if (obj == null || obj.Prefabs == null)
            {
                Draw();
                return;
            }

            var names = new List<string>();
            foreach (var prefab in obj.Prefabs)
            {
                if (prefab == null) continue;

                var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
                var cleanPrefabName = EntityPrefabUtility.Clean(EntityPrefabUtility.GetPrefabNameFromPath(prefabPath));

                if (cleanPrefabName == null || cleanPrefabName.Length < 1) continue;

                names.Add(cleanPrefabName);
            }

            var prefabCount = 0;
            names = names.Distinct().ToList();
            foreach (var name in names)
            {
                if (name == null) continue;

                ++prefabCount;
            }

            if (prefabCount != lastPrefabCount) EntityPrefabUtility.GenerateClass(obj.name, names);

            lastPrefabCount = prefabCount;

            Draw();
        }

        void Draw()
        {
            EditorGUILayout.Space();

            reorderableSettings.DoLayoutList();

            EditorGUILayout.LabelField("Scene, group, and prefab names must be unique.", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("Names cannot start with a number.", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("Offenders will be ignored in codegen and conversion.", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.Space();

            serializedObj.ApplyModifiedProperties();
        }
    }
}
