using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The class responsible for a creation and retriangulation of the terrain
/// </summary>
public class HexGrid : MonoBehaviour {

	[SerializeField]
	int cellCountX = 20, cellCountZ = 15;

	// Prefabs
	[SerializeField]
	HexCell cellPrefab;
	[SerializeField]
	Text cellCoordLabelPrefab;
	[SerializeField]
	CellResourceViewer cellResourceViewerPrefab;
	[SerializeField]
	HexGridChunk chunkPrefab;

	public bool createNewMapOnStart = true;

	[SerializeField]
	PathViewer pathViewer;

	[SerializeField]
	Material terrainMaterial;

	[SerializeField]
	Material waterMaterial;

	public int CellCountX { get => cellCountX; }
	public int CellCountZ { get => cellCountZ; }


	public void ShowPath(HexCoordinates from, HexCoordinates to, int speed, bool lastAction = false)
	{
		int[] cost;
		HexCoordinates[] path = HexMapMaster.Instance.FindPath(from, to, out cost, lastAction ? speed : speed * 2);
		pathViewer.BuildPath(path, cost, lastAction ? 0 : speed + 1);
	}

	public void HidePath()
	{
		pathViewer.ClearPath();
	}

	private void Update()
    {
		if (HexMapMaster.Instance.IsMapCreated)
			HexMapMaster.Instance.CenterMap();
    }

    public GridSaveData SaveData
    {
		get
        {
			GridSaveData data = new GridSaveData();
			data.cellCountX = cellCountX;
			data.cellCountZ = cellCountZ;
			List<CellSaveData> cellData = new List<CellSaveData>();
			foreach (HexCell cell in cells)
            {
				cellData.Add(cell.SaveData);
            }
			data.cellData = cellData.ToArray();
			return data;
        }
    }

	public void AttachToColumn(Transform obj, int index)
    {
		if (index >= 0 && index < columns.Length)
        {
			obj.SetParent(columns[index], false);
        }
		else
        {
			Debug.LogError("Column's index out of range");
        }
    }

	Transform[] columns;
	HexGridChunk[] chunks;
	HexCell[] cells;

	int chunkCountX, chunkCountZ;

	int currentCenterColumnIndex = -1;

	void Start () {
		if (createNewMapOnStart)
			CreateEmptyMap();
	}

	void CreateEmptyMap()
    {
		CreateMap(cellCountX, cellCountZ);
	}

	public bool LoadMap(GridSaveData data)
    {
		ClearMap();

		cellCountX = data.cellCountX;
		cellCountZ = data.cellCountZ;
		currentCenterColumnIndex = -1;
		chunkCountX = cellCountX / GameDataPresenter.Instance.HexMetrics.ChunkSizeX;
		chunkCountZ = cellCountZ / GameDataPresenter.Instance.HexMetrics.ChunkSizeZ;
		CreateChunks();
		CreateCells(data);
		GameDataPresenter.Instance.HexMetrics.MapWidth = cellCountX * GameDataPresenter.Instance.HexMetrics.InnerDiameter;
		return true;
	}

	public bool CreateMap (int x, int z)
	{
		if (
			x <= 0 || x % GameDataPresenter.Instance.HexMetrics.ChunkSizeX != 0 ||
			z <= 0 || z % GameDataPresenter.Instance.HexMetrics.ChunkSizeZ != 0
		) {
			Debug.LogError("Unsupported map size.");
			return false;
		}

		ClearMap();

		cellCountX = x;
		cellCountZ = z;
		currentCenterColumnIndex = -1;
		chunkCountX = cellCountX / GameDataPresenter.Instance.HexMetrics.ChunkSizeX;
		chunkCountZ = cellCountZ / GameDataPresenter.Instance.HexMetrics.ChunkSizeZ;
		CreateChunks();
		CreateCells();
		GameDataPresenter.Instance.HexMetrics.MapWidth = cellCountX * GameDataPresenter.Instance.HexMetrics.InnerDiameter;
		return true;
	}

	public void ClearMap()
    {
		HexMapMaster.Instance.ResetCells();
		if (columns != null)
		{
			for (int i = 0; i < columns.Length; i++)
			{
				Destroy(columns[i].gameObject);
			}
		}
	}

