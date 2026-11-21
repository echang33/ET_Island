using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Run after GameManager places the player and PlayerMovement initializes controls.
[DefaultExecutionOrder(100)]
public class TrainingStartup : MonoBehaviour {
    [SerializeField] private GameManager gameManager;
    private PlayerMovement movement;
    private PlayerInput playerInput;
    private bool movementWasEnabled;
    private bool inputWasEnabled;
    private float previousTimeScale;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private bool waiting;
    private bool keyPressed;
    private int openingFrame;

    private void Start() {
        if (gameManager != null && gameManager.player != null) {
            movement = gameManager.player.GetComponent<PlayerMovement>();
            playerInput = gameManager.player.GetComponent<PlayerInput>();
        }

        previousTimeScale = Time.timeScale;
        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        if (movement != null) {
            movementWasEnabled = movement.enabled;
            movement.enabled = false;
        }
        if (playerInput != null) {
            inputWasEnabled = playerInput.enabled;
            playerInput.enabled = false;
        }
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        waiting = true;
        openingFrame = Time.frameCount;

        // Keep the prompt inside the existing Tutorial panel.
        TMP_Text referenceText = GetComponentInChildren<TMP_Text>();
        GameObject promptObject = new GameObject("Start Training Prompt", typeof(RectTransform));
        RectTransform rect = promptObject.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 24f);
        rect.sizeDelta = new Vector2(550f, 36f);
        TextMeshProUGUI prompt = promptObject.AddComponent<TextMeshProUGUI>();
        if (referenceText != null) prompt.font = referenceText.font;
        prompt.text = "Press any key to start training";
        prompt.fontSize = 22f;
        prompt.alignment = TextAlignmentOptions.Center;
        prompt.color = new Color(1f, 0.82f, 0.4f);
        prompt.raycastTarget = false;
    }

    private void Update() {
        if (!waiting || !Application.isFocused || Time.frameCount == openingFrame
            || Keyboard.current == null) return;

        if (Keyboard.current.anyKey.wasPressedThisFrame) keyPressed = true;
        // Consume the start key so Space or E cannot also jump or interact.
        if (keyPressed && !Keyboard.current.anyKey.isPressed) {
            RestoreGameplay();
            gameObject.SetActive(false);
        }
    }

    private void OnDisable() {
        RestoreGameplay();
    }

    private void RestoreGameplay() {
        if (!waiting) return;
        waiting = false;
        Time.timeScale = previousTimeScale;
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
        if (movement != null) movement.enabled = movementWasEnabled;
        if (playerInput != null) playerInput.enabled = inputWasEnabled;
    }
}
