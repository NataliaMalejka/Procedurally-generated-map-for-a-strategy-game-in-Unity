using System.Collections.Generic;

// Represents a continent 
public class Continent
{
    // Unique index identifying the continent
    private int continetIndex = -1;
    public int ContinetIndex
    {
        get { return continetIndex; }
        set { continetIndex = value; }
    }

    // All hex cells belonging to this continent
    private List<HexCell> continentCells = new List<HexCell>();
    // All chunks that contain cells of this continent
    private List<Chunk> continentChunks = new List<Chunk>();

    // Adds a hex cell to the continent 
    public void AddCell(HexCell cell)
    {
        if (!continentCells.Contains(cell))
        {
            continentCells.Add(cell);
        }
    }

    // Returns all hex cells belonging to this continent
    public List<HexCell> GetContinentCells()
    {
        return continentCells;
    }

    // Adds a chunk to the continent 
    public void AddChunk(Chunk chunk)
    {
        if (!continentChunks.Contains(chunk))
        {
            continentChunks.Add(chunk);
        }
    }

    // Returns all chunks associated with this continent
    public List<Chunk> GetContinentChunks()
    {
        return continentChunks;
    }

}
