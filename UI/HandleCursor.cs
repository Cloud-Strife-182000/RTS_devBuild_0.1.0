using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleCursor : MonoBehaviour {
    public Texture2D mouse;
    public Texture2D goldmine;
    public Texture2D stonemine;
    public Texture2D enemy;
    public Texture2D Anchor;
    //  public Texture2D movingPoint;
    // public Texture2D cursorTexture;
    public CursorMode cm = CursorMode.Auto;
    public Vector2 hotspot = Vector2.zero;

    // Use this for initialization
    void Start ()
    {
        SetMouse();
	}
    public void SetMouse()
    {
        Cursor.SetCursor(mouse, hotspot, cm);
    }
    public void SetInteract()
    {
        Cursor.SetCursor(goldmine, hotspot, cm);
    }
    public void SetStone()
    {
        Cursor.SetCursor(stonemine, hotspot, cm);
    }
    public void SetEnemy()
    {
        Cursor.SetCursor(enemy, hotspot, cm);
    }
    public void SetAnchor()
    {
        Cursor.SetCursor(Anchor, hotspot, cm);
    }
}
