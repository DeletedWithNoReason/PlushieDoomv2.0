using UnityEngine;

public class SceneEventTrigger : MonoBehaviour
{
    [Header("Event to play when this scene loads")]
    public GameEvent eventOnStart;

    private void Start()
    {
        if (eventOnStart != null && EventManager.Instance != null)
        {
            EventManager.Instance.RunEvent(eventOnStart);
        }
    }
}