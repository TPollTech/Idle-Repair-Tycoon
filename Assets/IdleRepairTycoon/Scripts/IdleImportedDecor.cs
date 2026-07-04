using System.Collections;
using UnityEngine;

namespace IdleRepairTycoon
{
    public sealed class IdleImportedDecor : MonoBehaviour
    {
        private static readonly string[] OfficeDeskPaths =
        {
            "Assets/Office Room Furniture/Office/Prefabs/OfficeDesk_1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/OfficeDesk.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/OfficeDesk 1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/OfficeDesk1.prefab"
        };

        private static readonly string[] OfficeChairPaths =
        {
            "Assets/Office Room Furniture/Office/Prefabs/OfficeChair_1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/OfficeChair.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/OfficeChair 1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/OfficeChair1.prefab"
        };

        private static readonly string[] ArmchairPaths =
        {
            "Assets/Office Room Furniture/Office/Prefabs/Armchair_1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/Armchair.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/Armchair 1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/Armchair1.prefab"
        };

        private static readonly string[] MonitorPaths =
        {
            "Assets/Office Room Furniture/Office/Prefabs/Monitor_1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/Monitor.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/Monitor 1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/Monitor1.prefab"
        };

        private static readonly string[] MouseKeyboardPaths =
        {
            "Assets/Office Room Furniture/Office/Prefabs/MouseKeyboard_1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/MouseKeyboard.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/MouseKeyboard 1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/MouseKeyboard1.prefab"
        };

        private static readonly string[] FloorLampPaths =
        {
            "Assets/Office Room Furniture/Office/Prefabs/FloorLamp_1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/FloorLamp.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/FloorLamp 1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/FloorLamp1.prefab"
        };

        private static readonly string[] FloorPlantPaths =
        {
            "Assets/Office Room Furniture/Office/Prefabs/FloorPlant_1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/FloorPlant.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/FloorPlant 1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/FloorPlant1.prefab"
        };

        private static readonly string[] FramePaths =
        {
            "Assets/Office Room Furniture/Office/Prefabs/Frame_1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/Frame 1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/Frame.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/Frame1.prefab"
        };

        private static readonly string[] NestingTablePaths =
        {
            "Assets/Office Room Furniture/Office/Prefabs/NestingTable_1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/NestingTables_1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/NestingTable.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/NestingTables.prefab"
        };

        private static readonly string[] ObjectsPaths =
        {
            "Assets/Office Room Furniture/Office/Prefabs/Objects_1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/Objects.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/Objects 1.prefab",
            "Assets/Office Room Furniture/Office/Prefabs/Objects1.prefab"
        };

        private static readonly string[] StarterSmallBoxPaths =
        {
            "Assets/StarterAssets/Environment/Prefabs/Box_100x100x100_Prefab.prefab"
        };

        private static readonly string[] StarterMediumBoxPaths =
        {
            "Assets/StarterAssets/Environment/Prefabs/Box_350x250x200_Prefab.prefab",
            "Assets/StarterAssets/Environment/Prefabs/Box_350x250x300_Prefab.prefab"
        };

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

            BuildFrontCounter(worldRoot);
            BuildCashier(worldRoot);
            BuildWorkbenches(worldRoot);
            BuildWaitingArea(worldRoot);
            BuildShelvesAndProps(worldRoot);
        }

        private void BuildFrontCounter(Transform worldRoot)
        {
            PlaceFirst(worldRoot, "Lab_FrontCounter_OfficeDesk", new Vector3(-2.95f, 0.08f, -0.18f), new Vector3(0f, 180f, 0f), 0.62f, OfficeDeskPaths);
            PlaceFirst(worldRoot, "Lab_FrontCounter_OfficeChair", new Vector3(-2.95f, 0.05f, -0.90f), new Vector3(0f, 0f, 0f), 0.62f, OfficeChairPaths);
            PlaceFirst(worldRoot, "Lab_FrontCounter_Monitor", new Vector3(-3.18f, 0.94f, -0.16f), new Vector3(0f, 180f, 0f), 0.46f, MonitorPaths);
            PlaceFirst(worldRoot, "Lab_FrontCounter_MouseKeyboard", new Vector3(-2.88f, 0.88f, -0.22f), new Vector3(0f, 180f, 0f), 0.42f, MouseKeyboardPaths);
            PlaceFirst(worldRoot, "Lab_FrontCounter_PartsBox", new Vector3(-3.58f, 0.96f, 0.05f), new Vector3(0f, 20f, 0f), 0.10f, StarterSmallBoxPaths);
        }

        private void BuildCashier(Transform worldRoot)
        {
            PlaceFirst(worldRoot, "Lab_Cashier_OfficeDesk", new Vector3(4.15f, 0.08f, -0.55f), new Vector3(0f, 90f, 0f), 0.54f, OfficeDeskPaths);
            PlaceFirst(worldRoot, "Lab_Cashier_OfficeChair", new Vector3(4.72f, 0.05f, -0.70f), new Vector3(0f, -90f, 0f), 0.52f, OfficeChairPaths);
            PlaceFirst(worldRoot, "Lab_Cashier_Monitor", new Vector3(4.02f, 0.92f, -0.70f), new Vector3(0f, 90f, 0f), 0.42f, MonitorPaths);
            PlaceFirst(worldRoot, "Lab_Cashier_MouseKeyboard", new Vector3(4.08f, 0.87f, -0.42f), new Vector3(0f, 90f, 0f), 0.36f, MouseKeyboardPaths);
            PlaceFirst(worldRoot, "Lab_Cashier_SupplyBox", new Vector3(4.52f, 0.95f, -0.20f), new Vector3(0f, -18f, 0f), 0.09f, StarterSmallBoxPaths);
        }

