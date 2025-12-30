using TMPro;
using UnityEngine;

enum Layers
{
    Earth,
    Hot,
    Cold
}

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    private MapSize mapSize = MapSize.Medium;

    private string seedString;

    private int isEarth = 0;
    private int isHot = -1;
    private int isCold = -1;

    private int maxlayerIndex = 0;

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

    public void SetEarthBiome(int value)
    {
        isEarth = value;
    }

    public int IsEarthBiome()
    {
        return isEarth;
    }

    public void SetColdBiome(int value)
    {
        isCold = value;
    }

    public int IsColdBiome()
    {
        return isCold;
    }

    public void SetHotBiome(int value)
    {
        isHot = value;
    }

    public int IsHotBiome()
    {
        return isHot;
    }

    public int getMaxLayerIndex()
    {
        return maxlayerIndex;
    }

    public void SetLayersIndex()
    {
        int index = 0;

        if (isHot > -1)
        {
            isHot = index;
            index++;
        }
        if (isEarth > -1)
        {
            isEarth = index;
            index++;
        }
        if (isCold > -1)
        {
            isCold = index;
            index++;
        }
        if (isHot < 0 && isEarth < 0 && isCold < 0)
        {
            isEarth = 0;
        }

        maxlayerIndex = index;
    }
}
