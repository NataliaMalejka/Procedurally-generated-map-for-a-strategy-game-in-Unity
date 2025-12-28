using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RiverMesh : MonoBehaviour
{
    private Mesh mesh;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Color> colors = new List<Color>();

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshRenderer>().sharedMaterial = MapManager.Instance.GetRivermaterial();
    }

    public void Clear()
    {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        colors.Clear();
    }

    public void CreateTriangle(Vector3 v1, Vector3 v2, Vector3 v3, int waterColorIndex)
    {
        int index = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        colors.Add(new Color(waterColorIndex, 0f, 0f, 0f));
        colors.Add(new Color(waterColorIndex, 0f, 0f, 0f));
        colors.Add(new Color(waterColorIndex, 0f, 0f, 0f));

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
    }

    public void CreateRectangle(Vector3 v1, Vector3 v1d, Vector3 v2, Vector3 v2d, int waterColorIndex)
    {
        CreateTriangle(v1, v1d, v2, waterColorIndex);
        CreateTriangle(v1d, v2d, v2, waterColorIndex);
    }

    public void AddUV(Vector2 uv)
    {
        uvs.Add(uv);
    }

    public void Apply()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
