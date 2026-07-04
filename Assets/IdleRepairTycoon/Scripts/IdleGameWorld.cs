using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IdleRepairTycoon
{
    public sealed class IdleGameWorld : MonoBehaviour
    {
        private sealed class StationVisual
        {
            public string Id;
            public Transform Root;
            public Renderer BaseRenderer;
            public Renderer MachineRenderer;
            public Renderer TopRenderer;
            public Transform ProgressFill;
            public Transform Device;
            public Transform SelectionBase;
            public Transform Technician;
            public Transform TechnicianArm;
            public TextMesh Label;
            public Color BaseColor;
        }

        private IdleGameController controller;
        private Camera worldCamera;
        private Transform worldRoot;
        private Transform clientsRoot;
        private readonly List<Transform> clients = new List<Transform>();
        private readonly List<StationVisual> stationVisuals = new List<StationVisual>();
        private readonly Dictionary<Collider, string> stationColliderMap = new Dictionary<Collider, string>();

        public void Bind(IdleGameController gameController)
        {
            controller = gameController;
            BuildWorld();
        }

        private void Update()
        {
            if (controller == null) return;

            HandleSelectionInput();
            AnimateClients();
            RefreshStations();
        }

        private void BuildWorld()
        {
            GameObject root = new GameObject("IdleRepairWorld");
            root.transform.SetParent(transform, false);
            worldRoot = root.transform;

            BuildCamera();
            BuildLights();
            BuildRoom();
            BuildStations();
            BuildClients();
            RefreshStations();
        }

        private void BuildCamera()
        {
            GameObject cameraObject = new GameObject("IdleRepairWorldCamera");
            cameraObject.transform.SetParent(worldRoot, false);
            worldCamera = cameraObject.AddComponent<Camera>();
            worldCamera.orthographic = true;
            worldCamera.orthographicSize = 6.9f;
            worldCamera.clearFlags = CameraClearFlags.SolidColor;
            worldCamera.backgroundColor = new Color(0.70f, 0.80f, 0.93f, 1f);
            worldCamera.transform.position = new Vector3(6.4f, 7.8f, -8.6f);
            worldCamera.transform.LookAt(new Vector3(0f, 0.25f, 0.05f));
            worldCamera.nearClipPlane = 0.1f;
            worldCamera.farClipPlane = 100f;

            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera camera in cameras)
            {
                if (camera != null && camera != worldCamera)
                    camera.enabled = false;
            }

            cameraObject.tag = "MainCamera";
        }

        private void BuildLights()
        {
            GameObject sun = new GameObject("SoftSun");
            sun.transform.SetParent(worldRoot, false);
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.intensity = 1.15f;
            sun.transform.rotation = Quaternion.Euler(52f, -38f, 0f);

            GameObject fill = new GameObject("FillLight");
            fill.transform.SetParent(worldRoot, false);
            Light fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.intensity = 1.55f;
            fillLight.range = 9.5f;
            fill.transform.position = new Vector3(0f, 4.2f, -2.2f);
        }

        private void BuildRoom()
        {
            CreateCube("Floor", new Vector3(0f, -0.06f, 0f), new Vector3(11.2f, 0.10f, 7.0f), new Color(0.86f, 0.89f, 0.94f, 1f));
            CreateCube("BackWall", new Vector3(0f, 0.98f, 3.56f), new Vector3(11.2f, 1.95f, 0.16f), new Color(0.61f, 0.72f, 0.90f, 1f));
            CreateCube("LeftWall", new Vector3(-5.62f, 0.62f, 0.2f), new Vector3(0.16f, 1.25f, 6.75f), new Color(0.53f, 0.64f, 0.83f, 1f));
            CreateCube("RightWall", new Vector3(5.62f, 0.62f, 0.2f), new Vector3(0.16f, 1.25f, 6.75f), new Color(0.53f, 0.64f, 0.83f, 1f));

            CreateCube("EntranceMat", new Vector3(-4.15f, 0.03f, -2.85f), new Vector3(1.65f, 0.05f, 0.62f), new Color(0.17f, 0.42f, 0.82f, 1f));
            CreateCube("FrontCounter", new Vector3(-2.95f, 0.42f, 0.28f), new Vector3(1.65f, 0.84f, 0.62f), new Color(0.08f, 0.50f, 0.34f, 1f));
            CreateCube("Cashier", new Vector3(4.28f, 0.43f, -0.58f), new Vector3(1.18f, 0.86f, 0.72f), new Color(0.55f, 0.25f, 0.78f, 1f));
            CreateCube("RegisterScreen", new Vector3(4.28f, 1.02f, -0.82f), new Vector3(0.48f, 0.32f, 0.08f), new Color(0.04f, 0.06f, 0.11f, 1f));

            CreateSmallLabel("ENTRADA", new Vector3(-4.15f, 0.16f, -3.22f), 0.045f, new Color(0.02f, 0.04f, 0.08f, 1f));
            CreateSmallLabel("BALCÃO", new Vector3(-2.95f, 0.94f, -0.08f), 0.040f, Color.white);
            CreateSmallLabel("CAIXA", new Vector3(4.28f, 0.98f, -1.02f), 0.040f, Color.white);

            for (int i = 0; i < 4; i++)
            {
                float x = -4.1f + (i * 1.05f);
                CreateCube("Shelf" + i, new Vector3(x, 1.26f, 3.42f), new Vector3(0.78f, 0.30f, 0.10f), new Color(0.95f, 0.70f, 0.24f, 1f));
            }
        }

        private void BuildStations()
        {
            Vector3[] positions =
            {
                new Vector3(-1.75f, 0.35f, 1.20f),
                new Vector3(0.00f, 0.35f, 1.20f),
                new Vector3(1.75f, 0.35f, 1.20f),
                new Vector3(-0.85f, 0.35f, -1.05f),
                new Vector3(1.05f, 0.35f, -1.05f)
            };

            Color[] colors =
            {
                new Color(0.16f, 0.55f, 0.95f, 1f),
                new Color(0.10f, 0.66f, 0.43f, 1f),
                new Color(0.95f, 0.52f, 0.18f, 1f),
                new Color(0.38f, 0.38f, 0.80f, 1f),
                new Color(0.78f, 0.28f, 0.78f, 1f)
            };

            stationVisuals.Clear();
            stationColliderMap.Clear();

            for (int i = 0; i < controller.Stations.Count && i < positions.Length; i++)
            {
                StationRuntime station = controller.Stations[i];
                StationVisual visual = CreateStation(station, positions[i], colors[i]);
                stationVisuals.Add(visual);
            }
        }

        private StationVisual CreateStation(StationRuntime station, Vector3 position, Color color)
        {
            GameObject root = new GameObject("Station_" + station.Definition.Id);
            root.transform.SetParent(worldRoot, false);
            root.transform.position = position;

            GameObject selectionBase = CreateCube(root.transform, "SelectionBase", new Vector3(0f, -0.32f, 0f), new Vector3(1.48f, 0.05f, 1.20f), new Color(1f, 0.87f, 0.26f, 1f));
            Destroy(selectionBase.GetComponent<Collider>());

            GameObject baseObj = CreateCube(root.transform, "Base", Vector3.zero, new Vector3(1.18f, 0.66f, 0.96f), color);
            MapStationCollider(baseObj, station.Definition.Id);

            GameObject top = CreateCube(root.transform, "TableTop", new Vector3(0f, 0.39f, 0f), new Vector3(1.34f, 0.11f, 1.10f), Color.white);
            MapStationCollider(top, station.Definition.Id);

            GameObject machine = CreateCube(root.transform, "Machine", new Vector3(0f, 0.74f, 0.18f), new Vector3(0.58f, 0.42f, 0.36f), new Color(0.08f, 0.10f, 0.16f, 1f));
            GameObject device = CreateCube(root.transform, "Device", new Vector3(0f, 0.74f, -0.28f), new Vector3(0.46f, 0.07f, 0.32f), new Color(0.03f, 0.04f, 0.08f, 1f));
            Destroy(device.GetComponent<Collider>());

            GameObject progressBack = CreateCube(root.transform, "ProgressBack", new Vector3(0f, 0.98f, -0.58f), new Vector3(0.88f, 0.065f, 0.06f), new Color(0.66f, 0.72f, 0.82f, 1f));
            Destroy(progressBack.GetComponent<Collider>());
            GameObject progressFill = CreateCube(progressBack.transform, "ProgressFill", new Vector3(-0.42f, 0.02f, 0f), new Vector3(0.04f, 1.18f, 1.18f), new Color(0.20f, 0.78f, 0.48f, 1f));
            Destroy(progressFill.GetComponent<Collider>());

            Transform technician = CreateTechnician(root.transform, new Vector3(0.60f, 0.45f, 0.52f), color);
            Transform arm = technician.Find("Arm");

            TextMesh label = CreateSmallLabel(station.Definition.Title, new Vector3(position.x, 1.30f, position.z - 0.62f), 0.042f, Color.white);
            label.transform.SetParent(root.transform, true);

            return new StationVisual
            {
                Id = station.Definition.Id,
                Root = root.transform,
                BaseRenderer = baseObj.GetComponent<Renderer>(),
                MachineRenderer = machine.GetComponent<Renderer>(),
                TopRenderer = top.GetComponent<Renderer>(),
                ProgressFill = progressFill.transform,
                Device = device.transform,
                SelectionBase = selectionBase.transform,
                Technician = technician,
                TechnicianArm = arm,
                Label = label,
                BaseColor = color
            };
        }

        private Transform CreateTechnician(Transform parent, Vector3 localPosition, Color shirtColor)
        {
            GameObject tech = new GameObject("Technician");
            tech.transform.SetParent(parent, false);
            tech.transform.localPosition = localPosition;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(tech.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            body.transform.localScale = new Vector3(0.18f, 0.26f, 0.18f);
            SetColor(body, shirtColor);
            Destroy(body.GetComponent<Collider>());

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(tech.transform, false);
            head.transform.localPosition = new Vector3(0f, 0.78f, 0f);
            head.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            SetColor(head, new Color(0.96f, 0.72f, 0.52f, 1f));
            Destroy(head.GetComponent<Collider>());

            GameObject arm = CreateCube(tech.transform, "Arm", new Vector3(-0.16f, 0.48f, -0.18f), new Vector3(0.10f, 0.10f, 0.42f), new Color(0.96f, 0.72f, 0.52f, 1f));
            Destroy(arm.GetComponent<Collider>());

            return tech.transform;
        }

        private void BuildClients()
        {
            GameObject group = new GameObject("Clients");
            group.transform.SetParent(worldRoot, false);
            clientsRoot = group.transform;

            clients.Clear();
            for (int i = 0; i < 7; i++)
            {
                GameObject client = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                client.name = "Client_" + i;
                client.transform.SetParent(clientsRoot, false);
                client.transform.localScale = new Vector3(0.22f, 0.34f, 0.22f);
                SetColor(client, ClientColor(i));
                Destroy(client.GetComponent<Collider>());
                clients.Add(client.transform);

                GameObject phone = CreateCube(client.transform, "Phone", new Vector3(0.22f, 0.04f, 0.02f), new Vector3(0.10f, 0.14f, 0.035f), new Color(0.02f, 0.03f, 0.07f, 1f));
                Destroy(phone.GetComponent<Collider>());
            }
        }

        private void HandleSelectionInput()
        {
            if (worldCamera == null) return;

            bool pressed = false;
            Vector2 screenPosition = Vector2.zero;

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                pressed = true;
                screenPosition = Input.mousePosition;
            }
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Touch touch = Input.GetTouch(0);
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
                pressed = true;
                screenPosition = touch.position;
            }

            if (!pressed) return;

            Ray ray = worldCamera.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
            if (hits == null || hits.Length == 0) return;

            float bestDistance = float.MaxValue;
            string bestStation = null;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null) continue;
                if (!stationColliderMap.TryGetValue(hit.collider, out string stationId)) continue;

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestStation = stationId;
                }
            }

            if (!string.IsNullOrEmpty(bestStation))
                controller.SelectStation(bestStation);
        }

        private void AnimateClients()
        {
            if (clients.Count == 0 || controller == null) return;

            float speed = 0.09f + Mathf.Clamp01((float)(controller.CurrentIncomePerSecond / 600.0)) * 0.11f;
            for (int i = 0; i < clients.Count; i++)
            {
                Transform client = clients[i];
                float t = Mathf.Repeat(Time.time * speed + i * 0.145f, 1f);
                Vector3 a = new Vector3(-4.90f, 0.36f, -2.70f);
                Vector3 b = new Vector3(-2.95f, 0.36f, -0.40f);
                Vector3 c = new Vector3(0.70f, 0.36f, -0.16f);
                Vector3 d = new Vector3(4.30f, 0.36f, -1.05f);

                Vector3 pos;
                if (t < 0.32f) pos = Vector3.Lerp(a, b, t / 0.32f);
                else if (t < 0.70f) pos = Vector3.Lerp(b, c, (t - 0.32f) / 0.38f);
                else pos = Vector3.Lerp(c, d, (t - 0.70f) / 0.30f);

                pos.y += Mathf.Sin(Time.time * 8f + i) * 0.025f;
                client.position = pos;
                client.rotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 3f + i) * 8f, 0f);
            }
        }

        private void RefreshStations()
        {
            foreach (StationVisual visual in stationVisuals)
            {
                StationRuntime station = controller.GetStation(visual.Id);
                if (station == null) continue;

                bool selected = controller.SelectedStationId == visual.Id;
                bool unlocked = station.Save.Unlocked;
                float progress = station.NormalizedProgress();

                Color baseColor = unlocked ? visual.BaseColor : new Color(0.40f, 0.43f, 0.50f, 1f);
                if (selected) baseColor = Color.Lerp(baseColor, Color.white, 0.28f);

                SetRendererColor(visual.BaseRenderer, baseColor);
                SetRendererColor(visual.TopRenderer, unlocked ? Color.white : new Color(0.78f, 0.80f, 0.86f, 1f));
                SetRendererColor(visual.MachineRenderer, unlocked ? new Color(0.08f, 0.10f, 0.16f, 1f) : new Color(0.20f, 0.22f, 0.28f, 1f));

                if (visual.SelectionBase != null)
                    visual.SelectionBase.gameObject.SetActive(selected);

                if (visual.ProgressFill != null)
                {
                    visual.ProgressFill.gameObject.SetActive(unlocked);
                    float width = Mathf.Lerp(0.04f, 0.82f, progress);
                    visual.ProgressFill.localScale = new Vector3(width, 1.18f, 1.18f);
                    visual.ProgressFill.localPosition = new Vector3(-0.42f + width * 0.5f, 0.02f, 0f);
                }

                if (visual.Device != null)
                {
                    visual.Device.gameObject.SetActive(unlocked);
                    visual.Device.localPosition = new Vector3(0f, 0.74f + Mathf.Sin(Time.time * 8f + visual.Root.GetSiblingIndex()) * 0.020f, -0.28f);
                    visual.Device.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 3.5f) * 4f, 0f);
                }

                if (visual.Technician != null)
                {
                    visual.Technician.gameObject.SetActive(unlocked);
                    visual.Technician.localPosition = new Vector3(0.60f, 0.45f + Mathf.Sin(Time.time * 5f + visual.Root.GetSiblingIndex()) * 0.018f, 0.52f);
                }

                if (visual.TechnicianArm != null)
                    visual.TechnicianArm.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 7f + visual.Root.GetSiblingIndex()) * 18f, 0f);

                if (visual.Root != null)
                {
                    float pulse = selected ? 1f + Mathf.Sin(Time.time * 5f) * 0.018f : 1f;
                    visual.Root.localScale = Vector3.one * pulse;
                }

                if (visual.Label != null)
                {
                    visual.Label.gameObject.SetActive(selected);
                    visual.Label.text = unlocked
                        ? station.Definition.Title + "  Nv. " + station.Save.Level
                        : station.Definition.Title + " bloqueado";
                    visual.Label.color = unlocked ? Color.white : new Color(0.86f, 0.88f, 0.94f, 1f);
                    if (worldCamera != null) visual.Label.transform.rotation = worldCamera.transform.rotation;
                }
            }
        }

        private GameObject CreateCube(string name, Vector3 position, Vector3 scale, Color color)
        {
            return CreateCube(worldRoot, name, position, scale, color);
        }

        private GameObject CreateCube(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localScale = scale;
            SetColor(obj, color);
            return obj;
        }

        private TextMesh CreateSmallLabel(string text, Vector3 position, float characterSize, Color color)
        {
            GameObject obj = new GameObject("Label_" + text);
            obj.transform.SetParent(worldRoot, false);
            obj.transform.position = position;
            if (worldCamera != null) obj.transform.rotation = worldCamera.transform.rotation;

            TextMesh mesh = obj.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = characterSize;
            mesh.fontSize = 24;
            mesh.color = color;
            return mesh;
        }

        private void MapStationCollider(GameObject obj, string stationId)
        {
            Collider collider = obj.GetComponent<Collider>();
            if (collider != null) stationColliderMap[collider] = stationId;
        }

        private void SetColor(GameObject obj, Color color)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            SetRendererColor(renderer, color);
        }

        private void SetRendererColor(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            renderer.material.color = color;
        }

        private Color ClientColor(int index)
        {
            Color[] palette =
            {
                new Color(0.12f, 0.48f, 0.95f, 1f),
                new Color(0.11f, 0.62f, 0.38f, 1f),
                new Color(0.94f, 0.48f, 0.20f, 1f),
                new Color(0.66f, 0.34f, 0.86f, 1f),
                new Color(0.12f, 0.60f, 0.72f, 1f)
            };
            return palette[Mathf.Abs(index) % palette.Length];
        }
    }
}
