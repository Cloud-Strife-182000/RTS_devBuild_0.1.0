using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamColour : MonoBehaviour
{
    private Renderer mesh;

    private ActivePlayer player;

    private enemyStructure ES;
    private enemyUnit EU;
    private Player_Unit PU;
    private Player_Structure PS;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<Renderer>();

        player = FindObjectOfType<ActivePlayer>();

        if (PU = GetComponentInParent<Player_Unit>())
        {
            if (GetComponent<MeshFilter>())
            {
                if (gameObject.layer == 10)
                {
                    mesh.material.SetColor("_Color", player.superManager.playerColours[0]);
                }
                else
                {
                    foreach (Material mat in mesh.materials)
                    {
                        mat.SetColor("_TeamColor", player.superManager.playerColours[0]);
                    }
                }
            }
            else
            {
                foreach (Material mat in mesh.materials)
                {
                    mat.SetColor("_TeamColor", player.superManager.playerColours[0]);
                }
            }
        }
        else if (EU = GetComponentInParent<enemyUnit>())
        {
            if (GetComponent<MeshFilter>())
            {
                mesh.material.SetColor("_Color", player.superManager.playerColours[EU.player_ID]);
            }
            else
            {
                foreach (Material mat in mesh.materials)
                {
                    mat.SetColor("_TeamColor", player.superManager.playerColours[EU.player_ID]);
                }
            }
        }
        else if (PS = GetComponentInParent<Player_Structure>())
        {
            mesh.material.SetColor("_Color", player.superManager.playerColours[0]);
        }
        else if(ES = GetComponentInParent<enemyStructure>())
        {
            mesh.material.SetColor("_Color", player.superManager.playerColours[ES.player_ID]);
        }
    }

    /*private void Update()
    {
        if (player.superManager.changeTeamColorDuringRuntime)
        {
            if (PU)
            {
                foreach (Material mat in mesh.materials)
                {
                    mat.SetColor("_TeamColor", player.superManager.playerColours[0]);
                }
            }
            else if (EU)
            {
                foreach (Material mat in mesh.materials)
                {
                    mat.SetColor("_TeamColor", player.superManager.playerColours[EU.player_ID]);
                }
            }
            else if (PS)
            {
                mesh.material.SetColor("_Color", player.superManager.playerColours[0]);
            }
            else if (ES)
            {
                mesh.material.SetColor("_Color", player.superManager.playerColours[ES.player_ID]);
            }
        }
    }*/
}
