// Directions for a pointy-top hex grid, ordered clockwise
public enum HexDirection
{
    NE, E, SE, SW, W, NW
}

// Containing cube-coordinate direction vectors
public static class FindHexDirection
{
    // Cube coordinate direction offsets (q, r, s)
    public static readonly (int dq, int dr, int ds)[] hexDirections = new (int, int, int)[]
    {
        ( 0, 1,  -1), // NE
        ( 1,  0, -1), // E
        ( 1,  -1, 0), // SE
        (0,  -1,  1), // SW
        (-1,  0,  1), // W
        ( -1, 1,  0)  // NW
    };

    // Returns the opposite hex direction
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    // Returns the next direction clockwise
    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }
}

// Utility methods related to hex direction detection
public static class HexDirectionUtils
{
    // Returns the direction index from hex cell A to neighboring cell B.
    // Returns -1 if cells are not neighbors.
    public static int GetDirectionIndex(HexCell a, HexCell b)
    {
        int dq = b.Coordinates.Q - a.Coordinates.Q;
        int dr = b.Coordinates.R - a.Coordinates.R;
        int ds = b.Coordinates.S - a.Coordinates.S;

        for (int i = 0; i < FindHexDirection.hexDirections.Length; i++)
        {
            var dir = FindHexDirection.hexDirections[i];
            if (dir.dq == dq && dir.dr == dr && dir.ds == ds)
            {
                return i;
            }
        }

        return -1;
    }
}

