using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Обов'язково додаємо це

public class SceneStateManager : MonoBehaviour
{
    public static SceneStateManager Instance;
    private Dictionary<string, bool> objectStates = new Dictionary<string, bool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Підписуємося на подію завантаження сцени
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Відписуємося, щоб уникнути помилок пам'яті
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Знаходимо ВСІ StatePersister, НАВІТЬ ТІ, ЩО ВИМКНЕНІ (сплячі)
        StatePersister[] persisters = Resources.FindObjectsOfTypeAll<StatePersister>();
        
        foreach (var p in persisters)
        {
            // Resources.FindObjectsOfTypeAll знаходить і префаби в папках. 
            // Тому перевіряємо, чи об'єкт дійсно лежить на завантаженій сцені.
            if (p.gameObject.scene == scene)
            {
                p.RestoreFromManager();
            }
        }
    }

    public void SaveObjectState(string id, bool isActive)
    {
        objectStates[id] = isActive;
        Debug.Log($"<color=cyan>[Save]</color> Стан '{id}' збережено: {isActive}");
    }

    public bool TryGetObjectState(string id, out bool isActive)
    {
        bool found = objectStates.TryGetValue(id, out isActive);
        if (found) Debug.Log($"<color=yellow>[Load]</color> Стан '{id}' відновлено: {isActive}");
        return found;
    }
}