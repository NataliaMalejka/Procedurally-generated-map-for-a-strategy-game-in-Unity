using System.Collections.Generic;
using UnityEngine;

public class Continent : MonoBehaviour
{
    private int continetIndex = -1;
    public int ContinetIndex
    {
        get { return continetIndex; }
        set { continetIndex = value; }
    }

    private List<HexCell> continentCells = new List<HexCell>();
    private List<Chunk> continentChunks = new List<Chunk>();

    public void AddCell(HexCell cell)
    {
        if (!continentCells.Contains(cell))
        {
            continentCells.Add(cell);
        }
    }

    public List<HexCell> GetContinentCells()
    {
        return continentCells;
    }

    public void AddChunk(Chunk chunk)
    {
        if (!continentChunks.Contains(chunk))
        {
            continentChunks.Add(chunk);
        }
    }

    public List<Chunk> GetContinentChunks()
    {
        return continentChunks;
    }

}
