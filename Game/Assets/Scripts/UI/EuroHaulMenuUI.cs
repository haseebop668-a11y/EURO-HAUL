using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EuroHaul.Career;

namespace EuroHaul.UI
{
    /// <summary>
    /// Lightweight, runtime-built front/pause/settings UI for the prototype scene.
    /// Uses built-in uGUI only so the project does not need a paid UI package.
    /// </summary>
    public sealed class EuroHaulMenuUI : MonoBehaviour
    {
        private static readonly Color Navy = new Color(0.025f, 0.055f, 0.09f, 0.96f);
        private static readonly Color Panel = new Color(0.055f, 0.095f, 0.14f, 0.98f);
        private static readonly Color Blue = new Color(0.08f, 0.38f, 0.78f, 1f);
        private static readonly Color Gold = new Color(0.92f, 0.68f, 0.25f, 1f);
        private static readonly Color White = new Color(0.93f, 0.96f, 1f, 1f);
        private static readonly Color Muted = new Color(0.62f, 0.71f, 0.8f, 1f);

        private Canvas canvas;
        private GameObject homePanel;
        private GameObject settingsPanel;
        private GameObject pausePanel;
        private Text qualityValue;
        private Text fpsValue;
        private Button pauseButton;
        private bool settingsReturnToPause;
        private int qualityIndex;
        private readonly int[] fpsOptions = { 30, 60, 90, 120 };
        private int fpsIndex = 1;
        private bool started;

        private void Start()
        {
            EnsureEventSystem();
            BuildCanvas();
            qualityIndex = Mathf.Clamp(PlayerPrefs.GetInt("EH_Quality", QualitySettings.GetQualityLevel()), 0,
                Mathf.Max(0, QualitySettings.names.Length - 1));
            fpsIndex = Mathf.Clamp(PlayerPrefs.GetInt("EH_FPS", 1), 0, fpsOptions.Length - 1);
            ApplyQuality();
            ApplyFps();
            Time.timeScale = 0f;
            ShowHome();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && started)
            {
                if (pausePanel.activeSelf) Resume();
                else Pause();
            }
        }

        private void BuildCanvas()
        {
            GameObject root = new GameObject("EuroHaulMenuCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            homePanel = CreatePanel(root.transform, "HomePanel");
            settingsPanel = CreatePanel(root.transform, "SettingsPanel");
            pausePanel = CreatePanel(root.transform, "PausePanel");

            BuildHome();
            BuildSettings();
            BuildPause();

            pauseButton = CreateButton(root.transform, "Ⅱ", new Vector2(1f, 1f),
                new Vector2(-78f, -58f), new Vector2(92f, 68f), Blue, Pause);
            pauseButton.gameObject.name = "PauseButton";
            pauseButton.gameObject.SetActive(false);
        }

        private GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = Navy;
            return panel;
        }

        private void BuildHome()
        {
            CreateText(homePanel.transform, "EURO HAUL", 66, Gold, new Vector2(0.5f, 0.76f),
                new Vector2(0f, -80f), new Vector2(900f, 100f), FontStyle.Bold);
            CreateText(homePanel.transform, "DRIVE  •  DELIVER  •  BUILD YOUR FLEET", 22, Muted,
                new Vector2(0.5f, 0.68f), Vector2.zero, new Vector2(900f, 48f), FontStyle.Normal);
            CreateText(homePanel.transform, "TRUCK LAB  /  EARLY ACCESS PROTOTYPE", 17, Muted,
                new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(900f, 42f), FontStyle.Normal);

            CreateButton(homePanel.transform, "CONTINUE CAREER", new Vector2(0.5f, 0.54f),
                Vector2.zero, new Vector2(430f, 76f), Blue, BeginDrive);
            CreateButton(homePanel.transform, "NEW CAREER", new Vector2(0.5f, 0.43f),
                Vector2.zero, new Vector2(430f, 68f), new Color(0.12f, 0.27f, 0.38f, 1f), StartNewCareer);
            CreateButton(homePanel.transform, "SETTINGS", new Vector2(0.5f, 0.32f),
                Vector2.zero, new Vector2(430f, 64f), Panel, ShowSettings);
            CreateButton(homePanel.transform, "EXIT", new Vector2(0.5f, 0.22f),
                Vector2.zero, new Vector2(430f, 60f), new Color(0.22f, 0.12f, 0.12f, 1f), Quit);
        }

        private void BuildSettings()
        {
            CreateText(settingsPanel.transform, "SETTINGS", 48, Gold, new Vector2(0.5f, 0.76f),
                Vector2.zero, new Vector2(800f, 80f), FontStyle.Bold);
            qualityValue = CreateText(settingsPanel.transform, "", 24, White, new Vector2(0.5f, 0.59f),
                Vector2.zero, new Vector2(600f, 54f), FontStyle.Normal);
            fpsValue = CreateText(settingsPanel.transform, "", 24, White, new Vector2(0.5f, 0.47f),
                Vector2.zero, new Vector2(600f, 54f), FontStyle.Normal);
            CreateButton(settingsPanel.transform, "GRAPHICS QUALITY  ◀ / ▶", new Vector2(0.5f, 0.62f),
                new Vector2(0f, -55f), new Vector2(500f, 64f), Panel, CycleQuality);
            CreateButton(settingsPanel.transform, "FPS LIMIT  ◀ / ▶", new Vector2(0.5f, 0.49f),
                new Vector2(0f, -55f), new Vector2(500f, 64f), Panel, CycleFps);
            CreateText(settingsPanel.transform, "120 FPS is available only when the device can sustain it.",
                17, Muted, new Vector2(0.5f, 0.34f), Vector2.zero, new Vector2(900f, 44f), FontStyle.Normal);
            CreateButton(settingsPanel.transform, "BACK", new Vector2(0.5f, 0.22f),
                Vector2.zero, new Vector2(360f, 68f), Blue, ShowHome);
            RefreshSettingsLabels();
        }

