
public enum HexDirection
{
    NE, E, SE, SW, W, NW
}

public static class FindHexDirection
{
    public static readonly (int dq, int dr, int ds)[] hexDirections = new (int, int, int)[]
    {
        ( 0, 1,  -1), // NE
        ( 1,  0, -1), // E
        ( 1,  -1, 0), // SE
        (0,  -1,  1), // SW
        (-1,  0,  1), // W
        ( -1, 1,  0)  // NW
    };

    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
    }

    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }
}
