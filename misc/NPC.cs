using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;

    public bool isSelected;

    public GameObject selectionEffect;

    public string objectName;
    public Sprite objectSprite;
    public string objectInfo1;
    public string objectInfo2;

    public float damage;
    public int attackType; // 1 -> Melee, 2 -> Ranged, 3 -> Jet
    public int damageType; //1 -> Blunt, 2 -> Pierce, 3 -> Siege, 4 -> Elemental

    public bool isAggressive;

    public GameObject nearestEnemy;

    public Vector3 enemyDirection, attackerDirection;
    public float closestDistance;
    public float minAttackRange, maxAttackRange;

    private GameObject attacker;
    public float minRetaliateAttackRange;
    public float maxRetaliateAttackRange;

    public float bluntArmour, pierceArmour;

    public HealthBar HB;

    private bool walking, attacking, death;

    private ActivePlayer player;

    private float deathTimer;
    public float deathTimeLimit;

    [Header("Input Statistics(Melee)")]

    public float attackCountdown = 0f;
    public float attackRate = 0.8f;

    [Header("Input Statistics(Ranged)")]

    public GameObject bulletPrefab;
    public float fireRate = 0.8f;
    public float fireCountdown = 0f;
    public Transform firePoint;
    public GameObject fireEffect;

    [Header("Input Statistics(Jet)")]

    public JetSpray jetSpray;
    public float jetRate = 2f;
    public float jetCountdown = 0f;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        HB = GetComponentInChildren<HealthBar>();
        player = FindObjectOfType<ActivePlayer>();

        if (selectionEffect)
        {
            selectionEffect.transform.localPosition = new Vector3(0, 0.1f, 0);
        }

        if (isAggressive)
        {
            player.player_enemies.Add(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        NPCLoop();
    }

    public void NPCLoop()
    {
        death = HB.death;

        if (!death)
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

            WalkCheck();
            AttackCheck();

            if (attackType == 3)
            {
                if (!attacking)
                {
                    if (jetSpray.gameObject.activeSelf)
                    {
                        jetSpray.gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            anim.SetBool("death", death);

            if (CompareTag("objectToDestroy"))
            {
                player.superManager.scenarioManager.objectsToDestroy.Remove(gameObject);
            }

            if (agent.enabled)
            {
                agent.enabled = false;
            }

            deathTimer += Time.deltaTime;

            if (deathTimer >= deathTimeLimit)
            {
                Destroy(gameObject);
            }

        }
    }

    public void WalkCheck()
    {
        if (agent.stoppingDistance >= agent.remainingDistance)
        {
            walking = false;
        }
        else
        {
            walking = true;
        }

        anim.SetBool("walking", walking);
    }

    GameObject GetClosestEnemy(List<GameObject> Enemies)
    {
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject targetEnemy in Enemies)
        {
            if (targetEnemy)
            {
                Vector3 direction = targetEnemy.transform.position - position;
                float distance = direction.sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = targetEnemy;
                }
            }
        }
        return closestEnemy;
    }

    public void MeleeAttack(GameObject target)
    {
        if (attackCountdown <= 0f)
        {
            attackCountdown = 1 / attackRate;

            target.GetComponent<HealthBar>().DecreaseHealth(damage, gameObject, damageType);
        }

        attackCountdown -= Time.deltaTime;
    }

    public void RangedAttack(GameObject target)
    {
        if (fireCountdown <= 0f)
        {
            if (fireEffect)
            {
                if (fireEffect.activeSelf)
                {
                    fireEffect.SetActive(false);
                }

                if (!fireEffect.activeSelf)
                {
                    fireEffect.SetActive(true);
                }
            }

            Shoot(target);

            fireCountdown = 1 / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    public void JetAttack()
    {
        if (jetCountdown <= 0f)
        {
            if (jetSpray)
            {
                if (!jetSpray.gameObject.activeSelf)
                {
                    jetSpray.gameObject.SetActive(true);
                }
            }

            jetSpray.SprayDamage(damage, gameObject, damageType);

            jetCountdown = 1 / jetRate;
        }

        jetCountdown -= Time.deltaTime;
    }

    void Shoot(GameObject target)
    {
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        CatapultStoneThrow bullet = bulletGO.GetComponent<CatapultStoneThrow>();

        if (bullet != null)
        {
            bullet.Seek(target, damage, transform.gameObject, target.transform.position, damageType);
        }
    }

    public void Retaliate()
    {
        attacker = HB.attacker;

        float dist = Vector3.Distance(attacker.transform.position, transform.position);
        attackerDirection = attacker.transform.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(attackerDirection);

        if (dist < maxRetaliateAttackRange && !death)
        {
            if (!attacker.GetComponent<HealthBar>().death)
            {
                agent.destination = attacker.transform.position;

                if (dist <= minRetaliateAttackRange)
                {
                    walking = false;

                    agent.destination = transform.position;

                    transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 3);

                    attacking = true;

                    if (attackCountdown <= 0f)
                    {
                        attackCountdown = 1 / attackRate;

                        attacker.GetComponent<HealthBar>().DecreaseHealth(damage, transform.gameObject, damageType);
                    }

                    attackCountdown -= Time.deltaTime;
                }
                else
                {
                    attacking = false;
                }
            }
            else
            {
                attacking = false;
            }
        }
        else
        {
            HB.attacker = null;
            attacker = null;
        }
    }

    public void AttackCheck()
    {
        if (player.NPC_enemies.Count <= 0)
        {
            closestDistance = 100f;
            attacking = false;
        }
        else if (player.NPC_enemies.Count > 0)
        {
            nearestEnemy = GetClosestEnemy(player.NPC_enemies);

            if (nearestEnemy)
            {
                closestDistance = Vector3.Distance(transform.position, nearestEnemy.transform.position);

                enemyDirection = nearestEnemy.transform.position - transform.position;
                Quaternion rot = Quaternion.LookRotation(enemyDirection);

                if (isAggressive)
                {
                    if (closestDistance < maxAttackRange && !death)
                    {
                        if (!nearestEnemy.GetComponent<HealthBar>().death)
                        {
                            agent.destination = nearestEnemy.transform.position;

                            if (closestDistance <= minAttackRange)
                            {
                                walking = false;

                                agent.destination = transform.position;

                                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 3);

                                attacking = true;

                                if (attackCountdown <= 0f)
                                {
                                    attackCountdown = 1 / attackRate;

                                    nearestEnemy.GetComponent<HealthBar>().DecreaseHealth(damage, transform.gameObject, damageType);
                                }

                                attackCountdown -= Time.deltaTime;
                            }
                            else
                            {
                                attacking = false;
                            }
                        }
                        else
                        {
                            attacking = false;
                        }
                    }
                    else
                    {
                        attacking = false;
                    }
                }
                else if (HB.attacker)
                {
                    Retaliate();
                }
                else
                {
                    attacking = false;
                }
            }
        }

        anim.SetBool("attacking", attacking);
    }

    private void OnMouseEnter()
    {
        if (isAggressive)
        {
            player.cursor.SetEnemy();
        }
    }

    private void OnMouseExit()
    {
        player.cursor.SetMouse();
    }
}
