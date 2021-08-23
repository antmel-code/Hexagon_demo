using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexFeatureManager : MonoBehaviour
{
    [SerializeField]
    Transform[] trees;

    [SerializeField, Range(0, 2)]
    float treesMinScale = 0.5f;

    [SerializeField, Range(0, 2)]
    float treesMaxScale = 1f;

    [SerializeField]
    float scaleNoise = 1;

    [SerializeField, Range(0, 360)]
    int treesMinAngle = 0;

    [SerializeField, Range(0, 360)]
    int treesMaxAngle = 360;

    [SerializeField]
    float angleNoise = 1;

    List<GameObject> features;

    private void Awake()
    {
        features = new List<GameObject>();
    }

    public void Clear()
    {
        foreach(GameObject current in features)
        {
            Destroy(current);
        }
    }

    public void Apply()
    {

    }

    public void AddFeatures(HexCell cell)
    {
        if (cell.IsThereBuilding)
        {
            GameObject building = cell.Building;
            AddFeature(cell.transform.localPosition, building);
        }
        else
        {
            GameObject[] availableFeatures = cell.Features;

            if (cell.IsWater || availableFeatures.Length == 0)
                return;

            if (!cell.HasRoad)
                AddFeature(cell.transform.localPosition, availableFeatures);
            for (HexDirection dir = HexDirection.NE; dir <= HexDirection.NW; dir++)
            {
                if (!cell.HasRoadThroughEdge(dir))
                {
                    AddFeature(cell.transform.localPosition + GameDataPresenter.Instance.HexMetrics.GetFeaturesOffset(dir), availableFeatures);
                }
            }
        }
    }

    void AddFeature(Vector3 position, GameObject[] availableFeatures)
    {
        AddFeature(position, availableFeatures[Mathf.RoundToInt(GameDataPresenter.Instance.HexMetrics.GetLoopedNoise(position).x * (availableFeatures.Length - 1))]);
    }

    void AddFeature(Vector3 position, GameObject feature)
    {
        GameObject instance = Instantiate(feature);
        instance.transform.parent = transform;
        instance.transform.localPosition = PertrubPosition(position);
        instance.transform.localRotation = PertrubRotation(position);
        instance.transform.localScale = PertrubScale(position);
        features.Add(instance.gameObject);
    }

    protected Vector3 PertrubPosition(Vector3 position)
    {
        Vector4 noise = GameDataPresenter.Instance.HexMetrics.GetLoopedNoise(position);
        position.x += (noise.x * 2f - 1) * GameDataPresenter.Instance.HexMetrics.NoiseStrenght;
        position.z += (noise.z * 2f - 1) * GameDataPresenter.Instance.HexMetrics.NoiseStrenght;
        return position;
    }

    protected Quaternion PertrubRotation(Vector3 position)
    {
        Vector4 noise = GameDataPresenter.Instance.HexMetrics.GetLoopedNoise(position, angleNoise);
        float angle = noise.y * (treesMaxAngle - treesMinAngle) + treesMinAngle;
        return Quaternion.Euler(new Vector3(0f, angle, 0f));
    }

    protected Vector3 PertrubScale(Vector3 position)
    {
        Vector4 noise = GameDataPresenter.Instance.HexMetrics.GetLoopedNoise(position, scaleNoise);
        float scale = noise.y * (treesMaxScale - treesMinScale) + treesMinScale;
        return new Vector3(scale, scale, scale);
    }
}
