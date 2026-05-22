using UnityEngine;

public class StatePersister : MonoBehaviour
{
    public string uniqueId;
    public bool triggerOnce = true; 
    
    private bool _isInitialized = false;
    private static bool isAppQuitting = false;

    private void Awake()
    {
        // Якщо об'єкт увімкнений за замовчуванням, Awake спрацює першим
        RestoreFromManager();
    }

    // Тепер цей метод викликає SceneStateManager для ВСІХ об'єктів
    public void RestoreFromManager()
    {
        // Захист від подвійного виконання (якщо Awake вже відпрацював)
        if (_isInitialized) return; 
        
        if (string.IsNullOrEmpty(uniqueId) || SceneStateManager.Instance == null) return;

        if (SceneStateManager.Instance.TryGetObjectState(uniqueId, out bool savedState))
        {
            // Вмикаємо або вимикаємо об'єкт згідно з базою
            gameObject.SetActive(savedState);
        }
        
        // Фіксуємо, що об'єкт успішно налаштовано
        _isInitialized = true;
    }

    public void OnPlayerInteract()
    {
        if (triggerOnce)
        {
            SaveState(false);
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (isAppQuitting || !_isInitialized || SceneStateManager.Instance == null) return;
        SaveState(true);
    }

    private void OnDestroy()
    {
        if (isAppQuitting || SceneStateManager.Instance == null) return;

        if (triggerOnce && !gameObject.activeSelf)
        {
            SaveState(false);
        }
    }

    private void SaveState(bool state)
    {
        if (!string.IsNullOrEmpty(uniqueId))
        {
            SceneStateManager.Instance.SaveObjectState(uniqueId, state);
        }
    }

    private void OnApplicationQuit() => isAppQuitting = true;
}