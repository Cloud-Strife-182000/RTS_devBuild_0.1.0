using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyUnit : MonoBehaviour
{
    [Header("Selection")]

    public bool isSelected;

    [Header("References")]

    public enemyResourceManager ERM;
    public AI_StateMachine AI_St;
    public enemyBuildManager EBM;
    public Scanner scanner;
    public ShoreChecker rightLandCheck, leftLandCheck, backLandCheck;
    public NavMeshAgent agent;
    public ActivePlayer player;
    public Animator anim;
    public Animator horsemanAnim;

    public HealthBar HB;
    private Collider coll;

    private float deathTimer;
    public bool deathOverride, death;
    private bool walking, attacking, woodcutting, mining, hunting, farming;

    private bool unitCanvasSetActive;
    private bool incremented;
    public bool meshesEnabled = true;

    private float min_a, min_b;

    [Header("Input Statistics(Generic)")]

    public string objectName;
    public Sprite objectSprite;
    public string objectInfo1;
    public string objectInfo2;
    public GameObject selectionEffect;
    public int populationWeight;
    public int unitType; // 1 -> Villager, 2 -> Military, 3-> Cavalry, 4 -> Naval
    public int attackType; // 1 ->Melee, 2 ->Ranged, 3 -> Jet
    public int unitIndex;
    public float damage;
    public int damageType; //1 -> Blunt, 2 -> Pierce, 3 -> Siege, 4 -> Elemental
    public float bluntArmour, pierceArmour;
    public float foodCost, woodCost, goldCost, stoneCost, ironCost;
    public float minBuildingAttackRange, maxBuildingAttackRange;
    public float minUnitAttackRange, maxUnitAttackRange;
    public float deathTimeLimit;
    public GameObject[] swappables;
    public Renderer[] meshes;
    public GameObject nextUpgrade;

    [Header("Input Statistics(Economic)")]

    public float MaxHeldResource = 10;
    public float buildRate = 0.8f;
    public float buildStrength;
    public float gatheringRate = 1f;

    [Header("Input Statistics(Melee)")]

    public float attackCountdown = 0f;
    public float attackRate = 0.8f;

    [Header("Input Statistics(Ranged)")]

    public GameObject bulletPrefab;
    public float fireRate = 0.8f;
    public float fireCountdown = 0f;
    public Transform firePoint;
    public GameObject fireEffect;

    [Header("Input Statistics(Jet)")]

    public JetSpray jetSpray;
    public float jetRate = 2f;
    public float jetCountdown = 0f;

    [Header("Input Statistics(Naval")]

    public int holdingCapacity;
    public List<GameObject> unitsHeld;
    public float pickUpDistance;
    public bool shipCanAttack, shipCanTransport, shipCanGather;

    [Header("Auto-Update Statistics(Combat)")]

    public int player_ID;
    public float minAttackRange;
    public float maxAttackRange;
    public bool canAttack;
    public bool underAttack;
    public bool commandedToInvade;
    public Vector3 invadeTargetPosition;
    public bool garrisoned;

    [Header("Auto-Update Statistics(Gathering)")]

    public float minResource;
    public NodeManager.ResourceTypes minResourceName;
    public NodeManager[] resources;
    public enemyDropSite[] dropSites;
    public NodeManager lockedOnResource;
    public enemyDropSite nearestDropSite;
    public float resourceDistance;
    public float dropSiteDistance;
    public bool isGathering;
    public float heldResource;
    public NodeManager.ResourceTypes heldResourceType;
    public float resourceBounds;
    public float dropsiteBounds;
    public enemyStructure nearestGarrisonableES;
    public bool SiteTooFar = false;
    public bool foodGatherer, woodGatherer, stoneGatherer, goldGatherer, ironGatherer = false;

    [Header("Auto-Update Statistics(Building)")]

    public GameObject currentBuilding;
    public GameObject prevBuilding;
    public Vector3 buildPos;
    public GameObject newBuilding;
    public bool isBuilding = false;
    public float travelDistance;
    private float buildCountdown = 0f;
    public float terr_height;

    public bool canBuild = false;
    public bool needToRotate;
    public Collider[] hitColliders;

    public List<Vector3> possibleShoreBuildPoints, possibleBuildPoints;

    [Header("Enemy Statistics")]

    public GameObject nearestEnemy;
    public float enemyDistance;
    public Vector3 enemyDirection, attackerDirection;
    public GameObject attacker;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        AI_St = GetComponent<AI_StateMachine>();
    }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        player = FindObjectOfType<ActivePlayer>();
        HB = GetComponent<HealthBar>();
        coll = GetComponent<Collider>();
        meshes = GetComponentsInChildren<Renderer>();

        player_ID = AI_St.player_ID;

        scanner = Terrain.activeTerrain.GetComponent<Scanner>();

        if (selectionEffect)
        {
            selectionEffect.transform.localPosition = new Vector3(0, 0.1f, 0);
        }

        player.enemyUnits.Add(this);
        player.player_enemies.Add(gameObject);
        player.animal_enemies.Add(gameObject);
        player.NPC_enemies.Add(gameObject);
    }

    void Update()
    {
        death = HB.death;

        if (!death)
        {
            death = deathOverride;
        }

        if (!death)
        {
            if (isSelected)
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

            if(player.AI_enemies.Count > 0 && player.fogOfWar.gameObject.activeSelf)
            {
                if(Vector3.Distance(GetClosestEnemy(player.AI_enemies).transform.position, transform.position) > 20f)
                {
                    if (meshesEnabled)
                    {
                        coll.enabled = false;

                        if (anim)
                        {
                            anim.enabled = false;
                        }

                        foreach(Renderer mesh in meshes)
                        {
                            mesh.gameObject.SetActive(false);
                        }

                        meshesEnabled = false;
                    }
                }
                else
                {
                    if (!meshesEnabled)
                    {
                        coll.enabled = true;

                        foreach (Renderer mesh in meshes)
                        {
                            mesh.gameObject.SetActive(true);
                        }

                        if (anim)
                        {
                            anim.enabled = true;
                        }

                        meshesEnabled = true;
                    }
                }
            }
            

            WalkCheck();

            if(unitType == 1)
            {
                if (needToRotate == true)
                {
                    CheckAlignmentWithShoreLine(newBuilding, 1);
                }

                FleeCheck();
            }
            else if(unitType == 2 || unitType == 3)
            {
                if (!commandedToInvade)
                {
                    AttackCheck();
                }
                else
                {
                    agent.destination = invadeTargetPosition;

                    commandedToInvade = false;

                    if (AI_St.AIS.commandedToInvade)
                    {
                        AI_St.AIS.commandedToInvade = false;
                    }
                }
            }
            else if(unitType == 4 && shipCanAttack)
            {
                AttackCheck();
            }
        }
        else
        {
            AI_St.AIS.currentPopulation -= populationWeight;

            if (unitType == 3)
            {
                horsemanAnim.SetBool("attacking", false);
            }

            if (anim)
            {
                anim.SetBool("death", death);
            }

            if (agent.enabled)
            {
                agent.enabled = false;
            }

            if (unitType == 4)
            {
                if (deathTimer < deathTimeLimit / 2)
                {
                    transform.Rotate(new Vector3(0, 0, -120) * Time.deltaTime * 0.2f);
                }
                else
                {
                    transform.Translate(Vector3.down * Time.deltaTime * 3, Space.World);
                }
            }

            deathTimer += Time.deltaTime;

            if (deathTimer >= deathTimeLimit)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnEnable()
    {
        if (unitType == 1 || (unitType == 4 && shipCanGather))
        {
            StartCoroutine(GatherTick());
        }
    }

    GameObject GetClosestEnemy(List<GameObject> Enemies)
    {
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject targetEnemy in Enemies)
        {
            if (targetEnemy)
            {
                Vector3 direction = targetEnemy.transform.position - position;
                float distance = direction.sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = targetEnemy;
                }
            }
        }

        return closestEnemy;
    }

    public void Retaliate()
    {
        attacker = HB.attacker;

        float dist = Vector3.Distance(attacker.transform.position, transform.position);
        attackerDirection = attacker.transform.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(attackerDirection);

        if (dist < maxAttackRange && !death)
        {
            if (!attacker.GetComponent<HealthBar>().death)
            {
                agent.destination = attacker.transform.position;

                if (dist <= minAttackRange)
                {
                    walking = false;

                    agent.destination = transform.position;

                    transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 3);

                    attacking = true;

                    if (attackType == 1)
                    {
                        MeleeAttack(attacker);
                    }
                    else if (attackType == 2)
                    {
                        RangedAttack(attacker);
                    }
                }
                else
                {
                    attacking = false;
                }
            }
            else
            {
                attacking = false;
            }
        }
        else
        {
            HB.attacker = null;
            attacker = null;
        }
    }

    public void AttackCheck()
    {
        if (player.AI_enemies.Count <= 0)
        {
            enemyDistance = 100f;

            if (HB.attacker)
            {
                if (HB.attacker.GetComponent<PlaceableBuilding>())
                {
                    minAttackRange = minBuildingAttackRange;
                    maxAttackRange = maxBuildingAttackRange;
                }
                else
                {
                    minAttackRange = minUnitAttackRange;
                    maxAttackRange = maxUnitAttackRange;
                }

                Retaliate();
            }
            else
            {
                if (attacking)
                {
                    attacking = false;
                }
            }
        }
        else if (player.AI_enemies.Count > 0)
        {
            nearestEnemy = GetClosestEnemy(player.AI_enemies);

            if (nearestEnemy)
            {
                enemyDistance = Vector3.Distance(nearestEnemy.transform.position, transform.position);

                enemyDirection = nearestEnemy.transform.position - transform.position;
                Quaternion rot = Quaternion.LookRotation(enemyDirection);

                if (nearestEnemy.GetComponent<PlaceableBuilding>())
                {
                    minAttackRange = minBuildingAttackRange;
                    maxAttackRange = maxBuildingAttackRange;
                }
                else
                {
                    minAttackRange = minUnitAttackRange;
                    maxAttackRange = maxUnitAttackRange;
                }

                if (enemyDistance <= maxAttackRange && !death)
                {
                    if (!nearestEnemy.GetComponent<HealthBar>().death)
                    {
                        agent.destination = nearestEnemy.transform.position;

                        if (enemyDistance <= minAttackRange)
                        {
                            agent.destination = transform.position;

                            if (canAttack == true)
                            {
                                attacking = true;

                                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 3);

                                if (attackType == 1)
                                {
                                    MeleeAttack(nearestEnemy);
                                }
                                else if (attackType == 2)
                                {
                                    RangedAttack(nearestEnemy);
                                }
                            }
                            else
                            {
                                attacking = false;
                            }
                        }
                    }
                    else
                    {
                        attacking = false;
                    }
                }
                else if (HB.attacker)
                {
                    if (HB.attacker.GetComponent<PlaceableBuilding>())
                    {
                        minAttackRange = minBuildingAttackRange;
                        maxAttackRange = maxBuildingAttackRange;
                    }
                    else
                    {
                        minAttackRange = minUnitAttackRange;
                        maxAttackRange = maxUnitAttackRange;
                    }

                    Retaliate();
                }
                else
                {
                    attacking = false;
                }
            }
        }

        if (unitType == 3)
        {
            horsemanAnim.SetBool("attacking", attacking);
        }
        else
        {
            if (anim)
            {
                anim.SetBool("attacking", attacking);
            }
        }
    }

    public void WalkCheck()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            walking = false;
            canAttack = true;

        }
        else
        {
            walking = true;
            canAttack = false;
        }

        if (anim)
        {
            anim.SetBool("walking", walking);
        }
    }

    void Shoot(GameObject target)
    {
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        CatapultStoneThrow bullet = bulletGO.GetComponent<CatapultStoneThrow>();

        if (bullet != null)
        {
            bullet.Seek(target, damage, transform.gameObject, target.transform.position, damageType);
        }

    }

    public void MeleeAttack(GameObject target)
    {
        if (attackCountdown <= 0f)
        {
            attackCountdown = 1 / attackRate;

            target.GetComponent<HealthBar>().DecreaseHealth(damage, transform.gameObject, damageType);
        }

        attackCountdown -= Time.deltaTime;
    }

    public void JetAttack()
    {
        if (jetCountdown <= 0f)
        {
            if (jetSpray)
            {
                if (!jetSpray.gameObject.activeSelf)
                {
                    jetSpray.gameObject.SetActive(true);
                }
            }

            jetSpray.SprayDamage(damage, gameObject, damageType);

            jetCountdown = 1 / jetRate;
        }

        jetCountdown -= Time.deltaTime;
    }

    public void RangedAttack(GameObject target)
    {
        if (fireCountdown <= 0f)
        {
            if (fireEffect)
            {
                if (fireEffect.activeSelf)
                {
                    fireEffect.SetActive(false);
                }

                if (!fireEffect.activeSelf)
                {
                    fireEffect.SetActive(true);
                }
            }

            Shoot(target);

            fireCountdown = 1 / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    public void findMinimumResource()
    {
        min_a = ERM.food < ERM.wood ? ERM.food : ERM.wood;
        min_b = ERM.gold < ERM.stone ? ERM.gold : ERM.stone;
        minResource = min_a < min_b ? min_a : min_b;

        if (ERM.food == minResource)
        {
            minResourceName = NodeManager.ResourceTypes.Food;
        }
        else if (ERM.wood == minResource)
        {
            minResourceName = NodeManager.ResourceTypes.Wood;
        }
        else if (ERM.gold == minResource)
        {
            minResourceName = NodeManager.ResourceTypes.Gold;
        }
        else if (ERM.stone == minResource)
        {
            minResourceName = NodeManager.ResourceTypes.Stone;
        }
        else if (ERM.iron == minResource)
        {
            minResourceName = NodeManager.ResourceTypes.Iron;
        }
        else
        {
            minResourceName = NodeManager.ResourceTypes.Null;
        }
    }

    NodeManager GetClosestUnlockedResource(NodeManager[] Resources)
    {
        NodeManager closestResource = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (NodeManager targetResource in Resources)
        {
            Vector3 direction = targetResource.transform.position - position;
            float distance = direction.sqrMagnitude;
            if (distance < closestDistance && targetResource.lockedOnGatherers < targetResource.maxLockedOnGatherers)
            {
                if (!targetResource.building)
                {
                    closestDistance = distance;
                    closestResource = targetResource;
                }
                else if (targetResource.building)
                {
                    if (targetResource.building.playerID == AI_St.player_ID)
                    {
                        closestDistance = distance;
                        closestResource = targetResource;
                    }
                }
            }
        }
        return closestResource;
    }

    public NodeManager GetClosestUnlockedResourceOfType(NodeManager[] Resources, NodeManager.ResourceTypes resourceType)
    {
        NodeManager closestResource = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (NodeManager targetResource in Resources)
        {
            Vector3 direction = targetResource.transform.position - position;
            float distance = direction.sqrMagnitude;
            if (distance < closestDistance && targetResource.lockedOnGatherers < targetResource.maxLockedOnGatherers)
            {
                if (!targetResource.building && targetResource.resourceType == resourceType)
                {
                    closestDistance = distance;
                    closestResource = targetResource;
                }
                else if (targetResource.building && targetResource.resourceType == resourceType)
                {
                    if (targetResource.building.playerID == AI_St.player_ID)
                    {
                        closestDistance = distance;
                        closestResource = targetResource;
                    }
                }
            }
        }
        return closestResource;
    }

    private void ResourceCheck()
    {
        resourceDistance = Vector3.Distance(lockedOnResource.transform.position, transform.position);
        agent.destination = lockedOnResource.transform.position;
        heldResourceType = lockedOnResource.resourceType;
        GathererTypeCheck();
        resourceBounds = lockedOnResource.bounds.size.x / 2 + 1f;

        if (resourceDistance <= resourceBounds && heldResource < MaxHeldResource)
        {
            isGathering = true;

            if (heldResourceType == NodeManager.ResourceTypes.Wood)
            {
                woodcutting = true;

                if (swappables.Length > 0 && !meshesEnabled)
                {
                    swappables[0].SetActive(true);
                }
            }
            else if (heldResourceType == NodeManager.ResourceTypes.Food)
            {
                farming = true;
            }
            else if (heldResourceType == NodeManager.ResourceTypes.Gold || heldResourceType == NodeManager.ResourceTypes.Stone || heldResourceType == NodeManager.ResourceTypes.Iron)
            {
                mining = true;

                if (swappables.Length > 1 && !meshesEnabled)
                {
                    swappables[1].SetActive(true);
                }
            }

            if (!incremented)
            {
                lockedOnResource.gatherers++;
                incremented = true;
            }
        }
        else
        {
            isGathering = false;

            woodcutting = false;

            if (swappables.Length > 0 && !meshesEnabled)
            {
                swappables[0].SetActive(false);
            }

            mining = false;

            if (swappables.Length > 1 && !meshesEnabled)
            {
                swappables[1].SetActive(false);
            }

            farming = false;

            if (incremented)
            {
                lockedOnResource.gatherers--;
                lockedOnResource.availableResource -= heldResource;
                incremented = false;
            }
        }

        if (unitType == 1)
        {
            anim.SetBool("woodcutting", woodcutting);
            anim.SetBool("mining", mining);
            anim.SetBool("farming", farming);
        }
    }

    public void GoToNearestUnlockedResource()
    {
        if (!lockedOnResource)
        {
            resources = player.resources.ToArray();

            lockedOnResource = GetClosestUnlockedResource(resources);

            if(lockedOnResource.lockedOnGatherers < lockedOnResource.maxLockedOnGatherers)
            {
                lockedOnResource.lockedOnGatherers++;
            }

            ResourceCheck();
        }
        else
        {
            resourceDistance = Vector3.Distance(lockedOnResource.transform.position, transform.position);
            agent.destination = lockedOnResource.transform.position;
            heldResourceType = lockedOnResource.resourceType;
            resourceBounds = lockedOnResource.bounds.size.x / 2 + 1f;

            ResourceCheck();
        }
        
    }

    public void GoToNearestUnlockedResourceOfType(NodeManager.ResourceTypes resourceType)
    {
        if (!lockedOnResource)
        {
            resources = player.resources.ToArray();

            lockedOnResource = GetClosestUnlockedResourceOfType(resources, resourceType);

            if (lockedOnResource.lockedOnGatherers < lockedOnResource.maxLockedOnGatherers)
            {
                lockedOnResource.lockedOnGatherers++;
            }

            ResourceCheck();
        }
        else
        {
            ResourceCheck();
        }
    }

    private void GathererTypeCheck()
    {
        if (heldResourceType == NodeManager.ResourceTypes.Food)
        {
            foodGatherer = true;
            stoneGatherer = false;
            goldGatherer = false;
            woodGatherer = false;
            ironGatherer = false;
        }
        else if (heldResourceType == NodeManager.ResourceTypes.Wood)
        {
            woodGatherer = true;
            foodGatherer = false;
            stoneGatherer = false;
            goldGatherer = false;
            ironGatherer = false;
        }
        else if (heldResourceType == NodeManager.ResourceTypes.Gold)
        {
            goldGatherer = true;
            woodGatherer = false;
            foodGatherer = false;
            stoneGatherer = false;
            ironGatherer = false;
        }
        else if (heldResourceType == NodeManager.ResourceTypes.Stone)
        {
            stoneGatherer = true;
            goldGatherer = false;
            woodGatherer = false;
            foodGatherer = false;
            ironGatherer = false;
        }
        else if (heldResourceType == NodeManager.ResourceTypes.Iron)
        {
            ironGatherer = true;
            goldGatherer = false;
            woodGatherer = false;
            foodGatherer = false;
            stoneGatherer = false;
        }
    }

    public void GoToNearestDropSite()
    {
        dropSites = player.enemyDropSites.ToArray();

        nearestDropSite = GetClosestDropSite(dropSites, heldResourceType, transform.position);

        dropSiteDistance = Vector3.Distance(nearestDropSite.transform.position, transform.position);

        if (dropSiteDistance >= 65f)
        {
            SiteTooFar = true;
        }

        if (heldResource >= MaxHeldResource)
        {
            isGathering = false;
            agent.destination = nearestDropSite.transform.position;
        }

        dropsiteBounds = nearestDropSite.bounds.size.x / 2 + 1f;

        if (dropSiteDistance <= dropsiteBounds)
        {
            if (ERM.gold >= ERM.maxGold || ERM.wood >= ERM.maxWood || ERM.food >= ERM.maxFood || ERM.stone >= ERM.maxStone || ERM.iron >= ERM.maxIron)
            {
                agent.destination = transform.position;
            }

            else if (heldResourceType == NodeManager.ResourceTypes.Food)
            {
                ERM.food += heldResource;
                heldResource = 0;
            }

            else if (heldResourceType == NodeManager.ResourceTypes.Wood)
            {
                ERM.wood += heldResource;
                heldResource = 0;
            }

            else if (heldResourceType == NodeManager.ResourceTypes.Stone)
            {
                ERM.stone += heldResource;
                heldResource = 0;
            }

            else if (heldResourceType == NodeManager.ResourceTypes.Gold)
            {
                ERM.gold += heldResource;
                heldResource = 0;
            }

            else if (heldResourceType == NodeManager.ResourceTypes.Iron)
            {
                ERM.iron += heldResource;
                heldResource = 0;
            }
        }
    }

    public enemyDropSite GetClosestDropSite(enemyDropSite[] DropSites, NodeManager.ResourceTypes NRT, Vector3 position)
    {
        enemyDropSite closestDropSite = null;
        float closestDistance = Mathf.Infinity;

        foreach (enemyDropSite targetDropSite in DropSites)
        {
            if (targetDropSite)
            {
                Vector3 direction = targetDropSite.transform.position - position;
                float distance = direction.sqrMagnitude;
                if (distance < closestDistance)
                {
                    if (NRT == NodeManager.ResourceTypes.Gold)
                    {
                        if (targetDropSite.Gold == true && targetDropSite.playerID == AI_St.player_ID)
                        {
                            closestDistance = distance;
                            closestDropSite = targetDropSite;
                        }
                    }
                    if (NRT == NodeManager.ResourceTypes.Food)
                    {
                        if (targetDropSite.Food == true && targetDropSite.playerID == AI_St.player_ID)
                        {
                            closestDistance = distance;
                            closestDropSite = targetDropSite;
                        }
                    }
                    if (NRT == NodeManager.ResourceTypes.Wood)
                    {
                        if (targetDropSite.Wood == true && targetDropSite.playerID == AI_St.player_ID)
                        {
                            closestDistance = distance;
                            closestDropSite = targetDropSite;
                        }
                    }
                    if (NRT == NodeManager.ResourceTypes.Stone)
                    {
                        if (targetDropSite.Stone == true && targetDropSite.playerID == AI_St.player_ID)
                        {
                            closestDistance = distance;
                            closestDropSite = targetDropSite;
                        }
                    }
                    if (NRT == NodeManager.ResourceTypes.Iron)
                    {
                        if (targetDropSite.Iron == true && targetDropSite.playerID == AI_St.player_ID)
                        {
                            closestDistance = distance;
                            closestDropSite = targetDropSite;
                        }
                    }
                }
            }
        }
        return closestDropSite;
    }

    IEnumerator GatherTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (isGathering)
            {
                heldResource += gatheringRate;
            }

        }
    }

    public void InitializeBuildPos()
    {
        newBuilding = AI_St.AIS.buildOrder[0];

        buildPos = AI_St.AIS.buildPositions[0];
    }

    public void Build(GameObject building, Vector3 position)
    {
        buildPos = position;
        terr_height = Terrain.activeTerrain.SampleHeight(buildPos) + Terrain.activeTerrain.transform.position.y;
        buildPos.y = terr_height;

        canBuild = false;
        float mxDist = 10f;

        if (building.GetComponent<PlaceableBuilding>().placeableOnShore)
        {
            Vector3 origin = GetClosestPointFromPoint(scanner.eligibleShorePoints, transform.position);

            GetPossibleBuildPoints(origin, mxDist, 1, scanner.eligibleShorePoints, possibleShoreBuildPoints);

            while (possibleShoreBuildPoints.Count == 0)
            {
                mxDist += 10f;
                GetPossibleBuildPoints(origin, mxDist, 1, scanner.eligibleShorePoints, possibleShoreBuildPoints);
            }

            buildPos = GetClosestPointFromPoint(possibleShoreBuildPoints, transform.position);

            canBuild = true;
        }
        else
        {
            Vector3 origin = GetClosestPointFromPoint(scanner.eligibleTerrainPoints, buildPos);

            GetPossibleBuildPoints(origin, mxDist, 1, scanner.eligibleTerrainPoints, possibleBuildPoints);

            while (possibleBuildPoints.Count == 0)
            {
                mxDist += 10f;
                GetPossibleBuildPoints(origin, mxDist, 1, scanner.eligibleTerrainPoints, possibleBuildPoints);
            }

            buildPos = GetClosestPointFromPoint(possibleBuildPoints, buildPos);

            canBuild = true;
        }

        if (canBuild == true)
        {
            newBuilding = Instantiate(building, buildPos, building.transform.rotation);

            newBuilding.GetComponent<PlaceableBuilding>().playerID = player_ID;
            newBuilding.GetComponent<enemyStructure>().player_ID = player_ID;

            if (newBuilding.GetComponent<PlaceableBuilding>().placeableOnShore)
            {
                possibleShoreBuildPoints.Clear();

                needToRotate = true;
            }
            else
            {
                possibleBuildPoints.Clear();
            }

            BuildingCycle();

            isBuilding = true;
            agent.destination = newBuilding.transform.position;
        }

        anim.SetBool("building", isBuilding);
        swappables[2].SetActive(isBuilding);
    }

    public void GoToBuild(GameObject bldng)
    {
        agent.destination = bldng.transform.position;
        travelDistance = Vector3.Distance(bldng.transform.position, transform.position);

        if (travelDistance <= bldng.GetComponent<Collider>().bounds.size.x / 2 + 1f)
        {
            if (buildCountdown <= 0f)
            {
                buildCountdown = 1 / buildRate;

                bldng.GetComponent<HealthBar>().IncreaseHealth(buildStrength, transform.gameObject);
            }

            buildCountdown -= Time.deltaTime;
        }

        if (bldng.GetComponent<HealthBar>().currentHealth >= bldng.GetComponent<HealthBar>().maxHealth)
        {
            if(AI_St.AIS.gathererCountByType[0] <= 2)
            {
                if (bldng.GetComponent<NodeManager>())
                {
                    if(bldng.GetComponent<NodeManager>().resourceType == NodeManager.ResourceTypes.Food)
                    {
                        if (lockedOnResource)
                        {
                            lockedOnResource.lockedOnGatherers -= 1;

                            lockedOnResource = bldng.GetComponent<NodeManager>();
                        }
                        else
                        {
                            lockedOnResource = bldng.GetComponent<NodeManager>();
                        }
                    }
                }
            }

            isBuilding = false;
            canBuild = true;
        }

        anim.SetBool("building", isBuilding);
        swappables[2].SetActive(isBuilding);
    }

    public void BuildingCycle()
    {
        if (currentBuilding == null)
        {
            currentBuilding = newBuilding;
        }
        else
        {
            prevBuilding = currentBuilding;
            currentBuilding = newBuilding;

        }
    }

    public bool ColliderCheckOnPos(Vector3 posToCheck)
    {
        hitColliders = Physics.OverlapSphere(posToCheck, 4f);

        if (hitColliders.Length > 0)
        {
            foreach (Collider c in hitColliders)
            {
                if (c.GetComponent<PlaceableBuilding>() || c.GetComponent<NodeManager>())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Vector3 GetClosestPointFromPoint(List<Vector3> listofPoints, Vector3 posn)
    {
        float minDistance = Mathf.Infinity;
        Vector3 closestPoint = new Vector3(0, 0, 0);

        foreach (Vector3 Pos in listofPoints)
        {
            float distFromPt = Vector3.Distance(posn, Pos);

            if (distFromPt < minDistance)
            {
                minDistance = distFromPt;
                closestPoint = Pos;
            }
        }

        return closestPoint;
    }

    public void CheckAlignmentWithShoreLine(GameObject building, float speed)
    {
        ShoreChecker[] shoreCheckers = building.GetComponentsInChildren<ShoreChecker>();

        foreach (ShoreChecker shCh in shoreCheckers)
        {
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
        }

        if (rightLandCheck.terrainColliders.Count == 0)
        {
            Vector3 dir = leftLandCheck.transform.position - building.transform.position;
            Quaternion rot = Quaternion.LookRotation(dir);
            building.transform.rotation = Quaternion.Slerp(building.transform.rotation, rot, Time.deltaTime * speed);
        }

        if (leftLandCheck.terrainColliders.Count == 0)
        {
            Vector3 dir = rightLandCheck.transform.position - building.transform.position;
            Quaternion rot = Quaternion.LookRotation(dir);
            building.transform.rotation = Quaternion.Slerp(building.transform.rotation, rot, Time.deltaTime * speed);
        }

        if (backLandCheck.terrainColliders.Count == 0)
        {
            if (rightLandCheck.terrainColliders.Count == 0)
            {
                Vector3 dir = leftLandCheck.transform.position - building.transform.position;
                Quaternion rot = Quaternion.LookRotation(dir);
                building.transform.rotation = Quaternion.Slerp(building.transform.rotation, rot, Time.deltaTime * speed);
            }

            if (leftLandCheck.terrainColliders.Count == 0)
            {
                Vector3 dir = rightLandCheck.transform.position - building.transform.position;
                Quaternion rot = Quaternion.LookRotation(dir);
                building.transform.rotation = Quaternion.Slerp(building.transform.rotation, rot, Time.deltaTime * speed);
            }
        }

        if (backLandCheck.terrainColliders.Count > 0 && rightLandCheck.terrainColliders.Count > 0 && leftLandCheck.terrainColliders.Count > 0)
        {
            needToRotate = false;
        }
    }

    public void GetPossibleBuildPoints(Vector3 origin, float maxDistance, float minDistance, List<Vector3> pointsList, List<Vector3> newPointsList)
    {
        foreach (Vector3 Pos in pointsList)
        {
            float dist = Vector3.Distance(origin, Pos);

            if (dist <= maxDistance && dist > minDistance)
            {
                buildPos = Pos;

                bool collision = ColliderCheckOnPos(buildPos);

                if (!collision)
                {
                    newPointsList.Add(Pos);
                }
            }
        }
    }

    public void FleeCheck()
    {
        if (HB.attacker)
        {
            attacker = HB.attacker;

            float dist = Vector3.Distance(attacker.transform.position, transform.position);

            if(dist <= 10f)
            {
                if (!underAttack)
                {
                    underAttack = true;
                }

                nearestGarrisonableES = GetClosestEnemyStructure(player.e_structs, true);

                agent.destination = nearestGarrisonableES.transform.position;

            }
            else
            {
                HB.attacker = null;

                if (underAttack)
                {
                    underAttack = false;
                }
            }
        }
        else
        {
            if (underAttack)
            {
                underAttack = false;
            }
        }
    }

    enemyStructure GetClosestEnemyStructure(List<enemyStructure> e_structures, bool garrisonCheck)
    {
        enemyStructure closestES = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (enemyStructure targetStructure in e_structures)
        {
            if (targetStructure)
            {
                Vector3 direction = targetStructure.transform.position - position;
                float distance = direction.sqrMagnitude;

                if (garrisonCheck)
                {
                    if (distance < closestDistance && targetStructure.canGarrison)
                    {
                        closestDistance = distance;
                        closestES = targetStructure;
                    }
                }
                else
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestES = targetStructure;
                    }
                }
            }
        }

        return closestES;
    }

    public void AccountForEnemyResearch()
    {
        for (int i = 1; i < AI_St.AIS.techs.Count; i++)
        {
            if (AI_St.AIS.researched[i])
            {
                if (AI_St.AIS.techs[i])
                {
                    if (AI_St.AIS.techs[i].forUnit)
                    {
                        EnemyUnitImprovements(AI_St.AIS.techs[i]);
                    }
                }
            }
        }
    }

    private void EnemyUnitImprovements(Attributes attr)
    {
        if (unitType == attr.unitType)
        {
            if (attr.attackType == 0)
            {
                HB.maxHealth += attr.healthModifier;
                HB.currentHealth += attr.healthModifier;
                MaxHeldResource += attr.gatheringCapacityModifier;
                damage += attr.damageModifier;
                gatheringRate += attr.gatheringRateModifier;
                buildStrength += attr.buildingStrengthModifier;
            }
            else if (attackType == attr.attackType)
            {
                HB.maxHealth += attr.healthModifier;
                HB.currentHealth += attr.healthModifier;
                MaxHeldResource += attr.gatheringCapacityModifier;
                damage += attr.damageModifier;
                gatheringRate += attr.gatheringRateModifier;
                buildStrength += attr.buildingStrengthModifier;
            }
        }
    }

    public void Upgrade()
    {
        if (nextUpgrade)
        {
            Instantiate(nextUpgrade, transform.position, transform.rotation);
        }
    }

    private void OnMouseEnter()
    {
        player.cursor.SetEnemy();
    }

    private void OnMouseExit()
    {
        player.cursor.SetMouse();
    }
}
