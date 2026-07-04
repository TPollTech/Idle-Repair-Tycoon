using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IdleRepairTycoon
{
    public static class IdleAssetLoader
    {
        public static GameObject InstantiatePrefab(string assetPath, Transform parent, string objectName, Vector3 localPosition, Vector3 localEuler, Vector3 localScale)
        {
#if UNITY_EDITOR
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null) return null;

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) instance = Object.Instantiate(prefab);

            instance.name = string.IsNullOrEmpty(objectName) ? prefab.name : objectName;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(localEuler);
            instance.transform.localScale = localScale;
            return instance;
#else
            return null;
#endif
        }

        public static GameObject InstantiateFirstPrefab(Transform parent, string objectName, Vector3 localPosition, Vector3 localEuler, Vector3 localScale, params string[] candidatePaths)
        {
#if UNITY_EDITOR
            if (candidatePaths == null) return null;

            foreach (string path in candidatePaths)
            {
                if (string.IsNullOrEmpty(path)) continue;

                GameObject instance = InstantiatePrefab(path, parent, objectName, localPosition, localEuler, localScale);
                if (instance != null) return instance;
            }
#endif
            return null;
        }

        public static Material LoadMaterial(string assetPath)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Material>(assetPath);
#else
            return null;
#endif
        }

        public static bool AssetExists(string assetPath)
        {
#if UNITY_EDITOR
            return !string.IsNullOrEmpty(assetPath) && AssetDatabase.LoadAssetAtPath<Object>(assetPath) != null;
#else
            return false;
#endif
        }
    }
}
