using System;
using Psycho.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Psycho.UI
{
    public sealed class PsychoHudController : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        public static bool HasActiveHud { get; private set; }
        public static bool BlocksPlayerInput { get; private set; }

        [SerializeField] private Transform player;
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentPrayer = 1;
        [SerializeField] private int maxPrayer = 1;
        [SerializeField] private int runEnergy = 100;
        [SerializeField] private long moneyPouch;
        [SerializeField] private string recipeForDisasterStatus = "Recipe for Disaster: Not started";
        [SerializeField] private string nomadStatus = "Nomad's Requiem: Not started";
        [SerializeField] private string questSummary = "Gallows Dawn: Not started";

        private Canvas canvas;
        private RectTransform escapeMenu;
        private RectTransform contextPanel;
        private Text contextTitle;
        private Text contextBody;
        private Text toastText;
        private Text healthText;
        private Text prayerText;
        private Text runText;
        private Text moneyText;
        private Text coordinateText;
        private Image healthFill;
        private Image prayerFill;
        private Image runFill;
        private RectTransform minimapSweep;
        private Sprite circleSprite;
        private float toastUntil;
        private bool escapeMenuOpen;
        private int lastEscapeToggleFrame = -1;

        private void Awake()
        {
            HasActiveHud = true;
            BlocksPlayerInput = false;
            BuildHud();
            SetEscapeMenu(false);
            SetContextPanel(false, string.Empty, string.Empty);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDestroy()
        {
            HasActiveHud = false;
            BlocksPlayerInput = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleEscapeMenuFromInput();
            }

            UpdateStatusReadouts();
            UpdateMinimap();
            if (toastText != null)
            {
                toastText.enabled = Time.unscaledTime < toastUntil;
            }
        }

        private void OnGUI()
        {
            Event current = Event.current;
            if (current != null && current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
            {
                ToggleEscapeMenuFromInput();
                current.Use();
            }
        }

        private void BuildHud()
        {
            EnsureEventSystem();
            circleSprite = CreateCircleSprite(96);

            GameObject canvasObject = new GameObject("Psycho Gameplay Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            BuildTopRightCluster(canvas.transform);
            BuildBottomRightMenu(canvas.transform);
            BuildEscapeMenu(canvas.transform);
            BuildContextPanel(canvas.transform);
            BuildToast(canvas.transform);
        }

        private void BuildTopRightCluster(Transform parent)
        {
            RectTransform root = CreateRect("Top Right HUD", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(360f, 328f), new Vector2(-18f, -18f));

            Image panel = root.gameObject.AddComponent<Image>();
            panel.color = new Color(0.02f, 0.07f, 0.10f, 0.62f);

            RectTransform mapRoot = CreateRect("Minimap", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(184f, 184f), new Vector2(0f, -16f));
            Image mapBackground = mapRoot.gameObject.AddComponent<Image>();
            mapBackground.sprite = circleSprite;
            mapBackground.color = new Color(0.07f, 0.18f, 0.16f, 0.90f);

            RectTransform terrainTint = CreateRect("Minimap Terrain Tint", mapRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(152f, 152f), Vector2.zero);
            Image terrainImage = terrainTint.gameObject.AddComponent<Image>();
            terrainImage.sprite = circleSprite;
            terrainImage.color = new Color(0.26f, 0.50f, 0.24f, 0.76f);

            minimapSweep = CreateRect("Minimap Sweep", mapRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(4f, 76f), new Vector2(0f, 38f));
            Image sweepImage = minimapSweep.gameObject.AddComponent<Image>();
            sweepImage.color = new Color(0.60f, 0.93f, 1f, 0.42f);

            CreateMinimapBlip(mapRoot, new Vector2(-42f, 22f), new Color(0.90f, 0.72f, 0.35f, 1f));
            CreateMinimapBlip(mapRoot, new Vector2(36f, 26f), new Color(0.82f, 0.86f, 0.92f, 1f));
            CreateMinimapBlip(mapRoot, new Vector2(-12f, -44f), new Color(0.35f, 0.72f, 1f, 1f));
            CreateMinimapBlip(mapRoot, new Vector2(50f, -34f), new Color(0.96f, 0.44f, 0.36f, 1f));

            RectTransform playerDot = CreateRect("Player Dot", mapRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(12f, 12f), Vector2.zero);
            Image playerDotImage = playerDot.gameObject.AddComponent<Image>();
            playerDotImage.sprite = circleSprite;
            playerDotImage.color = new Color(1f, 0.92f, 0.42f, 1f);

            Text north = CreateText("N", mapRoot, 18, FontStyle.Bold, new Color(0.92f, 0.96f, 1f, 0.86f), TextAnchor.MiddleCenter);
            north.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            north.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            north.rectTransform.pivot = new Vector2(0.5f, 1f);
            north.rectTransform.sizeDelta = new Vector2(36f, 24f);
            north.rectTransform.anchoredPosition = new Vector2(0f, -8f);

            coordinateText = CreateText("Edgeville", root, 14, FontStyle.Bold, new Color(0.79f, 0.91f, 0.96f, 0.86f), TextAnchor.MiddleCenter);
            coordinateText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            coordinateText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            coordinateText.rectTransform.pivot = new Vector2(0.5f, 1f);
            coordinateText.rectTransform.sizeDelta = new Vector2(230f, 24f);
            coordinateText.rectTransform.anchoredPosition = new Vector2(0f, -198f);

            healthFill = CreateStatusBar(root, "HP", new Vector2(14f, -235f), new Color(0.84f, 0.12f, 0.12f, 1f), out healthText);
            prayerFill = CreateStatusBar(root, "PR", new Vector2(14f, -268f), new Color(0.32f, 0.58f, 1f, 1f), out prayerText);
            runFill = CreateStatusBar(root, "RUN", new Vector2(14f, -301f), new Color(0.30f, 0.80f, 0.36f, 1f), out runText);

            RectTransform pouch = CreateRect("Money Pouch", root, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(118f, 74f), new Vector2(-12f, 14f));
            Image pouchImage = pouch.gameObject.AddComponent<Image>();
            pouchImage.color = new Color(0.36f, 0.24f, 0.05f, 0.78f);
            moneyText = CreateText("Pouch\n0 gp", pouch, 15, FontStyle.Bold, new Color(1f, 0.86f, 0.38f, 1f), TextAnchor.MiddleCenter);
            Stretch(moneyText.rectTransform, new Vector2(8f, 5f), new Vector2(8f, 5f));
            moneyText.raycastTarget = false;
        }

        private void BuildBottomRightMenu(Transform parent)
        {
            RectTransform root = CreateRect("Bottom Right Menu Bar", parent, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(392f, 116f), new Vector2(-18f, 18f));
            Image panel = root.gameObject.AddComponent<Image>();
            panel.color = new Color(0.015f, 0.055f, 0.075f, 0.70f);

            string[] labels =
            {
                "Bag", "Combat", "Skills", "Prayer", "Quests",
                "Achieve", "Friends", "Clans", "Magic", "Summon"
            };

            for (int i = 0; i < labels.Length; i++)
            {
                int column = i % 5;
                int row = i / 5;
                Button button = CreateButton(labels[i], root, new Vector2(14f + column * 74f, -14f - row * 50f), new Vector2(68f, 42f));
                string capturedLabel = labels[i];
                button.onClick.AddListener(() => OpenMenuPanel(capturedLabel));
            }
        }

        private void BuildEscapeMenu(Transform parent)
        {
            escapeMenu = CreateRect("Escape Utility Menu", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(338f, 314f), Vector2.zero);
            Image panel = escapeMenu.gameObject.AddComponent<Image>();
            panel.color = new Color(0.018f, 0.070f, 0.095f, 0.92f);

            CreateText("Psycho Menu", escapeMenu, 24, FontStyle.Bold, new Color(1f, 0.88f, 0.62f, 1f), TextAnchor.MiddleCenter)
                .rectTransform.anchoredPosition = new Vector2(0f, 128f);

            string[] labels =
            {
                "Resume", "Settings", "Graphics",
                "Controls", "Quit Menu", "Quit Desktop"
            };

            for (int i = 0; i < labels.Length; i++)
            {
                int column = i % 3;
                int row = i / 3;
                Button button = CreateButton(labels[i], escapeMenu, new Vector2(18f + column * 104f, -68f - row * 82f), new Vector2(94f, 68f));
                string capturedLabel = labels[i];
                button.onClick.AddListener(() => HandleEscapeOption(capturedLabel));
            }
        }

        private void BuildContextPanel(Transform parent)
        {
            contextPanel = CreateRect("HUD Context Panel", parent, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(392f, 210f), new Vector2(-18f, 146f));
            Image panel = contextPanel.gameObject.AddComponent<Image>();
            panel.color = new Color(0.018f, 0.067f, 0.092f, 0.86f);

            contextTitle = CreateText(string.Empty, contextPanel, 22, FontStyle.Bold, new Color(1f, 0.88f, 0.58f, 1f), TextAnchor.UpperLeft);
            contextTitle.rectTransform.anchorMin = new Vector2(0f, 1f);
            contextTitle.rectTransform.anchorMax = new Vector2(1f, 1f);
            contextTitle.rectTransform.pivot = new Vector2(0f, 1f);
            contextTitle.rectTransform.sizeDelta = new Vector2(-28f, 32f);
            contextTitle.rectTransform.anchoredPosition = new Vector2(14f, -12f);

            contextBody = CreateText(string.Empty, contextPanel, 16, FontStyle.Normal, new Color(0.80f, 0.91f, 0.96f, 1f), TextAnchor.UpperLeft);
            contextBody.rectTransform.anchorMin = new Vector2(0f, 0f);
            contextBody.rectTransform.anchorMax = new Vector2(1f, 1f);
            contextBody.rectTransform.pivot = new Vector2(0f, 1f);
            contextBody.rectTransform.offsetMin = new Vector2(16f, 14f);
            contextBody.rectTransform.offsetMax = new Vector2(-16f, -50f);
        }

        private void BuildToast(Transform parent)
        {
            toastText = CreateText(string.Empty, parent, 18, FontStyle.Bold, new Color(1f, 0.92f, 0.65f, 1f), TextAnchor.MiddleCenter);
            toastText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            toastText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            toastText.rectTransform.pivot = new Vector2(0.5f, 0f);
            toastText.rectTransform.sizeDelta = new Vector2(760f, 34f);
            toastText.rectTransform.anchoredPosition = new Vector2(0f, 38f);
            toastText.enabled = false;
        }

        private void OpenMenuPanel(string label)
        {
            switch (label)
            {
                case "Bag":
                    SetContextPanel(true, "Backpack", "Inventory panel placeholder. Item sync will hook into the server inventory packet layer.");
                    break;
                case "Combat":
                    SetContextPanel(true, "Attack Style", "Accurate  |  Aggressive  |  Defensive  |  Controlled");
                    break;
                case "Skills":
                    SetContextPanel(true, "Skills", "Combat, skilling, and total level overview will mirror Java skill state here.");
                    break;
                case "Prayer":
                    SetContextPanel(true, "Prayers", "Quick prayers and curses will remain display-only until protocol actions are wired.");
                    break;
                case "Quests":
                    SetContextPanel(true, "Quests", recipeForDisasterStatus + "\n" + nomadStatus + "\n\n" + questSummary);
                    break;
                case "Achieve":
                    SetContextPanel(true, "Achievements", "Achievement tracker and Psycho progression goals.");
                    break;
                case "Friends":
                    SetContextPanel(true, "Friends", "Friends list, private chat, and ignore list shell.");
                    break;
                case "Clans":
                    SetContextPanel(true, "Clans", "Clan chat, ranks, and channel tools.");
                    break;
                case "Magic":
                    SetContextPanel(true, "Magic & Teleports", "Home  |  Edgeville  |  Varrock  |  Falador\nLumbridge  |  Grand Exchange  |  Bosses  |  Skilling");
                    break;
                case "Summon":
                    SetContextPanel(true, "Summoning", "No familiar active.\nCall  |  Renew  |  Dismiss  |  Special move");
                    break;
                default:
                    SetContextPanel(false, string.Empty, string.Empty);
                    break;
            }

            ShowToast(label + " panel opened");
        }

        private void HandleEscapeOption(string label)
        {
            switch (label)
            {
                case "Quit Menu":
                    BlocksPlayerInput = false;
                    SceneManager.LoadScene("PsychoLogin");
                    break;
                case "Quit Desktop":
                    Application.Quit();
                    ShowToast("Quit requested");
                    break;
                case "Resume":
                    SetEscapeMenu(false);
                    break;
                case "Settings":
                    SetContextPanel(true, "Settings", "Graphics: " + PsychoRuntimeVisualQuality.CurrentPresetName + "\nUse Graphics to cycle Performance, Balanced, and Ultra presets.\nQuit Menu returns to the title screen.");
                    SetEscapeMenu(false);
                    break;
                case "Graphics":
                    CycleGraphicsPreset();
                    SetEscapeMenu(false);
                    break;
                case "Controls":
                    SetContextPanel(true, "Controls", "Move: WASD\nSprint: Left Shift\nJump: Space\nInteract: E or Left Click\nCamera: Right Mouse / Middle Mouse / Wheel\nMenu: Esc");
                    SetEscapeMenu(false);
                    break;
            }
        }

        public void SetQuestSummary(string summary)
        {
            questSummary = string.IsNullOrWhiteSpace(summary) ? questSummary : summary;
        }

        public void ShowToastMessage(string message)
        {
            ShowToast(message);
        }

        private void CycleGraphicsPreset()
        {
            int nextPreset = (PsychoRuntimeVisualQuality.CurrentPreset + 1) % 3;
            PsychoRuntimeVisualQuality.ApplyPreset(nextPreset);
            string presetName = PsychoRuntimeVisualQuality.PresetName(nextPreset);
            SetContextPanel(true, "Graphics", "Preset: " + presetName + "\nPerformance lowers shadows and LOD distance.\nBalanced keeps the world readable.\nUltra favors 4K-style presentation on stronger PCs.");
            ShowToast("Graphics set to " + presetName);
        }

        private void SetEscapeMenu(bool open)
        {
            escapeMenuOpen = open;
            BlocksPlayerInput = open;
            if (escapeMenu != null)
            {
                escapeMenu.gameObject.SetActive(open);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ToggleEscapeMenuFromInput()
        {
            if (lastEscapeToggleFrame == Time.frameCount)
            {
                return;
            }

            lastEscapeToggleFrame = Time.frameCount;
            SetEscapeMenu(!escapeMenuOpen);
        }

        private void SetContextPanel(bool visible, string title, string body)
        {
            if (contextPanel != null)
            {
                contextPanel.gameObject.SetActive(visible);
            }

            if (contextTitle != null)
            {
                contextTitle.text = title;
            }

            if (contextBody != null)
            {
                contextBody.text = body;
            }
        }

        private void UpdateStatusReadouts()
        {
            int safeMaxHealth = Mathf.Max(1, maxHealth);
            int safeMaxPrayer = Mathf.Max(1, maxPrayer);
            int safeRun = Mathf.Clamp(runEnergy, 0, 100);

            if (healthFill != null)
            {
                healthFill.fillAmount = Mathf.Clamp01(currentHealth / (float)safeMaxHealth);
            }

            if (prayerFill != null)
            {
                prayerFill.fillAmount = Mathf.Clamp01(currentPrayer / (float)safeMaxPrayer);
            }

            if (runFill != null)
            {
                runFill.fillAmount = safeRun / 100f;
            }

            if (healthText != null)
            {
                healthText.text = currentHealth + "/" + safeMaxHealth;
            }

            if (prayerText != null)
            {
                prayerText.text = currentPrayer + "/" + safeMaxPrayer;
            }

            if (runText != null)
            {
                runText.text = safeRun + "%";
            }

            if (moneyText != null)
            {
                moneyText.text = "Pouch\n" + FormatNumber(moneyPouch) + " gp";
            }
        }

        private void UpdateMinimap()
        {
            if (minimapSweep != null)
            {
                minimapSweep.localRotation = Quaternion.Euler(0f, 0f, -Time.unscaledTime * 26f);
            }

            if (coordinateText != null && player != null)
            {
                coordinateText.text = "Edgeville  " + Mathf.RoundToInt(player.position.x) + ", " + Mathf.RoundToInt(player.position.z);
            }
        }

        private Image CreateStatusBar(RectTransform parent, string label, Vector2 anchoredPosition, Color fillColor, out Text valueText)
        {
            RectTransform root = CreateRect(label + " Status", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(186f, 24f), anchoredPosition);
            Text labelText = CreateText(label, root, 13, FontStyle.Bold, new Color(0.92f, 0.96f, 1f, 0.86f), TextAnchor.MiddleLeft);
            labelText.rectTransform.anchorMin = new Vector2(0f, 0f);
            labelText.rectTransform.anchorMax = new Vector2(0f, 1f);
            labelText.rectTransform.pivot = new Vector2(0f, 0.5f);
            labelText.rectTransform.sizeDelta = new Vector2(38f, 0f);
            labelText.rectTransform.anchoredPosition = new Vector2(8f, 0f);

            RectTransform track = CreateRect(label + " Track", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(84f, 12f), new Vector2(86f, 0f));
            Image trackImage = track.gameObject.AddComponent<Image>();
            trackImage.color = new Color(0f, 0f, 0f, 0.46f);

            RectTransform fill = CreateRect(label + " Fill", track, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

            valueText = CreateText(string.Empty, root, 13, FontStyle.Bold, new Color(0.94f, 0.98f, 1f, 0.94f), TextAnchor.MiddleRight);
            valueText.rectTransform.anchorMin = new Vector2(1f, 0f);
            valueText.rectTransform.anchorMax = new Vector2(1f, 1f);
            valueText.rectTransform.pivot = new Vector2(1f, 0.5f);
            valueText.rectTransform.sizeDelta = new Vector2(54f, 0f);
            valueText.rectTransform.anchoredPosition = new Vector2(-6f, 0f);

            return fillImage;
        }

        private Button CreateButton(string label, RectTransform parent, Vector2 anchoredPosition, Vector2 size)
        {
            RectTransform rect = CreateRect(label + " Button", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), size, anchoredPosition);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.08f, 0.22f, 0.28f, 0.94f);

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = CreateButtonColors();

            Text text = CreateText(label, rect, label.Length > 7 ? 12 : 14, FontStyle.Bold, new Color(1f, 0.88f, 0.62f, 1f), TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, new Vector2(4f, 2f), new Vector2(4f, 2f));
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 9;
            text.resizeTextMaxSize = label.Length > 7 ? 12 : 14;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return button;
        }

        private void CreateMinimapBlip(RectTransform parent, Vector2 position, Color color)
        {
            RectTransform blip = CreateRect("Minimap Blip", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(8f, 8f), position);
            Image image = blip.gameObject.AddComponent<Image>();
            image.sprite = circleSprite;
            image.color = color;
        }

        private void ShowToast(string message)
        {
            if (toastText == null)
            {
                return;
            }

            toastText.text = message;
            toastUntil = Time.unscaledTime + 2.4f;
            toastText.enabled = true;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 anchoredPosition)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            return rect;
        }

        private static Text CreateText(string text, Transform parent, int size, FontStyle style, Color color, TextAnchor anchor)
        {
            GameObject textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text uiText = textObject.AddComponent<Text>();
            uiText.text = text;
            uiText.font = LoadUiFont();
            uiText.fontSize = size;
            uiText.fontStyle = style;
            uiText.color = color;
            uiText.alignment = anchor;
            uiText.raycastTarget = false;
            RectTransform rect = uiText.rectTransform;
            rect.sizeDelta = new Vector2(220f, 34f);
            return uiText;
        }

        private static void Stretch(RectTransform rect, Vector2 minOffset, Vector2 maxOffset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = minOffset;
            rect.offsetMax = -maxOffset;
        }

        private static ColorBlock CreateButtonColors()
        {
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = new Color(0.08f, 0.22f, 0.28f, 0.94f);
            colors.highlightedColor = new Color(0.13f, 0.34f, 0.42f, 1f);
            colors.pressedColor = new Color(0.04f, 0.13f, 0.18f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.08f, 0.08f, 0.08f, 0.55f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            return colors;
        }

        private static Font LoadUiFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                return font;
            }

            return Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial" }, 18);
        }

        private static Sprite CreateCircleSprite(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Psycho HUD Circle"
            };

            float center = (size - 1) * 0.5f;
            float radius = center;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(radius - distance + 1f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static string FormatNumber(long value)
        {
            if (Math.Abs(value) >= 1000000000)
            {
                return (value / 1000000000f).ToString("0.##") + "b";
            }

            if (Math.Abs(value) >= 1000000)
            {
                return (value / 1000000f).ToString("0.##") + "m";
            }

            if (Math.Abs(value) >= 1000)
            {
                return (value / 1000f).ToString("0.#") + "k";
            }

            return value.ToString("0");
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("Psycho EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventSystem);
        }
    }
}
