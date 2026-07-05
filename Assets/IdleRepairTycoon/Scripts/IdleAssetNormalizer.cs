using UnityEngine;

namespace IdleRepairTycoon
{
    public static class IdleAssetNormalizer
    {
        public static Bounds CalculateRendererBounds(GameObject obj)
        {
            Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            bool found = false;
            foreach (Renderer r in renderers)
            {
                if (!found) { bounds = r.bounds; found = true; }
                else { bounds.Encapsulate(r.bounds); }
            }
            if (!found) bounds = new Bounds(obj.transform.position, Vector3.one);
            return bounds;
        }

        public static float MeasureWidth(GameObject obj)
        {
            Bounds b = CalculateRendererBounds(obj);
            return Mathf.Max(b.size.x, b.size.z);
        }

        public static float MeasureHeight(GameObject obj)
        {
            return CalculateRendererBounds(obj).size.y;
        }

        public static void NormalizeScale(GameObject obj, float targetWidth)
        {
            if (obj == null) return;
            float current = MeasureWidth(obj);
            if (current < 0.001f) return;
            obj.transform.localScale *= targetWidth / current;
        }

        public static void AlignToFloor(GameObject obj)
        {
            Bounds b = CalculateRendererBounds(obj);
            float yOffset = -b.min.y + obj.transform.position.y;
            obj.transform.position = new Vector3(obj.transform.position.x, yOffset, obj.transform.position.z);
        }

        public static void NormalizeAndAlign(GameObject obj, float targetWidth)
        {
            NormalizeScale(obj, targetWidth);
            AlignToFloor(obj);
        }

        public static GameObject InstantiateNormalized(string assetPath, Transform parent, string objectName, Vector3 localPosition, Vector3 localEuler, float targetWidth, bool alignToFloor = true)
        {
#if UNITY_EDITOR
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null) return null;

            GameObject instance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) instance = Object.Instantiate(prefab);
            instance.name = string.IsNullOrEmpty(objectName) ? prefab.name : objectName;

            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(localEuler);

            NormalizeAndAlign(instance, targetWidth);

            Collider[] colliders = instance.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
                Object.DestroyImmediate(colliders[i]);

            return instance;
#else
            return null;
#endif
        }
    }
}
