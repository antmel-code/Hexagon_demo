using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Food,
    Wood,
    Stone,
    Iron
}

public static class ResourceTypeExtension
{
    static string[] names = { "Food", "Wood", "Stone", "Iron" };

    public static string ToString(this ResourceType type)
    {
        return names[(int)type];
    }
}