	public void CenterMap(float positionX)
    {
		float columnWidth = GameDataPresenter.Instance.HexMetrics.ChunkSizeX * GameDataPresenter.Instance.HexMetrics.InnerDiameter;

		int centerColumnNumber = Mathf.RoundToInt(positionX / columnWidth);
		int newCenterColumnIndex = centerColumnNumber % chunkCountX;
		while (newCenterColumnIndex < 0)
        {
			newCenterColumnIndex += chunkCountX;
        }

		if (currentCenterColumnIndex != newCenterColumnIndex)
		{
			int minColumnNumber = centerColumnNumber - (chunkCountX / 2);
			int maxColumnNumber = centerColumnNumber + (chunkCountX / 2);
			if (chunkCountX % 2 == 0)
			{
				minColumnNumber += 1;
			}

			for (int n = minColumnNumber; n <= maxColumnNumber; n++)
            {
				int index = n % chunkCountX;
				while (index < 0)
                {
					index += chunkCountX;
                }
				int offset = n >= 0 ? n / chunkCountX : ((n + 1) / chunkCountX) - 1;
				Vector3 newPosition = columns[index].position;
				newPosition.x = columnWidth * chunkCountX * offset;
				columns[index].position = newPosition;

			}

			currentCenterColumnIndex = newCenterColumnIndex;
		}
	}

	void CreateChunks () {
		columns = new Transform[chunkCountX];
		for (int x = 0; x < chunkCountX; x++) {
			columns[x] = new GameObject("Column").transform;
			columns[x].SetParent(transform, false);
		}

		chunks = new HexGridChunk[chunkCountX * chunkCountZ];
		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(columns[x], false);
			}
		}
	}

	void CreateCells () {
		cells = new HexCell[cellCountZ * cellCountX];

		for (int z = 0, i = 0; z < cellCountZ; z++) {
			for (int x = 0; x < cellCountX; x++) {
				CreateCell(x, z, i++);
			}
		}
	}

	void CreateCells(GridSaveData data)
	{
		cells = new HexCell[cellCountZ * cellCountX];

		for (int z = 0, i = 0; z < cellCountZ; z++)
		{
			for (int x = 0; x < cellCountX; x++)
			{
				CreateCell(x, z, i).LoadCell(data.cellData[i++]);
			}
		}
	}

	/// <summary>
	/// Get cell by raycast
	/// </summary>
	public HexCell GetCell (Ray ray) {
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			return GetCell(hit.point);
		}
		return null;
	}

	public HexCell GetCell (Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		return GetCell(coordinates);
	}

	public HexCell GetCell (HexCoordinates coordinates) {
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ) {
			return null;
		}
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX) {
			return null;
		}
		return cells[x + z * cellCountX];
	}

	public HexCell GetCell (int xOffset, int zOffset) {
		return cells[xOffset + zOffset * cellCountX];
	}

	public HexCell GetCell (int cellIndex) {
		return cells[cellIndex];
	}

	public void ShowCoords() {
		if (HexMapMaster.Instance.IsMapCreated)
		{

			for (int i = 0; i < chunks.Length; i++)
			{
				chunks[i].ShowCoords();
			}
		}
	}

	public void ShowResources()
	{
		if (HexMapMaster.Instance.IsMapCreated)
		{
			for (int i = 0; i < chunks.Length; i++)
			{
				chunks[i].ShowResources();
			}
		}
	}

	public void HideCanvas()
	{
		if (HexMapMaster.Instance.IsMapCreated)
		{
			for (int i = 0; i < chunks.Length; i++)
			{
				chunks[i].HideCanvas();
			}
		}
	}

	public void ShowGrid()
    {
		terrainMaterial.EnableKeyword("GRID_ON");
		waterMaterial.EnableKeyword("GRID_ON");
	}

	public void HideGrid()
    {
		terrainMaterial.DisableKeyword("GRID_ON");
		waterMaterial.DisableKeyword("GRID_ON");
	}

	HexCell CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * GameDataPresenter.Instance.HexMetrics.InnerDiameter;
		position.y = 0f;
		position.z = z * (GameDataPresenter.Instance.HexMetrics.OuterRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate(cellPrefab);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

		Text coordLabel = Instantiate(cellCoordLabelPrefab);
		coordLabel.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
		coordLabel.text = cell.coordinates.ToVerticalString();
		cell.coordRect = coordLabel.rectTransform;

		CellResourceViewer resourceViewer = Instantiate(cellResourceViewerPrefab);
		resourceViewer.cell = cell;
		RectTransform resourceViewerRect = resourceViewer.GetComponent<RectTransform>();
		resourceViewerRect.anchoredPosition = new Vector2(position.x, position.z);
		cell.resourceRect = resourceViewerRect;

		cell.Elevation = 0;

		AddCellToChunk(x, z, cell);
		HexMapMaster.Instance.AddCell(cell);

		return cell;
	}

	void AddCellToChunk (int x, int z, HexCell cell) {
		int chunkX = x / GameDataPresenter.Instance.HexMetrics.ChunkSizeX;
		int chunkZ = z / GameDataPresenter.Instance.HexMetrics.ChunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * GameDataPresenter.Instance.HexMetrics.ChunkSizeX;
		int localZ = z - chunkZ * GameDataPresenter.Instance.HexMetrics.ChunkSizeZ;
		chunk.AddCell(localX + localZ * GameDataPresenter.Instance.HexMetrics.ChunkSizeX, cell);
	}
}