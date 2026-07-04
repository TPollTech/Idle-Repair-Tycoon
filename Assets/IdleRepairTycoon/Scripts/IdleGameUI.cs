using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IdleRepairTycoon
{
    public sealed class IdleGameUI : MonoBehaviour
    {
        private sealed class FloatingText
        {
            public RectTransform Rect;
            public Text Text;
            public Vector2 Velocity;
            public float Age;
            public float Life;
        }

        private IdleGameController controller;
        private Canvas canvas;
        private Font font;
        private Text cashText;
        private Text incomeText;
        private Text boostText;
        private Text reputationText;
        private Text selectedTitleText;
        private Text selectedInfoText;
        private Text selectedButtonText;
        private Text prestigeButtonText;
        private Text hintText;
        private Text toastText;
        private Text offlineText;
        private Image selectedProgressFill;
        private Button selectedButton;
        private Button prestigeButton;
        private GameObject offlinePanel;
        private RectTransform floatingLayer;
        private double lastOfflineIncome;
        private double lastObservedCash;
        private bool hasCashSnapshot;
        private float cashPopupCooldown;
        private float toastTimer;
        private readonly System.Collections.Generic.List<FloatingText> floaters = new System.Collections.Generic.List<FloatingText>();

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
            WatchCashDelta();
            AnimateFloaters();

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
            canvas.sortingOrder = 30;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = canvasObject.GetComponent<RectTransform>();
            Stretch(root);

            BuildTopHud(root);
            BuildHint(root);
            BuildSelectedStationPanel(root);
            BuildBottomActions(root);
            BuildOfflinePanel(root);
            BuildToast(root);

            floatingLayer = CreatePanel("FloatingLayer", root, new Color(0f, 0f, 0f, 0f));
            Stretch(floatingLayer);
            floatingLayer.SetAsLastSibling();
        }

        private void BuildTopHud(RectTransform root)
        {
            RectTransform hud = CreatePanel("TopHud", root, new Color(0.04f, 0.06f, 0.11f, 0.92f));
            hud.anchorMin = new Vector2(0.04f, 0.86f);
            hud.anchorMax = new Vector2(0.96f, 0.985f);
            hud.offsetMin = Vector2.zero;
            hud.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = hud.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 12, 12);
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text title = CreateText("Title", hud, "Idle Assistência", 32, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.86f, 0.94f, 1f, 1f));
            AddLayout(title.rectTransform, -1, 38);

            cashText = CreateText("Cash", hud, "R$ 0", 54, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            AddLayout(cashText.rectTransform, -1, 58);

            RectTransform row = CreatePanel("HudStats", hud, new Color(0f, 0f, 0f, 0f));
            AddLayout(row, -1, 40);
            HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 10;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            incomeText = CreatePill(row, "+R$ 0/s");
            reputationText = CreatePill(row, "Rep. 0");
            boostText = CreatePill(row, "Turbo off");
        }

        private void BuildHint(RectTransform root)
        {
            RectTransform hint = CreatePanel("Hint", root, new Color(1f, 1f, 1f, 0.84f));
            hint.anchorMin = new Vector2(0.08f, 0.785f);
            hint.anchorMax = new Vector2(0.92f, 0.835f);
            hint.offsetMin = Vector2.zero;
            hint.offsetMax = Vector2.zero;

            hintText = CreateText("HintText", hint, "Toque em uma bancada para melhorar", 23, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.12f, 0.16f, 0.25f, 1f));
            Stretch(hintText.rectTransform, 12, 12, 4, 4);
        }

        private void BuildSelectedStationPanel(RectTransform root)
        {
            RectTransform panel = CreatePanel("SelectedStationPanel", root, new Color(0.98f, 0.99f, 1f, 0.94f));
            panel.anchorMin = new Vector2(0.04f, 0.135f);
            panel.anchorMax = new Vector2(0.96f, 0.305f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            HorizontalLayoutGroup layout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 18, 18);
            layout.spacing = 18;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            RectTransform info = CreatePanel("StationInfo", panel, new Color(0f, 0f, 0f, 0f));
            AddLayout(info, 0, -1, 1f);
            VerticalLayoutGroup infoLayout = info.gameObject.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 8;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = true;
            infoLayout.childForceExpandHeight = false;

            selectedTitleText = CreateText("StationTitle", info, "Bancada", 32, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.08f, 0.12f, 0.22f, 1f));
            AddLayout(selectedTitleText.rectTransform, -1, 38);

            selectedInfoText = CreateText("StationInfoText", info, "Selecione uma estação", 22, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.28f, 0.32f, 0.43f, 1f));
            AddLayout(selectedInfoText.rectTransform, -1, 50);

            RectTransform progressBack = CreatePanel("SelectedProgressBack", info, new Color(0.80f, 0.84f, 0.92f, 1f));
            AddLayout(progressBack, -1, 18);
            selectedProgressFill = CreateImage("SelectedProgressFill", progressBack, new Color(0.14f, 0.68f, 0.42f, 1f));
            selectedProgressFill.rectTransform.anchorMin = new Vector2(0f, 0f);
            selectedProgressFill.rectTransform.anchorMax = new Vector2(0f, 1f);
            selectedProgressFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            selectedProgressFill.rectTransform.offsetMin = Vector2.zero;
            selectedProgressFill.rectTransform.offsetMax = Vector2.zero;

            selectedButton = CreateButton(panel, "Melhorar", new Color(0.18f, 0.52f, 0.95f, 1f), OnSelectedButtonPressed);
            AddLayout(selectedButton.GetComponent<RectTransform>(), 270, -1);
            selectedButtonText = selectedButton.GetComponentInChildren<Text>();
        }

        private void BuildBottomActions(RectTransform root)
        {
            RectTransform bottom = CreatePanel("BottomActions", root, new Color(0.04f, 0.06f, 0.11f, 0.94f));
            bottom.anchorMin = new Vector2(0.04f, 0.025f);
            bottom.anchorMax = new Vector2(0.96f, 0.115f);
            bottom.offsetMin = Vector2.zero;
            bottom.offsetMax = Vector2.zero;

            HorizontalLayoutGroup layout = bottom.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.spacing = 12;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            CreateButton(bottom, "Turbo 2x", new Color(0.16f, 0.50f, 0.96f, 1f), () => controller.WatchAdForBoost());
            CreateButton(bottom, "Pacote R$", new Color(0.12f, 0.63f, 0.38f, 1f), () => controller.WatchAdForMoneyPackage());
            prestigeButton = CreateButton(bottom, "Prestígio", new Color(0.72f, 0.42f, 0.16f, 1f), () => controller.Prestige());
            prestigeButtonText = prestigeButton.GetComponentInChildren<Text>();
        }

        private void BuildOfflinePanel(RectTransform root)
        {
            offlinePanel = CreatePanel("OfflinePanel", root, new Color(0f, 0f, 0f, 0.72f)).gameObject;
            Stretch(offlinePanel.GetComponent<RectTransform>());
            offlinePanel.SetActive(false);

            RectTransform modal = CreatePanel("OfflineModal", offlinePanel.transform, Color.white);
            modal.anchorMin = new Vector2(0.08f, 0.36f);
            modal.anchorMax = new Vector2(0.92f, 0.64f);
            modal.offsetMin = Vector2.zero;
            modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 26, 26);
            layout.spacing = 14;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text title = CreateText("OfflineTitle", modal, "Enquanto você estava fora", 29, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.08f, 0.12f, 0.22f, 1f));
            AddLayout(title.rectTransform, -1, 42);

            offlineText = CreateText("OfflineText", modal, "Você ganhou R$ 0", 24, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.18f, 0.22f, 0.33f, 1f));
            AddLayout(offlineText.rectTransform, -1, 68);

            RectTransform row = CreatePanel("OfflineButtons", modal, new Color(0f, 0f, 0f, 0f));
            AddLayout(row, -1, 66);
            HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            CreateButton(row, "Receber", new Color(0.45f, 0.48f, 0.56f, 1f), () => offlinePanel.SetActive(false));
            CreateButton(row, "Dobrar anúncio", new Color(0.12f, 0.63f, 0.38f, 1f), () =>
            {
                offlinePanel.SetActive(false);
                controller.DoubleOfflineIncomeWithAd(lastOfflineIncome);
            });
        }

        private void BuildToast(RectTransform root)
        {
            RectTransform panel = CreatePanel("ToastPanel", root, new Color(0.04f, 0.05f, 0.08f, 0.92f));
            panel.anchorMin = new Vector2(0.08f, 0.335f);
            panel.anchorMax = new Vector2(0.92f, 0.335f);
            panel.sizeDelta = new Vector2(0, 70);
            panel.anchoredPosition = Vector2.zero;

            toastText = CreateText("ToastText", panel, "", 23, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Stretch(toastText.rectTransform, 12, 12, 6, 6);
            panel.gameObject.SetActive(false);
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
                    prestigeButtonText.text = controller.CanPrestige ? "Prestígio" : "Prestígio\n" + IdleGameBalance.FormatMoney(IdleGameBalance.PrestigeCost);
            }

            RefreshSelectedStation();
        }

        private void RefreshSelectedStation()
        {
            StationRuntime station = controller.SelectedStation;
            if (station == null)
            {
                selectedTitleText.text = "Nenhuma bancada";
                selectedInfoText.text = "Toque em uma bancada da oficina.";
                selectedButton.interactable = false;
                selectedButtonText.text = "Selecionar";
                selectedProgressFill.rectTransform.anchorMax = new Vector2(0f, 1f);
                return;
            }

            bool unlocked = station.Save.Unlocked;
            double cost = unlocked ? station.UpgradeCost() : station.Definition.UnlockCost;
            double profit = station.ProfitPerJob(controller.PrestigeMultiplier, controller.BoostMultiplier);

            string emoji = station.Definition.Emoji + " ";
            selectedTitleText.text = unlocked
                ? emoji + station.Definition.Title + "  Nv. " + station.Save.Level
                : emoji + station.Definition.Title + " bloqueado";

            selectedInfoText.text = unlocked
                ? station.Definition.Description + "\n" + IdleGameBalance.FormatMoney(profit) + " / serviço • " + station.DurationSeconds().ToString("0.0") + "s"
                : "Custo para liberar: " + IdleGameBalance.FormatMoney(cost);

            float p = station.NormalizedProgress();
            selectedProgressFill.rectTransform.anchorMax = new Vector2(p, 1f);
            selectedProgressFill.color = p < 0.5f
                ? Color.Lerp(new Color(0.14f, 0.68f, 0.42f, 1f), new Color(0.95f, 0.80f, 0.12f, 1f), p * 2f)
                : Color.Lerp(new Color(0.95f, 0.80f, 0.12f, 1f), new Color(0.92f, 0.26f, 0.16f, 1f), (p - 0.5f) * 2f);
            selectedButton.interactable = controller.Save.Cash >= cost;
            selectedButtonText.text = unlocked ? "Melhorar\n" + IdleGameBalance.FormatMoney(cost) : "Liberar\n" + IdleGameBalance.FormatMoney(cost);
        }

        private void OnSelectedButtonPressed()
        {
            StationRuntime station = controller.SelectedStation;
            if (station == null) return;

            if (station.Save.Unlocked) controller.UpgradeStation(station.Definition.Id);
            else controller.UnlockStation(station.Definition.Id);
            if (controller?.Audio != null) controller.Audio.PlayUpgrade();
        }

        private void ShowOfflinePanel(double amount, bool capped)
        {
            lastOfflineIncome = amount;
            offlineText.text = "Você ganhou " + IdleGameBalance.FormatMoney(amount) + (capped ? "\nLimite offline: 8 horas." : "");
            offlinePanel.SetActive(true);
            offlinePanel.transform.SetAsLastSibling();
            if (floatingLayer != null) floatingLayer.SetAsLastSibling();
        }

        private void ShowToast(string message)
        {
            if (toastText == null) return;
            toastText.text = message;
            toastText.transform.parent.gameObject.SetActive(true);
            toastText.transform.parent.SetAsLastSibling();
            if (floatingLayer != null) floatingLayer.SetAsLastSibling();
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
                CreateFloatingText("+" + IdleGameBalance.FormatMoney(delta));
                lastObservedCash = current;
                cashPopupCooldown = 0.36f;
            }
        }

        private void CreateFloatingText(string message)
        {
            if (floatingLayer == null) return;

            Text text = CreateText("FloatingMoney", floatingLayer, message, 35, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.20f, 0.86f, 0.52f, 1f));
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(520, 90);
            rect.anchoredPosition = new Vector2(Random.Range(-260f, 260f), Random.Range(10f, 280f));

            floaters.Add(new FloatingText
            {
                Rect = rect,
                Text = text,
                Velocity = new Vector2(Random.Range(-14f, 14f), 110f),
                Life = 1.15f,
                Age = 0f
            });
        }

        private void AnimateFloaters()
        {
            for (int i = floaters.Count - 1; i >= 0; i--)
            {
                FloatingText floater = floaters[i];
                if (floater == null || floater.Rect == null)
                {
                    floaters.RemoveAt(i);
                    continue;
                }

                floater.Age += Time.deltaTime;
                floater.Rect.anchoredPosition += floater.Velocity * Time.deltaTime;

                Color color = floater.Text.color;
                color.a = Mathf.Clamp01(1f - floater.Age / Mathf.Max(0.01f, floater.Life));
                floater.Text.color = color;

                if (floater.Age >= floater.Life)
                {
                    Destroy(floater.Rect.gameObject);
                    floaters.RemoveAt(i);
                }
            }
        }

        private Text CreatePill(RectTransform parent, string value)
        {
            RectTransform pill = CreatePanel("Pill", parent, new Color(0.12f, 0.16f, 0.27f, 1f));
            Text text = CreateText("Text", pill, value, 20, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.82f, 0.92f, 1f, 1f));
            Stretch(text.rectTransform, 8, 8, 2, 2);
            return text;
        }

        private Button CreateButton(RectTransform parent, string label, Color color, UnityEngine.Events.UnityAction action)
        {
            GameObject obj = new GameObject(label + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);

            Image image = obj.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;

            Button button = obj.GetComponent<Button>();
            if (action != null) button.onClick.AddListener(action);
            if (controller?.Audio != null) button.onClick.AddListener(() => controller.Audio.PlayClick());

            Text text = CreateText("Text", obj.transform, label, 22, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform, 8, 8, 6, 6);
            return button;
        }

        private RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = color.a > 0.02f;
            return obj.GetComponent<RectTransform>();
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
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
            label.raycastTarget = false;
            return label;
        }

        private static void CreateEventSystemIfNeeded()
        {
            if (EventSystem.current != null) return;
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventSystem);
        }

        private static void Stretch(RectTransform rect, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void AddLayout(RectTransform rect, float preferredWidth, float preferredHeight, float flexibleWidth = 0f)
        {
            LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
            if (preferredWidth >= 0) layout.preferredWidth = preferredWidth;
            if (preferredHeight >= 0) layout.preferredHeight = preferredHeight;
            layout.flexibleWidth = flexibleWidth;
        }

        private static string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            int remaining = seconds % 60;
            return minutes.ToString("00") + ":" + remaining.ToString("00");
        }
    }
}
