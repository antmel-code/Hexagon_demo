using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundCity : Ability
{
    public override AbilityType AbilityType
    {
        get => AbilityType.FoundCity;
    }

    public override void Activate()
    {
        var cell = HexMapMaster.Instance.GetCellOnMap(owner.CurrentHex);
        cell.Build("Village", owner.Owner);
        HexMapMaster.Instance.BuildCity(owner.CurrentHex);
        owner.Destroy();
    }

    public override bool CanBeUsed()
    {
        return base.CanBeUsed() && !HexMapMaster.Instance.GetCellOnMap(owner.CurrentHex).IsWater;
    }
}
