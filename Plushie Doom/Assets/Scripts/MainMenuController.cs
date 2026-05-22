using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Індекс першої ігрової локації у Build Settings")]
    public int firstLevelIndex = 2;

    void Start()
    {
        AudioSource menuMusic = GetComponent<AudioSource>();
        if (menuMusic != null) menuMusic.Play();
    }

    public void StartNewGame()
    {
        Debug.Log($"[MainMenu] Завантаження ігрової локації (Index: {firstLevelIndex})");
        
        SceneManager.LoadScene(firstLevelIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}