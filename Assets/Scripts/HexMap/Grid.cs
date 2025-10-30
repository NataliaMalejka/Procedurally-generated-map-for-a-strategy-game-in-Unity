using System.Linq;
using UnityEngine;

enum MapSize
{
    Small,
    Medium,
    Large
}

public class Grid : MonoBehaviour
{
    [SerializeField] private MapSize mapSize;

    [SerializeField] private Chunk chunkPrefab;
    private Chunk[] chunks;
    private int xChunkCount;
    private int zChunkCount;

    private HexCell[] cells;

    private void Start()
    {
        CreateChunks();
    }

    private (int x, int z) SetChunkCounts(MapSize size)
    {
        return size switch
        {
            MapSize.Small => (5, 3),
            MapSize.Medium => (10, 6),
            MapSize.Large => (15, 9),
            _ => (10, 6)
        };
    }

    private void CreateChunks()
    {
        (xChunkCount, zChunkCount) = SetChunkCounts(mapSize);

        chunks = new Chunk[xChunkCount * zChunkCount];
        cells = new HexCell[xChunkCount * zChunkCount * chunkPrefab.GetXCellCount() * chunkPrefab.GetZCellCount()];

        int index = 0;

        for (int i = 0; i < xChunkCount; i++) 
        {
            for (int j = 0; j < zChunkCount; j++) 
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

        position.x = (index % xChunkCount) * HexData.distanceToEdge * 2 * chunk.GetXCellCount();
        position.y = 0f;
        position.z = (index / xChunkCount) * HexData.distanceToCorner * 1.5f * chunk.GetZCellCount();

        chunk.transform.position = position;
    }

    private void AddCells(Chunk chunk, int index)
    {
        int cellIndex = 0;

        for (int i = 0; i < chunk.GetXCellCount(); i++)
        {
            for (int j = 0; j < chunk.GetZCellCount(); j++)
            {
                HexCell cell = chunk.CreateCell(cellIndex, index / xChunkCount);
                cells[cellIndex + index * chunk.GetZCellCount() * chunk.GetXCellCount()] = cell;

                cellIndex++;
            }
        }
    }
}
