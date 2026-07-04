using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IdleRepairTycoon
{
    public sealed class IdleGameUI : MonoBehaviour
    {
        private IdleGameController controller;
        private Canvas canvas;
        private Text cashText;
        private Text incomeText;
        private Text boostText;
        private Text reputationText;
        private Text toastText;
        private Button boostButton;
        private Button packageButton;
        private Button prestigeButton;
        private GameObject offlinePanel;
        private Text offlineText;
        private double lastOfflineIncome;
        private readonly List<StationCard> stationCards = new List<StationCard>();
        private float toastTimer;

        public void Bind(IdleGameController gameController)
        {
            controller = gameController;
            controller.OnStateChanged += Refresh;
            controller.OnToast += ShowToast;
            controller.OnOfflineIncomeCalculated += ShowOfflinePanel;
            BuildUI();
        }

        private void Update()
        {
            if (toastTimer > 0)
            {
                toastTimer -= Time.deltaTime;
                if (toastTimer <= 0 && toastText != null) toastText.gameObject.SetActive(false);
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
            canvas = CreateCanvas();
            CreateEventSystemIfNeeded();

            GameObject safeArea = CreateUIObject("SafeArea", canvas.transform);
            RectTransform safeRect = safeArea.GetComponent<RectTransform>();
            safeRect.anchorMin = Vector2.zero;
            safeRect.anchorMax = Vector2.one;
            safeRect.offsetMin = new Vector2(0, 0);
            safeRect.offsetMax = new Vector2(0, 0);

            Image bg = safeArea.AddComponent<Image>();
            bg.color = new Color(0.93f, 0.95f, 1f, 1f);

            VerticalLayoutGroup mainLayout = safeArea.AddComponent<VerticalLayoutGroup>();
            mainLayout.padding = new RectOffset(24, 24, 24, 20);
            mainLayout.spacing = 16;
            mainLayout.childAlignment = TextAnchor.UpperCenter;
            mainLayout.childForceExpandHeight = false;
            mainLayout.childControlHeight = true;
            mainLayout.childControlWidth = true;

            GameObject header = CreatePanel("Header", safeArea.transform, new Color(0.21f, 0.27f, 0.72f, 1f), 28);
            AddLayoutElement(header, -1, 172);
            VerticalLayoutGroup headerLayout = header.AddComponent<VerticalLayoutGroup>();
            headerLayout.padding = new RectOffset(20, 20, 16, 14);
            headerLayout.spacing = 3;
            headerLayout.childAlignment = TextAnchor.MiddleCenter;

            Text title = CreateText("Title", header.transform, "Idle Assistência", 32, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            AddLayoutElement(title.gameObject, -1, 38);
            cashText = CreateText("Cash", header.transform, "R$ 0", 29, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            AddLayoutElement(cashText.gameObject, -1, 34);
            incomeText = CreateText("Income", header.transform, "+R$ 0/s", 18, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.92f, 0.94f, 1f, 1f));
            AddLayoutElement(incomeText.gameObject, -1, 24);
            reputationText = CreateText("Reputation", header.transform, "Reputação 0", 16, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.92f, 0.94f, 1f, 1f));
            AddLayoutElement(reputationText.gameObject, -1, 22);

            GameObject actionRow = CreateUIObject("ActionRow", safeArea.transform);
            AddLayoutElement(actionRow, -1, 72);
            HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 10;
            actionLayout.childControlWidth = true;
            actionLayout.childForceExpandWidth = true;
            actionLayout.childControlHeight = true;

            boostButton = CreateButton(actionRow.transform, "Turbo 2x", () => controller.WatchAdForBoost(), new Color(0.25f, 0.55f, 0.98f, 1f));
            packageButton = CreateButton(actionRow.transform, "Pacote R$", () => controller.WatchAdForMoneyPackage(), new Color(0.16f, 0.62f, 0.32f, 1f));
            prestigeButton = CreateButton(actionRow.transform, "Prestígio", () => controller.Prestige(), new Color(0.72f, 0.42f, 0.16f, 1f));

            GameObject boostInfo = CreatePanel("BoostInfo", safeArea.transform, Color.white, 18);
            AddLayoutElement(boostInfo, -1, 42);
            boostText = CreateText("BoostText", boostInfo.transform, "Turbo inativo", 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.17f, 0.18f, 0.25f, 1f));
            Stretch(boostText.GetComponent<RectTransform>());

            ScrollRect scroll = CreateScroll(safeArea.transform);
            AddLayoutElement(scroll.gameObject, -1, 0, flexibleHeight: 1);

            foreach (StationRuntime station in controller.Stations)
            {
                StationCard card = new StationCard(this, station, scroll.content);
                stationCards.Add(card);
            }

            toastText = CreateText("Toast", canvas.transform, "", 18, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            RectTransform toastRect = toastText.GetComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(0.08f, 0.08f);
            toastRect.anchorMax = new Vector2(0.92f, 0.08f);
            toastRect.sizeDelta = new Vector2(0, 58);
            Image toastBg = toastText.gameObject.AddComponent<Image>();
            toastBg.color = new Color(0.05f, 0.06f, 0.09f, 0.88f);
            toastBg.raycastTarget = false;
            toastText.gameObject.SetActive(false);

            BuildOfflinePanel();
            Refresh();
        }

        private void BuildOfflinePanel()
        {
            offlinePanel = CreatePanel("OfflinePanel", canvas.transform, new Color(0f, 0f, 0f, 0.72f), 0);
            Stretch(offlinePanel.GetComponent<RectTransform>());
            offlinePanel.SetActive(false);

            GameObject modal = CreatePanel("OfflineModal", offlinePanel.transform, Color.white, 30);
            RectTransform modalRect = modal.GetComponent<RectTransform>();
            modalRect.anchorMin = new Vector2(0.08f, 0.36f);
            modalRect.anchorMax = new Vector2(0.92f, 0.64f);
            modalRect.offsetMin = Vector2.zero;
            modalRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = modal.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 22, 22);
            layout.spacing = 14;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childAlignment = TextAnchor.MiddleCenter;

            Text title = CreateText("OfflineTitle", modal.transform, "Enquanto você estava fora", 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.12f, 0.14f, 0.22f, 1f));
            AddLayoutElement(title.gameObject, -1, 34);
            offlineText = CreateText("OfflineText", modal.transform, "Você ganhou R$ 0", 20, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.18f, 0.2f, 0.3f, 1f));
            AddLayoutElement(offlineText.gameObject, -1, 58);

            GameObject row = CreateUIObject("OfflineButtons", modal.transform);
            AddLayoutElement(row, -1, 58);
            HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;

            CreateButton(row.transform, "Receber", () => offlinePanel.SetActive(false), new Color(0.44f, 0.47f, 0.56f, 1f));
            CreateButton(row.transform, "Dobrar com anúncio", () =>
            {
                offlinePanel.SetActive(false);
                controller.DoubleOfflineIncomeWithAd(lastOfflineIncome);
            }, new Color(0.16f, 0.62f, 0.32f, 1f));
        }

        private void ShowOfflinePanel(double amount, bool capped)
        {
            lastOfflineIncome = amount;
            offlineText.text = "Você ganhou " + IdleGameBalance.FormatMoney(amount) + (capped ? "\nLimite offline: 8 horas." : "");
            offlinePanel.SetActive(true);
        }

        private void Refresh()
        {
            if (controller == null || cashText == null) return;

            cashText.text = IdleGameBalance.FormatMoney(controller.Save.Cash);
            incomeText.text = "+" + IdleGameBalance.FormatMoney(controller.CurrentIncomePerSecond) + "/s";
            reputationText.text = "Reputação: " + controller.Save.ReputationStars + "  •  Multiplicador: x" + controller.PrestigeMultiplier.ToString("0.00");

            if (controller.HasActiveBoost)
            {
                boostText.text = "Turbo 2x ativo: " + FormatTime(controller.BoostSecondsRemaining);
            }
            else
            {
                boostText.text = "Turbo inativo — assista anúncio para 2x por 5 min";
            }

            prestigeButton.interactable = controller.CanPrestige;

            foreach (StationCard card in stationCards)
            {
                card.Refresh(controller);
            }
        }

        private void ShowToast(string message)
        {
            if (toastText == null) return;
            toastText.text = message;
            toastText.gameObject.SetActive(true);
            toastTimer = 2.1f;
        }

        private static string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            int remaining = seconds % 60;
            return minutes.ToString("00") + ":" + remaining.ToString("00");
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("IdleRepairCanvas", typeof(RectTransform));
            Canvas c = canvasObject.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.pixelPerfect = false;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return c;
        }

        private static void CreateEventSystemIfNeeded()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null) return;
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private ScrollRect CreateScroll(Transform parent)
        {
            GameObject scrollObj = CreateUIObject("StationScroll", parent);
            Image scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = new Color(0, 0, 0, 0);

            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 35;

            GameObject viewport = CreateUIObject("Viewport", scrollObj.transform);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect);
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.white;

            GameObject content = CreateUIObject("Content", viewport.transform);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 24);
            layout.spacing = 14;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            return scroll;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color, float radiusIgnored)
        {
            GameObject panel = CreateUIObject(name, parent);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static Text CreateText(string name, Transform parent, string text, int size, FontStyle style, TextAnchor alignment, Color color)
        {
            GameObject obj = CreateUIObject(name, parent);
            Text t = obj.AddComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = alignment;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            return t;
        }

        private static Button CreateButton(Transform parent, string label, UnityAction action, Color color)
        {
            GameObject obj = CreatePanel(label + "Button", parent, color, 18);
            AddLayoutElement(obj, -1, 54);
            Button button = obj.AddComponent<Button>();
            button.targetGraphic = obj.GetComponent<Image>();
            button.onClick.AddListener(action);

            Text text = CreateText("Text", obj.transform, label, 17, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.GetComponent<RectTransform>());
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void AddLayoutElement(GameObject obj, float preferredWidth = -1, float preferredHeight = -1, float flexibleHeight = 0)
        {
            LayoutElement element = obj.GetComponent<LayoutElement>() ?? obj.AddComponent<LayoutElement>();
            if (preferredWidth >= 0) element.preferredWidth = preferredWidth;
            if (preferredHeight >= 0) element.preferredHeight = preferredHeight;
            element.flexibleHeight = flexibleHeight;
        }

        private sealed class StationCard
        {
            private readonly StationRuntime station;
            private readonly Text titleText;
            private readonly Text descriptionText;
            private readonly Text metaText;
            private readonly Image progressFill;
            private readonly Button mainButton;
            private readonly Text buttonText;

            public StationCard(IdleGameUI ui, StationRuntime stationRuntime, Transform parent)
            {
                station = stationRuntime;

                GameObject card = CreatePanel("Station_" + station.Definition.Id, parent, Color.white, 24);
                AddLayoutElement(card, -1, 178);

                VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(18, 18, 14, 14);
                layout.spacing = 8;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandHeight = false;

                GameObject topRow = CreateUIObject("TopRow", card.transform);
                AddLayoutElement(topRow, -1, 54);
                HorizontalLayoutGroup topLayout = topRow.AddComponent<HorizontalLayoutGroup>();
                topLayout.spacing = 12;
                topLayout.childControlHeight = true;
                topLayout.childControlWidth = true;

                Text icon = CreateText("Icon", topRow.transform, station.Definition.Emoji, 34, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.21f, 0.27f, 0.72f, 1f));
                AddLayoutElement(icon.gameObject, 60, 54);

                GameObject titleBox = CreateUIObject("TitleBox", topRow.transform);
                AddLayoutElement(titleBox, -1, 54, 1);
                VerticalLayoutGroup titleLayout = titleBox.AddComponent<VerticalLayoutGroup>();
                titleLayout.spacing = 0;
                titleLayout.childControlHeight = true;
                titleLayout.childControlWidth = true;

                titleText = CreateText("Title", titleBox.transform, station.Definition.Title, 21, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.12f, 0.14f, 0.22f, 1f));
                AddLayoutElement(titleText.gameObject, -1, 28);
                descriptionText = CreateText("Description", titleBox.transform, station.Definition.Description, 14, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.45f, 0.47f, 0.56f, 1f));
                AddLayoutElement(descriptionText.gameObject, -1, 24);

                metaText = CreateText("Meta", card.transform, "", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.17f, 0.18f, 0.25f, 1f));
                AddLayoutElement(metaText.gameObject, -1, 24);

                GameObject progressBar = CreatePanel("Progress", card.transform, new Color(0.86f, 0.88f, 0.94f, 1f), 12);
                AddLayoutElement(progressBar, -1, 16);
                GameObject fill = CreatePanel("Fill", progressBar.transform, new Color(0.25f, 0.55f, 0.98f, 1f), 12);
                progressFill = fill.GetComponent<Image>();
                RectTransform fillRect = fill.GetComponent<RectTransform>();
                fillRect.anchorMin = new Vector2(0, 0);
                fillRect.anchorMax = new Vector2(0, 1);
                fillRect.pivot = new Vector2(0, 0.5f);
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;

                mainButton = CreateButton(card.transform, "Melhorar", () =>
                {
                    if (station.Save.Unlocked) ui.controller.UpgradeStation(station.Definition.Id);
                    else ui.controller.UnlockStation(station.Definition.Id);
                }, new Color(0.21f, 0.27f, 0.72f, 1f));
                buttonText = mainButton.GetComponentInChildren<Text>();
            }

            public void Refresh(IdleGameController controller)
            {
                if (station.Save.Unlocked)
                {
                    titleText.text = station.Definition.Title + "  Nv. " + station.Level;
                    metaText.text = "Lucro: " + IdleGameBalance.FormatMoney(station.ProfitPerJob(controller.PrestigeMultiplier, controller.BoostMultiplier))
                        + "  •  Tempo: " + station.DurationSeconds().ToString("0.0") + "s"
                        + "  •  Up: " + IdleGameBalance.FormatMoney(station.UpgradeCost());
                    buttonText.text = "Melhorar — " + IdleGameBalance.FormatMoney(station.UpgradeCost());
                    mainButton.interactable = controller.Save.Cash >= station.UpgradeCost();
                    SetProgress(station.NormalizedProgress());
                }
                else
                {
                    titleText.text = station.Definition.Title + "  Bloqueado";
                    string required = station.Definition.RequiredReputation > 0 ? "  •  Req. reputação " + station.Definition.RequiredReputation : "";
                    metaText.text = "Liberar: " + IdleGameBalance.FormatMoney(station.Definition.UnlockCost) + required;
                    buttonText.text = "Liberar — " + IdleGameBalance.FormatMoney(station.Definition.UnlockCost);
                    bool hasReputation = controller.Save.ReputationStars >= station.Definition.RequiredReputation;
                    mainButton.interactable = controller.Save.Cash >= station.Definition.UnlockCost && hasReputation;
                    SetProgress(0);
                }
            }

            private void SetProgress(float value)
            {
                RectTransform rect = progressFill.GetComponent<RectTransform>();
                rect.anchorMax = new Vector2(Mathf.Clamp01(value), 1);
            }
        }
    }
}
