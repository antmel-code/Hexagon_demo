using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleFormation
{
    Single,
    Wedge,
    Turtle,
    WideWedge,
    WideTurtle
}

public static class BattleFormations
{
    public static Vector3[] FormationOffsets(BattleFormation formation)
    {
        if (formation == BattleFormation.Wedge)
        {
            Vector3[] offsets = new Vector3[5];
            offsets[0] = new Vector3(0f, 0f, 2f);
            offsets[1] = new Vector3(-2f, 0f, 0f);
            offsets[2] = new Vector3(2f, 0f, 0f);
            offsets[3] = new Vector3(4f, 0f, -2f);
            offsets[4] = new Vector3(-4f, 0f, -2f);
            return offsets;
        }
        else if (formation == BattleFormation.Turtle)
        {
            Vector3[] offsets = new Vector3[6];
            offsets[0] = new Vector3(0f, 0f, 2f);
            offsets[1] = new Vector3(-3f, 0f, 2f);
            offsets[2] = new Vector3(3f, 0f, 2f);
            offsets[3] = new Vector3(0f, 0f, -1f);
            offsets[4] = new Vector3(-3f, 0f, -1f);
            offsets[5] = new Vector3(3f, 0f, -1f);
            return offsets;
        }
        else
        {
            Vector3[] offsets = new Vector3[1];
            offsets[0] = Vector3.zero;
            return offsets;
        }

    }

    public static Vector3[] FormationOffsets(BattleFormation formation, float angle)
    {
        Vector3[] offsets = FormationOffsets(formation);
        for (int i = 0; i < offsets.Length; i++)
        {
            offsets[i] = Quaternion.Euler(0f, angle, 0f) * offsets[i];
        }
        return offsets;
    }
}
