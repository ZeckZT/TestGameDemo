using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Data")]

public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]

    public int maxHealth;

    public int currentHealth;

    public float baseDefence;

    public float currentDefence;

    [Header("Kill")]
    public int killPoint;

    [Header("Level")]
    public int currentLevel;
    public int maxLevel;
    public int upLevelExp;
    public int currentExp;
    //提升幅度
    public float levelBuff;
    //增加升级属性的提升幅度。
    public float LevelMultiplier
    {
        get{return 1+(currentLevel -1)*levelBuff;}
    }
    public void UpdateExp(int Exppoint,AttackData_SO at)
    {
        if(currentLevel >= 10)
        {
            currentExp += Exppoint;
            if(currentExp >= upLevelExp)
            {
                currentExp = upLevelExp -1;
            }
            else
            {
                currentExp = Mathf.Min(currentExp += Exppoint,Exppoint-1);
                GameObject.FindGameObjectWithTag("Player").GetComponent<FloatText>().GenerateExp(Exppoint);
                GameObject.FindGameObjectWithTag("Player").GetComponent<FloatText>().DestroyGameobject();
            }
                
        } 
        else
        {
            currentExp += Exppoint;
        }
        
        while(currentExp>=upLevelExp && currentLevel < maxLevel)
        {
            at.UPdate();
            currentExp = currentExp - upLevelExp;
            LeveUp();
        }
            
    }

    private void LeveUp()
    {
        //升级能提升的属性都在这里实现
        currentLevel = Mathf.Clamp(currentLevel +1, 0, maxLevel);
        upLevelExp += (int)(upLevelExp * LevelMultiplier);
        if(currentExp>=upLevelExp && currentLevel == maxLevel)
           currentExp = upLevelExp - 1;
        if(currentLevel<=maxLevel)
        {
            maxHealth = maxHealth + 10;
            currentHealth = currentHealth + 10;
            baseDefence = baseDefence + 0.5f;
            currentDefence = currentDefence + 0.5f;
            GameObject.FindGameObjectWithTag("Player").GetComponent<FloatText>().LevelUp();
            GameObject.FindGameObjectWithTag("Player").GetComponent<FloatText>().DestroyGameobject();
        }
        
        
        
    }
}