        private void BuildPause()
        {
            CreateText(pausePanel.transform, "PAUSED", 54, Gold, new Vector2(0.5f, 0.68f),
                Vector2.zero, new Vector2(700f, 80f), FontStyle.Bold);
            CreateButton(pausePanel.transform, "RESUME", new Vector2(0.5f, 0.53f),
                Vector2.zero, new Vector2(400f, 76f), Blue, Resume);
            CreateButton(pausePanel.transform, "SETTINGS", new Vector2(0.5f, 0.41f),
                Vector2.zero, new Vector2(400f, 68f), Panel, ShowSettingsFromPause);
            CreateButton(pausePanel.transform, "MAIN MENU", new Vector2(0.5f, 0.29f),
                Vector2.zero, new Vector2(400f, 68f), Panel, ReturnToHome);
        }

        private void StartNewCareer()
        {
            if (CareerManager.Instance != null)
                CareerManager.Instance.StartNewCareer("Driver");
            BeginDrive();
        }

        private void BeginDrive()
        {
            started = true;
            Time.timeScale = 1f;
            pauseButton.gameObject.SetActive(true);
            homePanel.SetActive(false);
            settingsPanel.SetActive(false);
            pausePanel.SetActive(false);
        }

        private void Pause()
        {
            if (!started) return;
            Time.timeScale = 0f;
            pauseButton.gameObject.SetActive(false);
            pausePanel.SetActive(true);
            settingsPanel.SetActive(false);
            homePanel.SetActive(false);
        }

        private void Resume()
        {
            started = true;
            Time.timeScale = 1f;
            pauseButton.gameObject.SetActive(true);
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
        }

        private void ReturnToHome()
        {
            started = false;
            Time.timeScale = 0f;
            pauseButton.gameObject.SetActive(false);
            ShowHome();
        }

        private void ShowHome()
        {
            if (settingsReturnToPause && started)
            {
                settingsReturnToPause = false;
                pausePanel.SetActive(true);
                settingsPanel.SetActive(false);
                homePanel.SetActive(false);
                return;
            }
            homePanel.SetActive(true);
            settingsPanel.SetActive(false);
            pausePanel.SetActive(false);
        }

        private void ShowSettings()
        {
            settingsReturnToPause = false;
            homePanel.SetActive(false);
            settingsPanel.SetActive(true);
            pausePanel.SetActive(false);
        }

        private void ShowSettingsFromPause()
        {
            settingsReturnToPause = true;
            pausePanel.SetActive(false);
            settingsPanel.SetActive(true);
        }

        private void CycleQuality()
        {
            if (QualitySettings.names.Length == 0) return;
            qualityIndex = (qualityIndex + 1) % QualitySettings.names.Length;
            ApplyQuality();
            RefreshSettingsLabels();
        }

        private void CycleFps()
        {
            fpsIndex = (fpsIndex + 1) % fpsOptions.Length;
            ApplyFps();
            RefreshSettingsLabels();
        }

        private void ApplyQuality()
        {
            if (QualitySettings.names.Length == 0) return;
            qualityIndex = Mathf.Clamp(qualityIndex, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(qualityIndex, true);
            PlayerPrefs.SetInt("EH_Quality", qualityIndex);
            PlayerPrefs.Save();
        }

        private void ApplyFps()
        {
            Application.targetFrameRate = fpsOptions[fpsIndex];
            QualitySettings.vSyncCount = 0;
            PlayerPrefs.SetInt("EH_FPS", fpsIndex);
            PlayerPrefs.Save();
        }

        private void RefreshSettingsLabels()
        {
            if (qualityValue != null)
                qualityValue.text = QualitySettings.names.Length == 0
                    ? "QUALITY  •  DEFAULT"
                    : "QUALITY  •  " + QualitySettings.names[qualityIndex];
            if (fpsValue != null)
                fpsValue.text = "FRAME LIMIT  •  " + fpsOptions[fpsIndex] + " FPS";
        }

        private void Quit()
        {
#if UNITY_EDITOR
            Debug.Log("EURO HAUL: Exit requested (ignored in Editor).");
#else
            Application.Quit();
#endif
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static Text CreateText(Transform parent, string value, int size, Color color,
            Vector2 anchor, Vector2 offset, Vector2 dimensions, FontStyle style)
        {
            GameObject obj = new GameObject("Text_" + value.Replace(" ", ""), typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = offset;
            rect.sizeDelta = dimensions;
            Text text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(12, size - 8);
            text.resizeTextMaxSize = size;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchor,
            Vector2 offset, Vector2 dimensions, Color color, UnityEngine.Events.UnityAction action)
        {
            GameObject obj = new GameObject("Button_" + label.Replace(" ", ""), typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = offset;
            rect.sizeDelta = dimensions;
            Image image = obj.GetComponent<Image>();
            image.color = color;
            Button button = obj.GetComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, White, 0.18f);
            colors.pressedColor = Color.Lerp(color, Gold, 0.2f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            button.onClick.AddListener(action);

            Text text = CreateText(obj.transform, label, 22, White, new Vector2(0.5f, 0.5f),
                Vector2.zero, dimensions - new Vector2(24f, 12f), FontStyle.Bold);
            text.gameObject.name = "Label";
            return button;
        }
    }
}