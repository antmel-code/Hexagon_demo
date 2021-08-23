using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsController : MonoBehaviour
{
    public event System.Action onUnitLost = () => { };
    public event System.Action<Unit> onActiveUnitChange = (Unit) => { };
    public event System.Action onActiveUnitAction = () => { };
    public event System.Action onActiveUnitRecover = () => { };

    Unit activeUnit;

    public Unit ActiveUnit
    {
        get => activeUnit;
        set
        {
            if (activeUnit)
            {
                activeUnit.ShowIndicator = false;
                HexMapMaster.Instance.HidePath();
                activeUnit.onDestroy -= DeactivateUnit;
                activeUnit.onAction -= onActiveUnitAction;
                activeUnit.onRecover -= onActiveUnitRecover;
            }
            activeUnit = value;
            if (activeUnit)
            {
                activeUnit.ShowIndicator = true;
                HexMapMaster.Instance.HidePath();
                activeUnit.onDestroy += DeactivateUnit;
                activeUnit.onAction += onActiveUnitAction;
                activeUnit.onRecover += onActiveUnitRecover;
                onActiveUnitChange(activeUnit);
            }
            else
            {
                onUnitLost();
            }
        }
    }

    void DeactivateUnit()
    {
        ActiveUnit = null;
    }

    public void UseAbility(int index)
    {
        if (activeUnit)
        {
            activeUnit.UseAbility(index);
        }
    }

    public void HexMouseDown(HexCoordinates hex)
    {

    }

    public void HexMouseUp(HexCoordinates hex)
    {
        if (!HexMapMaster.Instance.AreThereMovingUnits)
        {
            if (activeUnit.IsReachable(hex))
            {
                activeUnit.GoTo(hex);
                HexMapMaster.Instance.HidePath();
            }
        }
    }

    public void HexMouseDrag(HexCoordinates hex)
    {
        if (!HexMapMaster.Instance.AreThereMovingUnits && ActiveUnit.RemainingActionCount > 0)
        {
            HexMapMaster.Instance.ShowPath(activeUnit.CurrentHex, hex, activeUnit.Speed, ActiveUnit.RemainingActionCount == 1);
        }
    }
}
