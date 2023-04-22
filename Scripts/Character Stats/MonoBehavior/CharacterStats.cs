using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public event Action<int, int> UpdateHealthBarOnAttack;
    public CharacterData_SO templateData;
    public CharacterData_SO characterData;

    public AttackData_SO attackData;

    
    [HideInInspector]
    public bool isCritical;
    //read from DATA SO

    void Awake()
    {
        if(templateData != null)
        {
            characterData = Instantiate(templateData);
        }
    }
    public int MaxHealth
    {
        get{if(characterData != null)return characterData.maxHealth; else return 0;}
        set{characterData.maxHealth = value;}
    }
    public int CurrentHealth
    {
        get{if(characterData != null)return characterData.currentHealth; else return 0;}
        set{characterData.currentHealth = value;}
    }
    public float BaseDefence
    {
        get{if(characterData != null)return characterData.baseDefence; else return 0;}
        set{characterData.baseDefence = value;}
    }
    public float CurrenDefence
    {
        get{if(characterData != null)return characterData.currentDefence; else return 0;}
        set{characterData.currentDefence = value;}
    }
    public int CurrentLevel
    {
        get{if(characterData != null)return characterData.currentLevel; else return 0;}
        set{characterData.currentLevel = value;}
    }
    //伤害计算
    public void TakeDamage(CharacterStats attacker, CharacterStats defener)
    {
        //FTDamage是自己多加的显示伤害的代码。
        var FTDamage = defener.GetComponent<FloatText>();
        float damage = Mathf.Max(attacker.CurrentDamage() - defener.CurrenDefence, 1);
        //跳出伤害数值
        FTDamage.GenerateDamage(damage);
        FTDamage.DestroyGameobject();
        CurrentHealth = (int)Mathf.Max(CurrentHealth - damage, 0);
        
        if(attacker.isCritical)
        {
            defener.GetComponent<Animator>().SetTrigger("Hit");
        }
        //更新血量显示
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
        //获取经验
        if(CurrentHealth <= 0)
        {
            attacker.characterData.UpdateExp(characterData.killPoint,attacker.attackData);
            FTDamage.DestroyGameobject();
        }
            
    }

    //武器的伤害计算
    public void TakeDamage(int damage, CharacterStats defener)
    {
        var FTDamage = defener.GetComponent<FloatText>();
        FTDamage.GenerateDamage((float)Mathf.Max(damage - defener.CurrenDefence, 0));
        FTDamage.DestroyGameobject();
        int CurrentDamage = (int)Mathf.Max(damage - defener.CurrenDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - CurrentDamage, 0);
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
        if(CurrentHealth <= 0)
        {
            GameManager.Instance.playerStats.characterData.UpdateExp(characterData.killPoint,GameManager.Instance.playerStats.attackData);
            FTDamage.DestroyGameobject();
        }
    }
    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamge, attackData.maxDamge);
        
        if(isCritical)
        {
            coreDamage *=attackData.criticalMultiplier;
        }
  
        return(int)coreDamage;
    }
}
