using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkMesh : MonoBehaviour
{
    private Mesh mesh;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Color> colors = new List<Color>();

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
        int index = vertices.Count;

        vertices.Add(AddNoise(v1));
        vertices.Add(AddNoise(v2));
        vertices.Add(AddNoise(v3));

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        AddColor(color, color, color);
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

    public void CreateRectangularCellsConnection(Vector3 v1, Vector3 v2, int index, Color color, HexCell neighbourCell, Vector3 m1, Vector3 m2)
    {
        Vector3 distance = HexData.GetDistanceBetweenEdges(index);

        Vector3 v1d = v1 + distance;
        v1d.y = neighbourCell.TerreinLevel;
        Vector3 v2d = v2 + distance;
        v2d.y = neighbourCell.TerreinLevel;

        Vector3 m1d = m1 + distance;
        m1d.y = neighbourCell.TerreinLevel;
        Vector3 m2d = m2 + distance;
        m2d.y = neighbourCell.TerreinLevel;

        CreateRectangle(color, neighbourCell.CellColor, v1, v1d, m1, m1d);
        CreateRectangle(color, neighbourCell.CellColor, m1, m1d, m2, m2d);
        CreateRectangle(color, neighbourCell.CellColor, m2, m2d, v2, v2d);
    }

    private void CreateRectangle(Color cellColor, Color neighbourColor, Vector3 v1, Vector3 v1d, Vector3 v2, Vector3 v2d)
    {
        CreateTriangle(v1, v1d, v2);
        AddColor(cellColor, neighbourColor, cellColor);

        CreateTriangle(v1d, v2d, v2);
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
