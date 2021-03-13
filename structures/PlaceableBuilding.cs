using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricFogAndMist;

public class PlaceableBuilding : MonoBehaviour
{
    public GameObject building;

    public string objectName;
    public Sprite objectSprite;
    public string objectInfo1;
    public string objectInfo2;

    public bool isBuilt, placeableOnShore, isWall, isWallSegment;
    public float fogClearRadius, fogClearRadiusBeforeBuild;

    public float lengthOfSegment = 5f;

    public GameObject wallEndPoint;

    public PlaceableBuilding wallSegment;
    public List<PlaceableBuilding> wallSegments;

    public int foodCost;
    public int woodCost;
    public int stoneCost;
    public int goldCost;
    public int ironCost;

    public new List<Collider> collider = new List<Collider>();
    public int playerID;
    public Bounds bounds;
    public Renderer meshRend;
    public bool traversingOverWater, onShoreLine, terrainPresent, waterPresent;
    public float waterHeight, shoreHeight, minHeight, maxHeight;
    private List<RaycastHit> waterChecks = new List<RaycastHit>();
    private List<float> heights = new List<float>();
    private ShoreChecker[] shoreCheckers;
    public ShoreChecker shoreCheck, landCheck, rightLandCheck, leftLandCheck, backLandCheck, alignmentAreaCheck;
    public Quaternion initialRotation;

    public List<GameObject> constructionPrefabs;
    public List<GameObject> buildingExtras;
    public List<float> constructionDeadlines;
    public int constructionStages, currentConstructionStage;

    public HealthBar HB;
    private ActivePlayer player;
    private BuildingPlacement BP;
    public NodeManager nm;
    public Drops drops;
    public enemyDropSite EDS;
    public Player_Structure PS;
    public enemyStructure ES;
    private bool NMDisabled, DropsDisabled, enemyDropsDisabled;
    private Vector3[] temp;
    public bool deactivated, fogAreaCleared;

    public VolumetricFog fogOfWar, secondaryFogOfWar, minimapFogOfWar;

    void Awake()
    {
        if (GetComponent<Drops>())
        {
            drops = GetComponent<Drops>();

            if (!isBuilt)
            {
                drops.enabled = false;
                DropsDisabled = true;
            }
        }

        if (GetComponent<NodeManager>())
        {
            nm = GetComponent<NodeManager>();

            if (!isBuilt)
            {
                nm.enabled = false;
                NMDisabled = true;
            }
        }

        if (GetComponent<enemyDropSite>())
        {
            EDS = GetComponent<enemyDropSite>();

            if (!isBuilt)
            {
                EDS.enabled = false;
                enemyDropsDisabled = true;
            }
        }
    }

