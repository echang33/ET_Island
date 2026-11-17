using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartUI : MonoBehaviour
{
    public Image[] hearts;
    public int lives = 3;

    public void LoseHeart()
    {
        if (lives <= 0) return;

        lives--;

        // Gray out the lost heart
        hearts[lives].color = Color.gray;
    }
}