using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates{GUARD, PATROL, CHASE, DEAD}

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]

public class EnemyController : MonoBehaviour,IEndGameObserver
{
    

    private NavMeshAgent agent;

    private Animator anim;

    private Collider coll;

    private EnemyStates enemyStates;

    protected CharacterStats characterStats;

    

    [Header("Basic Settings")]

    public float sightRadius;

    protected GameObject attackTarget;

    public bool isGuard;

    private float speed;

    private Quaternion guardRotation;
    private float lastAttackTime;
    private float remainLookAtTime;
    public float lookAtTime;

    [Header("Patrol State")]

    public float patrolRange;

    private Vector3 wayPoint;

    private Vector3 guardPos;
    
    
    //动画用的bool值
    bool isWalk;
    
    bool isChase;

    bool isFolow;

    bool isDead;
    
    bool playerDead;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();
    }

    void Start()
    {
        if(isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();

        }
        GameManager.Instance.AddObserver(this);
    }
    
    // 切换场景用
    // void OnEnable()
    // {
    //     GameManager.Instance.AddObserver(this);
    // }

    void OnDisable()
    {
        if(!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }
    
    void Update()
    {
        if(characterStats.CurrentHealth == 0)
            isDead = true;
        if(!playerDead)
        {
        SwitchStates();
        SwitchAnimation();
        lastAttackTime -= Time.deltaTime;
        }
    }
    void SwitchAnimation()
    {
        anim.SetBool("Walk",isWalk);
        anim.SetBool("Chase",isChase);
        anim.SetBool("Folow",isFolow);
        anim.SetBool("Critical",characterStats.isCritical);
        anim.SetBool("Death",isDead);
    }
    void SwitchStates()
    {
        if(isDead)
           enemyStates = EnemyStates.DEAD;
        //如果发现Player 切换追击状态
        else if(FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
        }
        switch(enemyStates)
        {
            case EnemyStates.GUARD:
                isChase = false;
                if(transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;

                    if(Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.001f);
                    }
                       

                }
                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;
                
                //判断是否已经到了目标点
                if(Vector3.Distance(wayPoint,transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    //判断是否处于观察时间
                    if(remainLookAtTime>0)
                         remainLookAtTime -= Time.deltaTime;
                    else
                        GetNewWayPoint();
                }
                else
                {
                    isWalk = true;

                    agent.destination = wayPoint;
                }
                break;
            case EnemyStates.CHASE:
                isWalk = false;
                isChase = true;
                agent.speed = speed;
                if(!FoundPlayer())
                {
                    //拉脱返回初始状态
                    isFolow = false;
                    isChase = false;
                    if(remainLookAtTime>0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else if(isGuard)
                        enemyStates = EnemyStates.GUARD;
                    else
                        enemyStates = EnemyStates.PATROL;
                    agent.destination = transform.position;
                }
                else
                {
                    isFolow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }

                if(TargetInAttackRange() || TargetInSkillRange())
                {
                    isFolow = false;
                    agent.isStopped = true;
                    if(lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;

                        //判断暴击
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                        //执行攻击
                        Attack();
                    }
                }
                break;
            case EnemyStates.DEAD:
                coll.enabled = false;
                //agent.enabled = false;
                agent.radius = 0;
                Destroy(gameObject, 2f);
                break;
        }
    }

    void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if(TargetInAttackRange())
        {
            anim.SetTrigger("Attack");
        }
        if(TargetInSkillRange())
        {
            anim.SetTrigger("Skill");
        }
    }

    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);

        foreach (var target in colliders)
        {
            if(target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;

        return false;
    }

    bool TargetInAttackRange()
    {
        if(attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position,transform.position) <= characterStats.attackData.attackRange;
        else
            return false;
    }
    
    bool TargetInSkillRange()
    {
        if(attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position,transform.position)<=characterStats.attackData.skillRange;
        else
            return false;
    }
    void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);

        NavMeshHit hit;

        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1)?hit.position : transform.position;


        //此代码没判断点是否选择了不可移动的点。这样会导致无法继续巡逻。 
        //wayPoint = randomPoint;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position,sightRadius);
    }

    void EnemyHit()
    {
        if(attackTarget != null && transform.isFacingTarget(attackTarget.transform))
        {   
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats); 
        }
        else
        {
            //跳出闪避字样
            //print("闪避");
        }
        
    }

    public void EndNotify()
    {
        //获胜动画
        //停止移动
        //停止Agent
        anim.SetBool("Win",true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;

    }
}
