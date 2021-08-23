using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitAbilityViewer : MonoBehaviour
{
    Ability ability;

    [SerializeField]
    Image image;

    [SerializeField]
    Button button;

    bool interactable = true;

    public bool Interactable
    {
        get => interactable;
        set
        {
            interactable = value;
            UpdateInteractivity();
        }
    }

    public AbilityType AbilityType
    {
        get => ability.AbilityType;
    }

    public Ability Ability
    {
        get => ability;
        set => ability = value;
    }

    private void Start()
    {
        ForceUpdate();
    }

    public void ForceUpdate()
    {
        UpdateImage();
        UpdateInteractivity();
    }

    void UpdateImage()
    {
        if (ability != null)
            image.sprite = GameDataPresenter.Instance.GetSpriteByAbilityType(ability.AbilityType);
    }

    void UpdateInteractivity()
    {
        if (ability == null)
            return;
        if (interactable && ability.CanBeUsed())
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }
}
