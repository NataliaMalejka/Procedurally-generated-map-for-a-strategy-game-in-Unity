using UnityEngine;

public struct HexCoordinates
{
    private int localX, localZ;

    public int globalX, globalZ;

    private int q, r, s;

    private int indexInGrid;

    public HexCoordinates(int localX, int localZ, int globalX, int globalZ)
    {
        this.localX = localX;
        this.localZ = localZ;
        this.globalX = globalX;
        this.globalZ = globalZ;

        q = globalX - globalZ / 2;
        r = globalZ;
        s = -q - r;

        indexInGrid = MapManager.Instance.GetCellIndex(globalX, globalZ);
    }

    public Vector3Int Neighbor(HexDirection direction)
    {
        var (dq, dr, ds) = FindHexDirection.hexDirections[(int)direction];
        return new Vector3Int(q + dq, r + dr, s + ds);
    }

    public int IndexInGrid
    {
        get { return indexInGrid; }
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

    public int Q
    {
        get { return q; }
    }

    public int R
    {
        get { return r; }
    }

    public int S
    {
        get { return s; }
    }

}
