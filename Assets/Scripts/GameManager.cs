/*
 * GameManager.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Manages lives, respawning, and game over.
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    public Life playerLife;
    public Transform player;
    public Rigidbody playerRigidbody;
    public MonoBehaviour[] controlScripts;

    [Header("Lives")]
    public HeartUI heartUI;
    public int maxLives = 3;
    private int lives;
    private float startingHealth;

    [Header("Respawn")]
    public float respawnDelay = 1f;
    public Transform normalSpawn;
    public Transform alternateSpawn;
    public bool useAlternateSpawn;

    [Header("Death UI")]
    [FormerlySerializedAs("gameOverText")]
    public TextMeshProUGUI deathText;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Button gameOverRestartButton;

    [Header("Fall Detection")]
    public bool killOnFall = true;
    public float killHeight = -20f;

    private enum State { Playing, Respawning, GameOver }
    private State state = State.Playing;
    private float nextActionTime;
    private bool wasKinematic;
    private bool[] originalControlStates;

    void Start()
    {
        if (playerLife == null && player != null)
            playerLife = player.GetComponent<Life>();

        if (player == null && playerLife != null)
            player = playerLife.transform;

        if (playerRigidbody == null && player != null)
            playerRigidbody = player.GetComponent<Rigidbody>();

        if (playerLife == null)
        {
            Debug.LogError("GameManager needs a Player Life reference.");
            enabled = false;
            return;
        }

        lives = maxLives;
        startingHealth = playerLife.amount;
        // Existing scenes can leave this list empty; still freeze player controls on death.
        if (controlScripts == null || controlScripts.Length == 0)
        {
            controlScripts = new MonoBehaviour[] {
                player.GetComponent<PlayerMovement>(), player.GetComponent<PlayerInput>()
            };
        }
        originalControlStates = new bool[controlScripts.Length];
        MoveToSpawn();
        playerLife.onDeath.AddListener(HandleDeath);
        if (deathText != null) deathText.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameOverRestartButton != null)
            gameOverRestartButton.onClick.AddListener(RestartGame);
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (state == State.Respawning &&
            Time.time >= nextActionTime)
        {
            MoveToSpawn();
            if (deathText != null) deathText.gameObject.SetActive(false);
            playerLife.Revive(startingHealth);
            SetControls(true);
            state = State.Playing;
        }
        else if (state == State.Playing &&
                 killOnFall && player != null &&
                 player.position.y < killHeight)
        {
            playerLife.amount = 0f;
        }
    }

    private void HandleDeath()
    {
        if (state != State.Playing) return;
        lives--;
        if (heartUI != null) heartUI.LoseHeart();
        // Freeze player and camera controls
        SetControls(false);
        if (lives > 0)
        {
            state = State.Respawning;
            if (deathText != null) deathText.gameObject.SetActive(true);
            nextActionTime = Time.time + respawnDelay;
        }
        else
        {
            state = State.GameOver;
            if (deathText != null) deathText.gameObject.SetActive(false);
            if (gameOverPanel != null)
            {
                gameOverPanel.transform.SetAsLastSibling();
                gameOverPanel.SetActive(true);
            }
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (gameOverRestartButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameOverRestartButton.gameObject);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Scene scene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
        // TestLevel can be played in the Editor without being in Build Settings.
        if (scene.buildIndex < 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                scene.path, new LoadSceneParameters(LoadSceneMode.Single));
            return;
        }
#endif
        SceneManager.LoadScene(scene.path);
    }

    private void SetControls(bool enable)
    {
        for (int i = 0; i < controlScripts.Length; i++)
        {
            if (controlScripts[i] == null) continue;
            if (!enable) {
                originalControlStates[i] = controlScripts[i].enabled;
            }
            controlScripts[i].enabled =
                enable ? originalControlStates[i] : false;
        }

        if (playerRigidbody != null)
        {
            if (!enable)
            {
                wasKinematic = playerRigidbody.isKinematic;

                if (!wasKinematic)
                {
                    playerRigidbody.velocity = Vector3.zero;
                    playerRigidbody.angularVelocity = Vector3.zero;
                }

                playerRigidbody.isKinematic = true;
            }
            else
            {
                playerRigidbody.isKinematic = wasKinematic;
            }
        }
    }

    private void MoveToSpawn()
    {
        Transform spawn = useAlternateSpawn
            ? alternateSpawn : normalSpawn;

        if (spawn == null || player == null) return;

        if (playerRigidbody != null)
        {
            playerRigidbody.position = spawn.position;
            playerRigidbody.rotation = spawn.rotation;

            if (!playerRigidbody.isKinematic)
            {
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            player.SetPositionAndRotation(
                spawn.position, spawn.rotation);
        }
    }

    private void OnDestroy()
    {
        if (playerLife != null)
            playerLife.onDeath.RemoveListener(HandleDeath);
        if (gameOverRestartButton != null)
            gameOverRestartButton.onClick.RemoveListener(RestartGame);
    }
}
