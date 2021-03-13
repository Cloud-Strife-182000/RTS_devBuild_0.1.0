using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drops : MonoBehaviour
{
    public bool Wood, Food, Stone, Gold, Iron = false;

    private ActivePlayer player;
    public Bounds bounds;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<ActivePlayer>();
        bounds = GetComponent<Collider>().bounds;
        player.dropSites.Add(this);
    }
}
