using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public enum EdgeType
{
    Flat,
    Smooth,
    Cliff
}

public struct Edge
{
    EdgeType edgeType;

    private Vector3 v1;
    private Vector3 v2;

    private HexDirection hexDirection;

    public Edge(EdgeType edgeType, HexDirection hexDirection, int terrainLevel, Vector3 hexPos)
    {
        Vector3 centre = new Vector3(
           hexPos.x,
           terrainLevel,
           hexPos.z
        );

        this.edgeType = edgeType;

        this.v1 = centre + HexData.GetSolidCorner((int)hexDirection);
        this.v2 = centre + HexData.GetSolidCorner((int)(hexDirection + 1)%6);

        this.hexDirection = hexDirection;
    }
}
