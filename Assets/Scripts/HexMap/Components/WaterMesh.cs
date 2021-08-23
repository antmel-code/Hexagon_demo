using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterMesh : HexMesh
{
    public override void Init()
    {
        useCollider = true;
        useVertexColors = true;
        useTextureCoordinates1 = true;
        useTextureCoordinates2 = true;
    }

    public void Triangulate(HexCell cell)
    {
        if (cell.IsWater)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                Triangulate(d, cell);
            }
        }
    }

    void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction) * GameDataPresenter.Instance.HexMetrics.WaterShoreFactor;
        Vector3 v2 = center + GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction) * GameDataPresenter.Instance.HexMetrics.WaterShoreFactor;
        Vector3 v3 = center + GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v4 = center + GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);

        HexCell neighbor = cell.GetNeighbor(direction) ? cell.GetNeighbor(direction) : cell;
        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
        HexCell previousNeighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;

        Color cellColor = GameDataPresenter.Instance.GetWatherColorByName(cell.NameOfHexType);

        // Mark waterfall
        float waterfall = neighbor.IsWater && cell.Elevation > neighbor.Elevation ? 1f : 0f;
        float waterfallLeft = Mathf.Max(previousNeighbor.IsWater && cell.Elevation > previousNeighbor.Elevation ? 1f : 0f, waterfall);
        float waterfallRight = Mathf.Max(nextNeighbor.IsWater && cell.Elevation > nextNeighbor.Elevation ? 1f : 0f, waterfall);

        // Mark merge
        float merge = neighbor.IsWater && cell.Elevation < neighbor.Elevation ? 1f : 0f;
        float mergeLeft = Mathf.Max(previousNeighbor.IsWater && cell.Elevation < previousNeighbor.Elevation ? 1f : 0f, merge);
        float mergeRight = Mathf.Max(nextNeighbor.IsWater && cell.Elevation < nextNeighbor.Elevation ? 1f : 0f, merge);

        // Mark shore
        float shore = !neighbor.IsWater ? 1f : 0f;
        float shoreLeft = Mathf.Max(!previousNeighbor.IsWater ? 1f : 0f, shore);
        float shoreRight = Mathf.Max(!nextNeighbor.IsWater ? 1f : 0f, shore);

        // Mark UV for flows
        float UFlowLeft = direction <= HexDirection.SE ? 1f : 0f;
        float UFlowRight = direction <= HexDirection.SE ? 0f : 1f;
        float UFlowMiddle = (UFlowLeft + UFlowRight) / 2;

        Vector3 nearEdgeCenter = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 farEdgeCenter = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisyTriangleFan(center, v1, v2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddTriangleFanColor(cellColor, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddTriangleFanUVs(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddTriangleFanUVs2(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);

        AddNoisySubdividedQuad(v1, nearEdgeCenter, v3, farEdgeCenter, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(cellColor, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(shoreLeft, mergeLeft), new Vector2(shore, merge), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(new Vector2(UFlowLeft, 0f), new Vector2(UFlowMiddle, 0f), new Vector2(UFlowLeft, waterfallLeft), new Vector2(UFlowMiddle, waterfall), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(nearEdgeCenter, v2, farEdgeCenter, v4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(cellColor, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(shore, merge), new Vector2(shoreRight, mergeRight), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(new Vector2(UFlowMiddle, 0f), new Vector2(UFlowRight, 0f), new Vector2(UFlowMiddle, waterfall), new Vector2(UFlowRight, waterfallRight), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        if (neighbor.IsWater)
        {
            if (direction <= HexDirection.SE)
                TriangulateBridge(cell, direction);
            if (nextNeighbor.IsWater)
            {
                if (direction <= HexDirection.E)
                    TriangulateCorner(cell, direction);
            }
        }



    }

    void TriangulateBridge(HexCell cell, HexDirection direction)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null)
        {
            return;
        }

        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
        HexCell previousNeighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;

        // Mark shore
        float shoreLeft = !previousNeighbor.IsWater ? 1f : 0;
        float shoreRight = !nextNeighbor.IsWater ? 1f : 0;

        // Mark merge
        float mergeLeft = previousNeighbor.IsWater && previousNeighbor.Elevation > cell.Elevation ? 1f : 0f;
        float mergeRight = nextNeighbor.IsWater && nextNeighbor.Elevation > cell.Elevation ? 1f : 0f;

        // Mark flow UVs
        float UFlowLeft = direction <= HexDirection.SE ? 1f : 0f;
        float UFlowRight = direction <= HexDirection.SE ? 0f : 1f;

        // Mark waterfall
        float waterfall = cell.Elevation != neighbor.Elevation ? 1f : 0f;
        float waterfallLeft = Mathf.Max(previousNeighbor.Elevation < cell.Elevation ? 1f : 0f, waterfall);
        float waterfallRight = Mathf.Max(nextNeighbor.Elevation < cell.Elevation ? 1f : 0f, waterfall);

        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            bool rightSlope = nextNeighbor.Elevation == neighbor.Elevation || nextNeighbor.Elevation == cell.Elevation;
            bool leftSlope = previousNeighbor.Elevation == neighbor.Elevation || previousNeighbor.Elevation == cell.Elevation;

            if (rightSlope && !leftSlope)
            {
                TriangulateBridgeTerracesCliff(cell, direction, true);
            }
            else if (!rightSlope && leftSlope)
            {
                TriangulateBridgeTerracesCliff(cell, direction);
            }
            else if (!rightSlope && !leftSlope)
            {
                TriangulateBridgeCliffTerracesCliff(cell, direction);
            }
            else
            {
                TriangulateBridgeTerraces(cell, direction);
            }
        }
        else if (cell.GetEdgeType(direction) == HexEdgeType.Cliff)
        {
            TriangulateBridgeCliff(cell, direction);
        }
        else
        {
            TriangulateBridgeFlat(cell, direction);
        }

    }

    void TriangulateBridgeCliff(HexCell cell, HexDirection direction)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
        HexCell previousNeighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;

        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v3.y = v4.y = GameDataPresenter.Instance.HexMetrics.ElevationStep * neighbor.Elevation;

        Color c1 = GameDataPresenter.Instance.GetWatherColorByName(cell.NameOfHexType);
        Color c2 = GameDataPresenter.Instance.GetWatherColorByName(neighbor.NameOfHexType);

        // Mark shore
        float shoreLeft = !previousNeighbor.IsWater ? 1f : 0;
        float shoreRight = !nextNeighbor.IsWater ? 1f : 0;

        // Mark merge
        float mergeLeft = neighbor.Elevation == cell.Elevation && previousNeighbor.IsWater && previousNeighbor.Elevation > cell.Elevation ? 1f : 0f;
        float mergeRight = neighbor.Elevation == cell.Elevation && nextNeighbor.IsWater && nextNeighbor.Elevation > cell.Elevation ? 1f : 0f;

        TriangulateBridgeFlat(v1, v2, v3, v4, c1, c2, shoreLeft, shoreRight, mergeLeft, mergeRight, 1f, 1f, true, direction <= HexDirection.SE);
    }

    void TriangulateBridgeFlat(HexCell cell, HexDirection direction)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
        HexCell previousNeighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;

        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v3.y = v4.y = GameDataPresenter.Instance.HexMetrics.ElevationStep * neighbor.Elevation;

        Color c1 = GameDataPresenter.Instance.GetWatherColorByName(cell.NameOfHexType);
        Color c2 = GameDataPresenter.Instance.GetWatherColorByName(neighbor.NameOfHexType);

        // Mark shore
        float shoreLeft = !previousNeighbor.IsWater ? 1f : 0;
        float shoreRight = !nextNeighbor.IsWater ? 1f : 0;

        // Mark merge
        float mergeLeft = neighbor.Elevation == cell.Elevation && previousNeighbor.IsWater && previousNeighbor.Elevation > cell.Elevation ? 1f : 0f;
        float mergeRight = neighbor.Elevation == cell.Elevation && nextNeighbor.IsWater && nextNeighbor.Elevation > cell.Elevation ? 1f : 0f;

        // Mark waterfall
        float waterfall = cell.Elevation != neighbor.Elevation ? 1f : 0f;
        float waterfallLeft = Mathf.Max(previousNeighbor.IsWater && previousNeighbor.Elevation < cell.Elevation ? 1f : 0f, waterfall);
        float waterfallRight = Mathf.Max(nextNeighbor.IsWater && nextNeighbor.Elevation < cell.Elevation ? 1f : 0f, waterfall);

        TriangulateBridgeFlat(v1, v2, v3, v4, c1, c2, shoreLeft, shoreRight, mergeLeft, mergeRight, waterfallLeft, waterfallRight, false, direction <= HexDirection.SE);
    }

    void TriangulateBridgeTerraces(HexCell cell, HexDirection direction)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
        HexCell previousNeighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;
        
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v3.y = v4.y = GameDataPresenter.Instance.HexMetrics.ElevationStep * neighbor.Elevation;

        Color c1 = GameDataPresenter.Instance.GetWatherColorByName(cell.NameOfHexType);
        Color c2 = GameDataPresenter.Instance.GetWatherColorByName(neighbor.NameOfHexType);

        // Mark shore
        float shoreLeft = !previousNeighbor.IsWater ? 1f : 0;
        float shoreRight = !nextNeighbor.IsWater ? 1f : 0;

        TriangulateBridgeTerraces(v1, v2, v3, v4, c1, c2, shoreLeft, shoreRight, direction <= HexDirection.SE);
    }

    void TriangulateBridgeCliffTerracesCliff(HexCell cell, HexDirection direction)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
        HexCell previousNeighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;

        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v3.y = v4.y = GameDataPresenter.Instance.HexMetrics.ElevationStep * neighbor.Elevation;

        Color c1 = GameDataPresenter.Instance.GetWatherColorByName(cell.NameOfHexType);
        Color c2 = GameDataPresenter.Instance.GetWatherColorByName(neighbor.NameOfHexType);

        // Mark shore
        float shoreLeft = !previousNeighbor.IsWater ? 1f : 0;
        float shoreRight = !nextNeighbor.IsWater ? 1f : 0;

        // Mark merge
        float mergeLeft = neighbor.Elevation == cell.Elevation && previousNeighbor.IsWater && previousNeighbor.Elevation > cell.Elevation ? 1f : 0f;
        float mergeRight = neighbor.Elevation == cell.Elevation && nextNeighbor.IsWater && nextNeighbor.Elevation > cell.Elevation ? 1f : 0f;

        TriangulateBridgeCliffTerracesCliff(v1, v2, v3, v4, c1, c2, shoreLeft, shoreRight, mergeLeft, mergeRight, direction <= HexDirection.SE);
    }

    void TriangulateBridgeTerracesCliff(HexCell cell, HexDirection direction, bool invert = false)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
        HexCell previousNeighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;

        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v3.y = v4.y = GameDataPresenter.Instance.HexMetrics.ElevationStep * neighbor.Elevation;

        Color c1 = GameDataPresenter.Instance.GetWatherColorByName(cell.NameOfHexType);
        Color c2 = GameDataPresenter.Instance.GetWatherColorByName(neighbor.NameOfHexType);

        // Mark shore
        float shoreLeft = !previousNeighbor.IsWater ? 1f : 0;
        float shoreRight = !nextNeighbor.IsWater ? 1f : 0;
        
        // Mark merge
        float mergeRight = neighbor.Elevation == cell.Elevation && nextNeighbor.IsWater && nextNeighbor.Elevation > cell.Elevation ? 1f : 0f;

        if (!invert)
            TriangulateBridgeTerracesCliff(v1, v2, v3, v4, c1, c2, shoreLeft, shoreRight, mergeRight, direction <= HexDirection.SE);
        else
            TriangulateBridgeTerracesCliff(v4, v3, v2, v1, c2, c1, shoreRight, shoreLeft, mergeRight, direction <= HexDirection.SE);
    }

    void TriangulateBridgeFlat(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        AddNoisySubdividedQuad(v1, v2, v3, v4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddSubdividedQuadUVs2(0f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
    }

    void TriangulateBridgeFlat(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2, float shoreLeft, float shoreRight, float mergeLeft, float mergeRight, float waterfallLeft, float waterfallRight, bool isWaterfallItself, bool inverseUFlow)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        float UFlowLeft = !inverseUFlow ? 0f : 1f;
        float UFlowRight = !inverseUFlow ? 1f : 0f;
        float UFlowMiddle = (UFlowLeft + UFlowRight) / 2;

        AddNoisySubdividedQuad(v1, v5, v3, v6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(new Vector2(shoreLeft, mergeLeft), new Vector2(0f, 0f), new Vector2(shoreLeft, mergeLeft), new Vector2(0f, 0f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        if (isWaterfallItself)
            AddSubdividedQuadUVs2(new Vector2(UFlowLeft, 1f), new Vector2(UFlowMiddle, 1f), new Vector2(UFlowLeft, 1f), new Vector2(UFlowMiddle, 1f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        else
            AddSubdividedQuadUVs2(new Vector2(UFlowLeft, waterfallLeft), new Vector2(UFlowLeft, 0f), new Vector2(UFlowRight, waterfallLeft), new Vector2(UFlowRight, 0f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(new Vector2(0f, 0f), new Vector2(shoreRight, mergeRight), new Vector2(0f, 0f), new Vector2(shoreRight, mergeRight), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        if (isWaterfallItself)
            AddSubdividedQuadUVs2(new Vector2(UFlowMiddle, 1f), new Vector2(UFlowRight, 1f), new Vector2(UFlowMiddle, 1f), new Vector2(UFlowRight, 1f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        else
            AddSubdividedQuadUVs2(new Vector2(UFlowLeft, 0f), new Vector2(UFlowLeft, waterfallRight), new Vector2(UFlowRight, 0f), new Vector2(UFlowRight, waterfallRight), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        AddNoisySubdividedQuad(v1, v2, v3, v4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddSubdividedQuadUVs2(0f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
    }

    void TriangulateBridgeFlatRightShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(0f, 0.5f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(0.5f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeCliffRightShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(0f, 0.5f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeFlatRiver(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {

        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(1f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(0f, 0.5f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(0.5f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeCliffRiver(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(1f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(0f, 0.5f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeFlatLeftShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(1f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(0f, 0.5f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(0.5f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeCliffLeftShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(1f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(0f, 0.5f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeCliffTerracesCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);
            Vector3 tv5 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p3 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i);
            Vector3 p4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedLeft(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p3);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0f, 0f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p2, p4);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeCliffTerracesCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2, float shoreLeft, float shoreRight, float mergeLeft, float mergeRight, bool inverseUFlow)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        float UFlowRight = inverseUFlow ? 1f : 0f;
        float UFlowLeft = inverseUFlow ? 0f : 1f;
        float UFlowMiddle = UFlowLeft + UFlowRight / 2;

        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);
            Vector3 tv5 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p3 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i);
            Vector3 p4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedLeft(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p3);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(new Vector2(shoreLeft, mergeLeft), new Vector2(0f, 0f), new Vector2(shoreLeft, mergeLeft), new Vector2(0f, 0f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(new Vector2(UFlowLeft, 1f), new Vector2(UFlowMiddle, 1f), new Vector2(UFlowLeft, 1f), new Vector2(UFlowMiddle, 1f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p2, p4);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(new Vector2(0f, 0f), new Vector2(shoreRight, mergeRight), new Vector2(0f, 0f), new Vector2(shoreRight, mergeRight), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(new Vector2(UFlowMiddle, 1f), new Vector2(UFlowRight, 1f), new Vector2(UFlowMiddle, 1f), new Vector2(UFlowRight, 1f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeCliffTerracesCliffRiver(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);
            Vector3 tv5 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p3 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i);
            Vector3 p4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedLeft(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p3);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0f, 0.5f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p2, p4);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeCliffTerracesCliffRightShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);
            Vector3 tv5 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p3 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i);
            Vector3 p4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedLeft(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p3);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0f, 0.5f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p2, p4);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeCliffTerracesCliffLeftShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);
            Vector3 tv5 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p3 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i);
            Vector3 p4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedLeft(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p3);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0f, 0.5f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p2, p4);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedRight(tv1, tv2, tv3, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel, p1, p2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadUVs2(0f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        }
    }

    void TriangulateBridgeTerracesCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2, float shoreLeft, float shoreRight, float mergeRight, bool inverseUFlow)
    {
        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedRight(tv1, tv2, tv3, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel, p1, p2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadUVs(new Vector2(shoreLeft, 0f), new Vector2(0f, 0f), new Vector2(shoreLeft, 0f), new Vector2(0f, 0f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(new Vector2(0f, 0f), new Vector2(shoreRight, mergeRight), new Vector2(0f, 0f), new Vector2(shoreRight, mergeRight), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            if (!inverseUFlow)
                AddSubdividedQuadUVs2(0f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            else
                AddSubdividedQuadUVs2(1f, 0f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        }
    }

    void TriangulateBridgeTerracesCliffRightShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);

            Vector3 tv5 = Vector3.Lerp(GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1), GameDataPresenter.Instance.HexMetrics.CliffLerp(v5, v6, i - 1), 0.5f);
            Vector3 tv6 = Vector3.Lerp(GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i), GameDataPresenter.Instance.HexMetrics.CliffLerp(v5, v6, i), 0.5f);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0, 0, 0, 0, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0f, 0.5f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0, 1, 0, 0, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesCliffLeftShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);

            Vector3 tv5 = Vector3.Lerp(GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1), GameDataPresenter.Instance.HexMetrics.CliffLerp(v5, v6, i - 1), 0.5f);
            Vector3 tv6 = Vector3.Lerp(GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i), GameDataPresenter.Instance.HexMetrics.CliffLerp(v5, v6, i), 0.5f);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0f, 0.5f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesCliffRiver(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);

            Vector3 tv5 = Vector3.Lerp(GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1), GameDataPresenter.Instance.HexMetrics.CliffLerp(v5, v6, i - 1), 0.5f);
            Vector3 tv6 = Vector3.Lerp(GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i), GameDataPresenter.Instance.HexMetrics.CliffLerp(v5, v6, i), 0.5f);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0f, 0.5f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerraces(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            AddNoisySubdividedQuad(tv1, tv2, tv3, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadUVs2(0f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        }
    }

    void TriangulateBridgeTerraces(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2, float shoreLeft, float shoreRight, bool inverseUFlow)
    {
        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            float topUV = i % 2 == 0 ? 0f : 1f;
            float bottomUV = i % 2 == 0 ? 1f : 0f;

            AddNoisySubdividedQuad(tv1, tv2, tv3, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadUVs(new Vector2(shoreLeft, 0f), new Vector2(0f, 0f), new Vector2(shoreLeft, 0f), new Vector2(0f, 0f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(new Vector2(0f, 0f), new Vector2(shoreRight, 0f), new Vector2(0f, 0f), new Vector2(shoreRight, 0f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            if (!inverseUFlow)
                AddSubdividedQuadUVs2(0f, 1f, bottomUV, topUV, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            else
                AddSubdividedQuadUVs2(1f, 0f, bottomUV, topUV, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        }
    }

    void TriangulateBridgeTerracesRiver(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i);

            Vector3 tv5 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1, 0, 0, 0, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0f, 0.5f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuad(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesRightShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i);

            Vector3 tv5 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0f, 0.5f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuad(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 1f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesLeftShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i);

            Vector3 tv5 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0f, 0.5f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuad(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs2(0.5f, 1f, 1f, 1f, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        Color bottomColor = GameDataPresenter.Instance.GetWatherColorByName(bottomCell.NameOfHexType);
        Color leftColor = GameDataPresenter.Instance.GetWatherColorByName(leftCell.NameOfHexType);
        Color rightColor = GameDataPresenter.Instance.GetWatherColorByName(rightCell.NameOfHexType);

        HexEdgeType leftEdgeType = GameDataPresenter.Instance.HexMetrics.GetEdgeType(bottomCell.Elevation, leftCell.Elevation);
        HexEdgeType rightEdgeType = GameDataPresenter.Instance.HexMetrics.GetEdgeType(bottomCell.Elevation, rightCell.Elevation);

        if (leftEdgeType == HexEdgeType.Slope && rightEdgeType == HexEdgeType.Slope)
        {
            TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
        }
        else if (leftEdgeType == HexEdgeType.Slope && rightEdgeType == HexEdgeType.Flat)
        {
            TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
        }
        else if (leftEdgeType == HexEdgeType.Flat && rightEdgeType == HexEdgeType.Slope)
        {
            TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
        }
        else
        {
            float waterfall = leftCell.Elevation == bottomCell.Elevation && bottomCell.Elevation == rightCell.Elevation ? 0f : 1;
            AddNoisyTriangle(bottom, left, right);
            AddTriangleColor(bottomColor, leftColor, rightColor);
            AddTriangleUVs(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            if (leftCell.Elevation == rightCell.Elevation)
                AddTriangleUVs2(new Vector2(0.5f, waterfall), new Vector2(0f, waterfall), new Vector2(1f, waterfall));
            else if(leftCell.Elevation > rightCell.Elevation)
                AddTriangleUVs2(new Vector2(0f, waterfall), new Vector2(0.5f, waterfall), new Vector2(1f, waterfall));
            else
                AddTriangleUVs2(new Vector2(0f, waterfall), new Vector2(1f, waterfall), new Vector2(0.5f, waterfall));

        }
    }
    void TriangulateCornerTerraces(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        Color bottomColor = GameDataPresenter.Instance.GetWatherColorByName(bottomCell.NameOfHexType);
        Color leftColor = GameDataPresenter.Instance.GetWatherColorByName(leftCell.NameOfHexType);
        Color rightColor = GameDataPresenter.Instance.GetWatherColorByName(rightCell.NameOfHexType);

        Vector3 v4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, left, 1);
        Vector3 v5 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, right, 1);
        Color c4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottomColor, leftColor, 1);
        Color c5 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottomColor, rightColor, 1);

        AddNoisyTriangle(bottom, v4, v5);
        AddTriangleColor(bottomColor, c4, c5);
        AddTriangleUVs(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        AddTriangleUVs2(new Vector2(0.5f, 1f), new Vector2(0f, 1f), new Vector2(1f, 1f));

        for (int i = 2; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, left, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, right, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, left, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, right, i);
            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottomColor, leftColor, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottomColor, rightColor, i - 1);
            Color tc3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottomColor, leftColor, i);
            Color tc4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottomColor, rightColor, i);

            AddNoisyQuad(tv1, tv2, tv3, tv4);
            AddQuadColor(tc1, tc2, tc3, tc4);
            AddQuadUVs(0f, 0f, 0f, 0f);
            AddQuadUVs2(0f, 1f, 1f, 1f);
        }
    }

    void TriangulateCorner(HexCell cell, HexDirection direction)
    {

        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise());
        if (neighbor == null || nextNeighbor == null)
        {
            return;
        }

        Vector3 center = cell.transform.localPosition;
        Vector3 v2 = center + GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v4 = v2 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v4.y = neighbor.Elevation * GameDataPresenter.Instance.HexMetrics.ElevationStep;
        Vector3 v5 = v2 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction.Clockwise());
        v5.y = nextNeighbor.Elevation * GameDataPresenter.Instance.HexMetrics.ElevationStep;

        if (cell.Elevation <= neighbor.Elevation)
        {
            if (cell.Elevation <= nextNeighbor.Elevation)
            {
                TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
            }
            else
            {
                TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
            }
        }
        else if (neighbor.Elevation <= nextNeighbor.Elevation)
        {
            TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
        }
        else
        {
            TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
        }
    }
}
