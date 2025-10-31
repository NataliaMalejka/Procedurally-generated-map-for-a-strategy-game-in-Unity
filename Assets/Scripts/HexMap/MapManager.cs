using System;
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

    private int[] continentsCentres;

    public int[] GetContinentsCentres()
    {
        return continentsCentres;
    }

    private void Awake()
    {
        Instance = this;

        (xChunkCount, zChunkCount) = SetChunkCounts(mapSize);

        SetContinentsCentres();
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

    private void SetContinentsCentres()
    {
        continentsCentres = new int[continentsCount];

        UnityEngine.Random.InitState(seed);

        for (int i = 0; i < continentsCentres.Length; i++)
        {
            continentsCentres[i] = UnityEngine.Random.Range(0, xChunkCount * zChunkCount * xCellCount * zCellCount);
        }
    }
}
