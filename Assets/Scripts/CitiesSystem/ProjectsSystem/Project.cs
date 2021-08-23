using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectType { Unit, District }
public enum ProjectUniqueness { Common, UniqueForCity, UniqueForEmpire, UniqueForWorld }

public abstract class Project
{
    int ownerIndex = 0;
    ResourceStack[] cost = new ResourceStack[0];
    int duration = 1;
    ProjectUniqueness uniqueness = ProjectUniqueness.Common;

    int remainingTrurns = 1;
    public void Progress()
    {
        remainingTrurns--;
        if (remainingTrurns <= 0)
        {
            Activate();
        }
    }

    abstract protected void Activate();

    abstract public ProjectType Type { get; }
}
