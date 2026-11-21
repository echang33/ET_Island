using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public Button resumeButton;
    public Button instructionButton;
    public GameObject controlsPanel;
    public Button controlsBackButton;
    private PlayerMovement movement;
    private PlayerInput playerInput;
    private bool movementWasEnabled;
    private bool inputWasEnabled;
    private bool isPaused;
    private float previousTimeScale;
    private bool previousAudioPause;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private readonly List<GameObject> hiddenMenuItems = new List<GameObject>();

    private void Awake()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumePressed);
        if (instructionButton != null) instructionButton.onClick.AddListener(OnInstructionsPressed);
        if (controlsBackButton != null) controlsBackButton.onClick.AddListener(OnControlsBackPressed);
    }

    public void OnInstructionsPressed()
    {
        if (!isPaused || controlsPanel == null) return;
        // Controls can live inside the menu Canvas. Keep its parent active and
        // hide only the other menu items, preserving their original active states.
        if (controlsPanel.transform.IsChildOf(pauseMenu.transform))
        {
            foreach (Transform child in pauseMenu.transform)
            {
                if (controlsPanel.transform == child || controlsPanel.transform.IsChildOf(child)) continue;
                if (child.gameObject.activeSelf)
                {
                    hiddenMenuItems.Add(child.gameObject);
                    child.gameObject.SetActive(false);
                }
            }
        }
        else pauseMenu.SetActive(false);
        controlsPanel.SetActive(true);
        if (controlsBackButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(controlsBackButton.gameObject);
    }

    public void OnControlsBackPressed()
    {
        if (!isPaused) return;
        if (controlsPanel != null) controlsPanel.SetActive(false);
        RestoreMenuItems();
        pauseMenu.SetActive(true);
        if (instructionButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(instructionButton.gameObject);
    }

    public void OnResumePressed()
    {
        if (!isPaused) return;
        isPaused = false;
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        RestoreMenuItems();
        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPause;
        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousCursorLock;
        if (movement != null) movement.enabled = movementWasEnabled;
        if (playerInput != null) playerInput.enabled = inputWasEnabled;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (!Application.isFocused || keyboard == null) return;
        if (!keyboard.mKey.wasPressedThisFrame && !keyboard.escapeKey.wasPressedThisFrame) return;

        if (isPaused && controlsPanel != null && controlsPanel.activeSelf
            && keyboard.escapeKey.wasPressedThisFrame) OnControlsBackPressed();
        else if (isPaused) OnResumePressed();
        else Pause();
    }

    private void Pause()
    {
        // Tutorials and puzzles already own the pause and cursor while open.
        if (pauseMenu == null || Time.timeScale <= 0f) return;
        GameManager manager = FindFirstObjectByType<GameManager>();
        movement = manager != null && manager.player != null
            ? manager.player.GetComponent<PlayerMovement>()
            : FindFirstObjectByType<PlayerMovement>();
        if (movement == null || !movement.isActiveAndEnabled) return;
        Life life = movement.GetComponent<Life>();
        if (life != null && life.IsDead) return;
        playerInput = movement.GetComponent<PlayerInput>();
        if (playerInput != null && !playerInput.isActiveAndEnabled) return;

        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        movementWasEnabled = movement.enabled;
        inputWasEnabled = playerInput != null && playerInput.enabled;
        isPaused = true;

        movement.enabled = false;
        if (playerInput != null) playerInput.enabled = false;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (controlsPanel != null) controlsPanel.SetActive(false);
        pauseMenu.SetActive(true);
        if (resumeButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    private void RestoreMenuItems()
    {
        foreach (GameObject item in hiddenMenuItems)
        {
            if (item != null) item.SetActive(true);
        }
        hiddenMenuItems.Clear();
    }

    private void OnDisable()
    {
        OnResumePressed();
    }

    private void OnDestroy()
    {
        if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResumePressed);
        if (instructionButton != null) instructionButton.onClick.RemoveListener(OnInstructionsPressed);
        if (controlsBackButton != null) controlsBackButton.onClick.RemoveListener(OnControlsBackPressed);
    }
}
