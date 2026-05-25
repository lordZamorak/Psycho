using Psycho.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Psycho.Gameplay
{
    public sealed class PsychoIntroQuestController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Vector3 intakePosition;
        [SerializeField] private Vector3 cellPosition;
        [SerializeField] private Vector3 releasePosition;
        [SerializeField] private Vector3 villageGoalPosition;
        [SerializeField] private float villageGoalRadius = 7.0f;

        private Canvas canvas;
        private Text titleText;
        private Text bodyText;
        private Text objectiveText;
        private Button continueButton;
        private PsychoPlayableCharacter playableCharacter;
        private CharacterController characterController;
        private PsychoHudController hud;
        private IntroStep step;

        private enum IntroStep
        {
            Intake,
            Cell,
            Breakout,
            Escape,
            Complete
        }

        private void Start()
        {
            if (PlayerPrefs.GetInt("PsychoIntroQuestCompleted", 0) == 1 && PlayerPrefs.GetInt("PsychoNewGameActive", 0) == 0)
            {
                enabled = false;
                return;
            }

            if (player == null)
            {
                GameObject foundPlayer = GameObject.Find("Playable Adventurer");
                player = foundPlayer == null ? null : foundPlayer.transform;
            }

            if (player == null)
            {
                enabled = false;
                return;
            }

            playableCharacter = player.GetComponent<PsychoPlayableCharacter>();
            characterController = player.GetComponent<CharacterController>();
            hud = FindAnyObjectByType<PsychoHudController>();
            EnsureEventSystem();
            BuildCanvas();
            BeginIntake();
        }

        private void Update()
        {
            if (step != IntroStep.Escape || player == null)
            {
                return;
            }

            Vector3 delta = player.position - villageGoalPosition;
            delta.y = 0f;
            if (delta.magnitude <= villageGoalRadius)
            {
                CompleteQuest();
            }
        }

        public void Configure(Transform playerTransform, Vector3 intake, Vector3 cell, Vector3 release, Vector3 villageGoal, float goalRadius)
        {
            player = playerTransform;
            intakePosition = intake;
            cellPosition = cell;
            releasePosition = release;
            villageGoalPosition = villageGoal;
            villageGoalRadius = goalRadius;
        }

        private void BeginIntake()
        {
            step = IntroStep.Intake;
            TeleportPlayer(intakePosition);
            SetPlayerLocked(true);
            string prisonerName = PlayerPrefs.GetString("PsychoPlayerName", "Prisoner");
            string race = PlayerPrefs.GetString("PsychoPlayerRace", "Human");
            string hair = PlayerPrefs.GetString("PsychoPlayerHair", "Short");
            SetPanel(
                "Prison Intake",
                "A tired priest in rough cloth lowers his quill.\n\n\"Name? Race? Hair? The record must be right before dawn.\"\n\n" + prisonerName + "  |  " + race + "  |  " + hair,
                "Submit to intake");
            SetObjective("Quest: Gallows Dawn\nObjective: survive processing.");
        }

        private void MoveToCell()
        {
            step = IntroStep.Cell;
            TeleportPlayer(cellPosition);
            SetPlayerLocked(true);
            SetPanel(
                "The Cell",
                "Iron closes behind you. The keep drinks himself to sleep outside the bars while the village waits for morning.",
                "Wait through the night");
            SetObjective("Objective: wait in the village prison.");
        }

        private void BeginBreakout()
        {
            step = IntroStep.Breakout;
            TeleportPlayer(cellPosition);
            SetPlayerLocked(true);
            SetPanel(
                "Prison Break",
                "A crash splits the dark. Someone has forced the side door. The keep is snoring, the priest is gone, and the gallows rope is still wet with rain.",
                "Run");
            SetObjective("Objective: escape before morning.");
        }

        private void ReleasePlayer()
        {
            step = IntroStep.Escape;
            TeleportPlayer(releasePosition);
            SetPlayerLocked(false);
            SetPanel(
                "Escape",
                "Follow the road lights to the nearby village. Once you reach safety, the world opens.",
                string.Empty);
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
            }

            SetObjective("Objective: reach North Edgeville Farmstead.");
        }

        private void CompleteQuest()
        {
            step = IntroStep.Complete;
            PlayerPrefs.SetInt("PsychoIntroQuestCompleted", 1);
            PlayerPrefs.SetInt("PsychoNewGameActive", 0);
            PlayerPrefs.Save();
            SetPlayerLocked(false);
            SetPanel(
                "Quest Complete",
                "Gallows Dawn complete. You are free to explore Psycho.",
                "Continue");
            SetObjective("Quest complete: Gallows Dawn.");
            if (hud != null)
            {
                hud.SetQuestSummary("Gallows Dawn: Complete");
                hud.ShowToastMessage("Gallows Dawn complete");
            }
        }

        private void OnContinueClicked()
        {
            switch (step)
            {
                case IntroStep.Intake:
                    MoveToCell();
                    break;
                case IntroStep.Cell:
                    BeginBreakout();
                    break;
                case IntroStep.Breakout:
                    ReleasePlayer();
                    break;
                case IntroStep.Complete:
                    if (canvas != null)
                    {
                        canvas.gameObject.SetActive(false);
                    }

                    break;
            }
        }

        private void SetPanel(string title, string body, string buttonLabel)
        {
            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (bodyText != null)
            {
                bodyText.text = body;
            }

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(!string.IsNullOrWhiteSpace(buttonLabel));
                Text label = continueButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = buttonLabel;
                }
            }
        }

        private void SetObjective(string objective)
        {
            if (objectiveText != null)
            {
                objectiveText.text = objective;
            }

            if (hud != null)
            {
                hud.SetQuestSummary(objective);
            }
        }

        private void SetPlayerLocked(bool locked)
        {
            if (playableCharacter != null)
            {
                playableCharacter.enabled = !locked;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void TeleportPlayer(Vector3 position)
        {
            if (player == null)
            {
                return;
            }

            bool controllerWasEnabled = characterController != null && characterController.enabled;
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            player.position = position;
            if (characterController != null)
            {
                characterController.enabled = controllerWasEnabled;
            }
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("Psycho Intro Quest Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform panel = CreateRect("Intro Dialogue Panel", canvas.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(820f, 246f), new Vector2(0f, 54f));
            Image image = panel.gameObject.AddComponent<Image>();
            image.color = new Color(0.015f, 0.040f, 0.055f, 0.92f);

            titleText = CreateText("Prison Intake", panel, 26, FontStyle.Bold, new Color(1f, 0.86f, 0.58f, 1f), TextAnchor.UpperLeft);
            titleText.rectTransform.anchoredPosition = new Vector2(24f, -16f);
            titleText.rectTransform.sizeDelta = new Vector2(760f, 34f);

            bodyText = CreateText(string.Empty, panel, 18, FontStyle.Normal, new Color(0.86f, 0.94f, 0.98f, 1f), TextAnchor.UpperLeft);
            bodyText.rectTransform.anchoredPosition = new Vector2(24f, -58f);
            bodyText.rectTransform.sizeDelta = new Vector2(760f, 112f);

            continueButton = CreateButton("Continue", panel, new Vector2(586f, 24f), new Vector2(190f, 48f));
            continueButton.onClick.AddListener(OnContinueClicked);

            objectiveText = CreateText(string.Empty, canvas.transform, 19, FontStyle.Bold, new Color(1f, 0.92f, 0.62f, 1f), TextAnchor.UpperLeft);
            objectiveText.rectTransform.anchorMin = new Vector2(0f, 1f);
            objectiveText.rectTransform.anchorMax = new Vector2(0f, 1f);
            objectiveText.rectTransform.pivot = new Vector2(0f, 1f);
            objectiveText.rectTransform.anchoredPosition = new Vector2(28f, -28f);
            objectiveText.rectTransform.sizeDelta = new Vector2(760f, 68f);
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 position)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }

        private static Text CreateText(string value, Transform parent, int size, FontStyle style, Color color, TextAnchor anchor)
        {
            GameObject textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = LoadUiFont();
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string label, Transform parent, Vector2 position, Vector2 size)
        {
            RectTransform rect = CreateRect(label + " Button", parent, new Vector2(0f, 0f), new Vector2(0f, 0f), size, position);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.32f, 0.40f, 0.96f);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateText(label, rect, 18, FontStyle.Bold, new Color(1f, 0.88f, 0.62f, 1f), TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static Font LoadUiFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial" }, 18);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            new GameObject("Psycho EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
