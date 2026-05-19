using UnityEngine;

[ExecuteInEditMode]
public class VisibleColliders : MonoBehaviour
{
    void OnDrawGizmos()
    {
        BoxCollider2D[] colliders = GetComponentsInChildren<BoxCollider2D>();
        
        Gizmos.color = Color.green; // Колір ліній
        
        foreach (var col in colliders)
        {
            Gizmos.matrix = col.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.offset, col.size);
        }
    }
}