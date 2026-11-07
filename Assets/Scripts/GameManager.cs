/*
 * GameManager.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Places the player at the selected spawn, detects falls, and reloads the scene after death.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Required to reload scenes
using TMPro; // Required to use TextMeshPro UI

public class GameManager : MonoBehaviour
{
    [Header("Player Health")]
    public Life playerLife;
    
    [Header("Game Over UI and Restart")]
    public TextMeshProUGUI gameOverText;
    
    // How long to wait before reloading (in seconds)
    public float restartDelay = 2f; 

    [Header("Fall Safeguard")]
    public bool killOnFall = true;
    [Tooltip("World-space Y position below which the player dies. Set below the lowest playable floor.")]
    public float killHeight = -20f;

    [Header("Spawn Settings")]
    [Tooltip("Choose the alternate spawn when the scene starts. Set before pressing Play.")]
    public bool useAlternateSpawn;
    [Tooltip("The player root (ChiPohLe). If empty, uses Player Life's transform.")]
    public Transform player;
    public Transform normalSpawn;
    public Transform alternateSpawn;

    private bool gameOverStarted;
    // Store values rather than scene references: markers are destroyed by LoadScene.
    // This pending snapshot is consumed once by the next matching scene instance.
    private static bool hasPendingRespawn;
    private static string pendingScenePath;
    private static bool pendingAlternateSpawn;
    private static Vector3 pendingPosition;
    private static Quaternion pendingRotation;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRespawnState()
    {
        // Also reset when entering Play with domain reload disabled in the Unity Editor.
        hasPendingRespawn = false;
        pendingScenePath = null;
    }

    void Start()
    {
        PlacePlayerAtSpawn();

        if (playerLife == null && player != null)
        {
            playerLife = player.GetComponent<Life>();
        }

        if (playerLife != null)
        {
            playerLife.onDeath.AddListener(GameOver);
        }
        
        // Ensure the game over text is hidden when the level starts
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!killOnFall || gameOverStarted || playerLife == null || Time.timeScale == 0f)
        {
            return;
        }

        Transform playerTransform = player != null ? player : playerLife.transform;
        if (playerTransform.position.y < killHeight)
        {
            // Let Life invoke the same death event used by damage from enemies.
            playerLife.amount = 0f;
        }
    }

    private void PlacePlayerAtSpawn()
    {
        Transform playerTransform = player != null ? player : playerLife != null ? playerLife.transform : null;
        bool restoringRespawn = hasPendingRespawn && pendingScenePath == gameObject.scene.path;
        if (restoringRespawn)
        {
            useAlternateSpawn = pendingAlternateSpawn;
        }
        Transform spawn = useAlternateSpawn ? alternateSpawn : normalSpawn;
        Vector3 position = restoringRespawn ? pendingPosition : spawn != null ? spawn.position : Vector3.zero;
        Quaternion rotation = restoringRespawn ? pendingRotation : spawn != null ? spawn.rotation : Quaternion.identity;
        hasPendingRespawn = false;

        // Keep the scene's existing player position until spawn markers are assigned.
        if (!restoringRespawn && spawn == null)
        {
            if (useAlternateSpawn)
            {
                Debug.LogWarning("Alternate spawn is selected, but no Alternate Spawn marker is assigned.", this);
            }
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("Assign Player or Player Life to use the spawn markers.", this);
            return;
        }

        if (playerTransform.TryGetComponent<Rigidbody>(out var body))
        {
            body.position = position;
            body.rotation = rotation;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        else
        {
            playerTransform.SetPositionAndRotation(position, rotation);
        }
    }

    private void GameOver()
    {
        if (gameOverStarted)
        {
            return;
        }
        gameOverStarted = true;

        // Scene reloads restore serialized Inspector values. Carry the selected
        // marker's pose across this reload, including markers assigned during Play.
        Transform spawn = useAlternateSpawn ? alternateSpawn : normalSpawn;
        if (spawn != null)
        {
            pendingScenePath = gameObject.scene.path;
            pendingAlternateSpawn = useAlternateSpawn;
            pendingPosition = spawn.position;
            pendingRotation = spawn.rotation;
            hasPendingRespawn = true;
        }

        // Detach the camera from the player so it doesn't get turned off
        if (Camera.main != null)
        {
            Camera.main.transform.SetParent(null);
        }

        // Show the Game Over text on the screen
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }

        // Give the game-over text and independently spawned death particles time to display.
        StartCoroutine(ReloadSceneRoutine());
    }

    // A Coroutine that waits for a few seconds, then executes code
    private IEnumerator ReloadSceneRoutine()
    {
        yield return new WaitForSeconds(restartDelay);
        
        // Find the name of the current scene and load it again
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
