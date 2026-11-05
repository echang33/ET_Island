/*
Team members: Ethan Chang, Ryan Wu, Steven tan
*/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Required to reload scenes
using TMPro; // Required to use TextMeshPro UI

public class GameManager : MonoBehaviour
{
    public Life playerLife; 
    
    // The UI text that will appear when you die
    public TextMeshProUGUI gameOverText;
    
    // How long to wait before reloading (in seconds)
    public float restartDelay = 2f; 

    [Header("Spawn Settings")]
    [Tooltip("Choose the alternate spawn when the scene starts. Set before pressing Play.")]
    public bool useAlternateSpawn;
    [Tooltip("The player root (ChiPohLe). If empty, uses Player Life's transform.")]
    public Transform player;
    public Transform normalSpawn;
    public Transform alternateSpawn;

    void Start()
    {
        PlacePlayerAtSpawn();

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

    private void PlacePlayerAtSpawn()
    {
        Transform playerTransform = player != null ? player : playerLife != null ? playerLife.transform : null;
        Transform spawn = useAlternateSpawn ? alternateSpawn : normalSpawn;

        // Keep the scene's existing player position until spawn markers are assigned.
        if (spawn == null)
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
            body.position = spawn.position;
            body.rotation = spawn.rotation;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        else
        {
            playerTransform.SetPositionAndRotation(spawn.position, spawn.rotation);
        }
    }

    private void GameOver()
    {
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

        // 3. Start a timer to reload the scene instead of doing it instantly
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
