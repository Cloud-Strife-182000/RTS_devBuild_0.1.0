using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class objectSelection : MonoBehaviour
{
    public UserControl UC;

    public Player_Unit PU;
    public enemyUnit EU;
    public Player_Structure PS;
    public enemyStructure ES;
    public NodeManager nm;
    public Wander wa;
    public HealthBar HB;

    public NPC npc;
    public NPCBuilding npcBuilding;

    public bool isSelecting;
    private RectTransform rt;

    public Vector3 startScreenPos;
    public List<Player_Unit> selectables;
    public Canvas canvas;
    public Image selectionBox;

    private ActivePlayer player;
    private PlaceableBuilding bldng;

    private LayerMask unitMask = 1 << 8;
    public bool selectedUnit;

    public List<Player_Unit> selectedUnits;

    // Start is called before the first frame update
    void Start()
    {
        UC = GetComponent<UserControl>();
        rt = selectionBox.GetComponent<RectTransform>();
        player = GetComponent<ActivePlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!UC.techTree.activeSelf && !UC.mainMenu.activeSelf && !player.subtitles.activeSelf)
        {
            if (Input.GetMouseButtonDown(0) && !UC.cursorIsBusy)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                LeftClick();
                    
                startScreenPos = Input.mousePosition;
                isSelecting = true;
                selectables = player.player_units;
            }

            SelectionBoxCheck();
        }
    }

    public void LeftClick()
    {
        selectedUnit = false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, unitMask))
        {
            if (hit.collider.GetComponent<Player_Unit>())
            {
                selectedUnit = true;

                if (UC.counter == 0)
                {
                    UC.prevSelectedObject = hit.collider.gameObject;
                    PU = UC.prevSelectedObject.GetComponent<Player_Unit>();
                    PU.isSelected = true;
                    UC.selectedObjectType = 1;
                    HB = PU.HB;
                    UC.tempObject = UC.prevSelectedObject;
                    UC.counter++;
                }
                else
                {
                    UC.prevSelectedObject = UC.tempObject;

                    if (!UC.prevSelectedObject)
                    {
                        UC.selectedObject = hit.collider.gameObject;
                        PU = UC.selectedObject.GetComponent<Player_Unit>();
                        PU.isSelected = true;
                        HB = PU.HB;
                        UC.selectedObjectType = 1;
                        UC.tempObject = UC.selectedObject;
                        UC.counter++;
                    }

                    if (UC.prevSelectedObject)
                    {
                        SelCheck(UC.prevSelectedObject);
                    }

                    UC.selectedObject = hit.collider.gameObject;
                    PU = UC.selectedObject.GetComponent<Player_Unit>();
                    PU.isSelected = true;
                    HB = PU.HB;
                    UC.selectedObjectType = 1;
                    UC.tempObject = UC.selectedObject;
                    UC.counter++;
                }
            }
            else if (hit.collider.GetComponent<enemyUnit>())
            {
                selectedUnit = true;

                if (UC.counter == 0)
                {
                    UC.prevSelectedObject = hit.collider.gameObject;
                    EU = UC.prevSelectedObject.GetComponent<enemyUnit>();
                    EU.isSelected = true;
                    HB = EU.HB;
                    UC.selectedObjectType = 2;
                    UC.tempObject = UC.prevSelectedObject;
                    UC.counter++;
                }
                else
                {
                    UC.prevSelectedObject = UC.tempObject;

                    if (!UC.prevSelectedObject)
                    {
                        UC.selectedObject = hit.collider.gameObject;
                        EU = UC.selectedObject.GetComponent<enemyUnit>();
                        EU.isSelected = true;
                        HB = EU.HB;
                        UC.selectedObjectType = 2;
                        UC.tempObject = UC.selectedObject;
                        UC.counter++;
                    }

                    if (UC.prevSelectedObject)
                    {
                        SelCheck(UC.prevSelectedObject);
                    }

                    UC.selectedObject = hit.collider.gameObject;
                    EU = UC.selectedObject.GetComponent<enemyUnit>();
                    EU.isSelected = true;
                    HB = EU.HB;
                    UC.selectedObjectType = 2;
                    UC.tempObject = UC.selectedObject;
                    UC.counter++;
                }
            }
            else if (hit.collider.GetComponent<NPC>())
            {
                selectedUnit = true;

                if (UC.counter == 0)
                {
                    UC.prevSelectedObject = hit.collider.gameObject;
                    npc = UC.prevSelectedObject.GetComponent<NPC>();
                    npc.isSelected = true;
                    HB = npc.HB;
                    UC.selectedObjectType = 3;
                    UC.tempObject = UC.prevSelectedObject;
                    UC.counter++;
                }
                else
                {
                    UC.prevSelectedObject = UC.tempObject;

                    if (!UC.prevSelectedObject)
                    {
                        UC.selectedObject = hit.collider.gameObject;
                        npc = UC.selectedObject.GetComponent<NPC>();
                        npc.isSelected = true;
                        HB = npc.HB;
                        UC.selectedObjectType = 3;
                        UC.tempObject = UC.selectedObject;
                        UC.counter++;
                    }

                    if (UC.prevSelectedObject)
                    {
                        SelCheck(UC.prevSelectedObject);
                    }

                    UC.selectedObject = hit.collider.gameObject;
                    npc = UC.selectedObject.GetComponent<NPC>();
                    npc.isSelected = true;
                    HB = npc.HB;
                    UC.selectedObjectType = 3;
                    UC.tempObject = UC.selectedObject;
                    UC.counter++;
                }

            }
        }

        if (!selectedUnit)
        {
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.collider.GetComponent<Player_Structure>())
                {
                    selectedUnit = false;

                    if (UC.counter == 0)
                    {
                        UC.prevSelectedObject = hit.collider.gameObject;
                        PS = UC.prevSelectedObject.GetComponent<Player_Structure>();
                        PS.isSelected = true;
                        HB = PS.HB;
                        UC.selectedObjectType = 4;
                        UC.tempObject = UC.prevSelectedObject;
                        UC.counter++;
                    }
                    else
                    {
                        UC.prevSelectedObject = UC.tempObject;

                        if (!UC.prevSelectedObject)
                        {
                            UC.selectedObject = hit.collider.gameObject;
                            PS = UC.selectedObject.GetComponent<Player_Structure>();
                            PS.isSelected = true;
                            HB = PS.HB;
                            UC.selectedObjectType = 4;
                            UC.tempObject = UC.selectedObject;
                            UC.counter++;
                        }

                        if (UC.prevSelectedObject)
                        {
                            SelCheck(UC.prevSelectedObject);
                        }

                        UC.selectedObject = hit.collider.gameObject;
                        PS = UC.selectedObject.GetComponent<Player_Structure>();
                        PS.isSelected = true;
                        HB = PS.HB;
                        UC.selectedObjectType = 4;
                        UC.tempObject = UC.selectedObject;
                        UC.counter++;
                    }
                }
                else if (hit.collider.GetComponent<enemyStructure>())
                {
                    selectedUnit = false;

                    if (UC.counter == 0)
                    {
                        UC.prevSelectedObject = hit.collider.gameObject;
                        ES = UC.prevSelectedObject.GetComponent<enemyStructure>();
                        ES.isSelected = true;
                        HB = ES.HB;
                        UC.selectedObjectType = 5;
                        UC.tempObject = UC.prevSelectedObject;
                        UC.counter++;
                    }
                    else
                    {
                        UC.prevSelectedObject = UC.tempObject;

                        if (!UC.prevSelectedObject)
                        {
                            UC.selectedObject = hit.collider.gameObject;
                            ES = UC.selectedObject.GetComponent<enemyStructure>();
                            ES.isSelected = true;
                            HB = ES.HB;
                            UC.selectedObjectType = 5;
                            UC.tempObject = UC.selectedObject;
                            UC.counter++;
                        }

                        if (UC.prevSelectedObject)
                        {
                            SelCheck(UC.prevSelectedObject);
                        }

                        UC.selectedObject = hit.collider.gameObject;
                        ES = UC.selectedObject.GetComponent<enemyStructure>();
                        ES.isSelected = true;
                        HB = ES.HB;
                        UC.selectedObjectType = 5;
                        UC.tempObject = UC.selectedObject;
                        UC.counter++;
                    }
                }
                else if (hit.collider.GetComponent<NPCBuilding>())
                {
                    selectedUnit = false;

                    if (UC.counter == 0)
                    {
                        UC.prevSelectedObject = hit.collider.gameObject;
                        npcBuilding = UC.selectedObject.GetComponent<NPCBuilding>();
                        npcBuilding.isSelected = true;
                        HB = npcBuilding.HB;
                        UC.selectedObjectType = 6;
                        UC.tempObject = UC.prevSelectedObject;
                        UC.counter++;
                    }
                    else
                    {
                        UC.prevSelectedObject = UC.tempObject;

                        if (!UC.prevSelectedObject)
                        {
                            UC.selectedObject = hit.collider.gameObject;
                            npcBuilding = UC.selectedObject.GetComponent<NPCBuilding>();
                            npcBuilding.isSelected = true;
                            HB = npcBuilding.HB;
                            UC.selectedObjectType = 6;
                            UC.tempObject = UC.selectedObject;
                            UC.counter++;
                        }

                        if (UC.prevSelectedObject)
                        {
                            SelCheck(UC.prevSelectedObject);
                        }

                        UC.selectedObject = hit.collider.gameObject;
                        npcBuilding = UC.selectedObject.GetComponent<NPCBuilding>();
                        npcBuilding.isSelected = true;
                        HB = npcBuilding.HB;
                        UC.selectedObjectType = 6;
                        UC.tempObject = UC.selectedObject;
                        UC.counter++;
                    }
                }
                else if (hit.collider.GetComponent<Wander>() && hit.collider.GetComponent<Wander>().enabled)
                {
                    selectedUnit = false;

                    if (UC.counter == 0)
                    {
                        UC.prevSelectedObject = hit.collider.gameObject;
                        wa = UC.prevSelectedObject.GetComponent<Wander>();
                        wa.isSelected = true;
                        HB = wa.HB;
                        UC.selectedObjectType = 7;
                        UC.tempObject = UC.prevSelectedObject;
                        UC.counter++;
                    }
                    else
                    {
                        UC.prevSelectedObject = UC.tempObject;

                        if (!UC.prevSelectedObject)
                        {
                            UC.selectedObject = hit.collider.gameObject;
                            wa = UC.selectedObject.GetComponent<Wander>();
                            wa.isSelected = true;
                            HB = wa.HB;
                            UC.selectedObjectType = 7;
                            UC.tempObject = UC.selectedObject;
                            UC.counter++;
                        }

                        if (UC.prevSelectedObject)
                        {
                            SelCheck(UC.prevSelectedObject);
                        }

                        UC.selectedObject = hit.collider.gameObject;
                        wa = UC.selectedObject.GetComponent<Wander>();
                        wa.isSelected = true;
                        HB = wa.HB;
                        UC.selectedObjectType = 7;
                        UC.tempObject = UC.selectedObject;
                        UC.counter++;
                    }
                }
                else if (hit.collider.gameObject.GetComponent<NodeManager>())
                {
                    selectedUnit = false;

                    if (bldng = hit.collider.GetComponent<PlaceableBuilding>())
                    {
                        if (bldng.isBuilt)
                        {
                            resourceNodeSelection(hit);
                        }
                    }
                    else
                    {
                        resourceNodeSelection(hit);
                    }
                }
                else
                {
                    selectedUnit = false;

                    if (UC.prevSelectedObject)
                    {
                        SelCheck(UC.prevSelectedObject);
                    }

                    if (UC.tempObject != UC.prevSelectedObject)
                    {
                        SelCheck(UC.tempObject);
                    }

                    if (UC.selectedObject)
                    {
                        UC.selectedObject = null;
                    }

                    if(UC.counter == 1)
                    {
                        UC.prevSelectedObject = null;
                    }
                }
            }
        }
    }

    private void SelCheck(GameObject objectToCheck)
    {
        if (objectToCheck)
        {
            if (objectToCheck.GetComponent<Player_Unit>())
            {
                PU.isSelected = false;
            }
            else if (objectToCheck.GetComponent<Player_Structure>())
            {
                PS.isSelected = false;
            }
            else if (objectToCheck.GetComponent<enemyUnit>())
            {
                EU.isSelected = false;
            }
            else if (objectToCheck.GetComponent<NPC>())
            {
                npc.isSelected = false;
            }
            else if (objectToCheck.GetComponent<NPCBuilding>())
            {
                npcBuilding.isSelected = false;
            }
            else if (objectToCheck.GetComponent<enemyStructure>())
            {
                ES.isSelected = false;
            }
            else if (objectToCheck.GetComponent<Wander>())
            {
                if (objectToCheck.GetComponent<Wander>().enabled)
                {
                    wa.isSelected = false;
                }
            }
            else if (objectToCheck.GetComponent<NodeManager>())
            {
                nm.ResourceSelected = false;
            }

        }
    }

    private void resourceNodeSelection(RaycastHit hit)
    {
        if (UC.counter == 0)
        {
            UC.prevSelectedObject = hit.collider.gameObject;
            nm = UC.prevSelectedObject.GetComponent<NodeManager>();
            nm.ResourceSelected = true;
            UC.selectedObjectType = 8;
            UC.tempObject = UC.prevSelectedObject;
            UC.counter++;
        }
        else
        {
            UC.prevSelectedObject = UC.tempObject;

            if (!UC.prevSelectedObject)
            {
                UC.selectedObject = hit.collider.gameObject;
                nm = UC.selectedObject.GetComponent<NodeManager>();
                nm.ResourceSelected = true;
                UC.selectedObjectType = 8;
                UC.tempObject = UC.selectedObject;
                UC.counter++;
            }

            if (UC.prevSelectedObject)
            {
                SelCheck(UC.prevSelectedObject);
            }

            UC.selectedObject = hit.collider.gameObject;
            nm = UC.selectedObject.GetComponent<NodeManager>();
            nm.ResourceSelected = true;
            UC.selectedObjectType = 8;
            UC.tempObject = UC.selectedObject;
            UC.counter++;
        }
    }

    private void SelectionBoxCheck()
    {
        if (Input.GetMouseButtonUp(0))
        {
            isSelecting = false;
            selectionBox.gameObject.SetActive(false);
        }

        if (isSelecting)
        {
            selectionBox.gameObject.SetActive(true);
            Bounds b = new Bounds();

            b.center = Vector3.Lerp(startScreenPos, Input.mousePosition, 0.5f);

            b.size = new Vector3(Mathf.Abs(startScreenPos.x - Input.mousePosition.x),
            Mathf.Abs(startScreenPos.y - Input.mousePosition.y), 0);

            rt.position = b.center;
            rt.sizeDelta = canvas.transform.InverseTransformVector(b.size);

            foreach (Player_Unit selectable in selectables)
            {
                if (selectable)
                {
                    //If the screenPosition of the worldobject is within our selection bounds, we can add it to our selection
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(selectable.transform.position);
                    screenPos.z = 0;
                    UpdateSelection(selectable, (b.Contains(screenPos)));
                }
            }
        }
    }

    public void UpdateSelection(Player_Unit s, bool value)
    {
        if (s.isSelectedByBox != value)
        {
            s.isSelectedByBox = value;

            if (s.isSelectedByBox)
            {
                selectedUnits.Add(s);
            }
            else
            {
                selectedUnits.Remove(s);
            }
        }
    }

    public void ClearSelected()
    {
        selectables.ForEach(x => x.isSelectedByBox = false);
    }
}
