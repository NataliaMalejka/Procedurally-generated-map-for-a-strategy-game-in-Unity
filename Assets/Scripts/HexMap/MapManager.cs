using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

enum MapSize
{
    Small,
    Medium,
    Large
}

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [SerializeField] private MapSize mapSize;
    [SerializeField] private int seed;

    public int xChunkCount { get; private set; }
    public int zChunkCount { get; private set; }

    public int xCellCount { get; private set; } = 6;
    public int zCellCount { get; private set; } = 6;

    public int minXMargin { get; private set; } = 3;
    public int minZMargin { get; private set; } = 2;
    public int maxXMargin { get; private set; }
    public int maxZMargin { get; private set; }

    private int regionsCount;

    private int continentsCount;
    private int[] continentsCentres;

    private int[] continentsCellsAmound;

    private Vector2[] continentDir;

    public float perlinScale = 0.15f;
    //public float perlinThreshold = 0.33f;

    public float distWeight = 0.3f;
    public float perlinWeight = 0.5f;
    public float dirWeight = 0.3f;
    public float minScore = 0.7f;
    public float minCoastNoise = 0.8f;
    public float maxCoastNoise = 1.2f;

    private List<int> chunksToRefresh = new List<int>();

    public int[] GetContinentsCentres()
    {
        return continentsCentres;
    }

    public List<int> GetChunksToRefresh()
    {
        return chunksToRefresh;
    }

    private void Awake()
    {
        Instance = this;

        UnityEngine.Random.InitState(seed);

        (xChunkCount, zChunkCount) = SetChunkCounts(mapSize);

        continentsCount = SetContinentsCount(mapSize);

        SetMaxMargin();

        SetContinentCellsAmound();

        SetContinentsInRegions();
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

    private int SetContinentsCount(MapSize size)
    {
        return size switch
        {
            MapSize.Small => (UnityEngine.Random.Range(2, 4)),
            MapSize.Medium => (UnityEngine.Random.Range(3,6)),
            MapSize.Large => (UnityEngine.Random.Range(4, 7)),
            _ => (UnityEngine.Random.Range(2, 4))
        };
    }

    private void SetContinentCellsAmound()
    {
        continentsCellsAmound = new int[continentsCount];

        int actualCellsX = xCellCount * xChunkCount - minXMargin * 2;
        int actualCellsZ = zCellCount * zChunkCount - minZMargin * 2;

        int actualCells = actualCellsX * actualCellsZ;

        for (int i = 0; i < continentsCount; i++)
        {
            continentsCellsAmound[i] = UnityEngine.Random.Range((actualCells / 100 * 30) / continentsCount, (actualCells / 100 * 40) / continentsCount); 
        }
    }

    private void SetMaxMargin()
    {
        maxXMargin = xCellCount * xChunkCount - 3;
        maxZMargin = zCellCount * zChunkCount - 2;

        regionsCount = continentsCount / 2 + continentsCount % 2;
        if(regionsCount < 2) regionsCount = 2;

        continentsCentres = new int[continentsCount];
        continentDir = new Vector2[continentsCount];
    }

    private void SetContinentsInRegions()
    {
        int localXMin = minXMargin;
        int localXMax = xChunkCount * xCellCount / regionsCount - 3;

        int localZMin = minZMargin;
        int localZMax = zChunkCount * zCellCount / 2 - 2;

        int regionIndex = 2;

        int dirX = 0;
        int dirZ = 0;

        for (int i = 0; i < continentsCount; i++)
        {
            dirX = 1;
            dirZ = 1;

            if (i != 0)
            {
                if (i % 2 == 1)
                {
                    localZMin = localZMax + 4;
                    localZMax = maxZMargin;
                    dirZ = -1;

                }
                if (i % 2 == 0 || continentsCount == 2)
                {
                    localZMin = minZMargin;
                    localZMax = zChunkCount * zCellCount / 2 - 2;
                    dirZ = 1;

                    localXMin = localXMax + 6;
                    localXMax = xChunkCount * xCellCount / regionsCount * regionIndex - 3;

                    if (localXMax == maxXMargin) dirX = -1; 
                    else dirX = 0; 

                    regionIndex++;
                }
            }

            continentDir[i] = new Vector2(dirX, dirZ);
            SetContinentsCentres(localXMin, localXMax, localZMin, localZMax, i);
        }
    }

    private void SetContinentsCentres(int localXMin, int localXMax, int localZMin, int localZMax, int index)
    {
        int xPos = UnityEngine.Random.Range(localXMin, localXMax);
        int zPos = UnityEngine.Random.Range(localZMin, localZMax);

        continentsCentres[index] = GetCellIndex(xPos, zPos);
    }

    public int GetCellIndex(int xPos, int zPos)
    {
        int chunkX = xPos / xCellCount;
        int chunkZ = zPos / zCellCount;

        int chunkIndex = chunkX * zChunkCount + chunkZ;

        int localX = (xPos - chunkX * xCellCount);
        int localZ = (zPos - chunkZ * zCellCount);

        int localCellIndex = localX * zCellCount + localZ;

        return chunkIndex * (xCellCount * zCellCount) + localCellIndex;
    }

    public void GenerateContinents(HexCell[] gridCells) 
    {
        chunksToRefresh.Clear();

        for (int i = 0; i < continentsCentres.Length; i++) 
        {
            int cellsCreated = 0;
            int centreIndex = continentsCentres[i];

            HexCell centreCell = gridCells[centreIndex];
            Queue<HexCell> cellsToCheck = new Queue<HexCell>();

            SetContinentPart(centreCell, i, cellsToCheck);
            cellsCreated++;

            cellsCreated += AddCloseNeighborCells(centreCell, i, cellsToCheck);

            while (cellsToCheck.Count > 0 && cellsCreated < continentsCellsAmound[Array.IndexOf(continentsCentres, centreIndex)])
            {
                HexCell currentCell = cellsToCheck.Dequeue();

                for (int j = 0; j < 6; j++)
                {
                    HexCell neighborCell = currentCell.GetNeighbor((HexDirection)j);

                    if(neighborCell == null) continue;

                    if (neighborCell.ContinentIndex != -1) continue;

                    if (IsBehindBorders(neighborCell)) continue;

                    if (AdjacentToOtherContinent(neighborCell, i)) continue;

                    if (CheckNoise(neighborCell, centreCell, i)) continue;

                    SetContinentPart(neighborCell, i, cellsToCheck);
                    cellsCreated++;
                }
            }
        }
    }

    private void SetContinentPart(HexCell cell, int continentIndex, Queue<HexCell> cellsToCheck)
    {
        cell.SetContinent(continentIndex);
        cellsToCheck.Enqueue(cell);

        int chunkIndex = cell.HexChunk.GetIndexInGrid();

        AddChunkToRefreshList(chunkIndex);
        ChcekNeighbourChunks(cell, chunkIndex);
    }

    private void ChcekNeighbourChunks(HexCell cell, int chunkIndex)
    {
        if (cell.Coordinates.LocalX == 0 && cell.Coordinates.GlobalX != 0)
        {
            AddChunkToRefreshList(chunkIndex - zChunkCount);
        }
        else if (cell.Coordinates.LocalX == xCellCount - 1 && cell.Coordinates.globalX < xChunkCount * xCellCount - 1)
        {
            AddChunkToRefreshList(chunkIndex + zChunkCount);
        }

        if (cell.Coordinates.LocalZ == 0 && cell.Coordinates.globalZ != 0)
        {
            AddChunkToRefreshList(chunkIndex - 1);
        }
        else if (cell.Coordinates.LocalZ == zCellCount - 1 && cell.Coordinates.globalZ < zChunkCount * zCellCount - 1)
        {
            AddChunkToRefreshList(chunkIndex + 1);
        }
    }

    private void AddChunkToRefreshList(int chunkIndex)
    {
        if (!chunksToRefresh.Contains(chunkIndex))
            chunksToRefresh.Add(chunkIndex);
    }

    private int AddCloseNeighborCells(HexCell centreCell, int continentIndex, Queue<HexCell> cellsToCheck)
    {
        int addedCells = 0;

        for (int i = 0; i < 6; i++)
        {
            HexCell closeNeighborCell = centreCell.GetNeighbor((HexDirection)i);

            if (closeNeighborCell != null && !IsBehindBorders(closeNeighborCell))
            {
                SetContinentPart(closeNeighborCell, continentIndex, cellsToCheck);
                addedCells++;
            }
        }

        return addedCells;
    }

    private bool IsBehindBorders(HexCell cell)
    {
        if (cell.Coordinates.globalX < minXMargin || cell.Coordinates.globalX >= maxXMargin ||
            cell.Coordinates.globalZ < minZMargin || cell.Coordinates.globalZ >= maxZMargin)
        {
            return true;
        }
        else
            return false;
    }

    private bool AdjacentToOtherContinent(HexCell neighborCell, int currentContinentIndex)
    {
        bool adjacentToOtherContinent = false;
        for (int i = 0; i < 6; i++)
        {
            HexCell adjacentCell = neighborCell.GetNeighbor((HexDirection)i);

            if (adjacentCell == null) continue;

            if (adjacentCell.ContinentIndex != -1 && adjacentCell.ContinentIndex != currentContinentIndex)
            {
                adjacentToOtherContinent = true;
                break;
            }
        }

        return adjacentToOtherContinent;
    }

    private bool CheckNoise(HexCell neighborCell, HexCell centreCell, int index)
    {
        float dist = Vector2.Distance(neighborCell.Coordinates.GetCellPos(), centreCell.Coordinates.GetCellPos());
        float distFactor = Mathf.Clamp01(1f - dist / (zCellCount* zChunkCount / 2));

        float noiseFactor = Mathf.PerlinNoise(
        neighborCell.Coordinates.globalX * perlinScale,
        neighborCell.Coordinates.globalZ * perlinScale
        );

        Vector2 toHex = (neighborCell.Coordinates.GetCellPos() - centreCell.Coordinates.GetCellPos()).normalized;
        float dirFactor = Vector2.Dot(toHex, continentDir[index].normalized) * 0.5f + 0.5f; 
     
        float coastNoise = UnityEngine.Random.Range(minCoastNoise, maxCoastNoise);

        float score =
            distFactor * distWeight +
            noiseFactor * perlinWeight +
            dirFactor * dirWeight;

        return score * coastNoise < minScore;
    }
}
