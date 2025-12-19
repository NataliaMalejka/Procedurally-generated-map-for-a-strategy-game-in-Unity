using TMPro;
using UnityEngine;

public enum Biome
{
    Ocean,
    Mountain,
    Tundra,
    Grassland,
    ContinentalDry,
    continentalWet,
    RainForest,
    Savanna,
    Desert
}

public class HexCell : MonoBehaviour
{
    [SerializeField] private TextMeshPro coordinateText;
    [SerializeField] private Edge edgePrefab;

    private Chunk hexChunk;
    public Chunk HexChunk
    {
        get { return hexChunk; }
        set { hexChunk = value; }
    }

    private HexCoordinates coordinates;
    public HexCoordinates Coordinates
    {
        get { return coordinates; }
        set { coordinates = value; }
    }

    private Biome cellBiome;

    private Color[] colors = new Color[] { Color.turquoise, Color.black, Color.white, Color.yellowGreen, Color.limeGreen, Color.forestGreen, Color.darkGreen, Color.orange, Color.yellow, Color.pink};

    private Color cellColor = Color.turquoise;
    public Color CellColor
    {
        get { return cellColor; }
        set { cellColor = value; }
    }

    private int textureIndex = -1;
    public int TextureIndex
    {
        get { return textureIndex; }
        set { textureIndex = value; }
    }

    private HexCell[] neighbors = new HexCell[6];
    private Edge[] edges = new Edge[6];

    private bool ocean = false;
    public bool isOcean
    {
        get { return ocean; }
        set { ocean = value; }
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

    private int terrainLevel = -1; 
    public int TerrainLevel
    {
        get { return terrainLevel; }
        set { terrainLevel = value; }
    }

    private float centreTerrainlevel = -1;
    public float CentreTerrainLevel
    {
        get { return centreTerrainlevel; }
        set { centreTerrainlevel = value; }
    }

    private int terrainLevelIndex = -1;
    public int TerrainLevelIndex
    {
        get { return terrainLevelIndex; }
        set { terrainLevelIndex = value; }
    }

    private bool isMountain = false;
    public bool IsMountain
    {
        get { return isMountain; }
        set { isMountain = value; }
    }

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

    public void AddEdge(EdgeType edgeType, HexDirection direction)
    {
        Edge edge = Instantiate<Edge>(edgePrefab);
        edge.SetData(edgeType, direction, terrainLevel, transform.localPosition, hexChunk.transform.position, this);
        edges[(int)direction] = edge;
        edge.transform.SetParent(transform);
    }

    public Edge GetEdge(int index)
    {
        return edges[index];
    }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public HexCell[] GetNeighbors()
    {
        return neighbors;
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public void SetContinent(int index)
    {
        continentIndex = index;
    }

    public void SetTerrainLevel(int level)
    {
        if (level >= colors.Length)
           level = colors.Length - 1;

        if(level < 0)
            level = 0;

        terrainLevelIndex = level;
        terrainLevel = terrainLevelIndex * HexData.levelStepHeight;
        centreTerrainlevel = terrainLevel;
    }

    public void SetBiome(Biome biome)
    {
        cellBiome = biome;
        cellColor = colors[(int)cellBiome];
        textureIndex = (int)cellBiome;
    }

    public void SetCoordinateText()
    {
        coordinateText.text = coordinates.Q.ToString() + "\n" + coordinates.R.ToString() + "\n" + coordinates.S.ToString();
    }

    public void SetGlobalCoordinateText()
    {
        coordinateText.text = coordinates.GlobalX.ToString() + "\n" + coordinates.GlobalZ.ToString() + "\n" + coordinates.IndexInGrid;
    }
}
