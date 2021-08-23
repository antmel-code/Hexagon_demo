using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayMesh : HexMesh
{
    public override void Init()
    {
        useVertexColors = true;
        useTextureCoordinates1 = true;
    }

    public void Triangulate(HexCell cell)
    {
        if (cell.Owner != PlayerIdentifier.None)
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
        Vector3 v1 = center +GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction) *GameDataPresenter.Instance.HexMetrics.WaterShoreFactor;
        Vector3 v2 = center +GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction) *GameDataPresenter.Instance.HexMetrics.WaterShoreFactor;
        Vector3 v3 = center +GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v4 = center +GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);

        HexCell neighbor = cell.GetNeighbor(direction) ? cell.GetNeighbor(direction) : cell;
        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
        HexCell previousNeighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;

        Color cellColor = GameDataPresenter.Instance.GetPlayerColor(cell.Owner);

        // Mark edge
        float edge = neighbor.Owner != cell.Owner ? 1f : 0f;
        float edgeLeft = Mathf.Max(previousNeighbor.Owner != cell.Owner ? 1f : 0f, edge);
        float edgeRight = Mathf.Max(nextNeighbor.Owner != cell.Owner ? 1f : 0f, edge);

        Vector3 nearEdgeCenter = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 farEdgeCenter = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisyTriangleFan(center, v1, v2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddTriangleFanColor(cellColor,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddTriangleFanUVs(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);

        AddNoisySubdividedQuad(v1, nearEdgeCenter, v3, farEdgeCenter,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(cellColor,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(edgeLeft, 0f), new Vector2(edge, 0f),GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(nearEdgeCenter, v2, farEdgeCenter, v4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(cellColor,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(edge, 0f), new Vector2(edgeRight, 0f),GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        if (neighbor.Owner == cell.Owner)
        {
            if (direction <= HexDirection.SE)
                TriangulateBridge(cell, direction);
            if (nextNeighbor.Owner == cell.Owner)
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

        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            bool rightSlope = nextNeighbor.Elevation == neighbor.Elevation || nextNeighbor.Elevation == cell.Elevation;
            bool leftSlope = previousNeighbor.Elevation == neighbor.Elevation || previousNeighbor.Elevation == cell.Elevation;

            if (rightSlope && !leftSlope)
            {
                //TriangulateBridgeTerracesCliff(neighbor, direction.Opposite());
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
        Vector3 v1 = center +GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center +GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 +GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 +GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v3.y = v4.y =GameDataPresenter.Instance.HexMetrics.ElevationStep * neighbor.Elevation;

        Color c1 = GameDataPresenter.Instance.GetPlayerColor(cell.Owner);
        Color c2 = GameDataPresenter.Instance.GetPlayerColor(neighbor.Owner);

        // Mark edge
        float edgeLeft = previousNeighbor.Owner != cell.Owner ? 1f : 0;
        float edgeRight = nextNeighbor.Owner != cell.Owner ? 1f : 0;

        TriangulateBridgeFlat(v1, v2, v3, v4, c1, c2, edgeLeft, edgeRight);
    }

    void TriangulateBridgeFlat(HexCell cell, HexDirection direction)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
        HexCell previousNeighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;

        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center +GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center +GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 +GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 +GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v3.y = v4.y =GameDataPresenter.Instance.HexMetrics.ElevationStep * neighbor.Elevation;

        Color c1 = GameDataPresenter.Instance.GetPlayerColor(cell.Owner);
        Color c2 = GameDataPresenter.Instance.GetPlayerColor(neighbor.Owner);

        // Mark edge
        float edgeLeft = previousNeighbor.Owner != cell.Owner ? 1f : 0;
        float edgeRight = nextNeighbor.Owner != cell.Owner ? 1f : 0;

        TriangulateBridgeFlat(v1, v2, v3, v4, c1, c2, edgeLeft, edgeRight);
    }

    void TriangulateBridgeTerraces(HexCell cell, HexDirection direction)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
        HexCell previousNeighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;
        
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center +GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center +GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 +GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 +GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v3.y = v4.y =GameDataPresenter.Instance.HexMetrics.ElevationStep * neighbor.Elevation;

        Color c1 = GameDataPresenter.Instance.GetPlayerColor(cell.Owner);
        Color c2 = GameDataPresenter.Instance.GetPlayerColor(neighbor.Owner);

        // Mark edge
        float edgeLeft = previousNeighbor.Owner != cell.Owner ? 1f : 0;
        float edgeRight = nextNeighbor.Owner != cell.Owner ? 1f : 0;

        TriangulateBridgeTerraces(v1, v2, v3, v4, c1, c2, edgeLeft, edgeRight);
    }

    void TriangulateBridgeCliffTerracesCliff(HexCell cell, HexDirection direction)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
        HexCell previousNeighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;

        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center +GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center +GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 +GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 +GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v3.y = v4.y =GameDataPresenter.Instance.HexMetrics.ElevationStep * neighbor.Elevation;

        Color c1 = GameDataPresenter.Instance.GetPlayerColor(cell.Owner);
        Color c2 = GameDataPresenter.Instance.GetPlayerColor(neighbor.Owner);

        // Mark edge
        float edgeLeft = previousNeighbor.Owner != cell.Owner ? 1f : 0;
        float edgeRight = nextNeighbor.Owner != cell.Owner ? 1f : 0;

        TriangulateBridgeCliffTerracesCliff(v1, v2, v3, v4, c1, c2, edgeLeft, edgeRight);
    }

    void TriangulateBridgeTerracesCliff(HexCell cell, HexDirection direction, bool invert = false)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
        HexCell previousNeighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;

        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center +GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center +GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 +GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 +GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v3.y = v4.y =GameDataPresenter.Instance.HexMetrics.ElevationStep * neighbor.Elevation;

        Color c1 = GameDataPresenter.Instance.GetPlayerColor(cell.Owner);
        Color c2 = GameDataPresenter.Instance.GetPlayerColor(neighbor.Owner);

        // Mark edge
        float edgeLeft = previousNeighbor.Owner != cell.Owner ? 1f : 0;
        float edgeRight = nextNeighbor.Owner != cell.Owner ? 1f : 0;

        if (!invert)
        {
            TriangulateBridgeTerracesCliff(v1, v2, v3, v4, c1, c2, edgeLeft, edgeRight);
        }
        else
        {
            TriangulateBridgeTerracesCliff(v4, v3, v2, v1, c1, c2, edgeRight, edgeLeft);
        }
    }

    void TriangulateBridgeFlat(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        AddNoisySubdividedQuad(v1, v2, v3, v4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
    }

    void TriangulateBridgeFlat(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2, float edgeLeft, float edgeRight)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(new Vector2(edgeLeft, 0f), new Vector2(0f, 0f), new Vector2(edgeLeft, 0f), new Vector2(0f, 0f),GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddNoisySubdividedQuad(v5, v2, v6, v4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(new Vector2(0f, 0f), new Vector2(edgeRight, 0f), new Vector2(0f, 0f), new Vector2(edgeRight, 0f),GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        AddNoisySubdividedQuad(v1, v2, v3, v4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
    }

    void TriangulateBridgeFlatRightShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 1f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeCliffRightShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 1f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeFlatRiver(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {

        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(1f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 1f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeCliffRiver(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(1f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 1f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeFlatLeftShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(1f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeCliffLeftShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(1f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadColor(c1, c2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeCliffTerracesCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);
            Vector3 tv5 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i - 1);
            Vector3 p2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p3 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i);
            Vector3 p4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedLeft(tv1, tv5, tv3, tv6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p3);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p2, p4);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeCliffTerracesCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2, float edgeLeft, float edgeRight)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);
            Vector3 tv5 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i - 1);
            Vector3 p2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p3 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i);
            Vector3 p4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedLeft(tv1, tv5, tv3, tv6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p3);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(new Vector2(edgeLeft, 0f), new Vector2(0f, 0f), new Vector2(edgeLeft, 0f), new Vector2(0f, 0f),GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p2, p4);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(new Vector2(0f, 0f), new Vector2(edgeRight, 0f), new Vector2(0f, 0f), new Vector2(edgeRight, 0f),GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            
        }
    }

    void TriangulateBridgeCliffTerracesCliffRiver(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);
            Vector3 tv5 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i - 1);
            Vector3 p2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p3 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i);
            Vector3 p4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedLeft(tv1, tv5, tv3, tv6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p3);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p2, p4);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 1f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeCliffTerracesCliffRightShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);
            Vector3 tv5 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i - 1);
            Vector3 p2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p3 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i);
            Vector3 p4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedLeft(tv1, tv5, tv3, tv6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p3);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p2, p4);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 1f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeCliffTerracesCliffLeftShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);
            Vector3 tv5 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i - 1);
            Vector3 p2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p3 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i);
            Vector3 p4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedLeft(tv1, tv5, tv3, tv6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p3);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p2, p4);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedRight(tv1, tv2, tv3, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel, p1, p2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        }
    }

    void TriangulateBridgeTerracesCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2, float edgeLeft, float edgeRight)
    {
        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedRight(tv1, tv2, tv3, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel, p1, p2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadUVs(new Vector2(edgeLeft, 0f), new Vector2(0f, 0f), new Vector2(edgeLeft, 0f), new Vector2(0f, 0f),GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(new Vector2(0f, 0f), new Vector2(edgeRight, 0f), new Vector2(0f, 0f), new Vector2(edgeRight, 0f),GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesCliffRightShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);

            Vector3 tv5 = Vector3.Lerp(GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1),GameDataPresenter.Instance.HexMetrics.CliffLerp(v5, v6, i - 1), 0.5f);
            Vector3 tv6 = Vector3.Lerp(GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i),GameDataPresenter.Instance.HexMetrics.CliffLerp(v5, v6, i), 0.5f);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0, 0, 0, 0,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0, 1, 0, 0,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesCliffLeftShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);

            Vector3 tv5 = Vector3.Lerp(GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1),GameDataPresenter.Instance.HexMetrics.CliffLerp(v5, v6, i - 1), 0.5f);
            Vector3 tv6 = Vector3.Lerp(GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i),GameDataPresenter.Instance.HexMetrics.CliffLerp(v5, v6, i), 0.5f);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesCliffRiver(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);
        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.CliffLerp(v2, v4, i);

            Vector3 tv5 = Vector3.Lerp(GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1),GameDataPresenter.Instance.HexMetrics.CliffLerp(v5, v6, i - 1), 0.5f);
            Vector3 tv6 = Vector3.Lerp(GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i),GameDataPresenter.Instance.HexMetrics.CliffLerp(v5, v6, i), 0.5f);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            Vector3 p1 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p2 =GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 1f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerraces(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            AddNoisySubdividedQuad(tv1, tv2, tv3, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        }
    }

    void TriangulateBridgeTerraces(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2, float edgeLeft, float edgeRight)
    {
        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            float topUV = i % 2 == 0 ? 0f : 1f;
            float bottomUV = i % 2 == 0 ? 1f : 0f;

            AddNoisySubdividedQuad(tv1, tv2, tv3, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadUVs(new Vector2(edgeLeft, 0f), new Vector2(0f, 0f), new Vector2(edgeLeft, 0f), new Vector2(0f, 0f),GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(new Vector2(0f, 0f), new Vector2(edgeRight, 0f), new Vector2(0f, 0f), new Vector2(edgeRight, 0f),GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesRiver(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i);

            Vector3 tv5 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1, 0, 0, 0,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuad(tv5, tv2, tv6, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 1f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesRightShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i);

            Vector3 tv5 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuad(tv5, tv2, tv6, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 1f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesLeftShore(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        for (int i = 1; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i);

            Vector3 tv5 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i - 1);
            Vector3 tv6 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(v5, v6, i);

            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(c1, c2, i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuad(tv5, tv2, tv6, tv4,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadColor(tc1, tc2,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0f, 0f, 0f, 0f,GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        Color bottomColor = GameDataPresenter.Instance.GetPlayerColor(bottomCell.Owner);
        Color leftColor = GameDataPresenter.Instance.GetPlayerColor(leftCell.Owner);
        Color rightColor = GameDataPresenter.Instance.GetPlayerColor(rightCell.Owner);

        HexEdgeType leftEdgeType =GameDataPresenter.Instance.HexMetrics.GetEdgeType(bottomCell.Elevation, leftCell.Elevation);
        HexEdgeType rightEdgeType =GameDataPresenter.Instance.HexMetrics.GetEdgeType(bottomCell.Elevation, rightCell.Elevation);

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
            

        }
    }
    void TriangulateCornerTerraces(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        Color bottomColor = GameDataPresenter.Instance.GetPlayerColor(bottomCell.Owner);
        Color leftColor = GameDataPresenter.Instance.GetPlayerColor(leftCell.Owner);
        Color rightColor = GameDataPresenter.Instance.GetPlayerColor(rightCell.Owner);

        Vector3 v4 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, left, 1);
        Vector3 v5 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, right, 1);
        Color c4 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottomColor, leftColor, 1);
        Color c5 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottomColor, rightColor, 1);

        AddNoisyTriangle(bottom, v4, v5);
        AddTriangleColor(bottomColor, c4, c5);
        AddTriangleUVs(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));

        for (int i = 2; i <=GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, left, i - 1);
            Vector3 tv2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, right, i - 1);
            Vector3 tv3 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, left, i);
            Vector3 tv4 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, right, i);
            Color tc1 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottomColor, leftColor, i - 1);
            Color tc2 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottomColor, rightColor, i - 1);
            Color tc3 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottomColor, leftColor, i);
            Color tc4 =GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottomColor, rightColor, i);

            AddNoisyQuad(tv1, tv2, tv3, tv4);
            AddQuadColor(tc1, tc2, tc3, tc4);
            AddQuadUVs(0f, 0f, 0f, 0f);
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
        Vector3 v2 = center +GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v4 = v2 +GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v4.y = neighbor.Elevation *GameDataPresenter.Instance.HexMetrics.ElevationStep;
        Vector3 v5 = v2 +GameDataPresenter.Instance.HexMetrics.GetBridge(direction.Clockwise());
        v5.y = nextNeighbor.Elevation *GameDataPresenter.Instance.HexMetrics.ElevationStep;

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
