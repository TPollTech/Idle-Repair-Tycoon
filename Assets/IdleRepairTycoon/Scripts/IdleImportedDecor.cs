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

            // Só usa assets que fazem sentido para um laboratório/assistência:
            // caixas de estoque, caixas de peças e pequenos volumes técnicos.
            // Assets de natureza/flores/árvores ficam fora para não quebrar o tema.
            Place(worldRoot, "Assets/StarterAssets/Environment/Prefabs/Box_350x250x200_Prefab.prefab", "Lab_Stock_Box_A", new Vector3(-4.25f, 0.18f, 2.96f), new Vector3(0f, 18f, 0f), 0.18f);
            Place(worldRoot, "Assets/StarterAssets/Environment/Prefabs/Box_350x250x300_Prefab.prefab", "Lab_Stock_Box_B", new Vector3(-3.55f, 0.20f, 2.92f), new Vector3(0f, -12f, 0f), 0.16f);
            Place(worldRoot, "Assets/StarterAssets/Environment/Prefabs/Box_350x250x200_Prefab.prefab", "Lab_Stock_Box_C", new Vector3(4.78f, 0.18f, 2.48f), new Vector3(0f, -22f, 0f), 0.16f);
            Place(worldRoot, "Assets/StarterAssets/Environment/Prefabs/Box_350x250x300_Prefab.prefab", "Lab_Stock_Box_D", new Vector3(4.20f, 0.20f, 2.74f), new Vector3(0f, 28f, 0f), 0.14f);
            Place(worldRoot, "Assets/StarterAssets/Environment/Prefabs/Box_100x100x100_Prefab.prefab", "Lab_Counter_Parts_Box", new Vector3(-3.18f, 1.08f, 0.06f), new Vector3(0f, 22f, 0f), 0.13f);
            Place(worldRoot, "Assets/StarterAssets/Environment/Prefabs/Box_100x100x100_Prefab.prefab", "Lab_Register_Supply_Box", new Vector3(4.54f, 1.02f, -0.18f), new Vector3(0f, -18f, 0f), 0.10f);

            AddStationBox(worldRoot, "Station_film", "Lab_Film_Parts_Box");
            AddStationBox(worldRoot, "Station_battery", "Lab_Battery_Parts_Box");
            AddStationBox(worldRoot, "Station_screen", "Lab_Screen_Parts_Box");
            AddStationBox(worldRoot, "Station_notebook", "Lab_Notebook_Parts_Box");
            AddStationBox(worldRoot, "Station_premium", "Lab_Premium_Parts_Box");
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