    void Start()
    {
        if (playerID == 0)
        {
            PS = GetComponent<Player_Structure>();
        }
        else
        {
            ES = GetComponent<enemyStructure>();
        }

        if (EDS)
        {
            EDS.playerID = playerID;
        }
        
        player = FindObjectOfType<ActivePlayer>();
        HB = GetComponent<HealthBar>();
        BP = player.GetComponentInChildren<BuildingPlacement>();

        player.placeableBuildings.Add(this);

        if (PS)
        {
            fogOfWar = player.fogOfWar;
            secondaryFogOfWar = player.secondaryFogOfWar;
            minimapFogOfWar = player.minimapFogOfWar;

            player.AI_enemies.Add(gameObject);
            player.NPC_enemies.Add(gameObject);

            AccountForPlayerResearch();
        }
        else if (ES)
        {
            player.player_enemies.Add(gameObject);
            player.NPC_enemies.Add(gameObject);

            AccountForEnemyResearch();
        }

        initialRotation = transform.rotation;
        bounds = GetComponent<Collider>().bounds;

        constructionStages = constructionPrefabs.Count;
        currentConstructionStage = 0;
        
        if(constructionStages > 0 && !isBuilt)
        {
            meshRend = constructionPrefabs[constructionStages - 1].GetComponentInChildren<MeshRenderer>();

            constructionDeadlines.Add(1);

            if (constructionStages > 2)
            {
                for (int i = 1; i < constructionStages - 1; i++)
                {
                    constructionDeadlines.Add((HB.maxHealth / (constructionStages - 1)) * i);
                }
            }

            constructionDeadlines.Add(HB.maxHealth);

            if (!PS)
            {
                foreach (GameObject BE in buildingExtras)
                {
                    BE.SetActive(false);
                }

                foreach (GameObject constrPrefab in constructionPrefabs)
                {
                    constrPrefab.SetActive(false);
                }

                constructionPrefabs[0].SetActive(true);
            }
            
        }

        shoreCheckers = GetComponentsInChildren<ShoreChecker>();

        foreach (ShoreChecker shCh in shoreCheckers)
        {
            if (shCh.frontWater)
            {
                shoreCheck = shCh;
            }

            if (shCh.frontLand)
            {
                landCheck = shCh;
            }

            if (shCh.rightLand)
            {
                rightLandCheck = shCh;
            }

            if (shCh.leftLand)
            {
                leftLandCheck = shCh;
            }

            if (shCh.backLand)
            {
                backLandCheck = shCh;
            }

            if (shCh.alignmentArea)
            {
                alignmentAreaCheck = shCh;
            }
        }
    }

