using System.Collections.Generic;
using Unity.VisualScripting;
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

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        for (int i = 0; i < 3; i++)
        {
            colors.Add(color);
        }

    }

    public void CreateTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int index = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

    }

    public void CreateRectangularCellsConnection(Vector3 v1, Vector3 v2, int index, Color color, HexCell neighbourCell)
    {
        Vector3 distance = HexData.GetDistanceBetweenEdges(index);

        Vector3 v1d = v1 + distance;
        Vector3 v2d = v2 + distance;
        v1d.y = neighbourCell.TerreinLevel;
        v2d.y = neighbourCell.TerreinLevel;

        CreateTriangle(v1, v1d, v2);
        colors.Add(color);
        colors.Add(neighbourCell.CellColor);
        colors.Add(color);

        CreateTriangle(v1d, v2d, v2);
        colors.Add(neighbourCell.CellColor);
        colors.Add(neighbourCell.CellColor);
        colors.Add(color);
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
        colors.Add(color);
        colors.Add(neighbourCell.CellColor);
        colors.Add(nextNeighbourCell.CellColor);
    }

    private void AddColor(Color color)
    {
        for (int i = 0; i < 3; i++)
        {
            colors.Add(color);
        }
    }

    public void AddNoise(Vector3 v)
    {
        
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
