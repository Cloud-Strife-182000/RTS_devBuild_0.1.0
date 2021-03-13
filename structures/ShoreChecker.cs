using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoreChecker : MonoBehaviour
{
    public bool frontWater, frontLand, rightLand, leftLand, backLand, alignmentArea;

    public List<Collider> waterColliders = new List<Collider>();
    public List<Collider> terrainColliders = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<RTSWater>())
        {
            waterColliders.Add(other);
        }

        if (other.GetComponent<Terrain>())
        {
            terrainColliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<RTSWater>())
        {
            waterColliders.Remove(other);
        }

        if (other.GetComponent<Terrain>())
        {
            terrainColliders.Remove(other);
        }
    }
}
