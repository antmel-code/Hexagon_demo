using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityController : MonoBehaviour
{
    public event System.Action onCityLost = () => { };
    public event System.Action<City> onActiveCityChange = (City) => { };

    City activeCity;

    public City ActiveCity
    {
        get => activeCity;
        set
        {
            if (activeCity == value)
                return;
            activeCity = value;
            if (activeCity == null)
            {
                onCityLost();
            }
            else
            {
                onActiveCityChange(activeCity);
            }
        }
    }

    // Temp code for demo
    public void SpawnUnitInActiveCity()
    {
        if (CanSpawnUnitInActiveCity)
        {
            HexMapMaster.Instance.SpawnUnit(activeCity.Center, "Colonialists");
        }
    }

    public bool CanSpawnUnitInActiveCity
    {
        get => !HexMapMaster.Instance.IsThereUnit(activeCity.Center);
    }
    // end

    public void HexMouseDown(HexCoordinates hex)
    {

    }

    public void HexMouseUp(HexCoordinates hex)
    {
        
    }

    public void HexMouseDrag(HexCoordinates hex)
    {
        
    }
}
