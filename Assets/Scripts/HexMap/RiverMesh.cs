using System.Collections.Generic;
using UnityEngine;

// Generates mesh for river rendering 
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RiverMesh : MonoBehaviour
{
    private Mesh mesh;

    // Mesh data
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Color> colors = new List<Color>();

    // Layer and water color index
    private int chunkLevel = 0;
    private int waterIndex = 0;

    private void Awake()
    {
        // Get mesh reference and assign river material
        mesh = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshRenderer>().sharedMaterial = MapManager.Instance.GetRivermaterial();
    }

    // Sets chunk layer level and water color index
    public void SetChunkLevel(int level, int index)
    {
        chunkLevel = level;
        waterIndex = index;
    }

    // Clears mesh data
    public void Clear()
    {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        colors.Clear();
    }

    // Creates a single triangle and adds it to the mesh
    public void CreateTriangle(Vector3 v1, Vector3 v2, Vector3 v3, int Index)
    {
        int index = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        // Encode water index in vertex color
        colors.Add(new Color(waterIndex, 0f, 0f, 0f));
        colors.Add(new Color(waterIndex, 0f, 0f, 0f));
        colors.Add(new Color(waterIndex, 0f, 0f, 0f));

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
    }

    // Creates a rectangle using two triangles
    public void CreateRectangle(Vector3 v1, Vector3 v1d, Vector3 v2, Vector3 v2d, int waterColorIndex)
    {
        CreateTriangle(v1, v1d, v2, waterColorIndex);
        CreateTriangle(v1d, v2d, v2, waterColorIndex);
    }

    // Adds a UV 
    public void AddUV(Vector2 uv)
    {
        uvs.Add(uv);
    }

    // Applies mesh data
    public void Apply()
    {
        // Assign layer based on chunk level
        gameObject.layer = LayerMask.NameToLayer($"Layer_{chunkLevel}");

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
