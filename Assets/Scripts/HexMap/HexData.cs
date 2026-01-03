using UnityEngine;

public static class HexData
{
    public static float distanceToCorner = 10f;                             // a
    public static float distanceToEdge= distanceToCorner * 0.8660254038f;   // a * sqrt(3) / 2

    public static float solidPart = 0.75f;
    public static float distanceBetweenEdgesScaler = 1 - solidPart;

    public static int oceanDistanceLevelStep = 2;
    public static int levelStepHeight = 2;
    public static float waterLevel = levelStepHeight * 0.5f;
    public static float oceanWaterLevel = -1;

    public static float LayersDistance = 1000f;

    public static Vector3[] corners =
    {
        new Vector3(0f, 0f, distanceToCorner),                      //upper
        new Vector3(distanceToEdge, 0f, 0.5f * distanceToCorner),   //right upper
        new Vector3(distanceToEdge, 0f, -0.5f * distanceToCorner),  //right lowe
        new Vector3(0f, 0f, -distanceToCorner),                     //lower
        new Vector3(-distanceToEdge, 0f, -0.5f * distanceToCorner), //left lower
        new Vector3(-distanceToEdge, 0f, 0.5f * distanceToCorner)   //right lower
    };

    public static Vector3 GetCorner(int index)
    {
        return corners[index];
    }

    public static Vector3 GetSolidCorner(int index)
    {
        return corners[index] * solidPart;
    }

    public static Vector3 GetDistanceBetweenEdges(int index)
    {
        return (corners[index] + corners[(index + 1) % 6]) * distanceBetweenEdgesScaler;
    }

    public static int HexDistance(HexCell a, HexCell b)
    {
        HexCoordinates ca = a.Coordinates;
        HexCoordinates cb = b.Coordinates;

        return Mathf.Max(
            Mathf.Abs(ca.Q - cb.Q),
            Mathf.Abs(ca.R - cb.R),
            Mathf.Abs(ca.S - cb.S)
        );
    }
}
