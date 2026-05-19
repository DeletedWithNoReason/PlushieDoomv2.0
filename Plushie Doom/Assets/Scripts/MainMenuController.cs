using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Settings")]
    public string bootstrapSceneName = "Bootstrap";

    void Start()
    {
        // Якщо хочеш запустити музику просто кодом
        AudioSource menuMusic = GetComponent<AudioSource>();
        if (menuMusic != null) menuMusic.Play();
    }
    public void StartNewGame()
    {
        Debug.Log($"[MainMenu] Завантаження сцени ініціалізації: {bootstrapSceneName}");
        
        SceneManager.LoadScene(bootstrapSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}