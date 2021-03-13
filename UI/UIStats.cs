using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStats : MonoBehaviour
{
    private UserControl UC;
    private objectSelection OS;

    public bool health, damage, slashArmour, pierceArmour, standardFortification, siegeFortification, heldResource, resourceType, objectName;
    public bool garrisoned, garrisonCapacity, populationContribution, objectSprite, objectInfo1, objectInfo2;

    private Text txt;
    private Image img;
    private Player_Unit PU;
    private enemyUnit EU;
    private NodeManager nm;

    // Start is called before the first frame update
    void Start()
    {
        UC = FindObjectOfType<UserControl>();
        OS = UC.GetComponent<objectSelection>();

        if (objectSprite)
        {
            img = GetComponent<Image>();
        }
        else
        {
            txt = GetComponent<Text>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStats();
    }

    public void UpdateStats()
    {
        if (health)
        {
            if(UC.selectedObjectType != 8)
            {
                txt.text = OS.HB.currentHealth.ToString();
            }
        }
        else if (damage)
        {
            if (UC.selectedObjectType == 1)
            {
                txt.text = OS.PU.damage.ToString();
            }
            else if (UC.selectedObjectType == 2)
            {
                txt.text = OS.EU.damage.ToString();
            }
            else if (UC.selectedObjectType == 4)
            {
                txt.text = OS.PS.damage.ToString();
            }
            else if (UC.selectedObjectType == 7)
            {
                txt.text = OS.wa.damage.ToString();
            }
            else if (UC.selectedObjectType == 3)
            {
                txt.text = OS.npc.damage.ToString();
            }
            else if (UC.selectedObjectType == 6)
            {
                txt.text = OS.npcBuilding.damage.ToString();
            }
        }
        else if (slashArmour)
        {
            if (UC.selectedObjectType == 1)
            {
                txt.text = OS.PU.bluntArmour.ToString();
            }
            else if (UC.selectedObjectType == 2)
            {
                txt.text = OS.EU.bluntArmour.ToString();
            }
            else if (UC.selectedObjectType == 3)
            {
                txt.text = OS.npc.bluntArmour.ToString();
            }
        }
        else if (pierceArmour)
        {
            if (UC.selectedObjectType == 1)
            {
                txt.text = OS.PU.pierceArmour.ToString();
            }
            else if (UC.selectedObjectType == 2)
            {
                txt.text = OS.EU.pierceArmour.ToString();
            }
            else if (UC.selectedObjectType == 3)
            {
                txt.text = OS.npc.pierceArmour.ToString();
            }
        }
        else if (standardFortification)
        {
            if (UC.selectedObjectType == 4)
            {
                txt.text = OS.PS.standardFortification.ToString();
            }
            else if (UC.selectedObjectType == 5)
            {
                txt.text = OS.ES.standardFortification.ToString();
            }
            else if (UC.selectedObjectType == 6)
            {
                txt.text = OS.npcBuilding.standardFortification.ToString();
            }
        }
        else if (siegeFortification)
        {
            if (UC.selectedObjectType == 4)
            {
                txt.text = OS.PS.siegeFortification.ToString();
            }
            else if (UC.selectedObjectType == 5)
            {
                txt.text = OS.ES.siegeFortification.ToString();
            }
            else if (UC.selectedObjectType == 6)
            {
                txt.text = OS.npcBuilding.siegeFortification.ToString();
            }
        }
        else if (heldResource)
        {
            if (UC.selectedObjectType == 1)
            {
                if(OS.PU.heldResource > 0)
                {
                    txt.text = OS.PU.heldResource.ToString();
                }
                else
                {
                    txt.text = "";
                }
            }
            else if (UC.selectedObjectType == 2)
            {
                if(OS.EU.heldResource > 0)
                {
                    txt.text = OS.EU.heldResource.ToString();
                }
                else
                {
                    txt.text = "";
                }
            }
            else if (UC.selectedObjectType == 8)
            {
                if(OS.nm.availableResource > 0)
                {
                    txt.text = OS.nm.availableResource.ToString();
                }
                else
                {
                    txt.text = "";
                }
            }
        }
        else if (resourceType)
        {
            if (UC.selectedObjectType == 1)
            {
                PU = OS.PU;

                if(PU.heldResource > 0)
                {
                    if (PU.heldResourceType == NodeManager.ResourceTypes.Food)
                    {
                        txt.text = "Food";
                    }
                    else if (PU.heldResourceType == NodeManager.ResourceTypes.Wood)
                    {
                        txt.text = "Wood";
                    }
                    else if (PU.heldResourceType == NodeManager.ResourceTypes.Gold)
                    {
                        txt.text = "Gold";
                    }
                    else if (PU.heldResourceType == NodeManager.ResourceTypes.Stone)
                    {
                        txt.text = "Stone";
                    }
                    else if (PU.heldResourceType == NodeManager.ResourceTypes.Iron)
                    {
                        txt.text = "Iron";
                    }
                }
                else
                {
                    txt.text = "";
                }
            }
            else if (UC.selectedObjectType == 2)
            {
                EU = OS.EU;

                if(EU.heldResource > 0)
                {
                    if (EU.heldResourceType == NodeManager.ResourceTypes.Food)
                    {
                        txt.text = "Food";
                    }
                    else if (EU.heldResourceType == NodeManager.ResourceTypes.Wood)
                    {
                        txt.text = "Wood";
                    }
                    else if (EU.heldResourceType == NodeManager.ResourceTypes.Gold)
                    {
                        txt.text = "Gold";
                    }
                    else if (EU.heldResourceType == NodeManager.ResourceTypes.Stone)
                    {
                        txt.text = "Stone";
                    }
                    else if (EU.heldResourceType == NodeManager.ResourceTypes.Iron)
                    {
                        txt.text = "Iron";
                    }
                }
                else
                {
                    txt.text = "";
                }
            }
            else if (UC.selectedObjectType == 8)
            {
                nm = OS.nm;

                if (nm.resourceType == NodeManager.ResourceTypes.Food)
                {
                    txt.text = "Food";
                }
                else if (nm.resourceType == NodeManager.ResourceTypes.Wood)
                {
                    txt.text = "Wood";
                }
                else if (nm.resourceType == NodeManager.ResourceTypes.Gold)
                {
                    txt.text = "Gold";
                }
                else if (nm.resourceType == NodeManager.ResourceTypes.Stone)
                {
                    txt.text = "Stone";
                }
                else if (nm.resourceType == NodeManager.ResourceTypes.Iron)
                {
                    txt.text = "Iron";
                }
            }
        }
        else if (objectName)
        {
            if (UC.selectedObjectType == 1)
            {
                txt.text = OS.PU.objectName;
            }
            else if (UC.selectedObjectType == 2)
            {
                txt.text = OS.EU.objectName;
            }
            else if (UC.selectedObjectType == 4)
            {
                txt.text = OS.PS.building.objectName;
            }
            else if (UC.selectedObjectType == 5)
            {
                txt.text = OS.ES.building.objectName;
            }
            else if (UC.selectedObjectType == 6)
            {
                txt.text = OS.npcBuilding.objectName;
            }
            else if (UC.selectedObjectType == 7)
            {
                txt.text = OS.wa.objectName;
            }
            else if (UC.selectedObjectType == 8)
            {
                txt.text = OS.nm.objectName;
            }
            else if (UC.selectedObjectType == 3)
            {
                txt.text = OS.npc.objectName;
            }
        }
        else if (garrisonCapacity)
        {
            if (UC.selectedObjectType == 4)
            {
                txt.text = OS.PS.garrisonCapacity.ToString();
            }
        }
        else if (garrisoned)
        {
            if (UC.selectedObjectType == 4)
            {
                if (OS.PS.garrisonedUnits.Count > 0)
                {
                    txt.text = OS.PS.garrisonedUnits.Count.ToString();
                }
                else
                {
                    txt.text = "";
                }
            }
        }
        else if (populationContribution)
        {
            if (UC.selectedObjectType == 4)
            {
                txt.text = OS.PS.populationContribution.ToString();
            }
        }
        else if (objectSprite)
        {
            if (UC.selectedObjectType == 1)
            {
                img.sprite = OS.PU.objectSprite;
            }
            else if (UC.selectedObjectType == 2)
            {
                img.sprite = OS.EU.objectSprite;
            }
            else if (UC.selectedObjectType == 4)
            {
                img.sprite = OS.PS.building.objectSprite;
            }
            else if (UC.selectedObjectType == 5)
            {
                img.sprite = OS.ES.building.objectSprite;
            }
            else if (UC.selectedObjectType == 6)
            {
                img.sprite = OS.npcBuilding.objectSprite;
            }
            else if (UC.selectedObjectType == 7)
            {
                img.sprite = OS.wa.objectSprite;
            }
            else if (UC.selectedObjectType == 8)
            {
                img.sprite = OS.nm.objectSprite;
            }
            else if (UC.selectedObjectType == 3)
            {
                img.sprite = OS.npc.objectSprite;
            }
        }
        else if (objectInfo1)
        {
            if (UC.selectedObjectType == 1)
            {
                txt.text = OS.PU.objectInfo1;
            }
            else if (UC.selectedObjectType == 2)
            {
                txt.text = OS.EU.objectInfo1;
            }
            else if (UC.selectedObjectType == 4)
            {
                txt.text = OS.PS.building.objectInfo1;
            }
            else if (UC.selectedObjectType == 5)
            {
                txt.text = OS.ES.building.objectInfo1;
            }
            else if (UC.selectedObjectType == 6)
            {
                txt.text = OS.npcBuilding.objectInfo1;
            }
            else if (UC.selectedObjectType == 7)
            {
                txt.text = OS.wa.objectInfo1;
            }
            else if (UC.selectedObjectType == 8)
            {
                txt.text = OS.nm.objectInfo1;
            }
            else if (UC.selectedObjectType == 3)
            {
                txt.text = OS.npc.objectInfo1;
            }
        }
        else if (objectInfo2)
        {
            if (UC.selectedObjectType == 1)
            {
                txt.text = OS.PU.objectInfo2;
            }
            else if (UC.selectedObjectType == 2)
            {
                txt.text = OS.EU.objectInfo2;
            }
            else if (UC.selectedObjectType == 4)
            {
                txt.text = OS.PS.building.objectInfo2;
            }
            else if (UC.selectedObjectType == 5)
            {
                txt.text = OS.ES.building.objectInfo2;
            }
            else if (UC.selectedObjectType == 6)
            {
                txt.text = OS.npcBuilding.objectInfo2;
            }
            else if (UC.selectedObjectType == 7)
            {
                txt.text = OS.wa.objectInfo2;
            }
            else if (UC.selectedObjectType == 8)
            {
                txt.text = OS.nm.objectInfo2;
            }
            else if (UC.selectedObjectType == 3)
            {
                txt.text = OS.npc.objectInfo2;
            }
        }
    }
}
