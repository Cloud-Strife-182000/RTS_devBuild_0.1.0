using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBuilding : MonoBehaviour
{
    public HealthBar HB;
    private ActivePlayer player;

    public string objectName;
    public Sprite objectSprite;
    public string objectInfo1;
    public string objectInfo2;

    public bool isEnemy;

    public bool isSelected;
    public GameObject selectionEffect;

    public float damage;

    public float standardFortification, siegeFortification;

    private void Start()
    {
        HB = GetComponent<HealthBar>();
        player = FindObjectOfType<ActivePlayer>();

        if (selectionEffect)
        {
            selectionEffect.transform.localPosition = new Vector3(0, 0.1f, 0);
        }

        if (isEnemy)
        {
            player.player_enemies.Add(gameObject);
        }
    }

    private void Update()
    {
        if (!HB.death)
        {
            if (isSelected)
            {
                if (selectionEffect)
                {
                    if (!selectionEffect.activeSelf)
                    {    
                        selectionEffect.SetActive(true);
                    }
                }
            }
            else
            {
                if (selectionEffect)
                {
                    if (selectionEffect.activeSelf)
                    {
                        selectionEffect.SetActive(false);
                    }
                }
            }
        }
        else
        {
            if (CompareTag("objectToDestroy"))
            {
                FindObjectOfType<ScenarioManager>().objectsToDestroy.Remove(gameObject);
            }

            Destroy(gameObject);
        }
    }
    private void OnMouseEnter()
    {
        if (isEnemy)
        {
            player.cursor.SetEnemy();
        }
    }

    private void OnMouseExit()
    {
        player.cursor.SetMouse();
    }

}
