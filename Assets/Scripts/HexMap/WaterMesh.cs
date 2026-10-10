using System.Collections.Generic;
using UnityEngine;

// Generates ocean mesh
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterMesh : MonoBehaviour
{
    private Mesh mesh;

    // Mesh data
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Color> colors = new List<Color>();

    // Chunk layer index
    private int chunkLevel = 0;

    private void Awake()
    {
        // Get mesh reference and assign water material
        mesh = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshRenderer>().sharedMaterial = MapManager.Instance.GetWatermaterial();
    }

    // Sets chunk layer level
    public void SetChunkLevel(int level)
    {
        chunkLevel = level;
    }

    // Clears mesh data
    public void Clear()
    {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
    }

    // Creates an ocean triangle
    public void CreateTriangle(Vector3 v1, Vector3 v2, Vector3 v3, bool isOcean, int waterColor)
    {
        int index = vertices.Count;

        // Apply correct water height
        if (isOcean)
        {
            v1 = ApplyOceanLevel(v1);
            v2 = ApplyOceanLevel(v2);
            v3 = ApplyOceanLevel(v3);
        }
        else
        {
            v1 = ApplyWaterLevel(v1);
            v2 = ApplyWaterLevel(v2);
            v3 = ApplyWaterLevel(v3);
        }

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        // Encode water color and chunk level in vertex color
        colors.Add(new Color(waterColor, chunkLevel, 0f, 0f));
        colors.Add(new Color(waterColor, chunkLevel, 0f, 0f));
        colors.Add(new Color(waterColor, chunkLevel, 0f, 0f));

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
    }

    // Creates a rectangle using two triangles
    public void CreateRectangle(Vector3 v1, Vector3 v1d, Vector3 v2, Vector3 v2d, bool isOcean, int waterColor)
    {
        CreateTriangle(v1, v1d, v2, isOcean, waterColor);

        CreateTriangle(v1d, v2d, v2, isOcean, waterColor);
    }

    // Applies lake level
    private Vector3 ApplyWaterLevel(Vector3 v)
    {
        return new Vector3(v.x, v.y + HexData.waterLevel, v.z);
    }

    // Applies ocean level
    private Vector3 ApplyOceanLevel(Vector3 v)
    {
        return new Vector3(v.x, HexData.oceanWaterLevel, v.z);
    }

    // Applies mesh data 
    public void Apply()
    {
        gameObject.layer = LayerMask.NameToLayer($"Layer_{chunkLevel}");

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
