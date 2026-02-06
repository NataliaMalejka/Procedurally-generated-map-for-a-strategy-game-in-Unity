using System;
using System.Collections.Generic;
using UnityEngine;

// Map size
public enum MapSize
{
    Small,
    Medium,
    Large
}

[System.Serializable]
public class BiomeObjects
{
    public Biome biome;
    public GameObject[] objects;
}

// Generate map
public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    private MapSize mapSize;

    // Chunks inn layer
    public int xChunkCount { get; private set; }
    public int zChunkCount { get; private set; }

    // Cells in chunk
    public int xCellCount { get; private set; } = 6;
    public int zCellCount { get; private set; } = 6;

    // Map margin
    public int minXMargin { get; private set; } = 5;
    public int minZMargin { get; private set; } = 5;
    public int maxXMargin { get; private set; }
    public int maxZMargin { get; private set; }

    // Continennts region
    private int regionsCount;

    private Continent[] continents;
    private int continentsCount;

    private int[] continentsCentres;
    private int contonentCentreMargin = 7;

    // Max cell count in continent
    private int[] maxContinentsCellsAmound;

    // Continent direction (for continent growth)
    private Vector2[] continentDir;

    // Wind direction (for calculate climate)
    private HexDirection windDirection;

    // For create rivers
    private List<HexCell> potencionalRiverSources = new List<HexCell>();

    // Parameters for map generation
    [Header("Continent Shape")]
    [SerializeField] private float perlinScale = 0.15f;
    [SerializeField] private float distWeight = 0.3f;
    [SerializeField] private float perlinWeight = 0.5f;
    [SerializeField] private float dirWeight = 0.3f;
    [SerializeField] private float maxScore = 0.7f;
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

    [Header("Rivers")]
    [SerializeField] private float riverSuorceMinLevel = 4;
    [SerializeField] private float riverSuorceMinMoisture = 0.4f;
    [SerializeField] private int riversPerContinentMin = 4;
    [SerializeField] private int riversPerContinentMax = 7;

    [Header("Hex Noise")]
    [SerializeField] private Texture2D hexMeshNoise;

    [Header("Textures")]
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private Material riverMaterial;
    [SerializeField] private Texture2D snowTexture;
    [SerializeField] private Texture2DArray terrainTextureAlbedo;
    [SerializeField] private Texture2DArray terrainTextureNormal;

    // Diffrent water color for each layer
    [SerializeField] private Color[] waterColors;
    private Texture2D paletteTex;
    private int paletteCount;

    [Header("Structures")]
    public BiomeObjects[] biomeObjects;
    [SerializeField] private int structureChance;

    [Header("Units")]
    [SerializeField] private Unit UnitPrefab;

    private const int biomesPerLayer = 7;

    // Get object from dictionary
    public GameObject[] GetObjects(Biome biome)
    {
        foreach (var group in biomeObjects)
        {
            if (group.biome == biome)
                return group.objects;
        }

        return null;
    }

    private void Awake()
    {
        Instance = this;

        ApplySeed();

        SetTextures();

        mapSize = GameSettings.Instance.GetMapSize();

        (xChunkCount, zChunkCount) = SetChunkCounts(mapSize);

        //calculate margin based by map size
        maxXMargin = xCellCount * xChunkCount - minXMargin;
        maxZMargin = zCellCount * zChunkCount - minZMargin;
    }

    // Seed from GameSettings 
    // Random if missing
    private void ApplySeed()
    {
        int seed = UnityEngine.Random.Range(0, int.MaxValue);

        if (int.TryParse(GameSettings.Instance.GetSeedString(), out int parsedSeed))
        {
            if (parsedSeed >= 0 && parsedSeed <= int.MaxValue)
            {
                seed = parsedSeed;
            }
        }

        UnityEngine.Random.InitState(seed);
    }

    // Set Shader data
    private void SetTextures()
    {
        terrainMaterial.SetTexture("_TexColor", terrainTextureAlbedo);

        terrainMaterial.SetTexture("_TexNormal", terrainTextureNormal);

        snowTexture.wrapMode = TextureWrapMode.Repeat;
        snowTexture.filterMode = FilterMode.Bilinear;
        terrainMaterial.SetTexture("_TexSnow", snowTexture);

        terrainMaterial.SetFloat("_ColdMax", coldMax);
    }

    // Water color texture
    private void BuildPaletteTexture()
    {
        paletteCount = waterColors.Length / 2;

        paletteTex = new Texture2D(paletteCount, 2, TextureFormat.RGBA32, false);
        paletteTex.filterMode = FilterMode.Point;
        paletteTex.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < paletteCount; i++)
        {
            paletteTex.SetPixel(i, 0, waterColors[i * 2]);
            paletteTex.SetPixel(i, 1, waterColors[i * 2 + 1]);
        }

        paletteTex.Apply();

        waterMaterial.SetTexture("_ColorPalette", paletteTex);
        riverMaterial.SetTexture("_ColorPalette", paletteTex);

        waterMaterial.SetFloat("_PaletteSize", paletteCount);
        riverMaterial.SetFloat("_PaletteSize", paletteCount);
    }

    public Material GetTerrainmaterial()
    {
        return terrainMaterial;
    }

    public Material GetWatermaterial()
    {
        return waterMaterial;
    }

    public Material GetRivermaterial()
    {
        return riverMaterial;
    }

    public Texture2D GetHexMeshNoise()
    {
        return hexMeshNoise;
    }

    public Unit GetUnitPrefab()
    {
        return UnitPrefab;
    }

    // Chunk count (map size)
    private (int x, int z) SetChunkCounts(MapSize size)
    {
        return size switch
        {
            MapSize.Small => (15, 10),
            MapSize.Medium => (21, 14),
            MapSize.Large => (24, 16),
            _ => (21, 14)
        };
    }

    // Continents count (map size)
    private int SetContinentsCount(MapSize size)
    {
        return size switch
        {
            MapSize.Small => (UnityEngine.Random.Range(2, 4)),
            MapSize.Medium => (UnityEngine.Random.Range(3, 6)),
            MapSize.Large => (UnityEngine.Random.Range(4, 7)),
            _ => (UnityEngine.Random.Range(3, 6))
        };
    }

    // Regions count (continent count)
    private void SetMaxMargin()
    {
        regionsCount = continentsCount / 2 + continentsCount % 2;
        if (regionsCount < 2) regionsCount = 2;

        continentsCentres = new int[continentsCount];
        continentDir = new Vector2[continentsCount];
    }

    // Max cell per continent
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

    // Set start continents cells in diffrent regions 
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

    // Random continent centres (in region)
    private void SetContinentsCentres(int localXMin, int localXMax, int localZMin, int localZMax, int index)
    {
        int xPos = UnityEngine.Random.Range(localXMin, localXMax);
        int zPos = UnityEngine.Random.Range(localZMin, localZMax);

        continentsCentres[index] = GetCellIndex(xPos, zPos);
    }

    // Get cell index in grid
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

    // Map generation process
    public void GenerateMap(HexCell[] gridCells, int biomeLayerIndex, int level)
    {
        BuildPaletteTexture();

        continentsCount = SetContinentsCount(mapSize);

        continents = new Continent[continentsCount];

        SetMaxMargin();

        SetContinentCellsAmound();

        SetContinentsInRegions();

        GenerateContinents(gridCells);

        SetOceans(gridCells);

        RemoveLakes();

        CalculateDistancToOcean(gridCells);

        CalculateBiomesData(gridCells);

        CalculateMoisture(gridCells);

        SetBiome(gridCells, biomeLayerIndex);

        SmoothBiomes(gridCells);

        DetectEdgeType(gridCells);

        CreateRivers();

        SetOceanDeep(gridCells);

        RandomStructures(gridCells);

        RandomUnits(gridCells);

        GroupSmoothEdges(gridCells);

        SetTextPos(gridCells, level);
    }

    // Continenst growth
    private void GenerateContinents(HexCell[] gridCells)
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

            // Add all neighbour start cells
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

                    // Add cell to continent
                    SetContinentPart(neighborCell, i);
                    cellsToCheck.Enqueue(neighborCell);

                    cellsCreated++;
                }
            }
        }
    }

    // Add cell and chunk to continent
    private void SetContinentPart(HexCell cell, int continentIndex)
    {
        cell.SetContinent(continentIndex);
        continents[continentIndex].AddCell(cell);

        Chunk chunk = cell.HexChunk;

        continents[continentIndex].AddChunk(cell.HexChunk);
    }

    // Add neighbours of start cell to continent
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

    // Is cell behind map border
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

    // Is cell near to other continent
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
        // Distance-based factor (limits continent growth range)
        float dist = Vector2.Distance(neighborCell.Coordinates.GetCellPos(), centreCell.Coordinates.GetCellPos());
        float distFactor = Mathf.Clamp01(1f - dist / (zCellCount * zChunkCount / 2));

        // Perlin noise factor for irregular continent shape
        float noiseFactor = Mathf.PerlinNoise(neighborCell.Coordinates.globalX * perlinScale, neighborCell.Coordinates.globalZ * perlinScale);

        // Directional growth factor (preferred expansion direction)
        Vector2 toHex = (neighborCell.Coordinates.GetCellPos() - centreCell.Coordinates.GetCellPos()).normalized;
        float dirFactor = Vector2.Dot(toHex, continentDir[index].normalized) * 0.5f + 0.5f;

        // Random coastline variation
        float coastNoise = UnityEngine.Random.Range(minCoastNoise, maxCoastNoise);

        // Growth bonus based on current continent fill
        float cellsFill = (float)continents[index].GetContinentCells().Count / maxContinentsCellsAmound[index];
        float growthBonus = 1f + growthBonusFactor * (1f - cellsFill * cellsFill);

        // Final combining all weighted factors
        float score = distFactor * distWeight + noiseFactor * perlinWeight + dirFactor * dirWeight;

        // Cell is added to continent if score is below maxScore
        return score * coastNoise * growthBonus < maxScore;
    }

    // Set ocean data is cell is not in any continent
    private void SetOceans(HexCell[] gridCells)
    {
        var cellsToCheck = new Queue<HexCell>();

        HexCell startCell = gridCells[0];
        startCell.IsOcean = true;
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

                if (neighborCell.IsOcean) continue;

                neighborCell.IsOcean = true;
                neighborCell.DistanceFromOcean = 0;
                neighborCell.Moisture = 1f;

                cellsToCheck.Enqueue(neighborCell);
            }
        }
    }

    // Remove water cell inside continent
    private void RemoveLakes()
    {    
        for (int i = 0; i < continentsCount; i++)
        {
            var chunkList = continents[i].GetContinentChunks();

            foreach (var chunk in chunkList)
            {
                foreach (var cell in chunk.GetCells())
                {
                    if (cell.TerrainLevelIndex == -2 && !cell.IsOcean)
                        SetContinentPart(cell, i);
                }
            }
        }       
    }

    // Distance ro ocean (for calculate climate)
    private void CalculateDistancToOcean(HexCell[] gridCells)
    {
        Queue<HexCell> queue = new Queue<HexCell>();

        for (int i = 0; i < gridCells.Length; i++)
        {
            HexCell cell = gridCells[i];

            if (cell.IsOcean)
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

    // Climate data
    private void CalculateBiomesData(HexCell[] gridCells)
    {
        foreach (var cell in gridCells)
        {
            if (!cell.IsOcean)
            {
                SetTerrainLevel(cell);
                SetTemperature(cell);

                if (cell.IsMountain)
                {
                    CreateMountain(cell);
                }
            }
            else
            {
                cell.Temperature = 0.5f;
                SetArctic(cell, gridCells);
            }
        }
    }

    // terrain level = base level (ocenat distance) + noise highlands + hill noise + mountains noise
    private void SetTerrainLevel(HexCell cell)
    {
        int heightDistanceToOcean = CalculateDictanceToOceanHeight(cell);

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

        if(SetMountains(cell) > 0) // if mountain
            cell.IsMountain = true;

        cell.SetTerrainLevel(terrainLevel);
    }

    // Temperature based on latitude and terrain level
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

    // Set mountain terrain level
    private void CreateMountain(HexCell cell)
    {
        int baseMountainLevel = 3;

        for (int i = 0; i < 6; i++) 
        {
            HexCell neighbourCell = cell.GetNeighbor((HexDirection)i);

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

    // Arctic on map borders
    private void SetArctic(HexCell cell, HexCell[] gridCells)
    {
        if (cell.Coordinates.GlobalZ == 0 || cell.Coordinates.GlobalZ == (zCellCount * zChunkCount - 1))
        {
            SetArcticCell(cell);
        }
        else if (cell.Coordinates.GlobalZ == 1 || cell.Coordinates.GlobalZ == (zCellCount * zChunkCount - 2))
        {
            HexCell neighbourCell = gridCells[GetCellIndex(cell.Coordinates.GlobalX, zCellCount * zChunkCount - 3)];

            if (UnityEngine.Random.value < 0.8f || (cell.Coordinates.GlobalZ == (zCellCount * zChunkCount - 2) && !neighbourCell.IsOcean))
            {
                SetArcticCell(cell);
            }
        }
        else if (cell.Coordinates.GlobalZ == 2 || cell.Coordinates.GlobalZ == (zCellCount * zChunkCount - 3))
        {
            if (UnityEngine.Random.value < 0.4f)
            {
                HexCell neighbourCell = gridCells[GetCellIndex(cell.Coordinates.GlobalX, 1)];

                if ((cell.Coordinates.GlobalZ == 2 && !neighbourCell.IsOcean) || cell.Coordinates.GlobalZ == (zCellCount * zChunkCount - 3))
                {
                    SetArcticCell(cell);
                }
            }
        }
    }

    // Arctic cell data
    private void SetArcticCell(HexCell cell)
    {
        cell.IsOcean = false;
        cell.IsArctic = true;
        cell.SetTerrainLevel(2);
        SetTemperature(cell);
    }

    // Heigh based on distance to ocean
    private int CalculateDictanceToOceanHeight(HexCell cell)
    {
        return (int)(cell.DistanceFromOcean * distanceWeight * HexData.oceanDistanceLevelStep);
    }

    // Highlands noise
    private int SetDistanceNoise(HexCell cell)
    {
        float distanceNoise = Mathf.PerlinNoise(
            cell.Coordinates.globalX * perlinLargeTerrainScale + 1000f,
            cell.Coordinates.globalZ * perlinLargeTerrainScale + 1000f
        );

        distanceNoise *= distanceNoiseTerrainAmp * HexData.oceanDistanceLevelStep;

        return (int)(distanceNoise);
    }

    // Hill noise
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

    // Mountains noise
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

    // MOisture for calculate climate
    private void CalculateMoisture(HexCell[] gridCells)
    {
        SetWind();
        ApplyOceanMoisture(gridCells);
        ApplyWind(gridCells);
        ApplyMountainsDry(gridCells);
        ApplyMountainsRain(gridCells);
    }

    // Random wind direction
    private void SetWind()
    {
        windDirection = (HexDirection)UnityEngine.Random.Range(0, 6);
    }

    // Moisture based on ocean distance
    private void ApplyOceanMoisture(HexCell[] gridCells)
    {
        foreach (var cell in gridCells)
        {
            if (!cell.IsOcean)
            {
                cell.Moisture = Mathf.Max(0f, 1f - 0.1f * cell.DistanceFromOcean);
            }
        }
    }

    // Applies wind-based moisture transport 
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

    // Finds the upwind moisture source 
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

    // Applies drying effect caused by mountains on the downwind side
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

    // Applies increased rainfall on the windward side of mountains
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

    // Set climate
    private void SetBiome(HexCell[] gridCells, int biomeLayerIndex)
    {
        foreach (var cell in gridCells)
        {
            if (cell.IsOcean)
            {
                cell.SetBiome(Biome.Ocean);
                continue;
            }
            if (cell.IsMountain)
            {
                cell.SetBiome(Biome.Mountain);
                continue;
            }

            AddPotencionalRiverSource(cell);

            Biome baseBiome;

            if (cell.Temperature < coldMax)
                baseBiome = Biome.Tundra;
            else if (cell.Temperature < moderateTempMax)
            {
                if (cell.Moisture < dryMax)
                    baseBiome = Biome.Grassland;
                else if (cell.Moisture < moderateMoistureMax)
                    baseBiome = Biome.ContinentalDry;
                else
                    baseBiome = Biome.ContinentalWet;
            }
            else
            {
                if (cell.Moisture < dryMax)
                    baseBiome = Biome.Desert;
                else if (cell.Moisture < moderateMoistureMax)
                    baseBiome = Biome.Savanna;
                else
                    baseBiome = Biome.RainForest;
            }

            cell.SetBiome(GetBiome(baseBiome, biomeLayerIndex));
        }
    }

    // Climate based on layer
    private Biome GetBiome(Biome baseBiome, int layer)
    {
        int baseIndex = (int)baseBiome - 2;

        if (baseIndex < 0 || baseIndex >= biomesPerLayer)
            return baseBiome;

        return (Biome)(2 + baseIndex + layer * biomesPerLayer);
    }

    // Correct biomes
    private void SmoothBiomes(HexCell[] gridCells)
    {
        Dictionary<HexCell, Biome> newBiomes = new Dictionary<HexCell, Biome>();

        foreach (var cell in gridCells)
        {
            if (cell.IsOcean || cell.IsMountain || cell.IsLake)
                continue;

            Biome currentBiome = cell.GetBiome();

            bool hasSameBiomeNeighbour = false;
            Dictionary<Biome, int> biomeCount = new Dictionary<Biome, int>();

            for (int j = 0; j < 6; j++)
            {
                HexCell neighbour = cell.GetNeighbor((HexDirection)j);
                if (neighbour == null)
                    continue;

                if (neighbour.IsOcean || neighbour.IsMountain || neighbour.IsLake)
                    continue;

                Biome neighbourBiome = neighbour.GetBiome();

                if (neighbourBiome == currentBiome)
                {
                    hasSameBiomeNeighbour = true;
                    break;
                }

                if (!biomeCount.ContainsKey(neighbourBiome))
                    biomeCount[neighbourBiome] = 0;

                biomeCount[neighbourBiome]++;
            }

            if (hasSameBiomeNeighbour || biomeCount.Count == 0)
                continue;

            Biome mostCommonBiome = currentBiome;
            int maxCount = 0;

            foreach (var kvp in biomeCount)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    mostCommonBiome = kvp.Key;
                }
            }

            newBiomes[cell] = mostCommonBiome;
        }

        foreach (var kvp in newBiomes)
        {
            kvp.Key.SetBiome(kvp.Value);
        }
    }

    // Potenciona river source based on terrain level nad moisture
    private void AddPotencionalRiverSource(HexCell cell)
    {
        if (cell.TerrainLevelIndex >= riverSuorceMinLevel && cell.Moisture >= riverSuorceMinMoisture && cell.Temperature > coldMax && cell.Temperature < moderateTempMax)
        {
            float heightWeight = Normalize(cell.TerrainLevelIndex, riverSuorceMinLevel, 6f);
            float moistureWeight = Normalize(cell.Moisture, riverSuorceMinMoisture, 1f);

            float finalWeight = (heightWeight * 0.6f) + (moistureWeight * 0.4f);

            int repetitions = 0;

            if (finalWeight > 0.75f)
                repetitions = 3; 
            else if (finalWeight > 0.5f)
                repetitions = 2; 
            else if (finalWeight > 0.25f)
                repetitions = 1;

            for (int i = 0; i < repetitions; i++)
            {
                potencionalRiverSources.Add(cell);
            }
        }
    }

    private float Normalize(float value, float min, float max)
    {
        return Mathf.Clamp01((value - min) / (max - min));
    }

    // Detects and assigns edge types for all cells in the grid
    private void DetectEdgeType(HexCell[] gridCells)
    {
        for (int i = 0; i < gridCells.Length; i++)
        {
            HexCell cell = gridCells[i];

            ChcekNeighbourEdges(cell);
        }
    }

    // Determines edge type between a cell and each of its neighbours
    private void ChcekNeighbourEdges(HexCell cell)
    {
        for (int j = 0; j < 6; j++)
        {
            HexCell neighbourCell = cell.GetNeighbor((HexDirection)j);

            EdgeType type;

            if (neighbourCell == null)
            {
                type = EdgeType.None;
            }
            else
            {
                int diff = Mathf.Abs(
                    cell.TerrainLevelIndex - neighbourCell.TerrainLevelIndex
                );

                if (cell.IsMountain || neighbourCell.IsMountain)
                    type = EdgeType.Mountain;
                else if (diff == 0)
                    type = EdgeType.Flat;
                else if (diff == 1)
                    type = EdgeType.Smooth;
                else
                    type = EdgeType.Cliff;
            }

            cell.UpdateEdge(j, type);
        }
    }

    // Collects all edges marked as Smooth
    private List<Edge> FindSmoothEdges(HexCell[] gridCells)
    {
        List<Edge> smoothEdges = new List<Edge>();

        foreach (HexCell cell in gridCells)
        {
            for (int i = 0; i < 6; i++)
            {
                 Edge edge = cell.GetEdge(i);

                if (edge == null) 
                    continue;

                if (edge.GetEdgeType() == EdgeType.Smooth)
                {
                    bool oceanEdge = cell.IsOcean || (cell.GetNeighbor((HexDirection)i) != null && cell.GetNeighbor((HexDirection)i).IsOcean);

                    edge.SetOceanEdge(oceanEdge);
                    smoothEdges.Add(edge);
                }
            }
        }

        return smoothEdges;
    }

    // Groups connected smooth edges into continuous chains and smooths them
    private void GroupSmoothEdges(HexCell[] gridCells)
    {
        List<List<Edge>> groupsSmoothEdges = new List<List<Edge>>();
        List<bool> groupTouchesOcean = new List<bool>();
        HashSet<Edge> used = new HashSet<Edge>();

        var allSmoothEdges = FindSmoothEdges(gridCells);

        foreach (var e in allSmoothEdges)
        {
            if (used.Contains(e))
                continue;

            List<Edge> chain = new List<Edge>();
            bool touchesOcean = e.IsOceanEdge;
            chain.Add(e);
            used.Add(e);

            // Extend chain forward
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

                        if (other.IsOceanEdge)
                            touchesOcean = true;

                        used.Add(other);
                        end = other.GetFullV2();
                        extended = true;
                        break;
                    }
                    else if (SamePoint(end, other.GetFullV2()))
                    {
                        chain.Add(other);

                        if (other.IsOceanEdge)
                            touchesOcean = true;

                        used.Add(other);
                        end = other.GetFullV1();
                        extended = true;
                        break;
                    }
                }
            }

            // Extend chain forward
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

                        if (other.IsOceanEdge)
                            touchesOcean = true;

                        used.Add(other);
                        start = other.GetFullV1();
                        extended = true;
                        break;
                    }
                    else if (SamePoint(start, other.GetFullV1()))
                    {
                        chain.Insert(0, other);

                        if (other.IsOceanEdge)
                            touchesOcean = true;

                        used.Add(other);
                        start = other.GetFullV2();
                        extended = true;
                        break;
                    }
                }
            }

            groupsSmoothEdges.Add(chain);
            groupTouchesOcean.Add(touchesOcean);
        }

        // Smooth all detected chains
        SmoothEdges(groupsSmoothEdges, groupTouchesOcean);
    }

    private bool SamePoint(Vector3 a, Vector3 b)
    {
        return (a - b).sqrMagnitude < 0.0001f;
    }

    // Applies Chaikin smoothing to groups of edges
    private void SmoothEdges(List<List<Edge>> groupsSmoothEdges, List<bool> groupTouchesOcean)
    {
        for (int i = 0; i < groupsSmoothEdges.Count; i++)
        {
            var group = groupsSmoothEdges[i];

            if (group.Count <= 1) continue;

            List<Vector3> vertices = FindChainStart(group);

            bool isLoop = IsLoop(vertices);

            // Remove duplicate vertices to avoid over-smoothing corners
            List<int> map;
            List<Vector3> unique = RemoveDuplicates(vertices, out map);

            // Ocean edges get more smoothing
            int iterations = groupTouchesOcean[i] ? 3 : 1;   
            List<Vector3> smoothUnique = ChaikinSmoothSameCount(unique, isLoop, iterations);

            List<Vector3> result = ReapplyDuplicates(smoothUnique, map);

            WriteBackToEdges(group, result);
        }
    }

    // Builds an ordered list of vertices from an edge chain
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

    // Checks if the chain forms a closed loop
    private bool IsLoop(List<Vector3> v)
    {
        return SamePoint(v[0], v[v.Count - 1]);
    }

    // Removes duplicate vertices and builds a mapping to unique list
    private List<Vector3> RemoveDuplicates(List<Vector3> baseVertices, out List<int> mapToUnique)
    {
        mapToUnique = new List<int>();
        List<Vector3> unique = new List<Vector3>();

        for (int i = 0; i < baseVertices.Count; i++)
        {
            bool found = false;
            for (int j = 0; j < unique.Count; j++)
            {
                if (SamePoint(baseVertices[i], unique[j]))
                {
                    mapToUnique.Add(j);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                mapToUnique.Add(unique.Count);
                unique.Add(baseVertices[i]);
            }
        }

        return unique;
    }

    // Smooths vertices
    private List<Vector3> ChaikinSmoothSameCount(List<Vector3> pts, bool loop, int iterations)
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


    // Resamples (it contains targetCount points)
    // If loop == true, the line is treated as closed
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

    // Restores duplicate vertices after smoothing unique points
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

    // Writes smoothed vertex positions back to the corresponding edges
    private void WriteBackToEdges(List<Edge> edges, List<Vector3> newPts)
    {
        int p = 0;

        for (int i = 0; i < edges.Count; i++)
        {
            edges[i].SetGlobalV1(newPts[p++]);
            edges[i].SetGlobalV2(newPts[p++]);
        }
    }

    // Creates rivers for each continent independently
    private void CreateRivers()
    {
        var sourcesByContinent = GroupSourcesByContinent();

        foreach (var group in sourcesByContinent)
        {
            List<HexCell> sources = group.Value;

            if (sources.Count == 0)
                continue;

            int riverCount = UnityEngine.Random.Range(riversPerContinentMin, Mathf.Min(riversPerContinentMax, sources.Count));

            int safety = 0;
            int maxAttempts = sources.Count * 2;

            while (riverCount > 0 && safety < maxAttempts)
            {
                safety++;

                HexCell source = sources[UnityEngine.Random.Range(0, sources.Count)];

                if (source.IsRiver)
                    continue;

                if (HasRiverSourceNeighbour(source))
                    continue;

                if (CreateRiverFromSource(source))
                    riverCount--;
            }
        }
    }

    // Creates rivers for each continent independently
    private bool CreateRiverFromSource(HexCell startCell)
    {
        HashSet<HexCell> visited = new HashSet<HexCell>();

        List<HexCell> riverPath = new List<HexCell>();
        riverPath.Add(startCell);

        HexCell cell = startCell;

        int safety = 0;
        int maxLength = 200;

        bool mergedIntoRiver = false;
        bool endedInOcean = false;

        while (safety < maxLength)
        {
            safety++;

            if (cell.IsOcean || cell.IsLake)
            {
                endedInOcean = true;
                break;
            }

            HexCell neighbour = GetNextRiverCell(cell, visited);

            if (neighbour == null)
            {
                break;
            }

            if (visited.Contains(neighbour))
            {
                int loopIndex = riverPath.IndexOf(neighbour);
                riverPath.RemoveRange(loopIndex + 1, riverPath.Count - loopIndex - 1);
                break;
            }

            if (neighbour.IsRiver)
            {
                riverPath.Add(neighbour);
                mergedIntoRiver = true;
                endedInOcean = true;
                break;
            }

            visited.Add(neighbour);
            riverPath.Add(neighbour);
            cell = neighbour;
        }

        if (riverPath.Count < 4)
            return false;

        River river = new River();

        for (int i = 0; i < riverPath.Count - 1; i++)
        {
            HexCell from = riverPath[i];
            HexCell to = riverPath[i + 1];

            if (mergedIntoRiver && i == riverPath.Count - 2)
            {
                ConnectIntoExistingRiver(from, to);
            }
            else
            {
                SetRiverEdge(from, to);
            }

            from.IsRiver = true;
            from.AddRiver(river);
            river.AddCell(from);
        }

        HexCell lastCell = riverPath[riverPath.Count-1];

        lastCell.IsRiver = true;
        lastCell.AddRiver(river);
        river.AddCell(lastCell);

        if (!endedInOcean && !mergedIntoRiver)
        {
            CreateLakes(lastCell);
        }

        return true;
    }

    // Selects the next river cell based on downhill flow and proximity to ocean
    private HexCell GetNextRiverCell(HexCell cell, HashSet<HexCell> visited)
    {
        List<HexCell> candidates = new List<HexCell>();

        for (int i = 0; i < 6; i++)
        {
            HexCell neighbour = cell.GetNeighbor((HexDirection)i);
            if (neighbour == null)
                continue;
            if (neighbour.TerrainLevelIndex > cell.TerrainLevelIndex)
                continue;
            if (neighbour.IsMountain)
                continue;
            if (visited.Contains(neighbour))
                continue;

            candidates.Add(neighbour);
        }

        if (candidates.Count == 0)
            return null;

        candidates.Sort((a, b) =>
        {
            int oceanCompare = a.DistanceFromOcean.CompareTo(b.DistanceFromOcean);
            if (oceanCompare != 0)
                return oceanCompare;

            return a.TerrainLevelIndex.CompareTo(b.TerrainLevelIndex);
        });

        int bestCount = Mathf.Min(2, candidates.Count);
        return candidates[UnityEngine.Random.Range(0, bestCount)];
    }

    // Groups potential river source cells by continent index
    private Dictionary<int, List<HexCell>> GroupSourcesByContinent()
    {
        Dictionary<int, List<HexCell>> grouped = new Dictionary<int, List<HexCell>>();

        foreach (HexCell cell in potencionalRiverSources)
        {
            int continent = cell.ContinentIndex;

            if (!grouped.ContainsKey(continent))
                grouped[continent] = new List<HexCell>();

            grouped[continent].Add(cell);
        }

        return grouped;
    }

    // Checks whether any neighboring cell already contains a river
    private bool HasRiverSourceNeighbour(HexCell cell)
    {
        for (int i = 0; i < 6; i++)
        {
            HexCell neighbour = cell.GetNeighbor((HexDirection)i);

            if (neighbour == null)
                continue;

            if (neighbour.IsRiver)
                return true;
        }

        return false;
    }

    // Checks whether any neighboring cell already contains a river
    private void SetRiverEdge(HexCell from, HexCell to)
    {
        int dir = GetDirectionIndex(from, to);
        if (dir == -1)
            return;

        Edge outEdge = from.GetEdge(dir);
        Edge inEdge = to.GetEdge((dir + 3) % 6);

        if (outEdge.InRiver || outEdge.OutRiver)
            return;

        if (inEdge.InRiver || inEdge.OutRiver)
            return;

        outEdge.OutRiver = true;
        inEdge.InRiver = true;
    }

    // Returns the hex direction index from one cell to another
    private int GetDirectionIndex(HexCell from, HexCell to)
    {
        for (int i = 0; i < 6; i++)
        {
            if (from.GetNeighbor((HexDirection)i) == to)
                return i;
        }
        return -1;
    }

    // Connects a newly generated river into an existing river system
    private void ConnectIntoExistingRiver(HexCell from, HexCell to)
    {
        int dir = GetDirectionIndex(from, to);
        if (dir == -1)
            return;

        Edge fromOut = from.GetEdge(dir);
        Edge toIn = to.GetEdge((dir + 3) % 6);

        fromOut.OutRiver = true;

        toIn.InRiver = true;

        for (int i = 0; i < 6; i++)
        {
            if (to.GetEdge(i).OutRiver)
            {
                fromOut.OutRiver = true;
                break;
            }
        }
    }

    // Correct neighbour edges
    private void SetNeighbourEdges(HexCell cell, int terrainLevel)
    {
        cell.SetTerrainLevel(terrainLevel);

        ChcekNeighbourEdges(cell);

        for (int i = 0; i < 6; i++)
        {
            HexCell neighbour = cell.GetNeighbor((HexDirection)i);

            if (neighbour == null)
                continue;

            ChcekNeighbourEdges(neighbour);
        }
    }

    // Create lake if river did not find ocean
    private void CreateLakes(HexCell lake)
    {
        int lakeTerrainIndex = lake.TerrainLevelIndex;

        bool hasOutflow = false;

        for (int i = 0; i < 6; i++)
        {
            HexCell neighbour = lake.GetNeighbor((HexDirection)i);

            if (neighbour == null)
                continue;

            if (neighbour.TerrainLevelIndex <= lakeTerrainIndex)
            {
                hasOutflow = true;
                lakeTerrainIndex = neighbour.TerrainLevelIndex;
            }

            if(neighbour.IsOcean)
            {
                lake.IsOcean = true;
                SetNeighbourEdges(lake, neighbour.TerrainLevelIndex);
                break;
            }
        }

        lake.IsLake = true;

        if (!hasOutflow)
        {
            return;
        }
        else
        {
            SetNeighbourEdges(lake, lakeTerrainIndex - 1);
        }
    }

    // Oceant deep and coast deep
    private void SetOceanDeep(HexCell[] gridCells)
    {
        foreach(var cell in gridCells)
        {
            if (cell.IsOcean)
            {
                bool coast = false;

                for (int i = 0; i < 6; i++)
                {
                    HexCell neighbour = cell.GetNeighbor((HexDirection)i);

                    if (neighbour == null)
                        continue;

                    if(!neighbour.IsOcean && neighbour.TerrainLevelIndex==0)
                    {
                        coast = true;
                        break;
                    }                  
                }

                if (coast)
                {
                    SetNeighbourEdges(cell, -1);
                }
            }
        }
    }

    // Set random structure on cell based on climate
    private void RandomStructures(HexCell[] gridCells)
    {
        foreach (var cell in gridCells)
        {
            if(!cell.IsOcean && !cell.IsMountain && !cell.IsLake && !cell.IsRiver)
            {
                bool noNeighbourStructure = true;

                for (int i = 0; i < 6; i++)
                {
                    HexCell neighbour = cell.GetNeighbor((HexDirection)i);

                    if (neighbour == null)
                        continue;

                    if (neighbour.StructureIndex != -1)
                    {
                        noNeighbourStructure = false;
                        break;
                    }
                }

                if (!noNeighbourStructure)
                    continue;

                if (UnityEngine.Random.Range(0, 100) >= structureChance)
                    continue;;

                GameObject[] objects = GetObjects(cell.GetBiome());
                if (objects == null)
                    continue;

                int structures = objects.Length;

                if (structures <= 0)
                    continue;

                cell.StructureIndex = UnityEngine.Random.Range(0, structures);
            }
        }
    }

    // Unit on random cell
    private void RandomUnits(HexCell[] gridCells)
    {
        List<HexCell> potentialCells = new List<HexCell>();

        foreach (var cell in gridCells)
        {
            if (cell.IsOcean || cell.IsMountain || cell.IsLake || cell.IsRiver || cell.StructureIndex != -1 || cell.Temperature < coldMax || cell.IsUnit) 
                continue;

            potentialCells.Add(cell);
        }

        int index = UnityEngine.Random.Range(0, potentialCells.Count);
        potentialCells[index].IsUnit = true;
    }

    // Cell UI pos based on layer
    private void SetTextPos(HexCell[] gridCells, int biomeLayerIndex)
    {
        foreach (var cell in gridCells)
        {
            cell.LayerIndex = biomeLayerIndex;
            cell.SetCellUIPos();
        }
    }
}
