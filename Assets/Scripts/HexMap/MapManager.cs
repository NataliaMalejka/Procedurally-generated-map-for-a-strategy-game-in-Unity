using System;
using System.Collections.Generic;
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

    public float perlinScale = 0.15f;
    public float perlinThreshold = 0.3f;

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
    }

    private void SetContinentsInRegions()
    {
        int localXMin = minXMargin;
        int localXMax = xChunkCount * xCellCount / regionsCount - 3;

        int localZMin = minZMargin;
        int localZMax = zChunkCount * zCellCount / 2 - 2;

        int regionIndex = 2;

        for (int i = 0; i < continentsCount; i++)
        {
            if (i != 0)
            {
                if (i % 2 == 1)
                {
                    localZMin = localZMax + 4;
                    localZMax = maxZMargin;
                }
                if (i % 2 == 0 || continentsCount == 2)
                {
                    localZMin = minZMargin;
                    localZMax = zChunkCount * zCellCount / 2 - 2;

                    localXMin = localXMax + 6;
                    localXMax = xChunkCount * xCellCount / regionsCount * regionIndex - 3;

                    regionIndex++;

                    if (localXMax > maxXMargin)
                        localXMax = maxXMargin;
                }
            }

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

                    if (CheckNoise(neighborCell)) continue;

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

        if (!chunksToRefresh.Contains(chunkIndex))
            chunksToRefresh.Add(chunkIndex);
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
        for (int k = 0; k < 6; k++)
        {
            HexCell adjacentCell = neighborCell.GetNeighbor((HexDirection)k);

            if (adjacentCell == null) continue;

            if (adjacentCell.ContinentIndex != -1 && adjacentCell.ContinentIndex != currentContinentIndex)
            {
                adjacentToOtherContinent = true;
                break;
            }
        }

        return adjacentToOtherContinent;
    }

    private bool CheckNoise(HexCell neighborCell)
    {
        float noise = Mathf.PerlinNoise(
        neighborCell.Coordinates.globalX * perlinScale,
        neighborCell.Coordinates.globalZ * perlinScale
        );

        return noise < perlinThreshold;
    }
}
