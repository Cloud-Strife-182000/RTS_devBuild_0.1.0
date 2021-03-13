using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VolumetricFogAndMist;
public class BuildingPlacement : MonoBehaviour
{
    public PlaceableBuilding placeableBuilding;
    public Transform currentBuilding;
    public bool hasPlaced;
    public ResourceManager RM;
    public ParticleSystem dust;
    public float terr_height = 0f;
    public Vector3 p, m;
    public bool placeableArea, allowSubmerge, isPlacing;

    private ActivePlayer player;
    private UserControl UC;
    private objectSelection OS;
    public VolumetricFog fogOfWar, secondaryFogOfWar;
    public float alphaAtBuildPos;

    public List<Player_Unit> builders;
    public List<PlaceableBuilding> walls;

    public PlaceableBuilding startWall, endWall;
    public Transform startWallPoint, endWallPoint;

    public Vector3 wallDirection;
    public float distBetWallPts;

    public List<bool> wallPosChecks;

    //dir = endpt - startpoint
    //pos of each seg = startpoint + dir.normalized * sizeofseg
    //rot of each seg = quaternion.lookrotation(dir)

    // Use this for initialization
    void Start()
    {
        player = FindObjectOfType<ActivePlayer>();
        UC = player.GetComponent<UserControl>();
        OS = player.GetComponent<objectSelection>();
        fogOfWar = player.fogOfWar;
        secondaryFogOfWar = player.secondaryFogOfWar;
    }

    // Update is called once per frame
    void Update()
    {
        Place();
    }

