using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The ability is not ready yet
public class Attack : Ability
{
    public override AbilityType AbilityType
    {
        get => AbilityType.Attack;
    }

    public override void Activate()
    {
        
    }

    public override bool CanBeUsed()
    {
        return false;
    }
}
