using System.Collections.Generic;
using UnityEngine;

// Defines all biome types used by hex cells
// Prefixes:
// H = Hot biome
// C = Cold biome
public enum Biome
{
    Ocean,
    Mountain,

    Tundra,
    Grassland,
    ContinentalDry,
    ContinentalWet,
    RainForest,
    Savanna,
    Desert,

    HTundra,
    HGrassland,
    HContinentalDry,
    HContinentalWet,
    HRainForest,
    HSavanna,
    HDesert,

    CTundra,
    CGrassland,
    CContinentalDry,
    CContinentalWet,
    CRainForest,
    CSavanna,
    CDesert
}

// Represents a single hex tile in the map
public class HexCell : MonoBehaviour
{
    // Chunk this hex belongs to
    private Chunk hexChunk;
    public Chunk HexChunk
    {
        get { return hexChunk; }
        set { hexChunk = value; }
    }

    // Cube/grid coordinates of the hex
    private HexCoordinates coordinates;
    public HexCoordinates Coordinates
    {
        get { return coordinates; }
        set { coordinates = value; }
    }

    // Layer index
    private int layerIndex = -1;
    public int LayerIndex
    {
        get { return layerIndex; }
        set { layerIndex = value; }
    }

    // Offset for UI elements above the hex
    private Vector3 uiPos = Vector3.zero;
    public Vector3 UiPos
    {
        get { return uiPos; }
    }

    // Pathfinding marker displayed on this cell
    private PathMarker marker = null;
    public PathMarker Marker
    {
        get { return marker; }
        set { marker = value; }
    }

    // Biome assigned to this cell
    private Biome cellBiome;

    // Texture index for rendering
    private int textureIndex = -1;
    public int TextureIndex
    {
        get { return textureIndex; }
        set { textureIndex = value; }
    }

    // Neighboring cells 
    private HexCell[] neighbors = new HexCell[6];
    // Edge data 
    private Edge[] edges = new Edge[6];

    // Flags describing terrain type
    private bool isOcean = false;
    public bool IsOcean
    {
        get { return isOcean; }
        set { isOcean = value; }
    }

    private int distanceFromOcean = -1;
    public int DistanceFromOcean
    {
        get { return distanceFromOcean; }
        set { distanceFromOcean = value; }
    }

    private int continentIndex = -1;
    public int ContinentIndex
    {
        get { return continentIndex; }
        set { continentIndex = value; }
    }

    // Terrain height data
    private int terrainLevel = -4;
    public int TerrainLevel
    {
        get { return terrainLevel; }
    }

    private float centreTerrainlevel = -4;
    public float CentreTerrainLevel
    {
        get { return centreTerrainlevel; }
        set { centreTerrainlevel = value; }
    }

    private int terrainLevelIndex = -2;
    public int TerrainLevelIndex
    {
        get { return terrainLevelIndex; }
    }

    // Structure placed on this cell
    private int structureIndex = -1;
    public int StructureIndex
    {
        get { return structureIndex; }
        set { structureIndex = value; }
    }

    // Gameplay flags
    private bool isUnit = false;
    public bool IsUnit
    {
        get { return isUnit; }
        set { isUnit = value; }
    }

    private bool isMountain = false;
    public bool IsMountain
    {
        get { return isMountain; }
        set { isMountain = value; }
    }

    private bool isArctic = false;
    public bool IsArctic
    {
        get { return isArctic; }
        set { isArctic = value; }
    }

    // Climate simulation values
    private float temperature = 0;
    public float Temperature
    {
        get { return temperature; }
        set { temperature = value; }
    }

    private float moisture = 0;
    public float Moisture
    {
        get { return moisture; }
        set { moisture = value; }
    }

    // Water features
    private bool isLake = false;
    public bool IsLake
    {
        get { return isLake; }
        set { isLake = value; }
    }

    private bool isRiver = false;
    public bool IsRiver
    {
        get { return isRiver; }
        set { isRiver = value; }
    }

    // Rivers passing through this cell
    private List<River> rivers = new List<River>();

    public List<River> GetRivers()
    {
        return rivers;
    }

    // Adds a river reference to this cell
    public void AddRiver(River river)
    {
        if (!rivers.Contains(river))
            rivers.Add(river);
    }

    // Creates a new edge 
    public void AddEdge(EdgeType edgeType, HexDirection direction)
    {
        Edge edge = new Edge();
        edge.SetData(edgeType, direction, terrainLevel, transform.localPosition, hexChunk.transform.position, this);
        edges[(int)direction] = edge;
    }

    // Updates an existing edge or creates it if missing
    public void UpdateEdge(int index, EdgeType type)
    {
        Edge edge = edges[index];
        if (edge == null)
        {
            AddEdge(type, (HexDirection)index);
            return;
        }

        edge.SetData(type, (HexDirection)index, terrainLevel, transform.localPosition, hexChunk.transform.position, this);
    }

    public Edge GetEdge(int index)
    {
        return edges[index];
    }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    // Sets a neighbor relationship in both directions
    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public void SetContinent(int index)
    {
        continentIndex = index;
    }

    // Sets terrain height
    public void SetTerrainLevel(int level)
    {
        terrainLevelIndex = level;
        terrainLevel = terrainLevelIndex * HexData.levelStepHeight;
        centreTerrainlevel = terrainLevel;
    }

    // Set biome and texture index
    public void SetBiome(Biome biome)
    {
        cellBiome = biome;
        textureIndex = (int)cellBiome;
    }

    public Biome GetBiome()
    {
        return cellBiome;
    }

    // Calculates vertical offset for UI elements above the cell
    public void SetCellUIPos()
    {
        var pos = Vector3.zero;

        if (IsOcean)
        {
            pos.y = HexData.waterLevel + 3.1f;
        }
        else
            pos.y = centreTerrainlevel + 4.1f;

        uiPos = pos;
    }
}
