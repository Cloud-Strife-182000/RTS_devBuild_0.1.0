using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MiniMapClick : MonoBehaviour, IPointerClickHandler
{
    public ActivePlayer player;
    public Vector2 localCursor;
    public Vector2 offset;

    private Texture tex;
    private Rect r;

    private void Start()
    {
        player = FindObjectOfType<ActivePlayer>();

        tex = GetComponent<RawImage>().texture;
        r = GetComponent<RawImage>().rectTransform.rect;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RawImage>().rectTransform, eventData.pressPosition, eventData.pressEventCamera, out localCursor))
        {
            float coordX = Mathf.Clamp(0, (((localCursor.x - r.x) * tex.width) / r.width), tex.width);
            float coordY = Mathf.Clamp(0, (((localCursor.y - r.y) * tex.height) / r.height), tex.height);

            float recalcX = coordX / tex.width;
            float recalcY = coordY / tex.height;

            localCursor = new Vector2(recalcX, recalcY);

            float xWorld = localCursor.x * 1000;
            float zWorld = localCursor.y * 1000;

            xWorld += offset.x;
            zWorld += offset.y;

            player.transform.position = new Vector3(xWorld, player.transform.position.y, zWorld);
        }
    }
}
