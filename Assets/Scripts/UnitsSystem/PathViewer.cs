using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathViewer : MonoBehaviour
{
    [SerializeField]
    PathPointViewer pathPintViewerPrefab;

    List<RectTransform> builtPath = new List<RectTransform>();

    public void BuildPath(HexCoordinates[] path, int[] cost, int warningDistance)
    {
        ClearPath();
        for (int i = 0; i < path.Length; i++)
        {
            HexCoordinates point = path[i];
            int pointCost = cost[i];
            RectTransform newPoint = Instantiate(pathPintViewerPrefab).GetComponent<RectTransform>();
            newPoint.GetComponent<PathPointViewer>().Cost = pointCost;
            if (pointCost >= warningDistance)
            {
                newPoint.GetComponent<PathPointViewer>().Type = PathPointViewer.PathPointType.Warning;
            }
            Vector3 cellPosition = HexMapMaster.Instance.GetCellOnMap(point).transform.position;
            newPoint.SetParent(transform, false);
            newPoint.anchoredPosition = new Vector3(cellPosition.x, cellPosition.z);
            Vector3 newPosition = newPoint.localPosition;
            newPosition.z = HexMapMaster.Instance.GetCellOnMap(point).Elevation * -GameDataPresenter.Instance.HexMetrics.ElevationStep;
            newPoint.localPosition = newPosition;
            builtPath.Add(newPoint);
        }
    }

    public void ClearPath()
    {
        foreach(RectTransform point in builtPath)
        {
            Destroy(point.gameObject);
        }
        builtPath.Clear();
    }
}
