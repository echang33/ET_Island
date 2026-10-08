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

    void Start()
    {
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