using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RestartButton : MonoBehaviour
{
    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Restart);
    }

    void Restart()
    {
        GameManager manager = FindFirstObjectByType<GameManager>();
        if (manager != null) manager.RestartGame();
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(Restart);
    }
}
