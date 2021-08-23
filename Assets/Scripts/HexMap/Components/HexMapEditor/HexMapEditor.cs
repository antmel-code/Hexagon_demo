using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{

    public Material terrainMaterial;
    public Material waterMaterial;

    string selectedNameOfHexType;

    string selectedNameOfBuildingType;

    public HexGrid hexGrid;

    public HexGridSaver saver;

    private int activeElevation;

    private int brushSize;

    bool applyType = false;

    bool applyElevation = true;

    bool isDrag;

    HexDirection dragDirection;

    HexCell previousCell;

    enum OptionalToggle
    {
        Ignore, Yes, No
    }

    OptionalToggle roadMode = OptionalToggle.Ignore;

    OptionalToggle buildingsMode = OptionalToggle.Ignore;

    private void Start()
    {
        ShowGrid(false);
    }

    public void SaveMap()
    {
        saver.Save();
    }

    public void LoadMap()
    {
        saver.Load();
    }

    public void ShowGrid(bool visible)
    {
        if (visible)
        {
            terrainMaterial.EnableKeyword("GRID_ON");
            waterMaterial.EnableKeyword("GRID_ON");
        }
        else
        {
            terrainMaterial.DisableKeyword("GRID_ON");
            waterMaterial.DisableKeyword("GRID_ON");
        }
    }

    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }

    public void SetBuildingsMode(int mode)
    {
        buildingsMode = (OptionalToggle)mode;
    }

    public void SelectHexType(string newNameOfType)
    {
        selectedNameOfHexType = newNameOfType;
    }

    public void SelectBuildingType(string newNameOfType)
    {
        selectedNameOfBuildingType = newNameOfType;
    }

    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    public void SetApplyType(bool value)
    {
        applyType = value;
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }
    private void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
        else
        {
            previousCell = null;
        }
    }

    private void HandleInput()
    {
        isDrag = false;
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            HexCell currentCell = HexMapMaster.Instance.GetCellOnMap(hit.point);
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            EditCells(currentCell, brushSize);
            previousCell = currentCell;
        }
        else
        {
            previousCell = null;
        }
    }

    void ValidateDrag (HexCell currentCell)
    {
        for (dragDirection = HexDirection.NE; dragDirection <= HexDirection.NW; dragDirection++)
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    public void EditCells(HexCell center, int radius)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(HexMapMaster.Instance.GetCellOnMap(new HexCoordinates(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(HexMapMaster.Instance.GetCellOnMap(new HexCoordinates(x, z)));
            }
        }
    }

    private void EditCell(HexCell cell)
    {
        if (!cell)
        {
            return;
        }
        if (applyType)
        {
            cell.NameOfHexType = selectedNameOfHexType;
        }
        if (applyElevation)
        {
            cell.Elevation = activeElevation;
        }
        if (roadMode == OptionalToggle.No)
        {
            cell.RemoveRoads();
        }
        if (buildingsMode == OptionalToggle.No)
        {
            cell.RemoveBuilding();
        }
        if (buildingsMode == OptionalToggle.Yes)
        {
            cell.Build(selectedNameOfBuildingType, PlayerIdentifier.None);
        }
        if (isDrag)
        {
            HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
            if (roadMode == OptionalToggle.Yes)
            {
                otherCell.AddRoad(dragDirection);
            }
        }
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void ShowCoords()
    {
        if (hexGrid)
            hexGrid.ShowCoords();
    }

    public void ShowResources()
    {
        if (hexGrid)
            hexGrid.ShowResources();
    }

    public void HideCanvas()
    {
        if (hexGrid)
            hexGrid.HideCanvas();
    }
}
