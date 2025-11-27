using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkMesh : MonoBehaviour
{
    private Mesh mesh;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Color> colors = new List<Color>();

    private int iterations = 3;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    public void Clear()
    {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
    }

    public void CreateTriangleWithColor(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
    {
        CreateTriangle(v1, v2, v3);
        AddColor(color, color, color);
    }

    public void CreateSmoothTriangleWithColor(Vector3 c, Vector3 v1, Vector3 v2, Vector3 m1, Vector3 m2, Color color)
    {
        c = AddNoise(c);

        List<Vector3> verticles = new List<Vector3>()
        {
            AddNoise(v1), AddNoise(m1), AddNoise(m2), AddNoise(v2)
        };

        verticles = ChaikinSmooth(verticles);

        for(int i = 0; i < verticles.Count-1; i++)
        {
            CreateSmoothTriangle(c, verticles[i], verticles[i + 1]);
            AddColor(color, color, color);
        }
    }

    public void CreateTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int index = vertices.Count;

        vertices.Add(AddNoise(v1));
        vertices.Add(AddNoise(v2));
        vertices.Add(AddNoise(v3));

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
    }

    private void CreateSmoothTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int index = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
    }

    public void CreateRectangularCellsConnection(Vector3 v1, Vector3 v2, int index, Color color, HexCell neighbourCell, Vector3 m1, Vector3 m2)
    {
        Vector3 distance = HexData.GetDistanceBetweenEdges(index);

        Vector3 v1d = new Vector3(v1.x + distance.x, neighbourCell.TerreinLevel, v1.z + distance.z);
        Vector3 v2d = new Vector3(v2.x + distance.x, neighbourCell.TerreinLevel, v2.z + distance.z);
        Vector3 m1d = new Vector3(m1.x + distance.x, neighbourCell.TerreinLevel, m1.z + distance.z);
        Vector3 m2d = new Vector3(m2.x + distance.x, neighbourCell.TerreinLevel, m2.z + distance.z);

        CreateRectangle(color, neighbourCell.CellColor, v1, v1d, m1, m1d);
        CreateRectangle(color, neighbourCell.CellColor, m1, m1d, m2, m2d);
        CreateRectangle(color, neighbourCell.CellColor, m2, m2d, v2, v2d);
    }

    public void CreateSmoothConnection(Vector3 v1, Vector3 v2, int index, Color color, HexCell neighbourCell, Vector3 m1, Vector3 m2)
    {
        Vector3 distance = HexData.GetDistanceBetweenEdges(index);

        Vector3 v1d = new Vector3(v1.x + distance.x, neighbourCell.TerreinLevel, v1.z + distance.z);
        Vector3 v2d = new Vector3(v2.x + distance.x, neighbourCell.TerreinLevel, v2.z + distance.z);
        Vector3 m1d = new Vector3(m1.x + distance.x, neighbourCell.TerreinLevel, m1.z + distance.z);
        Vector3 m2d = new Vector3(m2.x + distance.x, neighbourCell.TerreinLevel, m2.z + distance.z);

        List<Vector3> verticles = new List<Vector3>()
        {
            AddNoise(v1), AddNoise(m1), AddNoise(m2), AddNoise(v2)
        };

        List<Vector3> verticlesD = new List<Vector3>()
        {
            AddNoise(v1d), AddNoise(m1d), AddNoise(m2d), AddNoise(v2d)
        };

        verticles = ChaikinSmooth(verticles);
        verticlesD = ChaikinSmooth(verticlesD);

        for (int i = 0; i < verticles.Count - 1; i++)
        {
            CreateSmoothRectangle(color, neighbourCell.CellColor, verticles[i], verticlesD[i], verticles[i + 1], verticlesD[i + 1]);
        }
    }

    private void CreateRectangle(Color cellColor, Color neighbourColor, Vector3 v1, Vector3 v1d, Vector3 v2, Vector3 v2d)
    {
        CreateTriangle(v1, v1d, v2);
        AddColor(cellColor, neighbourColor, cellColor);

        CreateTriangle(v1d, v2d, v2);
        AddColor(neighbourColor, neighbourColor, cellColor);
    }

    private void CreateSmoothRectangle(Color cellColor, Color neighbourColor, Vector3 v1, Vector3 v1d, Vector3 v2, Vector3 v2d)
    {
        CreateSmoothTriangle(v1, v1d, v2);
        AddColor(cellColor, neighbourColor, cellColor);

        CreateSmoothTriangle(v1d, v2d, v2);
        AddColor(neighbourColor, neighbourColor, cellColor);
    }

    public void CreateTriangleCellsConnection(Vector3 v1, int index, Color color, HexCell neighbourCell, HexCell nextNeighbourCell)
    {
        Vector3 distance1 = HexData.GetDistanceBetweenEdges(index);
        Vector3 distance2 = HexData.GetDistanceBetweenEdges(index +1);

        Vector3 v1d1 = v1 + distance1;
        Vector3 v1d2 = v1 + distance2;
        v1d1.y = neighbourCell.TerreinLevel;
        v1d2.y = nextNeighbourCell.TerreinLevel;

        CreateTriangle(v1, v1d1, v1d2);
        AddColor(color, neighbourCell.CellColor, nextNeighbourCell.CellColor);
    }

    List<Vector3> ChaikinSmooth(List<Vector3> verticles)
    {
        for (int i = 0; i < iterations; i++)
        {
            List<Vector3> outPts = new List<Vector3>();

            outPts.Add(verticles[0]);

            for (int j = 0; j < verticles.Count - 1; j++)
            {
                Vector3 p0 = verticles[j];
                Vector3 p1 = verticles[j + 1];

                Vector3 Q = 0.75f * p0 + 0.25f * p1;
                Vector3 R = 0.25f * p0 + 0.75f * p1;

                outPts.Add(Q);
                outPts.Add(R);
            }

            outPts.Add(verticles[verticles.Count - 1]);

            verticles = outPts;
        }

        return verticles;
    }

    private void AddColor(Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }

    private Vector3 AddNoise(Vector3 v)
    {
        Vector3 chunkPos = this.GetComponentInParent<Transform>().position;

        Vector4 noise = MapManager.Instance.hexMeshNoise.GetPixelBilinear(v.x + chunkPos.x, v.z + chunkPos.z);

        v.x += noise.x * HexData.hexMeshNoiseStrength;
        v.z += noise.z * HexData.hexMeshNoiseStrength;

        return v;
    }

    public void Apply()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
