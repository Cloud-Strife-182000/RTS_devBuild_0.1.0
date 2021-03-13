using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VolumetricFogAndMist;
using UnityEngine.EventSystems;

public class Player_Unit : MonoBehaviour
{
    [Header("Selection")]

    public bool isSelected;
    public bool isSelectedByBox;

    [Header("References")]

    public NavMeshAgent agent;
    public VolumetricFog fogOfWar, secondaryFogOfWar, minimapFogOfWar;
    public ActivePlayer player;
    public HealthBar HB;
    public Animator horsemanAnim;
    public Animator anim;
    public UnitCanvas UC;

    private BuildingManager BM;
    private HandleCursor cursor;

    private bool walking, attacking, woodcutting, mining, hunting, farming;
    private bool unitCanvasSetActive;
    private bool lookForNextResource;

    private float deathTimer;

    private float redirectDistanceLimit = 1f;

    private ResourceManager RM;
    private Drops nearestDrop;
    private PlaceableBuilding collidedBuilding;
    private NodeManager[] resources;
    private bool incremented;
    private bool AttackCursorActive, AnchorCursorActive;
    private bool deathOverride;
    private bool fogCleared;
    private Player_Unit clickedUnit;

    private LayerMask unitMask = 1 << 8;

    [Header("Input Statistics(Generic)")]

    public string objectName;
    public Sprite objectSprite;
    public string objectInfo1;
    public string objectInfo2;
    public GameObject selectionEffect;
    public GameObject toggleWeapon;
    public bool canClearFog = true;
    public int populationWeight;
    public int unitType; // 1-> Villager, 2-> Military, 3-> Cavalry, 4-> Naval
    public int attackType; // 1 -> Melee, 2 -> Ranged, 3 -> Jet
    public int unitIndex;
    public float damage;
    public int damageType; //1 -> Blunt, 2 -> Pierce, 3 -> Siege, 4 -> Elemental
    public float bluntArmour, pierceArmour;
    public float fogClearRadius;
    public float foodCost, woodCost, goldCost, stoneCost, ironCost;
    public float minBuildingAttackRange, maxBuildingAttackRange;
    public float minUnitAttackRange, maxUnitAttackRange;
    public float manualAttackRange, friendDetectionRange, deathTimeLimit;
    public bool CanRetaliate;
    public GameObject[] swappables;
    public GameObject nextUpgrade;
    public float spawnIncrement, spawnRate;

    [Header("Input Statistics(Economic)")]

    public float buildStrength;
    public float buildCountdown = 0f;
    public float buildRate = 0.8f;
    public float resourceCapacity, nodeSearchDistance;
    public float huntAttackRange;
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

    public float minAttackRange;
    public float maxAttackRange;
    public bool canAttack;
    public bool redirect;
    public Vector3 temp_destination;
    public GameObject targetClicked;

    [Header("Auto-Update Statistics(Economic)")]

    public float heldResource;
    public bool isGathering, isBuilding, assignedToBuild, canDepositResources;
    public PlaceableBuilding structureToBuild;
    public Vector3 buildingDirection;
    public float travelDistance, resourceDistance, dropSiteDistance;
    public NodeManager targetNode, nextTargetNode;
    public NodeManager.ResourceTypes heldResourceType;
    public Player_Structure nearestPS, nearestGarrisonablePS;
    public Drops[] drops;
    public List<PlaceableBuilding> buildOrder;
    public bool garrisoned;
    public bool dontMove;

    [Header("Enemy Statistics")]

    public bool death;
    public GameObject nearestEnemy;
    public bool animalTargeted, destroyableTargeted, enemyTargeted, NPCTargeted;
    public float enemyDistance;
    public Vector3 enemyDirection, attackerDirection;
    public GameObject attacker;

    void Awake()
    {
        player = FindObjectOfType<ActivePlayer>();
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<Animator>())
        {
            anim = GetComponent<Animator>();
        }

        player.currentPopulation += populationWeight;

        RM = player.GetComponent<ResourceManager>();
        HB = GetComponent<HealthBar>();
        BM = player.GetComponentInChildren<BuildingManager>();
        cursor = player.GetComponentInChildren<HandleCursor>();

        fogOfWar = player.fogOfWar;
        secondaryFogOfWar = player.secondaryFogOfWar;
        minimapFogOfWar = player.minimapFogOfWar;

        if (GetComponentInChildren<UnitCanvas>())
        {
            UC = GetComponentInChildren<UnitCanvas>();
            UC.player = player;

            UC.gameObject.SetActive(false);
        }

        player.player_units.Add(this);
        player.AI_enemies.Add(gameObject);
        player.animal_enemies.Add(gameObject);
        player.NPC_enemies.Add(gameObject);

        if (selectionEffect)
        {
            selectionEffect.transform.localPosition = new Vector3(0, 0.1f, 0);
        }

        ToggleRetaliation();

