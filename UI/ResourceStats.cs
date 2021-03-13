using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceStats : MonoBehaviour
{
    public bool food, wood, gold, stone, iron;
    public bool currentPopulation, populationCapacity;

    public bool totalEnemies, enemiesKilled, enemiesRemaining;

    private ActivePlayer player;
    private ResourceManager RM;
    private ScenarioManager scenM;
    private Text txt;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<ActivePlayer>();

        RM = player.GetComponent<ResourceManager>();

        scenM = player.superManager.scenarioManager;

        txt = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateResourceStats();
    }

    private void UpdateResourceStats()
    {
        if (food)
        {
            txt.text = RM.food.ToString();
        }
        else if (wood)
        {
            txt.text = RM.wood.ToString();
        }
        else if (gold)
        {
            txt.text = RM.gold.ToString();
        }
        else if (stone)
        {
            txt.text = RM.stone.ToString();
        }
        else if (iron)
        {
            txt.text = RM.iron.ToString();
        }
        else if (currentPopulation)
        {
            txt.text = player.currentPopulation.ToString();
        }
        else if (populationCapacity)
        {
            txt.text = player.populationCapacity.ToString();
        }
        else if(totalEnemies)
        {
            txt.text = scenM.initialNumOfObjsToDestroy.ToString();
        }
        else if (enemiesKilled)
        {
            txt.text = scenM.objectsDestroyed.ToString();
        }
        else if (enemiesRemaining)
        {
            txt.text = scenM.objectsRemaining.ToString();
        }
    }
}
