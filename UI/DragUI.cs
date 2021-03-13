using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragUI : MonoBehaviour, IDragHandler
{
    public float dragSpeed = 5f;

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Vector3.Lerp(transform.position, Input.mousePosition, Time.deltaTime * dragSpeed);
    }
}