        AccountForPlayerResearch();
    }

    // Update is called once per frame
    void Update()
    {
        if (!fogCleared && canClearFog)
        {
            fogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadius, 0, 0, 0.85f);
            secondaryFogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadius, 0, 0, 0.85f);
            minimapFogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadius, 0, 0, 0.85f);

            fogCleared = true;
        }

        UnitLoop();
    }

    public void UnitLoop()
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
                if (UC)
                {
                    if (!unitCanvasSetActive && !player.subtitles.activeSelf)
                    {
                        UC.gameObject.SetActive(true);

                        unitCanvasSetActive = true;
                    }

                    if (unitCanvasSetActive && player.subtitles.activeSelf)
                    {
                        UC.gameObject.SetActive(false);

                        unitCanvasSetActive = false;
                    }
                }
            }
            else
            {
                if (UC)
                {
                    if (unitCanvasSetActive)
                    {
                        UC.gameObject.SetActive(false);

                        unitCanvasSetActive = false;
                    }
                }
            }

            if (isSelected || isSelectedByBox)
            {
                if (selectionEffect)
                {
                    if (!selectionEffect.activeSelf)
                    {
                        selectionEffect.SetActive(true);
                    }
                }

                if (AttackCursorActive || AnchorCursorActive)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (EventSystem.current.IsPointerOverGameObject())
                        {
                            return;
                        }

                        cursor.SetMouse();

                        Rightclick();

                        player.UC.cursorIsBusy = false;

                        AttackCursorActive = false;
                        AnchorCursorActive = false;
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }

                    if (AttackCursorActive)
                    {
                        cursor.SetMouse();
                        player.UC.cursorIsBusy = false;
                        AttackCursorActive = false;
                    }
                    else if (AnchorCursorActive)
                    {
                        cursor.SetMouse();
                        player.UC.cursorIsBusy = false;
                        AnchorCursorActive = false;
                    }

                    if (!dontMove)
                    {
                        Rightclick();
                    }
                    else
                    {
                        dontMove = false;
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

            WalkCheck();

            if (unitType == 1)
            {
                ResourceCheck();

                if (buildOrder.Count == 1)
                {
                    if (buildOrder[0])
                    {
                        structureToBuild = buildOrder[0];
                    }
                }

                if (buildOrder.Count > 0)
                {
                    if (!buildOrder[0])
                    {
                        buildOrder.Remove(buildOrder[0]);
                    }
                }

                if (structureToBuild)
                {
                    agent.destination = structureToBuild.transform.position;

                    assignedToBuild = true;

                    BuildCheck(structureToBuild);

                    if (structureToBuild)
                    {
                        buildingDirection = structureToBuild.transform.position - transform.position;
                    }
                }

                if (isBuilding && redirect == false)
                {
                    Quaternion rot = Quaternion.LookRotation(buildingDirection);
                    agent.destination = transform.position;

                    transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 3);

                    if (assignedToBuild)
                    {
                        if (structureToBuild)
                        {
                            Build(structureToBuild.gameObject);
                        }
                        else
                        {
                            assignedToBuild = false;
                            isBuilding = false;
                            anim.SetBool("building", isBuilding);
                        }
                    }
                    else if (targetClicked)
                    {
                        Build(targetClicked);
                    }
                }

                VillagerAttackCheck();
            }

            if (unitType == 2 || unitType == 3)
            {
                MilitaryAttackCheck();
            }

            if (unitType == 4)
            {
                if (shipCanGather)
                {
                    ResourceCheck();
                }

                if (shipCanAttack)
                {
                    MilitaryAttackCheck();
                }
            }

            if(attackType == 3)
            {
                if (!attacking)
                {
                    if (jetSpray.gameObject.activeSelf)
                    {
                        jetSpray.gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
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

    private void Rightclick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        clickedUnit = null;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, unitMask))
        {
            if (hit.collider.GetComponent<Player_Unit>())
            {
                clickedUnit = hit.collider.GetComponent<Player_Unit>();

                canAttack = false;
                redirect = true;
                animalTargeted = false;
                destroyableTargeted = false;
                enemyTargeted = false;

                targetClicked = clickedUnit.gameObject;

                if (clickedUnit.unitType == 4 && unitType != 4 && clickedUnit.shipCanTransport)
                {
                    if (agent.enabled)
                    {
                        agent.destination = targetClicked.transform.position;
                    }

                    temp_destination = targetClicked.transform.position;

                    if (!AttackCursorActive && clickedUnit.unitsHeld.Count <= clickedUnit.holdingCapacity && Vector3.Distance(transform.position, targetClicked.transform.position) <= clickedUnit.pickUpDistance)
                    {
                        clickedUnit.unitsHeld.Add(gameObject);
                        gameObject.SetActive(false);
                    }
                }

                assignedToBuild = false;
                structureToBuild = null;
            }
        }

        if (!clickedUnit)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject.GetComponent<Terrain>())
                {
                    canAttack = false;
                    redirect = true;
                    animalTargeted = false;
                    destroyableTargeted = false;
                    enemyTargeted = false;

                    targetClicked = null;

                    Formation(hit.point);
                    temp_destination = hit.point;

                    player.UC.rightClickOnEnvEffect.transform.position = new Vector3(hit.point.x, hit.point.y + 1, hit.point.z);

                    if (!player.UC.rightClickOnEnvEffect.isPlaying)
                    {
                        player.UC.rightClickOnEnvEffect.Play();
                    }

                    if (AnchorCursorActive)
                    {
                        if (Vector3.Distance(transform.position, temp_destination) <= pickUpDistance)
                        {
                            foreach (GameObject unit in unitsHeld)
                            {
                                temp_destination.y = Terrain.activeTerrain.SampleHeight(temp_destination) + Terrain.activeTerrain.transform.position.y;
                                unit.transform.position = temp_destination;

                                unit.SetActive(true);
                            }

                            foreach (GameObject unit in unitsHeld)
                            {
                                unitsHeld.Remove(unit);
                            }
                        }
                    }

                    assignedToBuild = false;
                    structureToBuild = null;
                }

                if (hit.collider.GetComponent<RTSWater>())
                {
                    canAttack = false;
                    redirect = true;
                    animalTargeted = false;
                    destroyableTargeted = false;
                    enemyTargeted = false;

                    targetClicked = null;

                    Formation(hit.point);
                    temp_destination = hit.point;

                    player.UC.rightClickOnEnvEffect.transform.position = new Vector3(hit.point.x, hit.point.y + 1, hit.point.z);

                    if (!player.UC.rightClickOnEnvEffect.isPlaying)
                    {
                        player.UC.rightClickOnEnvEffect.Play();
                    }

                    assignedToBuild = false;
                    structureToBuild = null;
                }

                if (player.player_enemies.Contains(hit.collider.gameObject))
                {
                    temp_destination = hit.collider.gameObject.transform.position;
                    redirect = true;

                    targetClicked = hit.collider.gameObject;

                    agent.destination = hit.collider.gameObject.transform.position;

                    animalTargeted = false;
                    destroyableTargeted = false;
                    enemyTargeted = true;

                    assignedToBuild = false;
                    structureToBuild = null;
                }

                if (hit.collider.gameObject.GetComponent<NodeManager>())
                {
                    temp_destination = hit.collider.gameObject.transform.position;
                    redirect = true;

                    targetClicked = hit.collider.gameObject;

                    if (unitType == 1 || (unitType == 4 && shipCanGather))
                    {
                        if (targetClicked.GetComponent<PlaceableBuilding>())
                        {
                            if (targetClicked.GetComponent<PlaceableBuilding>().playerID == 0)
                            {
                                targetNode = hit.collider.gameObject.GetComponent<NodeManager>();
                            }
                        }
                        else
                        {
                            targetNode = hit.collider.gameObject.GetComponent<NodeManager>();
                        }
                    }

                    agent.destination = hit.collider.gameObject.transform.position;

                    animalTargeted = false;
                    destroyableTargeted = false;
                    enemyTargeted = false;

                    assignedToBuild = false;
                    structureToBuild = null;
                }
                else
                {
                    targetNode = null;
                    isGathering = false;
                }

                if (hit.collider.gameObject.GetComponent<Wander>())
                {
                    temp_destination = hit.collider.gameObject.transform.position;
                    redirect = true;

                    targetClicked = hit.collider.gameObject;

                    agent.destination = hit.collider.gameObject.transform.position;

                    animalTargeted = true;
                    destroyableTargeted = false;
                    enemyTargeted = false;

                    assignedToBuild = false;
                    structureToBuild = null;
                }

                if (hit.collider.gameObject.GetComponent<Destroyable>())
                {
                    temp_destination = hit.collider.gameObject.transform.position;
                    redirect = true;

                    targetClicked = hit.collider.gameObject;

                    agent.destination = hit.collider.gameObject.transform.position;

                    animalTargeted = false;
                    destroyableTargeted = true;
                    enemyTargeted = false;

                    assignedToBuild = false;
                    structureToBuild = null;
                }

                if (hit.collider.GetComponent<PlaceableBuilding>())
                {
                    temp_destination = hit.collider.gameObject.transform.position;
                    redirect = true;

                    targetClicked = hit.collider.gameObject;

                    agent.destination = hit.collider.gameObject.transform.position;

                    if (targetClicked.GetComponent<PlaceableBuilding>().PS)
                    {
                        if (!targetClicked.GetComponent<PlaceableBuilding>().isBuilt)
                        {
                            buildOrder.Add(targetClicked.GetComponent<PlaceableBuilding>());
                        }

                        enemyTargeted = false;
                    }
                    else if (player.player_enemies.Contains(hit.collider.gameObject))
                    {
                        enemyTargeted = true;
                    }
                    else
                    {
                        enemyTargeted = false;
                    }

                    animalTargeted = false;
                    destroyableTargeted = false;
                }
                else
                {
                    buildOrder.Clear();
                    isBuilding = false;

                    if(unitType == 1)
                    {
                        anim.SetBool("building", isBuilding);
                        swappables[2].SetActive(isBuilding);
                    }
                }

                if (hit.collider.gameObject.GetComponent<Drops>())
                {
                    temp_destination = hit.collider.gameObject.transform.position;
                    redirect = true;

                    targetClicked = hit.collider.gameObject;

                    agent.destination = hit.collider.gameObject.transform.position;

                    if (hit.collider.GetComponent<PlaceableBuilding>())
                    {
                        if (hit.collider.GetComponent<PlaceableBuilding>().isBuilt)
                        {
                            canDepositResources = true;
                        }
                    }
                    else
                    {
                        canDepositResources = true;
                    }

                    animalTargeted = false;
                    destroyableTargeted = false;
                    enemyTargeted = false;

                    assignedToBuild = false;
                    structureToBuild = null;
                }
                else
                {
                    canDepositResources = false;
                }

                if (hit.collider.GetComponent<NPC>())
                {
                    canAttack = false;
                    redirect = true;
                    animalTargeted = false;
                    destroyableTargeted = false;
                    enemyTargeted = false;

                    targetClicked = hit.collider.gameObject;

                    if (targetClicked.GetComponent<NPC>().isAggressive)
                    {
                        NPCTargeted = true;
                    }
                    else
                    {
                        NPCTargeted = false;
                    }

                    agent.destination = hit.point;
                    temp_destination = hit.point;

                    assignedToBuild = false;
                    structureToBuild = null;
                }
                else if (hit.collider.GetComponent<NPCBuilding>())
                {
                    canAttack = false;
                    redirect = true;
                    animalTargeted = false;
                    destroyableTargeted = false;
                    enemyTargeted = false;

                    targetClicked = hit.collider.gameObject;

                    if (targetClicked.GetComponent<NPCBuilding>().isEnemy)
                    {
                        NPCTargeted = true;
                    }
                    else
                    {
                        NPCTargeted = false;
                    }

                    agent.destination = hit.point;
                    temp_destination = hit.point;

                    assignedToBuild = false;
                    structureToBuild = null;
                }
                else
                {
                    NPCTargeted = false;
                }
            }
        }
    }

    public void WalkCheck()
    {
        if (!targetClicked)
        {
            if (attackType == 1)
            {
                redirectDistanceLimit = 1f;
            }
            else if (attackType == 2 || attackType == 3)
            {
                redirectDistanceLimit = 5f;
            }
        }
        else if (targetNode)
        {
            redirectDistanceLimit = targetNode.bounds.size.x / 2 + 1f;
        }
        else if (targetClicked.GetComponent<PlaceableBuilding>() || targetClicked.GetComponent<NPCBuilding>())
        {
            if (attackType == 1)
            {
                redirectDistanceLimit = 5f;
            }
            else if (attackType == 2 || attackType == 3)
            {
                redirectDistanceLimit = 10f;
            }
        }
        else
        {
            if (attackType == 1)
            {
                redirectDistanceLimit = 1f;
            }
            else if (attackType == 2 || attackType == 3)
            {
                redirectDistanceLimit = 5f;
            }
        }

        if (Vector3.Distance(transform.position, temp_destination) < redirectDistanceLimit)
        {
            redirect = false;
        }

        if (isBuilding)
        {
            walking = false;

            if (CanRetaliate)
            {
                canAttack = true;
            }
            else
            {
                canAttack = false;
            }
        }
        else if (agent.remainingDistance <= agent.stoppingDistance)
        {
            walking = false;
            canAttack = true;
        }
        else
        {
            walking = true;
            canAttack = false;
        }

        if (!walking)
        {
            if (secondaryFogOfWar.gameObject.activeSelf && canClearFog)
            {
                secondaryFogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadius, 0, 0, 0);
            }
        }
        else
        {
            if (fogOfWar.gameObject.activeSelf && canClearFog)
            {
                fogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadius, 0, 0, 0);
                minimapFogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadius, 0, 0, 0);

                nearestPS = GetClosestPlayerStructure(player.p_structs, false);

                if (nearestPS.building.isBuilt)
                {
                    if (Vector3.Distance(transform.position, nearestPS.transform.position) > nearestPS.building.fogClearRadius)
                    {
                        secondaryFogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadius, 0, false, 0, 0, 2, 0);
                    }
                }
                else
                {
                    secondaryFogOfWar.SetFogOfWarAlpha(transform.position, fogClearRadius, 0, false, 0, 0, 2, 0);
                }
            }
        }

        if (anim)
        {
            anim.SetBool("walking", walking);
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

    public void ManualAttack()
    {
        if (!targetClicked)
        {
            animalTargeted = false;
            destroyableTargeted = false;
            enemyTargeted = false;
            NPCTargeted = false;

            agent.destination = transform.position;
        }
        else
        {
            if(targetClicked.GetComponent<PlaceableBuilding>() || targetClicked.GetComponent<NPCBuilding>())
            {
                manualAttackRange = minBuildingAttackRange;
            }
            else
            {
                manualAttackRange = minUnitAttackRange;
            }

            agent.destination = targetClicked.transform.position;

            Vector3 dir = targetClicked.transform.position - transform.position;
            Quaternion rot = Quaternion.LookRotation(dir);

            if (Vector3.Distance(targetClicked.transform.position, transform.position) <= manualAttackRange && !death)
            {
                if (!targetClicked.GetComponent<HealthBar>().death)
                {
                    walking = false;

                    agent.destination = transform.position;

                    if (canAttack == true)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 3);

                        attacking = true;

                        if (attackType == 1)
                        {
                            MeleeAttack(targetClicked);
                        }
                        else if (attackType == 2)
                        {
                            RangedAttack(targetClicked);
                        }
                        else if(attackType == 3)
                        {
                            JetAttack();
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
                attacking = false;
            }
        }
    }

    public void Retaliate()
    {
        attacker = HB.attacker;

        float dist = Vector3.Distance(attacker.transform.position, transform.position);
        attackerDirection = attacker.transform.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(attackerDirection);

        if (dist < maxAttackRange && redirect == false && !death)
        {
            if (!attacker.GetComponent<HealthBar>().death)
            {
                agent.destination = attacker.transform.position;

                if (dist <= minAttackRange)
                {
                    walking = false;

                    agent.destination = transform.position;

                    if (canAttack == true)
                    {
                        attacking = true;

                        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 3);

                        if (attackType == 1)
                        {
                            MeleeAttack(attacker);
                        }
                        else if (attackType == 2)
                        {
                            RangedAttack(attacker);
                        }
                        else if(attackType == 3)
                        {
                            JetAttack();
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
        else
        {
            HB.attacker = null;
            attacker = null;
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

    Player_Structure GetClosestPlayerStructure(List<Player_Structure> p_structures, bool garrisonCheck)
    {
        Player_Structure closestPS = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (Player_Structure targetStructure in p_structures)
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
                        closestPS = targetStructure;
                    }
                }
                else
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPS = targetStructure;
                    }
                }
            }
        }

        return closestPS;
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

    public void MilitaryAttackCheck()
    {
        if (player.player_enemies.Count <= 0)
        {
            enemyDistance = 100f;

            if (animalTargeted == true || destroyableTargeted == true || NPCTargeted == true)
            {
                ManualAttack();
            }
            else if (HB.attacker)
            {
                if (CanRetaliate)
                {
                    if (HB.attacker.GetComponent<PlaceableBuilding>() || HB.attacker.GetComponent<NPCBuilding>())
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
            }
            else
            {
                attacking = false;
            }
        }
        else if (player.player_enemies.Count > 0)
        {
            nearestEnemy = GetClosestEnemy(player.player_enemies);

            if (nearestEnemy)
            {
                enemyDistance = Vector3.Distance(nearestEnemy.transform.position, transform.position);

                enemyDirection = nearestEnemy.transform.position - transform.position;
                Quaternion rot = Quaternion.LookRotation(enemyDirection);

                if (nearestEnemy.GetComponent<PlaceableBuilding>() || nearestEnemy.GetComponent<NPCBuilding>())
                {
                    minAttackRange = minBuildingAttackRange;
                    maxAttackRange = maxBuildingAttackRange;
                }
                else
                {
                    minAttackRange = minUnitAttackRange;
                    maxAttackRange = maxUnitAttackRange;
                }

                if (animalTargeted == true || destroyableTargeted == true || enemyTargeted == true || NPCTargeted == true)
                {
                    ManualAttack();
                }
                else if (enemyDistance <= maxAttackRange && redirect == false && !death)
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
                                else if(attackType == 3)
                                {
                                    JetAttack();
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
                    if (CanRetaliate)
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
                }
                else
                {
                    attacking = false;
                }
            }
        }

        if(unitType == 3)
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

        if (toggleWeapon)
        {
            if (toggleWeapon.activeSelf && !attacking)
            {
                toggleWeapon.SetActive(false);
            }
            else if(!toggleWeapon.activeSelf && attacking)
            {
                toggleWeapon.SetActive(true);
            }
        }
    }

    public void ResourceCheck()
    {
        if (targetNode)
        {
            if(targetNode.availableResource <= resourceCapacity)
            {
                lookForNextResource = true;
            }
            else
            {
                lookForNextResource = false;
            }

            resourceDistance = Vector3.Distance(targetNode.transform.position, transform.position);
            Vector3 dir = targetNode.transform.position - transform.position;
            Quaternion rot = Quaternion.LookRotation(dir);

            heldResourceType = targetNode.resourceType;

            if (resourceDistance <= targetNode.bounds.size.x / 2 + 1f && heldResource < resourceCapacity)
            {
                isGathering = true;

                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 3);

                if (heldResourceType == NodeManager.ResourceTypes.Wood)
                {
                    woodcutting = true;

                    if(swappables.Length > 0)
                    {
                        swappables[0].SetActive(true);
                    }
                }
                else if(heldResourceType == NodeManager.ResourceTypes.Food)
                {
                    farming = true;
                }
                else if(heldResourceType == NodeManager.ResourceTypes.Gold || heldResourceType == NodeManager.ResourceTypes.Stone || heldResourceType == NodeManager.ResourceTypes.Iron)
                {
                    mining = true;

                    if (swappables.Length > 1)
                    {
                        swappables[1].SetActive(true);
                    }
                }

                if (!incremented)
                {
                    targetNode.gatherers++;
                    incremented = true;
                }
                
            }
            else
            {
                isGathering = false;

                woodcutting = false;

                if(swappables.Length > 0)
                {
                    if (swappables[0].activeSelf)
                    {
                        swappables[0].SetActive(false);
                    }
                }
                
                mining = false;

                if(swappables.Length > 1)
                {
                    if (swappables[1].activeSelf)
                    {
                        swappables[1].SetActive(false);
                    }
                }

                farming = false;

                if (incremented)
                {
                    targetNode.gatherers--;
                    targetNode.availableResource -= heldResource;
                    incremented = false;
                }
            }

            if(unitType == 1)
            {
                anim.SetBool("woodcutting", woodcutting);
                anim.SetBool("mining", mining);
                anim.SetBool("farming", farming);
            }
            
        }
        else
        {
            if (isGathering)
            {
                isGathering = false;
            }

            if (woodcutting)
            {
                woodcutting = false;

                if (swappables.Length > 0)
                {
                    swappables[0].SetActive(false);
                }

                anim.SetBool("woodcutting", woodcutting);
            }

            if (mining)
            {
                mining = false;

                if (swappables.Length > 1)
                {
                    swappables[1].SetActive(false);
                }

                anim.SetBool("mining", mining);
            }

            if (farming)
            {
                farming = false;
            }

            if (heldResource != 0)
            {
                if (redirect == false && canDepositResources)
                {
                    FindNearestDropSite();
                }
            }

            if (lookForNextResource)
            {
                resources = player.resources.ToArray();

                nextTargetNode = GetClosestResourceOfType(resources, targetNode.resourceType);

                if (nextTargetNode)
                {
                    if (Vector3.Distance(nextTargetNode.transform.position, targetNode.transform.position) <= nodeSearchDistance)
                    {
                        targetNode = nextTargetNode;
                    }
                    else
                    {
                        lookForNextResource = false;
                    }
                }
            }
        }


        if (heldResource >= resourceCapacity)
        {
            isGathering = false;

            if(redirect == false && canDepositResources)
            {
                FindNearestDropSite();
            }
        }
    }

    public void FindNearestDropSite()
    {
        drops = player.dropSites.ToArray();

        nearestDrop = GetClosestDropOff(drops, heldResourceType);

        agent.destination = nearestDrop.transform.position;

        dropSiteDistance = Vector3.Distance(nearestDrop.transform.position, transform.position);

        if(dropSiteDistance < nearestDrop.bounds.size.x / 2 + 1f)
        {
            DepositCarryingResources(nearestDrop);
        }
    }

    Drops GetClosestDropOff(Drops[] dropOffs, NodeManager.ResourceTypes NRT)
    {
        Drops closestDrop = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (Drops targetDrop in dropOffs)
        {
            Vector3 direction = targetDrop.transform.position - position;
            float distance = direction.sqrMagnitude;
            if (distance < closestDistance)
            {
                if (NRT == NodeManager.ResourceTypes.Gold)
                {
                    if (targetDrop.Gold == true)
                    {
                        closestDistance = distance;
                        closestDrop = targetDrop;
                    }
                }
                if (NRT == NodeManager.ResourceTypes.Food)
                {
                    if (targetDrop.Food == true)
                    {
                        closestDistance = distance;
                        closestDrop = targetDrop;
                    }
                }
                if (NRT == NodeManager.ResourceTypes.Wood)
                {
                    if (targetDrop.Wood == true)
                    {
                        closestDistance = distance;
                        closestDrop = targetDrop;
                    }
                }
                if (NRT == NodeManager.ResourceTypes.Stone)
                {
                    if (targetDrop.Stone == true)
                    {
                        closestDistance = distance;
                        closestDrop = targetDrop;
                    }
                }
                if (NRT == NodeManager.ResourceTypes.Iron)
                {
                    if (targetDrop.Iron == true)
                    {
                        closestDistance = distance;
                        closestDrop = targetDrop;
                    }
                }
            }
        }

        return closestDrop;
    }

    public void DepositCarryingResources(Drops dropSite)
    {
        if (RM.gold >= RM.maxGold || RM.wood >= RM.maxWood || RM.food >= RM.maxFood || RM.stone >= RM.maxStone || RM.iron >= RM.maxIron)
        {
            agent.destination = transform.position;
        }
        else if (heldResourceType == NodeManager.ResourceTypes.Food && dropSite.Food)
        {
            RM.food += heldResource;
            heldResource = 0;

            if (targetNode)
            {
                agent.destination = targetNode.transform.position;
            }

        }
        else if (heldResourceType == NodeManager.ResourceTypes.Wood && dropSite.Wood)
        {
            RM.wood += heldResource;
            heldResource = 0;

            if (targetNode)
            {
                agent.destination = targetNode.transform.position;
            }

        }
        else if (heldResourceType == NodeManager.ResourceTypes.Stone && dropSite.Stone)
        {
            RM.stone += heldResource;
            heldResource = 0;

            if (targetNode)
            {
                agent.destination = targetNode.transform.position;
            }

        }
        else if (heldResourceType == NodeManager.ResourceTypes.Gold && dropSite.Gold)
        {
            RM.gold += heldResource;
            heldResource = 0;

            if (targetNode)
            {
                agent.destination = targetNode.transform.position;
            }

        }
        else if (heldResourceType == NodeManager.ResourceTypes.Iron && dropSite.Iron)
        {
            RM.iron += heldResource;
            heldResource = 0;

            if (targetNode)
            {
                agent.destination = targetNode.transform.position;
            }
        }
    }

    public void VillagerAttackCheck()
    {
        if (animalTargeted == true)
        {
            Hunt();

            anim.SetBool("hunting", hunting);
            swappables[3].SetActive(hunting);
        }
        else if(destroyableTargeted == true || enemyTargeted == true)
        {
            ManualAttack();
        }
        else if(HB.attacker)
        {
            if (isBuilding)
            {
                isBuilding = false;
                buildOrder.Clear();

                anim.SetBool("building", isBuilding);
                swappables[2].SetActive(isBuilding);
            }

            if (isGathering)
            {
                targetNode = null;
            }

            if (CanRetaliate)
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
                if(redirect == false)
                {
                    nearestGarrisonablePS = GetClosestPlayerStructure(player.p_structs, true);

                    if (nearestGarrisonablePS)
                    {
                        agent.destination = nearestGarrisonablePS.transform.position;
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

        anim.SetBool("attacking", attacking);
        swappables[4].SetActive(attacking);
    }

    public void Build(GameObject structure)
    {
        HealthBar buildingHB = structure.GetComponent<HealthBar>();

        if (buildingHB.currentHealth < buildingHB.maxHealth)
        {
            if (buildCountdown <= 0f)
            {
                buildCountdown = 1 / buildRate;

                buildingHB.IncreaseHealth(buildStrength, gameObject);
            }

            buildCountdown -= Time.deltaTime;
        }
        else
        {
            isBuilding = false;
            assignedToBuild = false;
            structureToBuild = null;
        }
    }

    public void BuildCheck(PlaceableBuilding bldng)
    {
        if (!bldng.isBuilt)
        {
            travelDistance = Vector3.Distance(bldng.transform.position, transform.position);

            if (travelDistance <= bldng.bounds.size.x / 2 + 1f)
            {
                isBuilding = true;
            }
        }
        else
        {
            if (structureToBuild)
            {
                if(buildOrder.Count > 1)
                {
                    isBuilding = false;
                    assignedToBuild = false;
                    buildOrder.Remove(structureToBuild);

                    structureToBuild = buildOrder[0];
                }
                else if (structureToBuild.nm)
                {
                    targetNode = structureToBuild.nm;
                    isGathering = true;

                    structureToBuild.nm.gatherers++;

                    heldResourceType = structureToBuild.nm.resourceType;

                    isBuilding = false;
                    assignedToBuild = false;
                    buildOrder.Remove(structureToBuild);
                    structureToBuild = null;
                }
                else if (structureToBuild.drops)
                {
                    resources = player.resources.ToArray();
                    NodeManager CF = null, CW = null, CG = null, CS = null, CI = null;
                    float CFD = 1001, CWD = 1001, CGD = 1001, CSD = 1001, CID = 1001;

                    if (structureToBuild.drops.Food)
                    {
                        CF = GetClosestResourceOfType(resources, NodeManager.ResourceTypes.Food);

                        if (CF)
                        {
                            CFD = Vector3.Distance(CF.transform.position, transform.position);
                        }
                    }
                    if (structureToBuild.drops.Wood)
                    {
                        CW = GetClosestResourceOfType(resources, NodeManager.ResourceTypes.Wood);

                        if (CW)
                        {
                            CWD = Vector3.Distance(CW.transform.position, transform.position);
                        }
                    }
                    if (structureToBuild.drops.Gold)
                    {
                        CG = GetClosestResourceOfType(resources, NodeManager.ResourceTypes.Gold);

                        if (CG)
                        {
                            CGD = Vector3.Distance(CG.transform.position, transform.position);
                        }
                    }
                    if (structureToBuild.drops.Stone)
                    {
                        CS = GetClosestResourceOfType(resources, NodeManager.ResourceTypes.Stone);

                        if (CS)
                        {
                            CSD = Vector3.Distance(CS.transform.position, transform.position);
                        }
                    }
                    if (structureToBuild.drops.Iron)
                    {
                        CI = GetClosestResourceOfType(resources, NodeManager.ResourceTypes.Iron);

                        if (CI)
                        {
                            CID = Vector3.Distance(CI.transform.position, transform.position);
                        }
                    }

                    float shortestDist = Mathf.Min(CFD, CWD, CGD, CSD, CID);

                    if (shortestDist <= nodeSearchDistance)
                    {
                        if (shortestDist == CFD)
                        {
                            targetNode = CF;
                            agent.destination = targetNode.transform.position;
                        }
                        else if (shortestDist == CWD)
                        {
                            targetNode = CW;
                            agent.destination = targetNode.transform.position;
                        }
                        else if (shortestDist == CGD)
                        {
                            targetNode = CG;
                            agent.destination = targetNode.transform.position;
                        }
                        else if (shortestDist == CSD)
                        {
                            targetNode = CS;
                            agent.destination = targetNode.transform.position;
                        }
                        else if (shortestDist == CID)
                        {
                            targetNode = CI;
                            agent.destination = targetNode.transform.position;
                        }
                    }

                    isBuilding = false;
                    assignedToBuild = false;
                    buildOrder.Remove(structureToBuild);
                    structureToBuild = null;
                }
                else
                {
                    isBuilding = false;
                    assignedToBuild = false;
                    buildOrder.Remove(structureToBuild);
                    structureToBuild = null;
                }
            }
            else
            {
                if (buildOrder.Count > 1)
                {
                    isBuilding = false;
                    assignedToBuild = false;
                    structureToBuild = buildOrder[0];
                }
                else
                {
                    isBuilding = false;
                    assignedToBuild = false;
                    structureToBuild = null;
                }
                
            }

            targetClicked = null;
        }

        anim.SetBool("building", isBuilding);
        swappables[2].SetActive(isBuilding);
    }

    NodeManager GetClosestResourceOfType(NodeManager[] Resources, NodeManager.ResourceTypes resourceType)
    {
        NodeManager closestResource = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (NodeManager targetResource in Resources)
        {
            if (targetResource)
            {
                Vector3 direction = targetResource.transform.position - position;
                float distance = direction.sqrMagnitude;
                if (distance < closestDistance)
                {
                    if (!targetResource.building && targetResource.resourceType == resourceType)
                    {
                        closestDistance = distance;
                        closestResource = targetResource;
                    }
                    else if (targetResource.building && targetResource.resourceType == resourceType)
                    {
                        if (targetResource.building.playerID == 0)
                        {
                            closestDistance = distance;
                            closestResource = targetResource;
                        }
                    }
                }
            }
        }

        return closestResource;
    }

    public void Hunt()
    {
        if (!targetClicked)
        {
            animalTargeted = false;
        }
        else
        {
            Vector3 dir = targetClicked.transform.position - transform.position;
            Quaternion rot = Quaternion.LookRotation(dir);

            if (Vector3.Distance(targetClicked.transform.position, transform.position) <= huntAttackRange && !death)
            {
                if (!targetClicked.GetComponent<HealthBar>().death)
                {
                    walking = false;

                    agent.destination = transform.position;

                    if (canAttack == true)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 3);

                        hunting = true;

                        RangedAttack(targetClicked);
                    }
                    else
                    {
                        hunting = false;
                    }
                }
                else
                {
                    hunting = false;

                    if (targetClicked.GetComponent<NodeManager>())
                    {
                        targetNode = targetClicked.GetComponent<NodeManager>();
                        agent.destination = targetNode.transform.position;
                        HB.attacker = null;
                        targetClicked = null;
                    }
                }

            }
            else
            {
                hunting = false;
            }
        }
    }

    public void AccountForPlayerResearch()
    {
        for (int i = 1; i < player.techs.Count; i++)
        {
            if (player.researched[i])
            {
                if (player.techs[i])
                {
                    if (player.techs[i].forUnit)
                    {
                        PlayerUnitImprovements(player.techs[i]);
                    }
                }
            }
        }
    }

    private void PlayerUnitImprovements(Attributes attr)
    {
        if (unitType == attr.unitType)
        {
            if (attr.attackType == 0)
            {
                HB.maxHealth += attr.healthModifier;
                HB.currentHealth += attr.healthModifier;
                resourceCapacity += attr.gatheringCapacityModifier;
                damage += attr.damageModifier;
                gatheringRate += attr.gatheringRateModifier;
                buildStrength += attr.buildingStrengthModifier;
            }
            else if (attackType == attr.attackType)
            {
                HB.maxHealth += attr.healthModifier;
                HB.currentHealth += attr.healthModifier;
                resourceCapacity += attr.gatheringCapacityModifier;
                damage += attr.damageModifier;
                gatheringRate += attr.gatheringRateModifier;
                buildStrength += attr.buildingStrengthModifier;
            }
        }
    }

    public void AttackOnLeftClick()
    {
        cursor.SetEnemy();
        player.UC.cursorIsBusy = true;
        AttackCursorActive = true;
    }

    public void DropOffRemainingUnits()
    {
        cursor.SetAnchor();
        player.UC.cursorIsBusy = true;
        AnchorCursorActive = true;
    }

    public void Die()
    {
        deathOverride = true;
    }

    void OnEnable()
    {
        if (unitType == 1 || (unitType == 4 && shipCanGather))
        {
            StartCoroutine(GatherTick());
        }

        StartCoroutine(GarrisonCheck());
    }

    IEnumerator GatherTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (isGathering)
            {
                heldResource += gatheringRate;

                canDepositResources = true;
            }
        }
    }

    IEnumerator GarrisonCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (targetClicked)
            {
                Player_Structure PS;

                if (PS = targetClicked.GetComponent<Player_Structure>())
                {
                    if (PS.building.isBuilt)
                    {
                        if(PS.canGarrison && PS.garrisonedUnits.Count < PS.garrisonCapacity)
                        {
                            if(Vector3.Distance(targetClicked.transform.position, transform.position) <= PS.building.bounds.size.x/2 + 1f)
                            {
                                garrisoned = true;

                                agent.destination = transform.position;

                                PS.garrisonedUnits.Add(this);

                                gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
    }

    public void ToggleRetaliation()
    {
        CanRetaliate = !CanRetaliate;
    }

    public void CallBuildFunction(int index)
    {
        BM.SetBuilding(index);
    }

    public void Upgrade()
    {
        if (nextUpgrade)
        {
            Instantiate(nextUpgrade, transform.position, transform.rotation);
        }
    }

    private void Formation(Vector3 destination)
    {
        if(player.OS.selectedUnits.Count > 1)
        {
            Vector3 sum = Vector3.zero;
            Vector3 center;
            Vector3 startVector;

            foreach(Player_Unit PU in player.OS.selectedUnits)
            {
                sum += PU.transform.position;
            }

            center = sum / player.OS.selectedUnits.Count;

            foreach (Player_Unit PU in player.OS.selectedUnits)
            {
                startVector = PU.transform.position - center;
                PU.agent.destination = destination + startVector;
            }
        }
        else
        {
            agent.destination = destination;
        }
    }

}
