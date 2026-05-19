using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Tooltip("Унікальний ідентифікатор точки (наприклад: 'WakeUpBed', 'BridgeLeft')")]
    public string spawnerId;

    private void OnDrawGizmos()
    {
        // Малює синю напівпрозору сферу в редакторі, щоб завжди бачити спавнер
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.7f);
        Gizmos.DrawSphere(transform.position, 0.3f);
        
        // Малює лінію донизу для орієнтації
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.5f);
    }
}