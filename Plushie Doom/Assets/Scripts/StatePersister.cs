using UnityEngine;
using UnityEngine.SceneManagement;

public class StatePersister : MonoBehaviour
{
    public string uniqueId;
    private static bool isAppQuitting = false;

    private void Start()
    {
        Restore();
    }

    private void Restore()
    {
        if (string.IsNullOrEmpty(uniqueId) || SceneStateManager.Instance == null) return;

        if (SceneStateManager.Instance.TryGetObjectState(uniqueId, out bool savedState))
        {
            // Вимикаємо/вмикаємо ТІЛЬКИ якщо стан відрізняється від початкового
            if (gameObject.activeSelf != savedState)
            {
                gameObject.SetActive(savedState);
            }
        }
    }

    private void OnDisable()
    {
        // Якщо ми виходимо з гри або сцена якраз вивантажується — НЕ зберігаємо нічого
        if (isAppQuitting || !gameObject.scene.isLoaded) return;

        if (SceneStateManager.Instance != null && !string.IsNullOrEmpty(uniqueId))
        {
            // Зберігаємо стан об'єкта як "вимкнений"
            SceneStateManager.Instance.SaveObjectState(uniqueId, false);
        }
    }

    private void OnEnable()
    {
        if (isAppQuitting || SceneStateManager.Instance == null || string.IsNullOrEmpty(uniqueId)) return;
        
        // Зберігаємо стан як "увімкнений" тільки якщо сцена повністю активна
        if (gameObject.scene.isLoaded)
        {
            SceneStateManager.Instance.SaveObjectState(uniqueId, true);
        }
    }

    private void OnApplicationQuit()
    {
        isAppQuitting = true;
    }
}