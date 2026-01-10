using System;
using System.Collections.Generic;
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

    private List<HexCell[]> gridLayers = new List<HexCell[]>();

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
        GenerateLayers();
    }

    private void GenerateLayers()
    {
        xChunks = MapManager.Instance.xChunkCount;
        zChunks = MapManager.Instance.zChunkCount;

        leftmostColumn = 0;
        rightmostColumn = xChunks - 1;

        ColumnWidth = MapManager.Instance.xCellCount * HexData.distanceToEdge * 2f;

        if (GameSettings.Instance.IsHotBiome() > -1)
        {
            cellsHot = CreateChunks(GameSettings.Instance.IsHotBiome(), (int)Layers.Hot);
            gridLayers.Add(cellsHot);
        }
        if (GameSettings.Instance.IsEarthBiome() > -1)
        {
            cellsEarth = CreateChunks(GameSettings.Instance.IsEarthBiome(), (int)Layers.Earth);
            gridLayers.Add(cellsEarth);
        }
        if (GameSettings.Instance.IsColdBiome() > -1)
        {
            cellsCold = CreateChunks(GameSettings.Instance.IsColdBiome(), (int)Layers.Cold);
            gridLayers.Add(cellsCold);
        }
    }

    private HexCell[] CreateChunks(int level, int biomelayerIndex)
    {
        chunks = new Chunk[xChunks * zChunks];
        HexCell[] cells = new HexCell[xChunks * zChunks * MapManager.Instance.xCellCount * MapManager.Instance.zCellCount];

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

        return cells;
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

    public HexCell GetCell(Vector3 worldPos)
    {
        Vector3 pos = transform.InverseTransformPoint(worldPos);
        pos.y = 0f;

        HexCoordinates cube = HexCoordinates.FromWorld(pos);

        int globalZ = cube.R;

        int maxZ = MapManager.Instance.zChunkCount * MapManager.Instance.zCellCount;
        if (globalZ < 0 || globalZ >= maxZ)
            return null;

        int globalX = cube.Q + globalZ / 2;

        int mapWidth = MapManager.Instance.xChunkCount * MapManager.Instance.xCellCount;
        globalX = ((globalX % mapWidth) + mapWidth) % mapWidth;

        int index = MapManager.Instance.GetCellIndex(globalX, globalZ);

        var grid = gridLayers[GameSettings.Instance.CurrentLayer];
        if (index < 0 || index >= grid.Length)
            return null;

        return grid[index];
    }
}
