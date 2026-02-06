using UnityEngine;

// Available biome layers
enum Layers
{
    Earth,
    Hot,
    Cold
}

// Stores global game settings 
public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    // Selected map size
    private MapSize mapSize = MapSize.Medium;

    // Seed string used for map generation
    private string seedString;

    // Biome enable flags 
    private int isEarth = 0;
    private int isHot = -1;
    private int isCold = -1;

    // Maximum layer index
    private int maxlayerIndex = 0;

    // Currently active layer
    private int currentLayer;
    public int CurrentLayer
    {
        get { return currentLayer; }
        set { currentLayer = value; }
    }

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

    // Sets map size using UI 
    public void SetMapSize(int size)
    {
        mapSize = (MapSize)size;
    }

    public MapSize GetMapSize()
    {
        return mapSize;
    }

    // Sets the world generation seed
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

    // Assigns sequential layer indices to enabled biomes
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
        // Ensure at least one layer exists
        if (isHot < 0 && isEarth < 0 && isCold < 0)
        {
            isEarth = 0;
        }

        maxlayerIndex = index;
    }
}
