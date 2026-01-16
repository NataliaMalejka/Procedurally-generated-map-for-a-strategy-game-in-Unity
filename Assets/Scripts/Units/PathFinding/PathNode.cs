using UnityEngine;

public class PathNode
{
    public HexCell Cell;
    public PathNode Parent;
    public int costFromStart;
    public int herustic;
    public int Priority => costFromStart + herustic;

    public PathNode(HexCell cell, PathNode parent, int c, int h)
    {
        Cell = cell;
        Parent = parent;
        costFromStart = c;
        herustic = h;
    }

    public void Update(PathNode parent, int c)
    {
        Parent = parent;
        costFromStart = c;
    }
}