using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_StateMachine : MonoBehaviour
{
    private enemyUnit EU;
    private HandleCursor cursor;

    public enemyBuildManager EBM;
    public enemyResourceManager ERM;

    public AI_Supervisor AIS;
    private AI_Supervisor[] AI_SMulti;

    public int player_ID;

    private PlaceableBuilding bldng;

    // Start is called before the first frame update
    void Start()
    {
        EU = GetComponent<enemyUnit>();

        if (!AIS)
        {
            FindAISupervisorWithSamePlayerID();
        }

        cursor = Camera.main.GetComponent<HandleCursor>();

        if(EU.unitType == 1 || (EU.unitType == 4 && EU.shipCanGather))
        {
            EU.InitializeBuildPos();

            AIS.gatherers.Add(EU);
        }
        else if(EU.unitType == 2)
        {
            AIS.combatants.Add(EU);
        }

        EU.AccountForEnemyResearch();

    }

    // Update is called once per frame
    void Update()
    {
        if (!EU.death)
        {
            ActionCheck();
        }
    }

    public void ActionCheck()
    {
        if (EU.unitType == 1 && !EU.underAttack)
        {
            if(AIS.buildOrder.Count > 0)
            {
                if (CheckCosts(AIS.buildOrder[0]) && !EU.isBuilding)
                {
                    enemyDropSite eds, nearesteds;
                    NodeManager nm = null;

                    if (eds = AIS.buildOrder[0].GetComponent<enemyDropSite>())
                    {
                        if (eds.Food)
                        {
                            nm = EU.GetClosestUnlockedResourceOfType(EU.player.resources.ToArray(), NodeManager.ResourceTypes.Food);
                        }
                        else if (eds.Wood)
                        {
                            nm = EU.GetClosestUnlockedResourceOfType(EU.player.resources.ToArray(), NodeManager.ResourceTypes.Wood);
                        }
                        else if (eds.Gold)
                        {
                            nm = EU.GetClosestUnlockedResourceOfType(EU.player.resources.ToArray(), NodeManager.ResourceTypes.Gold);
                        }
                        else if (eds.Stone)
                        {
                            nm = EU.GetClosestUnlockedResourceOfType(EU.player.resources.ToArray(), NodeManager.ResourceTypes.Stone);
                        }
                        else if (eds.Iron)
                        {
                            nm = EU.GetClosestUnlockedResourceOfType(EU.player.resources.ToArray(), NodeManager.ResourceTypes.Iron);
                        }

                        nearesteds = EU.GetClosestDropSite(EU.player.enemyDropSites.ToArray(), nm.resourceType, nm.transform.position);

                        if(Vector3.Distance(nm.transform.position, nearesteds.transform.position) > 80f)
                        {
                            EU.Build(AIS.buildOrder[0], nm.transform.position);

                            AIS.buildOrder.Remove(AIS.buildOrder[0]);
                            AIS.buildPositions.Remove(AIS.buildPositions[0]);

                            AccountForBuildingCost();
                        }
                        else
                        {
                            AIS.buildOrder.Remove(AIS.buildOrder[0]);
                            AIS.buildPositions.Remove(AIS.buildPositions[0]);
                        }
                    }
                    else
                    {
                        EU.Build(AIS.buildOrder[0], AIS.buildPositions[0]);

                        AIS.buildOrder.Remove(AIS.buildOrder[0]);
                        AIS.buildPositions.Remove(AIS.buildPositions[0]);

                        AccountForBuildingCost();
                    }
                }
                else if (EU.isBuilding == false)
                {
                    //EU.findMinimumResource();
                    if(EU.heldResource < EU.MaxHeldResource)
                    {
                        if (AIS.targetResource[1])
                        {
                            EU.GoToNearestUnlockedResourceOfType(NodeManager.ResourceTypes.Wood);
                        }
                        else if (AIS.targetResource[2])
                        {
                            EU.GoToNearestUnlockedResourceOfType(NodeManager.ResourceTypes.Gold);
                        }
                        else if (AIS.gatherers.Count < 5)
                        {
                            if (AIS.gathererCountByType[1] < 2)
                            {
                                EU.GoToNearestUnlockedResourceOfType(NodeManager.ResourceTypes.Wood);
                            }
                        }
                        else
                        {
                            EU.GoToNearestUnlockedResource();
                        }
                    }
                    else
                    {
                        EU.GoToNearestDropSite();
                    }

                    BuildDropSiteIfSiteTooFar();

                }
                else if (EU.isBuilding == true)
                {
                    EU.GoToBuild(EU.newBuilding);
                }
            }
            else
            {
                if (EU.isBuilding == false)
                {
                    //EU.findMinimumResource();

                    if (AIS.targetResource[1])
                    {
                        EU.GoToNearestUnlockedResourceOfType(NodeManager.ResourceTypes.Wood);
                    }
                    else
                    {
                        EU.GoToNearestUnlockedResource();
                    }

                    EU.GoToNearestDropSite();

                    BuildDropSiteIfSiteTooFar();

                }
                else if (EU.isBuilding == true)
                {
                    EU.GoToBuild(EU.newBuilding);
                }
            }
        }
        else if(EU.unitType == 4 && EU.shipCanGather && !EU.underAttack)
        {
            EU.GoToNearestUnlockedResourceOfType(NodeManager.ResourceTypes.Food);

            EU.GoToNearestDropSite();
        }
    }

    void FindAISupervisorWithSamePlayerID()
    {
        AI_SMulti = FindObjectsOfType<AI_Supervisor>();

        if (AI_SMulti.Length > 0)
        {
            foreach (AI_Supervisor ais in AI_SMulti)
            {
                if (ais.player_ID == player_ID)
                {
                    AIS = ais;
                }
            }

            ERM = AIS.GetComponent<enemyResourceManager>();
            EBM = AIS.GetComponent<enemyBuildManager>();

            EU.ERM = ERM;
            EU.EBM = EBM;

            AIS.currentPopulation += EU.populationWeight;
        }
    }

    public void BuildDropSiteIfSiteTooFar()
    {
        if (EU.SiteTooFar == true)
        {
            if (EU.heldResourceType == NodeManager.ResourceTypes.Food)
            {
                BuildDropSite(4);
            }
            else if(EU.heldResourceType == NodeManager.ResourceTypes.Wood)
            {
                BuildDropSite(5);
            }
            else if (EU.heldResourceType == NodeManager.ResourceTypes.Gold)
            {
                BuildDropSite(3);
            }
            else if (EU.heldResourceType == NodeManager.ResourceTypes.Stone)
            {
                BuildDropSite(3);
            }
            else if (EU.heldResourceType == NodeManager.ResourceTypes.Iron)
            {
                BuildDropSite(3);
            }

            EU.SiteTooFar = false;
        }
    }

    public void BuildDropSite(int index)
    {
        if (CheckCosts(EBM.buildings[index]))
        {
            if (EU.resourceDistance <= EU.resourceBounds)
            {
                Vector3 pos = new Vector3(EU.lockedOnResource.transform.position.x, 50f, EU.lockedOnResource.transform.position.z);

                EU.Build(EBM.buildings[index], pos);
            }
        }
    }

    public void AccountForBuildingCost()
    {
        bldng = EU.newBuilding.GetComponent<PlaceableBuilding>();

        ERM.wood -= bldng.GetComponent<PlaceableBuilding>().woodCost;
        ERM.food -= bldng.GetComponent<PlaceableBuilding>().foodCost;
        ERM.gold -= bldng.GetComponent<PlaceableBuilding>().goldCost;
        ERM.stone -= bldng.GetComponent<PlaceableBuilding>().stoneCost;
        ERM.iron -= bldng.GetComponent<PlaceableBuilding>().ironCost;
    }

    public bool CheckCosts(GameObject building)
    {
        bldng = building.GetComponent<PlaceableBuilding>();

        if(bldng.foodCost > ERM.food)
        {
            return false;
        }
        else if (bldng.woodCost > ERM.wood)
        {
            return false;
        }
        else if (bldng.goldCost > ERM.gold)
        {
            return false;
        }
        else if (bldng.stoneCost > ERM.stone)
        {
            return false;
        }
        else if (bldng.ironCost > ERM.iron)
        {
            return false;
        }

        return true;
    }

    void OnMouseEnter()
    {
        cursor.SetEnemy();
    }

    void OnMouseExit()
    {
        cursor.SetMouse();
    }
}
