using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScenarioManager : MonoBehaviour
{
    public Player_Unit unitToBeSelected;

    public SuperManager SM;
    public ActivePlayer player;

    private Vector3 posAboveTownHall, posAboveObjectToFocus;

    public List<GameObject> objectsToDestroy;

    public bool selectionFulfilled, travelFulfilled;
    public int initialNumOfObjsToDestroy, objectsRemaining, objectsDestroyed;

    private void Start()
    {
        posAboveTownHall = new Vector3(SM.playerTownHall.transform.position.x - 15, SM.playerTownHall.transform.position.y + 35, SM.playerTownHall.transform.position.z - 25);
        posAboveObjectToFocus = new Vector3(SM.objectToFocus.transform.position.x - 15, SM.objectToFocus.transform.position.y + 35, SM.objectToFocus.transform.position.z - 25);

        objectsToDestroy = GameObject.FindGameObjectsWithTag("objectToDestroy").ToList();
        initialNumOfObjsToDestroy = objectsToDestroy.Count;

        if(initialNumOfObjsToDestroy > 0)
        {
            StartCoroutine(CalculateCompletion());
        }
    }

    void Update()
    {
        if(SM.map_index == 0)
        {
            FirstTutorialScene();
        }
    }

    private void FirstTutorialScene()
    {
        ResetResources();

        if (!selectionFulfilled)
        {
            if (player.subsManager.currentSubtitleIndex == 3)
            {
                TogglePlayerUnits(unitToBeSelected.gameObject, false, false);
                TogglePlayerStructures(null, false, false);

                player.ToggleOffSubtitles();
                player.instructions.SetActive(true);
            }

            if (unitToBeSelected.isSelected)
            {
                selectionFulfilled = true;

                SM.playerTownHall.SetActive(true);

                SM.playerTownHall.GetComponent<Player_Structure>().canClearFog = true;

                SM.OBJS.objectives.text = SM.OBJS.listOfObjectives[SM.map_index].strings[1];

                player.instructions.SetActive(false);
                player.instructionManager.GoToNextInstruction();

                player.ToggleOnSubtitles();
            }
        }
        else if (!travelFulfilled)
        {
            if (player.subsManager.currentSubtitleIndex == 4)
            {
                player.transform.position = posAboveTownHall;
            }

            if (player.subsManager.currentSubtitleIndex == 5)
            {
                player.transform.position = posAboveObjectToFocus;
            }

            if (player.subsManager.currentSubtitleIndex == 7)
            {
                player.ToggleOffSubtitles();
                player.instructions.SetActive(true);
            }

            if (Vector3.Distance(SM.objectToFocus.transform.position, SM.playerTownHall.transform.position) <= 20f)
            {
                travelFulfilled = true;
                Player_Unit p;

                TogglePlayerUnits(unitToBeSelected.gameObject, true, true);
                TogglePlayerStructures(SM.playerTownHall, true, true);

                if (p = SM.objectToFocus.GetComponent<Player_Unit>())
                {
                    p.agent.destination = SM.objectToFocus.transform.position;
                }

                player.transform.position = new Vector3(SM.objectToFocus.transform.position.x - 15, SM.objectToFocus.transform.position.y + 35, SM.objectToFocus.transform.position.z - 25);

                SM.OBJS.objectives.text = SM.OBJS.listOfObjectives[SM.map_index].strings[2];
                SM.OBJS.enemyInfo.SetActive(true);

                foreach(GameObject go in GameObject.FindGameObjectsWithTag("clearFog"))
                {
                    player.fogOfWar.SetFogOfWarAlpha(go.transform.position, 20f, 0, 0, 0.85f);
                    player.minimapFogOfWar.SetFogOfWarAlpha(go.transform.position, 20f, 0, 0, 0.85f);
                }

                player.instructionManager.GoToNextInstruction();
                player.ToggleOnSubtitles();
            }
        }
    }

    private void TogglePlayerUnits(GameObject exception, bool on, bool allowToClearFog)
    {
        if (on)
        {
            foreach (Player_Unit pu in player.player_units)
            {
                if (pu)
                {
                    if (pu.gameObject != exception)
                    {
                        if (allowToClearFog)
                        {
                            pu.canClearFog = true;
                        }

                        pu.gameObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
            foreach (Player_Unit pu in player.player_units)
            {
                if (pu)
                {
                    if (pu.gameObject != exception)
                    {
                        pu.gameObject.SetActive(false);

                        if (allowToClearFog)
                        {
                            pu.canClearFog = true;
                        }
                    }
                }
            }
        }
    }

    private void TogglePlayerStructures(GameObject exception, bool on, bool allowToClearFog)
    {
        if (on)
        {
            foreach (Player_Structure ps in player.p_structs)
            {
                if (ps)
                {
                    if (ps.gameObject != exception)
                    {
                        if (allowToClearFog)
                        {
                            ps.canClearFog = true;
                        }

                        ps.gameObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
            foreach (Player_Structure ps in player.p_structs)
            {
                if (ps)
                {
                    if (ps.gameObject != exception)
                    {
                        ps.gameObject.SetActive(false);

                        if (allowToClearFog)
                        {
                            ps.canClearFog = true;
                        }
                    }
                }
            }
        }
    }

    private void ResetResources()
    {
        SM.RM.food = 0;
        SM.RM.wood = 0;
        SM.RM.gold = 0;
        SM.RM.stone = 0;
        SM.RM.iron = 0;
    }

    IEnumerator CalculateCompletion()
    {
        while (true)
        {
            objectsRemaining = objectsToDestroy.Count;

            objectsDestroyed = initialNumOfObjsToDestroy - objectsRemaining;

            yield return new WaitForSeconds(1f);
        }
    }
}
