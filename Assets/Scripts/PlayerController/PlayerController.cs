using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode { Survey, UnitsManaging, CitiesManaging, Aiming, Waiting }

[RequireComponent(typeof(HexSelector))]
[RequireComponent(typeof(UnitsController))]
[RequireComponent(typeof(CityController))]
public class PlayerController : Singleton<PlayerController>
{
    public static event System.Action onSurveyModEnter = () =>
    {
        onUnitManagingModExit();
        onCitiesManagingModExit();
        onAimingModExit();
        onWaitingModExit();
    };
    public static event System.Action onSurveyModExit = () => { };
    public static event System.Action onUnitManagingModEnter = () =>
    {
        onSurveyModExit();
        onCitiesManagingModExit();
        onAimingModExit();
        onWaitingModExit();
    };
    public static event System.Action onUnitManagingModExit = () => { };
    public static event System.Action onCitiesManagingModEnter = () =>
    {
        onSurveyModExit();
        onUnitManagingModExit();
        onAimingModExit();
        onWaitingModExit();
    };
    public static event System.Action onCitiesManagingModExit = () => { };
    public static event System.Action onAimingModEnter = () =>
    {
        onSurveyModExit();
        onUnitManagingModExit();
        onCitiesManagingModExit();
        onWaitingModExit();
    };
    public static event System.Action onAimingModExit = () => { };
    public static event System.Action onWaitingModEnter = () =>
    {
        onSurveyModExit();
        onUnitManagingModExit();
        onCitiesManagingModExit();
        onAimingModExit();
    };
    public static event System.Action onWaitingModExit = () => { };
    public static event System.Action onOptionsEnter = () => { };
    public static event System.Action onOptionsExit = () => { };

    UnitsController unitsController;

    CityController cityController;

    GameMode currentMode = GameMode.Survey;

    HexSelector hexSelector;

    bool options = false;

    public UnitsController UnitController
    {
        get => unitsController;
    }

    public GameMode Mode
    {
        get => currentMode;
        set
        {
            if (currentMode == value)
                return;
            currentMode = value;
            if (currentMode == GameMode.Survey)
            {
                onSurveyModEnter();
            }
            else if (currentMode == GameMode.UnitsManaging)
            {
                onUnitManagingModEnter();
            }
            else if (currentMode == GameMode.CitiesManaging)
            {
                onCitiesManagingModEnter();
            }
            else if (currentMode == GameMode.Aiming)
            {
                onAimingModEnter();
            }
            else
            {
                onWaitingModEnter();
            }
        }
    }

    public void OptionEnter()
    {
        if (options)
            return;
        options = true;
        onOptionsEnter();
    }

    public void OptionExit()
    {
        if (!options)
            return;
        options = false;
        onOptionsExit();
    }

    private void Awake()
    {
        base.Awake();

        hexSelector = GetComponent<HexSelector>();
        unitsController = GetComponent<UnitsController>();
        cityController = GetComponent<CityController>();

        hexSelector.OnDrag += HexDrag;
        hexSelector.OnMouseDown += HexMouseDown;
        hexSelector.OnMouseUp += HexMouseUp;

        unitsController.onUnitLost += TakeControl;
    }

    private void OnDestroy()
    {
        hexSelector.OnDrag -= HexDrag;
        hexSelector.OnMouseDown -= HexMouseDown;
        hexSelector.OnMouseUp -= HexMouseUp;

        unitsController.onUnitLost -= TakeControl;
    }

    void TakeControl()
    {
        currentMode = GameMode.Survey;
    }

    void HexMouseUp(HexCoordinates hex)
    {
        if (currentMode == GameMode.Survey)
        {
            if (HexMapMaster.Instance.IsThereUnit(hex))
            {
                unitsController.ActiveUnit = HexMapMaster.Instance.GetUnitOnMap(hex);
                Mode = GameMode.UnitsManaging;
            }
            else if (HexMapMaster.Instance.IsThereCity(hex))
            {
                cityController.ActiveCity = HexMapMaster.Instance.GetCityOnMap(hex);
                Mode = GameMode.CitiesManaging;
            }
        }
        else if (currentMode == GameMode.UnitsManaging)
        {
            if (HexMapMaster.Instance.IsThereUnit(hex))
            {
                unitsController.ActiveUnit = HexMapMaster.Instance.GetUnitOnMap(hex);
            }
            else
            {
                unitsController.HexMouseUp(hex);
            }
        }
        else if (currentMode == GameMode.CitiesManaging)
        {
            cityController.HexMouseUp(hex);
        }
        else
        {

        }
    }

    void HexMouseDown(HexCoordinates hex)
    {
        if (currentMode == GameMode.Survey)
        {

        }
        else if (currentMode == GameMode.UnitsManaging)
        {
            unitsController.HexMouseDown(hex);
        }
        else if (currentMode == GameMode.CitiesManaging)
        {
            cityController.HexMouseDown(hex);
        }
        else
        {

        }
    }

    void HexDrag(HexCoordinates hex)
    {
        if (currentMode == GameMode.Survey)
        {

        }
        else if (currentMode == GameMode.UnitsManaging)
        {
            unitsController.HexMouseDrag(hex);
        }
        else if (currentMode == GameMode.CitiesManaging)
        {
            cityController.HexMouseDrag(hex);
        }
        else
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (Mode != GameMode.Survey)
            {
                Mode = GameMode.Survey;
                unitsController.ActiveUnit = null;
                cityController.ActiveCity = null;
            }
            else
            {
                if (!options)
                    OptionEnter();
                else
                    OptionExit();
            }
        }
    }
}
