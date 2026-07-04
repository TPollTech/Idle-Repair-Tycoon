using System.Collections;
using UnityEngine;

namespace IdleRepairTycoon
{
    public sealed class IdleImportedDecor : MonoBehaviour
    {
        private static bool bootstrapCreated;
        private bool built;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (bootstrapCreated) return;
            bootstrapCreated = true;

            GameObject root = new GameObject("Idle Imported Decor Bootstrap");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<IdleImportedDecor>();
        }

        private IEnumerator Start()
        {
            for (int i = 0; i < 180; i++)
            {
                GameObject world = GameObject.Find("IdleRepairWorld");
                if (world != null)
                {
                    BuildDecor(world.transform);
                    yield break;
                }

                yield return null;
            }
        }

        private void BuildDecor(Transform worldRoot)
        {
            if (built || worldRoot == null) return;
            built = true;

            Material skybox = IdleAssetLoader.LoadMaterial("Assets/StarterAssets/Environment/Art/Skybox/SkyboxLiteWarm.mat");
            if (skybox != null) RenderSettings.skybox = skybox;

            Place(worldRoot, "Assets/SimpleNaturePack/Prefabs/Tree_01.prefab", "Asset_Tree_A", new Vector3(-5.05f, 0.02f, -3.30f), new Vector3(0f, 25f, 0f), 0.65f);
            Place(worldRoot, "Assets/SimpleNaturePack/Prefabs/Tree_03.prefab", "Asset_Tree_B", new Vector3(5.05f, 0.02f, -3.05f), new Vector3(0f, -18f, 0f), 0.58f);
            Place(worldRoot, "Assets/SimpleNaturePack/Prefabs/Bush_01.prefab", "Asset_Bush_A", new Vector3(-5.05f, 0.02f, -1.55f), new Vector3(0f, 8f, 0f), 0.48f);
            Place(worldRoot, "Assets/SimpleNaturePack/Prefabs/Bush_02.prefab", "Asset_Bush_B", new Vector3(5.05f, 0.02f, -1.65f), new Vector3(0f, -12f, 0f), 0.48f);
            Place(worldRoot, "Assets/SimpleNaturePack/Prefabs/Flowers_01.prefab", "Asset_Flowers_A", new Vector3(4.82f, 0.02f, 2.50f), new Vector3(0f, -30f, 0f), 0.34f);
            Place(worldRoot, "Assets/SimpleNaturePack/Prefabs/Rock_02.prefab", "Asset_Stone_A", new Vector3(-4.80f, 0.02f, 2.70f), new Vector3(0f, 40f, 0f), 0.42f);

            Place(worldRoot, "Assets/StarterAssets/Environment/Prefabs/Box_350x250x200_Prefab.prefab", "Asset_Stock_Box_A", new Vector3(-4.25f, 0.18f, 2.96f), new Vector3(0f, 18f, 0f), 0.18f);
            Place(worldRoot, "Assets/StarterAssets/Environment/Prefabs/Box_350x250x300_Prefab.prefab", "Asset_Stock_Box_B", new Vector3(-3.55f, 0.20f, 2.92f), new Vector3(0f, -12f, 0f), 0.16f);
            Place(worldRoot, "Assets/StarterAssets/Environment/Prefabs/Box_100x100x100_Prefab.prefab", "Asset_Counter_Box", new Vector3(-3.18f, 1.08f, 0.06f), new Vector3(0f, 22f, 0f), 0.13f);

            AddStationBox(worldRoot, "Station_film", "Asset_Film_Box");
            AddStationBox(worldRoot, "Station_battery", "Asset_Battery_Box");
            AddStationBox(worldRoot, "Station_screen", "Asset_Screen_Box");
            AddStationBox(worldRoot, "Station_notebook", "Asset_Notebook_Box");
            AddStationBox(worldRoot, "Station_premium", "Asset_Premium_Box");
        }

        private void AddStationBox(Transform worldRoot, string stationName, string objectName)
        {
            Transform station = worldRoot.Find(stationName);
            if (station == null) return;

            Place(station, "Assets/StarterAssets/Environment/Prefabs/Box_100x100x100_Prefab.prefab", objectName, new Vector3(-0.48f, 0.74f, 0.32f), new Vector3(0f, 15f, 0f), 0.10f);
        }

        private GameObject Place(Transform parent, string assetPath, string objectName, Vector3 localPosition, Vector3 localEuler, float uniformScale)
        {
            GameObject instance = IdleAssetLoader.InstantiatePrefab(assetPath, parent, objectName, localPosition, localEuler, Vector3.one * uniformScale);
            if (instance == null) return null;

            Collider[] colliders = instance.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                if (collider != null) Destroy(collider);
            }

            return instance;
        }
    }
}
