using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class for map managing
/// </summary>
public class HexMapMaster : Singleton<HexMapMaster>
{
    public static event System.Action onMapCreated = () => { };
    public static event System.Action onUnitsStartMoving = () => { };
    public static event System.Action onUnitsFinishMoving = () => { };

    [SerializeField]
    HexGrid grid;

    [SerializeField]
    Transform cameraBase;

    [SerializeField]
    Unit unitPrefab;

    Dictionary<HexCoordinates, HexCell> map = new Dictionary<HexCoordinates, HexCell>();

    Dictionary<HexCoordinates, Unit> units = new Dictionary<HexCoordinates, Unit>();

    Dictionary<int, City> cities = new Dictionary<int, City>();

    int movingUnitsCount = 0;

    int newCityIndex
    {
        get
        {
            int i = 0;
            while (true)
            {
                if (!cities.ContainsKey(i))
                {
                    return i;
                }
                i++;
            }
        }
    }

    public void CenterMap()
    {
        grid.CenterMap(cameraBase.position.x);
    }

    public void LookAtCell(HexCoordinates coord)
    {
        coord = LoopHexCoordinates(coord);
        Vector3 newPosition = GetCellOnMap(coord).transform.position;
        newPosition.y = cameraBase.position.y;
        cameraBase.position = newPosition;
    }

    public void BuildCity(HexCoordinates hex, string name = "City")
    {
        int cityIndex = newCityIndex;
        City newCity = new City();
        newCity.Name = name;
        newCity.Center = hex;
        cities.Add(cityIndex, newCity);
        GetCellOnMap(hex).CityIndex = cityIndex;
    }

    public void ShowPath(HexCoordinates from, HexCoordinates to, int speed, bool lastAction = false)
    {
        grid.ShowPath(from, to, speed, lastAction);
    }

    public void HidePath()
    {
        grid.HidePath();
    }

    public bool IsMapCreated
    {
        get
        {
            return map.Count == grid.CellCountX * grid.CellCountZ;
        }
    }

    public bool AreThereMovingUnits
    {
        get => movingUnitsCount > 0;
    }

    private void Awake()
    {
        base.Awake();
        ResetCells();

        
    }

    public void SpawnUnit(HexCoordinates hex, string architypeName)
    {
        SpawnUnit(hex, GameDataPresenter.Instance.GetUnitArchitypeByName(architypeName));
    }

    public void SpawnUnit(HexCoordinates hex, UnitArchitype architype)
    {
        Unit unit = Instantiate(unitPrefab);
        unit.CurrentHex = hex;

        unit.Owner = PlayerIdentifier.Player1;

        AddUnit(hex, unit);
    }

    public void DestroyUnit(HexCoordinates hex)
    {
        RemoveUnit(hex);
    }

    void AddUnit(HexCoordinates hex, Unit newUnit)
    {
        hex = LoopHexCoordinates(hex);

        units.Add(LoopHexCoordinates(hex), newUnit);
        newUnit.onMove += MoveUnit;
        newUnit.onMoveStart += UnitMoveStart;
        newUnit.onMoveFinish += UnitMoveFinish;

        int altX = hex.X + (hex.Z / 2);
        int columnIndex = altX / GameDataPresenter.Instance.HexMetrics.ChunkSizeX;
        grid.AttachToColumn(newUnit.transform, columnIndex);
    }

    void RemoveUnit(HexCoordinates hex)
    {
        hex = LoopHexCoordinates(hex);
        if (units.ContainsKey(hex))
        {
            units[hex].onMove -= MoveUnit;
            units[hex].onMoveStart -= UnitMoveStart;
            units[hex].onMoveFinish -= UnitMoveFinish;
            units.Remove(hex);
            
        }
    }

    void MoveUnit(HexCoordinates from, HexCoordinates to)
    {
        from = LoopHexCoordinates(from);
        to = LoopHexCoordinates(to);

        if (from == to)
        {
            return;
        }

        int fromAltX = from.X + (from.Z / 2);
        int toAltX = to.X + (to.Z / 2);
        bool leftEdge = fromAltX == 0 && toAltX == grid.CellCountX - 1;
        bool rightEdge = toAltX == 0 && fromAltX == grid.CellCountX - 1;

        if (units.ContainsKey(from))
        {
            if (!units.ContainsKey(to))
            {
                AddUnit(to, units[from]);
                RemoveUnit(from);
                
                if (leftEdge)
                {
                    units[to].transform.Translate(new Vector3(GameDataPresenter.Instance.HexMetrics.MapWidth, 0f, 0f));
                }
                else if (rightEdge)
                {
                    units[to].transform.Translate(new Vector3(-GameDataPresenter.Instance.HexMetrics.MapWidth, 0f, 0f));
                }
                int columnIndex = toAltX / GameDataPresenter.Instance.HexMetrics.ChunkSizeX;
                grid.AttachToColumn(units[to].transform, columnIndex);
            }
            else
            {
                Debug.LogWarning("There is unit in " + to.ToString());
            }
        }
        else
        {
            Debug.LogWarning("There is no unit in " + from.ToString());
        }
    }

