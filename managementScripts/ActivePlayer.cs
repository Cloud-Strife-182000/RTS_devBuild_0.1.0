using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricFogAndMist;
using UnityEngine.UI;

public class ActivePlayer : MonoBehaviour
{
    public string username;
    public bool human;
    public int Age;
    public HandleCursor cursor;

    public int currentPopulation, populationCapacity;

    public bool paused;
    public Canvas existingCanvasOpen;

    public GameObject victory, defeat;

    public UserControl UC;
    public objectSelection OS;
    public ResourceManager RM;
    public BuildingPlacement BP;

    public List<enemyUnit> enemyUnits;
    public List<PlaceableBuilding> placeableBuildings;
    public List<Player_Unit> player_units;
    public List<NodeManager> resources;
    public List<Drops> dropSites;
    public List<enemyDropSite> enemyDropSites;
    public List<Player_Structure> p_structs;
    public List<enemyStructure> e_structs;

    public List<GameObject> townHalls;

    public List<GameObject> player_enemies, animal_enemies, AI_enemies, NPC_enemies;
    public VolumetricFog fogOfWar, secondaryFogOfWar, minimapFogOfWar;

    public GameObject[] overlayWindows;
    public GameObject minimapCam, minimapViewportCam;

    public GameObject subtitles, instructions;
    public SubtitleManager subsManager;
    public SubtitleManager instructionManager;

    public List<bool> researched;
    public List<Attributes> techs;
    public GameObject availableTechs;

    public Light globalLight;
    public SuperManager superManager;

    private void Awake()
    {
        foreach(Attributes attr in availableTechs.GetComponentsInChildren<Attributes>())
        {
            techs[attr.researchIndex] = attr;
        }

        Application.targetFrameRate = 60;

        StartCoroutine(CheckForVictory());
    }

    public void TriggerUnlocks()
    {
        foreach (Player_Unit PU in player_units)
        {
            if (PU)
            {
                PU.UC.ActivateAgeSpecificButtons(PU.UC.militaryBuildActions);
                PU.UC.ActivateAgeSpecificButtons(PU.UC.economicBuildActions);
            }
        }
    }

    public void TriggerBuildingUpgrades()
    {
        if (BP.isPlacing)
        {
            GameObject temp = BP.placeableBuilding.PS.nextAgeReplacement;

            if (temp)
            {
                placeableBuildings.Remove(BP.placeableBuilding);
                Destroy(BP.currentBuilding.gameObject);
                BP.SetItem(temp);
            }
        }

        foreach(Player_Structure PS in p_structs)
        {
            if (PS)
            {
                if (PS.building.isBuilt)
                {
                    PS.Upgrade();
                }
            }
        }
    }

    public void TriggerUnitUpgrades(int index)
    {
        int next_index = 0;
        int nextListIndex = 0;
        int currentListIndex = 0;

        for(int i = 0; i < player_units.Count; i++)
        {
            if (player_units[i])
            {
                if(player_units[i].unitIndex == index)
                {
                    player_units[i].Upgrade();

                    if(next_index == 0)
                    {
                        next_index = player_units[i].nextUpgrade.GetComponent<Player_Unit>().unitIndex;
                    }

                    Destroy(player_units[i].gameObject);
                }
            }
        }

        foreach(Player_Structure PS in p_structs)
        {
            bool canSpawnCurrentUnit = false;

            if(PS.spawnables.Count > 0)
            {
                for (int i = 0; i < PS.spawnables.Count; i++)
                {
                    if (PS.spawnables[i])
                    {
                        if (PS.spawnables[i].unitIndex == index)
                        {
                            currentListIndex = i;

                            canSpawnCurrentUnit = true;

                            PS.spawnables[currentListIndex] = null;
                        }
                    }

                    if (PS.spawnables[i])
                    {
                        if (PS.spawnables[i].unitIndex == next_index)
                        {
                            nextListIndex = i;
                        }
                    }
                }
            }

            if (canSpawnCurrentUnit)
            {
                Sprite prevSprite = null;
                Sprite nextSprite = null;

                foreach(RTSButton b in PS.UC.frontPanelActions)
                {
                    if(b.index == currentListIndex)
                    {
                        prevSprite = b.button.image.sprite;
                        nextSprite = b.nextUpgradeSprite;
                        b.button.image.sprite = b.nextUpgradeSprite;
                        b.index = nextListIndex;
                        b.spawnRate = PS.spawnables[nextListIndex].spawnIncrement;
                        b.spawnIncrement = PS.spawnables[nextListIndex].spawnRate;
                    }
                }

                foreach(RTSButton b in PS.taskQueue)
                {
                    if(b.index == currentListIndex)
                    {
                        b.index = nextListIndex;
                    }
                }

                foreach(Image img in PS.queueImages)
                {
                    if (prevSprite)
                    {
                        if(img.sprite == prevSprite)
                        {
                            img.sprite = nextSprite;
                        }
                    }
                }
            }
        }
    }

