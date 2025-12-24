using System;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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
        position.y = cell.CentreTerrainLevel;
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
            cell.CentreTerrainLevel,
            cell.transform.localPosition.z
        );

        int t = cell.TextureIndex;

        for (int i = 0; i < 6; i++)
        {
            Vector3 v1 = cell.GetEdge(i).GetLocalV1();
            Vector3 v2 = cell.GetEdge(i).GetLocalV2();

            Vector3 middle1 = cell.GetEdge(i).GetMiddle1();
            Vector3 middle2 = cell.GetEdge(i).GetMiddle2();

            HexCell neighborCell = cell.GetNeighbor((HexDirection)i);

            if (neighborCell != null && cell.GetEdge(i).GetEdgeType() == EdgeType.Smooth)
            {
                chunkMesh.CreateSmoothTriangleWithColor(centre, v1, v2, middle1, middle2, t, cell);
            }
            else
            {
                chunkMesh.CreateTriangleWithColor(centre, v1, v2, t, cell, i);
            }

            if (cell.GetEdge(i).InRiver)
            {
                chunkMesh.CreateHexRiver(middle1, middle2, cell, cell.GetEdge(i).GetEdgeType() == EdgeType.Smooth, centre, i);
            }
            else if (cell.GetEdge(i).OutRiver)
            {
                bool hasInRiver = false;

                for (int j = 0; j < 6; j++)
                {
                    if (cell.GetEdge(j).InRiver)
                    {
                        hasInRiver = true;
                        break;
                    }
                }

                if (!hasInRiver)
                {
                    chunkMesh.CreateRiverSourceOrEnd(middle1, middle2, cell, cell.GetEdge(i).GetEdgeType() == EdgeType.Smooth, centre);
                }
            }

            if (i < 3)
            {               
                if (neighborCell!= null)
                {
                    if (cell.GetEdge(i).GetEdgeType() == EdgeType.Smooth)
                    {
                        chunkMesh.CreateSmoothConnection(v1, v2, i, t, cell, neighborCell, middle1, middle2);
                    }
                    else
                        chunkMesh.CreateRectangularCellsConnection(v1, v2, i, t, neighborCell, middle1, middle2, cell);
                   
                    if (i < 2 && cell.GetNeighbor((HexDirection)i + 1) != null)
                    {
                        HexCell nextNeighborCell = cell.GetNeighbor((HexDirection)i+1);

                        if (nextNeighborCell!=null)
                        {
                            chunkMesh.CreateTriangleCellsConnection(v2, i, t, cell ,neighborCell, nextNeighborCell);
                        }                      
                    }
                }              
            }
        }

        if(cell.StructureIndex >= 0)
        {
            CreateStructure(cell, centre);
        }
    }

    private void CreateStructure(HexCell cell, Vector3 pos)
    {
        GameObject[] structures = MapManager.Instance.GetObjects(cell.GetBiome());

        Vector3 centre = new Vector3(
            pos.x + this.transform.position.x,
            pos.y,
            pos.z + this.transform.position.z
        );

        if (structures == null || structures.Length == 0)
            return;

        GameObject cellStructure = structures[cell.StructureIndex];
        Vector3 baseEuler = cellStructure.transform.eulerAngles;

        Quaternion finalRotation = Quaternion.Euler(
            baseEuler.x,
            UnityEngine.Random.Range(0f, 360f),
            baseEuler.z
        );

        Instantiate(cellStructure, centre, finalRotation, cell.transform);
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
