using System;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField] private HexCell cellPrefab;
    private HexCell[] cells;
    private int xCellCount = 6;
    private int zCellCount = 6;

    [SerializeField] private ChunkMesh chunkMesh;

    private void Start()
    {
          
    }

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

    public HexCell CreateCells(int index)
    {
        HexCell[] cells = new HexCell[xCellCount * zCellCount];

        HexCell cell = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform);
        cells[index] = cell;
        cell.SetHexChunk(this);
        SetCellPosition(cell, index);
        CreateCellMesh(cell);

        return cell;
    }

    private void SetCellPosition(HexCell cell, int index)
    {
        Vector3 position;
        //position.x = (index % xCellCount + (index / xCellCount) * 0.5f - (index / xCellCount) / 2) * (HexData.distanceToEdge * 2f);
        //position.y = 0f;
        //position.z = (index / xCellCount) * (HexData.distanceToCorner * 1.5f);

        position.x = (index % xCellCount) * HexData.distanceToEdge * 2f;
        position.y = 0f;
        position.z = (index / xCellCount) * HexData.distanceToCorner * 1.5f;

        if(index / xCellCount % 2 == 1)
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
