using UnityEngine;

public class UniversalTrigger : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private bool triggerOnce = true;
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Якщо вже спрацював або це не гравець — ігноруємо
        if (hasTriggered || !other.CompareTag("Player")) return;

        if (gameEvent != null)
        {
            EventManager.Instance.RunEvent(gameEvent);
            
            if (triggerOnce)
            {
                hasTriggered = true;
                
                // "Розумний" підхід: замість просто вимикати колайдер, 
                // ми вимикаємо весь об'єкт. StatePersister це побачить і збереже.
                Debug.Log($"<color=green>[Trigger]</color> {gameObject.name} спрацював (одноразово). Вимикаю об'єкт.");
                gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning($"[UniversalTrigger] У об'єкта {gameObject.name} не призначено GameEvent!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !triggerOnce)
        {
            hasTriggered = false; 
        }
    }
}