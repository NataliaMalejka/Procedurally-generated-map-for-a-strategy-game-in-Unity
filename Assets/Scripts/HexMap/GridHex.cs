using System;
using System.Linq;
using UnityEngine;

public class GridHex : MonoBehaviour
{
    [SerializeField] private Chunk chunkPrefab;
    private Chunk[] chunks;
    private int xChunks;
    private int zChunks;

    public HexCell[] cellsEarth { get; private set; }
    public HexCell[] cellsCold { get; private set; }
    public HexCell[] cellsHot { get; private set; }

    private void Start()
    {
        xChunks = MapManager.Instance.xChunkCount;
        zChunks = MapManager.Instance.zChunkCount;

        int index = 0;

        if (GameSettings.Instance.IsHotBiome())
        {
            CreateChunks(index, cellsHot);
            index++;
        }
        if(GameSettings.Instance.IsEarthBiome())
        {
            CreateChunks(index, cellsEarth);
            index++;
        }
        if (GameSettings.Instance.IsColdBiome())
        {
            CreateChunks(index, cellsCold);
        }
        if(!GameSettings.Instance.IsHotBiome() && !GameSettings.Instance.IsEarthBiome() && !GameSettings.Instance.IsColdBiome())
        {
            CreateChunks(index, cellsEarth);
        }       
    }

    private void CreateChunks(int level, HexCell[] cells)
    {
        chunks = new Chunk[xChunks * zChunks];
        cells = new HexCell[xChunks * zChunks * MapManager.Instance.xCellCount * MapManager.Instance.zCellCount];

        int index = 0;

        for (int x = 0; x < xChunks; x++) 
        {
            for (int z = 0; z < zChunks; z++) 
            {            
                Chunk chunk = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
                chunks[index] = chunk;
                SetChunkPosition(chunk, index, level);
                chunk.SetLevel(level);
                chunk.SetGridCoords(x, z, index);

                AddCells(chunk, index, cells);
                index++;
            }
        }

        MapManager.Instance.GenerateMap(cells);

        foreach (Chunk chunk in chunks)
        {
            chunk.RefreshChunk();
        }
    }

    private void SetChunkPosition(Chunk chunk, int index, int level)
    {
        Vector3 position;

        position.x = (index / zChunks) * HexData.distanceToEdge * 2 * chunk.GetXCellCount();
        position.y = level * 1000f;
        position.z = (index % zChunks) * HexData.distanceToCorner * 1.5f * chunk.GetZCellCount();

        chunk.transform.position = position;
    }

    private void AddCells(Chunk chunk, int index, HexCell[] cells)
    {
        int cellIndex = 0;

        for (int x = 0; x < chunk.GetXCellCount(); x++)
        {
            for (int z = 0; z < chunk.GetZCellCount(); z++)
            {
                int cellGlobalIndex = cellIndex + index * chunk.GetZCellCount() * chunk.GetXCellCount();
            
                HexCell cell = chunk.CreateCell(cellGlobalIndex, index);
                cells[cellGlobalIndex] = cell;

                SetCellCoordinates(cell, x, z, index, chunk);
                SetCellNeighbors(cellGlobalIndex, cell, cells);

                cellIndex++;
            }
        }
    }

    private void SetCellCoordinates(HexCell cell, int x, int z, int index, Chunk chunk)
    {
        cell.Coordinates = new HexCoordinates(x, z,
            (index / zChunks) * chunk.GetXCellCount() + x,
            (index % zChunks) * chunk.GetZCellCount() + z
        );

        //cell.SetCoordinateText();
        //cell.SetGlobalCoordinateText();
    }

    private void SetCellNeighbors(int index, HexCell cell, HexCell[] cells)
    {
        foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection))) 
        {
            Vector3Int neighborCoordinates = cell.Coordinates.Neighbor(dir);
            int nq = neighborCoordinates[0];
            int nr = neighborCoordinates[1];
            int ns = neighborCoordinates[2];

            int indexZ = nr;
            int indexX = nq + indexZ / 2;

            int neighborIndex = MapManager.Instance.GetCellIndex(indexX, indexZ);

            if (neighborIndex >= 0 && neighborIndex<cells.Count() && cells[neighborIndex] != null && indexX >= 0 && indexZ >= 0) 
            {
               cell.SetNeighbor(dir, cells[neighborIndex]);
            }
        }     
    }
}
