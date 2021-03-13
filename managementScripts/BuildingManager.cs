using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    public ResourceManager RM;
    public GameObject[] buildings;

    public BuildingPlacement buildingPlacement;

    private PlaceableBuilding building;

    // Use this for initialization
    void Start()
    {
        buildingPlacement = GetComponent<BuildingPlacement>();
    }

    public void SetBuilding(int index)
    {
        if (CheckCosts(buildings[index]))
        {
            if (!buildingPlacement.isPlacing)
            {
                buildingPlacement.SetItem(buildings[index]);
            }
        }
    }

    private bool CheckCosts(GameObject bldng)
    {
        building = bldng.GetComponent<PlaceableBuilding>();

        if (building.foodCost > RM.food)
        {
            return false;
        }
        else if (building.woodCost > RM.wood)
        {
            return false;
        }
        else if (building.goldCost > RM.gold)
        {
            return false;
        }
        else if (building.stoneCost > RM.stone)
        {
            return false;
        }
        else if (building.ironCost > RM.iron)
        {
            return false;
        }

        return true;
    }
}
   

