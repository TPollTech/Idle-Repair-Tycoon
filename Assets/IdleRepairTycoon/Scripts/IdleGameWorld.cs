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
            public float LastProgress = -1f;
            public float FlashTimer;
            public Transform LockedSlot;
            public TextMesh LockedLabel;
            public float UnlockTimer;
        }

        private sealed class FloatingText3D
        {
            public TextMesh Text;
            public Transform Transform;
            public float Age;
            public float Life;
            public Vector3 Velocity;
        }

        private readonly List<FloatingText3D> floatingTexts3D = new List<FloatingText3D>();
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
            AnimateFloatingTexts3D();
        }

        private void BuildWorld()
        {
            GameObject root = new GameObject("IdleRepairWorld");
            root.transform.SetParent(transform, false);
            worldRoot = root.transform;

            BuildCamera();
            BuildLights();
            BuildRoom();
            BuildDecor();
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

        private void BuildDecor()
        {
            CreatePoster(new Vector3(-3.0f, 1.55f, 3.55f), new Color(0.95f, 0.38f, 0.22f, 1f));
            CreatePoster(new Vector3(0.0f, 1.60f, 3.55f), new Color(0.20f, 0.66f, 0.44f, 1f));
            CreatePoster(new Vector3(3.0f, 1.55f, 3.55f), new Color(0.22f, 0.48f, 0.94f, 1f));

            for (int i = 0; i < 4; i++)
            {
                float x = -4.1f + (i * 1.05f);
                CreateCube("ShelfPart" + i + "a", new Vector3(x - 0.22f, 1.44f, 3.38f), new Vector3(0.12f, 0.08f, 0.08f), new Color(0.94f, 0.74f, 0.30f, 1f));
                CreateCube("ShelfPart" + i + "b", new Vector3(x + 0.20f, 1.40f, 3.38f), new Vector3(0.10f, 0.10f, 0.08f), new Color(0.30f, 0.54f, 0.92f, 1f));
            }

            CreateChair(new Vector3(-5.25f, 0.0f, -2.10f), Quaternion.Euler(0f, 90f, 0f));
            CreateChair(new Vector3(-5.25f, 0.0f, -0.90f), Quaternion.Euler(0f, 90f, 0f));

            CreateCube("CounterMonitor", new Vector3(-2.95f, 1.00f, 0.55f), new Vector3(0.40f, 0.30f, 0.04f), new Color(0.04f, 0.06f, 0.12f, 1f));
            CreateCube("CounterStand", new Vector3(-2.95f, 0.84f, 0.55f), new Vector3(0.06f, 0.04f, 0.06f), new Color(0.50f, 0.52f, 0.56f, 1f));

            CreateClock();
            CreateHangingSign();

            GameObject cashierScreen = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cashierScreen.name = "CashierScreenCyl";
            cashierScreen.transform.SetParent(worldRoot, false);
            cashierScreen.transform.position = new Vector3(4.28f, 1.18f, -0.82f);
            cashierScreen.transform.localScale = new Vector3(0.28f, 0.02f, 0.28f);
            cashierScreen.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            SetColor(cashierScreen, new Color(0.15f, 0.45f, 0.85f, 1f));
            Destroy(cashierScreen.GetComponent<Collider>());
        }

        private void CreatePoster(Vector3 position, Color color)
        {
            CreateCube("Poster", position, new Vector3(0.72f, 0.52f, 0.02f), color);
        }

        private void CreateChair(Vector3 position, Quaternion rotation)
        {
            GameObject chair = new GameObject("Chair");
            chair.transform.SetParent(worldRoot, false);
            chair.transform.position = position;
            chair.transform.rotation = rotation;

            CreateCube(chair.transform, "Seat", new Vector3(0f, 0.28f, 0f), new Vector3(0.40f, 0.08f, 0.40f), new Color(0.14f, 0.16f, 0.24f, 1f));
            CreateCube(chair.transform, "Back", new Vector3(0f, 0.56f, -0.20f), new Vector3(0.38f, 0.48f, 0.06f), new Color(0.14f, 0.16f, 0.24f, 1f));
            CreateCube(chair.transform, "Leg1", new Vector3(-0.16f, 0.06f, -0.16f), new Vector3(0.04f, 0.12f, 0.04f), new Color(0.08f, 0.10f, 0.16f, 1f));
            CreateCube(chair.transform, "Leg2", new Vector3(0.16f, 0.06f, -0.16f), new Vector3(0.04f, 0.12f, 0.04f), new Color(0.08f, 0.10f, 0.16f, 1f));
            CreateCube(chair.transform, "Leg3", new Vector3(-0.16f, 0.06f, 0.16f), new Vector3(0.04f, 0.12f, 0.04f), new Color(0.08f, 0.10f, 0.16f, 1f));
            CreateCube(chair.transform, "Leg4", new Vector3(0.16f, 0.06f, 0.16f), new Vector3(0.04f, 0.12f, 0.04f), new Color(0.08f, 0.10f, 0.16f, 1f));
        }

        private void CreateClock()
        {
            GameObject clock = new GameObject("WallClock");
            clock.transform.SetParent(worldRoot, false);
            clock.transform.position = new Vector3(-1.8f, 1.80f, 3.55f);

            GameObject face = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            face.name = "ClockFace";
            face.transform.SetParent(clock.transform, false);
            face.transform.localPosition = Vector3.zero;
            face.transform.localScale = new Vector3(0.24f, 0.015f, 0.24f);
            face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            SetColor(face, Color.white);
            Destroy(face.GetComponent<Collider>());

            CreateCube(clock.transform, "HourHand", new Vector3(0f, 0.03f, -0.06f), new Vector3(0.02f, 0.01f, 0.08f), new Color(0.06f, 0.08f, 0.14f, 1f));
            CreateCube(clock.transform, "MinuteHand", new Vector3(0.10f, 0.03f, 0f), new Vector3(0.14f, 0.01f, 0.02f), new Color(0.06f, 0.08f, 0.14f, 1f));
        }

        private void CreateHangingSign()
        {
            CreateCube("HangingSign", new Vector3(-4.15f, 1.85f, -3.20f), new Vector3(0.85f, 0.10f, 0.04f), new Color(0.95f, 0.80f, 0.12f, 1f));
            CreateCube("SignWireL", new Vector3(-4.45f, 1.60f, -3.20f), new Vector3(0.02f, 0.40f, 0.02f), new Color(0.50f, 0.52f, 0.56f, 1f));
            CreateCube("SignWireR", new Vector3(-3.85f, 1.60f, -3.20f), new Vector3(0.02f, 0.40f, 0.02f), new Color(0.50f, 0.52f, 0.56f, 1f));
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

            GameObject lockedSlot = new GameObject("LockedSlot");
            lockedSlot.transform.SetParent(root.transform, false);
            GameObject slotBase = CreateCube(lockedSlot.transform, "SlotBase", Vector3.zero, new Vector3(1.18f, 0.18f, 0.96f), new Color(0.52f, 0.56f, 0.66f, 0.55f));
            Destroy(slotBase.GetComponent<Collider>());
            GameObject slotClick = CreateCube(lockedSlot.transform, "SlotClick", new Vector3(0f, 0.18f, 0f), new Vector3(0.90f, 0.10f, 0.76f), new Color(0f, 0f, 0f, 0f));
            MapStationCollider(slotClick, station.Definition.Id);
            GameObject slotBorder = CreateCube(lockedSlot.transform, "SlotBorder", new Vector3(0f, 0.05f, 0f), new Vector3(1.24f, 0.02f, 1.02f), new Color(0.72f, 0.78f, 0.88f, 0.50f));
            Destroy(slotBorder.GetComponent<Collider>());

            TextMesh lockedLabel = CreateSmallLabel("?", new Vector3(position.x, 0.80f, position.z), 0.035f, new Color(0.72f, 0.78f, 0.88f, 0.85f));
            lockedLabel.transform.SetParent(lockedSlot.transform, true);

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
                BaseColor = color,
                LockedSlot = lockedSlot.transform,
                LockedLabel = lockedLabel
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
            {
                controller.SelectStation(bestStation);
                if (controller.Audio != null) controller.Audio.PlaySelect();
            }
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

                // --- Locked slot ---
                if (!unlocked)
                {
                    if (visual.LockedSlot != null) visual.LockedSlot.gameObject.SetActive(true);
                    if (visual.BaseRenderer != null) visual.BaseRenderer.enabled = false;
                    if (visual.TopRenderer != null) visual.TopRenderer.enabled = false;
                    if (visual.MachineRenderer != null) visual.MachineRenderer.enabled = false;
                    if (visual.ProgressFill != null) visual.ProgressFill.gameObject.SetActive(false);
                    if (visual.Device != null) visual.Device.gameObject.SetActive(false);
                    if (visual.Technician != null) visual.Technician.gameObject.SetActive(false);
                    if (visual.SelectionBase != null) visual.SelectionBase.gameObject.SetActive(false);
                    if (visual.Label != null) visual.Label.gameObject.SetActive(false);
                    if (visual.LockedLabel != null)
                    {
                        visual.LockedLabel.text = station.Definition.Emoji + "  " + station.Definition.Title + "\nR$ " + IdleGameBalance.FormatMoney(station.Definition.UnlockCost);
                        visual.LockedLabel.gameObject.SetActive(true);
                        if (worldCamera != null) visual.LockedLabel.transform.rotation = worldCamera.transform.rotation;
                    }
                    continue;
                }

                // --- Unlock pop-in animation ---
                if (visual.LockedSlot != null && visual.LockedSlot.gameObject.activeSelf)
                {
                    visual.UnlockTimer = 1f;
                    visual.Root.localScale = Vector3.one * 0.01f;
                }
                if (visual.UnlockTimer > 0f)
                {
                    visual.UnlockTimer -= Time.deltaTime * 2.5f;
                    float s = 1f - Mathf.Clamp01(visual.UnlockTimer);
                    s = s * s * (3f - 2f * s);
                    visual.Root.localScale = Vector3.one * Mathf.Lerp(0.01f, 1f, s);
                }

                // --- Unlocked: hide locked slot, show station ---
                if (visual.LockedSlot != null) visual.LockedSlot.gameObject.SetActive(false);
                if (visual.BaseRenderer != null) visual.BaseRenderer.enabled = true;
                if (visual.TopRenderer != null) visual.TopRenderer.enabled = true;
                if (visual.MachineRenderer != null) visual.MachineRenderer.enabled = true;

                if (unlocked && visual.LastProgress >= 0f && progress < visual.LastProgress - 0.005f)
                {
                    double profit = station.ProfitPerJob(controller.PrestigeMultiplier, controller.BoostMultiplier);
                    if (profit > 0) { SpawnFloatingIncome(visual, profit); if (controller.Audio != null) controller.Audio.PlayCoin(); }
                    visual.FlashTimer = 0.35f;
                }
                visual.LastProgress = progress;

                Color flashColor = visual.BaseColor;
                if (visual.FlashTimer > 0f)
                {
                    visual.FlashTimer -= Time.deltaTime;
                    float t = visual.FlashTimer / 0.35f;
                    flashColor = Color.Lerp(visual.BaseColor, Color.white, t * 0.7f);
                }

                Color baseColor = unlocked ? flashColor : new Color(0.40f, 0.43f, 0.50f, 1f);
                if (selected) baseColor = Color.Lerp(baseColor, Color.white, 0.28f);

                SetRendererColor(visual.BaseRenderer, baseColor);
                SetRendererColor(visual.TopRenderer, unlocked ? Color.white : new Color(0.78f, 0.80f, 0.86f, 1f));
                SetRendererColor(visual.MachineRenderer, unlocked ? new Color(0.08f, 0.10f, 0.16f, 1f) : new Color(0.20f, 0.22f, 0.28f, 1f));
                if (unlocked && visual.MachineRenderer != null)
                {
                    float levelScale = Mathf.Min(1f + (station.Level - 1) * 0.035f, 1.8f);
                    visual.MachineRenderer.transform.localScale = new Vector3(0.58f * levelScale, 0.42f * levelScale, 0.36f * levelScale);
                }

                if (visual.SelectionBase != null)
                    visual.SelectionBase.gameObject.SetActive(selected);

                if (visual.ProgressFill != null)
                {
                    visual.ProgressFill.gameObject.SetActive(unlocked);
                    float width = Mathf.Lerp(0.04f, 0.82f, progress);
                    visual.ProgressFill.localScale = new Vector3(width, 1.18f, 1.18f);
                    visual.ProgressFill.localPosition = new Vector3(-0.42f + width * 0.5f, 0.02f, 0f);
                    if (unlocked)
                    {
                        Renderer fillRenderer = visual.ProgressFill.GetComponent<Renderer>();
                        if (fillRenderer != null)
                        {
                            Color fillColor = progress < 0.5f
                                ? Color.Lerp(new Color(0.20f, 0.78f, 0.48f, 1f), new Color(0.95f, 0.82f, 0.12f, 1f), progress * 2f)
                                : Color.Lerp(new Color(0.95f, 0.82f, 0.12f, 1f), new Color(0.92f, 0.28f, 0.18f, 1f), (progress - 0.5f) * 2f);
                            SetRendererColor(fillRenderer, fillColor);
                        }
                    }
                }

                if (visual.Device != null)
                {
                    visual.Device.gameObject.SetActive(unlocked);
                    float bobSpeed = unlocked ? 8f + progress * 6f : 0f;
                    float bobAmount = unlocked ? 0.020f + progress * 0.025f : 0f;
                    visual.Device.localPosition = new Vector3(0f, 0.74f + Mathf.Sin(Time.time * bobSpeed + visual.Root.GetSiblingIndex()) * bobAmount, -0.28f);
                    float spinSpeed = unlocked ? 3.5f + progress * 4f : 0f;
                    float spinAmount = unlocked ? 4f + progress * 8f : 0f;
                    visual.Device.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * spinSpeed) * spinAmount, 0f);
                }

                if (visual.Technician != null)
                {
                    visual.Technician.gameObject.SetActive(unlocked);
                    float techBob = unlocked ? 0.018f + progress * 0.022f : 0f;
                    visual.Technician.localPosition = new Vector3(0.60f, 0.45f + Mathf.Sin(Time.time * 5f + visual.Root.GetSiblingIndex()) * techBob, 0.52f);
                }

                if (visual.TechnicianArm != null)
                {
                    float armSpeed = unlocked ? 7f + progress * 5f : 7f;
                    float armAngle = unlocked ? 18f + progress * 20f : 18f;
                    visual.TechnicianArm.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * armSpeed + visual.Root.GetSiblingIndex()) * armAngle, 0f);
                }

                if (visual.Root != null && visual.UnlockTimer <= 0f)
                {
                    float pulse = selected ? 1f + Mathf.Sin(Time.time * 5f) * 0.018f : 1f;
                    visual.Root.localScale = Vector3.one * pulse;
                }

                if (visual.Label != null)
                {
                    visual.Label.gameObject.SetActive(selected);
                    visual.Label.text = unlocked
                        ? station.Definition.Emoji + " " + station.Definition.Title + "  Nv. " + station.Save.Level
                        : station.Definition.Emoji + " " + station.Definition.Title + " bloqueado";
                    visual.Label.color = unlocked ? Color.white : new Color(0.86f, 0.88f, 0.94f, 1f);
                    if (worldCamera != null) visual.Label.transform.rotation = worldCamera.transform.rotation;
                }
            }
        }

        private void AnimateFloatingTexts3D()
        {
            for (int i = floatingTexts3D.Count - 1; i >= 0; i--)
            {
                FloatingText3D ft = floatingTexts3D[i];
                if (ft == null || ft.Transform == null)
                {
                    floatingTexts3D.RemoveAt(i);
                    continue;
                }
                ft.Age += Time.deltaTime;
                ft.Transform.position += ft.Velocity * Time.deltaTime;
                float alpha = Mathf.Clamp01(1f - ft.Age / ft.Life);
                Color c = ft.Text.color;
                ft.Text.color = new Color(c.r, c.g, c.b, alpha);
                if (ft.Age >= ft.Life)
                {
                    Object.Destroy(ft.Transform.gameObject);
                    floatingTexts3D.RemoveAt(i);
                }
            }
        }

        private void SpawnFloatingIncome(StationVisual visual, double amount)
        {
            GameObject obj = new GameObject("FloatingIncome");
            obj.transform.SetParent(worldRoot, false);
            obj.transform.position = visual.Root.position + new Vector3(0f, 1.2f, 0f);
            if (worldCamera != null) obj.transform.rotation = worldCamera.transform.rotation;

            TextMesh text = obj.AddComponent<TextMesh>();
            text.text = "+" + IdleGameBalance.FormatMoney(amount);
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.048f;
            text.fontSize = 28;
            text.color = new Color(0.14f, 0.90f, 0.42f, 1f);

            floatingTexts3D.Add(new FloatingText3D
            {
                Text = text,
                Transform = obj.transform,
                Age = 0f,
                Life = 1.4f,
                Velocity = new Vector3(0f, 0.45f, 0f)
            });
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
