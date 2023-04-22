using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public  class Golem : EnemyController
{
    [Header("Skill")]
    public float kickForce = 100;
    //获取石头的预制体
    public GameObject rockPrefab;
    //获取右手丢出石头的位置
    public Transform rightHand;

    //石头人近战判断
    public void KickOff()
    {
        if(attackTarget != null && transform.isFacingTarget(attackTarget.transform))
        {   
        var targetStats = attackTarget.GetComponent<CharacterStats>();

        Vector3 direction = (attackTarget.transform.position - transform.position).normalized;
        //direction.Normalize();等于上面加了括号的Normalize

        attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
        attackTarget.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
        //根据情况添加功能
        attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
        targetStats.TakeDamage(characterStats, targetStats); 
        }
    }

    //丢石头攻击判断
    public void ThrowRock()
    {
        if(attackTarget != null)
        {
            //Instantiate是生成物体（预制体，生成位置，旋转）
            var rock = Instantiate(rockPrefab, rightHand.position, Quaternion.identity);
            //给生成的rock赋值攻击目标
            rock.GetComponent<Rock>().target = attackTarget;   
        }
        else
        {
            attackTarget = FindObjectOfType<PlayController>().gameObject;
            var rock = Instantiate(rockPrefab, rightHand.position, Quaternion.identity);
            rock.GetComponent<Rock>().target = attackTarget;   

        }
    }
}
