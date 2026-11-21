using UnityEngine;
using UnityEngine.UI;

// A non-blocking hint: gameplay and mouse look stay active while it is visible.
public class LadderPromptUI : MonoBehaviour {
    // The old checkbox stored HideLadderPrompt. Start the simplified hint fresh
    // so that choice cannot hide it before the player tries this version.
    private const string PreferenceKey = "LadderHintUsed";
    private PlayerMovement player;
    private GameObject panel;
    private Font font;

    public void Initialize(PlayerMovement movement) {
        player = movement;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject hud = new GameObject("Ladder Hint HUD", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler));
        hud.transform.SetParent(transform, false);
        Canvas canvas = hud.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2;
        CanvasScaler scaler = hud.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform panelRect = CreateRect("Ladder Hint", hud.transform,
            new Vector2(0f, -170f), new Vector2(600f, 64f));
        panel = panelRect.gameObject;
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.04f, 0.06f, 0.09f, 0.92f);
        background.raycastTarget = false;
        CreateText("Message", panelRect, Vector2.zero,
            new Vector2(570f, 40f), "Press E to engage with ladder.", 26);
        panel.SetActive(false);
    }

    private void LateUpdate() {
        if (panel == null) return;
        bool visible = player != null && player.CanShowLadderPrompt
            && PlayerPrefs.GetInt(PreferenceKey, 0) == 0;
        panel.SetActive(visible);
    }

    public void Acknowledge() {
        // Save only after the player uses a ladder, not just by walking past one.
        if (PlayerPrefs.GetInt(PreferenceKey, 0) == 0) {
            PlayerPrefs.SetInt(PreferenceKey, 1);
            PlayerPrefs.Save();
        }
        if (panel != null) panel.SetActive(false);
    }

    public static void ResetSavedPreference() {
        PlayerPrefs.DeleteKey(PreferenceKey);
        PlayerPrefs.Save();
    }

    private RectTransform CreateRect(string objectName, Transform parent,
        Vector2 position, Vector2 size) {
        GameObject obj = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private Text CreateText(string objectName, Transform parent, Vector2 position,
        Vector2 size, string message, int fontSize) {
        Text label = CreateRect(objectName, parent, position, size).gameObject.AddComponent<Text>();
        label.font = font;
        label.fontSize = fontSize;
        label.text = message;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.raycastTarget = false;
        return label;
    }
}
