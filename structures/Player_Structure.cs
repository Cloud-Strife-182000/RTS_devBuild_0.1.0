using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Structure : MonoBehaviour
{
    public bool isSelected, canAttack, destroyed;

    public GameObject selectionEffect;

    public float standardFortification, siegeFortification;

    public int populationContribution;

    public bool canClearFog = true;

    public float damage;
    public int damageType; //1 -> Blunt, 2 -> Pierce, 3 -> Siege, 4 -> Elemental
    public float attackRange;
    public GameObject bulletPrefab;
    public float fireRate = 0.8f;
    public float fireCountdown = 0f;
    public Transform firePoint;
    private float enemyDistance;
    private GameObject nearestEnemy;

    public Vector3 rallyPoint;
    public GameObject rallyPointGraphic;

    public List<Player_Unit> spawnables;
    public Player_Unit lastSpawnedObject;

    public ActivePlayer player;
    private List<GameObject> enemies;
    public PlaceableBuilding building;
    public UnitCanvas UC;

    public HealthBar HB;
    public bool unitCanvasSetActive;

    public int queueCounter;
    public List<RTSButton> taskQueue;
    public RTSButton currentQueueItem;
    public int currentQueueIndex;

    public GameObject queueImagesParent;
    public List<Image> queueImages;
    public List<int> queueIndexes;

    public bool canGarrison;
    public List<Player_Unit> garrisonedUnits;
    public int garrisonCapacity;
    public int buildingAge;
    public GameObject nextAgeReplacement;

    private void Awake()
    {
        player = FindObjectOfType<ActivePlayer>();
    }

    void Start()
    {
        if (GetComponent<PlaceableBuilding>())
        {
            building = GetComponent<PlaceableBuilding>();
        }

        if (GetComponentInChildren<UnitCanvas>())
        {
            UC = GetComponentInChildren<UnitCanvas>();
            UC.player = player;

            UC.gameObject.SetActive(false);
        }

        player.populationCapacity += populationContribution;

        if (queueImagesParent)
        {
            Button[] tempButtonArray;

            tempButtonArray = queueImagesParent.GetComponentsInChildren<Button>();

            foreach (Button b in tempButtonArray)
            {
                queueImages.Add(b.image);
            }
        }
        
        foreach(Image img in queueImages)
        {
            img.gameObject.SetActive(false);
        }

        if (rallyPointGraphic)
        {
            if (rallyPointGraphic.activeSelf)
            {
                rallyPointGraphic.SetActive(false);
            }
        }

        player.p_structs.Add(this);

        HB = GetComponent<HealthBar>();
    }

    void Update()
    {
        destroyed = HB.death;

        if (!destroyed)
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

                if (building)
                {
                    if (building.isBuilt)
                    {
                        if (Input.GetMouseButtonDown(1))
                        {
                            Rightclick();
                        }
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        Rightclick();
                    }
                }

                if(rallyPoint != Vector3.zero)
                {
                    if (rallyPointGraphic)
                    {
                        if (!rallyPointGraphic.activeSelf)
                        {
                            rallyPointGraphic.SetActive(true);
                        }
                    }
                }

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
                if (UC)
                {
                    if (unitCanvasSetActive)
                    {
                        UC.gameObject.SetActive(false);

                        unitCanvasSetActive = false;
                    }
                }

                if (rallyPointGraphic)
                {
                    if (rallyPointGraphic.activeSelf)
                    {
                        rallyPointGraphic.SetActive(false);
                    }
                }

                if (selectionEffect)
                {
                    if (selectionEffect.activeSelf)
                    {
                        selectionEffect.SetActive(false);
                    }
                }
            }

            if (building.isBuilt)
            {
                if (canGarrison)
                {
                    if(garrisonedUnits.Count > 0 && !canAttack)
                    {
                        AttackCheck();
                    }
                }

                if (canAttack)
                {
                    AttackCheck();
                }

                if(queueCounter > 0)
                {
                    currentQueueItem = taskQueue[0];
                    currentQueueIndex = queueIndexes[0];

                    if (currentQueueItem.spawnBar)
                    {
                        currentQueueItem.spawnBar.gameObject.SetActive(true);
                        Spawn_Unit();
                    }
                    else if (currentQueueItem.researchBar)
                    {
                        currentQueueItem.researchBar.gameObject.SetActive(true);
                        Research();
                    }
                }
            }
            
        }
        else
        {
            ReleaseGarrison();

            player.populationCapacity -= populationContribution;

            Destroy(gameObject);
        }
    }

    private void Rightclick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.GetComponent<Terrain>() || hit.collider.gameObject.GetComponent<RTSWater>())
            {
                rallyPoint = hit.point;

                if (rallyPointGraphic)
                {
                    rallyPointGraphic.transform.position = new Vector3(rallyPoint.x, rallyPoint.y + 1, rallyPoint.z) ;
                }
            }
        }
    }

    public void Spawn_Unit()
    {
        if (currentQueueItem.spawnBar.Value < 1)
        {
            if (currentQueueItem.spawnCountdown <= 0f)
            {
                currentQueueItem.spawnCountdown = 1 / currentQueueItem.spawnRate;

                currentQueueItem.spawnBar.Value += currentQueueItem.spawnIncrement;
            }

            currentQueueItem.spawnCountdown -= Time.deltaTime;
        }
        else
        {
            currentQueueItem.spawnBar.Value = 0;

            currentQueueItem.spawnBar.gameObject.SetActive(false);

            if (rallyPoint == Vector3.zero)
            {
                lastSpawnedObject = Instantiate(spawnables[currentQueueItem.index], transform.position, transform.rotation);
            }
            else
            {
                lastSpawnedObject = Instantiate(spawnables[currentQueueItem.index], transform.position, transform.rotation);

                lastSpawnedObject.agent.destination = rallyPoint;
            }

            queueImages[currentQueueIndex].sprite = null;
            queueImages[currentQueueIndex].gameObject.SetActive(false);
            queueCounter--;
            taskQueue.Remove(currentQueueItem);
            queueIndexes.Remove(currentQueueIndex);
        }
    }

    public void AccountForUnitCosts(Player_Unit PU)
    {
        player.RM.food -= PU.foodCost;
        player.RM.wood -= PU.woodCost;
        player.RM.gold -= PU.goldCost;
        player.RM.stone -= PU.stoneCost;
        player.RM.iron -= PU.ironCost;
    }

    public bool CheckUnitCosts(Player_Unit PU)
    {
        if (PU.foodCost > player.RM.food)
        {
            return false;
        }
        else if (PU.woodCost > player.RM.wood)
        {
            return false;
        }
        else if (PU.goldCost > player.RM.gold)
        {
            return false;
        }
        else if (PU.stoneCost > player.RM.stone)
        {
            return false;
        }
        else if (PU.ironCost > player.RM.iron)
        {
            return false;
        }

        return true;
    }

    public void RangedAttack(GameObject target)
    {
        if (fireCountdown <= 0f)
        {
            Shoot(target);

            fireCountdown = 1 / fireRate;

            target.GetComponent<HealthBar>().DecreaseHealth(damage, transform.gameObject, damageType);
        }

        fireCountdown -= Time.deltaTime;
    }

    void Shoot(GameObject target)
    {
        GameObject bulletGO = (GameObject)Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        CatapultStoneThrow bullet = bulletGO.GetComponent<CatapultStoneThrow>();

        if (bullet != null)
        {
            bullet.Seek(target, damage, transform.gameObject, target.transform.position, damageType);
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

    private void AttackCheck()
    {
        enemies = player.player_enemies;

        if(enemies.Count > 0)
        {
            nearestEnemy = GetClosestEnemy(enemies);

            enemyDistance = Vector3.Distance(nearestEnemy.transform.position, transform.position);

            if (enemyDistance <= attackRange && !destroyed)
            {
                if (!nearestEnemy.GetComponent<HealthBar>().death)
                {
                    RangedAttack(nearestEnemy);
                }
            }
        }
    }

    public void Research()
    {
        if (currentQueueItem.researchBar.Value < 1)
        {
            if (currentQueueItem.researchCountdown <= 0f)
            {
                currentQueueItem.researchCountdown = 1 / currentQueueItem.researchRate;

                currentQueueItem.researchBar.Value += currentQueueItem.researchIncrement;
            }

            currentQueueItem.researchCountdown -= Time.deltaTime;
        }
        else
        {
            ResearchImprovements();

            queueImages[currentQueueIndex].sprite = null;
            queueImages[currentQueueIndex].gameObject.SetActive(false);
            queueCounter--;
            taskQueue.Remove(currentQueueItem);
            queueIndexes.Remove(currentQueueIndex);

            if (currentQueueItem.isAdvanceAgeButton)
            {
                player.TriggerBuildingUpgrades();
            }

            if (currentQueueItem.nextTech)
            {
                if(currentQueueItem.nextTech.AgeToActivate <= player.Age)
                {
                    currentQueueItem.nextTech.gameObject.SetActive(true);
                }
            }

            UC.frontPanelActions.Remove(currentQueueItem);
            Destroy(currentQueueItem.gameObject);
        }
    }

    private void ResearchImprovements()
    {
        if(currentQueueItem.attr.researchIndex > 0)
        {
            player.researched[currentQueueItem.attr.researchIndex] = true;
        }

        if (currentQueueItem.attr.forUnit)
        {
            foreach (Player_Unit pu in player.player_units)
            {
                if (pu)
                {
                    if (pu.unitType == currentQueueItem.attr.unitType)
                    {
                        if(currentQueueItem.attr.attackType == 0)
                        {
                            CommonUnitImprovements(pu);
                        }
                        else if(pu.attackType == currentQueueItem.attr.attackType)
                        {
                            CommonUnitImprovements(pu);
                        }
                    }
                }
            }
        }
        else if (currentQueueItem.attr.forBuilding)
        {
            foreach(Player_Structure PS in player.p_structs)
            {
                if(currentQueueItem.attr.resourceToTarget != NodeManager.ResourceTypes.Null)
                {
                    if (PS.building.drops)
                    {
                        if (currentQueueItem.attr.resourceToTarget == NodeManager.ResourceTypes.Food)
                        {
                            if (PS.building.drops.Food)
                            {
                                CommonBuildingImprovements(PS);
                            }
                        }
                        else if (currentQueueItem.attr.resourceToTarget == NodeManager.ResourceTypes.Wood)
                        {
                            if (PS.building.drops.Wood)
                            {
                                CommonBuildingImprovements(PS);
                            }
                        }
                        else if (currentQueueItem.attr.resourceToTarget == NodeManager.ResourceTypes.Gold)
                        {
                            if (PS.building.drops.Gold)
                            {
                                CommonBuildingImprovements(PS);
                            }
                        }
                        else if (currentQueueItem.attr.resourceToTarget == NodeManager.ResourceTypes.Stone)
                        {
                            if (PS.building.drops.Stone)
                            {
                                CommonBuildingImprovements(PS);
                            }
                        }
                        else if (currentQueueItem.attr.resourceToTarget == NodeManager.ResourceTypes.Iron)
                        {
                            if (PS.building.drops.Iron)
                            {
                                CommonBuildingImprovements(PS);
                            }
                        }
                    }
                    else if (PS.building.nm)
                    {
                        if (PS.building.nm.resourceType == currentQueueItem.attr.resourceToTarget)
                        {
                            PS.building.nm.availableResource += currentQueueItem.attr.resourceAmountModifier;
                            CommonBuildingImprovements(PS);
                        }
                    }
                }
                else
                {
                    CommonBuildingImprovements(PS);
                }
            }
        }   
        else
        {
            if (currentQueueItem.isAdvanceAgeButton)
            {
                player.Age++;
            }

            if (currentQueueItem.attr.researchIndex == 4)
            {
                player.TriggerUnlocks();
            }

            if (currentQueueItem.isUpgradeButton)
            {
                player.TriggerUnitUpgrades(currentQueueItem.attr.unitIndex);
            }
        }
    }

    private void CommonBuildingImprovements(Player_Structure PS)
    {
        PS.HB.maxHealth += currentQueueItem.attr.healthModifier;
        PS.HB.currentHealth += currentQueueItem.attr.healthModifier;
        PS.standardFortification += currentQueueItem.attr.standardFortificationModifier;
        PS.siegeFortification += currentQueueItem.attr.siegeFortificationModifier;
    }

    private void CommonUnitImprovements(Player_Unit pu)
    {
        pu.HB.maxHealth += currentQueueItem.attr.healthModifier;
        pu.HB.currentHealth += currentQueueItem.attr.healthModifier;
        pu.resourceCapacity += currentQueueItem.attr.gatheringCapacityModifier;
        pu.damage += currentQueueItem.attr.damageModifier;
        pu.gatheringRate += currentQueueItem.attr.gatheringRateModifier;
        pu.buildStrength += currentQueueItem.attr.buildingStrengthModifier;
    }

    public bool CheckResearchCosts(Attributes att)
    {
        if (att.foodCost > player.RM.food)
        {
            return false;
        }
        else if (att.woodCost > player.RM.wood)
        {
            return false;
        }
        else if (att.goldCost > player.RM.gold)
        {
            return false;
        }
        else if (att.stoneCost > player.RM.stone)
        {
            return false;
        }
        else if (att.ironCost > player.RM.iron)
        {
            return false;
        }

        return true;
    }

    public void AccountForResearchCosts(Attributes att)
    {
        player.RM.food -= att.foodCost;
        player.RM.wood -= att.woodCost;
        player.RM.gold -= att.goldCost;
        player.RM.stone -= att.stoneCost;
        player.RM.iron -= att.ironCost;
    }

    public void Upgrade()
    {
        if (buildingAge < player.Age)
        {
            if (nextAgeReplacement)
            {
                Destroy(gameObject);
                Instantiate(nextAgeReplacement, transform.position, transform.rotation);
            }
        }
    }

    public void ReleaseGarrison()
    {
        for(int i = 0; i < garrisonCapacity; i++)
        {
            if (garrisonedUnits.Count > 0)
            {
                garrisonedUnits[0].gameObject.SetActive(true);

                garrisonedUnits[0].targetClicked = null;

                garrisonedUnits[0].garrisoned = false;

                if (rallyPoint != Vector3.zero)
                {
                    garrisonedUnits[0].agent.destination = rallyPoint;
                }

                garrisonedUnits.Remove(garrisonedUnits[0]);
            }
        }
    }
}

