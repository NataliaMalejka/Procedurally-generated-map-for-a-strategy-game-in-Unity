using System;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] private TextMeshPro coordinateText;

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

    public HexCell[] neighbors = new HexCell[6];
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

    private int terreinLevel = -1; 
    public int TerreinLevel
    {
        get { return terreinLevel; }
        set { terreinLevel = value; }
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

    public void AddEdge(EdgeType edgeType, HexDirection direction)
    {
        Edge edge = new Edge(edgeType, direction, terreinLevel, transform.localPosition, hexChunk.transform.position);
        edges[(int)direction] = edge;
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
        {
           level = colors.Length - 1;
        }

        if(level < 0)
        {
            level = 0;
        }

        if(level > 7)
        {
            IsMountain = true;
        }

        terrainLevelIndex = level;
        cellColor = colors[terrainLevelIndex];
        terreinLevel = terrainLevelIndex * HexData.levelStepHeight;
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
