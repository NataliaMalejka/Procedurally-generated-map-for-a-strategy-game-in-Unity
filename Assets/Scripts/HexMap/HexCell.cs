using System;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

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

    private Color[] colors = new Color[] { Color.white, Color.yellow, Color.orange, Color.orangeRed, Color.red, Color.hotPink, Color.magenta, Color.darkBlue, Color.gray7, Color.black };

    private Color cellColor = Color.turquoise;
    public Color CellColor
    {
        get { return cellColor; }
        set { cellColor = value; }
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

        if(level > 7)
            IsMountain = true;

        terrainLevelIndex = level;
        cellColor = colors[terrainLevelIndex];
        terrainLevel = terrainLevelIndex * HexData.levelStepHeight;
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