    void Update()
    {
        if (!isBuilt)
        {
            if (PS)
            {
                if (BP.hasPlaced)
                {
                    if (fogOfWar)
                    {
                        if (fogOfWar.gameObject.activeSelf)
                        {
                            fogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadiusBeforeBuild, 0, 0, 0.85f);
                            minimapFogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadiusBeforeBuild, 0, 0, 0.85f);
                            secondaryFogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadiusBeforeBuild, 0, 0, 0.85f);
                        }
                    }

                    if (!deactivated)
                    {
                        foreach (GameObject BE in buildingExtras)
                        {
                            BE.SetActive(false);
                        }

                        foreach (GameObject constrPrefab in constructionPrefabs)
                        {
                            constrPrefab.SetActive(false);
                        }

                        constructionPrefabs[0].SetActive(true);

                        
                        
                        deactivated = true;
                    }

                    IterateThroughBuildingStages();
                }
            }
            else
            {
                IterateThroughBuildingStages();
            }
        }
        else
        {
            if (fogOfWar && PS)
            {
                if (!fogAreaCleared && PS.canClearFog)
                {
                    if (fogOfWar.gameObject.activeSelf)
                    {
                        fogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadius, 0, 0, 0.85f);
                        minimapFogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadius, 0, 0, 0.85f);
                        secondaryFogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadius, 0, 0, 0.85f);
                    }

                    fogAreaCleared = true;
                }                
            }

            if (NMDisabled)
            {
                nm.enabled = true;

                NMDisabled = false;
            }

            if (DropsDisabled)
            {
                drops.enabled = true;

                DropsDisabled = false;
            }

            if (enemyDropsDisabled)
            {
                EDS.enabled = true;

                enemyDropsDisabled = false;
            }
        }
    }

    private void OnEnable()
    {
        if (PS)
        {
            if (secondaryFogOfWar.gameObject.activeSelf && PS.canClearFog)
            {
                StartCoroutine(ClearSecondaryFogAroundBuilding());
            }
        }
    }

    private void IterateThroughBuildingStages()
    {
        if (constructionStages > 0)
        {
            if (currentConstructionStage < constructionStages - 1)
            {
                if (HB.currentHealth >= constructionDeadlines[currentConstructionStage + 1])
                {
                    constructionPrefabs[currentConstructionStage].SetActive(false);
                    currentConstructionStage++;
                    constructionPrefabs[currentConstructionStage].SetActive(true);
                }
            }

            if (currentConstructionStage == constructionStages - 1)
            {
                foreach (GameObject BE in buildingExtras)
                {
                    BE.SetActive(true);
                }

                meshRend.material.SetColor("_Color", Color.white);

                isBuilt = true;
            }
        }
    }

    private void OnTriggerEnter(Collider c)
    {
        if (c.GetComponent<PlaceableBuilding>() || c.GetComponent<NodeManager>())
        {
            collider.Add(c);
        }

    }

    private void OnTriggerExit(Collider c)
    {
        if (c.GetComponent<PlaceableBuilding>() || c.GetComponent<NodeManager>())
        {
            collider.Remove(c);
        }
    }

    public bool CheckArea(float x, float y, float range, float height, Vector3 rayThrowPoint)
    {
        maxHeight = 0f;
        minHeight = 1000f;

        float xmin = x - range;
        float ymin = y - range;
        float xmax = x + range;
        float ymax = y + range;

        for (float i = xmin; i <= xmax; i++)
        {
            for (float j = ymin; j <= ymax; j++)
            {
                Vector3 temp = new Vector3(i, height, j);

                Vector3 direction = temp - rayThrowPoint;

                RaycastHit hit;
                if (Physics.Raycast(rayThrowPoint, direction, out hit, Mathf.Infinity))
                {
                    if (hit.collider.GetComponent<Terrain>())
                    {
                        float hitPointHeight = Terrain.activeTerrain.SampleHeight(hit.point) + Terrain.activeTerrain.transform.position.y;

                        if (hitPointHeight > maxHeight)
                        {
                            maxHeight = hitPointHeight;
                        }

                        if (hitPointHeight < minHeight)
                        {
                            minHeight = hitPointHeight;
                        }

                        if (!placeableOnShore)
                        {
                            if ((Vector3.up - hit.normal).magnitude > 0.2)
                            {
                                if (meshRend)
                                {
                                    meshRend.material.SetColor("_Color", Color.red);
                                }

                                return false;
                            }
                        }
                    }
                }
            }
        }

        if (meshRend)
        {
            meshRend.material.SetColor("_Color", Color.white);
        }

        return true;

    }

    public bool WaterCheck(float x, float y, float range, float height, Vector3 rayThrowPoint, PlaceableBuilding building, float fogAlpha)
    {
        if(fogAlpha > 0.75)
        {
            meshRend.material.SetColor("_Color", Color.red);
            return false;
        }

        terrainPresent = false;
        waterPresent = false;
        onShoreLine = false;

        waterChecks.Clear();
        heights.Clear();

        float xmin = x - range;
        float ymin = y - range;
        float xmax = x + range;
        float ymax = y + range;

        for (float i = xmin; i <= xmax; i++)
        {
            for (float j = ymin; j <= ymax; j++)
            {
                Vector3 temp = new Vector3(i, height, j);

                Vector3 direction = temp - rayThrowPoint;

                RaycastHit hit;
                if (Physics.Raycast(rayThrowPoint, direction, out hit, Mathf.Infinity))
                {
                    waterChecks.Add(hit);
                }
            }
        }

        foreach (RaycastHit hit in waterChecks) {

            if (hit.collider.GetComponent<Terrain>())
            {
                terrainPresent = true;

                heights.Add(Terrain.activeTerrain.SampleHeight(hit.collider.transform.position) + Terrain.activeTerrain.transform.position.y);
            }

            if (hit.collider.GetComponent<RTSWater>())
            {
                waterPresent = true;

                waterHeight = hit.collider.transform.position.y;
            }
        }

        if (terrainPresent && !waterPresent && meshRend)
        {
            traversingOverWater = false;

            if (building.placeableOnShore)
            {
                transform.rotation = initialRotation;
                meshRend.material.SetColor("_Color", Color.red);
                return false;
            }

            if (CheckArea(x, y, range, height, rayThrowPoint))
            {
                if (IsLegalPosition())
                {
                    meshRend.material.SetColor("_Color", Color.white);
                    return true;
                }
            }
            else
            {
                meshRend.material.SetColor("_Color", Color.red);
                return false;
            }

        }
        else if (terrainPresent && waterPresent && meshRend)
        {
            traversingOverWater = false;
            onShoreLine = true;

            shoreHeight = Mathf.Max(heights.ToArray());

            if (building.placeableOnShore)
            {
                if(alignmentAreaCheck.terrainColliders.Count > 0)
                {
                    CheckArea(x, y, range, height, rayThrowPoint);

                    if (maxHeight - minHeight <= 1f)
                    {
                        CheckAlignmentWithShoreLine();
                    }
                }

                if (landCheck.terrainColliders.Count > 0)
                {
                    if (shoreCheck.waterColliders.Count > 0 && shoreCheck.terrainColliders.Count == 0)
                    {
                        CheckArea(x, y, range, height, rayThrowPoint);

                        if (maxHeight - minHeight <= 1f)
                        {
                            meshRend.material.SetColor("_Color", Color.white);
                            return true;
                        }
                    }
                }

                meshRend.material.SetColor("_Color", Color.red);
                return false;
            }

            meshRend.material.SetColor("_Color", Color.red);
            return false;
        }
        else if (!terrainPresent && waterPresent && meshRend)
        {
            traversingOverWater = true;

            meshRend.material.SetColor("_Color", Color.red);
            return false;
        }

        meshRend.material.SetColor("_Color", Color.red);
        return false;

    }

    public void CheckAlignmentWithShoreLine()
    {
        if(rightLandCheck.terrainColliders.Count == 0)
        {
            Vector3 dir = leftLandCheck.transform.position - building.transform.position;
            Quaternion rot = Quaternion.LookRotation(dir);
            building.transform.rotation = Quaternion.Slerp(building.transform.rotation, rot, Time.deltaTime * 2);
        }

        if(leftLandCheck.terrainColliders.Count == 0)
        {
            Vector3 dir = rightLandCheck.transform.position - building.transform.position;
            Quaternion rot = Quaternion.LookRotation(dir);
            building.transform.rotation = Quaternion.Slerp(building.transform.rotation, rot, Time.deltaTime * 2);
        }

        if(backLandCheck.terrainColliders.Count == 0)
        {
            if (rightLandCheck.terrainColliders.Count == 0)
            {
                Vector3 dir = leftLandCheck.transform.position - building.transform.position;
                Quaternion rot = Quaternion.LookRotation(dir);
                building.transform.rotation = Quaternion.Slerp(building.transform.rotation, rot, Time.deltaTime * 2);
            }

            if (leftLandCheck.terrainColliders.Count == 0)
            {
                Vector3 dir = rightLandCheck.transform.position - building.transform.position;
                Quaternion rot = Quaternion.LookRotation(dir);
                building.transform.rotation = Quaternion.Slerp(building.transform.rotation, rot, Time.deltaTime * 2);
            }
        }
    }

    bool IsLegalPosition()
    {
        if (collider.Count > 0)
        {
            return false;
        }

        return true;
        
    }

    public void AccountForPlayerResearch()
    {
        for(int i=1; i < player.techs.Count; i++)
        {
            if (player.researched[i])
            {
                if (player.techs[i])
                {
                    if (player.techs[i].forBuilding)
                    {
                        PlayerStructureImprovements(player.techs[i]);
                    }
                }
            }
        }
    }

    public void AccountForEnemyResearch()
    {
        ES.FindERMWithSamePlayerID();

        ES.AIS = ES.ERM.GetComponent<AI_Supervisor>();

        ES.AIS.populationCapacity += ES.populationContribution;

        for (int i = 1; i < ES.AIS.techs.Count; i++)
        {
            if (ES.AIS.researched[i])
            {
                if (ES.AIS.techs[i])
                {
                    if (ES.AIS.techs[i].forBuilding)
                    {
                        EnemyStructureImprovements(ES.AIS.techs[i]);
                    }
                }
            }
        }
    }

    private void PlayerStructureImprovements(Attributes attr)
    {
        if (attr.resourceToTarget != NodeManager.ResourceTypes.Null)
        {
            if (drops)
            {
                if (attr.resourceToTarget == NodeManager.ResourceTypes.Food)
                {
                    if (drops.Food)
                    {
                        CommonPlayerImprovements(attr);
                    }
                }
                else if (attr.resourceToTarget == NodeManager.ResourceTypes.Wood)
                {
                    if (drops.Wood)
                    {
                        CommonPlayerImprovements(attr);
                    }
                }
                else if (attr.resourceToTarget == NodeManager.ResourceTypes.Gold)
                {
                    if (drops.Gold)
                    {
                        CommonPlayerImprovements(attr);
                    }
                }
                else if (attr.resourceToTarget == NodeManager.ResourceTypes.Stone)
                {
                    if (drops.Stone)
                    {
                        CommonPlayerImprovements(attr);
                    }
                }
                else if (attr.resourceToTarget == NodeManager.ResourceTypes.Iron)
                {
                    if (drops.Iron)
                    {
                        CommonPlayerImprovements(attr);
                    }
                }
            }
            else if (nm)
            {
                if (nm.resourceType == attr.resourceToTarget)
                {
                    nm.availableResource += attr.resourceAmountModifier;
                    CommonPlayerImprovements(attr);
                }
            }
        }
        else
        {
            CommonPlayerImprovements(attr);
        }
    }

    private void EnemyStructureImprovements(Attributes attr)
    {
        if(attr.resourceToTarget != NodeManager.ResourceTypes.Null)
        {
            if (EDS)
            {
                if (attr.resourceToTarget == NodeManager.ResourceTypes.Food)
                {
                    if (EDS.Food)
                    {
                        CommonEnemyImprovements(attr);
                    }
                }
                else if (attr.resourceToTarget == NodeManager.ResourceTypes.Wood)
                {
                    if (EDS.Wood)
                    {
                        CommonEnemyImprovements(attr);
                    }
                }
                else if (attr.resourceToTarget == NodeManager.ResourceTypes.Gold)
                {
                    if (EDS.Gold)
                    {
                        CommonEnemyImprovements(attr);
                    }
                }
                else if (attr.resourceToTarget == NodeManager.ResourceTypes.Stone)
                {
                    if (EDS.Stone)
                    {
                        CommonEnemyImprovements(attr);
                    }
                }
                else if (attr.resourceToTarget == NodeManager.ResourceTypes.Iron)
                {
                    if (EDS.Iron)
                    {
                        CommonEnemyImprovements(attr);
                    }
                }
            }
            else if (nm)
            {
                if (nm.resourceType == attr.resourceToTarget)
                {
                    nm.availableResource += attr.resourceAmountModifier;
                    CommonEnemyImprovements(attr);
                }
            }
        }
        else
        {
            CommonEnemyImprovements(attr);
        }
    }

    private void CommonPlayerImprovements(Attributes attr)
    {
        HB.maxHealth += attr.healthModifier;
        HB.currentHealth += attr.healthModifier;
        PS.standardFortification += attr.standardFortificationModifier;
        PS.siegeFortification += attr.siegeFortificationModifier;
    }

    private void CommonEnemyImprovements(Attributes attr)
    {
        HB.maxHealth += attr.healthModifier;
        HB.currentHealth += attr.healthModifier;
        ES.standardFortification += attr.standardFortificationModifier;
        ES.siegeFortification += attr.siegeFortificationModifier;
    }

    IEnumerator ClearSecondaryFogAroundBuilding()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            secondaryFogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadius, 0, 0, 0.85f);
        }
    }
}

