using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayController : MonoBehaviour
{
    private NavMeshAgent agent;

    private Animator anim;

    private GameObject attackTarget;
    
    private CharacterStats characterStats;

    private float lastAttackTime;
    private bool isDead;
    private float stopDistance;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        stopDistance = agent.stoppingDistance;
    }

    void OnEnable()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RegisterPlayer(characterStats);
    }
    void Start()
    {
        SaveManager.Instance.LoadPlayerData();
    }

    void OnDisable()
    {
        if(!MouseManager.IsInitialized) return;
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
    }


    void Update()
    {
        isDead = characterStats.CurrentHealth == 0;
        if(isDead)
           GameManager.Instance.NotifyObserver();
           
        SwitchAnimation();

        lastAttackTime -= Time.deltaTime;
    }
    
    private void SwitchAnimation()
    {
        anim.SetFloat("Speed",agent.velocity.sqrMagnitude);
        anim.SetBool("Death",isDead);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if(isDead) return;
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if(isDead) return;
        if(target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;
        transform.LookAt(attackTarget.transform);
         
        if(attackTarget.CompareTag("Attackable"))
        {
            //攻击石头攻击距离不够还未解决。
            while (Vector3.Distance(attackTarget.transform.position,transform.position) > characterStats.attackData.attackRange)
            {
                agent.destination = attackTarget.transform.position;
                yield return null;
             }
        }
        else
        {
            //真正的攻击距离应该是角色攻击距离加上怪物体积。所以距离应该是characterStats.attackData.attackRange + attackTarget.GetComponent<NavMeshAgent>().radius
            while (Vector3.Distance(attackTarget.transform.position,transform.position) > characterStats.attackData.attackRange + attackTarget.GetComponent<NavMeshAgent>().radius)
            {
                agent.destination = attackTarget.transform.position;
                yield return null;
            }
        }
        


        agent.isStopped = true;

        if(lastAttackTime < 0)
        {
            anim.SetBool("Critical",characterStats.isCritical);

            anim.SetTrigger("Attack");

            lastAttackTime = characterStats.attackData.coolDown;
        }
    }

    void Hit()
    {
        if(attackTarget.CompareTag("Attackable"))
        {
            if(attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HitNothing)
            {
                //转换石头的目标模式
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                //设置玩家攻击时候的初速度，防止一开始就进入hitNothing状态。
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                //使得石头朝玩家正前方飞出去
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward*25,ForceMode.Impulse);
            }
            
        }
        else
        {
            //获得攻击目标的数据
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            //调用TakeDamage传入数据计算伤害
            targetStats.TakeDamage(characterStats, targetStats);
        }

    }

}
