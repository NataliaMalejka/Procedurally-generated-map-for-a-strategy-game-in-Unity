using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkMesh : MonoBehaviour
{
    private Mesh mesh;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector4> uv3 = new List<Vector4>();
    private List<Color> colors = new List<Color>();

    private int iterations = 2;
    private float noiseStrengthNormal = 3f;
    private float noiseStrengthSmooth = 0.3f;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshRenderer>().sharedMaterial = MapManager.Instance.terrainMaterial;
    }

    public void Clear()
    {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        uv3.Clear();
        colors.Clear();
    }

    public void CreateTriangleWithColor(Vector3 c, Vector3 v1, Vector3 v2, int t, HexCell cell, int index)
    {
        Vector3 m1 = cell.GetEdge(index).GetMiddle1();
        Vector3 m2 = cell.GetEdge(index).GetMiddle2();

        if ((cell.GetEdge((index + 5) % 6).GetEdgeType() != EdgeType.Smooth))
        {
            v1 = AddNoise(v1, noiseStrengthNormal);
        }

        if ((cell.GetEdge((index + 1) % 6).GetEdgeType() != EdgeType.Smooth))
        {
            v2 = AddNoise(v2, noiseStrengthNormal);
        }

        CreateTriangle(AddNoise(c, noiseStrengthNormal), v1, AddNoise(m1, noiseStrengthNormal));
        AddTexture(t, t, t);
        CreateTriangle(AddNoise(c, noiseStrengthNormal), AddNoise(m1, noiseStrengthNormal), AddNoise(m2, noiseStrengthNormal));
        AddTexture(t, t, t);
        CreateTriangle(AddNoise(c, noiseStrengthNormal), AddNoise(m2, noiseStrengthNormal), v2);
        AddTexture(t, t, t);

    }

    public void CreateSmoothTriangleWithColor(Vector3 c, Vector3 v1, Vector3 v2, Vector3 m1, Vector3 m2, int t)
    {
        c = AddNoise(c, noiseStrengthNormal);

        List<Vector3> verticles = new List<Vector3>()
        {
            (v1), AddNoise(m1,  noiseStrengthSmooth), AddNoise(m2, noiseStrengthSmooth), (v2)
        };

        verticles = ChaikinSmooth(verticles);

        for(int i = 0; i < verticles.Count-1; i++)
        {
            CreateTriangle(c, verticles[i], verticles[i + 1]);
            AddTexture(t, t, t);
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

    public void CreateRectangularCellsConnection(Vector3 v1, Vector3 v2, int index, int t, HexCell neighbourCell, Vector3 m1, Vector3 m2, HexCell cell)
    {
        Vector3 v1d = neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.GetEdge((index + 3) % 6).GetLocalV2());
        Vector3 v2d = neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.GetEdge((index + 3) % 6).GetLocalV1());
        Vector3 m1d = neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.GetEdge((index + 3) % 6).GetMiddle2());
        Vector3 m2d = neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.GetEdge((index + 3) % 6).GetMiddle1());

        v1d = transform.InverseTransformPoint(v1d);
        v2d = transform.InverseTransformPoint(v2d);
        m1d = transform.InverseTransformPoint(m1d);
        m2d = transform.InverseTransformPoint(m2d);

        int t2 = neighbourCell.TextureIndex;

        if ((cell.GetEdge((index + 5) % 6).GetEdgeType() != EdgeType.Smooth))
        {
            v1 = AddNoise(v1, noiseStrengthNormal);
        }
        if ((cell.GetEdge((index + 1) % 6).GetEdgeType() != EdgeType.Smooth))
        {
            v2 = AddNoise(v2, noiseStrengthNormal);
        }

        if ((neighbourCell.GetEdge((index + 3) % 6).GetEdgeType() != EdgeType.Smooth))
        {
            if ((neighbourCell.GetEdge((index + 4) % 6).GetEdgeType() != EdgeType.Smooth))
                v1d = AddNoise(v1d, noiseStrengthNormal);

            if ((neighbourCell.GetEdge((index + 2) % 6).GetEdgeType() != EdgeType.Smooth))
                v2d = AddNoise(v2d, noiseStrengthNormal);
        }

        if(cell.GetEdge(index).GetEdgeType() == EdgeType.Cliff)
        {
            t = 1;
            t2 = 1;
        }

        CreateRectangle(t, t2, v1, v1d, AddNoise(m1, noiseStrengthNormal), AddNoise(m1d, noiseStrengthNormal));
        CreateRectangle(t, t2, AddNoise(m1, noiseStrengthNormal), AddNoise(m1d, noiseStrengthNormal), AddNoise(m2, noiseStrengthNormal), AddNoise(m2d, noiseStrengthNormal));
        CreateRectangle(t, t2, AddNoise(m2, noiseStrengthNormal), AddNoise(m2d, noiseStrengthNormal), v2, v2d);
    }

    public void CreateSmoothConnection(Vector3 v1, Vector3 v2, int index, int t, HexCell neighbourCell, Vector3 m1, Vector3 m2)
    {
        Vector3 v1d = neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.GetEdge((index + 3) % 6).GetLocalV2());
        Vector3 v2d = neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.GetEdge((index + 3) % 6).GetLocalV1());
        Vector3 m1d = neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.GetEdge((index + 3) % 6).GetMiddle2());
        Vector3 m2d = neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.GetEdge((index + 3) % 6).GetMiddle1());

        v1d = transform.InverseTransformPoint(v1d);
        v2d = transform.InverseTransformPoint(v2d);
        m1d = transform.InverseTransformPoint(m1d);
        m2d = transform.InverseTransformPoint(m2d);

        List<Vector3> verticles = new List<Vector3>()
        {
            (v1), AddNoise(m1, noiseStrengthSmooth), AddNoise(m2, noiseStrengthSmooth), (v2)
        };

        List<Vector3> verticlesD = new List<Vector3>()
        {
            (v1d), AddNoise(m1d, noiseStrengthSmooth), AddNoise(m2d, noiseStrengthSmooth), (v2d)
        };

        verticles = ChaikinSmooth(verticles);
        verticlesD = ChaikinSmooth(verticlesD);

        for (int i = 0; i < verticles.Count - 1; i++)
        {
            CreateRectangle(t, neighbourCell.TextureIndex, verticles[i], verticlesD[i], verticles[i + 1], verticlesD[i + 1]);
        }
    }

    private void CreateRectangle(int t, int neighbourT, Vector3 v1, Vector3 v1d, Vector3 v2, Vector3 v2d)
    {
        CreateTriangle(v1, v1d, v2);
        AddTexture(t, neighbourT, t);

        CreateTriangle(v1d, v2d, v2);
        AddTexture(neighbourT, neighbourT, t);
    }

    public void CreateTriangleCellsConnection(Vector3 v1, int index, int t, HexCell cell ,HexCell neighbourCell, HexCell nextNeighbourCell)
    {
        var cellEdge = cell.GetEdge((index) % 6).GetEdgeType();
        var cellEdge2 = cell.GetEdge((index + 1) % 6).GetEdgeType();

        if (cellEdge != EdgeType.Smooth && cellEdge2 != EdgeType.Smooth)
        {
            v1 = AddNoise(v1, noiseStrengthNormal);
        }

        Vector3 v1d1;
        Vector3 v1d2;

        int t2 = neighbourCell.TextureIndex;
        int t3 = nextNeighbourCell.TextureIndex;

        if (cellEdge == EdgeType.Cliff || cellEdge2 == EdgeType.Cliff)
            t = 1;

        if (index == 0) 
        {
            v1d1 = neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.GetEdge(3).GetLocalV1());
            v1d2 = nextNeighbourCell.HexChunk.transform.TransformPoint(nextNeighbourCell.GetEdge(5).GetLocalV1());

            v1d1 = transform.InverseTransformPoint(v1d1);
            v1d2 = transform.InverseTransformPoint(v1d2);

            var edge2 = neighbourCell.GetEdge(2).GetEdgeType();
            var edge3 = neighbourCell.GetEdge(3).GetEdgeType();
            var edge4 = nextNeighbourCell.GetEdge(4).GetEdgeType();
            var edge5 = nextNeighbourCell.GetEdge(5).GetEdgeType();

            if (edge2 != EdgeType.Smooth && edge3 != EdgeType.Smooth)
            {
                v1d1 = AddNoise(v1d1, noiseStrengthNormal);
            }

            if (edge2 == EdgeType.Cliff || edge3 == EdgeType.Cliff)
                t2 = 1;

            if (edge4 != EdgeType.Smooth && edge5 != EdgeType.Smooth)
            {
                v1d2 = AddNoise(v1d2, noiseStrengthNormal);               
            }

            if (edge4 == EdgeType.Cliff || edge5 == EdgeType.Cliff)
                t3 = 1;
        }
        else 
        {
            v1d1 = neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.GetEdge(3).GetLocalV2());
            v1d2 = nextNeighbourCell.HexChunk.transform.TransformPoint(nextNeighbourCell.GetEdge(5).GetLocalV2());

            v1d1 = transform.InverseTransformPoint(v1d1);
            v1d2 = transform.InverseTransformPoint(v1d2);

            var edge3 = neighbourCell.GetEdge(3).GetEdgeType();
            var edge4 = neighbourCell.GetEdge(4).GetEdgeType();
            var edge5 = nextNeighbourCell.GetEdge(5).GetEdgeType();
            var edge0 = nextNeighbourCell.GetEdge(0).GetEdgeType();

            if (edge3 != EdgeType.Smooth && edge4 != EdgeType.Smooth)
            {
                v1d1 = AddNoise(v1d1, noiseStrengthNormal);        
            }

            if (edge5 != EdgeType.Smooth && edge0 != EdgeType.Smooth)
            {
                v1d2 = AddNoise(v1d2, noiseStrengthNormal);
            }

            if (edge3 == EdgeType.Cliff || edge4 == EdgeType.Cliff)
                t2 = 1;

            if (edge5 == EdgeType.Cliff || edge0 == EdgeType.Cliff)
                t3 = 1;
        }

        CreateTriangle((v1), (v1d1), (v1d2));
        AddTexture(t, t2, t3);
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

    private void AddTexture(int t1, int t2, int t3)
    {
        colors.Add(new Color(1f, 0f, 0f, 0f));
        uv3.Add(new Vector4(t1, t2, t3, 0f));

        colors.Add(new Color(0f, 1f, 0f, 0f));
        uv3.Add(new Vector4(t1, t2, t3, 0f));

        colors.Add(new Color(0f, 0f, 1f, 0f));
        uv3.Add(new Vector4(t1, t2, t3, 0f));
    }

    private Vector3 AddNoise(Vector3 v, float noiseStrength)
    {
        Vector3 chunkPos = this.GetComponentInParent<Transform>().position;

        Vector4 noise = MapManager.Instance.hexMeshNoise.GetPixelBilinear(v.x + chunkPos.x, v.z + chunkPos.z);

        v.x += noise.x * noiseStrength;
        v.z += noise.z * noiseStrength;

        return v;
    }

    public void Apply()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.SetUVs(2, uv3);

        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
