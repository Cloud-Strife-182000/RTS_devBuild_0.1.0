using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyBuildManager : MonoBehaviour
{
    public GameObject[] buildings;
    public AI_TownHall AI_TH;
    public GameObject enemyTownHall;

    public int player_ID;

    public void CreateTownHall(Vector3 tempPos)
    {
        AI_TH = Instantiate(enemyTownHall, tempPos, enemyTownHall.transform.rotation).GetComponent<AI_TownHall>();

        Vector3 unit1Pos, unit2Pos;

        unit1Pos = new Vector3(tempPos.x + 10, 50, tempPos.z - 10);
        unit2Pos = new Vector3(tempPos.x - 10, 50, tempPos.z - 10);

        enemyUnit temp1 = Instantiate(AI_TH.GetComponent<enemyStructure>().spawnables[0], new Vector3(unit1Pos.x, Terrain.activeTerrain.SampleHeight(unit1Pos) + Terrain.activeTerrain.transform.position.y, unit1Pos.z), AI_TH.GetComponent<enemyStructure>().spawnables[0].transform.rotation);
        enemyUnit temp2 = Instantiate(AI_TH.GetComponent<enemyStructure>().spawnables[1], new Vector3(unit2Pos.x, Terrain.activeTerrain.SampleHeight(unit2Pos) + Terrain.activeTerrain.transform.position.y, unit2Pos.z), AI_TH.GetComponent<enemyStructure>().spawnables[1].transform.rotation);

        temp1.AI_St.player_ID = player_ID;
        temp2.AI_St.player_ID = player_ID;

        AI_TH.player_ID = player_ID;
        AI_TH.GetComponent<enemyStructure>().player_ID = player_ID;
        AI_TH.GetComponent<PlaceableBuilding>().playerID = player_ID;
    }

}
