using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityType { Attack, Research, Talk, FoundCity }

public abstract class Ability
{
    public void Use()
    {
        if (CanBeUsed())
        {
            Activate();
        }
    }

    public abstract void Activate();

    public virtual bool CanBeUsed()
    {
        return owner.RemainingActionCount >= cost;
    }

    public Unit owner;

    protected int cost = 1;

    public abstract AbilityType AbilityType { get; }
}

public static class AbilityCreator
{
    public static Ability CreateAbility(AbilityType type)
    {
        if (type == AbilityType.FoundCity)
        {
            return new FoundCity();
        }
        else if (type == AbilityType.Attack)
        {
            return new Attack();
        }
        else if (type == AbilityType.Research)
        {
            return new Research();
        }
        else if (type == AbilityType.Talk)
        {
            return new Talk();
        }
        return null;
    }
}

public static class AbilityTypeExtension
{
    static string[] names = { "Attack", "Research", "Talk", "FoundCity"};

    public static string ToString(this AbilityType type)
    {
        return names[(int)type];
    }
}
