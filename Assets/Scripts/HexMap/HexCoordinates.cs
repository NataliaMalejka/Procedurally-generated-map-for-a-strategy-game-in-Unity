using UnityEngine;

// Represents hex coordinates 
public struct HexCoordinates
{
    // Grid coordinates
    public int globalX, globalZ;

    // Cube coordinates 
    private int q, r, s;

    public int GlobalX => globalX;
    public int GlobalZ => globalZ;
    public int Q => q;
    public int R => r;
    public int S => s;

    // Set coordinates 
    public HexCoordinates(int localX, int localZ, int globalX, int globalZ)
    {
        this.globalX = globalX;
        this.globalZ = globalZ;

        q = globalX - globalZ / 2;
        r = globalZ;
        s = -q - r;
    }

    // Converts world position to hex coordinates
    public static HexCoordinates FromWorld(Vector3 position)
    {
        // Convert world space to cube coordinates
        float q = (position.x / (HexData.distanceToEdge * 2f)) - (position.z / (HexData.distanceToCorner * 3f));
        float r = position.z / (HexData.distanceToCorner * 1.5f);
        float s = -q - r;

        // Round to nearest cube coordinates
        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);
        int rs = Mathf.RoundToInt(s);

        // Correct rounding error by enforcing q + r + s = 0
        float dq = Mathf.Abs(q - rq);
        float dr = Mathf.Abs(r - rr);
        float ds = Mathf.Abs(s - rs);

        if (dq > dr && dq > ds)
            rq = -rr - rs;
        else if (dr > ds)
            rr = -rq - rs;

        return new HexCoordinates(rq, rr); 
    }

    // Creates cube coordinates 
    public HexCoordinates(int q, int r)
    {
        this.q = q;
        this.r = r;
        this.s = -q - r;

        globalZ = r;
        globalX = q + r / 2;
    }

    // Returns cube coordinates of a neighboring cell
    public Vector3Int Neighbor(HexDirection direction)
    {
        var (dq, dr, ds) = FindHexDirection.hexDirections[(int)direction];
        return new Vector3Int(q + dq, r + dr, s + ds);
    }

    // Returns grid position
    public Vector2 GetCellPos()
    {
        return new Vector2(globalX, globalZ);
    }
}
