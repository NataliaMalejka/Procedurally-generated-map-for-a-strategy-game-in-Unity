using System.Collections.Generic;
using Unity.VisualScripting;
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

    [SerializeField] private WaterMesh waterMesh;
    [SerializeField] private RiverMesh riverMesh;

    private Vector3 chunkPos;
    private int chunkLevel;
    private int waterColorIndex;

    private float maxPos;
    private bool lastColumn = false;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshRenderer>().sharedMaterial = MapManager.Instance.GetTerrainmaterial();
        maxPos = MapManager.Instance.xChunkCount * MapManager.Instance.xCellCount * HexData.distanceToEdge * 2;
    }

    public void SetMeshData(int level, Vector3 pos, int index, bool last)
    {
        chunkLevel = level;
        chunkPos = pos;
        waterColorIndex = index;
        lastColumn = last;
    }


    public void Clear()
    {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        uv3.Clear();
        colors.Clear();

        waterMesh.Clear();
        riverMesh.Clear();
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

        if(cell.IsOcean || cell.IsLake)
        {
           waterMesh.CreateTriangle(AddNoise(c, noiseStrengthNormal), v1, AddNoise(m1, noiseStrengthNormal), cell.IsOcean, chunkLevel, waterColorIndex);
           waterMesh.CreateTriangle(AddNoise(c, noiseStrengthNormal), AddNoise(m1, noiseStrengthNormal), AddNoise(m2, noiseStrengthNormal), cell.IsOcean, chunkLevel, waterColorIndex);
           waterMesh.CreateTriangle(AddNoise(c, noiseStrengthNormal), AddNoise(m2, noiseStrengthNormal), v2, cell.IsOcean, chunkLevel, waterColorIndex);
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

            if (cell.IsOcean || cell.IsLake)
            {
                waterMesh.CreateTriangle(c, verticles[i], verticles[i + 1], cell.IsOcean, chunkLevel, waterColorIndex);
            }
        }
    }

    public void CreateRiverSourceOrEnd(Vector3 m1i, Vector3 m2i, HexCell cell, bool isSmooth, Vector3 c)
    {
        if (!isSmooth)
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

        riverMesh.CreateTriangle(c, m1i, m2i, waterColorIndex);

        riverMesh.AddUV(new Vector2(1, 1));
        riverMesh.AddUV(new Vector2(0, 0));
        riverMesh.AddUV(new Vector2(1, 0));
    }

    public void CreateHexRiver(Vector3 m1i, Vector3 m2i, HexCell cell, bool isSmooth, Vector3 c, int index)
    {
        bool isEnd = true;

        for (int i = 0; i < 6; i++)
        {
            if (cell.GetEdge(i).OutRiver)
            {
                isEnd = false;

                if (!isSmooth)
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

                if ((index + 1) %6 == i || (index + 5) %6 == i || (index + 2) % 6 == i || (index + 4) % 6 == i)
                {
                    List<Vector3> verticlesE = new List<Vector3>();
                    List<Vector3> verticlesC = new List<Vector3>();

                    var betweenEdges = Vector3.Lerp(m2i, m1o, 0.5f);
                    var betweenCentre = Vector3.Lerp(c, betweenEdges, 0.5f);

                    if ((index + 1) % 6 == i || (index + 2) % 6 == i)
                    {
                        verticlesE.Add(m1i);
                        verticlesE.Add(c);
                        verticlesE.Add(m2o);

                        verticlesC.Add(m2i);
                        verticlesC.Add(betweenCentre);
                        verticlesC.Add(m1o);
                    }
                    else
                    {
                        verticlesE.Add(m1i);
                        verticlesE.Add(betweenCentre);
                        verticlesE.Add(m2o);

                        verticlesC.Add(m2i);
                        verticlesC.Add(c);
                        verticlesC.Add(m1o);
                    }
                        
                    verticlesE = ChaikinSmooth(verticlesE);
                    verticlesC = ChaikinSmooth(verticlesC);

                    for (int j = 0; j < verticlesE.Count - 1; j++)
                    {
                        riverMesh.CreateRectangle(verticlesE[j], verticlesC[j], verticlesE[j + 1], verticlesC[j + 1], waterColorIndex);

                        riverMesh.AddUV(new Vector2(1, 1));
                        riverMesh.AddUV(new Vector2(0, 0));
                        riverMesh.AddUV(new Vector2(1, 0));

                        riverMesh.AddUV(new Vector2(0, 1));
                        riverMesh.AddUV(new Vector2(0, 0));
                        riverMesh.AddUV(new Vector2(1, 1));
                    }
                }               
                else
                {
                    riverMesh.CreateRectangle(m2o, m1i, m1o, m2i, waterColorIndex);

                    riverMesh.AddUV(new Vector2(0, 0));
                    riverMesh.AddUV(new Vector2(0, 1));
                    riverMesh.AddUV(new Vector2(1, 0));

                    riverMesh.AddUV(new Vector2(0, 1));
                    riverMesh.AddUV(new Vector2(1, 1));
                    riverMesh.AddUV(new Vector2(1, 0));
                }
            }
        }

        if (isEnd)
        {
            //CreateRiverSourceOrEnd(m1i, m2i, cell, isSmooth, c);
        }
    }

    private void CreateTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
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

        bool endmap = false;

        if (v1d.x < v1.x &&lastColumn)
        {
            var newX = (v1.x - v1d.x) - maxPos;
            v1d = new Vector3(v1.x - newX, v1d.y, v1d.z);

            endmap = true;
        }
        if (v2d.x < v2.x && lastColumn)
        {
            var newX = (v2.x - v2d.x) - maxPos;
            v2d = new Vector3(v2.x - newX, v2d.y, v2d.z);

            endmap = true;
        }
        if (m1d.x < m1.x && lastColumn)
        {
            var newX = (m1.x - m1d.x) - maxPos;
            m1d = new Vector3(m1.x - newX, m1d.y, m1d.z);

            endmap = true;
        }
        if (m2d.x < m2.x && lastColumn)
        {
            var newX = (m2.x - m2d.x) - maxPos;
            m2d = new Vector3(m2.x - newX, m2d.y, m2d.z);

            endmap = true;
        }

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
                v1d = AddNoise(v1d, noiseStrengthNormal, endmap);

            if ((neighbourCell.GetEdge((index + 2) % 6).GetEdgeType() != EdgeType.Smooth))
                v2d = AddNoise(v2d, noiseStrengthNormal, endmap);
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
            CreateRectangle(t, t2, v1, v1d, AddNoise(m1, noiseStrengthNormal), AddNoise(m1d, noiseStrengthNormal, endmap), temp);
            CreateRectangle(t, t2, AddNoise(m1, noiseStrengthNormal), AddNoise(m1d, noiseStrengthNormal, endmap), AddNoise(m2, noiseStrengthNormal), AddNoise(m2d, noiseStrengthNormal, endmap), temp);
            CreateRectangle(t, t2, AddNoise(m2, noiseStrengthNormal), AddNoise(m2d, noiseStrengthNormal, endmap), v2, v2d, temp);
        }

        if((cell.GetEdge(index).InRiver && neighbourCell.GetEdge((index + 3) % 6).OutRiver) || (cell.GetEdge(index).OutRiver && neighbourCell.GetEdge((index + 3) % 6).InRiver))
        {
            foreach(var river in cell.GetRivers())
            {
                if (river.AreNeighboursInRiver(cell, neighbourCell))
                {
                    m1.y += 0.1f;
                    m2.y += 0.1f;
                    m1d.y += 0.1f;
                    m2d.y += 0.1f;

                    riverMesh.CreateRectangle(AddNoise(m1, noiseStrengthNormal), AddNoise(m1d, noiseStrengthNormal), AddNoise(m2, noiseStrengthNormal), AddNoise(m2d, noiseStrengthNormal), waterColorIndex);

                    if (cell.GetEdge(index).InRiver)
                    {
                        riverMesh.AddUV(new Vector2(1, 0));
                        riverMesh.AddUV(new Vector2(1, 1));
                        riverMesh.AddUV(new Vector2(0, 0));

                        riverMesh.AddUV(new Vector2(1, 1));
                        riverMesh.AddUV(new Vector2(0, 1));
                        riverMesh.AddUV(new Vector2(0, 0));
                    }
                    else
                    {
                        riverMesh.AddUV(new Vector2(1, 1));
                        riverMesh.AddUV(new Vector2(1, 0));
                        riverMesh.AddUV(new Vector2(0, 1));

                        riverMesh.AddUV(new Vector2(1, 0));
                        riverMesh.AddUV(new Vector2(0, 0));
                        riverMesh.AddUV(new Vector2(0, 1));
                    }
                }
            }        
        }

        if (cell.IsOcean || cell.IsLake || neighbourCell.IsOcean || neighbourCell.IsLake)
        {
            bool isOcean = cell.IsOcean || neighbourCell.IsOcean;

            m1 = AddNoise(m1, noiseStrengthNormal);
            m2 = AddNoise(m2, noiseStrengthNormal);
            m1d = AddNoise(m1d, noiseStrengthNormal, endmap);
            m2d = AddNoise(m2d, noiseStrengthNormal, endmap);

            var coastV1 = v1;
            var coastV2 = v2;

            var coastV1d = v1d;
            var coastV2d = v2d;

            var coastM1 = m1;
            var coastM2 = m2;

            var coastM1d = m1d;
            var coastM2d = m2d;

            if (cell.IsOcean || cell.IsLake)
            {
                var scale = 1/Mathf.Abs(v1.y - v1d.y);
                scale += 0.5f;
                coastV1d = Vector3.Lerp(v1, v1d, scale);
                coastV2d = Vector3.Lerp(v2, v2d, scale);

                coastM1d = Vector3.Lerp(m1, m1d, scale);
                coastM2d = Vector3.Lerp(m2, m2d, scale);

                coastV1d.y = coastV1.y;
                coastV2d.y = coastV2.y;
                coastM1d.y = coastM1.y;
                coastM2d.y = coastM2.y;
            }
            else
            {
                var scale = 1/Mathf.Abs(v1d.y - v1.y);
                scale += 0.5f;

                coastV1 = Vector3.Lerp(v1d, v1, scale);
                coastV2 = Vector3.Lerp(v2d, v2, scale);

                coastM1 = Vector3.Lerp(m1d, m1, scale);
                coastM2 = Vector3.Lerp(m2d, m2, scale);

                coastV1.y = coastV1d.y;
                coastV2.y = coastV2d.y;
                coastM1.y = coastM1d.y;
                coastM2.y = coastM2d.y;
            }

            waterMesh.CreateRectangle(coastV1, coastV1d, coastM1, coastM1d, isOcean, chunkLevel, waterColorIndex);
            waterMesh.CreateRectangle(coastM1 , coastM1d ,coastM2,  coastM2d, isOcean, chunkLevel, waterColorIndex);
            waterMesh.CreateRectangle(coastM2, coastM2d, coastV2, coastV2d, isOcean, chunkLevel, waterColorIndex);
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

        bool isOcean = cell.IsOcean && neighbourCell.IsOcean;

        for (int i = 0; i < verticles.Count - 1; i++)
        {
            if (cell.IsOcean || cell.IsLake || neighbourCell.IsOcean || neighbourCell.IsLake)
            {
                var ov1 = verticles[i];
                var ov2 = verticles[i + 1];
                var ov1d = verticlesD[i];
                var ov2d = verticlesD[i + 1];

                var coastO1 = ov1;
                var coastO2 = ov2;

                var coastO1d = ov1d;
                var coastO2d = ov2d;

                if (cell.IsOcean || cell.IsLake)
                {
                    coastO1d.y = ov1.y;
                    coastO2d.y = ov2.y;
                }
                else
                {
                    coastO1.y = ov1d.y;
                    coastO2.y = ov2d.y;
                }

                waterMesh.CreateRectangle(coastO1, coastO1d, coastO2, coastO2d, isOcean, chunkLevel, waterColorIndex);
            }

            CreateRectangle(t, neighbourCell.TextureIndex, verticles[i], verticlesD[i], verticles[i + 1], verticlesD[i + 1], temp);
        }

        if ((cell.GetEdge(index).InRiver && neighbourCell.GetEdge((index + 3) % 6).OutRiver) || (cell.GetEdge(index).OutRiver && neighbourCell.GetEdge((index + 3) % 6).InRiver))
        {
            foreach (var river in cell.GetRivers())
            {
                if (river.AreNeighboursInRiver(cell, neighbourCell))
                {
                    m1.y += 0.1f;
                    m2.y += 0.1f;
                    m1d.y += 0.1f;
                    m2d.y += 0.1f;

                    riverMesh.CreateRectangle(AddNoise(m1, noiseStrengthSmooth), AddNoise(m1d, noiseStrengthSmooth), AddNoise(m2, noiseStrengthSmooth), AddNoise(m2d, noiseStrengthSmooth), waterColorIndex);

                    if (cell.GetEdge(index).InRiver)
                    {
                        riverMesh.AddUV(new Vector2(1, 0));
                        riverMesh.AddUV(new Vector2(1, 1));
                        riverMesh.AddUV(new Vector2(0, 0));

                        riverMesh.AddUV(new Vector2(1, 1));
                        riverMesh.AddUV(new Vector2(0, 1));
                        riverMesh.AddUV(new Vector2(0, 0));
                    }
                    else
                    {
                        riverMesh.AddUV(new Vector2(1, 1));
                        riverMesh.AddUV(new Vector2(1, 0));
                        riverMesh.AddUV(new Vector2(0, 1));

                        riverMesh.AddUV(new Vector2(1, 0));
                        riverMesh.AddUV(new Vector2(0, 0));
                        riverMesh.AddUV(new Vector2(0, 1));
                    }
                }
            }                    
        }
    }

    private void CreateRectangle(int t, int neighbourT, Vector3 v1, Vector3 v1d, Vector3 v2, Vector3 v2d, float temp)
    { 
        CreateTriangle(v1, v1d, v2); 
        AddTexture(t, neighbourT, t, temp); 

        CreateTriangle(v1d, v2d, v2); 
        AddTexture(neighbourT, neighbourT, t, temp); 
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

        bool endmap = false;
        bool doubleEndmap = false;

        if (index == 0) 
        {
            v1d1 = neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.GetEdge(3).GetLocalV1());
            v1d2 = nextNeighbourCell.HexChunk.transform.TransformPoint(nextNeighbourCell.GetEdge(5).GetLocalV1());

            v1d1 = transform.InverseTransformPoint(v1d1);
            v1d2 = transform.InverseTransformPoint(v1d2);

            if (v1d2.x < v1.x && lastColumn)
            {
                var newX2 = (v1.x - v1d2.x) - maxPos;
                v1d2 = new Vector3(v1.x - newX2, v1d2.y, v1d2.z);

                if (v1d1.x < v1.x)
                {
                    var newX = (v1.x - v1d1.x) - maxPos;
                    v1d1 = new Vector3(v1.x - newX, v1d1.y, v1d1.z);

                    doubleEndmap = true;
                }

                endmap = true;
            }

            var edge2 = neighbourCell.GetEdge(2).GetEdgeType();
            var edge3 = neighbourCell.GetEdge(3).GetEdgeType();
            var edge4 = nextNeighbourCell.GetEdge(4).GetEdgeType();
            var edge5 = nextNeighbourCell.GetEdge(5).GetEdgeType();

            if (edge2 != EdgeType.Smooth && edge3 != EdgeType.Smooth)
            {
                v1d1 = AddNoise(v1d1, noiseStrengthNormal, doubleEndmap);
            }

            if (edge4 != EdgeType.Smooth && edge5 != EdgeType.Smooth)
            {
                v1d2 = AddNoise(v1d2, noiseStrengthNormal, endmap);               
            }

            if (edge2 == EdgeType.Cliff || edge3 == EdgeType.Cliff || edge2 == EdgeType.Mountain || edge3 == EdgeType.Mountain)
                t2 = 1;


            if (edge4 == EdgeType.Cliff || edge5 == EdgeType.Cliff || edge4 == EdgeType.Mountain || edge5 == EdgeType.Mountain)
                t3 = 1;
        }
        else 
        {
            v1d1 = neighbourCell.HexChunk.transform.TransformPoint(neighbourCell.GetEdge(3).GetLocalV2());
            v1d2 = nextNeighbourCell.HexChunk.transform.TransformPoint(nextNeighbourCell.GetEdge(5).GetLocalV2());

            v1d1 = transform.InverseTransformPoint(v1d1);
            v1d2 = transform.InverseTransformPoint(v1d2);

            if (v1d1.x < v1.x && lastColumn)
            {
                var newX = (v1.x - v1d1.x) - maxPos;
                v1d1 = new Vector3(v1.x - newX, v1d1.y, v1d1.z);

                if (v1d2.x < v1.x)
                {
                    var newX2 = (v1.x - v1d2.x) - maxPos;
                    v1d2 = new Vector3(v1.x - newX2, v1d2.y, v1d2.z);

                    doubleEndmap = true;
                }

                endmap = true;
            }

            var edge3 = neighbourCell.GetEdge(3).GetEdgeType();
            var edge4 = neighbourCell.GetEdge(4).GetEdgeType();
            var edge5 = nextNeighbourCell.GetEdge(5).GetEdgeType();
            var edge0 = nextNeighbourCell.GetEdge(0).GetEdgeType();

            if (edge3 != EdgeType.Smooth && edge4 != EdgeType.Smooth)
            {
                v1d1 = AddNoise(v1d1, noiseStrengthNormal, endmap);        
            }

            if (edge5 != EdgeType.Smooth && edge0 != EdgeType.Smooth)
            {
                v1d2 = AddNoise(v1d2, noiseStrengthNormal, doubleEndmap);
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

        CreateTriangle(v1, v1d1, v1d2);
        AddTexture(t, t2, t3, temp);

        if(cell.IsOcean || cell.IsLake || neighbourCell.IsOcean || neighbourCell.IsLake || nextNeighbourCell.IsOcean || nextNeighbourCell.IsLake)
        {
            CreateTriangleWaterConnection(v1, v1d1, v1d2, cell, neighbourCell, nextNeighbourCell);
        }
    }

    private void CreateTriangleWaterConnection(Vector3 v1, Vector3 v1d1, Vector3 v1d2, HexCell cell, HexCell neighbourCell, HexCell nextNeighbourCell)
    {
        var coastV1 = v1;
        var coastV1d1 = v1d1;
        var coastV1d2 = v1d2;

        if (cell.IsOcean || cell.IsLake)
        {
            if (!neighbourCell.IsOcean && !neighbourCell.IsLake && !nextNeighbourCell.IsOcean && !nextNeighbourCell.IsLake)
            {
                var scale = 1 / Mathf.Abs(v1.y - v1d1.y);
                scale += 0.5f;
                coastV1d1 = Vector3.Lerp(v1, v1d1, scale);

                var scale2 = 1 / Mathf.Abs(v1.y - v1d2.y);
                scale2 += 0.5f;
                coastV1d2 = Vector3.Lerp(v1, v1d2, scale2);

                coastV1d1.y = coastV1.y;
                coastV1d2.y = coastV1.y;

                waterMesh.CreateTriangle(v1, coastV1d1, coastV1d2, cell.IsOcean, chunkLevel, waterColorIndex);
            }
            else if (!neighbourCell.IsOcean && !neighbourCell.IsLake && (nextNeighbourCell.IsOcean || nextNeighbourCell.IsLake))
            {
                bool isOcean = cell.IsOcean || nextNeighbourCell.IsOcean;

                var scale = 1 / Mathf.Abs(v1.y - v1d1.y);
                scale += 0.5f;
                coastV1d1 = Vector3.Lerp(v1, v1d1, scale);

                var scale2 = 1 / Mathf.Abs(v1d2.y - v1d1.y);
                scale2 += 0.5f;
                coastV1d2 = Vector3.Lerp(v1d2, v1d1, scale2);

                coastV1d1.y = coastV1.y;
                coastV1d2.y = coastV1.y;

                waterMesh.CreateTriangle(v1, coastV1d1, v1d2, isOcean, chunkLevel, waterColorIndex);
                waterMesh.CreateTriangle(v1d2, coastV1d1, coastV1d2, isOcean, chunkLevel, waterColorIndex);
            }
            else if ((neighbourCell.IsOcean || neighbourCell.IsLake) && !nextNeighbourCell.IsOcean && !nextNeighbourCell.IsLake)
            {
                bool isOcean = cell.IsOcean || neighbourCell.IsOcean;

                var scale = 1 / Mathf.Abs(v1d1.y - v1d2.y);
                scale += 0.5f;
                coastV1d1 = Vector3.Lerp(v1d1, v1d2, scale);

                var scale2 = 1 / Mathf.Abs(v1.y - v1d2.y);
                scale2 += 0.5f;
                coastV1d2 = Vector3.Lerp(v1, v1d2, scale2);

                coastV1d1.y = coastV1.y;
                coastV1d2.y = coastV1.y;

                waterMesh.CreateTriangle(v1, v1d1, coastV1d2, isOcean, chunkLevel, waterColorIndex);
                waterMesh.CreateTriangle(coastV1d2, v1d1, coastV1d1, isOcean, chunkLevel, waterColorIndex);

            }
            else
            {
                bool isOcean = cell.IsOcean || neighbourCell.IsOcean || nextNeighbourCell.IsOcean;
                waterMesh.CreateTriangle(v1, v1d1, v1d2,isOcean, chunkLevel, waterColorIndex);
            }
        }
        else if (neighbourCell.IsOcean || neighbourCell.IsLake)
        {
            if(!nextNeighbourCell.IsOcean && !nextNeighbourCell.IsLake)
            {
                var scale = 1 / Mathf.Abs(v1d1.y - v1.y);
                scale += 0.5f;
                coastV1 = Vector3.Lerp(v1d1, v1, scale);

                var scale2 = 1 / Mathf.Abs(v1d1.y - v1d2.y);
                scale2 += 0.5f;
                coastV1d2 = Vector3.Lerp(v1d1, v1d2, scale2);

                coastV1.y = coastV1d1.y;
                coastV1d2.y = coastV1d1.y;

                waterMesh.CreateTriangle(coastV1, v1d1, coastV1d2, neighbourCell.IsOcean, chunkLevel, waterColorIndex);
            }
            else
            {
                bool isOcean = neighbourCell.IsOcean || neighbourCell.IsOcean; 

                var scale = 1 / Mathf.Abs(v1d1.y - v1.y);
                scale += 0.5f;
                coastV1 = Vector3.Lerp(v1d1, v1, scale);

                var scale2 = 1 / Mathf.Abs(v1d2.y - v1.y);
                scale2 += 0.5f;
                coastV1d2 = Vector3.Lerp(v1d2, v1, scale2);

                coastV1.y = coastV1d1.y;
                coastV1d2.y = coastV1d1.y;

                waterMesh.CreateTriangle(coastV1, v1d1, v1d2, isOcean, chunkLevel, waterColorIndex);
                waterMesh.CreateTriangle(coastV1, v1d2, coastV1d2, isOcean, chunkLevel, waterColorIndex);
            }

        }
        else if (nextNeighbourCell.IsOcean || nextNeighbourCell.IsLake)
        {
            var scale = 1 / Mathf.Abs(v1d2.y - v1.y);
            scale += 0.5f;
            coastV1 = Vector3.Lerp(v1d2, v1, scale);

            var scale2 = 1 / Mathf.Abs(v1d2.y - v1d1.y);
            scale2 += 0.5f;
            coastV1d1 = Vector3.Lerp(v1d2, v1d1, scale2);

            coastV1.y = coastV1d2.y;
            coastV1d1.y = coastV1d2.y;

            waterMesh.CreateTriangle(coastV1, coastV1d1, v1d2, nextNeighbourCell.IsOcean, chunkLevel, waterColorIndex);
        }
    }

    private List<Vector3> ChaikinSmooth(List<Vector3> verticles)
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
        uv3.Add(new Vector4(t1, t2, t3, chunkLevel));

        colors.Add(new Color(0f, 1f, 0f, temp));
        uv3.Add(new Vector4(t1, t2, t3, chunkLevel));

        colors.Add(new Color(0f, 0f, 1f, temp));
        uv3.Add(new Vector4(t1, t2, t3, chunkLevel));
    }

    public Vector3 AddNoise(Vector3 v, float noiseStrength, bool endMap = false)
    {
        Vector4 noise;

        if (endMap)
        {
            noise = MapManager.Instance.GetHexMeshNoise().GetPixelBilinear(v.x - HexData.distanceToEdge * 2 * MapManager.Instance.xCellCount, v.z);
        }
        else
            noise = MapManager.Instance.GetHexMeshNoise().GetPixelBilinear(v.x + chunkPos.x, v.z + chunkPos.z);

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

        waterMesh.Apply();
        riverMesh.Apply();
    }
}
