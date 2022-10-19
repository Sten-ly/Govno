using UnityEngine;
using UnityEngine.AI;
//proba
public class EnemyAI : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;
    public int detectDistance;
    public float aggroDistance;
    public float attackDistance;
    public float walkSpeed;
    public float runSpeed;
    public int rays;
    public int angle;
    public Vector3 offset;
    public Transform holdPointR;
    public Transform holdPointL;
    private Transform target;
    public bool inCombat;
    Vector3 saveTargetPos;
    Vector3 startPos;
    Quaternion startRot;
    float deagroDelay;
    float attackCD = 0f;
    Vector3 dir;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponent<Animator>();

        startPos = transform.position;

        startRot = transform.rotation;

        target = GameObject.FindGameObjectWithTag("Player").transform;

        //agent.enabled = false;

        //rays = angle / 10;
    }

    public void Death()
    {
        animator.SetTrigger("Dead"); //Проигрывает анимацию смерти

        //agent.destination = transform.position;

        agent.speed = 0;

        Invoke("DisableActor", animator.GetCurrentAnimatorStateInfo(0).length);

        GetComponent<CapsuleCollider>().enabled = false;
    }

    void DisableActor()
    {
        animator.enabled = false;

        agent.enabled = false;

        this.enabled = false;

        //gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    bool RayToScan()
    {
        bool result = false;

        for (int k = -rays; k < rays; k++)
        {
            for (int i = -rays; i < rays; i++)
            {
                Vector3 dir = transform.TransformDirection(new Vector3(Mathf.Sin(i * (angle / rays) * Mathf.Deg2Rad), k * (angle / rays) * Mathf.Deg2Rad, Mathf.Cos(i * (angle / rays) * Mathf.Deg2Rad)));

                RaycastHit hit = new RaycastHit();

                Vector3 pos = transform.position + offset;

                if (Physics.Raycast(pos, dir, out hit, detectDistance))
                {
                    if (hit.transform == target)
                    {
                        result = true;

                        Debug.DrawLine(pos, hit.point, Color.green);
                    }

                    else Debug.DrawLine(pos, hit.point, Color.blue);
                }

                else Debug.DrawRay(pos, dir * detectDistance, Color.red);
            }
        }
        
        return result;
    }

    public void DrawWeapon()
    {
        animator.SetTrigger("Draw");

        animator.SetBool("inCombat", true);

        holdPointR.GetChild(0).gameObject.SetActive(true);

        holdPointL.GetChild(0).gameObject.SetActive(true);
    }

    public void Aggro()
    {
        if (inCombat == false) DrawWeapon();

        //agent.enabled = true;

        inCombat = true;

        //agent.SetDestination(target.position);

        saveTargetPos = target.position;

        agent.speed = runSpeed;

        //transform.rotation = target.rotation;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        if (RayToScan() == true)
        {
            if (GetComponent<EnemyStats>().alertVolume == 0) DrawWeapon();

            float alert = GetComponent<EnemyStats>().maxAlert / distanceToPlayer;

            GetComponent<EnemyStats>().Alarm(alert);

            saveTargetPos = target.position;

            if (distanceToPlayer < aggroDistance)
            {
                animator.SetBool("inCombat", true);

                animator.SetBool("Moving", true);

                agent.SetDestination(target.position);

                agent.speed = runSpeed;
            }         
        }

        if (inCombat == true && RayToScan() == false)
        {
            agent.SetDestination(saveTargetPos);

            //GetComponent<EnemyStats>().Calm();

            animator.SetBool("Moving", true);
        }

        if (animator.GetBool("Moving") == true)
        {
            if (inCombat == true) agent.speed = runSpeed;

            else agent.speed = walkSpeed;    
        }

        if (Vector3.Distance(transform.position, agent.destination) <= 10 && inCombat == true && RayToScan() == false)
        {
            if (agent.destination == startPos && inCombat == false) Debug.Log("Враг вернулся на место");

            if (agent.destination == saveTargetPos || agent.destination == saveTargetPos) Debug.Log("Враг потерял меня из виду");

            animator.SetBool("Moving", false);
        }

        int attackType = 0;

        if (distanceToPlayer <= attackDistance & inCombat == true) //Если игрок в зоне удара, НПС атакует
        {
            if (Time.time >= attackCD)
            {
                attackType = Random.Range(1, 4);

                switch (attackType)
                {
                    case 1:

                        animator.SetInteger("AttackType", 1);

                        break;

                    case 2:

                        animator.SetInteger("AttackType", 2);

                        break;

                    case 3:

                        animator.SetInteger("AttackType", 3);

                        break;

                    default:

                        animator.SetInteger("AttackType", 0);

                        break;
                }

                attackCD = Time.time + animator.GetCurrentAnimatorStateInfo(0).length + Random.Range(0f, 1f);
            }
        } 
    }

    public void EnableCollider()
    {
        holdPointR.GetComponentInChildren<WeaponStats>().boxCol.enabled = true;
    }

    public void DisableCollider()
    {
        holdPointR.GetComponentInChildren<WeaponStats>().boxCol.enabled = false;
    }

    public void Knockback()
    {
        animator.SetTrigger("Reset");

        Debug.Log(transform.name + " попал по стене");
    }

    public void ResetAgent()
    {
        inCombat = false;

        animator.SetBool("inCombat", false);

        animator.SetTrigger("Draw");

        holdPointR.GetChild(0).gameObject.SetActive(false);

        holdPointL.GetChild(0).gameObject.SetActive(false);

        agent.SetDestination(startPos);

        animator.SetBool("Moving", true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }
}