    public void ResetPlayer()
    {
        RM.food = 200;
        RM.wood = 200;
        RM.stone = 200;
        RM.gold = 200;
        RM.iron = 200;

        placeableBuildings.Clear();
        resources.Clear();
        dropSites.Clear();

        for (int i = 0; i < player_units.Count; i++)
        {
            Player_Unit temp = player_units[i];

            if (temp)
            {
                player_units.Remove(temp);
                Destroy(temp.gameObject);
            }
        }

        for (int i = 0; i < p_structs.Count; i++)
        {
            Player_Structure temp = p_structs[i];

            if (temp)
            {
                p_structs.Remove(temp);
                Destroy(temp.gameObject);
            }
        }

        for (int i = 0; i < researched.Count; i++)
        {
            researched[i] = false;
        }

        Age = 0;
        populationCapacity = 50;
        currentPopulation = 0;

        for (int i = 0; i < enemyUnits.Count; i++)
        {
            enemyUnit temp = enemyUnits[i];

            if (temp)
            {
                enemyUnits.Remove(temp);
                Destroy(temp.gameObject);
            }
            
        }

        for (int i = 0; i < e_structs.Count; i++)
        {
            enemyStructure temp = e_structs[i];

            if (temp)
            {
                e_structs.Remove(temp);
                Destroy(temp.gameObject);
            }
        }

        player_units.Clear();
        enemyUnits.Clear();
        p_structs.Clear();
        e_structs.Clear();
        animal_enemies.Clear();
        player_enemies.Clear();
        NPC_enemies.Clear();
        AI_enemies.Clear();
        enemyDropSites.Clear();
        townHalls.Clear();
    }

    public void ToggleOnSubtitles()
    {
        SwitchOffOverlayWindows();

        if (!subtitles.activeSelf)
        {
            subtitles.SetActive(true);
        }
    }

    public void ToggleOffSubtitles()
    {
        if (subtitles.activeSelf)
        {
            subtitles.SetActive(false);

            OS.isSelecting = false;
        }
    }

    private void SwitchOffOverlayWindows()
    {
        if (existingCanvasOpen)
        {
            existingCanvasOpen.gameObject.SetActive(false);
        }

        foreach (GameObject go in overlayWindows)
        {
            if (go.activeSelf)
            {
                go.SetActive(false);
            }
        }

        minimapViewportCam.SetActive(false);
        minimapCam.SetActive(false);
    }

    IEnumerator CheckForVictory()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (superManager.scenarioManager)
            {
                if(superManager.scenarioManager.initialNumOfObjsToDestroy > 0)
                {
                    if(superManager.scenarioManager.objectsRemaining == 0)
                    {
                        Time.timeScale = 0;

                        victory.SetActive(true);
                    }
                }

                if (superManager.scenarioManager.unitToBeSelected.death && !victory.activeSelf)
                {
                    Time.timeScale = 0;

                    defeat.SetActive(true);
                }
            }
            else
            {
                if(townHalls.Count > 0)
                {
                    if (townHalls.Count == 1)
                    {
                        Time.timeScale = 0;

                        victory.SetActive(true);
                    }

                    if (!townHalls[0])
                    {
                        Time.timeScale = 0;

                        defeat.SetActive(true);
                    }
                }
            }
        }
    }
}
