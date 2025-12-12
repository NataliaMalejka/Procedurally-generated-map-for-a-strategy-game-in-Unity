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

    private HexDirection windDirection;

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

    [Header("Temperature")]
    [SerializeField] private float coldMax = 0.4f;    
    [SerializeField] private float moderateTempMax = 0.7f;    
    [SerializeField] private float temperatureTerrainScale = 20f;

    [Header("Moisture")]
    [SerializeField] private int windStrength = 4;
    [SerializeField] private float dryStrength = 0.2f;
    [SerializeField] private int dryDistance = 4;
    [SerializeField] private float rainStrength = 0.5f;
    [SerializeField] private int rainDistance = 3;
    [SerializeField] private float dryMax = 0.4f;
    [SerializeField] private float moderateMoistureMax = 0.7f;

    [Header("Hex Noise")]
    public Texture2D hexMeshNoise;

    [Header("Textures")]
    public Material terrainMaterial;
    public Texture2D[] texturesColor;

    private void Awake()
    {
        Instance = this;

        UnityEngine.Random.InitState(seed);

        SetTextures();

        (xChunkCount, zChunkCount) = SetChunkCounts(mapSize);

        continentsCount = SetContinentsCount(mapSize);

        continents = new Continent[continentsCount];

        SetMaxMargin();

        SetContinentCellsAmound();

        SetContinentsInRegions();
    }

    private void SetTextures()
    {
        int w = texturesColor[0].width;
        int h = texturesColor[0].height;

        var texArray = new Texture2DArray(
            w, h,
            texturesColor.Length,
            TextureFormat.RGBA32,
            true
        );

        texArray.wrapMode = TextureWrapMode.Repeat;
        texArray.filterMode = FilterMode.Bilinear;

        for (int i = 0; i < texturesColor.Length; i++)
        {
            Texture2D tex = texturesColor[i];

            Texture2D converted = ConvertToRGBA32(tex);

            Graphics.CopyTexture(converted, 0, 0, texArray, i, 0);
        }

        texArray.Apply();

        terrainMaterial.SetTexture("_MainTexture", texArray);
    }

    private Texture2D ConvertToRGBA32(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.ARGB32
        );

        Graphics.Blit(source, rt);

        Texture2D tex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, true);
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        tex.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return tex;
    }

    private (int x, int z) SetChunkCounts(MapSize size)
    {
        return size switch
        {
            MapSize.Small => (15, 10),
            MapSize.Medium => (21, 14),
            MapSize.Large => (27, 18),
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

    public void GenerateMap(HexCell[] gridCells)
    {
        GenerateContinents(gridCells);

        SetOceans(gridCells);

        RemoveLakes();

        CalculateDistancToOcean(gridCells);

        CalculateTerrainLevelAndTemperature(gridCells);

        CalculateMoisture(gridCells);

        SetBiomes(gridCells);

        CreateMountains(gridCells);

        DetectEdgeType(gridCells);
    }

    public void GenerateContinents(HexCell[] gridCells)
    {
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
    }

    private void SetContinentPart(HexCell cell, int continentIndex)
    {
        cell.SetContinent(continentIndex);
        continents[continentIndex].AddCell(cell);

        Chunk chunk = cell.HexChunk;

        continents[continentIndex].AddChunk(cell.HexChunk);
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

    private void SetOceans(HexCell[] gridCells)
    {
        var cellsToCheck = new Queue<HexCell>();

        HexCell startCell = gridCells[0];
        startCell.isOcean = true;
        startCell.DistanceFromOcean = 0;
        startCell.Moisture = 1f;
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
                neighborCell.Moisture = 1f;

                cellsToCheck.Enqueue(neighborCell);
            }
        }
    }

    private void RemoveLakes()
    {    
        for (int i = 0; i < continentsCount; i++)
        {
            var chunkList = continents[i].GetContinentChunks();

            foreach (var chunk in chunkList)
            {
                foreach (var cell in chunk.GetCells())
                {
                    if (cell.TerrainLevel == -1 && !cell.isOcean)
                        SetContinentPart(cell, i);
                }
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

    private void CalculateTerrainLevelAndTemperature(HexCell[] gridCells)
    {
        foreach (var cell in gridCells)
        {
            if (!cell.isOcean)
            {
                SetTerrainLevel(cell);
                SetTemperature(cell);
            }
        }
    }

    private void SetTerrainLevel(HexCell cell)
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

    private void SetTemperature(HexCell cell)
    {
        float latitude = (float)cell.Coordinates.GlobalZ / (zCellCount * zChunkCount);

        latitude *= 2f;
        if (latitude > 1f)
        {
            latitude = 2f - latitude;
        }

        float exponent = 0.9f;
        latitude = Mathf.Pow(latitude, exponent);

        float temperature = Mathf.LerpUnclamped(0f, 1f, latitude);

        temperature *= 1f - cell.TerrainLevelIndex / temperatureTerrainScale;

        temperature = Mathf.Clamp01(temperature);

        cell.Temperature = temperature;
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

    private void CalculateMoisture(HexCell[] gridCells)
    {
        SetWind();
        ApplyOceanMoisture(gridCells);
        ApplyWind(gridCells);
        ApplyMountainsDry(gridCells);
        ApplyMountainsRain(gridCells);
    }

    private void SetWind()
    {
        windDirection = (HexDirection)UnityEngine.Random.Range(0, 6);
    }

    private void ApplyOceanMoisture(HexCell[] gridCells)
    {
        foreach (var cell in gridCells)
        {
            if (!cell.isOcean)
            {
                cell.Moisture = Mathf.Max(0f, 1f - 0.1f * cell.DistanceFromOcean);
            }
        }
    }

    private void ApplyWind(HexCell[] gridCells)
    {
        Dictionary<HexCell, float> newMoisture = new Dictionary<HexCell, float>();

        foreach (var cell in gridCells)
        {
            HexCell source = FindMoistureSource(cell);

            if (source == null)
            {
                newMoisture[cell] = cell.Moisture;
                continue;
            }
            newMoisture[cell] = Mathf.Clamp01(source.Moisture);
        }

        foreach (var kv in newMoisture)
            kv.Key.Moisture = kv.Value;
    }

    private HexCell FindMoistureSource(HexCell start)
    {
        Queue<(HexCell cell, int dist)> q = new Queue<(HexCell, int)>();
        HashSet<HexCell> visited = new HashSet<HexCell>();

        q.Enqueue((start, 0));
        visited.Add(start);

        HexCell result = start;

        while (q.Count > 0)
        {
            var (current, dist) = q.Dequeue();

            if (dist == windStrength)
                continue;                   

            HexCell n = current.GetNeighbor(windDirection.Opposite());
            if (n == null || visited.Contains(n))
                continue;

            visited.Add(n);
            q.Enqueue((n, dist + 1));
            result = n;
        }

        return result;
    }

    private void ApplyMountainsDry(HexCell[] gridCells)
    {
        foreach (var cell in gridCells)
        {
            if (!cell.IsMountain)
                continue;


            HexDirection[] windDirs = new HexDirection[]
            {
                windDirection,
                windDirection.Next(),
            };

            Queue<(HexCell cell, int dist)> q = new Queue<(HexCell, int)>();
            HashSet<HexCell> visited = new HashSet<HexCell>();

            q.Enqueue((cell, 0));
            visited.Add(cell);

            while (q.Count > 0)
            {
                var (current, dist) = q.Dequeue();

                if (dist > dryDistance)
                    continue;

                float factor = 1f - (dryStrength * (dist / (float)dryDistance));
                factor = Mathf.Clamp01(factor);

                if (dist > 0)
                    current.Moisture *= factor;

                foreach (var dir in windDirs)
                {
                    HexCell n = current.GetNeighbor(dir);
                    if (n == null || visited.Contains(n))
                        continue;

                    visited.Add(n);
                    q.Enqueue((n, dist + 1));
                }
            }
        }
    }


    private void ApplyMountainsRain(HexCell[] gridCells)
    {
        foreach (var cell in gridCells)
        {
            if (!cell.IsMountain)
                continue;

            HexDirection[] windDirs = new HexDirection[]
            {
                windDirection.Opposite(),
                windDirection.Opposite().Next(),
            };

            Queue<(HexCell c, int dist)> q = new Queue<(HexCell c, int dist)>();
            HashSet<HexCell> visited = new HashSet<HexCell>();

            q.Enqueue((cell, 0));
            visited.Add(cell);

            while (q.Count > 0)
            {
                var (current, dist) = q.Dequeue();

                if (dist > rainDistance)
                    continue;

                if (dist > 0)
                {
                    float multiplier = 1f + rainStrength * (1f - dist / (float)rainDistance);
                    current.Moisture *= multiplier;
                    current.Moisture = Mathf.Clamp01(current.Moisture);
                }

                foreach (var dir in windDirs)
                {
                    HexCell n = current.GetNeighbor(dir);
                    if (n == null || visited.Contains(n))
                        continue;

                    visited.Add(n);
                    q.Enqueue((n, dist + 1));
                }
            }
        }
    }


    private void SetBiomes(HexCell[] gridCells)
    {
        foreach (var cell in gridCells)
        {
            if (cell.isOcean)
            {
                cell.SetBiome(Biome.Ocean);
                continue;
            }
            if (cell.IsMountain)
            {
                cell.SetBiome(Biome.Mountain);
                continue;
            }
            if (cell.Temperature < coldMax)
            {
                cell.SetBiome(Biome.Tundra);
                continue;
            }
            if (cell.Temperature < moderateTempMax)
            {
                if (cell.Moisture < dryMax)
                {
                    cell.SetBiome(Biome.Grassland);
                }
                else if (cell.Moisture < moderateMoistureMax)
                {
                    cell.SetBiome(Biome.ContinentalDry);
                }
                else
                {
                    cell.SetBiome(Biome.continentalWet);
                }
                continue;
            }
            else
            {
                if (cell.Moisture < dryMax)
                {
                    cell.SetBiome(Biome.Desert);
                }
                else if (cell.Moisture < moderateMoistureMax)
                {
                    cell.SetBiome(Biome.Savanna);
                }
                else
                {
                    cell.SetBiome(Biome.RainForest);
                }
                continue;
            }
        }
    }

    private void CreateMountains(HexCell[] gridCells)
    {
        foreach (var cell in gridCells)
        {
            if (cell.IsMountain)
            {
                int baseMountainLevel = 3;

                for (int j = 0; j < 6; j++)
                {
                    HexCell neighbourCell = cell.GetNeighbor((HexDirection)j);

                    if (neighbourCell != null)
                    {
                        if (!neighbourCell.IsMountain)
                        {
                            if (neighbourCell.TerrainLevel > baseMountainLevel)
                            {
                                baseMountainLevel = neighbourCell.TerrainLevel;
                            }
                        }
                    }
                }

                cell.SetTerrainLevel(baseMountainLevel);
                cell.CentreTerrainLevel += UnityEngine.Random.Range(7f, 13f);
            }
        }
    }

    private void DetectEdgeType(HexCell[] gridCells)
    {
        List<Edge> allSmoothEdges = new List<Edge>();

        for (int i = 0; i < gridCells.Length; i++)
        {
            HexCell cell = gridCells[i];

            for (int j = 0; j < 6; j++)
            {
                HexCell neighbourCell = cell.GetNeighbor((HexDirection)j);

                if (neighbourCell != null)
                {
                    int terrainLevelDiff = Mathf.Abs(cell.TerrainLevelIndex - neighbourCell.TerrainLevelIndex);

                    if(cell.IsMountain || neighbourCell.IsMountain)
                    {
                        cell.AddEdge(EdgeType.Mountain, (HexDirection)j);
                    }
                    else if (terrainLevelDiff == 0)
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

        GroupSmoothEdges(allSmoothEdges);
    }

    private void GroupSmoothEdges(List<Edge> allSmoothEdges)
    {
        List<List<Edge>> groupsSmoothEdges = new List<List<Edge>>();
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

        SmoothEdges(groupsSmoothEdges);
    }

    private bool SamePoint(Vector3 a, Vector3 b)
    {
        return (a - b).sqrMagnitude < 0.0001f;
    }

    private void SmoothEdges(List<List<Edge>> groupsSmoothEdges)
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

                Vector3 Q = p0 * 0.75f + p1 * 0.25f; 
                Vector3 R = p0 * 0.25f + p1 * 0.75f; 

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
