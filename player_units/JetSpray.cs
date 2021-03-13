using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetSpray : MonoBehaviour
{
    private HealthBar HB;
    public List<Collider> hitColliders;

    public void SprayDamage(float damage, GameObject sprayer, int damageType)
    {
        if(hitColliders.Count > 0)
        {
            foreach(Collider c in hitColliders)
            {
                if (HB = c.GetComponent<HealthBar>())
                {
                    HB.DecreaseHealth(damage, sprayer, damageType);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hitColliders.Contains(other))
        {
            hitColliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hitColliders.Contains(other))
        {
            hitColliders.Remove(other);
        }
    }
}
