using System;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField] private HexCell cellPrefab;
    private HexCell[] cells;
    private int xCells;
    private int zCells;

    private int posX;
    private int posZ;
    private int indexInGrid;

    [SerializeField] private ChunkMesh chunkMesh;

    public HexCell[] GetCells()
    {
       return cells;
    }

    public int GetXCellCount()
    {
        return xCells;
    }

    public int GetZCellCount()
    {
        return zCells;
    }

    public int GetIndexInGrid()
    {
        return indexInGrid;
    }

    public void SetGridCoords(int x, int z, int index)
    {
        posX = x;
        posZ = z;
        indexInGrid = index;
    }

    private void OnEnable()
    {
        xCells = MapManager.Instance.xCellCount;
        zCells = MapManager.Instance.zCellCount;

        cells = new HexCell[xCells * zCells];
    }

    public ChunkMesh GetChunkMesh()
    {
        return chunkMesh;
    }

    public HexCell CreateCell(int cellGlobalIndex, int chunkIndex)
    {
        int localCellIndex = cellGlobalIndex - (chunkIndex * xCells * zCells);

        HexCell cell = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform);

        cells[localCellIndex] = cell;
        cell.HexChunk = this;

        SetCellPosition(cell, localCellIndex, chunkIndex % MapManager.Instance.zChunkCount);

        return cell;
    }

    private void SetCellPosition(HexCell cell, int index, int zChunkIndex)
    {
        Vector3 position;

        position.x = (index / zCells) * HexData.distanceToEdge * 2f;
        position.y = cell.TerreinLevel;
        position.z = (index % zCells) * HexData.distanceToCorner * 1.5f;

        int rowIndex = (index % zCells) % 2;
        int offset = (zCells % 2 == 0) ? 1 : 1 - (zChunkIndex % 2);

        if (rowIndex == offset)
        {
            position.x += HexData.distanceToEdge;
        }

        cell.transform.localPosition = position;
    }

    private void CreateCellMesh(HexCell cell)
    {
        Vector3 centre = new Vector3(
            cell.transform.localPosition.x,
            cell.TerreinLevel,
            cell.transform.localPosition.z
        );

        Color color = cell.CellColor;

        for (int i = 0; i < 6; i++)
        {
            Vector3 v1 = centre + HexData.GetSolidCorner(i);
            Vector3 v2 = centre + HexData.GetSolidCorner((i + 1) % 6);

            chunkMesh.CreateTriangleWithColor(centre, v1, v2, color);

            if (i < 3)
            {
                HexCell neighborCell = cell.GetNeighbor((HexDirection)i);
                if (neighborCell!= null)
                {
                    chunkMesh.CreateRectangularCellsConnection(v1, v2, i, color, neighborCell);
                   
                    if (i < 2 && cell.GetNeighbor((HexDirection)i + 1) != null)
                    {
                        HexCell nextNeighborCell = cell.GetNeighbor((HexDirection)i+1);

                        if (nextNeighborCell!=null)
                        {
                            chunkMesh.CreateTriangleCellsConnection(v2, i, color, neighborCell, nextNeighborCell);
                        }                      
                    }
                }              
            }
        }
    }


    public void RefreshChunk()
    {
        chunkMesh.Clear();

        foreach (var cell in cells)
        {
            CreateCellMesh(cell);
        }

        chunkMesh.Apply();
    }
}
