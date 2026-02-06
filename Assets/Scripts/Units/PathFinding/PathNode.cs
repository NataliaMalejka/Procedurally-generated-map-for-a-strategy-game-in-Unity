
// Represents a single node used by the A* 
public class PathNode
{
    public HexCell Cell;
    public PathNode Parent;
    // Cost from the start node to this node
    public int costFromStart;
    // Heuristic cost estimate from this node to the goal
    public int heuristic;
    // Total estimated cost 
    public int Priority => costFromStart + heuristic;

    public PathNode(HexCell cell, PathNode parent, int c, int h)
    {
        Cell = cell;
        Parent = parent;
        costFromStart = c;
        heuristic = h;
    }

    public void Update(PathNode parent, int c)
    {
        Parent = parent;
        costFromStart = c;
    }
}