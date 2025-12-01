using System;
using System.Collections.Generic;
using UnityEngine;

enum MapSize
{
    Small,
    Medium,
    Large
}

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [SerializeField] private MapSize mapSize;
    [SerializeField] private int seed;

    public int xChunkCount { get; private set; }
    public int zChunkCount { get; private set; }

    public int xCellCount { get; private set; } = 6;
    public int zCellCount { get; private set; } = 6;

    public int minXMargin { get; private set; } = 5;
    public int minZMargin { get; private set; } = 4;
    public int maxXMargin { get; private set; }
    public int maxZMargin { get; private set; }

    private int regionsCount;

    private Continent[] continents;
    private int continentsCount;

    private int[] continentsCentres;
    private int contonentCentreMargin = 7;

    private int[] maxContinentsCellsAmound;

    private Vector2[] continentDir;

    [Header("Continent Shape")]
    [SerializeField] private float perlinScale = 0.15f;
    [SerializeField] private float distWeight = 0.3f;
    [SerializeField] private float perlinWeight = 0.5f;
    [SerializeField] private float dirWeight = 0.3f;
    [SerializeField] private float minScore = 0.7f;
    [SerializeField] private float minCoastNoise = 0.8f;
    [SerializeField] private float maxCoastNoise = 1.2f;
    [SerializeField] private float growthBonusFactor = 2.5f;

    [Header("Terrain Level")]
    [SerializeField] private float distanceWeight = 0.3f;

    [SerializeField] private float perlinLargeTerrainScale = 0.1f;
    [SerializeField] private float perlinMediumTerrainScale = 0.3f;
    [SerializeField] private float perlinSmallTerrainScale = 0.28f;

    [SerializeField] private float distanceNoiseTerrainAmp = 1f;
    [SerializeField] private float hillsNoiseTerrainAmp = 2f;
    [SerializeField] private float mountainsNoiseTerrainAmp = 10f;

    [SerializeField] private float hillsNoiseMargin = 0.6f;
    [SerializeField] private float mountainsNoiseMargin = 0.8f;

    [Header("Hex Noise")]
    public Texture2D hexMeshNoise;

    private List<Chunk> chunksToRefresh = new List<Chunk>();

    List<Edge> allSmoothEdges = new List<Edge>();
    List<List<Edge>> groupsSmoothEdges = new List<List<Edge>>();

    public int[] GetContinentsCentres()
    {
        return continentsCentres;
    }

    public List<Chunk> GetChunksToRefresh()
    {
        return chunksToRefresh;
    }

    private void Awake()
    {
        Instance = this;

        UnityEngine.Random.InitState(seed);

        (xChunkCount, zChunkCount) = SetChunkCounts(mapSize);

        continentsCount = SetContinentsCount(mapSize);

        continents = new Continent[continentsCount];

        SetMaxMargin();

        SetContinentCellsAmound();

        SetContinentsInRegions();
    }

    private (int x, int z) SetChunkCounts(MapSize size)//1,3 przelicznik
    {
        return size switch
        {
            MapSize.Small => (15, 10),//10 6     //5 3
            MapSize.Medium => (21, 14),//15 9     //10 6
            MapSize.Large => (27, 18),//20 12       //15 9
            _ => (21, 14)
        };
    }

    private int SetContinentsCount(MapSize size)
    {
        return size switch
        {
            MapSize.Small => (UnityEngine.Random.Range(2, 4)),
            MapSize.Medium => (UnityEngine.Random.Range(3, 6)),
            MapSize.Large => (UnityEngine.Random.Range(4, 7)),
            _ => (UnityEngine.Random.Range(2, 4))
        };
    }

    private void SetMaxMargin()
    {
        minXMargin *= (int)mapSize + 1;
        minZMargin *= (int)mapSize + 1;

        maxXMargin = xCellCount * xChunkCount - minXMargin;
        maxZMargin = zCellCount * zChunkCount - minZMargin;

        regionsCount = continentsCount / 2 + continentsCount % 2;
        if (regionsCount < 2) regionsCount = 2;

        continentsCentres = new int[continentsCount];
        continentDir = new Vector2[continentsCount];
    }

    private void SetContinentCellsAmound()
    {
        maxContinentsCellsAmound = new int[continentsCount];

        int actualCellsX = xCellCount * xChunkCount - minXMargin * 2;
        int actualCellsZ = zCellCount * zChunkCount - minZMargin * 2;

        int actualCells = actualCellsX * actualCellsZ;

        for (int i = 0; i < continentsCount; i++)
        {
            maxContinentsCellsAmound[i] = UnityEngine.Random.Range((actualCells / 100 * 50) / continentsCount, (actualCells / 100 * 60) / continentsCount);
        }
    }

    private void SetContinentsInRegions()
    {
        int localXMin = minXMargin + contonentCentreMargin;
        int localXMax = xChunkCount * xCellCount / regionsCount - minXMargin - contonentCentreMargin;

        int localZMin = minZMargin;
        int localZMax = zChunkCount * zCellCount / 2 - minZMargin;

        int regionIndex = 2;

        int dirX = 0;
        int dirZ = 0;

        for (int i = 0; i < continentsCount; i++)
        {
            dirX = 1;
            dirZ = 1;

            if (i != 0)
            {
                if (i % 2 == 1)
                {
                    localZMin = localZMax + minZMargin * 2 + contonentCentreMargin;
                    localZMax = maxZMargin - contonentCentreMargin;
                    dirZ = -1;

                }
                if (i % 2 == 0 || continentsCount == 2)
                {
                    localZMin = minZMargin + contonentCentreMargin;
                    localZMax = zChunkCount * zCellCount / 2 - minZMargin - contonentCentreMargin;
                    dirZ = 1;

                    localXMin = localXMax + minXMargin * 2 + contonentCentreMargin;
                    localXMax = xChunkCount * xCellCount / regionsCount * regionIndex - minXMargin - contonentCentreMargin;

                    if (localXMax == maxXMargin - contonentCentreMargin) dirX = -1;
                    else dirX = 0;

                    regionIndex++;
                }
            }
            //usunac dir na prawo/lewo

            continentDir[i] = new Vector2(dirX, dirZ);
            SetContinentsCentres(localXMin, localXMax, localZMin, localZMax, i);
        }
    }

    private void SetContinentsCentres(int localXMin, int localXMax, int localZMin, int localZMax, int index)
    {
        int xPos = UnityEngine.Random.Range(localXMin, localXMax);
        int zPos = UnityEngine.Random.Range(localZMin, localZMax);

        continentsCentres[index] = GetCellIndex(xPos, zPos);
    }

    public int GetCellIndex(int xPos, int zPos)
    {
        int chunkX = xPos / xCellCount;
        int chunkZ = zPos / zCellCount;

        int chunkIndex = chunkX * zChunkCount + chunkZ;

        int localX = (xPos - chunkX * xCellCount);
        int localZ = (zPos - chunkZ * zCellCount);

        int localCellIndex = localX * zCellCount + localZ;

        return chunkIndex * (xCellCount * zCellCount) + localCellIndex;
    }

    public void GenerateContinents(HexCell[] gridCells)
    {
        chunksToRefresh.Clear();

        for (int i = 0; i < continentsCentres.Length; i++)
        {
            continents[i] = new Continent();
            continents[i].ContinetIndex = i;

            int cellsCreated = 0;
            int centreIndex = continentsCentres[i];

            HexCell centreCell = gridCells[centreIndex];
            Queue<HexCell> cellsToCheck = new Queue<HexCell>();

            SetContinentPart(centreCell, i);
            cellsToCheck.Enqueue(centreCell);
            cellsCreated++;

            cellsCreated += AddCloseNeighborCells(centreCell, i, cellsToCheck);

            while (cellsToCheck.Count > 0 && cellsCreated < maxContinentsCellsAmound[Array.IndexOf(continentsCentres, centreIndex)])
            {
                HexCell currentCell = cellsToCheck.Dequeue();

                for (int j = 0; j < 6; j++)
                {
                    HexCell neighborCell = currentCell.GetNeighbor((HexDirection)j);

                    if (neighborCell == null) continue;

                    if (neighborCell.ContinentIndex != -1) continue;

                    if (IsBehindBorders(neighborCell)) continue;

                    if (AdjacentToOtherContinent(neighborCell, i)) continue;

                    if (CheckNoise(neighborCell, centreCell, i)) continue;

                    SetContinentPart(neighborCell, i);
                    cellsToCheck.Enqueue(neighborCell);

                    cellsCreated++;
                }
            }
        }

        SetOceans(gridCells);

        for (int i = 0; i < continentsCount; i++)
        {
            RemoveLakes(i);
        }

        CalculateDistancToOcean(gridCells);

        CalculateTerrainLevel(gridCells);

        DetectEdgeType(gridCells);
    }

    private void SetContinentPart(HexCell cell, int continentIndex)
    {
        //CalculateTerrainLevel(cell);

        cell.SetContinent(continentIndex);
        continents[continentIndex].AddCell(cell);
        //cellsToCheck.Enqueue(cell);

        Chunk chunk = cell.HexChunk;
        //int chunkIndex = cell.HexChunk.GetIndexInGrid();

        AddChunkToRefreshList(chunk);
        continents[continentIndex].AddChunk(cell.HexChunk);
        //ChcekNeighbourChunks(cell, chunkIndex);
    }

    //private void ChcekNeighbourChunks(HexCell cell, int chunkIndex)
    //{
    //    if (cell.Coordinates.LocalX == 0 && cell.Coordinates.GlobalX != 0)
    //    {
    //        AddChunkToRefreshList(chunkIndex - zChunkCount);
    //    }
    //    else if (cell.Coordinates.LocalX == xCellCount - 1 && cell.Coordinates.globalX < xChunkCount * xCellCount - 1)
    //    {
    //        AddChunkToRefreshList(chunkIndex + zChunkCount);
    //    }

    //    if (cell.Coordinates.LocalZ == 0 && cell.Coordinates.globalZ != 0)
    //    {
    //        AddChunkToRefreshList(chunkIndex - 1);
    //    }
    //    else if (cell.Coordinates.LocalZ == zCellCount - 1 && cell.Coordinates.globalZ < zChunkCount * zCellCount - 1)
    //    {
    //        AddChunkToRefreshList(chunkIndex + 1);
    //    }
    //}

    private void AddChunkToRefreshList(Chunk chunk)
    {
        if (!chunksToRefresh.Contains(chunk))
            chunksToRefresh.Add(chunk);
    }

    private int AddCloseNeighborCells(HexCell centreCell, int continentIndex, Queue<HexCell> cellsToCheck)
    {
        int addedCells = 0;

        for (int i = 0; i < 6; i++)
        {
            HexCell closeNeighborCell = centreCell.GetNeighbor((HexDirection)i);

            if (closeNeighborCell != null && !IsBehindBorders(closeNeighborCell))
            {
                SetContinentPart(closeNeighborCell, continentIndex);
                cellsToCheck.Enqueue(closeNeighborCell);

                addedCells++;
            }
        }

        return addedCells;
    }

    private bool IsBehindBorders(HexCell cell)
    {
        if (cell.Coordinates.globalX < minXMargin || cell.Coordinates.globalX >= maxXMargin ||
            cell.Coordinates.globalZ < minZMargin || cell.Coordinates.globalZ >= maxZMargin)
        {
            return true;
        }
        else
            return false;
    }

    private bool AdjacentToOtherContinent(HexCell neighborCell, int currentContinentIndex)
    {
        bool adjacentToOtherContinent = false;
        for (int i = 0; i < 6; i++)
        {
            HexCell adjacentCell = neighborCell.GetNeighbor((HexDirection)i);

            if (adjacentCell == null) continue;

            if (adjacentCell.ContinentIndex != -1 && adjacentCell.ContinentIndex != currentContinentIndex)
            {
                adjacentToOtherContinent = true;
                break;
            }
        }

        return adjacentToOtherContinent;
    }

    private bool CheckNoise(HexCell neighborCell, HexCell centreCell, int index)
    {
        float dist = Vector2.Distance(neighborCell.Coordinates.GetCellPos(), centreCell.Coordinates.GetCellPos());
        float distFactor = Mathf.Clamp01(1f - dist / (zCellCount * zChunkCount / 2));

        float noiseFactor = Mathf.PerlinNoise(
        neighborCell.Coordinates.globalX * perlinScale,
        neighborCell.Coordinates.globalZ * perlinScale
        );

        Vector2 toHex = (neighborCell.Coordinates.GetCellPos() - centreCell.Coordinates.GetCellPos()).normalized;
        float dirFactor = Vector2.Dot(toHex, continentDir[index].normalized) * 0.5f + 0.5f;

        float coastNoise = UnityEngine.Random.Range(minCoastNoise, maxCoastNoise);

        float cellsFill = (float)continents[index].GetContinentCells().Count / maxContinentsCellsAmound[index];
        float growthBonus = 1f + growthBonusFactor * (1f - cellsFill * cellsFill);

        float score =
            distFactor * distWeight +
            noiseFactor * perlinWeight +
            dirFactor * dirWeight;

        return score * coastNoise * growthBonus < minScore;
    }

    //private void CalculateTerrainLevel(HexCell neighborCell)
    //{
    //    float noiseFactor = Mathf.PerlinNoise(
    //    neighborCell.Coordinates.globalX * perlinMediumTerrainScale,
    //    neighborCell.Coordinates.globalZ * perlinMediumTerrainScale
    //    );

    //    neighborCell.TerreinLevel = (int)Mathf.Lerp(0, 5, noiseFactor);
    //}

    private void SetOceans(HexCell[] gridCells)
    {
        var cellsToCheck = new Queue<HexCell>();

        HexCell startCell = gridCells[0];
        startCell.isOcean = true;
        cellsToCheck.Enqueue(startCell);

        while (cellsToCheck.Count > 0)
        {
            HexCell currentCell = cellsToCheck.Dequeue();

            for (int j = 0; j < 6; j++)
            {
                HexCell neighborCell = currentCell.GetNeighbor((HexDirection)j);
                if (neighborCell == null) continue;

                if (neighborCell.ContinentIndex != -1) continue;

                if (neighborCell.isOcean) continue;

                neighborCell.isOcean = true;
                neighborCell.DistanceFromOcean = 0;

                cellsToCheck.Enqueue(neighborCell);
            }
        }
    }

    private void RemoveLakes(int continentIndex)
    {
        var chunkList = continents[continentIndex].GetContinentChunks();

        foreach (var chunk in chunkList)
        {
            foreach (var cell in chunk.GetCells())
            {
                if (cell.TerreinLevel == -1 && !cell.isOcean)
                    SetContinentPart(cell, continentIndex);
            }
        }
    }

    private void CalculateDistancToOcean(HexCell[] gridCells)
    {
        Queue<HexCell> queue = new Queue<HexCell>();

        for (int i = 0; i < gridCells.Length; i++)
        {
            HexCell cell = gridCells[i];

            if (cell.isOcean)
                queue.Enqueue(cell);
        }

        while (queue.Count > 0)
        {
            HexCell current = queue.Dequeue();
            int currentDist = current.DistanceFromOcean;

            for (int i = 0; i < 6; i++)
            {
                HexCell neighbor = current.GetNeighbor((HexDirection)i);
                if (neighbor == null)
                    continue;

                if (neighbor.DistanceFromOcean == -1)
                {
                    neighbor.DistanceFromOcean = currentDist + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private void CalculateTerrainLevel(HexCell[] gridCells)
    {
        foreach (var cell in gridCells)
        {
            if (!cell.isOcean)
            {
                int heightDistanceToOcean = CalculateDictanceToOceanHeight(cell) * HexData.oceanDistanceLevelStep;

                int distanceNoiseHeight = SetDistanceNoise(cell);
                int terrainLevel = distanceNoiseHeight + heightDistanceToOcean;

                if (terrainLevel > 5)
                    terrainLevel = 5;

                if (cell.DistanceFromOcean == 1)
                {
                    if (terrainLevel > 0)
                        terrainLevel -= 1;
                }
                else
                {
                    int hillHeight = SetHillNoise(cell);
                    terrainLevel += hillHeight;
                }

                int mountainsNoise = SetMountains(cell);
                terrainLevel += mountainsNoise;

                cell.SetTerrainLevel(terrainLevel);
            }
        }
    }

    private int CalculateDictanceToOceanHeight(HexCell cell)
    {
        return (int)(cell.DistanceFromOcean * distanceWeight);
    }

    private int SetDistanceNoise(HexCell cell)
    {
        float distanceNoise = Mathf.PerlinNoise(
            cell.Coordinates.globalX * perlinLargeTerrainScale + 1000f,
            cell.Coordinates.globalZ * perlinLargeTerrainScale + 1000f
        );

        distanceNoise *= distanceNoiseTerrainAmp * HexData.oceanDistanceLevelStep;

        return (int)(distanceNoise);
    }

    private int SetHillNoise(HexCell cell)
    {
        float hillsNoise = Mathf.PerlinNoise(
            cell.Coordinates.globalX * perlinMediumTerrainScale + 2000f,
            cell.Coordinates.globalZ * perlinMediumTerrainScale + 2000f
        );

        if (hillsNoise < hillsNoiseMargin) hillsNoise = 0;
        hillsNoise *= hillsNoiseTerrainAmp;

        return (int)(hillsNoise);
    }

    private int SetMountains(HexCell cell)
    {
        float mountainsNoise = Mathf.PerlinNoise(
             cell.Coordinates.globalX * perlinSmallTerrainScale + 3000f,
             cell.Coordinates.globalZ * perlinSmallTerrainScale + 3000f
         );

        if (mountainsNoise < mountainsNoiseMargin) mountainsNoise = 0;
        mountainsNoise *= mountainsNoiseTerrainAmp;

        return (int)mountainsNoise;
    }

    private void DetectEdgeType(HexCell[] gridCells)
    {
        for (int i = 0; i < gridCells.Length; i++)
        {
            HexCell cell = gridCells[i];

            for (int j = 0; j < 6; j++)
            {
                HexCell neighbourCell = cell.GetNeighbor((HexDirection)j);

                if (neighbourCell != null)
                {
                    int terrainLevelDiff = Mathf.Abs(cell.TerrainLevelIndex - neighbourCell.TerrainLevelIndex);

                    if (terrainLevelDiff == 0)
                    {
                        cell.AddEdge(EdgeType.Flat, (HexDirection)j);
                    }
                    else if (terrainLevelDiff == 1)
                    {
                        cell.AddEdge(EdgeType.Smooth, (HexDirection)j);

                        allSmoothEdges.Add(cell.GetEdge(j));
                    }
                    else if (terrainLevelDiff > 1)
                    {
                        cell.AddEdge(EdgeType.Cliff, (HexDirection)j);
                    }
                }
                else
                {
                    cell.AddEdge(EdgeType.None, (HexDirection)j);
                }
            }
        }

        GroupSmoothEdges();
    }

    //zaczynamy od jakiejs k v1 i v2 end =k, start=k
    //sprawdzamy end = k.v2
    //jesli v2 == inna v1 to end = inna.v1 dodoaj inna na koniec listy
    //jesli v2 == inna v2 to end = inna.v2 dodoaj inna na koniec listy
    //sprawdzamy start = k.v1
    //jesli v1 == inna v1 to start = inna.v1 dodaj inna na poczatek listy
    //jesli v1 == inna v2 to start = inna.v2 dodaj inna na poczatek listy

    private void GroupSmoothEdges()
    {
        HashSet<Edge> used = new HashSet<Edge>();

        foreach (var e in allSmoothEdges)
        {
            if (used.Contains(e))
                continue;

            List<Edge> chain = new List<Edge>();
            chain.Add(e);
            used.Add(e);

            Vector3 end = e.GetFullV2();
            bool extended = true;
            while (extended)
            {
                extended = false;
                foreach (var other in allSmoothEdges)
                {
                    if (used.Contains(other)) continue;

                    if (SamePoint(end, other.GetFullV1()))
                    {
                        chain.Add(other);
                        used.Add(other);
                        end = other.GetFullV2();
                        extended = true;
                        break;
                    }
                    else if (SamePoint(end, other.GetFullV2()))
                    {
                        chain.Add(other);
                        used.Add(other);
                        end = other.GetFullV1();
                        extended = true;
                        break;
                    }
                }
            }

            Vector3 start = e.GetFullV1();
            extended = true;
            while (extended)
            {
                extended = false;
                foreach (var other in allSmoothEdges)
                {
                    if (used.Contains(other)) continue;

                    if (SamePoint(start, other.GetFullV2()))
                    {
                        chain.Insert(0, other);
                        used.Add(other);
                        start = other.GetFullV1();
                        extended = true;
                        break;
                    }
                    else if (SamePoint(start, other.GetFullV1()))
                    {
                        chain.Insert(0, other);
                        used.Add(other);
                        start = other.GetFullV2();
                        extended = true;
                        break;
                    }
                }
            }

            groupsSmoothEdges.Add(chain);
        }

        SmoothEdges();
    }

    private bool SamePoint(Vector3 a, Vector3 b)
    {
        return (a - b).sqrMagnitude < 0.0001f;
    }

    //private void FindStartChain()
    //{
    //    for (int i = 0; i < groupsSmoothEdges.Count; i++) 
    //    {
    //        var group = groupsSmoothEdges[i];

    //        Edge e0 = group[0];
    //        Edge e1 = group[1];

    //        List<Vector3> v = new List<Vector3>();

    //        if (SamePoint(e0.GetFullV2(), e1.GetFullV1()))
    //        {
    //            for (int j = 0; j < group.Count; j++) 
    //            {
    //                v.Add(group[j].GetGlobalV1());
    //                v.Add(group[j].GetGlobalV2());
    //            }

    //            bool isLoop = IsLoop(v);
    //        }
    //        else
    //        {
    //            for(int j = group.Count - 1; j >= 0; j--)
    //            {
    //                v.Add(group[j].GetGlobalV1());
    //                v.Add(group[j].GetGlobalV2());
    //            }

    //            bool isLoop = IsLoop(v);
    //        }
    //    }
    //}

    //List<Vector3> RemoveDuplicates(List<Vector3> pts)
    //{
    //    List<Vector3> unique = new List<Vector3>();
    //    if (pts.Count == 0) return unique;

    //    unique.Add(pts[0]);

    //    for (int i = 1; i < pts.Count; i++)
    //    {
    //        if ((pts[i] - pts[i - 1]).sqrMagnitude > 0.000001f)
    //            unique.Add(pts[i]);
    //    }

    //    return unique;
    //}

    //bool IsLoop(List<Vector3> pts)
    //{
    //    return SamePoint(pts[0], pts[pts.Count - 1]);
    //}

    //List<Vector3> ChaikinSmoothSameCount(List<Vector3> baseVerticles, bool isLoop)
    //{
    //    baseVerticles = RemoveDuplicates(baseVerticles);

    //    int N = baseVerticles.Count;
    //    if (N < 3)
    //        return new List<Vector3>(baseVerticles);

    //    List<Vector3> smooth = new List<Vector3>(baseVerticles);

    //    for (int i = 0; i < 4; i++) 
    //    {
    //        List<Vector3> next = new List<Vector3>();

    //        int count = smooth.Count;

    //        if (!isLoop)
    //            next.Add(smooth[0]); 

    //        for (int j = 0; j < count - 1; j++)
    //        {
    //            Vector3 p = smooth[j];
    //            Vector3 q = smooth[(j + 1)];

    //            Vector3 Q = 0.75f * p + 0.25f * q;
    //            Vector3 R = 0.25f * p + 0.75f * q;

    //            next.Add(Q);
    //            next.Add(R);
    //        }

    //        if (!isLoop)
    //            next.Add(smooth[count - 1]); 
    //        else
    //        {
    //            Vector3 p = smooth[count - 1];
    //            Vector3 q = smooth[0];

    //            Vector3 Q = 0.75f * p + 0.25f * q;
    //            Vector3 R = 0.25f * p + 0.75f * q;

    //            next.Add(Q);
    //            next.Add(R);
    //        }

    //        smooth = next;
    //    }


    //    List<Vector3> result = new List<Vector3>();
    //    float step = (smooth.Count - 1) / (float)(N - 1);

    //    for (int i = 0; i < N; i++)
    //    {
    //        float fIndex = step * i;
    //        int a = Mathf.FloorToInt(fIndex);
    //        int b = Mathf.Min(a + 1, smooth.Count - 1);
    //        float t = fIndex - a;

    //        Vector3 v = Vector3.Lerp(smooth[a], smooth[b], t);
    //        result.Add(v);
    //    }

    //    if (!isLoop)
    //    {
    //        result[0] = baseVerticles[0];
    //        result[result.Count - 1] = baseVerticles[baseVerticles.Count - 1];
    //    }

    //    return result;
    //}


    //private void OnDrawGizmos()
    //{
    //    if (groupsSmoothEdges == null || groupsSmoothEdges.Count == 0)
    //        return;

    //    Color[] palette = new Color[]
    //    {
    //    Color.red,
    //    Color.green,
    //    Color.blue,
    //    Color.yellow,
    //    Color.cyan,
    //    Color.magenta,
    //    new Color(1f, 0.5f, 0f),      // orange
    //    new Color(0.5f, 0f, 1f),      // purple
    //    new Color(0f, 0.5f, 1f),      // azure
    //    new Color(0.4f, 1f, 0.2f),    // lime
    //    new Color(1f, 0.2f, 0.6f),    // pink
    //    new Color(0.6f, 0.6f, 0.6f),  // grey
    //    };

    //    int pCount = palette.Length;
    //    Vector3 offset = Vector3.up * 0.05f;

    //    for (int i = 0; i < groupsSmoothEdges.Count; i++)
    //    {
    //        List<Edge> chain = groupsSmoothEdges[i];
    //        if (chain == null || chain.Count == 0)
    //            continue;

    //        Color c = palette[i % pCount];
    //        Gizmos.color = c;

    //        float sphereSize = 0.08f;

    //        foreach (var edge in chain)
    //        {
    //            Vector3 p1 = edge.GetGlobalV1() + offset;
    //            Vector3 p2 = edge.GetGlobalV2() + offset;

    //            Gizmos.DrawSphere(p1, sphereSize);
    //            Gizmos.DrawSphere(p2, sphereSize);
    //            Gizmos.DrawLine(p1, p2);
    //        }

    //        Gizmos.color = Color.green;
    //        float bigSize = 3.5f;

    //        Vector3 startPoint = chain[0].GetGlobalV1() + offset;
    //        Gizmos.DrawSphere(startPoint, bigSize);

    //        Gizmos.color = Color.red;

    //        Vector3 endPoint = chain[chain.Count - 1].GetGlobalV2() + offset;
    //        Gizmos.DrawSphere(endPoint, bigSize);
    //    }
    //}

    private void SmoothEdges()
    {
        for (int i = 0; i < groupsSmoothEdges.Count; i++)
        {
            var group = groupsSmoothEdges[i];

            if (group.Count <= 1) continue;

            List<Vector3> verticles = FindChainStart(group);

            bool isLoop = IsLoop(verticles);

            List<int> map;
            List<Vector3> unique = RemoveDuplicates(verticles, out map);

            List<Vector3> smoothUnique = ChaikinSmoothSameCount(unique, isLoop);

            List<Vector3> result = ReapplyDuplicates(smoothUnique, map);

            WriteBackToEdges(group, result);
        }
    }

    private List<Vector3> FindChainStart(List<Edge> group)
    {
        if (group.Count <= 1)
            return new List<Vector3>();

        Edge e0 = group[0];
        Edge e1 = group[1];

        bool forward = SamePoint(e0.GetFullV2(), e1.GetFullV1());

        List<Vector3> v = new List<Vector3>();

        if (forward)
        {
            for (int j = 0; j < group.Count; j++)
            {
                v.Add(group[j].GetGlobalV1());
                v.Add(group[j].GetGlobalV2());
            }
        }
        else
        {
            for (int j = group.Count - 1; j >= 0; j--)
            {
                v.Add(group[j].GetGlobalV1());
                v.Add(group[j].GetGlobalV2());
            }
        }

        return v;
    }

    private bool IsLoop(List<Vector3> v)
    {
        return SamePoint(v[0], v[v.Count - 1]);
    }

    private List<Vector3> RemoveDuplicates(List<Vector3> baseVerticles, out List<int> mapToUnique)
    {
        mapToUnique = new List<int>();
        List<Vector3> unique = new List<Vector3>();

        for (int i = 0; i < baseVerticles.Count; i++)
        {
            bool found = false;
            for (int j = 0; j < unique.Count; j++)
            {
                if (SamePoint(baseVerticles[i], unique[j]))
                {
                    mapToUnique.Add(j);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                mapToUnique.Add(unique.Count);
                unique.Add(baseVerticles[i]);
            }
        }

        return unique;
    }

    private List<Vector3> ChaikinSmoothSameCount(List<Vector3> pts, bool loop, int iterations = 3)
    {
        if (pts.Count < 3)
            return new List<Vector3>(pts);

        List<Vector3> current = new List<Vector3>(pts);

        for (int it = 0; it < iterations; it++)
        {
            List<Vector3> subdiv = new List<Vector3>();

            int count = current.Count;

            for (int i = 0; i < count - 1; i++)
            {
                Vector3 p0 = current[i];
                Vector3 p1 = current[i + 1];

                Vector3 Q = p0 * 0.75f + p1 * 0.25f; // bli¿ej p0
                Vector3 R = p0 * 0.25f + p1 * 0.75f; // bli¿ej p1

                subdiv.Add(Q);
                subdiv.Add(R);
            }

            if (loop)
            {
                Vector3 p0 = current[count - 1];
                Vector3 p1 = current[0];

                Vector3 Q = p0 * 0.75f + p1 * 0.25f;
                Vector3 R = p0 * 0.25f + p1 * 0.75f;

                subdiv.Add(Q);
                subdiv.Add(R);
            }

            // nadpisz listê
            current = Resample(subdiv, pts.Count, loop: loop);
        }

        return current;
    }

    private List<Vector3> Resample(List<Vector3> subdiv, int targetCount, bool loop)
    {
        int m = subdiv.Count;

        List<float> cumulative = new List<float>(m);
        cumulative.Add(0f);

        float totalLength = 0f;

        for (int i = 1; i < m; i++)
        {
            float seg = Vector3.Distance(subdiv[i - 1], subdiv[i]);
            totalLength += seg;
            cumulative.Add(totalLength);
        }

        float loopLength = 0f;
        if (loop)
        {
            loopLength = Vector3.Distance(subdiv[m - 1], subdiv[0]);
            totalLength += loopLength;
        }

        List<Vector3> result = new List<Vector3>(targetCount);

        for (int i = 0; i < targetCount; i++)
        {
            float t = (totalLength * i) / (targetCount - (loop ? 0 : 1));
  
            if (loop && t > cumulative[m - 1])
            {
                float localT = t - cumulative[m - 1]; 
                float lerp = localT / loopLength;
                result.Add(Vector3.Lerp(subdiv[m - 1], subdiv[0], lerp));
                continue;
            }

            int k = 1;
            while (k < m && cumulative[k] < t)
                k++;

            if (k == m)
            {
                result.Add(subdiv[m - 1]);
                continue;
            }

            float segStart = cumulative[k - 1];
            float segEnd = cumulative[k];
            float alpha = (t - segStart) / (segEnd - segStart);

            Vector3 p = Vector3.Lerp(subdiv[k - 1], subdiv[k], alpha);
            result.Add(p);
        }

        return result;
    }

    private List<Vector3> ReapplyDuplicates(List<Vector3> uniqueSmoothed, List<int> mapToUnique)
    {
        List<Vector3> result = new List<Vector3>(mapToUnique.Count);

        for (int i = 0; i < mapToUnique.Count; i++)
        {
            int u = mapToUnique[i];
            result.Add(uniqueSmoothed[u]);
        }

        return result;
    }

    private void WriteBackToEdges(List<Edge> edges, List<Vector3> newPts)
    {
        int p = 0;

        for (int i = 0; i < edges.Count; i++)
        {
            edges[i].SetGlobalV1(newPts[p++]);
            edges[i].SetGlobalV2(newPts[p++]);
        }
    }
}
