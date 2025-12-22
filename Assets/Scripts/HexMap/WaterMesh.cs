using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterMesh : MonoBehaviour
{
    private Mesh mesh;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshRenderer>().sharedMaterial = MapManager.Instance.GetWatermaterial();
    }

    public void Clear()
    {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
    }

    public void CreateTriangle(Vector3 v1, Vector3 v2, Vector3 v3, bool isOcean)
    {
        int index = vertices.Count;

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

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
    }

    public void CreateRectangle(Vector3 v1, Vector3 v1d, Vector3 v2, Vector3 v2d, bool isOcean)
    {
        CreateTriangle(v1, v1d, v2, isOcean);

        CreateTriangle(v1d, v2d, v2, isOcean);
    }

    private Vector3 ApplyWaterLevel(Vector3 v)
    {
        return new Vector3(v.x, v.y + HexData.waterLevel, v.z);
    }

    private Vector3 ApplyOceanLevel(Vector3 v)
    {
        return new Vector3(v.x, HexData.oceanWaterLevel, v.z);
    }

    public void Apply()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
