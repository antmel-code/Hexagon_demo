using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridChunk : MonoBehaviour
{
	HexCell[] cells;

	TerrainMesh terrain;

	RoadMesh roads;

	WaterMesh water;

	OverlayMesh overlay;

	HexFeatureManager features;

	HexMapCanvas gridCanvas;

	void Awake()
	{
		gridCanvas = GetComponentInChildren<HexMapCanvas>();
		terrain = GetComponentInChildren<TerrainMesh>();
		terrain.Init();
		roads = GetComponentInChildren<RoadMesh>();
		roads.Init();
		water = GetComponentInChildren<WaterMesh>();
		water.Init();
		features = GetComponentInChildren<HexFeatureManager>();
		overlay = GetComponentInChildren<OverlayMesh>();
		overlay.Init();
		cells = new HexCell[GameDataPresenter.Instance.HexMetrics.ChunkSizeX * GameDataPresenter.Instance.HexMetrics.ChunkSizeZ];
	}

    private void Start()
    {
		HideCanvas();
    }

    public void ShowCoords()
	{
		gridCanvas.ShowCoords();
	}

	public void ShowResources()
	{
		gridCanvas.ShowResources();
	}

	public void HideCanvas()
    {
		gridCanvas.HideCanvas();
	}

	public void AddCell(int index, HexCell cell)
    {
		cells[index] = cell;
		cell.transform.parent = transform;
		cell.chunk = this;
		cell.coordRect.SetParent(gridCanvas.Coords.transform, false);
		cell.resourceRect.SetParent(gridCanvas.Resources.transform, false);
	}

    private void LateUpdate()
    {
		CalculateBoundaries();
		Triangulate();
		gridCanvas.RefreshResources();
		enabled = false;
    }

	void Triangulate()
    {
		terrain.Clear();
		roads.Clear();
		water.Clear();
		overlay.Clear();
		features.Clear();
		for (int i = 0; i < cells.Length; i++)
        {
			Triangulate(cells[i]);
        }
		terrain.Apply();
		roads.Apply();
		water.Apply();
		overlay.Apply();
		features.Apply();
    }

	void Triangulate(HexCell cell)
    {
		terrain.Triangulate(cell);
		roads.Triangulate(cell);
		water.Triangulate(cell);
		overlay.Triangulate(cell);
		features.AddFeatures(cell);
    }

	void CalculateBoundaries()
    {
		for (int i = 0; i < cells.Length; i++)
		{
			HexCell cell = cells[i];
			if (!cell.IsThereBuilding || cell.IsThereBuilding && cell.Owner == PlayerIdentifier.None)
			{
				for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
				{
					HexCell neighbor = cell.GetNeighbor(d) ? cell.GetNeighbor(d) : cell;
					if (neighbor.IsThereBuilding && neighbor.Owner != PlayerIdentifier.None)
					{
						cell.Owner = neighbor.Owner;
						break;
					}
					if (d == HexDirection.NW)
					{
						cell.Owner = PlayerIdentifier.None;
					}
				}
			}
		}
	}


	public void Refresh()
    {
		enabled = true;
	}
}
