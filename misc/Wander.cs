using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wander : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    public float delay, maxAttackRange, minAttackRange;
    private float timer, distance;

    private bool inPosition;
    private int counter, prevCounter;
    private Vector3 origin, destination;
    private float x_mod, z_mod;
    public float closestDistance;
    public float maxWalkDistance, minWalkDistance;
    public float attackCountdown = 0f;
    public float attackRate = 0.8f;

    public string objectName;
    public Sprite objectSprite;
    public string objectInfo1;
    public string objectInfo2;

    public GameObject selectionEffect;
    public bool isSelected;
    public bool isAggressive, hasBeenAttacked;

    public float damage;
    public int damageType;

    public GameObject nearestEnemy;

    public Vector3 enemyDirection, attackerDirection;

    private GameObject attacker;
    public float minRetaliateAttackRange;
    public float maxRetaliateAttackRange;

    public HealthBar HB;

    private bool walking, attacking, death;

    private ActivePlayer player;

    private float deathTimer;
    public float deathTimeLimit;

    private NodeManager nm;

    private void Awake()
    {
        if (GetComponent<NodeManager>())
        {
            nm = GetComponent<NodeManager>();

            nm.enabled = false;
        }
    }

    void Start()
    {
        counter = Random.Range(0, 3);
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        HB = GetComponentInChildren<HealthBar>();
        player = FindObjectOfType<ActivePlayer>();
        origin = transform.position;

        if (selectionEffect)
        {
            selectionEffect.transform.localPosition = new Vector3(0, 0.1f, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        WanderLoop();
    }

    public void WanderLoop()
    {
        death = HB.death;

        if (!death)
        {
            MoveAround();
            ActionCheck();
            WalkCheck();
            AttackCheck();

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
            anim.SetBool("death", death);

            if (agent.enabled)
            {
                agent.enabled = false;
            }

            if (nm)
            {
                nm.enabled = true;
                enabled = false;
            }
            else
            {
                deathTimer += Time.deltaTime;

                if (deathTimer >= deathTimeLimit)
                {
                    Destroy(gameObject);
                }
            }
            
        }

    }

    public void ActionCheck()
    {
        distance = Vector3.Distance(transform.position, destination);

        if(distance < 0.5)
        {
            if (!inPosition)
            {
                prevCounter = counter;

                while(counter == prevCounter)
                {
                    counter = Random.Range(0, 3); //Wanders to random points
                }

                //counter++; //Wanders in a quadrilateral
            }

            inPosition = true;
            walking = false;

            if (timer > delay)
            {
                timer = 0;
            }
        }
    }

    public void MoveAround()
    {
        if (counter == 0)
        {
            WaitAndMove(Northeast);
        }
        else if (counter == 1)
        {
            WaitAndMove(Northwest);
        }
        else if (counter == 2)
        {
            WaitAndMove(Southwest);
        }
        else if (counter == 3)
        {
            WaitAndMove(Southeast);
        }
        //else if (counter > 3)  #Wanders in a quadrilateral
        //{
        //    counter = 0;
        //}

    }

    public void WalkCheck()
    {
        if(agent.stoppingDistance >= agent.remainingDistance)
        {
            walking = false;
        }
        else
        {
            walking = true;
            inPosition = false;
        }

        anim.SetBool("walking", walking);
    }

    private void Northwest()
    {
        x_mod = Random.Range(minWalkDistance, maxWalkDistance);
        z_mod = Random.Range(minWalkDistance, maxWalkDistance);
        destination = new Vector3(origin.x - x_mod, origin.y, origin.z + z_mod);
        agent.destination = destination;
    }

    private void Southwest()
    {
        x_mod = Random.Range(minWalkDistance, maxWalkDistance);
        z_mod = Random.Range(minWalkDistance, maxWalkDistance);
        destination = new Vector3(origin.x - x_mod, origin.y, origin.z - z_mod);
        agent.destination = destination;
    }

    private void Northeast()
    {
        x_mod = Random.Range(minWalkDistance, maxWalkDistance);
        z_mod = Random.Range(minWalkDistance, maxWalkDistance);
        destination = new Vector3(origin.x + x_mod, origin.y, origin.z + z_mod);
        agent.destination = destination;
    }

    private void Southeast()
    {
        x_mod = Random.Range(minWalkDistance, maxWalkDistance);
        z_mod = Random.Range(minWalkDistance, maxWalkDistance);
        destination = new Vector3(origin.x + x_mod, origin.y, origin.z - z_mod);
        agent.destination = destination;
    }

    public void WaitAndMove(System.Action func)
    {
        timer += Time.deltaTime;

        if (timer > delay)
        {
            func();
        }
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
        if (player.animal_enemies.Count <= 0)
        {
            closestDistance = 100f;
            attacking = false;
        }
        else if (player.animal_enemies.Count > 0)
        {
            nearestEnemy = GetClosestEnemy(player.animal_enemies);

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
