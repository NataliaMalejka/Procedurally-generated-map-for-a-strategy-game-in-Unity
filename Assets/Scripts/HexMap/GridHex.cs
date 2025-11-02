using System;
using UnityEngine;


public class GridHex : MonoBehaviour
{
    [SerializeField] private Chunk chunkPrefab;
    private Chunk[] chunks;
    private int xChunks;
    private int zChunks;

    private HexCell[] cells;

    private void Start()
    {
        xChunks = MapManager.Instance.xChunkCount;
        zChunks = MapManager.Instance.zChunkCount;
        CreateChunks();
    }

    private void CreateChunks()
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
                SetChunkPosition(chunk, index);

                chunk.GetChunkMesh().Clear();
                AddCells(chunk, index);
                chunk.GetChunkMesh().Apply();

                index++;
            }
        }
    }
    
    private void SetChunkPosition(Chunk chunk, int index)
    {
        Vector3 position;

        position.x = (index / zChunks) * HexData.distanceToEdge * 2 * chunk.GetXCellCount();
        position.y = 0f;
        position.z = (index % zChunks) * HexData.distanceToCorner * 1.5f * chunk.GetZCellCount();

        chunk.transform.position = position;
    }

    private void AddCells(Chunk chunk, int index)
    {
        int cellIndex = 0;

        for (int x = 0; x < chunk.GetXCellCount(); x++)
        {
            for (int z = 0; z < chunk.GetZCellCount(); z++)
            {
                int cellGlobalIndex = cellIndex + index * chunk.GetZCellCount() * chunk.GetXCellCount();
                Color color = SetCellColor(cellGlobalIndex);
            
                HexCell cell = chunk.CreateCell(cellGlobalIndex, index, color);
                cells[cellGlobalIndex] = cell;

                SetCellCoordinates(cell, x, z, index, chunk);
                SetCellNeighbors(cell);

                cellIndex++;
            }
        }
    }

    private Color SetCellColor(int cellGlobalIndex)
    {
        Color color = Color.turquoise;

        foreach (int centre in MapManager.Instance.GetContinentsCentres())
        {
            if (cellGlobalIndex == centre)
            {
                color = Color.green;
            }
        }

        return color;
    }

    private void SetCellCoordinates(HexCell cell, int x, int z, int index, Chunk chunk)
    {
        cell.Coordinates = new HexCoordinates(x, z,
            (index / zChunks) * chunk.GetXCellCount() + x,
            (index % zChunks) * chunk.GetZCellCount() + z
        );

        cell.SetCoordinateText();
    }

    private void SetCellNeighbors(HexCell cell)
    {
        
    }
}
