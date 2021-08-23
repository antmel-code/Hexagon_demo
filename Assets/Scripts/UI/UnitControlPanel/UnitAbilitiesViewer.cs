using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitAbilitiesViewer : MonoBehaviour
{
    UnitAbilityViewer[] abilityViewers;

    bool isInteractable = true;

    public bool Interactable
    {
        get => isInteractable;
        set
        {
            if (isInteractable == value)
                return;
            isInteractable = value;
            if (isInteractable)
            {
                EnableAbilities();
            }
            else
            {
                DisablaAbilities();
            }
        }
    }

    private void Awake()
    {
        abilityViewers = GetComponentsInChildren<UnitAbilityViewer>();
    }

    public void UpdateAbilities(Unit unit)
    {
        gameObject.SetActive(true);
        UpdateViewers(unit);

    }

    public void UpdateOnlyInteractivity()
    {
        foreach (UnitAbilityViewer viewer in abilityViewers)
        {
            if (viewer.gameObject.activeSelf)
            {
                viewer.ForceUpdate();
            }
        }
    }

    void DisablaAbilities()
    {
        foreach (UnitAbilityViewer abilityViewer in abilityViewers)
        {
            abilityViewer.Interactable = false;
        }
    }

    void EnableAbilities()
    {
        foreach (UnitAbilityViewer abilityViewer in abilityViewers)
        {
            abilityViewer.Interactable = true;
        }
    }

    // Update is called once per frame
    void UpdateViewers(Unit unit)
    {
        for (int i = 0; i < abilityViewers.Length; i++)
        {
            Ability[] unitAbilities = unit ? unit.AbilityInstances : new Ability[0];
            if (i < unitAbilities.Length)
            {
                abilityViewers[i].gameObject.SetActive(true);
                abilityViewers[i].Ability = unitAbilities[i];
                abilityViewers[i].ForceUpdate();
            }
            else
            {
                abilityViewers[i].gameObject.SetActive(false);
            }
        }
    }
}
