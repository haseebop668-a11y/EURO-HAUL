using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EuroHaul.Career;

namespace EuroHaul.UI
{
    /// <summary>Compact career contract card for the prototype driving scene.</summary>
    public sealed class CareerHUD : MonoBehaviour
    {
        private static readonly Color Navy = new Color(0.025f, 0.055f, 0.09f, 0.92f);
        private static readonly Color Gold = new Color(0.92f, 0.68f, 0.25f, 1f);
        private static readonly Color White = new Color(0.93f, 0.96f, 1f, 1f);
        private CareerManager career;
        private Text details;
        private Text balance;
        private Text status;
        private Button actionButton;
        private Text actionLabel;
        private Image progressFill;

        private void Start()
        {
            career = CareerManager.Instance;
            if (career == null) return;
            Build();
            career.OnCareerChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (career != null) career.OnCareerChanged -= Refresh;
        }

        private void Build()
        {
            if (EventSystem.current == null)
                new GameObject("CareerEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            GameObject canvasObject = new GameObject("CareerHUDCanvas", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject panel = new GameObject("ContractCard", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(28f, -28f);
            panelRect.sizeDelta = new Vector2(480f, 250f);
            panel.GetComponent<Image>().color = Navy;

            details = AddText(panel.transform, "", 20, White, new Vector2(0.5f, 0.76f),
                new Vector2(0f, 0f), new Vector2(440f, 110f));
            balance = AddText(panel.transform, "", 18, Gold, new Vector2(0.5f, 0.30f),
                Vector2.zero, new Vector2(440f, 36f));
            status = AddText(panel.transform, "", 15, White, new Vector2(0.5f, 0.19f),
                Vector2.zero, new Vector2(440f, 30f));

            GameObject bar = new GameObject("ProgressBar", typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(panel.transform, false);
            RectTransform barRect = bar.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.08f, 0.43f);
            barRect.anchorMax = new Vector2(0.92f, 0.43f);
            barRect.sizeDelta = new Vector2(0f, 10f);
            bar.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(bar.transform, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2.one;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            progressFill = fill.GetComponent<Image>();
            progressFill.color = Gold;
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillAmount = 0f;

            actionButton = AddButton(panel.transform, new Vector2(0.5f, 0.08f),
                new Vector2(0f, -2f), new Vector2(250f, 40f), AcceptContract);
            actionLabel = actionButton.GetComponentInChildren<Text>();
        }

        private void Refresh()
        {
            if (career == null || career.Data == null || details == null) return;
            CareerSaveData data = career.Data;
            balance.text = "GAME CASH  " + data.gameCash.ToString("N0") +
                "     DRIVER LV. " + data.driverLevel;
            FreightContract active = data.activeContract;
            FreightContract offer = active ?? data.offeredContract;
            if (offer == null)
            {
                details.text = "No freight contract available";
                actionButton.gameObject.SetActive(false);
                return;
            }

            details.text = (active != null ? "ACTIVE DELIVERY" : "FREIGHT CONTRACT OFFER") +
                "\n" + offer.cargo + "\n" + offer.origin + "  →  " + offer.destination +
                "\nReward  " + offer.rewardCash.ToString("N0") + " cash  •  " + offer.rewardXp + " XP";
            status.text = active != null
                ? "Progress  " + Mathf.RoundToInt(career.ActiveContractProgress01 * 100f) + "%"
                : career.LastNotice;
            progressFill.fillAmount = active != null ? career.ActiveContractProgress01 : 0f;
            actionButton.gameObject.SetActive(active == null);
            actionLabel.text = active == null ? "ACCEPT CONTRACT" : "";
        }

        private void AcceptContract()
        {
            career?.AcceptOfferedContract();
            Refresh();
        }

        private static Text AddText(Transform parent, string value, int size, Color color,
            Vector2 anchor, Vector2 offset, Vector2 dimensions)
        {
            GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor; rect.anchorMax = anchor; rect.anchoredPosition = offset; rect.sizeDelta = dimensions;
            Text text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value; text.fontSize = size; text.color = color;
            text.alignment = TextAnchor.MiddleLeft; text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12; text.resizeTextMaxSize = size;
            return text;
        }

        private static Button AddButton(Transform parent, Vector2 anchor, Vector2 offset,
            Vector2 dimensions, UnityEngine.Events.UnityAction callback)
        {
            GameObject obj = new GameObject("AcceptContractButton", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor; rect.anchorMax = anchor; rect.anchoredPosition = offset; rect.sizeDelta = dimensions;
            Image image = obj.GetComponent<Image>(); image.color = new Color(0.08f, 0.38f, 0.78f, 1f);
            Button button = obj.GetComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(callback);
            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(obj.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
            Text label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.text = "ACCEPT CONTRACT"; label.fontSize = 17; label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter; label.color = Color.white;
            return button;
        }
    }
}