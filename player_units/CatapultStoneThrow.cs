using UnityEngine;
using System.Collections;

public class CatapultStoneThrow : MonoBehaviour
{
    public GameObject thrower;
    public GameObject target;

    public Vector3 targetPosition;

    public float firingAngle = 45.0f;
    public float gravity = 0.8f;
    public float speed = 70f;

    public Vector3 dir;
    public Vector3 posDir;

    public enum projectileType {Bullet, Arrow, Bolt}
    public projectileType typeOfProjectile;

    public bool canExplode;

    public float explosionRadius;
    public Vector3 launchPoint;
    public GameObject ImpactEffect;

    public Rigidbody rigid;

    public Collider[] affectedColliders;

    float damage;
    int damageType;

    private void Start()
    {
        launchPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(typeOfProjectile == projectileType.Bullet)
        {
            Bullet();
        }
        else if(typeOfProjectile == projectileType.Arrow)
        {
            Arrow();
        }
        else if(typeOfProjectile == projectileType.Bolt)
        {
            Bolt();
        }
    }

    public void Bullet()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        dir = target.transform.position - transform.position;

        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            if (canExplode)
            {
                Explode();
            }
            else
            {
                Damage(target);
            }

            Impact();

            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);

        transform.LookAt(target.transform);
    }

    public void Bolt()
    {
        float distanceThisFrame = speed * Time.deltaTime;

        if (target != null)
        {
            dir = target.transform.position - transform.position;

            if (dir.magnitude <= distanceThisFrame)
            {
                if (!canExplode)
                {
                    Damage(target);
                }
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        posDir = targetPosition - transform.position;

        if (posDir.magnitude <= distanceThisFrame)
        {
            Impact();

            if (canExplode)
            {
                Explode();
            }

            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);

        transform.LookAt(target.transform);
    }

    public void Arrow()
    {
        float distanceThisFrame = speed * Time.deltaTime;

        if (target != null)
        {
            dir = target.transform.position - transform.position;

            if (dir.magnitude <= distanceThisFrame)
            {
                if (!canExplode)
                {
                    Damage(target);
                }
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        posDir = targetPosition - transform.position;

        if(posDir.magnitude <= distanceThisFrame)
        {
            Impact();

            if (canExplode)
            {
                Explode();
            }

            return;
        }

        transform.LookAt(targetPosition);

        float R = Vector3.Distance(transform.position, targetPosition);
        float G = Physics.gravity.y;
        float tanAlpha = Mathf.Tan(firingAngle * Mathf.Deg2Rad);
        float H = targetPosition.y - transform.position.y;

        float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha) ) );
        float Vy = tanAlpha * Vz;

        Vector3 localVelocity = new Vector3(0f, Vy, Vz);
        Vector3 globalVelocity = transform.TransformDirection(localVelocity);

        rigid.velocity = globalVelocity;
    }

    public void Seek(GameObject _target, float _damage, GameObject _thrower, Vector3 _targetPosition, int _damageType)
    {
        target = _target;

        damage = _damage;

        damageType = _damageType;

        thrower = _thrower;

        targetPosition = _targetPosition;
    }

    void Impact()
    {
        Destroy(gameObject);

        if (ImpactEffect)
        {
            GameObject effectIns = (GameObject)Instantiate(ImpactEffect, transform.position, transform.rotation);

            Destroy(effectIns, 5f);
        }
    }

    void Explode()
    {
        affectedColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider collider in affectedColliders)
        {
            if(collider.GetComponent<HealthBar>())
            {
                Damage(collider.gameObject);
            }
        }

    }

    void Damage(GameObject enemy)
    {
        enemy.GetComponent<HealthBar>().DecreaseHealth(damage, thrower, damageType);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

}
   

