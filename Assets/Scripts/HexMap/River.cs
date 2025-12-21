using System.Collections.Generic;
using UnityEngine;

public class River : MonoBehaviour
{
    private List<HexCell> riverCells = new List<HexCell>();

    public HexCell Source => riverCells.Count > 0 ? riverCells[0] : null;
    public HexCell End => riverCells.Count > 0 ? riverCells[riverCells.Count-1] : null;

    public List<HexCell> GetCells()
    {
        return riverCells;
    }

    public void AddCell(HexCell cell)
    {
        if (riverCells.Count == 0 || riverCells[riverCells.Count - 1] != cell)
            riverCells.Add(cell);
    }

    public bool AreNeighboursInRiver(HexCell a, HexCell b)
    {
        int indexA = riverCells.IndexOf(a);
        int indexB = riverCells.IndexOf(b);

        if (indexA == -1 || indexB == -1)
            return false;

        return Mathf.Abs(indexA - indexB) == 1;
    }
}
