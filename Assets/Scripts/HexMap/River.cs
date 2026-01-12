using System.Collections.Generic;
using UnityEngine;

public class River : MonoBehaviour
{
    private List<HexCell> riverCells = new List<HexCell>();

    public void AddCell(HexCell cell)
    {
        if (riverCells.Count == 0 || riverCells[riverCells.Count - 1] != cell)
            riverCells.Add(cell);
    }

    public bool AreNeighboursInRiver(HexCell a, HexCell b)
    {
        int dir = HexDirectionUtils.GetDirectionIndex(a, b);
        if (dir == -1) return false;

        Edge ea = a.GetEdge(dir);
        Edge eb = b.GetEdge((dir + 3) % 6);

        return (ea.OutRiver && eb.InRiver) || (ea.InRiver && eb.OutRiver);
    }
}
