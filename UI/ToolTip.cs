using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltip;
    public Vector3 offset;

    private bool tooltipEnabled;
    private bool mouseOver;

    private void Update()
    {
        if (mouseOver)
        {
            if (tooltip)
            {
                tooltip.transform.position = Input.mousePosition + offset;

                tooltip.SetActive(true);
            }
        }
        else
        {
            if (tooltip)
            {
                tooltip.SetActive(false);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
    }
}
