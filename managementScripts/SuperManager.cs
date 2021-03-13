using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using HSVPicker;

public class SuperManager : MonoBehaviour
{
    private Bloom bloom = null;

    [Header("Load on Play Settings")]

    public ActivePlayer mainPlayer;
    public ColorPicker colorPicker;
    //public bool changeTeamColorDuringRuntime;
    public ResourceManager RM;
    public Objectives OBJS;
    public ScenarioManager scenarioManagerPrefab, scenarioManager;

    public GameObject loadingScreen;
    public bool isTutorialScene;

    public List<BookPro> tutorialCampaignBooks;

    public GameObject userInterface, mainMenu;

    public GameObject[] subMenus;

    public GameObject General_ENV;

    public GameObject[] tut_maps;
    public GameObject[] tut_map_ENVS;

    public GameObject[] maps;
    public GameObject[] map_ENVS;

    public GameObject AI;

    public List<GameObject> AIS;
    public int numSupervisors = 0;

    public GameObject activeMap, activeEnv;

    public List<Vector3> spawnPoints;
    public Vector3 playerSpawnPoint;

    public GameObject[] resourcePrefabs;
    public List<GameObject> extras;

    private float spawnX, spawnZ;
    private Collider[] hitColliders;
    private Scanner scanner;

    public GameObject[] startingSetup;
    public GameObject playerTownHall, objectToFocus;

    [Header("Game Settings")]

    public int map_index;
    public int numOfAIs = 1;
    public List<Color> playerColours;

    [Header("Graphics Settings")]

    public bool shadows;
    public bool setBloom;
    public bool setFog = true;

    private void Start()
    {
        PostProcessVolume PPV = Camera.main.GetComponent<PostProcessVolume>();
        PPV.profile.TryGetSettings(out bloom);

        colorPicker.CurrentColor = Color.yellow;
        playerColours[0] = Color.yellow;

        if (setBloom)
        {
            bloom.enabled.value = true;
        }

        if (shadows)
        {
            mainPlayer.globalLight.shadows = LightShadows.Hard;
        }
    }

    private void ToggleFogPreference()
    {
        if (setFog)
        {
            mainPlayer.fogOfWar.gameObject.SetActive(true);
            mainPlayer.secondaryFogOfWar.gameObject.SetActive(true);
            mainPlayer.minimapFogOfWar.gameObject.SetActive(true);

            mainPlayer.fogOfWar.ResetFogOfWar();
            mainPlayer.secondaryFogOfWar.ResetFogOfWar();
            mainPlayer.minimapFogOfWar.ResetFogOfWar();
        }
        else
        {
            mainPlayer.fogOfWar.gameObject.SetActive(false);
            mainPlayer.secondaryFogOfWar.gameObject.SetActive(false);
            mainPlayer.minimapFogOfWar.gameObject.SetActive(false);
        }
    }

    public void LoadTutorialScene()
    {
        General_ENV.SetActive(true);

        ToggleFogPreference();

        activeMap = Instantiate(tut_maps[map_index], Vector3.zero, tut_maps[map_index].transform.rotation);

        activeEnv = Instantiate(tut_map_ENVS[map_index], tut_maps[map_index].transform.position, maps[map_index].transform.rotation);

        playerTownHall = GameObject.FindGameObjectWithTag("Townhall");
        objectToFocus = GameObject.FindGameObjectWithTag("ObjectToFocus");

        transform.position = new Vector3(objectToFocus.transform.position.x - 15, objectToFocus.transform.position.y + 35, objectToFocus.transform.position.z - 25);
        transform.rotation = mainPlayer.UC.rotation;

        userInterface.SetActive(true);

        OBJS.objectives.text = OBJS.listOfObjectives[map_index].strings[0];

        InitialiseSubtitles();

        scenarioManager = Instantiate(scenarioManagerPrefab, Vector3.zero, new Quaternion(0,0,0,0));
        scenarioManager.unitToBeSelected = objectToFocus.GetComponent<Player_Unit>();
        scenarioManager.player = mainPlayer;
        scenarioManager.SM = this;

        mainMenu.SetActive(false);
    }

    public void LoadScene()
    {
        General_ENV.SetActive(true);

        ToggleFogPreference();

        activeMap = Instantiate(maps[map_index], Vector3.zero, maps[map_index].transform.rotation);

        activeEnv = Instantiate(map_ENVS[map_index], maps[map_index].transform.position, maps[map_index].transform.rotation);

        scanner = Terrain.activeTerrain.GetComponent<Scanner>();

        spawnX = Random.Range(0f, 1000f);
        spawnZ = Random.Range(0f, 1000f);

        Vector3 tempSpawnPoint = new Vector3(spawnX, 50f, spawnZ);

        playerSpawnPoint = GetClosestEligibleBuildPoint(scanner.eligibleTerrainPoints, tempSpawnPoint, 8f);

        spawnPoints.Add(playerSpawnPoint);

        transform.position = new Vector3(playerSpawnPoint.x - 15, playerSpawnPoint.y + 35, playerSpawnPoint.z - 36);
        transform.rotation = mainPlayer.UC.rotation;

        Vector3 unit1spawnpos = new Vector3(playerSpawnPoint.x - 10, 50, playerSpawnPoint.z - 10);
        Vector3 unit2spawnpos = new Vector3(playerSpawnPoint.x + 10, 50, playerSpawnPoint.z - 10);

        mainPlayer.townHalls.Add(Instantiate(startingSetup[0], playerSpawnPoint, startingSetup[0].transform.rotation));
        Instantiate(startingSetup[1], new Vector3(unit1spawnpos.x, Terrain.activeTerrain.SampleHeight(unit1spawnpos) + Terrain.activeTerrain.transform.position.y, unit1spawnpos.z), startingSetup[0].transform.rotation);
        Instantiate(startingSetup[2], new Vector3(unit2spawnpos.x, Terrain.activeTerrain.SampleHeight(unit2spawnpos) + Terrain.activeTerrain.transform.position.y, unit2spawnpos.z), startingSetup[0].transform.rotation);

        CreateResources(playerSpawnPoint, 40, 70);

        for (int i = 0; i < numOfAIs; i++)
        {
            numSupervisors++;
            AIS.Add(Instantiate(AI.gameObject));
        }

        userInterface.SetActive(true);

        mainPlayer.ToggleOffSubtitles();
        mainPlayer.instructions.SetActive(false);

        OBJS.objectives.text = OBJS.defaultObjective;

        mainMenu.SetActive(false);
    }

