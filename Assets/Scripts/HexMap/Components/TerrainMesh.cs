using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMesh : HexMesh
{
    Color color1 = new Color(1f, 0f, 0f);
    Color color2 = new Color(0f, 1f, 0f);
    Color color3 = new Color(0f, 0f, 1f);
    public override void Init()
    {
        useVertexColors = true;
        useCollider = true;
        useTextureCoordinates3 = true;
    }

    public void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction); ;

        HexTerrainType cellType = cell.TerrainType;
        Vector3 typeUV = new Vector3((int)cellType, (int)cellType, (int)cellType);

        if (!cell.IsWater)
        {
            AddNoisyTriangleFan(center, v1, v2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddTriangleFanColor(color1, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddTriangleFanUVs3(typeUV, typeUV, typeUV, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);

            if (direction <= HexDirection.SE)
                TriangulateBridge(cell, direction);
            if (direction <= HexDirection.E)
                TriangulateCorner(cell, direction);
        }
        else
        {
            if (cell.GetNeighbor(direction) && !cell.GetNeighbor(direction).IsWater)
            {
                if (direction <= HexDirection.SE)
                    TriangulateBridge(cell, direction);
                if (direction <= HexDirection.E)
                    TriangulateCorner(cell, direction);
            }
            if (cell.GetNeighbor(direction.Clockwise()) && !cell.GetNeighbor(direction.Clockwise()).IsWater)
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

        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + GameDataPresenter.Instance.HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + GameDataPresenter.Instance.HexMetrics.GetSecondSolidCorner(direction);
        Vector3 v3 = v1 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        Vector3 v4 = v2 + GameDataPresenter.Instance.HexMetrics.GetBridge(direction);
        v3.y = v4.y = GameDataPresenter.Instance.HexMetrics.ElevationStep * neighbor.Elevation;

        HexTerrainType type1 = cell.TerrainType;
        HexTerrainType type2 = neighbor.TerrainType;

        Vector3 typesUV = new Vector3((int)type1, (int)type2, (int)type2);

        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            HexCell nearestNaighbor = cell.GetNeighbor(direction) ? cell.GetNeighbor(direction) : cell;
            HexCell clockwiseNaighbor = cell.GetNeighbor(direction.Clockwise()) ? cell.GetNeighbor(direction.Clockwise()) : cell;
            HexCell counterClockwiseNaighbor = cell.GetNeighbor(direction.CounterClockwise()) ? cell.GetNeighbor(direction.CounterClockwise()) : cell;
            bool rightSlope = clockwiseNaighbor.Elevation == nearestNaighbor.Elevation || clockwiseNaighbor.Elevation == cell.Elevation;
            bool leftSlope = counterClockwiseNaighbor.Elevation == nearestNaighbor.Elevation || counterClockwiseNaighbor.Elevation == cell.Elevation;
            if (rightSlope && !leftSlope)
            {
                TriangulateBridgeTerracesCliff(v4, v3, v2, v1, typesUV, true);
            }
            else if (!rightSlope && leftSlope)
            {
                TriangulateBridgeTerracesCliff(v1, v2, v3, v4, typesUV);
            }
            else if (!rightSlope && !leftSlope)
            {
                TriangulateBridgeCliffTerracesCliff(v1, v2, v3, v4, typesUV);
            }
            else
            {
                TriangulateBridgeTerraces(v1, v2, v3, v4, typesUV);
            }
        }
        else
        {
            TriangulateBridgeFlat(v1, v2, v3, v4, typesUV);
        }

    }

    void TriangulateBridgeFlat(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 types)
    {
        AddNoisySubdividedQuad(v1, v2, v3, v4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddSubdividedQuadColor(color1, color2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        AddSubdividedQuadUVs3(types, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
    }

    void TriangulateBridgeCliffTerracesCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 types)
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

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(color1, color2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(color1, color2, i);

            Vector3 p1 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i - 1);
            Vector3 p2 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i - 1);
            Vector3 p3 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v1), Pertrub(v3), i);
            Vector3 p4 = GameDataPresenter.Instance.HexMetrics.CliffLerp(Pertrub(v2), Pertrub(v4), i);

            AddNoisySubdividedQuadPinnedLeft(tv1, tv5, tv3, tv6, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p1, p3);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs3(types, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);

            AddNoisySubdividedQuadPinnedRight(tv5, tv2, tv6, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2, p2, p4);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
            AddSubdividedQuadUVs3(types, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel / 2);
        }
    }

    void TriangulateBridgeTerracesCliff(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 types, bool invertColors = false)
    {
        Color c1 = invertColors ? color2 : color1;
        Color c2 = invertColors ? color1 : color2;

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
            AddSubdividedQuadUVs3(types, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        }
    }

    void TriangulateBridgeTerraces(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 types)
    {
        for (int i = 1; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v1, v3, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(v2, v4, i);

            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(color1, color2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(color1, color2, i);

            AddNoisySubdividedQuad(tv1, tv2, tv3, tv4, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadColor(tc1, tc2, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
            AddSubdividedQuadUVs3(types, GameDataPresenter.Instance.HexMetrics.EdgeSubdivisionLevel);
        }
    }

    void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        HexTerrainType bottomType = bottomCell.TerrainType;
        HexTerrainType leftType = leftCell.TerrainType;
        HexTerrainType rightType = rightCell.TerrainType;
        Vector3 typesUV = new Vector3((int)bottomType, (int)leftType, (int)rightType);

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
            AddNoisyTriangle(bottom, left, right);
            AddTriangleColor(color1, color2, color3);
            AddTriangleUVs3(typesUV);
        }
    }
    void TriangulateCornerTerraces(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        HexTerrainType bottomType = bottomCell.TerrainType;
        HexTerrainType leftType = leftCell.TerrainType;
        HexTerrainType rightType = rightCell.TerrainType;
        Vector3 typesUV = new Vector3((int)bottomType, (int)leftType, (int)rightType);

        Vector3 v4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, left, 1);
        Vector3 v5 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, right, 1);
        Color c4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(color1, color2, 1);
        Color c5 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(color1, color2, 1);

        AddNoisyTriangle(bottom, v4, v5);
        AddTriangleColor(color1, c4, c5);
        AddTriangleUVs3(typesUV);

        for (int i = 2; i <= GameDataPresenter.Instance.HexMetrics.TerraceSteps; i++)
        {
            Vector3 tv1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, left, i - 1);
            Vector3 tv2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, right, i - 1);
            Vector3 tv3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, left, i);
            Vector3 tv4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(bottom, right, i);
            Color tc1 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(color1, color2, i - 1);
            Color tc2 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(color1, color3, i - 1);
            Color tc3 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(color1, color2, i);
            Color tc4 = GameDataPresenter.Instance.HexMetrics.TerraceLerp(color1, color3, i);

            AddNoisyQuad(tv1, tv2, tv3, tv4);
            AddQuadColor(tc1, tc2, tc3, tc4);
            AddQuadUVs3(typesUV);
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
