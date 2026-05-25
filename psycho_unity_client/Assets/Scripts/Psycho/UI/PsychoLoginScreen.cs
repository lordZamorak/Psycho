using System;
using System.Threading;
using Psycho.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Psycho.UI
{
    public sealed class PsychoLoginScreen : MonoBehaviour
    {
        private const int ReferenceWidth = 1920;
        private const int ReferenceHeight = 1080;
        private const string BackgroundResource = "PsychoLogin/login_smithing_bg";

        [Header("Networking")]
        [SerializeField] private PsychoProtocolClient protocolClient;
        [SerializeField] private bool loadHostedWorldOnSuccess = true;
        [SerializeField] private string hostedWorldSceneName = "PsychoHostedTestWorld";

        [Header("Look")]
        [SerializeField] private Vector2 panelSize = new Vector2(980f, 560f);
        [SerializeField] private Vector2 shadowOffset = new Vector2(-38f, -42f);

        private Canvas canvas;
        private RawImage backgroundImage;
        private RectTransform panelRoot;
        private Text statusText;
        private InputField usernameField;
        private InputField passwordField;
        private InputField characterNameField;
        private Dropdown raceDropdown;
        private Dropdown hairDropdown;
        private Slider faceSlider;
        private Slider hairColorSlider;
        private Slider buildSlider;
        private Button loginButton;
        private Button previewButton;
        private Button newGameButton;
        private Font uiFont;
        private Texture2D crystalPanelTexture;
        private Texture2D northernBackgroundTexture;
        private Texture2D ironCrestTexture;
        private Sprite softDiscSprite;
        private LoginParticle[] particles;
        private CancellationTokenSource loginCancellation;
        private bool loginInFlight;

        private void Awake()
        {
            RebuildUi();
        }

        private void OnDestroy()
        {
            loginCancellation?.Cancel();
            loginCancellation?.Dispose();
        }

        private void Update()
        {
            float time = Time.unscaledTime;
            if (backgroundImage != null)
            {
                float panX = Mathf.Sin(time * 0.028f) * 0.006f;
                float panY = Mathf.Cos(time * 0.021f) * 0.004f;
                backgroundImage.uvRect = new Rect(panX, panY, 1f, 1f);
            }

            if (panelRoot != null)
            {
                if (panelRoot.gameObject.name == "Northern Main Menu")
                {
                    panelRoot.anchoredPosition = new Vector2(-92f, -28f + Mathf.Sin(time * 0.46f) * 3f);
                }
                else
                {
                    panelRoot.anchoredPosition = new Vector2(0f, Mathf.Sin(time * 0.40f) * 2f);
                }
            }

            AnimateParticles(time);
        }

        public void RebuildUi()
        {
            ClearExistingUi();
            uiFont = LoadUiFont();
            EnsureProtocolClient();
            BuildCanvas();
            BuildBackground();
            BuildParticles();
            BuildPanel();
        }

        private void EnsureProtocolClient()
        {
            if (protocolClient == null)
            {
                protocolClient = FindAnyObjectByType<PsychoProtocolClient>();
            }

            if (protocolClient == null)
            {
                GameObject networkObject = new GameObject("Psycho Protocol Client");
                protocolClient = networkObject.AddComponent<PsychoProtocolClient>();
            }
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("Psycho Login Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 1f;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildBackground()
        {
            Texture2D background = GetNorthernTitleBackgroundTexture();
            GameObject backgroundObject = CreateUiObject("Northern Fog Background", canvas.transform);
            backgroundImage = backgroundObject.AddComponent<RawImage>();
            backgroundImage.texture = background;
            backgroundImage.color = new Color(0.86f, 0.90f, 0.94f, 1f);
            Stretch(backgroundImage.rectTransform, Vector2.zero, Vector2.zero);

            CreateVignette("Top Vignette", new Color(0f, 0f, 0f, 0.64f), true, 220f);
            CreateVignette("Bottom Vignette", new Color(0f, 0f, 0f, 0.78f), false, 260f);
        }

        private void BuildParticles()
        {
            softDiscSprite = CreateSoftDiscSprite();
            GameObject particleRoot = CreateUiObject("Northern Snow And Smoke Drift", canvas.transform);
            RectTransform rootTransform = particleRoot.GetComponent<RectTransform>();
            Stretch(rootTransform, Vector2.zero, Vector2.zero);

            System.Random random = new System.Random(9437);
            particles = new LoginParticle[76];
            for (int i = 0; i < particles.Length; i++)
            {
                bool smoke = i < 20;
                GameObject particleObject = CreateUiObject(smoke ? "Low Fog Drift" : "Wind Snow Fleck", rootTransform);
                Image image = particleObject.AddComponent<Image>();
                image.sprite = softDiscSprite;
                image.raycastTarget = false;
                RectTransform rect = image.rectTransform;
                float size = smoke ? random.Next(42, 116) : random.Next(3, 7);
                rect.sizeDelta = new Vector2(size, size);
                rect.anchoredPosition = new Vector2(random.Next(-ReferenceWidth / 2, ReferenceWidth / 2), random.Next(-ReferenceHeight / 2, ReferenceHeight / 2));
                float speed = smoke
                    ? Mathf.Lerp(0.015f, 0.046f, (float)random.NextDouble())
                    : Mathf.Lerp(0.09f, 0.26f, (float)random.NextDouble());
                float baseAlpha = smoke
                    ? Mathf.Lerp(0.020f, 0.055f, (float)random.NextDouble())
                    : Mathf.Lerp(0.10f, 0.24f, (float)random.NextDouble());
                Color particleColor = smoke
                    ? new Color(0.30f, 0.35f, 0.37f, 1f)
                    : new Color(0.58f, 0.66f, 0.70f, 1f);
                image.color = new Color(particleColor.r, particleColor.g, particleColor.b, baseAlpha);
                particles[i] = new LoginParticle
                {
                    rect = rect,
                    image = image,
                    smoke = smoke,
                    seed = (float)random.NextDouble() * 999f,
                    speed = speed,
                    baseAlpha = baseAlpha,
                    color = particleColor
                };
            }
        }

        private void BuildOriginalIronCrest()
        {
            RawImage crest = CreateUiObject("Original Psycho Iron Crest", canvas.transform).AddComponent<RawImage>();
            crest.texture = GetIronCrestTexture();
            crest.raycastTarget = false;
            RectTransform rect = crest.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(520f, 700f);
            rect.anchoredPosition = new Vector2(-350f, 48f);
        }

        private void BuildPanel()
        {
            BuildOriginalIronCrest();

            GameObject root = CreateUiObject("Northern Main Menu", canvas.transform);
            panelRoot = root.GetComponent<RectTransform>();
            panelRoot.anchorMin = new Vector2(1f, 0.5f);
            panelRoot.anchorMax = new Vector2(1f, 0.5f);
            panelRoot.pivot = new Vector2(1f, 0.5f);
            panelRoot.sizeDelta = new Vector2(360f, 520f);
            panelRoot.anchoredPosition = new Vector2(-92f, -28f);

            CreateText("PSYCHO", canvas.transform, 76, FontStyle.Bold, new Color(0.70f, 0.72f, 0.70f, 0.94f), new Vector2(-355f, -352f), new Vector2(560f, 84f));
            CreateText("NORTHERN TEST WORLD", canvas.transform, 16, FontStyle.Bold, new Color(0.43f, 0.48f, 0.50f, 0.82f), new Vector2(-355f, -414f), new Vector2(420f, 32f));

            previewButton = CreateMenuButton("CONTINUE", new Vector2(0f, 150f), true, ContinueHostedWorld);
            newGameButton = CreateMenuButton("NEW", new Vector2(0f, 86f), false, ShowCharacterCreator);
            loginButton = CreateMenuButton("LOGIN", new Vector2(0f, 22f), false, OnLoginClicked);
            CreateMenuButton("QUIT", new Vector2(0f, -42f), false, QuitToDesktop);

            usernameField = CreateInput("Username", new Vector2(-4f, -136f), false);
            passwordField = CreateInput("Password", new Vector2(-4f, -194f), true);
            statusText = CreateText("Cold mist rolls over the pass.", panelRoot, 14, FontStyle.Normal, new Color(0.58f, 0.66f, 0.70f, 0.86f), new Vector2(-4f, -246f), new Vector2(310f, 34f));
        }

        private void BuildCharacterCreatorPanel()
        {
            GameObject root = CreateUiObject("Condemned Prisoner Creator", canvas.transform);
            panelRoot = root.GetComponent<RectTransform>();
            panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
            panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
            panelRoot.pivot = new Vector2(0.5f, 0.5f);
            panelRoot.sizeDelta = new Vector2(900f, 560f);
            panelRoot.anchoredPosition = Vector2.zero;

            RawImage panel = CreateUiObject("Aged Iron Parchment Panel", panelRoot).AddComponent<RawImage>();
            panel.texture = GetCrystalPanelTexture();
            panel.raycastTarget = false;
            Stretch(panel.rectTransform, Vector2.zero, Vector2.zero);

            CreateText("Prison Intake", panelRoot, 48, FontStyle.Bold, new Color(0.72f, 0.70f, 0.62f, 1f), new Vector2(0f, 182f), new Vector2(620f, 64f));
            CreateText("The priest asks for the name that will be written beside the morning noose.", panelRoot, 18, FontStyle.Normal, new Color(0.58f, 0.64f, 0.66f, 1f), new Vector2(0f, 136f), new Vector2(760f, 34f));

            characterNameField = CreateInput("Prisoner name", new Vector2(-180f, 72f), false);
            raceDropdown = CreateDropdown("Race", new Vector2(204f, 72f), new[] { "Human", "Highlander", "Dwarf", "Elf" });
            hairDropdown = CreateDropdown("Hair", new Vector2(-180f, 6f), new[] { "Short", "Braided", "Shaved", "Long" });
            faceSlider = CreateSlider("Face", new Vector2(204f, 12f), 0f, 7f, 2f);
            hairColorSlider = CreateSlider("Hair Tone", new Vector2(-180f, -62f), 0f, 5f, 1f);
            buildSlider = CreateSlider("Build", new Vector2(204f, -62f), 0f, 4f, 2f);

            CreateButton("Begin Sentence", new Vector2(114f, -152f), new Vector2(210f, 50f), StartNewGameFromCreator);
            CreateButton("Back", new Vector2(-142f, -152f), new Vector2(144f, 50f), RebuildUi);
            statusText = CreateText("The cell waits.", panelRoot, 16, FontStyle.Normal, new Color(0.80f, 0.91f, 0.96f, 1f), new Vector2(0f, -218f), new Vector2(620f, 32f));
        }

        private InputField CreateInput(string placeholder, Vector2 anchoredPosition, bool password)
        {
            GameObject fieldObject = CreateUiObject($"{placeholder} Field", panelRoot);
            RectTransform rect = fieldObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            float width = panelRoot != null && panelRoot.sizeDelta.x < 500f ? 300f : 460f;
            rect.sizeDelta = new Vector2(width, 48f);
            rect.anchoredPosition = anchoredPosition;

            Image image = fieldObject.AddComponent<Image>();
            image.color = new Color(0.015f, 0.018f, 0.020f, 0.84f);

            InputField input = fieldObject.AddComponent<InputField>();
            input.targetGraphic = image;
            input.transition = Selectable.Transition.ColorTint;
            input.colors = CreateSelectableColors(new Color(0.58f, 0.64f, 0.66f, 1f));
            input.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
            input.asteriskChar = '*';

            Text text = CreateTextObject("Text", fieldObject.transform, 20, FontStyle.Normal, new Color(0.78f, 0.82f, 0.82f, 1f), TextAnchor.MiddleLeft);
            Stretch(text.rectTransform, new Vector2(18f, 4f), new Vector2(18f, 4f));
            input.textComponent = text;

            Text placeholderText = CreateTextObject("Placeholder", fieldObject.transform, 18, FontStyle.Normal, new Color(0.42f, 0.48f, 0.50f, 0.72f), TextAnchor.MiddleLeft);
            Stretch(placeholderText.rectTransform, new Vector2(18f, 4f), new Vector2(18f, 4f));
            placeholderText.text = placeholder;
            input.placeholder = placeholderText;
            return input;
        }

        private Button CreateMenuButton(string label, Vector2 anchoredPosition, bool primary, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = CreateUiObject(label + " Menu Button", panelRoot);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(310f, 52f);
            rect.anchoredPosition = anchoredPosition;

            Image image = buttonObject.AddComponent<Image>();
            image.color = primary ? new Color(0.12f, 0.13f, 0.13f, 0.26f) : new Color(0f, 0f, 0f, 0f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = CreateMenuButtonColors(primary);
            button.onClick.AddListener(action);

            Text text = CreateTextObject("Label", buttonObject.transform, primary ? 32 : 25, primary ? FontStyle.Bold : FontStyle.Normal, primary ? new Color(0.88f, 0.88f, 0.82f, 1f) : new Color(0.56f, 0.58f, 0.57f, 0.96f), TextAnchor.MiddleRight);
            text.text = label;
            Stretch(text.rectTransform, Vector2.zero, Vector2.zero);

            if (primary)
            {
                Image marker = CreateUiObject("Iron Knot Marker", buttonObject.transform).AddComponent<Image>();
                marker.color = new Color(0.74f, 0.76f, 0.72f, 0.92f);
                RectTransform markerRect = marker.rectTransform;
                markerRect.anchorMin = new Vector2(1f, 0.5f);
                markerRect.anchorMax = new Vector2(1f, 0.5f);
                markerRect.pivot = new Vector2(0.5f, 0.5f);
                markerRect.sizeDelta = new Vector2(18f, 18f);
                markerRect.anchoredPosition = new Vector2(26f, 0f);
                markerRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }

            return button;
        }

        private Button CreateButton(string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = CreateUiObject(label + " Button", panelRoot);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.10f, 0.105f, 0.105f, 0.94f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = CreateSelectableColors(new Color(0.55f, 0.57f, 0.54f, 1f));
            button.onClick.AddListener(action);

            Text text = CreateTextObject("Label", buttonObject.transform, 20, FontStyle.Bold, new Color(0.78f, 0.74f, 0.62f, 1f), TextAnchor.MiddleCenter);
            text.text = label;
            Stretch(text.rectTransform, Vector2.zero, Vector2.zero);
            return button;
        }

        private Dropdown CreateDropdown(string label, Vector2 anchoredPosition, string[] options)
        {
            GameObject dropdownObject = CreateUiObject(label + " Dropdown", panelRoot);
            RectTransform rect = dropdownObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300f, 48f);
            rect.anchoredPosition = anchoredPosition;

            Image image = dropdownObject.AddComponent<Image>();
            image.color = new Color(0.015f, 0.018f, 0.020f, 0.88f);

            Dropdown dropdown = dropdownObject.AddComponent<Dropdown>();
            dropdown.targetGraphic = image;
            dropdown.options.Clear();
            for (int i = 0; i < options.Length; i++)
            {
                dropdown.options.Add(new Dropdown.OptionData(options[i]));
            }

            Text caption = CreateTextObject("Label", dropdownObject.transform, 18, FontStyle.Bold, new Color(0.76f, 0.72f, 0.62f, 1f), TextAnchor.MiddleLeft);
            caption.text = label + ": " + (options.Length == 0 ? string.Empty : options[0]);
            Stretch(caption.rectTransform, new Vector2(16f, 4f), new Vector2(16f, 4f));
            dropdown.captionText = caption;

            RectTransform template = CreateUiObject("Template", dropdownObject.transform).GetComponent<RectTransform>();
            template.anchorMin = new Vector2(0f, 0f);
            template.anchorMax = new Vector2(1f, 0f);
            template.pivot = new Vector2(0.5f, 1f);
            template.anchoredPosition = new Vector2(0f, -4f);
            template.sizeDelta = new Vector2(0f, 158f);
            Image templateImage = template.gameObject.AddComponent<Image>();
            templateImage.color = new Color(0.015f, 0.018f, 0.020f, 0.98f);

            RectTransform itemRoot = CreateUiObject("Item", template).GetComponent<RectTransform>();
            itemRoot.anchorMin = new Vector2(0f, 1f);
            itemRoot.anchorMax = new Vector2(1f, 1f);
            itemRoot.pivot = new Vector2(0.5f, 1f);
            itemRoot.sizeDelta = new Vector2(0f, 34f);
            itemRoot.anchoredPosition = Vector2.zero;
            Toggle toggle = itemRoot.gameObject.AddComponent<Toggle>();
            Image itemBackground = itemRoot.gameObject.AddComponent<Image>();
            itemBackground.color = new Color(0.075f, 0.080f, 0.080f, 0.94f);
            toggle.targetGraphic = itemBackground;

            Text item = CreateTextObject("Item Label", itemRoot, 16, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
            Stretch(item.rectTransform, new Vector2(14f, 2f), new Vector2(14f, 2f));
            dropdown.template = template;
            dropdown.itemText = item;
            template.gameObject.SetActive(false);
            dropdown.onValueChanged.AddListener(index =>
            {
                if (caption != null && index >= 0 && index < dropdown.options.Count)
                {
                    caption.text = label + ": " + dropdown.options[index].text;
                }
            });
            return dropdown;
        }

        private Slider CreateSlider(string label, Vector2 anchoredPosition, float min, float max, float value)
        {
            GameObject root = CreateUiObject(label + " Slider", panelRoot);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300f, 52f);
            rect.anchoredPosition = anchoredPosition;

            Text labelText = CreateTextObject("Label", root.transform, 16, FontStyle.Bold, new Color(0.76f, 0.72f, 0.62f, 1f), TextAnchor.UpperLeft);
            labelText.text = label;
            labelText.rectTransform.anchorMin = new Vector2(0f, 1f);
            labelText.rectTransform.anchorMax = new Vector2(1f, 1f);
            labelText.rectTransform.pivot = new Vector2(0f, 1f);
            labelText.rectTransform.sizeDelta = new Vector2(0f, 22f);
            labelText.rectTransform.anchoredPosition = new Vector2(0f, 0f);

            RectTransform track = CreateUiObject("Track", root.transform).GetComponent<RectTransform>();
            track.anchorMin = new Vector2(0f, 0.5f);
            track.anchorMax = new Vector2(1f, 0.5f);
            track.pivot = new Vector2(0.5f, 0.5f);
            track.offsetMin = new Vector2(8f, -8f);
            track.offsetMax = new Vector2(-8f, 8f);
            Image trackImage = track.gameObject.AddComponent<Image>();
            trackImage.color = new Color(0.015f, 0.018f, 0.020f, 0.88f);

            RectTransform fill = CreateUiObject("Fill", track).GetComponent<RectTransform>();
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0.5f, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = new Color(0.48f, 0.50f, 0.47f, 0.92f);

            RectTransform handle = CreateUiObject("Handle", track).GetComponent<RectTransform>();
            handle.sizeDelta = new Vector2(18f, 26f);
            Image handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = new Color(0.70f, 0.72f, 0.68f, 1f);

            Slider slider = root.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = true;
            slider.value = value;
            slider.targetGraphic = handleImage;
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private void ShowCharacterCreator()
        {
            ClearExistingUi();
            uiFont = LoadUiFont();
            BuildCanvas();
            BuildBackground();
            BuildParticles();
            BuildCharacterCreatorPanel();
        }

        private void ContinueHostedWorld()
        {
            PlayerPrefs.SetInt("PsychoNewGameActive", 0);
            LoadHostedWorld();
        }

        private void QuitToDesktop()
        {
            Application.Quit();
            SetStatus("Quit requested.");
        }

        private void StartNewGameFromCreator()
        {
            string prisonerName = characterNameField == null ? string.Empty : characterNameField.text.Trim();
            if (string.IsNullOrWhiteSpace(prisonerName))
            {
                SetStatus("The priest refuses a blank name.");
                return;
            }

            PlayerPrefs.SetString("PsychoPlayerName", prisonerName);
            PlayerPrefs.SetString("PsychoPlayerRace", DropdownValue(raceDropdown, "Human"));
            PlayerPrefs.SetString("PsychoPlayerHair", DropdownValue(hairDropdown, "Short"));
            PlayerPrefs.SetInt("PsychoPlayerFace", Mathf.RoundToInt(faceSlider == null ? 0f : faceSlider.value));
            PlayerPrefs.SetInt("PsychoPlayerHairTone", Mathf.RoundToInt(hairColorSlider == null ? 0f : hairColorSlider.value));
            PlayerPrefs.SetInt("PsychoPlayerBuild", Mathf.RoundToInt(buildSlider == null ? 0f : buildSlider.value));
            PlayerPrefs.SetInt("PsychoNewGameActive", 1);
            PlayerPrefs.SetInt("PsychoIntroQuestCompleted", 0);
            PlayerPrefs.Save();
            LoadHostedWorld();
        }

        private static string DropdownValue(Dropdown dropdown, string fallback)
        {
            if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0)
            {
                return fallback;
            }

            int index = Mathf.Clamp(dropdown.value, 0, dropdown.options.Count - 1);
            return dropdown.options[index].text;
        }

        private async void OnLoginClicked()
        {
            if (loginInFlight)
            {
                return;
            }

            EnsureProtocolClient();
            loginInFlight = true;
            SetControlsEnabled(false);
            SetStatus("Connecting...");
            loginCancellation?.Cancel();
            loginCancellation?.Dispose();
            loginCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(18));

            try
            {
                int response = await protocolClient.LoginAsync(usernameField.text, passwordField.text, false, loginCancellation.Token);
                SetStatus(DescribeLoginResponse(response));
                if (response == 2 && loadHostedWorldOnSuccess)
                {
                    LoadHostedWorld();
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Connection failed: {ex.Message}");
            }
            finally
            {
                loginInFlight = false;
                SetControlsEnabled(true);
            }
        }

        private void LoadHostedWorld()
        {
            try
            {
                SceneManager.LoadScene(hostedWorldSceneName);
            }
            catch (Exception ex)
            {
                SetStatus($"Unable to load hosted world: {ex.Message}");
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            if (loginButton != null)
            {
                loginButton.interactable = enabled;
            }

            if (previewButton != null)
            {
                previewButton.interactable = enabled;
            }

            if (newGameButton != null)
            {
                newGameButton.interactable = enabled;
            }

            if (usernameField != null)
            {
                usernameField.interactable = enabled;
            }

            if (passwordField != null)
            {
                passwordField.interactable = enabled;
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private static string DescribeLoginResponse(int response)
        {
            switch (response)
            {
                case 2:
                    return "Login accepted.";
                case 3:
                    return "Invalid username or password.";
                case 4:
                    return "Account disabled.";
                case 5:
                    return "Account already online.";
                case 7:
                    return "World is full.";
                case 14:
                    return "Server is being updated.";
                default:
                    return $"Login response {response}.";
            }
        }

        private void AnimateParticles(float time)
        {
            if (particles == null)
            {
                return;
            }

            for (int i = 0; i < particles.Length; i++)
            {
                LoginParticle particle = particles[i];
                float phase = time * particle.speed + particle.seed;
                if (particle.smoke)
                {
                    float x = -820f + Mathf.Repeat(phase * 150f + i * 83f, 1640f);
                    float y = -260f + Mathf.Sin(phase * 1.2f) * 44f + (i % 5) * 34f;
                    particle.rect.anchoredPosition = new Vector2(x, y);
                    particle.image.color = new Color(particle.color.r, particle.color.g, particle.color.b, particle.baseAlpha * (0.55f + Mathf.Sin(phase) * 0.25f));
                }
                else
                {
                    float x = -910f + Mathf.Repeat(phase * 360f + i * 47f, 1840f);
                    float y = 520f - Mathf.Repeat(phase * 270f + i * 71f, 1040f);
                    x += Mathf.Sin(phase * 2.1f) * 26f;
                    particle.rect.anchoredPosition = new Vector2(x, y);
                    particle.image.color = new Color(particle.color.r, particle.color.g, particle.color.b, particle.baseAlpha * (0.55f + Mathf.Sin(phase * 3.3f) * 0.28f));
                }
            }
        }

        private Text CreateText(string value, Transform parent, int size, FontStyle style, Color color, Vector2 anchoredPosition, Vector2 textSize)
        {
            Text text = CreateTextObject(value + " Text", parent, size, style, color, TextAnchor.MiddleCenter);
            text.text = value;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = textSize;
            rect.anchoredPosition = anchoredPosition;
            return text;
        }

        private Text CreateTextObject(string name, Transform parent, int size, FontStyle style, Color color, TextAnchor alignment)
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.font = uiFont;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Image CreatePanelImage(string name, Transform parent, Color color, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 position)
        {
            GameObject imageObject = CreateUiObject(name, parent);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return image;
        }

        private void CreateVignette(string name, Color color, bool top, float height)
        {
            GameObject imageObject = CreateUiObject(name, canvas.transform);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = top ? new Vector2(0f, 1f) : Vector2.zero;
            rect.anchorMax = top ? Vector2.one : new Vector2(1f, 0f);
            rect.pivot = top ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
            rect.offsetMin = top ? new Vector2(0f, -height) : Vector2.zero;
            rect.offsetMax = top ? Vector2.zero : new Vector2(0f, height);
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void Stretch(RectTransform rect, Vector2 minOffset, Vector2 maxOffset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = minOffset;
            rect.offsetMax = -maxOffset;
        }

        private static ColorBlock CreateSelectableColors(Color highlight)
        {
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = Color.white;
            colors.highlightedColor = highlight;
            colors.pressedColor = new Color(0.34f, 0.36f, 0.34f, 1f);
            colors.selectedColor = highlight;
            colors.disabledColor = new Color(0.18f, 0.18f, 0.18f, 0.50f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            return colors;
        }

        private static ColorBlock CreateMenuButtonColors(bool primary)
        {
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = primary ? new Color(0.12f, 0.13f, 0.13f, 0.26f) : new Color(0f, 0f, 0f, 0f);
            colors.highlightedColor = new Color(0.22f, 0.23f, 0.22f, primary ? 0.44f : 0.22f);
            colors.pressedColor = new Color(0.08f, 0.08f, 0.08f, 0.52f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0f, 0f, 0f, 0f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.10f;
            return colors;
        }

        private Texture2D GetNorthernTitleBackgroundTexture()
        {
            if (northernBackgroundTexture != null)
            {
                return northernBackgroundTexture;
            }

            const int width = 1024;
            const int height = 576;
            northernBackgroundTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Psycho Northern Title Background",
                wrapMode = TextureWrapMode.Clamp
            };

            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                float v = y / (float)(height - 1);
                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)(width - 1);
                    float noise = Mathf.PerlinNoise(u * 7.0f + 13.4f, v * 5.0f + 2.1f);
                    Color color = Color.Lerp(new Color(0.020f, 0.023f, 0.026f, 1f), new Color(0.12f, 0.15f, 0.17f, 1f), Mathf.Pow(v, 0.72f));
                    color = Color.Lerp(color, new Color(0.18f, 0.20f, 0.21f, 1f), noise * 0.12f);

                    float leftMist = Mathf.Clamp01(1f - u * 2.7f) * Mathf.Clamp01(1.25f - v);
                    color = Color.Lerp(color, new Color(0.24f, 0.27f, 0.28f, 1f), leftMist * 0.36f);

                    float mountainLine = 0.44f + Mathf.Sin(u * 17.0f) * 0.035f + Mathf.PerlinNoise(u * 6.0f, 0.43f) * 0.11f;
                    if (v < mountainLine)
                    {
                        float depth = Mathf.Clamp01((mountainLine - v) * 7.0f);
                        color = Color.Lerp(color, new Color(0.018f, 0.020f, 0.022f, 1f), depth * 0.62f);
                    }

                    float lowerFog = Mathf.Clamp01(1f - Mathf.Abs(v - 0.19f) * 5.4f) * (0.22f + noise * 0.26f);
                    color = Color.Lerp(color, new Color(0.19f, 0.22f, 0.23f, 1f), lowerFog * 0.34f);

                    float vignette = Mathf.Clamp01(Mathf.Abs(u - 0.5f) * 1.7f + Mathf.Abs(v - 0.5f) * 1.35f);
                    color = Color.Lerp(color, Color.black, vignette * 0.55f);
                    pixels[x + y * width] = color;
                }
            }

            northernBackgroundTexture.SetPixels32(pixels);
            northernBackgroundTexture.Apply();
            return northernBackgroundTexture;
        }

        private Texture2D GetCrystalPanelTexture()
        {
            if (crystalPanelTexture != null)
            {
                return crystalPanelTexture;
            }

            const int width = 768;
            const int height = 432;
            crystalPanelTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Psycho Aged Iron Login Panel"
            };

            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                float v = y / (float)(height - 1);
                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)(width - 1);
                    float edge = Mathf.Max(Mathf.Abs(u - 0.5f) * 2f, Mathf.Abs(v - 0.5f) * 2f);
                    float highlight = Mathf.Clamp01(1f - edge);
                    float grime = Mathf.PerlinNoise(u * 18f + 3.1f, v * 14f + 1.7f) * 0.12f;
                    Color color = Color.Lerp(new Color(0.018f, 0.019f, 0.018f, 0.94f), new Color(0.105f, 0.105f, 0.098f, 0.90f), highlight);
                    color = Color.Lerp(color, new Color(0.22f, 0.20f, 0.16f, 0.88f), Mathf.Clamp01(1f - v) * 0.10f + grime);
                    if (x < 3 || y < 3 || x > width - 4 || y > height - 4)
                    {
                        color = new Color(0.48f, 0.47f, 0.42f, 0.96f);
                    }
                    else if (x < 8 || y < 8 || x > width - 9 || y > height - 9)
                    {
                        color = Color.Lerp(color, new Color(0.24f, 0.23f, 0.21f, 0.90f), 0.75f);
                    }

                    pixels[x + y * width] = color;
                }
            }

            DrawThickTextureLine(pixels, width, height, 34, 368, 128, 250, new Color32(125, 124, 116, 52), 2);
            DrawThickTextureLine(pixels, width, height, 704, 358, 642, 254, new Color32(115, 116, 112, 50), 2);
            DrawThickTextureLine(pixels, width, height, 28, 82, 740, 82, new Color32(118, 104, 78, 80), 2);

            crystalPanelTexture.SetPixels32(pixels);
            crystalPanelTexture.Apply();
            return crystalPanelTexture;
        }

        private Texture2D GetIronCrestTexture()
        {
            if (ironCrestTexture != null)
            {
                return ironCrestTexture;
            }

            const int width = 512;
            const int height = 768;
            ironCrestTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Psycho Original Iron Crest",
                wrapMode = TextureWrapMode.Clamp
            };

            Color32[] pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(0, 0, 0, 0);
            }

            Color32 dark = new Color32(38, 40, 40, 210);
            Color32 mid = new Color32(105, 110, 108, 235);
            Color32 bright = new Color32(178, 182, 174, 245);
            Color32 shadow = new Color32(10, 10, 10, 160);

            DrawThickTextureLine(pixels, width, height, 256, 102, 256, 690, shadow, 16);
            DrawThickTextureLine(pixels, width, height, 256, 112, 256, 674, mid, 8);
            DrawThickTextureLine(pixels, width, height, 256, 112, 226, 248, bright, 4);
            DrawThickTextureLine(pixels, width, height, 256, 112, 286, 248, bright, 4);

            DrawThickTextureLine(pixels, width, height, 250, 246, 116, 128, shadow, 18);
            DrawThickTextureLine(pixels, width, height, 262, 246, 396, 128, shadow, 18);
            DrawThickTextureLine(pixels, width, height, 250, 246, 116, 128, mid, 9);
            DrawThickTextureLine(pixels, width, height, 262, 246, 396, 128, mid, 9);
            DrawThickTextureLine(pixels, width, height, 116, 128, 70, 356, dark, 13);
            DrawThickTextureLine(pixels, width, height, 396, 128, 442, 356, dark, 13);
            DrawThickTextureLine(pixels, width, height, 70, 356, 168, 500, mid, 10);
            DrawThickTextureLine(pixels, width, height, 442, 356, 344, 500, mid, 10);
            DrawThickTextureLine(pixels, width, height, 168, 500, 230, 424, bright, 5);
            DrawThickTextureLine(pixels, width, height, 344, 500, 282, 424, bright, 5);

            DrawThickTextureLine(pixels, width, height, 178, 392, 226, 344, bright, 7);
            DrawThickTextureLine(pixels, width, height, 334, 392, 286, 344, bright, 7);
            DrawThickTextureLine(pixels, width, height, 198, 548, 256, 670, mid, 8);
            DrawThickTextureLine(pixels, width, height, 314, 548, 256, 670, mid, 8);
            DrawThickTextureLine(pixels, width, height, 218, 316, 256, 276, bright, 5);
            DrawThickTextureLine(pixels, width, height, 294, 316, 256, 276, bright, 5);

            DrawThickTextureLine(pixels, width, height, 132, 154, 94, 220, bright, 4);
            DrawThickTextureLine(pixels, width, height, 380, 154, 418, 220, bright, 4);
            DrawThickTextureLine(pixels, width, height, 256, 674, 232, 724, bright, 5);
            DrawThickTextureLine(pixels, width, height, 256, 674, 280, 724, bright, 5);
            DrawThickTextureLine(pixels, width, height, 232, 724, 256, 748, mid, 4);
            DrawThickTextureLine(pixels, width, height, 280, 724, 256, 748, mid, 4);

            ironCrestTexture.SetPixels32(pixels);
            ironCrestTexture.Apply();
            return ironCrestTexture;
        }

        private Sprite CreateSoftDiscSprite()
        {
            Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false)
            {
                name = "Psycho Login Soft Particle"
            };
            Color32[] pixels = new Color32[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dx = (x - 31.5f) / 31.5f;
                    float dy = (y - 31.5f) / 31.5f;
                    float alpha = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    alpha = alpha * alpha;
                    pixels[x + y * 64] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 64f);
        }

        private static void DrawTextureLine(Color32[] pixels, int width, int height, int x0, int y0, int x1, int y1, Color32 color)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int error = dx - dy;
            while (true)
            {
                if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
                {
                    pixels[x0 + y0 * width] = color;
                }

                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                int doubledError = error * 2;
                if (doubledError > -dy)
                {
                    error -= dy;
                    x0 += sx;
                }

                if (doubledError < dx)
                {
                    error += dx;
                    y0 += sy;
                }
            }
        }

        private static void DrawThickTextureLine(Color32[] pixels, int width, int height, int x0, int y0, int x1, int y1, Color32 color, int radius)
        {
            int clampedRadius = Math.Max(1, radius);
            int radiusSquared = clampedRadius * clampedRadius;
            for (int offsetY = -clampedRadius; offsetY <= clampedRadius; offsetY++)
            {
                for (int offsetX = -clampedRadius; offsetX <= clampedRadius; offsetX++)
                {
                    if (offsetX * offsetX + offsetY * offsetY > radiusSquared)
                    {
                        continue;
                    }

                    DrawTextureLine(
                        pixels,
                        width,
                        height,
                        x0 + offsetX,
                        y0 + offsetY,
                        x1 + offsetX,
                        y1 + offsetY,
                        color);
                }
            }
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

        private void ClearExistingUi()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private sealed class LoginParticle
        {
            public RectTransform rect;
            public Image image;
            public bool smoke;
            public float seed;
            public float speed;
            public float baseAlpha;
            public Color color;
        }
    }
}
