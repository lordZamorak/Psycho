using System.Collections;
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
        private Text speakerText;
        private Text bodyText;
        private Text hintText;
        private Text objectiveText;
        private Image topLetterbox;
        private Image bottomLetterbox;
        private Button continueButton;
        private PsychoPlayableCharacter playableCharacter;
        private CharacterController characterController;
        private PsychoHudController hud;
        private IntroStep step;
        private Camera cinematicCamera;
        private Transform originalCameraParent;
        private Vector3 originalCameraLocalPosition;
        private Quaternion originalCameraLocalRotation;
        private float originalCameraFov;
        private bool capturedCamera;
        private GameObject questMarker;
        private float questMarkerBaseY;
        private Coroutine autoAdvanceRoutine;
        private Coroutine hidePanelRoutine;
        private Coroutine cameraMoveRoutine;

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
            BuildQuestMarker();
            BeginIntake();
        }

        private void Update()
        {
            UpdateQuestMarker();

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
            SetLetterboxVisible(true);
            SetCinematicCamera(intakePosition + new Vector3(3.1f, 2.0f, -3.5f), intakePosition + new Vector3(-0.35f, 1.15f, -0.25f), 42f, 1.25f);
            MoveQuestMarker(intakePosition + Vector3.up * 2.15f);
            SetPanel(
                "Prison Intake",
                "Rough-Cloth Priest",
                "The priest keeps his eyes on the ledger while the rope creaks outside.\n\n\"Name for the record: " + prisonerName + ". Race: " + race + ". Hair: " + hair + ". At dawn, the village watches. Until then, you belong to the cell.\"",
                "Accept the record",
                9.0f);
            SetObjective("Quest: Gallows Dawn\nObjective: survive the prison intake.");
        }

        private void MoveToCell()
        {
            step = IntroStep.Cell;
            TeleportPlayer(cellPosition);
            SetPlayerLocked(true);
            SetLetterboxVisible(true);
            SetCinematicCamera(cellPosition + new Vector3(2.6f, 1.8f, -3.0f), cellPosition + new Vector3(0f, 1.02f, 0.18f), 40f, 1.0f);
            MoveQuestMarker(cellPosition + Vector3.up * 2.35f);
            SetPanel(
                "The Cell",
                "Prison Keep",
                "Iron slams shut. The keep sets your confiscated gear by the door, drinks from a clay cup, and mutters through the bars.\n\n\"Sleep if you can. Morning comes quick for condemned folk.\"",
                "Wait through the night",
                8.0f);
            SetObjective("Objective: wait in the village prison while the keep sleeps.");
        }

        private void BeginBreakout()
        {
            step = IntroStep.Breakout;
            TeleportPlayer(cellPosition);
            SetPlayerLocked(true);
            SetLetterboxVisible(true);
            SetCinematicCamera(releasePosition + new Vector3(2.0f, 1.55f, -2.8f), cellPosition + new Vector3(0.2f, 1.05f, -0.42f), 44f, 0.85f);
            MoveQuestMarker(releasePosition + Vector3.up * 1.85f);
            SetPanel(
                "Prison Break",
                "Breakout Stranger",
                "A side gate bursts open in the rain. The keep does not wake. A stranger throws a ring of keys into the mud.\n\n\"Move. Follow the road lamps north. If you stop at the gallows, you die here.\"",
                "Run",
                7.5f);
            SetObjective("Objective: escape before morning.");
        }

        private void ReleasePlayer()
        {
            step = IntroStep.Escape;
            TeleportPlayer(releasePosition);
            SetPlayerLocked(false);
            RestorePlayerCamera();
            SetLetterboxVisible(false);
            MoveQuestMarker(villageGoalPosition + Vector3.up * 2.5f);
            SetPanel(
                "Escape",
                "Quest",
                "The gate is open. Follow the road markers to North Edgeville Farmstead. Reach the village to finish Gallows Dawn and begin free exploration.",
                string.Empty,
                0f);
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
            }

            SchedulePanelHide(5.0f);
            SetObjective("Objective: reach North Edgeville Farmstead.");
        }

        private void CompleteQuest()
        {
            step = IntroStep.Complete;
            PlayerPrefs.SetInt("PsychoIntroQuestCompleted", 1);
            PlayerPrefs.SetInt("PsychoNewGameActive", 0);
            PlayerPrefs.Save();
            SetPlayerLocked(false);
            RestorePlayerCamera();
            SetLetterboxVisible(false);
            MoveQuestMarker(villageGoalPosition + Vector3.up * 2.5f);
            SetPanel(
                "Quest Complete",
                "Quest",
                "Gallows Dawn complete. The village lights are behind you now, and the roads, forests, mountains, giant camps, and cities of Psycho are open.",
                "Continue",
                0f);
            SetObjective("Quest complete: Gallows Dawn.");
            if (hud != null)
            {
                hud.SetQuestSummary("Gallows Dawn: Complete");
                hud.ShowToastMessage("Gallows Dawn complete");
            }
        }

        private void OnContinueClicked()
        {
            CancelAutoAdvance();
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

                    if (questMarker != null)
                    {
                        questMarker.SetActive(false);
                    }

                    break;
            }
        }

        private void SetPanel(string title, string speaker, string body, string buttonLabel, float autoAdvanceSeconds)
        {
            CancelHidePanel();
            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (speakerText != null)
            {
                speakerText.text = speaker;
            }

            if (bodyText != null)
            {
                bodyText.text = body;
            }

            if (hintText != null)
            {
                hintText.text = autoAdvanceSeconds > 0f ? "Continue or wait for the scene to advance." : string.Empty;
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

            if (autoAdvanceSeconds > 0f)
            {
                autoAdvanceRoutine = StartCoroutine(AutoAdvanceAfter(autoAdvanceSeconds));
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

        private IEnumerator AutoAdvanceAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            autoAdvanceRoutine = null;
            OnContinueClicked();
        }

        private IEnumerator HidePanelAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            hidePanelRoutine = null;
            if (canvas != null && step == IntroStep.Escape)
            {
                canvas.gameObject.SetActive(false);
            }
        }

        private void SchedulePanelHide(float seconds)
        {
            CancelHidePanel();
            hidePanelRoutine = StartCoroutine(HidePanelAfter(seconds));
        }

        private void CancelAutoAdvance()
        {
            if (autoAdvanceRoutine != null)
            {
                StopCoroutine(autoAdvanceRoutine);
                autoAdvanceRoutine = null;
            }
        }

        private void CancelHidePanel()
        {
            if (hidePanelRoutine != null)
            {
                StopCoroutine(hidePanelRoutine);
                hidePanelRoutine = null;
            }
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

        private void SetCinematicCamera(Vector3 position, Vector3 lookAt, float fieldOfView, float blendSeconds = 0f)
        {
            cinematicCamera = cinematicCamera != null ? cinematicCamera : Camera.main;
            if (cinematicCamera == null)
            {
                return;
            }

            if (!capturedCamera)
            {
                originalCameraParent = cinematicCamera.transform.parent;
                originalCameraLocalPosition = cinematicCamera.transform.localPosition;
                originalCameraLocalRotation = cinematicCamera.transform.localRotation;
                originalCameraFov = cinematicCamera.fieldOfView;
                capturedCamera = true;
            }

            cinematicCamera.transform.SetParent(null, true);
            Quaternion targetRotation = Quaternion.LookRotation(lookAt - position, Vector3.up);
            if (blendSeconds <= 0.01f)
            {
                cinematicCamera.transform.position = position;
                cinematicCamera.transform.rotation = targetRotation;
                cinematicCamera.fieldOfView = fieldOfView;
                return;
            }

            CancelCameraMove();
            cameraMoveRoutine = StartCoroutine(BlendCameraTo(position, targetRotation, fieldOfView, blendSeconds));
        }

        private void RestorePlayerCamera()
        {
            CancelCameraMove();
            if (!capturedCamera || cinematicCamera == null)
            {
                return;
            }

            cinematicCamera.transform.SetParent(originalCameraParent, false);
            cinematicCamera.transform.localPosition = originalCameraLocalPosition;
            cinematicCamera.transform.localRotation = originalCameraLocalRotation;
            cinematicCamera.fieldOfView = originalCameraFov;
            capturedCamera = false;
        }

        private IEnumerator BlendCameraTo(Vector3 targetPosition, Quaternion targetRotation, float targetFov, float seconds)
        {
            Vector3 startPosition = cinematicCamera.transform.position;
            Quaternion startRotation = cinematicCamera.transform.rotation;
            float startFov = cinematicCamera.fieldOfView;
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / seconds);
                t = t * t * (3f - 2f * t);
                cinematicCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                cinematicCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                cinematicCamera.fieldOfView = Mathf.Lerp(startFov, targetFov, t);
                yield return null;
            }

            cinematicCamera.transform.position = targetPosition;
            cinematicCamera.transform.rotation = targetRotation;
            cinematicCamera.fieldOfView = targetFov;
            cameraMoveRoutine = null;
        }

        private void CancelCameraMove()
        {
            if (cameraMoveRoutine != null)
            {
                StopCoroutine(cameraMoveRoutine);
                cameraMoveRoutine = null;
            }
        }

        private void BuildQuestMarker()
        {
            questMarker = new GameObject("Gallows Dawn Runtime Quest Marker");
            questMarker.SetActive(false);

            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(1.0f, 0.76f, 0.18f, 1f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(1.0f, 0.52f, 0.08f, 1f) * 0.8f);

            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Marker Stem";
            stem.transform.SetParent(questMarker.transform, false);
            stem.transform.localPosition = new Vector3(0f, -0.34f, 0f);
            stem.transform.localScale = new Vector3(0.04f, 0.34f, 0.04f);
            stem.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(stem.GetComponent<Collider>());

            GameObject diamond = GameObject.CreatePrimitive(PrimitiveType.Cube);
            diamond.name = "Marker Diamond";
            diamond.transform.SetParent(questMarker.transform, false);
            diamond.transform.localPosition = Vector3.zero;
            diamond.transform.localRotation = Quaternion.Euler(42f, 45f, 0f);
            diamond.transform.localScale = new Vector3(0.34f, 0.34f, 0.34f);
            diamond.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(diamond.GetComponent<Collider>());
        }

        private void MoveQuestMarker(Vector3 position)
        {
            if (questMarker == null)
            {
                return;
            }

            questMarker.SetActive(true);
            questMarker.transform.position = position;
            questMarkerBaseY = position.y;
        }

        private void UpdateQuestMarker()
        {
            if (questMarker == null || !questMarker.activeSelf)
            {
                return;
            }

            Vector3 position = questMarker.transform.position;
            position.y = questMarkerBaseY + Mathf.Sin(Time.time * 2.3f) * 0.12f;
            questMarker.transform.position = position;
            questMarker.transform.Rotate(0f, 70f * Time.deltaTime, 0f, Space.World);
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

            topLetterbox = CreateLetterbox("Top Cinematic Bar", canvas.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1920f, 92f), Vector2.zero);
            bottomLetterbox = CreateLetterbox("Bottom Cinematic Bar", canvas.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1920f, 92f), Vector2.zero);
            SetLetterboxVisible(false);

            RectTransform panel = CreateRect("Intro Dialogue Panel", canvas.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(940f, 292f), new Vector2(0f, 50f));
            Image image = panel.gameObject.AddComponent<Image>();
            image.color = new Color(0.015f, 0.040f, 0.055f, 0.92f);

            titleText = CreateText("Prison Intake", panel, 28, FontStyle.Bold, new Color(1f, 0.86f, 0.58f, 1f), TextAnchor.UpperLeft);
            titleText.rectTransform.anchoredPosition = new Vector2(28f, -16f);
            titleText.rectTransform.sizeDelta = new Vector2(850f, 34f);

            speakerText = CreateText(string.Empty, panel, 17, FontStyle.Bold, new Color(0.62f, 0.88f, 1f, 1f), TextAnchor.UpperLeft);
            speakerText.rectTransform.anchoredPosition = new Vector2(28f, -52f);
            speakerText.rectTransform.sizeDelta = new Vector2(850f, 28f);

            bodyText = CreateText(string.Empty, panel, 18, FontStyle.Normal, new Color(0.86f, 0.94f, 0.98f, 1f), TextAnchor.UpperLeft);
            bodyText.rectTransform.anchoredPosition = new Vector2(28f, -82f);
            bodyText.rectTransform.sizeDelta = new Vector2(872f, 142f);

            hintText = CreateText(string.Empty, panel, 14, FontStyle.Italic, new Color(0.74f, 0.86f, 0.90f, 0.92f), TextAnchor.MiddleLeft);
            hintText.rectTransform.anchoredPosition = new Vector2(28f, 18f);
            hintText.rectTransform.sizeDelta = new Vector2(430f, 32f);

            continueButton = CreateButton("Continue", panel, new Vector2(704f, 24f), new Vector2(190f, 48f));
            continueButton.onClick.AddListener(OnContinueClicked);

            objectiveText = CreateText(string.Empty, canvas.transform, 19, FontStyle.Bold, new Color(1f, 0.92f, 0.62f, 1f), TextAnchor.UpperLeft);
            objectiveText.rectTransform.anchorMin = new Vector2(0f, 1f);
            objectiveText.rectTransform.anchorMax = new Vector2(0f, 1f);
            objectiveText.rectTransform.pivot = new Vector2(0f, 1f);
            objectiveText.rectTransform.anchoredPosition = new Vector2(28f, -28f);
            objectiveText.rectTransform.sizeDelta = new Vector2(760f, 68f);
        }

        private void SetLetterboxVisible(bool visible)
        {
            Color color = visible ? new Color(0f, 0f, 0f, 0.72f) : new Color(0f, 0f, 0f, 0f);
            if (topLetterbox != null)
            {
                topLetterbox.color = color;
            }

            if (bottomLetterbox != null)
            {
                bottomLetterbox.color = color;
            }
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

        private static Image CreateLetterbox(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 position)
        {
            RectTransform rect = CreateRect(name, parent, anchor, pivot, size, position);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = false;
            return image;
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
