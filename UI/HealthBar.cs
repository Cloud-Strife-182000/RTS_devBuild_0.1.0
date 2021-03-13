using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;

    public ProgressBarPro healthBar;
    private Vector3 screenPos;

    public float healthBarOffset;

    public float timer;
    public float waitTimeAfterDamage = 8f;

    private bool startTimer;

    private Destroyable destroyable;

    public bool death;

    public GameObject attacker;
    public GameObject healer;

    private ActivePlayer player;
    private Player_Unit p_unit;
    private enemyUnit enemy_Unit;
    private PlaceableBuilding building;
    private Drops dropSite;
    private enemyDropSite EDS;
    private NPC npc;
    private NPCBuilding npcBuilding;

    public float bluntArmour, pierceArmour;
    public float standardFortification, siegeFortification;

    // Start is called before the first frame update
    void Start()
    {
        if (!healthBar)
        {
            healthBar = GetComponentInChildren<ProgressBarPro>();
        }
        
        player = FindObjectOfType<ActivePlayer>();

        if (GetComponent<Player_Unit>())
        {
            p_unit = GetComponent<Player_Unit>();
            bluntArmour = p_unit.bluntArmour;
            pierceArmour = p_unit.pierceArmour;
        }

        if (GetComponent<enemyUnit>())
        {
            enemy_Unit = GetComponent<enemyUnit>();
            bluntArmour = enemy_Unit.bluntArmour;
            pierceArmour = enemy_Unit.pierceArmour;
        }

        if (GetComponent<NPC>())
        {
            npc = GetComponent<NPC>();
            bluntArmour = npc.bluntArmour;
            pierceArmour = npc.pierceArmour;
        }

        if (GetComponent<Destroyable>())
        {
            destroyable = GetComponent<Destroyable>();
        }

        if (GetComponent<PlaceableBuilding>())
        {
            building = GetComponent<PlaceableBuilding>();

            if (building.PS)
            {
                standardFortification = building.PS.standardFortification;
                siegeFortification = building.PS.siegeFortification;
            }
            else if (building.ES)
            {
                standardFortification = building.ES.standardFortification;
                siegeFortification = building.ES.siegeFortification;
            }
        }

        if (GetComponent<Drops>())
        {
            dropSite = GetComponent<Drops>();
        }

        if (GetComponent<enemyDropSite>())
        {
            EDS = GetComponent<enemyDropSite>();
        }

        if (GetComponent<NPCBuilding>())
        {
            npcBuilding = GetComponent<NPCBuilding>();

            standardFortification = npcBuilding.standardFortification;
            siegeFortification = npcBuilding.siegeFortification;
        }

        if (building)
        {
            if (!building.isBuilt)
            {
                currentHealth = 1;
                healthBar.Value = 0.01f;
            }
        }
        else
        {
            currentHealth = maxHealth;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (healthBar.enabled)
        {
            UpdateHealthBarPos();
        }
        
        CheckHealthStatus();
        VisibilityToggle();
    }

    public void UpdateHealthBarPos()
    {
        screenPos = Camera.main.WorldToScreenPoint(transform.position);
        screenPos.y += healthBarOffset;

        healthBar.transform.position = screenPos;
    }

    public void CheckHealthStatus()
    {
        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if(currentHealth <= 0f)
        {
            if (destroyable)
            {
                destroyable.Detonate();

                Destroy(gameObject);
            }

            if (p_unit)
            {
                player.player_units.Remove(p_unit);
                player.AI_enemies.Remove(gameObject);
                player.animal_enemies.Remove(gameObject);
                player.NPC_enemies.Remove(gameObject);
            }

            if (enemy_Unit)
            {
                player.enemyUnits.Remove(enemy_Unit);
                player.player_enemies.Remove(gameObject);
                player.animal_enemies.Remove(gameObject);
                player.NPC_enemies.Remove(gameObject);

                if (enemy_Unit.unitType == 1 || (enemy_Unit.unitType == 4 && enemy_Unit.shipCanGather))
                {
                    enemy_Unit.AI_St.AIS.gatherers.Remove(enemy_Unit);
                }
                else if(enemy_Unit.unitType == 2)
                {
                    enemy_Unit.AI_St.AIS.combatants.Remove(enemy_Unit);
                }
            }

            if (building)
            {
                player.placeableBuildings.Remove(building);

                if (building.PS)
                {
                    player.p_structs.Remove(building.PS);
                    player.AI_enemies.Remove(gameObject);
                    player.NPC_enemies.Remove(gameObject);
                }
                else if (building.ES)
                {
                    player.e_structs.Remove(building.ES);
                    player.player_enemies.Remove(gameObject);
                    player.NPC_enemies.Remove(gameObject);
                }
            }

            if (npc)
            {
                if (npc.isAggressive)
                {
                    player.player_enemies.Remove(gameObject);
                }
            }

            if (npcBuilding)
            {
                if (npcBuilding.isEnemy)
                {
                    player.player_enemies.Remove(gameObject);
                }
                
            }

            if (dropSite)
            {
                player.dropSites.Remove(dropSite);
            }

            if (EDS)
            {
                player.enemyDropSites.Remove(EDS);
            }

            death = true;

        }

    }

    public void VisibilityToggle()
    {
        if (startTimer == true)
        {
            timer += Time.deltaTime;

            if (timer >= waitTimeAfterDamage)
            {
                startTimer = false;
                healthBar.gameObject.SetActive(false);
            }
        }

        if (currentHealth < maxHealth && startTimer == true)
        {
            healthBar.gameObject.SetActive(true);
        }
        else
        {
            healthBar.gameObject.SetActive(false);
        }
    }

    public void DecreaseHealth(float damage, GameObject _attacker, int damageType)
    {
        attacker = _attacker;

        float actual_damage;

        if(damageType == 1)
        {
            actual_damage = damage - bluntArmour - standardFortification;
        }
        else if(damageType == 2)
        {
            actual_damage = damage - pierceArmour - standardFortification;
        }
        else if(damageType == 3)
        {
            actual_damage = damage - (siegeFortification * 2);
        }
        else
        {
            actual_damage = damage;
        }

        currentHealth = currentHealth - actual_damage;
        float dmg_percent = actual_damage / maxHealth;

        healthBar.Value -= dmg_percent;

        startTimer = true;
        timer = 0f;
    }

    public void IncreaseHealth(float regen, GameObject _healer)
    {
        healer = _healer;
        currentHealth += regen;

        float regen_percent = regen / maxHealth;

        healthBar.Value += regen_percent;

        startTimer = true;
        timer = 0f;
    }
}
