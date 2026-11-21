using UnityEngine;
using UnityEngine.UI;

public class InteractPromptUI : MonoBehaviour {
    private PlayerMovement player;
    private GameObject panel;

    public void Initialize(PlayerMovement movement) {
        player = movement;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject hud = new GameObject("Interact Prompt HUD",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        hud.transform.SetParent(transform, false);

        Canvas canvas = hud.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2;

        CanvasScaler scaler = hud.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        panel = new GameObject("Interact Prompt", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(hud.transform, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 50f);
        rect.sizeDelta = new Vector2(600f, 64f);

        Image background = panel.GetComponent<Image>();
        background.color = new Color(0.04f, 0.06f, 0.09f, 0.92f);
        background.raycastTarget = false;

        Text label = new GameObject("Message", typeof(RectTransform), typeof(Text))
            .GetComponent<Text>();

        label.transform.SetParent(panel.transform, false);
        label.font = font;
        label.fontSize = 26;
        label.text = "Press E to interact.";
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.raycastTarget = false;

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(15f, 0f);
        labelRect.offsetMax = new Vector2(-15f, 0f);

        panel.SetActive(false);
    }

    private void LateUpdate() {
        if (panel == null) return;
        panel.SetActive(player != null && player.CanShowInteractPrompt);
    }
}