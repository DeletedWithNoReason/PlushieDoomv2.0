using System.Collections.Generic;
using UnityEngine;

public class SceneStateManager : MonoBehaviour
{
    public static SceneStateManager Instance;
    private Dictionary<string, bool> objectStates = new Dictionary<string, bool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Переконайся, що цей об'єкт дійсно не знищується
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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