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

    public void CreateTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
    {
        int index = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        AddColor(color);

    }

    public void CreateRectangularCellsConnection(Vector3 v1, Vector3 v2, int index, Color color)
    {
        Vector3 distance = HexData.GetDistanceBetweenEdges(index);

        CreateTriangle(v1, v1 + distance, v2, color);
        CreateTriangle(v1 + distance, v2 + distance, v2, color);
    }

    public void CreateTriangleCellsConnection(Vector3 v1, int index, Color color)
    {
        Vector3 distance1 = HexData.GetDistanceBetweenEdges(index);
        Vector3 distance2 = HexData.GetDistanceBetweenEdges(index+1);

        CreateTriangle(v1, v1 + distance1, v1 + distance2, color);
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
