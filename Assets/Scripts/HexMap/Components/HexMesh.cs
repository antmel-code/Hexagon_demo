using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public abstract class HexMesh : MonoBehaviour
{
    Mesh hexMesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Color> colors = new List<Color>();
    List<Vector2> uvs = new List<Vector2>();
    List<Vector2> uvs2 = new List<Vector2>();
    List<Vector3> uvs3 = new List<Vector3>();

    protected bool useVertexColors = false;
    protected bool useTextureCoordinates1 = false;
    protected bool useTextureCoordinates2 = false;
    protected bool useTextureCoordinates3 = false;
    protected bool useCollider = false;

    MeshCollider meshCollider;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        hexMesh.name = "Hex Mesh";

        if (useCollider)
            meshCollider = gameObject.AddComponent<MeshCollider>();
    }

    /// <summary>
    /// Here You have to say what mesh data You are going to use
    /// </summary>
    public abstract void Init();

    public void Clear()
    {
        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();

        if (useVertexColors)
            colors.Clear();
        if (useTextureCoordinates1)
            uvs.Clear();
        if (useTextureCoordinates2)
            uvs2.Clear();
        if (useTextureCoordinates3)
            uvs3.Clear();
    }

    public void Apply()
    {
        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.colors = colors.ToArray();
        hexMesh.RecalculateNormals();

        if (useTextureCoordinates1)
            hexMesh.uv = uvs.ToArray();
        if (useTextureCoordinates2)
            hexMesh.uv2 = uvs2.ToArray();
        if (useTextureCoordinates3)
            hexMesh.SetUVs(2, uvs3.ToArray());

        if (useCollider)
            meshCollider.sharedMesh = hexMesh;
    }

    protected void AddTriangleColor(Color color)
    {
        AddTriangleColor(color, color, color);
    }

    protected void AddTriangleColor(Color color1, Color color2, Color color3)
    {
        if (!useVertexColors)
        {
            Debug.LogWarning("Vertex color usage is disabled");
            return;
        }
        colors.Add(color1);
        colors.Add(color2);
        colors.Add(color3);
    }

    protected void AddTriangleUVs(Vector2 uv1, Vector2 uv2, Vector3 uv3)
    {
        if (!useTextureCoordinates1)
        {
            Debug.LogWarning("Texture coordinates usage is disabled");
            return;
        }
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
    }

    protected void AddTriangleUVs2(Vector2 uv1, Vector2 uv2, Vector3 uv3)
    {
        if (!useTextureCoordinates2)
        {
            Debug.LogWarning("Texture coordinates (2nd chanel) usage is disabled");
            return;
        }
        uvs2.Add(uv1);
        uvs2.Add(uv2);
        uvs2.Add(uv3);
    }

    protected void AddTriangleUVs3(Vector3 uv1, Vector3 uv2, Vector3 uv3)
    {
        if (!useTextureCoordinates3)
        {
            Debug.LogWarning("Texture coordinates (3rd chanel) usage is disabled");
            return;
        }
        uvs3.Add(uv1);
        uvs3.Add(uv2);
        uvs3.Add(uv3);
    }

    protected void AddTriangleUVs3(Vector3 uv)
    {
        AddTriangleUVs3(uv, uv, uv);
    }

    protected void AddNoisyTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        AddTriangle(Pertrub(a), Pertrub(b), Pertrub(c));
    }

    protected void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        int currentIndex = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        triangles.Add(currentIndex);
        triangles.Add(currentIndex + 1);
        triangles.Add(currentIndex + 2);
    }

    protected void AddNoisyTriangleFan(Vector3 a, Vector3 b, Vector3 c, int subdivisions)
    {
        float part = 1f / subdivisions;
        for (int i = 1; i <= subdivisions; i++)
        {
            Vector3 sb = Vector3.Lerp(b, c, part * (i - 1));
            Vector3 sc = Vector3.Lerp(b, c, part * i);
            AddNoisyTriangle(a, sb, sc);
        }
    }

    protected void AddTriangleFanUVs(Vector2 uva, Vector2 uvb, Vector2 uvc, int subdivisions)
    {
        if (!useTextureCoordinates1)
        {
            Debug.LogWarning("Texture coordinates usage is disabled");
            return;
        }
        float part = 1f / subdivisions;
        for (int i = 1; i <= subdivisions; i++)
        {
            Vector2 suvb = Vector2.Lerp(uvb, uvc, part * (i - 1));
            Vector2 suvc = Vector2.Lerp(uvb, uvc, part * i);
            AddTriangleUVs(uva, suvb, suvc);
        }
    }

    protected void AddTriangleFanUVs2(Vector2 uva, Vector2 uvb, Vector2 uvc, int subdivisions)
    {
        if (!useTextureCoordinates2)
        {
            Debug.LogWarning("Texture coordinates (2nd chanel) usage is disabled");
            return;
        }
        float part = 1f / subdivisions;
        for (int i = 1; i <= subdivisions; i++)
        {
            Vector2 suvb = Vector2.Lerp(uvb, uvc, part * (i - 1));
            Vector2 suvc = Vector2.Lerp(uvb, uvc, part * i);
            AddTriangleUVs2(uva, suvb, suvc);
        }
    }

    protected void AddTriangleFanUVs3(Vector3 uva, Vector3 uvb, Vector3 uvc, int subdivisions)
    {
        if (!useTextureCoordinates3)
        {
            Debug.LogWarning("Texture coordinates (3rd chanel) usage is disabled");
            return;
        }
        float part = 1f / subdivisions;
        for (int i = 1; i <= subdivisions; i++)
        {
            Vector3 suvb = Vector3.Lerp(uvb, uvc, part * (i - 1));
            Vector3 suvc = Vector3.Lerp(uvb, uvc, part * i);
            AddTriangleUVs3(uva, suvb, suvc);
        }
    }

    protected void AddTriangleFanColor(Color color1, Color color2, Color color3, int subdivisions)
    {
        if (!useVertexColors)
        {
            Debug.LogWarning("Vertex color usage is disabled");
            return;
        }
        float part = 1f / subdivisions;
        for (int i = 1; i <= subdivisions; i++)
        {
            Color sc2 = Color.Lerp(color2, color3, part * (i - 1));
            Color sc3 = Color.Lerp(color2, color3, part * i);
            AddTriangleColor(color1, sc2, sc3);
        }
    }

    protected void AddTriangleFanColor(Color color, int subdivisions)
    {
        AddTriangleFanColor(color, color, color, subdivisions);
    }

    protected void AddNoisyQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        AddQuad(Pertrub(v1), Pertrub(v2), Pertrub(v3), Pertrub(v4));

    }

    protected void AddQuadUVs(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
    {
        if (!useTextureCoordinates1)
        {
            Debug.LogWarning("Texture coordinates usage is disabled");
            return;
        }
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
        uvs.Add(uv4);
    }

    protected void AddQuadUVs(float uMin, float uMax, float vMin, float vMax)
    {
        if (!useTextureCoordinates1)
        {
            Debug.LogWarning("Texture coordinates usage is disabled");
            return;
        }
        uvs.Add(new Vector2(uMin, vMin));
        uvs.Add(new Vector2(uMax, vMin));
        uvs.Add(new Vector2(uMin, vMax));
        uvs.Add(new Vector2(uMax, vMax));
    }

    protected void AddQuadUVs2(float uMin, float uMax, float vMin, float vMax)
    {
        if (!useTextureCoordinates2)
        {
            Debug.LogWarning("Texture coordinates (2nd chanel) usage is disabled");
            return;
        }
        uvs2.Add(new Vector2(uMin, vMin));
        uvs2.Add(new Vector2(uMax, vMin));
        uvs2.Add(new Vector2(uMin, vMax));
        uvs2.Add(new Vector2(uMax, vMax));
    }

    protected void AddQuadUVs2(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
    {
        if (!useTextureCoordinates2)
        {
            Debug.LogWarning("Texture coordinates (2nd chanel) usage is disabled");
            return;
        }
        uvs2.Add(uv1);
        uvs2.Add(uv2);
        uvs2.Add(uv3);
        uvs2.Add(uv4);
    }

    protected void AddQuadUVs3(Vector3 uv1, Vector3 uv2, Vector3 uv3, Vector3 uv4)
    {
        if (!useTextureCoordinates3)
        {
            Debug.LogWarning("Texture coordinates (3rd chanel) usage is disabled");
            return;
        }
        uvs3.Add(uv1);
        uvs3.Add(uv2);
        uvs3.Add(uv3);
        uvs3.Add(uv4);
    }

    protected void AddQuadUVs3(Vector3 uv)
    {
        AddQuadUVs3(uv, uv, uv, uv);
    }

    protected void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);

    }

    protected void AddQuadColor(Color c1, Color c2)
    {
        AddQuadColor(c1, c1, c2, c2);
    }
    protected void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        if (!useVertexColors)
        {
            Debug.LogWarning("Vertex colors usage is disabled");
            return;
        }
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }

    protected void AddNoisySubdividedQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, int subdivisions)
    {
        float part = 1f / subdivisions;
        for (int i = 1; i <= subdivisions; i++)
        {
            Vector3 sv1 = Vector3.Lerp(v1, v2, part * (i - 1));
            Vector3 sv2 = Vector3.Lerp(v1, v2, part * i);
            Vector3 sv3 = Vector3.Lerp(v3, v4, part * (i - 1));
            Vector3 sv4 = Vector3.Lerp(v3, v4, part * i);
            AddNoisyQuad(sv1, sv2, sv3, sv4);
        }
    }

    protected void AddNoisySubdividedQuadPinnedRight(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, int subdivisions, Vector3 p1, Vector3 p2)
    {
        float part = 1f / subdivisions;
        Vector3 sv1;
        Vector3 sv2;
        Vector3 sv3;
        Vector3 sv4;
        for (int i = 1; i < subdivisions; i++)
        {
            sv1 = Vector3.Lerp(v1, v2, part * (i - 1));
            sv2 = Vector3.Lerp(v1, v2, part * i);
            sv3 = Vector3.Lerp(v3, v4, part * (i - 1));
            sv4 = Vector3.Lerp(v3, v4, part * i);
            AddNoisyQuad(sv1, sv2, sv3, sv4);
        }
        sv1 = Vector3.Lerp(v1, v2, part * (subdivisions - 1));
        sv2 = p1;
        sv3 = Vector3.Lerp(v3, v4, part * (subdivisions - 1));
        sv4 = p2;
        AddQuad(Pertrub(sv1), sv2, Pertrub(sv3), sv4);
    }

    protected void AddNoisySubdividedQuadPinnedLeft(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, int subdivisions, Vector3 p1, Vector3 p2)
    {
        float part = 1f / subdivisions;
        Vector3 sv1;
        Vector3 sv2;
        Vector3 sv3;
        Vector3 sv4;
        sv1 = p1;
        sv2 = Vector3.Lerp(v1, v2, part);
        sv3 = p2;
        sv4 = Vector3.Lerp(v3, v4, part);
        AddQuad(sv1, Pertrub(sv2), sv3, Pertrub(sv4));
        for (int i = 2; i <= subdivisions; i++)
        {
            sv1 = Vector3.Lerp(v1, v2, part * (i - 1));
            sv2 = Vector3.Lerp(v1, v2, part * i);
            sv3 = Vector3.Lerp(v3, v4, part * (i - 1));
            sv4 = Vector3.Lerp(v3, v4, part * i);
            AddNoisyQuad(sv1, sv2, sv3, sv4);
        }
        
    }

    protected void AddSubdividedQuadColor(Color c1, Color c2, Color c3, Color c4, int subdivisions)
    {
        if (!useVertexColors)
        {
            Debug.LogWarning("Vertex color usage is disabled");
            return;
        }
        float part = 1f / subdivisions;
        for (int i = 1; i <= subdivisions; i++)
        {
            Color sc1 = Color.Lerp(c1, c2, part * (i - 1));
            Color sc2 = Color.Lerp(c1, c2, part * i);
            Color sc3 = Color.Lerp(c3, c4, part * (i - 1));
            Color sc4 = Color.Lerp(c3, c4, part * i);
            AddQuadColor(sc1, sc2, sc3, sc4);
        }
    }

    protected void AddSubdividedQuadColor(Color color1, Color color2, int subdivisions)
    {
        AddSubdividedQuadColor(color1, color1, color2, color2, subdivisions);
    }

    protected void AddSubdividedQuadColor(Color color, int subdivisions)
    {
        AddSubdividedQuadColor(color, color, color, color, subdivisions);
    }

    protected void AddSubdividedQuadUVs(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4, int subdivisions)
    {
        float part = 1f / subdivisions;
        for (int i = 1; i <= subdivisions; i++)
        {
            Vector2 suv1 = Vector2.Lerp(uv1, uv2, part * (i - 1));
            Vector2 suv2 = Vector2.Lerp(uv1, uv2, part * i);
            Vector2 suv3 = Vector2.Lerp(uv3, uv4, part * (i - 1));
            Vector2 suv4 = Vector2.Lerp(uv3, uv4, part * i);
            AddQuadUVs(suv1, suv2, suv3, suv4);
        }
    }

    protected void AddSubdividedQuadUVs2(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4, int subdivisions)
    {
        float part = 1f / subdivisions;
        for (int i = 1; i <= subdivisions; i++)
        {
            Vector2 suv1 = Vector2.Lerp(uv1, uv2, part * (i - 1));
            Vector2 suv2 = Vector2.Lerp(uv1, uv2, part * i);
            Vector2 suv3 = Vector2.Lerp(uv3, uv4, part * (i - 1));
            Vector2 suv4 = Vector2.Lerp(uv3, uv4, part * i);
            AddQuadUVs2(suv1, suv2, suv3, suv4);
        }
    }

    protected void AddSubdividedQuadUVs3(Vector3 uv1, Vector3 uv2, Vector3 uv3, Vector3 uv4, int subdivisions)
    {
        float part = 1f / subdivisions;
        for (int i = 1; i <= subdivisions; i++)
        {
            Vector3 suv1 = Vector3.Lerp(uv1, uv2, part * (i - 1));
            Vector3 suv2 = Vector3.Lerp(uv1, uv2, part * i);
            Vector3 suv3 = Vector3.Lerp(uv3, uv4, part * (i - 1));
            Vector3 suv4 = Vector3.Lerp(uv3, uv4, part * i);
            AddQuadUVs3(suv1, suv2, suv3, suv4);
        }
    }

    protected void AddSubdividedQuadUVs3(Vector3 uv, int subdivisions)
    {
        AddSubdividedQuadUVs3(uv, uv, uv, uv, subdivisions);
    }

    protected void AddSubdividedQuadUVs(float uMin, float uMax, float vMin, float vMax, int subdivisions)
    {
        Vector2 uv1 = new Vector2(uMin, vMin);
        Vector2 uv2 = new Vector2(uMax, vMin);
        Vector2 uv3 = new Vector2(uMin, vMax);
        Vector2 uv4 = new Vector2(uMax, vMax);
        AddSubdividedQuadUVs(uv1, uv2, uv3, uv4, subdivisions);
    }

    protected void AddSubdividedQuadUVs2(float uMin, float uMax, float vMin, float vMax, int subdivisions)
    {
        Vector2 uv1 = new Vector2(uMin, vMin);
        Vector2 uv2 = new Vector2(uMax, vMin);
        Vector2 uv3 = new Vector2(uMin, vMax);
        Vector2 uv4 = new Vector2(uMax, vMax);
        AddSubdividedQuadUVs2(uv1, uv2, uv3, uv4, subdivisions);
    }

    protected Vector3 Pertrub(Vector3 position)
    {
        Vector4 noise = GameDataPresenter.Instance.HexMetrics.GetLoopedNoise(position);
        position.x += (noise.x * 2f - 1) * GameDataPresenter.Instance.HexMetrics.NoiseStrenght;
        position.z += (noise.z * 2f - 1) * GameDataPresenter.Instance.HexMetrics.NoiseStrenght;
        return position;
    }
}