        private void BuildWorkbenches(Transform worldRoot)
        {
            BuildWorkbench(worldRoot, "Station_film", "Lab_Workbench_01", new Vector3(-0.08f, -0.35f, -0.02f), 0.48f);
            BuildWorkbench(worldRoot, "Station_battery", "Lab_Workbench_02", new Vector3(-0.08f, -0.35f, -0.02f), 0.48f);
            BuildWorkbench(worldRoot, "Station_screen", "Lab_Workbench_03", new Vector3(-0.08f, -0.35f, -0.02f), 0.50f);
            BuildWorkbench(worldRoot, "Station_notebook", "Lab_Workbench_04", new Vector3(-0.10f, -0.36f, -0.04f), 0.58f);
            BuildWorkbench(worldRoot, "Station_premium", "Lab_Workbench_05", new Vector3(-0.10f, -0.36f, -0.04f), 0.58f);
        }

        private void BuildWorkbench(Transform worldRoot, string stationName, string prefix, Vector3 localOffset, float deskScale)
        {
            Transform station = worldRoot.Find(stationName);
            if (station == null) return;

            PlaceFirst(station, prefix + "_Desk", localOffset, new Vector3(0f, 180f, 0f), deskScale, OfficeDeskPaths);
            PlaceFirst(station, prefix + "_Chair", new Vector3(0.58f, -0.31f, 0.55f), new Vector3(0f, 180f, 0f), Mathf.Max(0.40f, deskScale * 0.82f), OfficeChairPaths);
            PlaceFirst(station, prefix + "_Monitor", new Vector3(-0.18f, 0.40f, 0.14f), new Vector3(0f, 180f, 0f), deskScale * 0.42f, MonitorPaths);
            PlaceFirst(station, prefix + "_MouseKeyboard", new Vector3(0.18f, 0.38f, -0.16f), new Vector3(0f, 180f, 0f), deskScale * 0.36f, MouseKeyboardPaths);
            PlaceFirst(station, prefix + "_PartsBox", new Vector3(-0.48f, 0.39f, 0.30f), new Vector3(0f, 15f, 0f), 0.10f, StarterSmallBoxPaths);
        }

        private void BuildWaitingArea(Transform worldRoot)
        {
            PlaceFirst(worldRoot, "Lab_Waiting_Armchair_A", new Vector3(4.25f, 0.04f, 1.55f), new Vector3(0f, -135f, 0f), 0.68f, ArmchairPaths);
            PlaceFirst(worldRoot, "Lab_Waiting_Armchair_B", new Vector3(3.18f, 0.04f, 2.25f), new Vector3(0f, -50f, 0f), 0.62f, ArmchairPaths);
            PlaceFirst(worldRoot, "Lab_Waiting_Table", new Vector3(3.72f, 0.04f, 1.98f), new Vector3(0f, 20f, 0f), 0.52f, NestingTablePaths);
            PlaceFirst(worldRoot, "Lab_Waiting_FloorLamp", new Vector3(4.92f, 0.04f, 2.72f), new Vector3(0f, -20f, 0f), 0.72f, FloorLampPaths);
        }

        private void BuildShelvesAndProps(Transform worldRoot)
        {
            PlaceFirst(worldRoot, "Lab_Shelf_StockBox_A", new Vector3(-4.25f, 0.18f, 2.96f), new Vector3(0f, 18f, 0f), 0.18f, StarterMediumBoxPaths);
            PlaceFirst(worldRoot, "Lab_Shelf_StockBox_B", new Vector3(-3.55f, 0.20f, 2.92f), new Vector3(0f, -12f, 0f), 0.16f, StarterMediumBoxPaths);
            PlaceFirst(worldRoot, "Lab_Shelf_StockBox_C", new Vector3(4.78f, 0.18f, 2.48f), new Vector3(0f, -22f, 0f), 0.16f, StarterMediumBoxPaths);
            PlaceFirst(worldRoot, "Lab_Shelf_StockBox_D", new Vector3(4.20f, 0.20f, 2.74f), new Vector3(0f, 28f, 0f), 0.14f, StarterMediumBoxPaths);
            PlaceFirst(worldRoot, "Lab_Prop_FloorPlant_Internal", new Vector3(-4.88f, 0.04f, 1.95f), new Vector3(0f, 15f, 0f), 0.62f, FloorPlantPaths);
            PlaceFirst(worldRoot, "Lab_Prop_WallFrame_A", new Vector3(-1.80f, 1.72f, 3.46f), new Vector3(0f, 180f, 0f), 0.62f, FramePaths);
            PlaceFirst(worldRoot, "Lab_Prop_WallFrame_B", new Vector3(1.10f, 1.72f, 3.46f), new Vector3(0f, 180f, 0f), 0.58f, FramePaths);
            PlaceFirst(worldRoot, "Lab_Prop_TableObjects", new Vector3(-2.60f, 0.98f, 0.10f), new Vector3(0f, 30f, 0f), 0.30f, ObjectsPaths);
        }

        private GameObject PlaceFirst(Transform parent, string objectName, Vector3 localPosition, Vector3 localEuler, float uniformScale, params string[][] candidateGroups)
        {
            foreach (string[] group in candidateGroups)
            {
                GameObject instance = IdleAssetLoader.InstantiateFirstPrefab(parent, objectName, localPosition, localEuler, Vector3.one * uniformScale, group);
                if (instance != null)
                {
                    DisableColliders(instance);
                    return instance;
                }
            }

            return null;
        }

        private void DisableColliders(GameObject instance)
        {
            if (instance == null) return;

            Collider[] colliders = instance.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                if (collider != null) Destroy(collider);
            }
        }
    }
}
