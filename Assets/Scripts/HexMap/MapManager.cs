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
    [SerializeField] private int continentsCount;

    public int xChunkCount { get; private set; }
    public int zChunkCount { get; private set; }

    public int xCellCount { get; private set; } = 6;
    public int zCellCount { get; private set; } = 6;

    public int minXMargin { get; private set; } = 3;
    public int minZMargin { get; private set; } = 2;
    public int maxXMargin { get; private set; }
    public int maxZMargin { get; private set; }

    private int regionsCount;

    private int[] continentsCentres;

    public int[] GetContinentsCentres()
    {
        return continentsCentres;
    }

    private void Awake()
    {
        Instance = this;

        (xChunkCount, zChunkCount) = SetChunkCounts(mapSize);

        UnityEngine.Random.InitState(seed);

        continentsCount = SetContinentsCount(mapSize);

        SetMaxMargin();

        //SetContinentsCentres();
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
        Debug.Log("regions " + regionsCount);

        continentsCentres = new int[continentsCount];

        Debug.Log("contonents count " + continentsCount);

        int localXMin = minXMargin;
        int localXMax = xChunkCount * xCellCount / regionsCount - 3;

        int regionIndex = 2;

        for (int i = 0; i <continentsCount; i++)
        {
            Debug.Log("continent " + i);

            if (i !=0 && (i % 2==0 || continentsCount==2))
            {
                localXMin = localXMax + 6;
                localXMax = xChunkCount * xCellCount / regionsCount * regionIndex - 3;

                regionIndex++;

                Debug.Log("localXMax recalculated to " + localXMax);

                if (localXMax > maxXMargin)
                    localXMax = maxXMargin;
            }

            Debug.Log("border " + localXMin + " " + localXMax);

            SetContinentsCentres(localXMin, localXMax, i);
        }
    }

    private void SetContinentsCentres(int localXMin, int LocalXMax, int index)
    {
        int xPos = UnityEngine.Random.Range(localXMin, LocalXMax);
        int zPos = UnityEngine.Random.Range(minZMargin, maxZMargin);

        Debug.Log("Pos " + xPos + " " + zPos);

        continentsCentres[index] = GetCellIndex(xPos, zPos);
    }

    private void SetContinentsCentres()
    {
        for (int i = 0; i < continentsCentres.Length; i++)
        {
            int xPos = UnityEngine.Random.Range(minXMargin, maxXMargin);
            int zPos = UnityEngine.Random.Range(minZMargin, maxZMargin);

            continentsCentres[i] = GetCellIndex(xPos, zPos);
        }
    }

    private int GetCellIndex(int xPos, int zPos)
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
