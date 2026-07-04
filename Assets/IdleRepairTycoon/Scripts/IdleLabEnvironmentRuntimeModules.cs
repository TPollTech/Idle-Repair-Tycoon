using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IdleRepairTycoon
{
    public sealed class IdleLabEnvironmentRuntimeModules : MonoBehaviour
    {
        private static bool bootstrapCreated;
        private bool organized;
        private readonly Dictionary<string, Transform> modules = new Dictionary<string, Transform>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (bootstrapCreated) return;
            bootstrapCreated = true;

            GameObject root = new GameObject("Idle Lab Environment Modules Bootstrap");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<IdleLabEnvironmentRuntimeModules>();
        }

        private IEnumerator Start()
        {
            GameObject world = null;

            for (int i = 0; i < 180; i++)
            {
                world = GameObject.Find("IdleRepairWorld");
                if (world != null) break;
                yield return null;
            }

            if (world == null) yield break;

            // Espera alguns frames para o IdleGameWorld e o IdleImportedDecor terminarem de criar objetos.
            for (int i = 0; i < 12; i++) yield return null;

            Organize(world.transform);
        }

        private void Organize(Transform worldRoot)
        {
            if (organized || worldRoot == null) return;
            organized = true;

            Transform lab = EnsureChild(worldRoot, "LabEnvironment");
            modules.Clear();

            modules["00_Floor"] = EnsureChild(lab, "00_Floor");
            modules["01_Walls"] = EnsureChild(lab, "01_Walls");
            modules["02_FrontCounter"] = EnsureChild(lab, "02_FrontCounter");
            modules["03_Cashier"] = EnsureChild(lab, "03_Cashier");
            modules["04_Workbenches"] = EnsureChild(lab, "04_Workbenches");
            modules["05_Shelves"] = EnsureChild(lab, "05_Shelves");
            modules["06_WaitingArea"] = EnsureChild(lab, "06_WaitingArea");
            modules["07_Props"] = EnsureChild(lab, "07_Props");
            modules["08_Lighting"] = EnsureChild(lab, "08_Lighting");
            modules["09_CameraMarkers"] = EnsureChild(lab, "09_CameraMarkers");

            MoveByName(worldRoot, "Floor", "00_Floor");
            MoveByName(worldRoot, "EntranceMat", "00_Floor");

            MoveByName(worldRoot, "BackWall", "01_Walls");
            MoveByName(worldRoot, "LeftWall", "01_Walls");
            MoveByName(worldRoot, "RightWall", "01_Walls");
            MoveByPrefix(worldRoot, "Label_ENTRADA", "01_Walls");
            MoveByPrefix(worldRoot, "Label_BALCÃO", "01_Walls");
            MoveByPrefix(worldRoot, "Label_CAIXA", "01_Walls");

            MoveByName(worldRoot, "FrontCounter", "02_FrontCounter");
            MoveByName(worldRoot, "Lab_Counter_Parts_Box", "02_FrontCounter");
            MoveByName(worldRoot, "Asset_Counter_Box", "02_FrontCounter");
            MoveByName(worldRoot, "Small_Box_Counter", "02_FrontCounter");

            MoveByName(worldRoot, "Cashier", "03_Cashier");
            MoveByName(worldRoot, "RegisterScreen", "03_Cashier");
            MoveByName(worldRoot, "Lab_Register_Supply_Box", "03_Cashier");

            MoveStation(worldRoot, "Station_film", "Workbench_01_Peliculas");
            MoveStation(worldRoot, "Station_battery", "Workbench_02_Baterias");
            MoveStation(worldRoot, "Station_screen", "Workbench_03_Telas");
            MoveStation(worldRoot, "Station_notebook", "Workbench_04_Notebooks");
            MoveStation(worldRoot, "Station_premium", "Workbench_05_Premium");

            MoveByPrefix(worldRoot, "Shelf", "05_Shelves");
            MoveByPrefix(worldRoot, "Lab_Stock_Box", "05_Shelves");
            MoveByPrefix(worldRoot, "Asset_Stock_Box", "05_Shelves");
            MoveByName(worldRoot, "Stock_Box_A", "05_Shelves");
            MoveByName(worldRoot, "Stock_Box_B", "05_Shelves");

            MoveByName(worldRoot, "Clients", "06_WaitingArea");

            MoveByPrefix(worldRoot, "Asset_", "07_Props");
            MoveByPrefix(worldRoot, "Decor_", "07_Props");

            MoveByName(worldRoot, "SoftSun", "08_Lighting");
            MoveByName(worldRoot, "FillLight", "08_Lighting");
            MoveByName(worldRoot, "Directional Light", "08_Lighting");

            MoveByName(worldRoot, "IdleRepairWorldCamera", "09_CameraMarkers");
            MoveByName(worldRoot, "Main Camera", "09_CameraMarkers");
        }

        private void MoveStation(Transform worldRoot, string stationName, string moduleName)
        {
            Transform workbenches = modules["04_Workbenches"];
            Transform module = EnsureChild(workbenches, moduleName);
            Transform station = FindImmediateChild(worldRoot, stationName);
            if (station != null) station.SetParent(module, true);
        }

        private void MoveByName(Transform worldRoot, string objectName, string moduleName)
        {
            Transform obj = FindImmediateChild(worldRoot, objectName);
            if (obj == null || !modules.TryGetValue(moduleName, out Transform module)) return;
            obj.SetParent(module, true);
        }

        private void MoveByPrefix(Transform worldRoot, string prefix, string moduleName)
        {
            if (!modules.TryGetValue(moduleName, out Transform module)) return;

            List<Transform> matches = new List<Transform>();
            for (int i = 0; i < worldRoot.childCount; i++)
            {
                Transform child = worldRoot.GetChild(i);
                if (child != null && child.name.StartsWith(prefix))
                    matches.Add(child);
            }

            foreach (Transform match in matches)
                match.SetParent(module, true);
        }

        private Transform EnsureChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing;

            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            return obj.transform;
        }

        private Transform FindImmediateChild(Transform parent, string name)
        {
            if (parent == null) return null;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null && child.name == name)
                    return child;
            }

            return null;
        }
    }
}
