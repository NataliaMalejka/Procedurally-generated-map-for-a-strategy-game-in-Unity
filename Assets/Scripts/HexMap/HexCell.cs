using TMPro;
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

    private Color cellColor;
    public Color CellColor
    {
        get { return cellColor; }
        set { cellColor = value; }
    }

    private HexCell[] neighbors;

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public void SetCoordinateText()
    {
        coordinateText.text = coordinates.GlobalX.ToString() + ", " + coordinates.GlobalZ.ToString();
    }
}
