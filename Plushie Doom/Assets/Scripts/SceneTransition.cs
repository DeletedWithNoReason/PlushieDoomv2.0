using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [Header("Destination Scenes by Karma")]
    public string lowKarmaScene;     // Менше 30
    public string neutralKarmaScene; // 30 - 70
    public string highKarmaScene;    // Більше 70

    [Header("Spawn Settings")]
    [Tooltip("ID спавнера на новій сцені, куди переміститься гравець")]
    public string targetSpawnerId; 
    public string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered || !other.CompareTag(playerTag)) return;

        // ТУТ ТРЕБА ОТРИМАТИ РЕАЛЬНУ КАРМУ (поки що заглушка)
        float currentKarma = 50f; // Тимчасове значення для тесту

        // Визначаємо, яку сцену вантажити
        string sceneToLoad = GetSceneNameByKarma(currentKarma);

        if (!string.IsNullOrEmpty(sceneToLoad) && UIManagement.Instance != null)
        {
            hasTriggered = true;
            // Передаємо ID замість координат
            UIManagement.Instance.TransitionToScene(sceneToLoad, targetSpawnerId);
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Помилка: назва сцени порожня або відсутній UIManagement!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            hasTriggered = false;
        }
    }

    private string GetSceneNameByKarma(float karma)
    {
        if (karma < 30f) return lowKarmaScene;
        if (karma <= 70f) return neutralKarmaScene;
        return highKarmaScene;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
        }
    }
}