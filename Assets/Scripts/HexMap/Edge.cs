using UnityEngine;

public enum EdgeType
{
    Flat,
    Smooth,
    Cliff
}

public class Edge
{
    EdgeType edgeType;

    private Vector3 v1Global;
    private Vector3 v2Global;

    private Vector3 v1GlobalFull;
    private Vector3 v2GlobalFull;

    private HexDirection hexDirection;

    private Vector3 centre;
    private Vector3 chunkPos;

    public Edge(EdgeType edgeType, HexDirection hexDirection, int terrainLevel, Vector3 hexPos, Vector3 chunkPos)
    {
        Vector3 centre = new Vector3(
           hexPos.x + chunkPos.x,
           terrainLevel,
           hexPos.z + chunkPos.z
        );

        this.centre = centre;

        this.edgeType = edgeType;

        this.v1Global = centre + HexData.GetSolidCorner((int)hexDirection);
        this.v2Global = centre + HexData.GetSolidCorner((int)(hexDirection + 1) % 6);

        //v1Global = new Vector3(v1Global.x + chunkPos.x, v1Global.y, v1Global.z + chunkPos.z);
        //v2Global = new Vector3(v2Global.x + chunkPos.x, v2Global.y, v2Global.z + chunkPos.z);

        this.v1GlobalFull = centre + HexData.GetCorner((int)hexDirection);
        this.v2GlobalFull = centre + HexData.GetCorner((int)(hexDirection + 1) % 6);

       // v1GlobalFull = new Vector3(v1GlobalFull.x + chunkPos.x, v1GlobalFull.y, v1GlobalFull.z + chunkPos.z);
       // v2GlobalFull = new Vector3(v2GlobalFull.x + chunkPos.x, v2GlobalFull.y, v2GlobalFull.z + chunkPos.z);

        this.hexDirection = hexDirection;
        this.chunkPos = chunkPos;
    }

    public Vector3 GetGlobalV1()
    {
        return v1Global;
    }

    public void SetGlobalV1(Vector3 newV1Global)
    {
        v1Global = newV1Global;
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

    public void SetFullV1(Vector3 newV1GlobalFull)
    {
        v1GlobalFull = newV1GlobalFull;
    }

    public Vector3 GetGlobalV2()
    {
        return v2Global;
    }

    public void SetGlobalV2(Vector3 newV2Global)
    {
        v2Global = newV2Global;
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

    public void SetFullV2(Vector3 newV2GlobalFull)
    {
        v2GlobalFull = newV2GlobalFull;
    }

}
