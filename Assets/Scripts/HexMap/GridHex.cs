using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridHex : MonoBehaviour
{
    [SerializeField] private Chunk chunkPrefab;
    private Chunk[] chunks;
    public int xChunks { get; private set; }
    public int zChunks { get; private set; }

    public HexCell[] cellsEarth { get; private set; }
    public HexCell[] cellsCold { get; private set; }
    public HexCell[] cellsHot { get; private set; }

    public Dictionary<int, List<Chunk>> columns = new Dictionary<int, List<Chunk>>();

    private int leftmostColumn;
    public int LeftmostColumn
    {
        get { return leftmostColumn; }
        set { leftmostColumn = value; }
    }

    private int rightmostColumn;
    public int RightmostColumn
    {
        get { return rightmostColumn; }
        set { rightmostColumn = value; }
    }

    public float ColumnWidth { get; private set; }

    private void Start()
    {
        xChunks = MapManager.Instance.xChunkCount;
        zChunks = MapManager.Instance.zChunkCount;

        leftmostColumn = 0;
        rightmostColumn = xChunks - 1;

        ColumnWidth = MapManager.Instance.xCellCount * HexData.distanceToEdge * 2f;

        if (GameSettings.Instance.IsHotBiome() > -1)
        {
            CreateChunks(GameSettings.Instance.IsHotBiome(), cellsHot,(int)Layers.Hot);
        }
        if(GameSettings.Instance.IsEarthBiome() > -1)
        {
            CreateChunks(GameSettings.Instance.IsEarthBiome(), cellsEarth, (int)Layers.Earth);
        }
        if (GameSettings.Instance.IsColdBiome() > -1)
        {
            CreateChunks(GameSettings.Instance.IsColdBiome(), cellsCold, (int)Layers.Cold);
        }     
    }

    private void CreateChunks(int level, HexCell[] cells, int biomelayerIndex)
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
                chunk.SetWaterColorIndex(biomelayerIndex);
                chunk.SetGridCoords(x, z, index);

                AddCells(chunk, index, cells);
                index++;

                if (!columns.TryGetValue(x, out List<Chunk> column))
                {
                    column = new List<Chunk>();
                    columns.Add(x, column);
                }

                column.Add(chunk);
                chunk.SetColumnIndex(x);
            }
        }

        MapManager.Instance.GenerateMap(cells, biomelayerIndex);

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
            Vector3Int n = cell.Coordinates.Neighbor(dir);

            int nq = n.x;
            int nr = n.y;

            int indexZ = nr;
            int indexX = nq + indexZ / 2;

            if (indexZ < 0 || indexZ >= MapManager.Instance.zChunkCount * MapManager.Instance.zCellCount)
                continue;

            indexX = WrapX(indexX);

            int neighborIndex = MapManager.Instance.GetCellIndex(indexX, indexZ);

            if (neighborIndex >= 0 &&
                neighborIndex < cells.Length &&
                cells[neighborIndex] != null)
            {
                cell.SetNeighbor(dir, cells[neighborIndex]);
            }
        }
    }

    private int WrapX(int x)
    {
        int width = MapManager.Instance.xChunkCount * MapManager.Instance.xCellCount;
        return (x % width + width) % width;
    }
}
