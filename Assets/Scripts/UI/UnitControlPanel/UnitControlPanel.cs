using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitControlPanel : MonoBehaviour
{
    [SerializeField]
    UnitAbilitiesViewer abilitiesViewer;

    private void Start()
    {
        BindEvents();
        UnitLost();
    }

    void BindEvents()
    {
        UnitsController uc = PlayerController.Instance.UnitController;
        uc.onActiveUnitChange += ChangeActiveUnit;
        uc.onUnitLost += UnitLost;
        uc.onActiveUnitAction += Refresh;
        uc.onActiveUnitRecover += Refresh;
        HexMapMaster.onUnitsStartMoving += DisableAbilities;
        HexMapMaster.onUnitsFinishMoving += EnableAbilities;
    }

    void DisableAbilities()
    {
        abilitiesViewer.Interactable = false;
    }

    void EnableAbilities()
    {
        abilitiesViewer.Interactable = true;
    }

    private void OnDestroy()
    {
        UnitsController uc = PlayerController.Instance.UnitController;
        uc.onActiveUnitChange -= ChangeActiveUnit;
        uc.onUnitLost -= UnitLost;
        uc.onActiveUnitAction -= Refresh;
        uc.onActiveUnitRecover -= Refresh;
        HexMapMaster.onUnitsStartMoving -= DisableAbilities;
        HexMapMaster.onUnitsFinishMoving -= EnableAbilities;
    }

    void UnitLost()
    {
        gameObject.SetActive(false);
    }

    void ChangeActiveUnit(Unit unit)
    {
        gameObject.SetActive(true);
        abilitiesViewer.UpdateAbilities(unit);
    }

    void Refresh()
    {
        abilitiesViewer.UpdateOnlyInteractivity();
    }

}