    public void SetShadows()
    {
        if (!shadows)
        {
            mainPlayer.globalLight.shadows = LightShadows.Hard;
            shadows = true;
        }
        else
        {
            mainPlayer.globalLight.shadows = LightShadows.None;
            shadows = false;
        }
    }

    public void SetBloom()
    {
        if (!setBloom)
        {
            bloom.enabled.value = true;
            setBloom = true;
        }
        else
        {
            bloom.enabled.value = false;
            setBloom = false;
        }
    }

    public void AdjustBloom(float val)
    {
        bloom.intensity.value = val;
    }

    public float AssignBloom()
    {
        return bloom.intensity.value;
    }

    public void SetFog()
    {
        if (!setFog)
        {
            setFog = true;
        }
        else
        {
            setFog = false;
        }
    }

    public void ExitToMainMenu()
    {
        Destroy(activeMap);

        userInterface.SetActive(false);

        foreach(GameObject submenu in subMenus)
        {
            if (submenu.activeSelf)
            {
                submenu.SetActive(false);
            }
        }

        foreach(GameObject ais in AIS)
        {
            Destroy(ais);
        }

        numSupervisors = 0;

        mainPlayer.ResetPlayer();

        Destroy(activeEnv);

        for(int i = 0; i < extras.Count; i++)
        {
            Destroy(extras[i]);
        }

        General_ENV.SetActive(false);

        mainMenu.SetActive(true);

        foreach(enemyUnit leftover in FindObjectsOfType<enemyUnit>())
        {
            Destroy(leftover.gameObject);
        }

        foreach(enemyStructure leftover in FindObjectsOfType<enemyStructure>())
        {
            Destroy(leftover.gameObject);
        }

        foreach(Player_Unit leftover in FindObjectsOfType<Player_Unit>())
        {
            Destroy(leftover.gameObject);
        }

        foreach(Player_Structure leftover in FindObjectsOfType<Player_Structure>())
        {
            Destroy(leftover.gameObject);
        }

        if (scenarioManager)
        {
            Destroy(scenarioManager.gameObject);

            foreach(BookPro book in tutorialCampaignBooks)
            {
                book.currentPaper = 0;
                book.UpdatePages();
            }
        }

        OBJS.enemyInfo.SetActive(false);

        colorPicker.CurrentColor = Color.yellow;
        playerColours[0] = Color.yellow;

        mainPlayer.victory.SetActive(false);
        mainPlayer.defeat.SetActive(false);
        Time.timeScale = 1;

        mainPlayer.cursor.SetMouse();
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

    public Vector3 GetClosestEligibleBuildPoint(List<Vector3> listofPoints, Vector3 posn, float sphereRadius)
    {
        float minDistance = Mathf.Infinity;
        Vector3 closestPoint = new Vector3(0, 0, 0);

        foreach (Vector3 Pos in listofPoints)
        {
            float distFromPt = Vector3.Distance(posn, Pos);

            if (distFromPt < minDistance)
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

    public void CreateResources(Vector3 origin, float minimum, float maximum)
    {
        for (int i = 0; i < resourcePrefabs.Length; i++)
        {
            float negX = Random.Range(-maximum, -minimum);
            float negZ = Random.Range(-maximum, -minimum);

            float posX = Random.Range(minimum, maximum);
            float posZ = Random.Range(minimum, maximum);

            float randX = Random.value;
            float randZ = Random.value;

            float X, Z;

            if (randX <= 0.5f)
            {
                X = origin.x + negX;
            }
            else
            {
                X = origin.x + posX;
            }

            if (randZ <= 0.5f)
            {
                Z = origin.z + negZ;
            }
            else
            {
                Z = origin.z + posZ;
            }

            Vector3 temp = new Vector3(X, 50f, Z);

            extras.Add(Instantiate(resourcePrefabs[i], GetClosestEligibleBuildPoint(scanner.eligibleTerrainPoints, temp, 6f), resourcePrefabs[i].transform.rotation));
        }
    }

    private void InitialiseSubtitles()
    {
        mainPlayer.subsManager.subtitleListIndex = map_index;
        mainPlayer.subsManager.currentSubtitleIndex = 0;
        mainPlayer.subsManager.subtitleText.text = mainPlayer.subsManager.subtitleList[mainPlayer.subsManager.subtitleListIndex].strings[0];
        mainPlayer.ToggleOnSubtitles();
        mainPlayer.instructionManager.subtitleListIndex = map_index;
        mainPlayer.instructionManager.currentSubtitleIndex = 0;
        mainPlayer.instructionManager.subtitleText.text = mainPlayer.instructionManager.subtitleList[mainPlayer.instructionManager.subtitleListIndex].strings[0];
        mainPlayer.instructions.SetActive(false);
    }
}
