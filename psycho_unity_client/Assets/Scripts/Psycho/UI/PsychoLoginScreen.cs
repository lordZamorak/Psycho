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
        [SerializeField] private Vector2 panelSize = new Vector2(1440f, 810f);
        [SerializeField] private Vector2 shadowOffset = new Vector2(-58f, -64f);

        private Canvas canvas;
        private RawImage backgroundImage;
        private RectTransform panelRoot;
        private Text statusText;
        private InputField usernameField;
        private InputField passwordField;
        private Button loginButton;
        private Button previewButton;
        private Font uiFont;
        private Texture2D crystalPanelTexture;
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
                panelRoot.anchoredPosition = new Vector2(0f, Mathf.Sin(time * 0.68f) * 7f);
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
            Texture2D background = Resources.Load<Texture2D>(BackgroundResource);
            GameObject backgroundObject = CreateUiObject("Smithing Background", canvas.transform);
            backgroundImage = backgroundObject.AddComponent<RawImage>();
            backgroundImage.texture = background;
            backgroundImage.color = Color.white;
            Stretch(backgroundImage.rectTransform, Vector2.zero, Vector2.zero);

            CreateVignette("Top Vignette", new Color(0f, 0f, 0f, 0.34f), true, 128f);
            CreateVignette("Bottom Vignette", new Color(0f, 0f, 0f, 0.46f), false, 190f);
        }

        private void BuildParticles()
        {
            softDiscSprite = CreateSoftDiscSprite();
            GameObject particleRoot = CreateUiObject("Live Photo Particles", canvas.transform);
            RectTransform rootTransform = particleRoot.GetComponent<RectTransform>();
            Stretch(rootTransform, Vector2.zero, Vector2.zero);

            System.Random random = new System.Random(9437);
            particles = new LoginParticle[58];
            for (int i = 0; i < particles.Length; i++)
            {
                bool smoke = i < 12;
                GameObject particleObject = CreateUiObject(smoke ? "Smoke Drift" : "Forge Ember", rootTransform);
                Image image = particleObject.AddComponent<Image>();
                image.sprite = softDiscSprite;
                image.raycastTarget = false;
                RectTransform rect = image.rectTransform;
                float size = smoke ? random.Next(38, 96) : random.Next(3, 9);
                rect.sizeDelta = new Vector2(size, size);
                particles[i] = new LoginParticle
                {
                    rect = rect,
                    image = image,
                    smoke = smoke,
                    seed = (float)random.NextDouble() * 999f,
                    speed = smoke ? UnityEngine.Random.Range(0.025f, 0.07f) : UnityEngine.Random.Range(0.18f, 0.42f),
                    baseAlpha = smoke ? UnityEngine.Random.Range(0.05f, 0.13f) : UnityEngine.Random.Range(0.34f, 0.72f),
                    color = smoke
                        ? new Color(0.56f, 0.63f, 0.69f, 1f)
                        : (i % 5 == 0 ? new Color(0.46f, 0.86f, 1f, 1f) : new Color(1f, UnityEngine.Random.Range(0.46f, 0.78f), 0.24f, 1f))
                };
            }
        }

        private void BuildPanel()
        {
            GameObject root = CreateUiObject("Floating Crystal Login", canvas.transform);
            panelRoot = root.GetComponent<RectTransform>();
            panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
            panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
            panelRoot.pivot = new Vector2(0.5f, 0.5f);
            panelRoot.sizeDelta = panelSize;
            panelRoot.anchoredPosition = Vector2.zero;

            for (int i = 5; i >= 0; i--)
            {
                Image shadow = CreatePanelImage(
                    $"Panel Scene Shadow {i}",
                    panelRoot,
                    new Color(0f, 0f, 0f, 0.10f + i * 0.025f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    panelSize + new Vector2(i * 36f, i * 28f),
                    shadowOffset + new Vector2(-i * 8f, -i * 5f));
                shadow.raycastTarget = false;
            }

            RawImage panel = CreateUiObject("Crystal Panel Texture", panelRoot).AddComponent<RawImage>();
            panel.texture = GetCrystalPanelTexture();
            panel.raycastTarget = false;
            Stretch(panel.rectTransform, Vector2.zero, Vector2.zero);

            CreateText("Psycho", panelRoot, 68, FontStyle.Bold, new Color(0.98f, 0.86f, 0.61f, 1f), new Vector2(0f, 270f), new Vector2(720f, 82f));
            CreateText("Local hosted test world", panelRoot, 24, FontStyle.Normal, new Color(0.76f, 0.86f, 0.94f, 1f), new Vector2(0f, 220f), new Vector2(720f, 38f));
            CreateText("PSYCHO UNITY CLIENT", panelRoot, 15, FontStyle.Bold, new Color(0.50f, 0.86f, 1f, 0.80f), new Vector2(0f, -342f), new Vector2(560f, 32f));

            usernameField = CreateInput("Username", new Vector2(0f, 84f), false);
            passwordField = CreateInput("Password", new Vector2(0f, 12f), true);
            loginButton = CreateButton("Login", new Vector2(106f, -76f), new Vector2(176f, 48f), OnLoginClicked);
            previewButton = CreateButton("Preview World", new Vector2(-110f, -76f), new Vector2(204f, 48f), LoadHostedWorld);
            statusText = CreateText("Ready.", panelRoot, 18, FontStyle.Normal, new Color(0.80f, 0.91f, 0.96f, 1f), new Vector2(0f, -144f), new Vector2(720f, 34f));
        }

        private InputField CreateInput(string placeholder, Vector2 anchoredPosition, bool password)
        {
            GameObject fieldObject = CreateUiObject($"{placeholder} Field", panelRoot);
            RectTransform rect = fieldObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(460f, 48f);
            rect.anchoredPosition = anchoredPosition;

            Image image = fieldObject.AddComponent<Image>();
            image.color = new Color(0.015f, 0.055f, 0.085f, 0.88f);

            InputField input = fieldObject.AddComponent<InputField>();
            input.targetGraphic = image;
            input.transition = Selectable.Transition.ColorTint;
            input.colors = CreateSelectableColors(new Color(0.72f, 0.93f, 1f, 1f));
            input.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
            input.asteriskChar = '*';

            Text text = CreateTextObject("Text", fieldObject.transform, 22, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
            Stretch(text.rectTransform, new Vector2(18f, 4f), new Vector2(18f, 4f));
            input.textComponent = text;

            Text placeholderText = CreateTextObject("Placeholder", fieldObject.transform, 20, FontStyle.Normal, new Color(0.64f, 0.77f, 0.86f, 0.62f), TextAnchor.MiddleLeft);
            Stretch(placeholderText.rectTransform, new Vector2(18f, 4f), new Vector2(18f, 4f));
            placeholderText.text = placeholder;
            input.placeholder = placeholderText;
            return input;
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
            image.color = new Color(0.11f, 0.33f, 0.43f, 0.94f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = CreateSelectableColors(new Color(0.96f, 0.78f, 0.42f, 1f));
            button.onClick.AddListener(action);

            Text text = CreateTextObject("Label", buttonObject.transform, 20, FontStyle.Bold, new Color(1f, 0.91f, 0.72f, 1f), TextAnchor.MiddleCenter);
            text.text = label;
            Stretch(text.rectTransform, Vector2.zero, Vector2.zero);
            return button;
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
                    float x = -610f + Mathf.Sin(phase * 1.7f) * 70f + (i % 4) * 42f;
                    float y = -140f + Mathf.Repeat(phase * 118f, 620f);
                    particle.rect.anchoredPosition = new Vector2(x, y);
                    particle.image.color = new Color(particle.color.r, particle.color.g, particle.color.b, particle.baseAlpha * (0.55f + Mathf.Sin(phase) * 0.25f));
                }
                else
                {
                    float x = -740f + Mathf.Repeat(phase * 520f + i * 37f, 1120f);
                    float y = -290f + Mathf.Repeat(phase * 390f + i * 53f, 590f);
                    x += Mathf.Sin(phase * 3.1f) * 34f;
                    particle.rect.anchoredPosition = new Vector2(x, y);
                    particle.image.color = new Color(particle.color.r, particle.color.g, particle.color.b, particle.baseAlpha * (0.55f + Mathf.Sin(phase * 4.3f) * 0.35f));
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
            colors.pressedColor = new Color(0.76f, 0.96f, 1f, 1f);
            colors.selectedColor = highlight;
            colors.disabledColor = new Color(0.42f, 0.48f, 0.52f, 0.50f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            return colors;
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
                name = "Psycho Crystal Login Panel"
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
                    Color color = Color.Lerp(new Color(0.015f, 0.045f, 0.075f, 0.90f), new Color(0.075f, 0.22f, 0.31f, 0.82f), highlight);
                    color = Color.Lerp(color, new Color(0.18f, 0.42f, 0.56f, 0.65f), Mathf.Clamp01(1f - v) * 0.24f);
                    if (x < 3 || y < 3 || x > width - 4 || y > height - 4)
                    {
                        color = new Color(0.62f, 0.90f, 1f, 0.95f);
                    }
                    else if (x < 8 || y < 8 || x > width - 9 || y > height - 9)
                    {
                        color = Color.Lerp(color, new Color(0.26f, 0.68f, 0.90f, 0.84f), 0.75f);
                    }

                    pixels[x + y * width] = color;
                }
            }

            DrawTextureLine(pixels, width, height, 42, 390, 212, 42, new Color32(190, 246, 255, 72));
            DrawTextureLine(pixels, width, height, 212, 42, 414, 398, new Color32(255, 255, 255, 34));
            DrawTextureLine(pixels, width, height, 696, 36, 534, 396, new Color32(114, 212, 255, 132));
            DrawTextureLine(pixels, width, height, 28, 82, 740, 82, new Color32(245, 194, 104, 90));

            crystalPanelTexture.SetPixels32(pixels);
            crystalPanelTexture.Apply();
            return crystalPanelTexture;
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
