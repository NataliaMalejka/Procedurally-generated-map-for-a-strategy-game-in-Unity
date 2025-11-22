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

    private Color[] colors = new Color[] { Color.white, Color.yellow, Color.orange, Color.red, Color.purple, Color.magenta };

    private Color cellColor = Color.turquoise;
    public Color CellColor
    {
        get { return cellColor; }
        set { cellColor = value; }
    }

    public HexCell[] neighbors = new HexCell[6];

    private bool ocean = false;
    public bool isOcean
    {
        get { return ocean; }
        set { ocean = value; }
    }

    private int continentIndex = -1;
    public int ContinentIndex
    {
        get { return continentIndex; }
        set { continentIndex = value; }
    }

    private int terreinLevel = -1; //water
    public int TerreinLevel
    {
        get { return terreinLevel; }
        set { terreinLevel = value; }
    }

    private int[] edges = new int[6];

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
        //cellColor = Color.green;
        //terreinLevel = 0;
        cellColor = colors[terreinLevel];
        continentIndex = index;
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
