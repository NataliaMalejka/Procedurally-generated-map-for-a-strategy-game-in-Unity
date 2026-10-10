using System.Collections.Generic;
using UnityEngine;

// Represents a river consisting of connected hex cells
public class River : MonoBehaviour
{
    // Ordered list of hex cells that form the river
    private List<HexCell> riverCells = new List<HexCell>();

    // Adds a hex cell to the river
    // Prevents adding the same cell twice in a row
    public void AddCell(HexCell cell)
    {
        if (riverCells.Count == 0 || riverCells[riverCells.Count - 1] != cell)
            riverCells.Add(cell);
    }

    // Checks if two neighboring hex cells are connected by a river
    public bool AreNeighboursInRiver(HexCell a, HexCell b)
    {
        int dir = HexDirectionUtils.GetDirectionIndex(a, b);
        if (dir == -1) return false;

        Edge ea = a.GetEdge(dir);
        Edge eb = b.GetEdge((dir + 3) % 6);

        return (ea.OutRiver && eb.InRiver) || (ea.InRiver && eb.OutRiver);
    }
}
