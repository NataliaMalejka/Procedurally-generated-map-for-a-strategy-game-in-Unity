using TMPro;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    private MapSize mapSize = MapSize.Medium;

    private string seedString;

    private bool isEarth = true;
    private bool isCold = false;
    private bool isHot = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMapSize(int size)
    {
        mapSize = (MapSize)size;
    }

    public MapSize GetMapSize()
    {
        return mapSize;
    }

    public void SetSeed(string text)
    {
        seedString = text;
    }

    public string GetSeedString()
    {
        return seedString;
    }

    public void SetEarthBiome(bool value)
    {
        isEarth = value;
    }

    public bool IsEarthBiome()
    {
        return isEarth;
    }

    public void SetColdBiome(bool value)
    {
        isCold = value;
    }

    public bool IsColdBiome()
    {
        return isCold;
    }

    public void SetHotBiome(bool value)
    {
        isHot = value;
    }

    public bool IsHotBiome()
    {
        return isHot;
    }
}
