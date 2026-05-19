using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIInspector : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // При кліку лівою кнопкою
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                Debug.Log("Raycast влучив у: " + result.gameObject.name, result.gameObject);
            }
        }
    }
}