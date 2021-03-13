using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyStructure : MonoBehaviour
{
    public bool canSpawn, canAttack, destroyed, deathOverride;

    public float standardFortification, siegeFortification;

    public int populationContribution;

    public enemyUnit lastSpawned;
    public Vector3 rallyPoint;
    public List<enemyUnit> spawnables;

    public float damage;
    public int damageType; //1 -> Blunt, 2 -> Pierce, 3 -> Siege, 4 -> Elemental
    public float attackRange;
    public GameObject bulletPrefab;
    public float fireRate = 0.8f;
    public float fireCountdown = 0f;
    public Transform firePoint;
    private float enemyDistance;
    private GameObject nearestEnemy;

    public bool canGarrison;
    public int garrisonCapacity;
    public List<enemyUnit> garrisonedUnits;

    public bool hasSpawned;
    public enemyResourceManager ERM;
    public AI_Supervisor AIS;
    public int player_ID;
    private enemyResourceManager[] ERMS;
    private List<GameObject> enemies;

    public HealthBar HB;
    public PlaceableBuilding building;
    private ActivePlayer player;

    public int buildingAge;
    public GameObject nextAgeReplacement;

    public bool isSelected;
    public GameObject selectionEffect;

    // Start is called before the first frame update
    void Start()
    {
        rallyPoint = transform.position;

        building = GetComponent<PlaceableBuilding>();
        HB = GetComponent<HealthBar>();
        player = FindObjectOfType<ActivePlayer>();

        if (selectionEffect)
        {
            selectionEffect.transform.localPosition = new Vector3(0, 0.1f, 0);
        }

        player.e_structs.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        destroyed = HB.death;

        if (!destroyed)
        {
            destroyed = deathOverride;
        }

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

        if (!destroyed)
        {
            if (canSpawn)
            {
                Spawn();
            }

            if (canGarrison)
            {
                if (garrisonedUnits.Count > 0 && !canAttack)
                {
                    AttackCheck();
                }
            }

            if (canAttack)
            {
                AttackCheck();
            }
        }
        else
        {
            Destroy(gameObject);

            AIS.populationCapacity -= populationContribution;
        }
    }

    public void Spawn_Unit(int index)
    {
        if (rallyPoint == Vector3.zero)
        {
            lastSpawned = Instantiate(AIS.spawnOrder[index], transform.position, transform.rotation);

            lastSpawned.AI_St.player_ID = AIS.player_ID;
        }
        else
        {
            lastSpawned = Instantiate(AIS.spawnOrder[index], transform.position, transform.rotation);

            lastSpawned.AI_St.player_ID = AIS.player_ID;

            lastSpawned.agent.destination = rallyPoint;
        }

        AccountForUnitCost();

        AIS.spawnOrder.Remove(AIS.spawnOrder[0]);
    }

    public void Spawn()
    {
        if (spawnables.Count > 0 && building.isBuilt && AIS.spawnOrder.Count > 0)
        {
            if (spawnables.Contains(AIS.spawnOrder[0]))
            {
                if (AIS.spawnOrder[0].unitType == 1 || (AIS.spawnOrder[0].unitType == 4 && AIS.spawnOrder[0].shipCanGather))
                {
                    if (!ERM.stopSpawningGatherers)
                    {
                        if (CheckCosts(AIS.spawnOrder[0]))
                        {
                            Spawn_Unit(0);
                        }
                    }
                }
                else if (AIS.spawnOrder[0].unitType == 2 || (AIS.spawnOrder[0].unitType == 4 && AIS.spawnOrder[0].shipCanAttack))
                {
                    if (CheckCosts(AIS.spawnOrder[0]))
                    {
                        Spawn_Unit(0);
                    }
                }
            }
        }
    }

    public void FindERMWithSamePlayerID()
    {
        ERMS = FindObjectsOfType<enemyResourceManager>();

        foreach(enemyResourceManager E in ERMS)
        {
            if(E.player_ID == player_ID)
            {
                ERM = E;
            }
        }
    }

    public bool CheckCosts(enemyUnit spawnedUnit)
    {
        if (spawnedUnit.foodCost > ERM.food)
        {
            return false;
        }
        else if (spawnedUnit.woodCost > ERM.wood)
        {
            return false;
        }
        else if (spawnedUnit.goldCost > ERM.gold)
        {
            return false;
        }
        else if (spawnedUnit.stoneCost > ERM.stone)
        {
            return false;
        }
        else if (spawnedUnit.ironCost > ERM.iron)
        {
            return false;
        }

        return true;
    }

    public void AccountForUnitCost()
    {
        ERM.wood -= lastSpawned.woodCost;
        ERM.food -= lastSpawned.foodCost;
        ERM.gold -= lastSpawned.goldCost;
        ERM.stone -= lastSpawned.stoneCost;
        ERM.iron -= lastSpawned.ironCost;
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
        if (building.isBuilt)
        {
            enemies = player.AI_enemies;

            nearestEnemy = GetClosestEnemy(enemies);

            if (nearestEnemy)
            {
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
    }

    public void Upgrade()
    {
        if(buildingAge < AIS.Age)
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
        for (int i = 0; i < garrisonCapacity; i++)
        {
            if (garrisonedUnits.Count > 0)
            {
                garrisonedUnits[0].gameObject.SetActive(true);

                garrisonedUnits[0].garrisoned = false;

                if (rallyPoint != Vector3.zero)
                {
                    garrisonedUnits[0].agent.destination = rallyPoint;
                }

                garrisonedUnits.Remove(garrisonedUnits[0]);
            }
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