    public void Place()
    {
        if (currentBuilding != null && !hasPlaced)
        {
            isPlacing = true;
            m = Input.mousePosition;
            m = new Vector3(m.x, m.y, transform.position.y - terr_height);
            p = GetComponent<Camera>().ScreenToWorldPoint(m);

            if(placeableBuilding.traversingOverWater == true && allowSubmerge == false)
            {
                terr_height = placeableBuilding.waterHeight;
            }
            else if(placeableBuilding.onShoreLine == true)
            {
                terr_height = placeableBuilding.shoreHeight;
            }
            else
            {
                terr_height = Terrain.activeTerrain.SampleHeight(p) + Terrain.activeTerrain.transform.position.y;
            }

            currentBuilding.position = new Vector3(p.x, terr_height, p.z);

            if (fogOfWar.gameObject.activeSelf)
            {
                alphaAtBuildPos = fogOfWar.GetFogOfWarAlpha(currentBuilding.position);
            }
            else
            {
                alphaAtBuildPos = 0;
            }

            Vector3 temp = new Vector3(p.x, 150f, p.z);
            placeableArea = placeableBuilding.WaterCheck(p.x, p.z, placeableBuilding.bounds.extents.x + 1f, terr_height, temp, placeableBuilding, alphaAtBuildPos);

            if (endWallPoint && startWallPoint)
            {
                wallDirection = endWallPoint.position - startWallPoint.position;
                distBetWallPts = Vector3.Distance(endWallPoint.position, startWallPoint.position);

                float lengthOfSegment = placeableBuilding.wallSegment.lengthOfSegment;
                float numOfSegments = (distBetWallPts / lengthOfSegment) - 2;

                if (startWall.wallSegments.Count < numOfSegments)
                {
                    GameObject tempSegment = Instantiate(startWall.wallSegment.gameObject);
                    startWall.wallSegments.Add(tempSegment.GetComponent<PlaceableBuilding>());
                    wallPosChecks.Add(true);
                }

                for (int i=0; i<numOfSegments; i++)
                {
                    if(startWall.wallSegments.Count > i)
                    {
                        if (startWall.wallSegments[i])
                        {
                            Transform segmentTransform = startWall.wallSegments[i].transform;

                            segmentTransform.position = startWallPoint.position + lengthOfSegment * (i+1) * wallDirection.normalized;
                            segmentTransform.rotation = Quaternion.LookRotation(wallDirection);

                            float alpha = fogOfWar.GetFogOfWarAlpha(segmentTransform.position);
                            Vector3 temp1 = new Vector3(segmentTransform.position.x, 150f, segmentTransform.position.z);
                            wallPosChecks[i] = startWall.wallSegments[i].WaterCheck(segmentTransform.position.x, segmentTransform.position.z, startWall.wallSegments[i].bounds.extents.x + 1f, terr_height, temp1, startWall.wallSegments[i], alpha);

                        }
                    }
                }

                for (int i=0; i<10; i++)
                {
                    if (distBetWallPts < (i + 1) * lengthOfSegment)
                    {
                        if (startWall.wallSegments.Count > i)
                        {
                            if (startWall.wallSegments[i])
                            {
                                PlaceableBuilding tempSegment = startWall.wallSegments[i];
                                startWall.wallSegments.Remove(tempSegment);
                                player.placeableBuildings.Remove(tempSegment);
                                endWall.collider.Remove(tempSegment.GetComponent<Collider>());
                                Destroy(tempSegment.gameObject);
                                wallPosChecks.Remove(wallPosChecks[i]);
                            }
                        }
                    }
                }
            }

            if (Input.GetKey(KeyCode.R))
            {
                currentBuilding.Rotate(Vector3.down * 1f, Space.Self);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                if (placeableArea && !wallPosChecks.Contains(false))
                {
                    if (placeableBuilding.isWall)
                    {
                        if (UC.counter == 1)
                        {
                            if (UC.prevSelectedObject.GetComponent<Player_Unit>())
                            {
                                builders.Add(UC.prevSelectedObject.GetComponent<Player_Unit>());
                            }
                            else if (OS.selectedUnits.Count > 0)
                            {
                                foreach (Player_Unit PU in OS.selectedUnits)
                                {
                                    builders.Add(PU);
                                }
                            }
                        }
                        else if (UC.counter > 1)
                        {
                            if (UC.selectedObject.GetComponent<Player_Unit>())
                            {
                                builders.Add(UC.selectedObject.GetComponent<Player_Unit>());
                            }
                            else if (OS.selectedUnits.Count > 0)
                            {
                                foreach (Player_Unit PU in OS.selectedUnits)
                                {
                                    builders.Add(PU);
                                }
                            }
                        }

                        if (placeableBuilding.meshRend)
                        {
                            placeableBuilding.meshRend.material.SetColor("_Color", Color.white);
                        }

                        walls.Add(placeableBuilding);

                        if (startWall)
                        {
                            foreach(PlaceableBuilding wallSeg in startWall.wallSegments)
                            {
                                if (wallSeg)
                                {
                                    walls.Add(wallSeg);
                                }
                            }
                        }

                        startWall = placeableBuilding;
                        startWallPoint = startWall.transform;

                        SetItem(placeableBuilding.wallEndPoint);

                        endWall = placeableBuilding;
                        endWallPoint = endWall.transform;
                    }
                    else
                    {
                        hasPlaced = true;
                        isPlacing = false;

                        if (placeableBuilding.meshRend)
                        {
                            placeableBuilding.meshRend.material.SetColor("_Color", Color.white);
                        }

                        RM.food -= placeableBuilding.foodCost;
                        RM.wood -= placeableBuilding.woodCost;
                        RM.gold -= placeableBuilding.goldCost;
                        RM.stone -= placeableBuilding.stoneCost;
                        RM.iron -= placeableBuilding.ironCost;

                        if (UC.counter == 1)
                        {
                            if (UC.prevSelectedObject)
                            {
                                if (UC.prevSelectedObject.GetComponent<Player_Unit>())
                                {
                                    UC.prevSelectedObject.GetComponent<Player_Unit>().buildOrder.Add(placeableBuilding);
                                }
                                else if (OS.selectedUnits.Count > 0)
                                {
                                    foreach (Player_Unit PU in OS.selectedUnits)
                                    {
                                        PU.buildOrder.Add(placeableBuilding);
                                    }
                                }
                            }
                        }
                        else if (UC.counter > 1)
                        {
                            if (UC.selectedObject)
                            {
                                if (UC.selectedObject.GetComponent<Player_Unit>())
                                {
                                    UC.selectedObject.GetComponent<Player_Unit>().buildOrder.Add(placeableBuilding);
                                }
                                else if (OS.selectedUnits.Count > 0)
                                {
                                    foreach (Player_Unit PU in OS.selectedUnits)
                                    {
                                        PU.buildOrder.Add(placeableBuilding);
                                    }
                                }
                            }
                        }

                        dust.gameObject.transform.position = new Vector3(p.x, terr_height, p.z);
                        dust.Play();
                        placeableArea = false;
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                player.placeableBuildings.Remove(placeableBuilding);
                Destroy(currentBuilding.gameObject);
                hasPlaced = true;
                isPlacing = false;
                placeableArea = false;

                if (startWall)
                {
                    foreach (PlaceableBuilding segment in startWall.wallSegments)
                    {
                        Destroy(segment.gameObject);
                    }
                }

                if (builders.Count > 0)
                {
                    foreach(Player_Unit PU in builders)
                    {
                        foreach(PlaceableBuilding wall in walls)
                        {
                            RM.food -= wall.foodCost;
                            RM.wood -= wall.woodCost;
                            RM.gold -= wall.goldCost;
                            RM.stone -= wall.stoneCost;
                            RM.iron -= wall.ironCost;

                            PU.buildOrder.Add(wall);
                        }

                        PU.structureToBuild = PU.buildOrder[0];
                    }

                    builders.Clear();
                    walls.Clear();

                    startWall = null;
                    endWall = null;
                    startWallPoint = null;
                    endWallPoint = null;
                }
                else
                {
                    if (UC.counter == 1)
                    {
                        if (UC.prevSelectedObject.GetComponent<Player_Unit>())
                        {
                            UC.prevSelectedObject.GetComponent<Player_Unit>().dontMove = true;
                        }
                        else if (OS.selectedUnits.Count > 0)
                        {
                            foreach (Player_Unit PU in OS.selectedUnits)
                            {
                                PU.dontMove = true;
                            }
                        }
                    }
                    else if (UC.counter > 1)
                    {
                        if (UC.selectedObject.GetComponent<Player_Unit>())
                        {
                            UC.selectedObject.GetComponent<Player_Unit>().dontMove = true;
                        }
                        else if (OS.selectedUnits.Count > 0)
                        {
                            foreach (Player_Unit PU in OS.selectedUnits)
                            {
                                PU.dontMove = true;
                            }
                        }
                    }
                }
            }
        }
    }

    public void SetItem(GameObject b)    
    {
        hasPlaced = false;
        currentBuilding = Instantiate(b).transform;
        placeableBuilding = currentBuilding.GetComponent<PlaceableBuilding>();
    }
}
