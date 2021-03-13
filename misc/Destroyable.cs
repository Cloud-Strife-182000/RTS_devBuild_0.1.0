using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyable : MonoBehaviour
{
    // Start is called before the first frame update

    public ParticleSystem destructionVisual;

    public bool detonated;

    public void Detonate()
    {
        if(detonated == false)
        {
            if (destructionVisual)
            {
                ParticleSystem tempVisual = Instantiate(destructionVisual);
                tempVisual.transform.position = transform.position;
            }

            detonated = true;
        }
    }
}
