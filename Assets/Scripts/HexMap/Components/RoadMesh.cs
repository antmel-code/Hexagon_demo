using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadMesh : HexMesh
{
    public override void Init()
    {
        useVertexColors = false;
        useTextureCoordinates1 = true;
    }

    public void Triangulate(HexCell cell)
    {
        Vector3 center = cell.transform.localPosition;

        for (HexDirection direction = HexDirection.NE; direction <= HexDirection.SE; direction++)
        {
            if (cell.HasRoadThroughEdge(direction))
            {
                TriangulateBridge(cell, direction);
            }
        }

        if (cell.HasRoad)
        {
            for (HexDirection direction = HexDirection.NE; direction <= HexDirection.NW; direction++)
            {
                Vector3 v1 = center + GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
                Vector3 v2 = center + GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
                if (!cell.HasRoadThroughEdge(direction))
                {
                    if (!cell.HasRoadThroughEdge(direction.CounterClockwise()))
                    {
                        v1 = Vector3.Lerp(v1, center, 0.5f);
                    }
                    if (!cell.HasRoadThroughEdge(direction.Clockwise()))
                    {
                        v2 = Vector3.Lerp(v2, center, 0.5f);
                    }
                    AddNoisyTriangleFan(center, v1, v2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
                    //AddTriangleFanColor(cell.Color, HexMetrics.EdgeSubdivisionLevel);

                    AddTriangleFanUVs(new Vector2(1f, 0), Vector2.zero, Vector2.zero, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
                }
                else
                {
                    Vector3 roadCenter = Vector3.Lerp(v1, v2, 0.5f);
                    AddNoisyTriangleFan(center, v1, roadCenter, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
                    //AddTriangleFanColor(cell.Color, HexMetrics.EdgeSubdivisionLevel / 2);
                    AddNoisyTriangleFan(center, roadCenter, v2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
                    //AddTriangleFanColor(cell.Color, HexMetrics.EdgeSubdivisionLevel / 2);

                    AddTriangleFanUVs(new Vector2(1f, 0f), Vector2.zero, new Vector2(1f, 0f), GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
                    AddTriangleFanUVs(new Vector2(1f, 0f), new Vector2(1f, 0f), Vector2.zero, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

                }
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

        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v3.y = v4.y = GameDataPresenter.Instance.HexMetrics.ElevationStep * neighbor.Elevation;


        //Color c1 = cell.Color;
        //Color c2 = neighbor.Color;

        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            HexCell nearestNaighbor = cell.GetNeighbor(direction) ? cell.GetNeighbor(direction) : cell;
            HexCell clockwiseNaighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
            HexCell counterClockwiseNaighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;
            bool rightSlope = clockwiseNaighbor.Elevation == nearestNaighbor.Elevation || clockwiseNaighbor.Elevation == cell.Elevation;
            bool leftSlope = counterClockwiseNaighbor.Elevation == nearestNaighbor.Elevation || counterClockwiseNaighbor.Elevation == cell.Elevation;
            if (rightSlope && !leftSlope)
            {
                TriangulateBridgeTerracesCliff(v4, v3, v2, v1);
            }
            else if (!rightSlope && leftSlope)
            {
                TriangulateBridgeTerracesCliff(v1, v2, v3, v4);
            }
            else if (!rightSlope && !leftSlope)
            {
                TriangulateBridgeCliffTerracesCliff(v1, v2, v3, v4);
            }
            else
            {
                TriangulateBridgeTerraces(v1, v2, v3, v4);
            }
        }
        else
        {
            TriangulateBridgeFlat(v1, v2, v3, v4);
        }

    }

    void TriangulateBridgeFlat(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        Vector3 v5 = Vector3.Lerp(v1, v2, 0.5f);
        Vector3 v6 = Vector3.Lerp(v3, v4, 0.5f);

        AddNoisySubdividedQuad(v1, v5, v3, v6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(0, 1, 0, 0, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

        AddNoisySubdividedQuad(v5, v2, v6, v4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        AddSubdividedQuadUVs(1, 0, 0, 0, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
    }

    void TriangulateBridgeCliffTerracesCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
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

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p3 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i);
            Vector3 p4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedLeft(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p3);
            AddSubdividedQuadUVs(0, 1, 0, 0, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p2, p4);
            AddSubdividedQuadUVs(1, 0, 0, 0, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
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

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0, 1, 0, 0, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p2);
            AddSubdividedQuadUVs(1, 0, 0, 0, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerraces(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
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

            AddNoisySubdividedQuad(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(0, 1, 0, 0, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuad(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs(1, 0, 0, 0, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

}
