using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public enum ResourceTypes { Gold, Wood, Stone, Food, Iron, Null }
    public ResourceTypes resourceType;
    public GameObject selectionEffect;
    public string objectName;
    public Sprite objectSprite;
    public string objectInfo1;
    public string objectInfo2;
    public float harvestTime;
    public float availableResource;

    public bool ResourceSelected;
    private ResourceTypes heldResourceType;

    private ActivePlayer player;

    public PlaceableBuilding building;
    public Bounds bounds;

    public int gatherers, lockedOnGatherers, maxLockedOnGatherers;

    void Start()
    {
        player = FindObjectOfType<ActivePlayer>();

        player.resources.Add(this);

        if (GetComponent<PlaceableBuilding>())
        {
            building = GetComponent<PlaceableBuilding>();
        }
        
        bounds = GetComponent<Collider>().bounds;

        if (selectionEffect)
        {
            selectionEffect.transform.localPosition = new Vector3(0, 0.1f, 0);
        }
    }

    private void Update()
    {
        if (availableResource <= 0)
        {
            player.resources.Remove(this);

            Destroy(gameObject);
        }
        else
        {
            if (ResourceSelected)
            {
                if (selectionEffect)
                {
                    if (!selectionEffect.activeSelf)
                    {
                        selectionEffect.SetActive(true);
                    }
                }
            }
            else
            {
                if (selectionEffect)
                {
                    if (selectionEffect.activeSelf)
                    {
                        selectionEffect.SetActive(false);
                    }
                }
            }
        }
    }

    void OnMouseEnter()
    {
        if (enabled)
        {
            player.cursor.SetInteract();
        }
        
    }

    void OnMouseExit()
    {
        if (enabled)
        {
            player.cursor.SetMouse();
        }
        
    }

}
