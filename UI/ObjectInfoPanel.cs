using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInfoPanel : MonoBehaviour
{
    private UserControl UC;
    private objectSelection OS;

    public GameObject content, resourceInfo, objectInfo, garrisonInfo, popInfo, buildingArmourInfo, unitArmourInfo;

    private void Start()
    {
        UC = FindObjectOfType<UserControl>();
        OS = UC.GetComponent<objectSelection>();
    }

    void Update()
    {
        if(UC.counter == 1)
        {
            if (UC.prevSelectedObject)
            {
                if (!content.activeSelf)
                {
                    content.SetActive(true);
                }

                CheckExtraInfo();
            }
            else
            {
                if (content.activeSelf)
                {
                    content.SetActive(false);
                }
            }
        }
        else if(UC.counter > 1)
        {
            if (UC.selectedObject)
            {
                if (!content.activeSelf)
                {
                    content.SetActive(true);
                }

                CheckExtraInfo();
            }
            else
            {
                if (content.activeSelf)
                {
                    content.SetActive(false);
                }
            }
        }
    }

    private void CheckExtraInfo()
    {
        if(UC.selectedObjectType == 1)
        {
            if(OS.PU.unitType == 1 || (OS.PU.unitType == 4 && OS.PU.shipCanGather))
            {
                ActivateInfo(resourceInfo);
            }
            else
            {
                ActivateInfo(objectInfo);
            }

            ActivateInfo(unitArmourInfo);
        }
        else if (UC.selectedObjectType == 2)
        {
            if (OS.EU.unitType == 1 || (OS.EU.unitType == 4 && OS.EU.shipCanGather))
            {
                ActivateInfo(resourceInfo);
            }
            else
            {
                ActivateInfo(objectInfo);
            }

            ActivateInfo(unitArmourInfo);
        }
        else if (UC.selectedObjectType == 8 || UC.selectedObjectType == 7)
        {
            ActivateInfo(resourceInfo);
        }
        else if(UC.selectedObjectType == 4)
        {
            if (OS.PS.canGarrison)
            {
                ActivateInfo(garrisonInfo);
            }
            else if(OS.PS.populationContribution > 0)
            {
                ActivateInfo(popInfo);
            }
            else
            {
                ActivateInfo(objectInfo);
            }

            ActivateInfo(buildingArmourInfo);
        }
        else if(UC.selectedObjectType == 5)
        {
            if (OS.ES.canGarrison)
            {
                ActivateInfo(garrisonInfo);
            }
            else if (OS.ES.populationContribution > 0)
            {
                ActivateInfo(popInfo);
            }
            else
            {
                ActivateInfo(objectInfo);
            }

            ActivateInfo(buildingArmourInfo);
        }
        else if(UC.selectedObjectType == 6)
        {
            ActivateInfo(buildingArmourInfo);
        } 
        else
        {
            ActivateInfo(objectInfo);
        }
    }

    private void ActivateInfo(GameObject info)
    {
        if (info == resourceInfo)
        {
            if (garrisonInfo.activeSelf || objectInfo.activeSelf || popInfo.activeSelf || !resourceInfo.activeSelf)
            {
                objectInfo.SetActive(false);
                garrisonInfo.SetActive(false);
                popInfo.SetActive(false);
                resourceInfo.SetActive(true);
            }
        }
        else if (info == objectInfo)
        {
            if (garrisonInfo.activeSelf || resourceInfo.activeSelf || popInfo.activeSelf || !objectInfo.activeSelf)
            {
                resourceInfo.SetActive(false);
                garrisonInfo.SetActive(false);
                popInfo.SetActive(false);
                objectInfo.SetActive(true);
            }
        }
        else if (info == garrisonInfo)
        {
            if (resourceInfo.activeSelf || objectInfo.activeSelf || popInfo.activeSelf || !garrisonInfo.activeSelf)
            {
                objectInfo.SetActive(false);
                resourceInfo.SetActive(false);
                popInfo.SetActive(false);
                garrisonInfo.SetActive(true);
            }
        }
        else if(info == popInfo)
        {
            if (garrisonInfo.activeSelf || objectInfo.activeSelf || resourceInfo.activeSelf || !popInfo.activeSelf)
            {
                objectInfo.SetActive(false);
                resourceInfo.SetActive(false);
                garrisonInfo.SetActive(false);
                popInfo.SetActive(true);
            }
        }
        else if (info == unitArmourInfo)
        {
            if (buildingArmourInfo.activeSelf || !unitArmourInfo.activeSelf)
            {
                buildingArmourInfo.SetActive(false);
                unitArmourInfo.SetActive(true);
            }
        }
        else if(info == buildingArmourInfo)
        {
            if (unitArmourInfo.activeSelf || !buildingArmourInfo.activeSelf)
            {
                unitArmourInfo.SetActive(false);
                buildingArmourInfo.SetActive(true);
            }
        }
    }
}
