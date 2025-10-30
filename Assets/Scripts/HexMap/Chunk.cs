using System;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField] private HexCell cellPrefab;
    private HexCell[] cells;
    private int xCellCount = 6;
    private int zCellCount = 6;

    [SerializeField] private ChunkMesh chunkMesh;

    public int GetXCellCount()
    {
        return xCellCount;
    }

    public int GetZCellCount()
    {
        return zCellCount;
    }

    public ChunkMesh GetChunkMesh()
    {
        return chunkMesh;
    }

    public HexCell CreateCell(int index, int zChunkIndex)
    {
        HexCell[] cells = new HexCell[xCellCount * zCellCount];

        HexCell cell = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform);
        cells[index] = cell;

        cell.SetHexChunk(this);

        SetCellPosition(cell, index, zChunkIndex);
        CreateCellMesh(cell);

        return cell;
    }

    private void SetCellPosition(HexCell cell, int index, int zChunkIndex)
    {
        Vector3 position;

        position.x = (index % xCellCount) * HexData.distanceToEdge * 2f;
        position.y = 0f;
        position.z = (index / xCellCount) * HexData.distanceToCorner * 1.5f;

        int rowIndex = (index / xCellCount) % 2;
        int offset= (zCellCount % 2 == 0) ? 1 : 1 - (zChunkIndex % 2);

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
}
