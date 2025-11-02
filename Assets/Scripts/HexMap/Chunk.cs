using System;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField] private HexCell cellPrefab;
    private HexCell[] cells;
    private int xCells;
    private int zCells;

    [SerializeField] private ChunkMesh chunkMesh;

    public int GetXCellCount()
    {
        return xCells;
    }

    public int GetZCellCount()
    {
        return zCells;
    }

    private void OnEnable()
    {
        xCells = MapManager.Instance.xCellCount;
        zCells = MapManager.Instance.zCellCount;
    }

    public ChunkMesh GetChunkMesh()
    {
        return chunkMesh;
    }

    public HexCell CreateCell(int cellGlobalIndex, int chunkIndex)
    {
        HexCell[] cells = new HexCell[xCells * zCells];

        int localCellIndex = cellGlobalIndex - (chunkIndex * xCells * zCells);

        HexCell cell = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform);

        cells[localCellIndex] = cell;
        cell.HexChunk = this;

        SetCellPosition(cell, localCellIndex, chunkIndex % MapManager.Instance.zChunkCount);
        CreateCellMesh(cell);

        return cell;
    }

    private void SetCellPosition(HexCell cell, int index, int zChunkIndex)
    {
        Vector3 position;

        position.x = (index / zCells) * HexData.distanceToEdge * 2f;
        position.y = 0f;
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
        Vector3 centre = cell.transform.localPosition;

        for (int i = 0; i < 6; i++)
        {
            chunkMesh.CreateTriangle(
                centre,
                centre + HexData.corners[i],
                centre + HexData.corners[(i + 1) % 6]
            );
        }
    }

    public void AddCellColor(HexCell cell)
    {
        chunkMesh.AddColor(cell.CellColor);
    }
}
