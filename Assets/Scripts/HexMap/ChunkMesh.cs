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
        GetComponent<MeshRenderer>().sharedMaterial = MapManager.Instance.GetTerrainmaterial();
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

        if (cell.IsMountain)
        {
            CreateMountainSlope(c, v1, v2, t, m1, m2, index, cell);
        }
        else
        { 
            CreateTriangle(AddNoise(c, noiseStrengthNormal), v1, AddNoise(m1, noiseStrengthNormal));
            AddTexture(t, t, t, cell.Temperature);
            CreateTriangle(AddNoise(c, noiseStrengthNormal), AddNoise(m1, noiseStrengthNormal), AddNoise(m2, noiseStrengthNormal));
            AddTexture(t, t, t, cell.Temperature);
            CreateTriangle(AddNoise(c, noiseStrengthNormal), AddNoise(m2, noiseStrengthNormal), v2);
            AddTexture(t, t, t, cell.Temperature);       
        }
    }

    private void CreateMountainSlope(Vector3 c, Vector3 v1, Vector3 v2, int t, Vector3 m1, Vector3 m2, int index, HexCell cell)
    {
        var s1v1 = Vector3.Lerp(c, v1, 1f / 3f);
        var s2v1 = Vector3.Lerp(c, v1, 2f / 3f);
        var s1v2 = Vector3.Lerp(c, v2, 1f / 3f);
        var s2v2 = Vector3.Lerp(c, v2, 2f / 3f);
        var s1m1 = Vector3.Lerp(c, m1, 1f / 3f);
        var s2m1 = Vector3.Lerp(c, m1, 2f / 3f);
        var s1m2 = Vector3.Lerp(c, m2, 1f / 3f);
        var s2m2 = Vector3.Lerp(c, m2, 2f / 3f);

        CreateTriangle(AddNoise(c, noiseStrengthNormal), s1v1, AddNoise(s1m1, noiseStrengthNormal));
        AddTexture(t, t, t, cell.Temperature);
        CreateTriangle(AddNoise(c, noiseStrengthNormal), AddNoise(s1m1, noiseStrengthNormal), AddNoise(s1m2, noiseStrengthNormal));
        AddTexture(t, t, t, cell.Temperature);
        CreateTriangle(AddNoise(c, noiseStrengthNormal), AddNoise(s1m2, noiseStrengthNormal), s1v2);
        AddTexture(t, t, t, cell.Temperature);

        CreateRectangle(t,t, s1v1, s2v1, AddNoise(s1m1, noiseStrengthNormal), AddNoise(s2m1, noiseStrengthNormal), cell.Temperature);
        CreateRectangle(t, t, AddNoise(s1m1, noiseStrengthNormal), AddNoise(s2m1, noiseStrengthNormal), AddNoise(s1m2, noiseStrengthNormal), AddNoise(s2m2, noiseStrengthNormal), cell.Temperature);
        CreateRectangle(t, t, AddNoise(s1m2, noiseStrengthNormal), AddNoise(s2m2, noiseStrengthNormal), s1v2, s2v2, cell.Temperature);

        if(cell.GetNeighbor((HexDirection)index).IsMountain)
        {
            CreateTriangle(s2v1, v1, AddNoise(s2m1, noiseStrengthNormal));
            AddTexture(t, t, t, cell.Temperature);
            CreateTriangle(s2v2, AddNoise(s2m2, noiseStrengthNormal), v2);
            AddTexture(t, t, t, cell.Temperature);
        }
        else
        {
            CreateRectangle(t, t, s2v1, v1, AddNoise(s2m1, noiseStrengthNormal), AddNoise(m1, noiseStrengthNormal), cell.Temperature);
            CreateRectangle(t, t, AddNoise(s2m1, noiseStrengthNormal), AddNoise(m1, noiseStrengthNormal), AddNoise(s2m2, noiseStrengthNormal), AddNoise(m2, noiseStrengthNormal), cell.Temperature);
            CreateRectangle(t, t, AddNoise(s2m2, noiseStrengthNormal), AddNoise(m2, noiseStrengthNormal), s2v2, v2, cell.Temperature);
        }

    }

    public void CreateSmoothTriangleWithColor(Vector3 c, Vector3 v1, Vector3 v2, Vector3 m1, Vector3 m2, int t, HexCell cell)
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
            AddTexture(t, t, t, cell.Temperature);
        }

    }

    public void CreateRiverSourceOrEnd(Vector3 m1i, Vector3 m2i, HexCell cell, bool isSmooth, Vector3 c)
    {
        if(!isSmooth)
        {
            m1i = AddNoise(m1i, noiseStrengthNormal);
            m2i = AddNoise(m2i, noiseStrengthNormal);
        }
        else
        {
            m1i = AddNoise(m1i, noiseStrengthSmooth);
            m2i = AddNoise(m2i, noiseStrengthSmooth);
        }

        c = AddNoise(c, noiseStrengthNormal);

        m1i.y += 0.1f;
        m2i.y += 0.1f;
        c.y += 0.1f;

        CreateTriangle(c, m1i, m2i);
        AddTexture(9, 9, 9, cell.Temperature);
    }

    public void CreateHexRiver(Vector3 m1i, Vector3 m2i, HexCell cell, bool isSmooth, Vector3 c)
    {
        if(!isSmooth)
        {
            m1i = AddNoise(m1i, noiseStrengthNormal);
            m2i = AddNoise(m2i, noiseStrengthNormal);
        }
        else
        {
            m1i = AddNoise(m1i, noiseStrengthSmooth);
            m2i = AddNoise(m2i, noiseStrengthSmooth);
        }

        bool isEnd = true;

        for (int i = 0; i < 6; i++)
        {
            if (cell.GetEdge(i).OutRiver)
            {
                isEnd = false;

                Vector3 m1o = cell.GetEdge(i).GetMiddle1();
                Vector3 m2o = cell.GetEdge(i).GetMiddle2();

                if (cell.GetEdge(i).GetEdgeType() != EdgeType.Smooth)
                {
                    m1o = AddNoise(m1o, noiseStrengthNormal);
                    m2o = AddNoise(m2o, noiseStrengthNormal);
                }
                else
                {
                    m1o = AddNoise(m1o, noiseStrengthSmooth);
                    m2o = AddNoise(m2o, noiseStrengthSmooth);
                }

                m1i.y += 0.1f;
                m2i.y += 0.1f;
                m1o.y += 0.1f;
                m2o.y += 0.1f;

                CreateRiverRectangle(9, 9, m1i, m2i, m1o, m2o, cell.Temperature);
            }
        }

        if(isEnd)
        {
            CreateRiverSourceOrEnd(m1i, m2i, cell, isSmooth, c);
        }
    }

    public void CreateTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int index = vertices.Count;

        if (!IsTriangleFacingUp(v1, v2, v3))
        {
            (v2, v3) = (v3, v2);
        }

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
    }

    private bool IsTriangleFacingUp(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1);
        return normal.y > 0f;
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

        if(cell.GetEdge(index).GetEdgeType() == EdgeType.Cliff || cell.GetEdge(index).GetEdgeType() == EdgeType.Mountain)
        {
            t = 1;
            t2 = 1;
        }

        var temp = cell.Temperature;

        if(cell.TerrainLevel > neighbourCell.TerrainLevel)
            temp = cell.Temperature;

        else if(cell.TerrainLevel < neighbourCell.TerrainLevel)
            temp = neighbourCell.Temperature;

        else if (cell.TerrainLevel == neighbourCell.TerrainLevel)
        {
            if(neighbourCell.Temperature < temp)
                temp = neighbourCell.Temperature;
        }

        if (cell.IsMountain && neighbourCell.IsMountain)
        {
            CreateMountainRectangleConnection(t, v1, v2, v1d, v2d, m1, m2, m1d, m2d, cell, neighbourCell);
        }
        else
        {
            CreateRectangle(t, t2, v1, v1d, AddNoise(m1, noiseStrengthNormal), AddNoise(m1d, noiseStrengthNormal), temp);
            CreateRectangle(t, t2, AddNoise(m1, noiseStrengthNormal), AddNoise(m1d, noiseStrengthNormal), AddNoise(m2, noiseStrengthNormal), AddNoise(m2d, noiseStrengthNormal), temp);
            CreateRectangle(t, t2, AddNoise(m2, noiseStrengthNormal), AddNoise(m2d, noiseStrengthNormal), v2, v2d, temp);
        }

        if((cell.GetEdge(index).InRiver && neighbourCell.GetEdge((index + 3) % 6).OutRiver) || (cell.GetEdge(index).OutRiver && neighbourCell.GetEdge((index + 3) % 6).InRiver))
        {
            m1.y += 0.1f;
            m2.y += 0.1f;
            m1d.y += 0.1f;
            m2d.y += 0.1f;

            CreateRectangle(9, 9, AddNoise(m1, noiseStrengthNormal), AddNoise(m1d, noiseStrengthNormal), AddNoise(m2, noiseStrengthNormal), AddNoise(m2d, noiseStrengthNormal), 1);
        }
    }

    private void CreateMountainRectangleConnection(int t, Vector3 v1, Vector3 v2, Vector3 v1d, Vector3 v2d, Vector3 m1, Vector3 m2, Vector3 m1d, Vector3 m2d, HexCell cell, HexCell neighbourCell)
    {
        Vector3 c = new Vector3(
            cell.transform.localPosition.x,
            cell.CentreTerrainLevel,
            cell.transform.localPosition.z
        );

        Vector3 cn = transform.InverseTransformPoint(neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.transform.localPosition));
        cn.y = neighbourCell.CentreTerrainLevel;

        var s2m1 = Vector3.Lerp(c, m1, 2f / 3f);
        var s2m2 = Vector3.Lerp(c, m2, 2f / 3f);

        var s2m1n = Vector3.Lerp(cn, m1d, 2f / 3f);
        var s2m2n = Vector3.Lerp(cn, m2d, 2f / 3f);

        CreateRectangle(t, t, v1, v1d, AddNoise(s2m1, noiseStrengthNormal), AddNoise(s2m1n, noiseStrengthNormal), cell.Temperature);
        CreateRectangle(t, t, AddNoise(s2m2, noiseStrengthNormal), AddNoise(s2m2n, noiseStrengthNormal), v2, v2d, cell.Temperature);

        var chainCentre = Vector3.Lerp(AddNoise(s2m1, noiseStrengthNormal), AddNoise(s2m2n, noiseStrengthNormal), Random.Range(0.1f, 0.9f));
        chainCentre.y += Random.Range(0.5f, 2.3f);

        var m1Chain = Vector3.Lerp(AddNoise(s2m1, noiseStrengthNormal), chainCentre, Random.Range(0.1f, 0.9f));
        var m2Chain = Vector3.Lerp(AddNoise(s2m2, noiseStrengthNormal), chainCentre, Random.Range(0.1f, 0.9f));
        var m1nChain = Vector3.Lerp(AddNoise(s2m1n, noiseStrengthNormal), chainCentre, Random.Range(0.1f, 0.9f));
        var m2nChain = Vector3.Lerp(AddNoise(s2m2n, noiseStrengthNormal), chainCentre, Random.Range(0.1f, 0.9f));

        CreateRectangle(t, t, AddNoise(s2m1, noiseStrengthNormal), AddNoise(s2m1n, noiseStrengthNormal), m1Chain, m1nChain, cell.Temperature);
        CreateRectangle(t, t, m2Chain, m2nChain, AddNoise(s2m2, noiseStrengthNormal), AddNoise(s2m2n, noiseStrengthNormal), cell.Temperature);
        CreateRectangle(t, t, AddNoise(s2m1, noiseStrengthNormal), m1Chain, AddNoise(s2m2, noiseStrengthNormal), m2Chain, cell.Temperature);
        CreateRectangle(t, t, m1nChain, AddNoise(s2m1n, noiseStrengthNormal), m2nChain, AddNoise(s2m2n, noiseStrengthNormal), cell.Temperature);

        chainCentre.y += Random.Range(0.1f, 1.3f);

        CreateTriangle(m1Chain, chainCentre, m2Chain);
        AddTexture(t, t, t, cell.Temperature);
        CreateTriangle(m1nChain, m2nChain, chainCentre);
        AddTexture(t, t, t, cell.Temperature);
        CreateTriangle(chainCentre, m1Chain, m1nChain);
        AddTexture(t, t, t, cell.Temperature);
        CreateTriangle(chainCentre, m2nChain, m2Chain);
        AddTexture(t, t, t, cell.Temperature);

    }

    public void CreateSmoothConnection(Vector3 v1, Vector3 v2, int index, int t, HexCell cell, HexCell neighbourCell, Vector3 m1, Vector3 m2)
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

        var temp = cell.Temperature;

        if (cell.TerrainLevel > neighbourCell.TerrainLevel)
            temp = cell.Temperature;

        else if (cell.TerrainLevel < neighbourCell.TerrainLevel)
            temp = neighbourCell.Temperature;

        else if (cell.TerrainLevel == neighbourCell.TerrainLevel)
        {
            if (neighbourCell.Temperature < temp)
                temp = neighbourCell.Temperature;
        }

        for (int i = 0; i < verticles.Count - 1; i++)
        {
            CreateRectangle(t, neighbourCell.TextureIndex, verticles[i], verticlesD[i], verticles[i + 1], verticlesD[i + 1], temp);
        }

        if ((cell.GetEdge(index).InRiver && neighbourCell.GetEdge((index + 3) % 6).OutRiver) || (cell.GetEdge(index).OutRiver && neighbourCell.GetEdge((index + 3) % 6).InRiver))
        {
            m1.y += 0.1f;
            m2.y += 0.1f;
            m1d.y += 0.1f;
            m2d.y += 0.1f;

            CreateRectangle(9, 9, AddNoise(m1, noiseStrengthSmooth), AddNoise(m1d, noiseStrengthSmooth), AddNoise(m2, noiseStrengthSmooth), AddNoise(m2d, noiseStrengthSmooth), 1);
        }
    }

    private void CreateRectangle(int t, int neighbourT, Vector3 v1, Vector3 v1d, Vector3 v2, Vector3 v2d, float temp)
    { 
        CreateTriangle(v1, v1d, v2); 
        AddTexture(t, neighbourT, t, temp); 

        CreateTriangle(v1d, v2d, v2); 
        AddTexture(neighbourT, neighbourT, t, temp); 
    }

    private void CreateRiverRectangle(int t, int neighbourT, Vector3 v1, Vector3 v1d, Vector3 v2, Vector3 v2d, float temp)
    {
        CreateTriangle(v1, v1d, v2);
        AddTexture(t, neighbourT, t, 1);

        CreateTriangle(v2, v2d, v1);
        AddTexture(t, neighbourT, t, 1);
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

        if (cellEdge == EdgeType.Cliff || cellEdge2 == EdgeType.Cliff || cellEdge == EdgeType.Mountain|| cellEdge2 == EdgeType.Mountain)
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

            if (edge2 == EdgeType.Cliff || edge3 == EdgeType.Cliff || edge2 == EdgeType.Mountain || edge3 == EdgeType.Mountain)
                t2 = 1;

            if (edge4 != EdgeType.Smooth && edge5 != EdgeType.Smooth)
            {
                v1d2 = AddNoise(v1d2, noiseStrengthNormal);               
            }

            if (edge4 == EdgeType.Cliff || edge5 == EdgeType.Cliff || edge4 == EdgeType.Mountain || edge5 == EdgeType.Mountain)
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

            if (edge3 == EdgeType.Cliff || edge4 == EdgeType.Cliff || edge3 == EdgeType.Mountain || edge4 == EdgeType.Mountain)
                t2 = 1;

            if (edge5 == EdgeType.Cliff || edge0 == EdgeType.Cliff || edge5 == EdgeType.Mountain || edge0 == EdgeType.Mountain)
                t3 = 1;
        }

        var temp = cell.Temperature;
        var highestTerrain = cell.TerrainLevel;

        if (neighbourCell.TerrainLevel > highestTerrain)
        {
            highestTerrain = neighbourCell.TerrainLevel;
            temp = neighbourCell.Temperature;
        }
        else if (neighbourCell.TerrainLevel == highestTerrain &&
                 neighbourCell.Temperature < temp)
        {
            temp = neighbourCell.Temperature;
        }

        if (nextNeighbourCell.TerrainLevel > highestTerrain)
        {
            highestTerrain = nextNeighbourCell.TerrainLevel;
            temp = nextNeighbourCell.Temperature;
        }
        else if (nextNeighbourCell.TerrainLevel == highestTerrain &&
                 nextNeighbourCell.Temperature < temp)
        {
            temp = nextNeighbourCell.Temperature;
        }

        CreateTriangle((v1), (v1d1), (v1d2));
        AddTexture(t, t2, t3, temp);
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

    private void AddTexture(int t1, int t2, int t3, float temp)
    {
        colors.Add(new Color(1f, 0f, 0f, temp));
        uv3.Add(new Vector4(t1, t2, t3, 0f));

        colors.Add(new Color(0f, 1f, 0f, temp));
        uv3.Add(new Vector4(t1, t2, t3, 0f));

        colors.Add(new Color(0f, 0f, 1f, temp));
        uv3.Add(new Vector4(t1, t2, t3, 0f));
    }

    private Vector3 AddNoise(Vector3 v, float noiseStrength)
    {
        Vector3 chunkPos = this.GetComponentInParent<Transform>().position;

        Vector4 noise = MapManager.Instance.GetHexMeshNoise().GetPixelBilinear(v.x + chunkPos.x, v.z + chunkPos.z);

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
