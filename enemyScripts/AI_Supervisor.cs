using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AI_Supervisor : MonoBehaviour
{
    public int currentPopulation, populationCapacity;
    public int Age;
    public enemyResourceManager ERM;
    public enemyBuildManager EBM;
    public int player_ID;

    public ActivePlayer player;
    public SuperManager SM;
    public Scanner scanner;

    public Collider[] hitColliders;

    public List<GameObject> buildOrder;
    public List<Vector3> buildPositions;

    public List<Attributes> researchOrder;

    public List<enemyUnit> units;
    public List<enemyUnit> spawnOrder;

    private AI_TownHall AI_TH;

    public List<enemyUnit> gatherers, combatants;
    public int difference;
    public int initialSpawnGathererLimit = 0;

    public float[] gathererCountByType = new float[5];
    public bool[] targetResource = new bool[5];
    public float[,] gathererRatios = new float[5,5];

    public List<bool> researched;
    public List<Attributes> techs;
    public GameObject availableTechs;

    public bool commandedToInvade;
    public int TownHallIndex;
    public List<GameObject> otherTownHalls;
    public GameObject targetTownHall;
    public bool[] ageInvasions = new bool[4];

    private int i;
    private WaitForSeconds wf1s = new WaitForSeconds(1f);
    public WaitForSeconds wf5s = new WaitForSeconds(5f);

    private void Awake()
    {
        player = FindObjectOfType<ActivePlayer>();
        SM = player.superManager;

        foreach(Attributes attr in availableTechs.GetComponentsInChildren<Attributes>())
        {
            techs[attr.researchIndex] = attr;
        }
        
        scanner = Terrain.activeTerrain.GetComponent<Scanner>();

        player_ID = SM.numSupervisors;

        ERM = GetComponent<enemyResourceManager>();
        EBM = GetComponent<enemyBuildManager>();

        ERM.player_ID = player_ID;
        EBM.player_ID = player_ID;

        float spawnX = Random.Range(0f, 1000f);
        float spawnZ = Random.Range(0f, 1000f);

        Vector3 tempSpawnPoint = new Vector3(spawnX, 50f, spawnZ);

        EBM.CreateTownHall(GetClosestEligibleBuildPoint(tempSpawnPoint, SM.spawnPoints, scanner.eligibleTerrainPoints, 300f, 1000f, 8f));

        AI_TH = EBM.AI_TH;

        SM.CreateResources(AI_TH.transform.position, 40, 70);

        TownHallIndex = player.townHalls.Count;
        player.townHalls.Add(AI_TH.gameObject);

        FarmRush();
    }

    void Start()
    {
        for(i = 0; i < player.townHalls.Count; i++)
        {
            if(i != TownHallIndex)
            {
                otherTownHalls.Add(player.townHalls[i]);
            }
        }

        for(i = 0; i < spawnOrder.Count; i++)
        {
            if(spawnOrder[i].unitType != 1)
            {
                break;
            }

            initialSpawnGathererLimit++;
        }

        initialSpawnGathererLimit += 2;

        StartCoroutine(CheckStatus());
        StartCoroutine(UpdateInfo());
        StartCoroutine(PeriodicResearch());
        StartCoroutine(PeriodicInvasions());
        StartCoroutine(UpdateBuildOrder());
    }

    private void FarmRush()
    {
        AddSpawn(0, 2);
        AddSpawn(1, 2);
        AddSpawn(2, 5);
        AddSpawn(11, 5);
        AddSpawn(23, 5);
        AddSpawn(0, 2);
        AddSpawn(1, 2);
        AddSpawn(29, 1);
        AddSpawn(31, 1);
        AddSpawn(2, 5);
        AddSpawn(11, 5);
        AddSpawn(23, 5);

        AddBuilding(5, AI_TH.gameObject, 0, 0);
        AddBuilding(0, AI_TH.gameObject, -10, -15);
        AddBuilding(0, AI_TH.gameObject, -10, 0);
        AddBuilding(0, AI_TH.gameObject, -10, 15);
        AddBuilding(0, AI_TH.gameObject, 10, 0);

        AddBuilding(9, AI_TH.gameObject, 20, 0);
        AddBuilding(6, AI_TH.gameObject, 20, -15);
        AddBuilding(6, AI_TH.gameObject, 20, -15);
        AddBuilding(6, AI_TH.gameObject, 20, -15);
        AddBuilding(8, AI_TH.gameObject, 20, 15);
        AddBuilding(4, AI_TH.gameObject, 15, 20);
        AddBuilding(3, AI_TH.gameObject, -20, 15);

        AddBuilding(2, AI_TH.gameObject, -10, -15);
        AddBuilding(0, AI_TH.gameObject, -10, 0);
        AddBuilding(0, AI_TH.gameObject, -10, 0);
        AddBuilding(9, AI_TH.gameObject, 20, 10);

        AddResearch(1);
        AddResearch(2);
        AddResearch(3);
        AddResearch(5);
        AddResearch(6);
        AddResearch(8);
        AddResearch(9);
        AddResearch(10);
    }

    void AddBuilding(int index, GameObject origin, float x, float z)
    {
        buildOrder.Add(EBM.buildings[index]);
        buildPositions.Add(new Vector3(origin.transform.position.x + x, 50f, AI_TH.transform.position.z + z));
    }

    void AddSpawn(int index, int num)
    {
        if(num == 1)
        {
            spawnOrder.Add(units[index]);
        }
        else
        {
            for(int i=0; i < num; i++)
            {
                spawnOrder.Add(units[index]);
            }
        }
    }

    void AddResearch(int index)
    {
        researchOrder.Add(techs[index]);
    }

    IEnumerator CheckStatus()
    {
        while (true)
        {
            yield return wf1s;

            if (!AI_TH)
            {
                player.townHalls.Remove(player.townHalls[player_ID]);

                for(int i = 0; i < player.enemyUnits.Count; i++)
                {
                    if(player.enemyUnits[i].player_ID == player_ID)
                    {
                        player.enemyUnits[i].deathOverride = true;
                    }
                }

                for(int i=0; i < player.e_structs.Count; i++)
                {
                    if(player.e_structs[i].player_ID == player_ID)
                    {
                        player.e_structs[i].deathOverride = true;
                    }
                }

                Destroy(gameObject);
            }
        }
    }

    IEnumerator UpdateInfo()
    {
        while (true)
        {
            yield return wf1s;

            for(int i = 0; i < 5; i++)
            {
                gathererCountByType[i] = 0;
            }

            difference = gatherers.Count - combatants.Count;

            if(difference >= initialSpawnGathererLimit)
            {
                ERM.stopSpawningGatherers = true;
            }

            foreach(enemyUnit ga in gatherers)
            {
                if (ga.foodGatherer)
                {
                    gathererCountByType[0]++;
                }
                else if (ga.woodGatherer)
                {
                    gathererCountByType[1]++;
                }
                else if (ga.goldGatherer)
                {
                    gathererCountByType[2]++;
                }
                else if (ga.stoneGatherer)
                {
                    gathererCountByType[3]++;
                }
                else if (ga.ironGatherer)
                {
                    gathererCountByType[4]++;
                }
            }

            for(int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if(gathererCountByType[j] > 0)
                    {
                        gathererRatios[i,j] = gathererCountByType[i] / gathererCountByType[j];
                    }
                    else
                    {
                        gathererRatios[i, j] = 0;
                    }
                    
                }
            }

            if(gathererCountByType[0] <= 5)
            {
                if (gathererCountByType[1] - gathererCountByType[0] >= 2)
                {
                    targetResource[0] = true;
                    targetResource[1] = false;
                }
                else if(gathererCountByType[0] - gathererCountByType[1] >= 2)
                {
                    targetResource[0] = false;
                    targetResource[1] = true;
                }
                else
                {
                    targetResource[0] = false;
                    targetResource[1] = false;
                }
            }
            else
            {
                if(gathererCountByType[0] - gathererCountByType[2] >= 3)
                {
                    targetResource[0] = false;
                    targetResource[1] = false;
                    targetResource[2] = true;
                }
            }
        }
    }

    public bool CheckResearchCosts(Attributes att)
    {
        if (att.foodCost > ERM.food)
        {
            return false;
        }
        else if (att.woodCost > ERM.wood)
        {
            return false;
        }
        else if (att.goldCost > ERM.gold)
        {
            return false;
        }
        else if (att.stoneCost > ERM.stone)
        {
            return false;
        }
        else if (att.ironCost > ERM.iron)
        {
            return false;
        }

        return true;
    }

    public void ResearchImprovements(Attributes attr)
    {
        if (attr.forUnit)
        {
            foreach (enemyUnit eu in player.enemyUnits)
            {
                if (eu)
                {
                    if (eu.unitType == attr.unitType)
                    {
                        if (attr.attackType == 0)
                        {
                            eu.HB.maxHealth += attr.healthModifier;
                            eu.HB.currentHealth += attr.healthModifier;
                            eu.MaxHeldResource += attr.gatheringCapacityModifier;
                            eu.damage += attr.damageModifier;
                            eu.gatheringRate += attr.gatheringRateModifier;
                            eu.buildStrength += attr.buildingStrengthModifier;
                        }
                        else if (eu.attackType == attr.attackType)
                        {
                            eu.HB.maxHealth += attr.healthModifier;
                            eu.HB.currentHealth += attr.healthModifier;
                            eu.MaxHeldResource += attr.gatheringCapacityModifier;
                            eu.damage += attr.damageModifier;
                            eu.gatheringRate += attr.gatheringRateModifier;
                            eu.buildStrength += attr.buildingStrengthModifier;
                        }
                    }
                }
            }
        }
        else if (attr.forBuilding)
        {
            foreach (enemyStructure ES in player.e_structs)
            {
                if (ES.building.EDS)
                {
                    if (attr.resourceToTarget == NodeManager.ResourceTypes.Food)
                    {
                        if (ES.building.EDS.Food)
                        {
                            ES.HB.maxHealth += attr.healthModifier;
                            ES.HB.currentHealth += attr.healthModifier;
                        }
                    }
                    else if (attr.resourceToTarget == NodeManager.ResourceTypes.Wood)
                    {
                        if (ES.building.EDS.Wood)
                        {
                            ES.HB.maxHealth += attr.healthModifier;
                            ES.HB.currentHealth += attr.healthModifier;
                        }
                    }
                    else if (attr.resourceToTarget == NodeManager.ResourceTypes.Gold)
                    {
                        if (ES.building.EDS.Gold)
                        {
                            ES.HB.maxHealth += attr.healthModifier;
                            ES.HB.currentHealth += attr.healthModifier;
                        }
                    }
                    else if (attr.resourceToTarget == NodeManager.ResourceTypes.Stone)
                    {
                        if (ES.building.EDS.Stone)
                        {
                            ES.HB.maxHealth += attr.healthModifier;
                            ES.HB.currentHealth += attr.healthModifier;
                        }
                    }
                    else if (attr.resourceToTarget == NodeManager.ResourceTypes.Iron)
                    {
                        if (ES.building.EDS.Iron)
                        {
                            ES.HB.maxHealth += attr.healthModifier;
                            ES.HB.currentHealth += attr.healthModifier;
                        }
                    }
                }
                else if (ES.building.nm)
                {
                    if (ES.building.nm.resourceType == attr.resourceToTarget)
                    {
                        ES.building.nm.availableResource += attr.resourceAmountModifier;
                        ES.HB.maxHealth += attr.healthModifier;
                        ES.HB.currentHealth += attr.healthModifier;
                    }
                }
            }
        }
        else
        {
            if(attr.unitIndex > 0)
            {
                TriggerEnemyUnitUpgrades(attr.unitIndex);
            }
        }
    }

    public void AccountForResearchCosts(Attributes attr)
    {
        ERM.food -= attr.foodCost;
        ERM.wood -= attr.woodCost;
        ERM.gold -= attr.goldCost;
        ERM.stone -= attr.stoneCost;
        ERM.iron -= attr.ironCost;
    }

    IEnumerator PeriodicResearch()
    {
        while (true)
        {
            yield return wf5s;

            if (researchOrder.Count > 0)
            {
                if (CheckResearchCosts(researchOrder[0]))
                {
                    AccountForResearchCosts(researchOrder[0]);

                    ResearchImprovements(researchOrder[0]);

                    researched[researchOrder[0].researchIndex] = true;
                    researchOrder.Remove(researchOrder[0]);
                }
            }
        }
    }

    public bool ColliderCheckOnPos(Vector3 posToCheck, float sphereRadius)
    {
        hitColliders = Physics.OverlapSphere(posToCheck, sphereRadius);

        if (hitColliders.Length > 0)
        {
            foreach (Collider c in hitColliders)
            {
                if (c.GetComponent<PlaceableBuilding>() || c.GetComponent<NodeManager>() || c.GetComponent<RTSWater>())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Vector3 GetClosestEligibleBuildPoint(Vector3 posn, List<Vector3> origins, List<Vector3> listofPoints, float minimumDistance, float maximumDistance, float sphereRadius)
    {
        float minDistance = Mathf.Infinity;
        Vector3 closestPoint = new Vector3(0, 0, 0);

        foreach (Vector3 Pos in listofPoints)
        {
            bool eligible = true;
            float distFromPt = Vector3.Distance(posn, Pos);

            foreach (Vector3 origin in origins)
            {
                float dist = Vector3.Distance(origin, Pos);

                if (dist < minimumDistance || dist > maximumDistance)
                {
                    eligible = false;
                }
            }

            if (distFromPt < minDistance && eligible)
            {
                if (!ColliderCheckOnPos(Pos, sphereRadius))
                {
                    minDistance = distFromPt;
                    closestPoint = Pos;
                }
            }
        }

        return closestPoint;
    }

    IEnumerator PeriodicInvasions()
    {
        while (true)
        {
            yield return wf5s;

            Invade(0, 0, 10);

            if (ageInvasions[1] && !commandedToInvade)
            {
                Invade(0, 1, 20);
            }

            if(ageInvasions[2] && !commandedToInvade)
            {
                Invade(1, 2, 30);
            }

            if (ageInvasions[3] && !commandedToInvade)
            {
                Invade(1, 3, 40);
            }
        }
    }

    private void Invade(int age, int index, int numCombatants)
    {
        if (Age == age)
        {
            if (!ageInvasions[index])
            {
                if (combatants.Count >= numCombatants)
                {
                    /*int randomIndex;

                    if (otherTownHalls.Count > 1)
                    {
                        randomIndex = Random.Range(0, otherTownHalls.Count);
                    }
                    else
                    {
                        randomIndex = 0;
                    }*/

                    targetTownHall = otherTownHalls[0]; //invades only player base for now
                     
                    foreach (enemyUnit combatant in combatants)
                    {
                        combatant.commandedToInvade = true;

                        combatant.invadeTargetPosition = targetTownHall.transform.position;
                    }

                    commandedToInvade = true;
                    ageInvasions[index] = true;
                }
            }
        }
    }

    IEnumerator UpdateBuildOrder()
    {
        while (true)
        {
            if (currentPopulation >= populationCapacity)
            {
                buildOrder.Add(EBM.buildings[6]);

                buildPositions.Add(AI_TH.transform.position);

                for(int i = 1; i<buildOrder.Count-1; i++)
                {
                    buildOrder[i + 1] = buildOrder[i];
                }

                for (int i = 1; i < buildPositions.Count - 1; i++)
                {
                    buildPositions[i + 1] = buildPositions[i];
                }

                buildOrder[0] = EBM.buildings[6];

                buildPositions[0] = AI_TH.transform.position;
            }

            yield return new WaitForSeconds(210);
        }
    }

    public void TriggerEnemyUnitUpgrades(int index)
    {
        int next_index = 0;

        for (int i = 0; i < player.enemyUnits.Count; i++)
        {
            if (player.enemyUnits[i])
            {
                if (player.enemyUnits[i].unitIndex == index)
                {
                    player.enemyUnits[i].Upgrade();

                    if(next_index == 0)
                    {
                        next_index = player.enemyUnits[i].nextUpgrade.GetComponent<enemyUnit>().unitIndex;
                    }

                    Destroy(player.enemyUnits[i].gameObject);
                }
            }
        }

        for(int i = 0; i < spawnOrder.Count; i++)
        {
            if (spawnOrder[i])
            {
                if(spawnOrder[i].unitIndex == index)
                {
                    spawnOrder[i] = units[next_index];
                }
            }
        }

        foreach (enemyStructure ES in player.e_structs)
        {
            if (ES.spawnables.Count > 0)
            {
                for (int i = 0; i < ES.spawnables.Count; i++)
                {
                    if (ES.spawnables[i])
                    {
                        if(ES.spawnables[i].unitIndex == index)
                        {
                            ES.spawnables[i] = null;
                        }
                    }
                }
            }
        }
    }
}
