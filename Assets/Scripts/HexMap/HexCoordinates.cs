using UnityEngine;

public struct HexCoordinates
{
    private int localX, localZ;

    private int globalX, globalZ;

    public HexCoordinates(int localX, int localZ, int globalX, int globalZ)
    {
        this.localX = localX;
        this.localZ = localZ;
        this.globalX = globalX;
        this.globalZ = globalZ;
    }

    public void SetLocalCoordinates(int x, int z)
    {
        localX = x;
        localZ = z;
    }

    public void SetGlobalCoordinates(int x, int z)
    {
        globalX = x;
        globalZ = z;
    }

    public int LocalX
    {
        get { return localX; }
    }

    public int LocalZ
    {
        get { return localZ; }
    }

    public int GlobalX
    {
        get { return globalX; }
    }

    public int GlobalZ
    {
        get { return globalZ; }
    }
}
