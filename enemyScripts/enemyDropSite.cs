using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyDropSite : MonoBehaviour
{
    public bool Wood, Food, Stone, Gold, Iron = false;
    public int playerID;
    public Bounds bounds;

    private ActivePlayer player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<ActivePlayer>();
        bounds = GetComponent<Collider>().bounds;

        player.enemyDropSites.Add(this);
    }
}
