using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IdleRepairTycoon
{
    public sealed class IdleGameUI : MonoBehaviour
    {
        private sealed class StationCard
        {
            public string Id;
            public Text Title;
            public Text Description;
            public Text Reward;
            public Text ButtonText;
            public Image ProgressFill;
            public Button ActionButton;
        }

        private sealed class FloatingText
        {
            public RectTransform Rect;
            public Text Text;
            public float Age;
            public float Life;
            public Vector2 Velocity;
        }

        private IdleGameController controller;
        private Canvas canvas;
        private Font font;
        private RectTransform popupLayer;
        private RectTransform clientLayer;
        private Text cashText;
        private Text incomeText;
        private Text boostText;
        private Text reputationText;
        private Text toastText;
        private Text offlineText;
        private GameObject offlinePanel;
        private Button prestigeButton;
        private Text prestigeButtonText;
        private double lastOfflineIncome;
        private double lastObservedCash;
        private bool hasCashSnapshot;
        private float cashPopupCooldown;
        private float toastTimer;
        private readonly List<StationCard> stationCards = new List<StationCard>();
        private readonly List<RectTransform> clients = new List<RectTransform>();
        private readonly List<FloatingText> floaters = new List<FloatingText>();

        public void Bind(IdleGameController gameController)
        {
            controller = gameController;
            controller.OnStateChanged += Refresh;
            controller.OnToast += ShowToast;
            controller.OnOfflineIncomeCalculated += ShowOfflinePanel;

            BuildUI();
            lastObservedCash = controller.Save.Cash;
            hasCashSnapshot = true;
            Refresh();
        }

        private void Update()
        {
            AnimateClients();
            AnimateFloaters();
            WatchCashDelta();

            if (cashPopupCooldown > 0f) cashPopupCooldown -= Time.deltaTime;
            if (toastTimer > 0f)
            {
                toastTimer -= Time.deltaTime;
                if (toastTimer <= 0f && toastText != null)
                    toastText.transform.parent.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (controller == null) return;
            controller.OnStateChanged -= Refresh;
            controller.OnToast -= ShowToast;
            controller.OnOfflineIncomeCalculated -= ShowOfflinePanel;
        }

        private void BuildUI()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            CreateEventSystemIfNeeded();

            GameObject canvasObject = new GameObject("IdleRepairCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = canvasObject.GetComponent<RectTransform>();
            Stretch(root);
            canvasObject.AddComponent<Image>().color = new Color(0.92f, 0.95f, 1f, 1f);

            RectTransform safe = CreatePanel("SafeArea", root, new Color(0f, 0f, 0f, 0f));
            Stretch(safe);

            VerticalLayoutGroup mainLayout = safe.gameObject.AddComponent<VerticalLayoutGroup>();
            mainLayout.padding = new RectOffset(28, 28, 28, 28);
            mainLayout.spacing = 16;
            mainLayout.childAlignment = TextAnchor.UpperCenter;
            mainLayout.childForceExpandWidth = true;
            mainLayout.childForceExpandHeight = false;

            BuildHeader(safe);
            BuildWorkshopVisual(safe);
            BuildStationList(safe);
            BuildBottomActions(safe);
            BuildOfflinePanel(root);
            BuildToast(root);

            popupLayer = CreatePanel("PopupLayer", root, new Color(0f, 0f, 0f, 0f));
            Stretch(popupLayer);
        }

        private void BuildHeader(RectTransform parent)
        {
            RectTransform header = CreatePanel("Header", parent, new Color(0.07f, 0.10f, 0.19f, 1f));
            AddLayout(header, -1, 250);

            VerticalLayoutGroup layout = header.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 18, 18);
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text title = CreateText("Title", header, "Idle Assistência", 48, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            AddLayout(title.rectTransform, -1, 58);

            cashText = CreateText("Cash", header, "R$ 0", 64, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.70f, 0.92f, 1f, 1f));
            AddLayout(cashText.rectTransform, -1, 72);

            RectTransform row = CreatePanel("StatsRow", header, new Color(0f, 0f, 0f, 0f));
            AddLayout(row, -1, 58);

            HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;

            incomeText = CreatePill(row, "+R$ 0/s");
            reputationText = CreatePill(row, "Rep. 0");
            boostText = CreatePill(row, "Turbo off");
        }

        private void BuildWorkshopVisual(RectTransform parent)
        {
            RectTransform area = CreatePanel("Workshop", parent, new Color(0.84f, 0.90f, 0.98f, 1f));
            AddLayout(area, -1, 560);

            Text sign = CreateText("Sign", area, "OFICINA EM MOVIMENTO", 34, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.08f, 0.12f, 0.22f, 1f));
            Anchor(sign.rectTransform, 0.05f, 0.84f, 0.95f, 0.97f);

            RectTransform wall = CreatePanel("InfoWall", area, Color.white);
            Anchor(wall, 0.06f, 0.52f, 0.94f, 0.80f);
            Text info = CreateText("Info", wall, "Clientes chegam, deixam aparelhos, as bancadas consertam e o caixa gira sozinho.", 27, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.25f, 0.30f, 0.42f, 1f));
            Stretch(info.rectTransform, 18, 18, 12, 12);

            CreateWorkshopBlock(area, "CLIENTES", 0.06f, 0.14f, 0.24f, 0.42f, new Color(0.18f, 0.56f, 0.95f, 1f));
            CreateWorkshopBlock(area, "BALCÃO", 0.30f, 0.14f, 0.48f, 0.42f, new Color(0.12f, 0.68f, 0.44f, 1f));
            CreateWorkshopBlock(area, "BANCADAS", 0.54f, 0.14f, 0.75f, 0.42f, new Color(0.96f, 0.62f, 0.16f, 1f));
            CreateWorkshopBlock(area, "CAIXA", 0.81f, 0.14f, 0.94f, 0.42f, new Color(0.70f, 0.34f, 0.92f, 1f));

            RectTransform line = CreatePanel("FlowLine", area, new Color(0.12f, 0.17f, 0.28f, 1f));
            Anchor(line, 0.14f, 0.08f, 0.88f, 0.105f);

            clientLayer = CreatePanel("Clients", area, new Color(0f, 0f, 0f, 0f));
            Stretch(clientLayer);
            clients.Clear();
            for (int i = 0; i < 7; i++)
            {
                RectTransform client = CreatePanel("Client" + i, clientLayer, Color.white);
                client.sizeDelta = new Vector2(56, 56);
                Text face = CreateText("Face", client, "C", 23, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.08f, 0.12f, 0.22f, 1f));
                Stretch(face.rectTransform);
                clients.Add(client);
            }
        }

        private void BuildStationList(RectTransform parent)
        {
            RectTransform scrollRoot = CreatePanel("StationScroll", parent, new Color(0f, 0f, 0f, 0f));
            AddLayout(scrollRoot, -1, 0, 1f);

            ScrollRect scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 32;
            scroll.movementType = ScrollRect.MovementType.Elastic;

            RectTransform viewport = CreatePanel("Viewport", scrollRoot, new Color(0f, 0f, 0f, 0.01f));
            Stretch(viewport);
            Mask mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            RectTransform content = CreatePanel("Content", viewport, new Color(0f, 0f, 0f, 0f));
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;

            stationCards.Clear();
            foreach (StationRuntime station in controller.Stations)
                stationCards.Add(CreateStationCard(content, station));
        }

        private StationCard CreateStationCard(RectTransform parent, StationRuntime station)
        {
            RectTransform card = CreatePanel("Station_" + station.Definition.Id, parent, Color.white);
            AddLayout(card, -1, 188);

            HorizontalLayoutGroup row = card.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(18, 18, 16, 16);
            row.spacing = 16;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = true;

            RectTransform icon = CreatePanel("Icon", card, new Color(0.09f, 0.14f, 0.24f, 1f));
            AddLayout(icon, 118, -1);
            Text emoji = CreateText("Emoji", icon, station.Definition.Emoji, 44, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Stretch(emoji.rectTransform);

            RectTransform body = CreatePanel("Body", card, new Color(0f, 0f, 0f, 0f));
            AddLayout(body, 0, -1, 1f);

            VerticalLayoutGroup bodyLayout = body.gameObject.AddComponent<VerticalLayoutGroup>();
            bodyLayout.spacing = 6;
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = true;
            bodyLayout.childForceExpandHeight = false;

            Text title = CreateText("Title", body, station.Definition.Title, 28, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.09f, 0.12f, 0.20f, 1f));
            AddLayout(title.rectTransform, -1, 34);
            Text description = CreateText("Description", body, station.Definition.Description, 20, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.35f, 0.40f, 0.52f, 1f));
            AddLayout(description.rectTransform, -1, 28);
            Text reward = CreateText("Reward", body, "", 22, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.10f, 0.54f, 0.33f, 1f));
            AddLayout(reward.rectTransform, -1, 28);

            RectTransform bar = CreatePanel("ProgressBar", body, new Color(0.82f, 0.86f, 0.93f, 1f));
            AddLayout(bar, -1, 18);
            Image fill = CreateImage("Fill", bar, new Color(0.18f, 0.56f, 0.95f, 1f));
            fill.rectTransform.anchorMin = new Vector2(0, 0);
            fill.rectTransform.anchorMax = new Vector2(0, 1);
            fill.rectTransform.pivot = new Vector2(0, 0.5f);
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;

            Button button = CreateButton(card, "Melhorar", new Color(0.18f, 0.56f, 0.95f, 1f), null);
            AddLayout(button.GetComponent<RectTransform>(), 245, -1);
            Text buttonText = button.GetComponentInChildren<Text>();
            string id = station.Definition.Id;
            button.onClick.AddListener(delegate
            {
                StationRuntime target = FindStation(id);
                if (target == null) return;
                if (target.Save.Unlocked) controller.UpgradeStation(id);
                else controller.UnlockStation(id);
            });

            return new StationCard
            {
                Id = id,
                Title = title,
                Description = description,
                Reward = reward,
                ButtonText = buttonText,
                ProgressFill = fill,
                ActionButton = button
            };
        }

        private void BuildBottomActions(RectTransform parent)
        {
            RectTransform actions = CreatePanel("BottomActions", parent, new Color(0.07f, 0.10f, 0.19f, 1f));
            AddLayout(actions, -1, 150);

            HorizontalLayoutGroup layout = actions.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 14;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            CreateButton(actions, "Turbo 2x", new Color(0.18f, 0.56f, 0.95f, 1f), () => controller.WatchAdForBoost());
            CreateButton(actions, "Pacote R$", new Color(0.12f, 0.64f, 0.38f, 1f), () => controller.WatchAdForMoneyPackage());
            prestigeButton = CreateButton(actions, "Prestígio", new Color(0.72f, 0.42f, 0.16f, 1f), () => controller.Prestige());
            prestigeButtonText = prestigeButton.GetComponentInChildren<Text>();
        }

        private void BuildOfflinePanel(RectTransform root)
        {
            offlinePanel = CreatePanel("OfflinePanel", root, new Color(0f, 0f, 0f, 0.72f)).gameObject;
            Stretch(offlinePanel.GetComponent<RectTransform>());
            offlinePanel.SetActive(false);

            RectTransform modal = CreatePanel("OfflineModal", offlinePanel.transform, Color.white);
            Anchor(modal, 0.08f, 0.34f, 0.92f, 0.66f);

            VerticalLayoutGroup layout = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 26, 26);
            layout.spacing = 14;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;

            Text title = CreateText("Title", modal, "Enquanto você estava fora", 28, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.08f, 0.12f, 0.22f, 1f));
            AddLayout(title.rectTransform, -1, 40);
            offlineText = CreateText("OfflineText", modal, "Você ganhou R$ 0", 23, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.18f, 0.22f, 0.33f, 1f));
            AddLayout(offlineText.rectTransform, -1, 64);

            RectTransform row = CreatePanel("Buttons", modal, new Color(0f, 0f, 0f, 0f));
            AddLayout(row, -1, 64);
            HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;

            CreateButton(row, "Receber", new Color(0.45f, 0.48f, 0.56f, 1f), () => offlinePanel.SetActive(false));
            CreateButton(row, "Dobrar anúncio", new Color(0.12f, 0.64f, 0.38f, 1f), () =>
            {
                offlinePanel.SetActive(false);
                controller.DoubleOfflineIncomeWithAd(lastOfflineIncome);
            });
        }

        private void BuildToast(RectTransform root)
        {
            RectTransform panel = CreatePanel("ToastPanel", root, new Color(0.04f, 0.05f, 0.08f, 0.90f));
            panel.anchorMin = new Vector2(0.08f, 0.08f);
            panel.anchorMax = new Vector2(0.92f, 0.08f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(0, 68);
            panel.anchoredPosition = Vector2.zero;
            toastText = CreateText("Toast", panel, "", 21, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Stretch(toastText.rectTransform, 12, 12, 6, 6);
            panel.gameObject.SetActive(false);
        }

        private void ShowOfflinePanel(double amount, bool capped)
        {
            lastOfflineIncome = amount;
            offlineText.text = "Você ganhou " + IdleGameBalance.FormatMoney(amount) + (capped ? "\nLimite offline: 8 horas." : "");
            offlinePanel.SetActive(true);
        }

        private void Refresh()
        {
            if (controller == null || controller.Save == null || cashText == null) return;

            cashText.text = IdleGameBalance.FormatMoney(controller.Save.Cash);
            incomeText.text = "+" + IdleGameBalance.FormatMoney(controller.CurrentIncomePerSecond) + "/s";
            reputationText.text = "Rep. " + controller.Save.ReputationStars + "  x" + controller.PrestigeMultiplier.ToString("0.00");
            boostText.text = controller.HasActiveBoost ? "Turbo " + FormatTime(controller.BoostSecondsRemaining) : "Turbo off";

            if (prestigeButton != null)
            {
                prestigeButton.interactable = controller.CanPrestige;
                if (prestigeButtonText != null)
                    prestigeButtonText.text = controller.CanPrestige ? "Prestígio\nDisponível" : "Prestígio\n" + IdleGameBalance.FormatMoney(IdleGameBalance.PrestigeCost);
            }

            foreach (StationCard card in stationCards)
            {
                StationRuntime station = FindStation(card.Id);
                if (station == null) continue;

                bool unlocked = station.Save.Unlocked;
                double cost = unlocked ? station.UpgradeCost() : station.Definition.UnlockCost;
                double previewReward = station.ProfitPerJob(controller.PrestigeMultiplier, controller.BoostMultiplier);

                card.Title.text = unlocked ? station.Definition.Title + "  Nv. " + station.Save.Level : station.Definition.Title + "  bloqueado";
                card.Description.text = unlocked ? station.Definition.Description : "Liberar para ativar esta bancada.";
                card.Reward.text = unlocked
                    ? "Ganha " + IdleGameBalance.FormatMoney(previewReward) + " a cada " + station.DurationSeconds().ToString("0.0") + "s"
                    : "Custo: " + IdleGameBalance.FormatMoney(cost);

                card.ProgressFill.rectTransform.anchorMax = new Vector2(station.NormalizedProgress(), 1f);
                card.ButtonText.text = unlocked ? "Melhorar\n" + IdleGameBalance.FormatMoney(cost) : "Liberar\n" + IdleGameBalance.FormatMoney(cost);
                card.ActionButton.interactable = controller.Save.Cash >= cost;
            }
        }

        private void ShowToast(string message)
        {
            if (toastText == null) return;
            toastText.text = message;
            toastText.transform.parent.gameObject.SetActive(true);
            toastText.transform.parent.SetAsLastSibling();
            toastTimer = 2.2f;
        }

        private void WatchCashDelta()
        {
            if (controller == null || controller.Save == null) return;
            double current = controller.Save.Cash;
            if (!hasCashSnapshot)
            {
                lastObservedCash = current;
                hasCashSnapshot = true;
                return;
            }

            double delta = current - lastObservedCash;
            if (delta < 0)
            {
                lastObservedCash = current;
                return;
            }

            if (delta >= 1d && cashPopupCooldown <= 0f)
            {
                CreateFloatingText("+" + IdleGameBalance.FormatMoney(delta), new Vector2(Random.Range(-260f, 260f), Random.Range(120f, 420f)), new Vector2(Random.Range(-12f, 12f), 110f), new Color(0.09f, 0.55f, 0.31f, 1f), 34, 1.15f);
                lastObservedCash = current;
                cashPopupCooldown = 0.35f;
            }
        }

        private StationRuntime FindStation(string id)
        {
            foreach (StationRuntime station in controller.Stations)
                if (station.Definition.Id == id) return station;
            return null;
        }

        private void AnimateClients()
        {
            if (clientLayer == null || controller == null) return;
            bool active = controller.CurrentIncomePerSecond > 0.01;
            float activity = Mathf.Clamp01((float)(controller.CurrentIncomePerSecond / 80.0));

            for (int i = 0; i < clients.Count; i++)
            {
                RectTransform dot = clients[i];
                dot.gameObject.SetActive(active);
                if (!active) continue;

                float t = Mathf.Repeat(Time.time * (0.07f + activity * 0.12f) + i * 0.16f, 1f);
                float x = Mathf.Lerp(-390f, 390f, t);
                float y = -170f + Mathf.Sin(Time.time * 2.1f + i) * 12f;
                dot.anchoredPosition = new Vector2(x, y);
                dot.localScale = Vector3.one * Mathf.Lerp(0.82f, 1.08f, Mathf.PingPong(Time.time * 1.4f + i * 0.2f, 1f));
            }
        }

        private void AnimateFloaters()
        {
            for (int i = floaters.Count - 1; i >= 0; i--)
            {
                FloatingText f = floaters[i];
                if (f == null || f.Rect == null)
                {
                    floaters.RemoveAt(i);
                    continue;
                }

                f.Age += Time.deltaTime;
                f.Rect.anchoredPosition += f.Velocity * Time.deltaTime;
                Color c = f.Text.color;
                c.a = Mathf.Clamp01(1f - f.Age / Mathf.Max(0.01f, f.Life));
                f.Text.color = c;

                if (f.Age >= f.Life)
                {
                    Destroy(f.Rect.gameObject);
                    floaters.RemoveAt(i);
                }
            }
        }

        private void CreateFloatingText(string message, Vector2 start, Vector2 velocity, Color color, int size, float life)
        {
            if (popupLayer == null) return;
            Text text = CreateText("FloatingText", popupLayer, message, size, FontStyle.Bold, TextAnchor.MiddleCenter, color);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(520, 82);
            rect.anchoredPosition = start;
            floaters.Add(new FloatingText { Rect = rect, Text = text, Velocity = velocity, Life = life, Age = 0f });
        }

        private void CreateWorkshopBlock(RectTransform parent, string label, float minX, float minY, float maxX, float maxY, Color color)
        {
            RectTransform block = CreatePanel(label, parent, color);
            Anchor(block, minX, minY, maxX, maxY);
            Text text = CreateText("Text", block, label, 25, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform, 6, 6, 6, 6);
        }

        private Text CreatePill(RectTransform parent, string label)
        {
            RectTransform pill = CreatePanel("Pill", parent, new Color(0.14f, 0.19f, 0.32f, 1f));
            Text text = CreateText("Text", pill, label, 23, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.82f, 0.92f, 1f, 1f));
            Stretch(text.rectTransform, 8, 8, 4, 4);
            return text;
        }

        private Button CreateButton(RectTransform parent, string label, Color color, UnityEngine.Events.UnityAction action)
        {
            GameObject obj = new GameObject(label + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>();
            image.color = color;
            Button button = obj.GetComponent<Button>();
            if (action != null) button.onClick.AddListener(action);

            Text text = CreateText("Text", obj.transform, label, 23, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform, 6, 6, 6, 6);
            return button;
        }

        private RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>();
            image.color = color;
            return obj.GetComponent<RectTransform>();
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private Text CreateText(string name, Transform parent, string text, int size, FontStyle style, TextAnchor alignment, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);
            Text label = obj.GetComponent<Text>();
            label.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = alignment;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private static void CreateEventSystemIfNeeded()
        {
            if (EventSystem.current != null) return;
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventSystem);
        }

        private static void Stretch(RectTransform rect, float left = 0, float right = 0, float top = 0, float bottom = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void Anchor(RectTransform rect, float minX, float minY, float maxX, float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void AddLayout(RectTransform rect, float preferredWidth, float preferredHeight, float flexibleHeight = 0f)
        {
            LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
            if (preferredWidth >= 0) layout.preferredWidth = preferredWidth;
            if (preferredHeight >= 0) layout.preferredHeight = preferredHeight;
            layout.flexibleHeight = flexibleHeight;
        }

        private static string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            int remaining = seconds % 60;
            return minutes.ToString("00") + ":" + remaining.ToString("00");
        }
    }
}
