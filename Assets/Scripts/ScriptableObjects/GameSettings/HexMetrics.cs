using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HexMetrics", menuName = "Hex Metrics")]
public class HexMetrics : ScriptableObject
{
	public static string Path
	{
		get => "GameSettings/HexMetrics";
	}

	public static HexMetrics Instance
	{
		get
		{
			HexMetrics metrix = Resources.Load<HexMetrics>(Path);
			if (metrix == null)
			{
				Debug.LogError("Could not find HexMetrics.asset");
			}
			return metrix;
		}
	}

	[SerializeField]
	float outerRadius = 10f;

	[SerializeField]
	float solidFactor = 0.75f;

	[SerializeField]
	float elevationStep = 1.5f;

	[SerializeField]
	float noiseStrenght = 5f;

	[SerializeField]
	float waterShoreFactor = 0.5f;

	[SerializeField]
	float feautersSpacing = 0.9f;

	[SerializeField]
	float noiseScale = 0.002f;

	[SerializeField]
	int terracesPerSlope = 2;

	/// <summary>
	/// Subdivision count devided by 2
	/// </summary>
	[SerializeField]
	int edgeSubdivisionLevel = 4;

	[SerializeField]
	Texture2D noiseSource;

	[SerializeField]
	int chunkSizeX = 5;
	[SerializeField]
	int chunkSizeZ = 5;

	public int ChunkSizeX { get => chunkSizeX; }
	public int ChunkSizeZ { get => chunkSizeZ; }

	public float HorizontalTerraceStepSize { get => 1f / TerraceSteps; }
	public float VerticalTerraceStepSize { get => 1f / (terracesPerSlope + 1); }
	public int TerracesPerSlope { get => terracesPerSlope; }
	public int TerraceSteps { get => terracesPerSlope * 2 + 1; }
	public float ElevationStep { get => elevationStep; }
	public float OuterRadius { get => outerRadius; }
	public float InnerRadius { get => outerRadius * 0.866025404f; }
	public float InnerDiameter { get => InnerRadius * 2; }
	public float NoiseStrenght { get => noiseStrenght; }
	public int EdgeSubdivisionLevel { get => edgeSubdivisionLevel * 2; }

	public float SolidFactor { get => solidFactor; }
	public float BlendFactor { get => 1f - solidFactor; }

	public float WaterShoreFactor { get => waterShoreFactor; }

	public Texture2D NoiseSource { get => noiseSource; set => noiseSource = value; }
	public float MapWidth { get; set; }

	public Vector3[] Corners
	{
		get => new Vector3[]{
			new Vector3(0f, 0f, OuterRadius),
			new Vector3(InnerRadius, 0f, 0.5f * OuterRadius),
			new Vector3(InnerRadius, 0f, -0.5f * OuterRadius),
			new Vector3(0f, 0f, -OuterRadius),
			new Vector3(-InnerRadius, 0f, -0.5f * OuterRadius),
			new Vector3(-InnerRadius, 0f, 0.5f * OuterRadius)
		};
	}

	public Vector3 GetFirstCorner(HexDirection direction)
	{
		return Corners[(int)direction];
	}

	public Vector3 GetSecondCorner(HexDirection direction)
	{
		return Corners[((int)direction + 1) % 6];
	}

	public Vector3 GetFirstSolidCorner(HexDirection direction)
	{
		return GetFirstCorner(direction) * SolidFactor;
	}
	public Vector3 GetSecondSolidCorner(HexDirection direction)
	{
		return GetSecondCorner(direction) * SolidFactor;
	}

	public Vector3 GetBridge(HexDirection direction)
	{
		return (GetFirstCorner(direction) + GetSecondCorner(direction)) * BlendFactor;
	}

	public Vector3 GetFeaturesOffset(HexDirection direction)
	{
		return (GetFirstCorner(direction) + GetSecondCorner(direction)) * 0.5f * solidFactor * feautersSpacing;
	}

	public Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
	{
		float h = step * HorizontalTerraceStepSize;
		a.x += (b.x - a.x) * h;
		a.z += (b.z - a.z) * h;
		float v = ((1 + step) / 2) * VerticalTerraceStepSize;
		a.y += (b.y - a.y) * v;
		return a;
	}

	public Vector3 CliffLerp(Vector3 a, Vector3 b, int step)
	{
		float h = ((1 + step) / 2) * VerticalTerraceStepSize;
		a.x += (b.x - a.x) * h;
		a.z += (b.z - a.z) * h;
		float v = ((1 + step) / 2) * VerticalTerraceStepSize;
		a.y += (b.y - a.y) * v;
		return a;
	}

	public Color TerraceLerp(Color a, Color b, int step)
	{
		float h = step * HorizontalTerraceStepSize;
		return Color.Lerp(a, b, h);
	}

	public HexEdgeType GetEdgeType(int elevation1, int elevation2)
	{
		if (elevation1 == elevation2)
			return HexEdgeType.Flat;
		int delta = Mathf.Abs(elevation1 - elevation2);
		if (delta == 1)
		{
			return HexEdgeType.Slope;
		}
		return HexEdgeType.Cliff;
	}

	public Vector4 GetNoise(Vector3 position)
	{
		position *= noiseScale;
		if (noiseSource)
			return noiseSource.GetPixelBilinear(position.x, position.z);
		else
			return Vector4.zero;
	}

	public Vector4 GetLoopedNoise(Vector3 position, float noiseScale = 1f)
	{
		Vector4 originalNoise = GetNoise(position * noiseScale);
		position.x -= MapWidth;
		Vector4 additiveNoise = GetNoise(position * noiseScale);
		float alpha = (Mathf.Cos(position.x / MapWidth * Mathf.PI) + 1) * 0.5f;
		return Vector4.Lerp(originalNoise, additiveNoise, alpha);
	}
}