    public HexCoordinates[] FindPath(HexCoordinates from, HexCoordinates to, out int[] cost, int costLimit = 20)
    {
        to = LoopHexCoordinates(to);
        from = LoopHexCoordinates(from);

        if (from == to)
        {
            cost = new int[0];
            return new HexCoordinates[0];
        }

        // Fonding path
        bool isFunded = false;
        PriorityQueue<HexCoordinates> queue = new PriorityQueue<HexCoordinates>();
        Dictionary<HexCoordinates, int> hexCosts = new Dictionary<HexCoordinates, int>();

        hexCosts.Add(from, 0);
        queue.Enqueue(0, from);

        while (!isFunded)
        {
            if (queue.Count == 0)
            {
                cost = new int[0];
                return new HexCoordinates[0];
            }
            HexCoordinates current = queue.Dequeue();

            // Check cost limit
            if (hexCosts[current] > costLimit)
            {
                cost = new int[0];
                return new HexCoordinates[0];
            }
            else if (current == to)
            {
                isFunded = true;
            }

            if (!isFunded)
            {
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    HexCoordinates neighbor = LoopHexCoordinates(current.GetNeighbor(d));
                    if (GetCellOnMap(neighbor) != null && GetCellOnMap(current).GetEdgeType(d) != HexEdgeType.Cliff)
                    {
                        int coast = GetCellOnMap(current).HasRoadThroughEdge(d) ? 1 : 2;
                        if (IsThereUnit(LoopHexCoordinates(current.GetNeighbor(d))))
                        {
                            coast = 1000000;
                        }
                        int value = hexCosts[current] + coast;

                        if (!hexCosts.ContainsKey(neighbor))
                        {
                            hexCosts.Add(neighbor, value);
                            queue.Enqueue(-value, neighbor);
                        }
                        else if (hexCosts[neighbor] > value)
                        {
                            hexCosts[neighbor] = value;
                            queue.UpdatePriorityForAllSimilarObjects(neighbor, -value);
                        }
                    }
                }
            }
        }

        // Check cost limit
        if (hexCosts[to] > costLimit)
        {
            cost = new int[0];
            return new HexCoordinates[0];
        }

        // Building path
        Stack<HexCoordinates> path = new Stack<HexCoordinates>();

        path.Push(to);

        while (path.Peek() != from)
        {
            int minValue = hexCosts[to];
            HexCoordinates nextHex = new HexCoordinates(0, 0);
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                if (GetCellOnMap(path.Peek().GetNeighbor(d)) && GetCellOnMap(path.Peek()).GetEdgeType(d) != HexEdgeType.Cliff)
                {
                    HexCoordinates neighbor = LoopHexCoordinates(path.Peek().GetNeighbor(d));
                    if (hexCosts.ContainsKey(neighbor))
                    {
                        if (hexCosts[neighbor] < minValue)
                        {
                            minValue = hexCosts[neighbor];
                            nextHex = neighbor;
                        }
                    }
                }
            }
            path.Push(nextHex);
        }

        //Build cost
        List<int> costList = new List<int>();
        foreach (HexCoordinates hex in path)
        {
            costList.Add(hexCosts[hex]);
        }

        cost = costList.ToArray();
        return path.ToArray(); ;
    }

    public HexCell GetCellOnMap(HexCoordinates coordinates)
    {
        if (map.ContainsKey(coordinates))
            return map[coordinates];
        else
        {
            HexCoordinates loopedCoord = LoopHexCoordinates(coordinates);

            if (map.ContainsKey(loopedCoord))
                return map[loopedCoord];
            else
                return null;
        }
    }

    public City GetCityByIndex(int index)
    {
        if (cities.ContainsKey(index))
            return cities[index];
        else
            return null;
    }

    public City GetCityOnMap(HexCoordinates hex)
    {
        return GetCityByIndex(GetCellOnMap(hex).CityIndex);
    }

    void UnitMoveStart()
    {
        if (movingUnitsCount == 0)
        {
            onUnitsStartMoving();
        }
        movingUnitsCount++;
    }

    void UnitMoveFinish()
    {
        movingUnitsCount--;
        if (movingUnitsCount == 0)
        {
            onUnitsFinishMoving();
        }
    }

    HexCoordinates LoopHexCoordinates(HexCoordinates originalCoords)
    {
        int altX = originalCoords.X + (originalCoords.Z / 2);

        bool rightEdge = altX >= grid.CellCountX;
        bool leftEdge = altX < 0;
        bool topEdge = originalCoords.Z >= grid.CellCountZ;
        bool bottomEdge = originalCoords.Z < 0;

        if (!topEdge && !bottomEdge && (rightEdge || leftEdge))
        {
            int z = originalCoords.Z;
            int loopCount = rightEdge ? Mathf.Abs(altX) / grid.CellCountX : ((Mathf.Abs(altX) - 1) / grid.CellCountX) + 1;
            int x = rightEdge ? (altX - grid.CellCountX * loopCount) - (z / 2) : (altX + grid.CellCountX * loopCount) - (z / 2);

            return new HexCoordinates(x, z);

        }
        else
        {
            return new HexCoordinates(originalCoords.X, originalCoords.Z);
        }
    }

    public HexCell GetCellOnMap(Vector3 position)
    {
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        HexCell targetCell = GetCellOnMap(coordinates);
        return targetCell;
    }

    public Unit GetUnitOnMap(HexCoordinates hex)
    {
        hex = LoopHexCoordinates(hex);
        if (units.ContainsKey(hex))
        {
            return units[hex];
        }
        else
        {
            return null;
        }
    }

    public bool IsThereUnit(HexCoordinates hex)
    {
        return units.ContainsKey(LoopHexCoordinates(hex));
    }

    public bool IsThereCity(HexCoordinates hex)
    {
        return GetCellOnMap(hex).IsThereBuilding && cities.ContainsKey(GetCellOnMap(hex).CityIndex);
    }

    public void ResetCells()
    {
        map.Clear();
    }

    public void AddCell(HexCell cell)
    {
        map.Add(cell.coordinates, cell);
        if (IsMapCreated)
        {
            onMapCreated.Invoke();
        }
    }

}
