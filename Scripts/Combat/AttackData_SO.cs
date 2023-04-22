using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Attack/Attack Data")]

public class AttackData_SO : ScriptableObject
{
    public float attackRange;

    public float skillRange;

    public float coolDown;

    public float minDamge;

    public float maxDamge;

    public float criticalMultiplier;

    public float criticalChance;

    internal void UPdate()
    {
        minDamge = minDamge + 0.5f;
        maxDamge = maxDamge + 0.5f;
    }
}
