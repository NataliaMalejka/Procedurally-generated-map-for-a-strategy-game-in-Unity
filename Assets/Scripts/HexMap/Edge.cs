using UnityEngine;

// Types of terrain transitions between hex cells
public enum EdgeType
{
    Flat,
    Smooth,
    Cliff,
    Mountain,
    None
}

// Represents a single edge of a hex cell
public class Edge
{
    EdgeType edgeType;

    // Solid edge vertices
    private Vector3 v1Global;
    private Vector3 v2Global;

    // Full edge vertices 
    private Vector3 v1GlobalFull;
    private Vector3 v2GlobalFull;

    private HexDirection hexDirection;
    private HexCell cell;

    private Vector3 chunkPos;

    // Ocean and river flags
    private bool isOceanEdge = false;
    public bool IsOceanEdge => isOceanEdge;

    private bool inRiver = false;
    public bool InRiver
    {
        get { return inRiver; }
        set { inRiver = value; }
    }

    private bool outRiver = false;
    public bool OutRiver
    {
        get { return outRiver; }
        set { outRiver = value; }
    }

    // Initializes edge geometry and data
    public void SetData(EdgeType edgeType, HexDirection hexDirection, int terrainLevel, Vector3 hexPos, Vector3 chunkPos, HexCell cell)
    {
        Vector3 centre = new Vector3(
           hexPos.x + chunkPos.x,
           terrainLevel,
           hexPos.z + chunkPos.z
        );

        this.edgeType = edgeType;

        this.v1Global = centre + HexData.GetSolidCorner((int)hexDirection);
        this.v2Global = centre + HexData.GetSolidCorner((int)(hexDirection + 1) % 6);

        this.v1GlobalFull = centre + HexData.GetCorner((int)hexDirection);
        this.v2GlobalFull = centre + HexData.GetCorner((int)(hexDirection + 1) % 6);

        this.hexDirection = hexDirection;
        this.chunkPos = chunkPos;

        this.cell = cell;
    }

    public Vector3 GetGlobalV1()
    {
        return v1Global;
    }

    // Sets vertex and updates neighboring edge
    public void SetGlobalV1(Vector3 newV1Global)
    {
        v1Global = newV1Global;
        CorrectNeighbourEdgeV1toV2();
    }

    private void CorrectNeighbourEdgeV1toV2()
    {
        Edge neighbourEdge = cell.GetEdge(((int)hexDirection + 5) % 6);
        neighbourEdge.v2Global = v1Global;
    }

    public Vector3 GetLocalV1()
    {
        Vector3 v1Local = new Vector3(
            v1Global.x - chunkPos.x,
            v1Global.y,
            v1Global.z - chunkPos.z
        );

        return v1Local;
    }

    public Vector3 GetFullV1()
    {
        return v1GlobalFull;
    }

    public Vector3 GetGlobalV2()
    {
        return v2Global;
    }

    public void SetGlobalV2(Vector3 newV2Global)
    {
        v2Global = newV2Global;
        CorrectNeighbourEdgeV2toV1();
    }

    private void CorrectNeighbourEdgeV2toV1()
    {
        Edge neighbourEdge = cell.GetEdge(((int)hexDirection + 1) % 6);
        neighbourEdge.v1Global = v2Global;
    }

    public Vector3 GetLocalV2()
    {
        Vector3 v2Local = new Vector3(
            v2Global.x - chunkPos.x,
            v2Global.y,
            v2Global.z - chunkPos.z
        );

        return v2Local;
    }

    public Vector3 GetFullV2()
    {
        return v2GlobalFull;
    }

    // First interpolation point along the edge
    public Vector3 GetMiddle1()
    {
        return Vector3.Lerp(GetLocalV1(), GetLocalV2(), 1f / 3f);
    }

    // Second interpolation point along the edge
    public Vector3 GetMiddle2()
    {
        return Vector3.Lerp(GetLocalV1(), GetLocalV2(), 2f / 3f);
    }

    public EdgeType GetEdgeType()
    {
        return edgeType;
    }

    public void SetOceanEdge(bool value)
    {
        isOceanEdge = value;
    }

}
