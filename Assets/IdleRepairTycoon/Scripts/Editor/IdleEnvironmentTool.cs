using UnityEngine;
using UnityEditor;
using System.IO;

namespace IdleRepairTycoon
{
    public sealed class IdleEnvironmentTool : EditorWindow
    {
        private string materialsPath = "Assets/IdleRepairTycoon/Materials";
        private string prefabsPath = "Assets/IdleRepairTycoon/Prefabs";
        private Vector2 scroll;

        [MenuItem("TPoll/Idle Repair Tycoon/Criar Laboratório Editável", false, 1)]
        private static void OpenWindow()
        {
            IdleEnvironmentTool window = GetWindow<IdleEnvironmentTool>();
            window.titleContent = new GUIContent("Lab Environment");
            window.minSize = new Vector2(380, 520);
            window.Show();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            GUILayout.Space(12);

            DrawHeader("Materiais do Laboratório");
            materialsPath = EditorGUILayout.TextField("Caminho", materialsPath);
            if (GUILayout.Button("Gerar Materiais URP", GUILayout.Height(32)))
                GenerateMaterials();
            GUILayout.Space(6);

            DrawHeader("Laboratório Editável");
            prefabsPath = EditorGUILayout.TextField("Caminho", prefabsPath);
            if (GUILayout.Button("Gerar LabEnvironment_EDITAVEL", GUILayout.Height(32)))
                GenerateLabEnvironment();
            GUILayout.Space(6);

            DrawHeader("Assets importados");
            if (GUILayout.Button("Normalizar assets decorativos existentes", GUILayout.Height(28)))
                NormalizeExistingDecor();

            EditorGUILayout.EndScrollView();
        }

        private static void DrawHeader(string text)
        {
            GUILayout.Label(text, EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
        }

        private void GenerateMaterials()
        {
            EnsureFolder(materialsPath);

            CreateMaterial("Lab_Floor_LightGray", new Color(0.86f, 0.89f, 0.94f));
            CreateMaterial("Lab_Wall_SoftBlue", new Color(0.61f, 0.72f, 0.90f));
            CreateMaterial("Lab_Wall_SideBlue", new Color(0.53f, 0.64f, 0.83f));
            CreateMaterial("Lab_Accent_Cyan", new Color(0.17f, 0.42f, 0.82f));
            CreateMaterial("Lab_Furniture_Dark", new Color(0.12f, 0.14f, 0.22f));
            CreateMaterial("Lab_Chair_Dark", new Color(0.14f, 0.16f, 0.24f));
            CreateMaterial("Lab_Device_Black", new Color(0.04f, 0.06f, 0.12f));
            CreateMaterial("Lab_Box_Cardboard", new Color(0.72f, 0.62f, 0.48f));
            CreateMaterial("Lab_Plant_Green", new Color(0.28f, 0.62f, 0.32f));
            CreateMaterial("Lab_Table_White", new Color(0.95f, 0.96f, 1.00f));
            CreateMaterial("Lab_Machine_Dark", new Color(0.08f, 0.10f, 0.16f));
            CreateMaterial("Lab_Shelf_Yellow", new Color(0.95f, 0.70f, 0.24f));
            CreateMaterial("Lab_Register_Purple", new Color(0.55f, 0.25f, 0.78f));

            AssetDatabase.Refresh();
            Debug.Log("Materiais gerados em: " + materialsPath);
        }

        private static void CreateMaterial(string name, Color color)
        {
            string path = materialsPath + "/" + name + ".mat";
            if (File.Exists(path)) return;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
        }

        private void GenerateLabEnvironment()
        {
            EnsureFolder(prefabsPath);

            GameObject root = new GameObject("LabEnvironment_EDITAVEL");
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            CreateGroup(root, "00_Floor");
            CreateGroup(root, "01_Walls");
            CreateGroup(root, "02_FrontCounter");
            CreateGroup(root, "03_Cashier");
            CreateGroup(root, "04_Workbenches");
            CreateGroup(root, "05_Shelves");
            CreateGroup(root, "06_WaitingArea");
            CreateGroup(root, "07_Props");
            CreateGroup(root, "08_Lighting");
            CreateGroup(root, "09_CameraMarkers");

            string prefabPath = prefabsPath + "/LabEnvironment_EDITAVEL.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.Refresh();
            Debug.Log("LabEnvironment_EDITAVEL gerado em: " + prefabPath);
        }

        private static void CreateGroup(GameObject parent, string name)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent.transform, false);
            group.transform.localPosition = Vector3.zero;
            group.transform.localRotation = Quaternion.identity;
            group.transform.localScale = Vector3.one;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent))
                    EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static void NormalizeExistingDecor()
        {
            IdleImportedDecor decor = Object.FindAnyObjectByType<IdleImportedDecor>();
            if (decor == null)
            {
                Debug.LogWarning("Nenhum IdleImportedDecor encontrado na cena. Entre em Play Mode primeiro.");
                return;
            }

            Transform world = decor.transform.Find("IdleRepairWorld");
            if (world == null) world = GameObject.Find("IdleRepairWorld")?.transform;
            if (world == null) { Debug.LogWarning("IdleRepairWorld não encontrado."); return; }

            Renderer[] renderers = world.GetComponentsInChildren<Renderer>();
            int count = 0;
            foreach (Renderer r in renderers)
            {
                if (r.name.Contains("Box") || r.name.Contains("Chair") || r.name.Contains("Clock"))
                {
                    IdleAssetNormalizer.NormalizeAndAlign(r.gameObject, 0.35f);
                    count++;
                }
            }
            Debug.Log("Normalizados: " + count + " objetos decorativos.");
        }
    }
}
