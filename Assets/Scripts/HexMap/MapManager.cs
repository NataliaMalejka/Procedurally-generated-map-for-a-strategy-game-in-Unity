using System;
using System.Drawing;
using System.Linq;
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

    public int[] GetContinentsCentres()
    {
        return continentsCentres;
    }

    private void Awake()
    {
        Instance = this;

        UnityEngine.Random.InitState(seed);

        (xChunkCount, zChunkCount) = SetChunkCounts(mapSize);

        continentsCount = SetContinentsCount(mapSize);

        SetMaxMargin();
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
            _ => (UnityEngine.Random.Range(minXMargin, maxXMargin))
        };
    }

    private void SetMaxMargin()
    {
        maxXMargin = xCellCount * xChunkCount - 3;
        maxZMargin = zCellCount * zChunkCount - 2;

        regionsCount = continentsCount / 2 + continentsCount % 2;
        if(regionsCount < 2) regionsCount = 2;

        continentsCentres = new int[continentsCount];

        int localXMin = minXMargin;
        int localXMax = xChunkCount * xCellCount / regionsCount - 3;

        int localZMin = minZMargin;
        int localZMax = zChunkCount * zCellCount / 2 - 2;

        int regionIndex = 2;

        for (int i = 0; i <continentsCount; i++)
        {
            if (i != 0) 
            {
                if (i % 2 == 1)
                {
                    localZMin = localZMax + 4;
                    localZMax = maxZMargin;
                }
                if(i % 2 == 0 || continentsCount == 2)
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
}
