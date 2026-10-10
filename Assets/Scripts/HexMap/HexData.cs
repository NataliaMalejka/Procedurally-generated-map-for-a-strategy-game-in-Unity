using UnityEngine;

// Global static data describing hex
public static class HexData
{
    // Distance from center to corner of a hex
    public static float distanceToCorner = 10f;                             // a
    public static float distanceToEdge= distanceToCorner * 0.8660254038f;   // a * sqrt(3) / 2

    // Percentage of hex considered solid (to create slope)
    public static float solidPart = 0.75f;
    public static float distanceBetweenEdgesScaler = 1 - solidPart;

    public static int oceanDistanceLevelStep = 2;
    public static int levelStepHeight = 2;
    public static float waterLevel = levelStepHeight * 0.5f;
    public static float oceanWaterLevel = -1;

    // Vertical distance between map layers
    public static float LayersDistance = 1000f;

    // Local-space positions of hex corners
    public static Vector3[] corners =
    {
        new Vector3(0f, 0f, distanceToCorner),                      //top
        new Vector3(distanceToEdge, 0f, 0.5f * distanceToCorner),   //top right
        new Vector3(distanceToEdge, 0f, -0.5f * distanceToCorner),  //bottom right 
        new Vector3(0f, 0f, -distanceToCorner),                     //bottom
        new Vector3(-distanceToEdge, 0f, -0.5f * distanceToCorner), //bottom left
        new Vector3(-distanceToEdge, 0f, 0.5f * distanceToCorner)   //top left
    };

    // Returns a corner position by index
    public static Vector3 GetCorner(int index)
    {
        return corners[index];
    }

    // Returns a solid corner position
    public static Vector3 GetSolidCorner(int index)
    {
        return corners[index] * solidPart;
    }

    // Calculates hex distance using cube coordinates
    public static int HexDistance(HexCell a, HexCell b)
    {
        HexCoordinates ca = a.Coordinates;
        HexCoordinates cb = b.Coordinates;

        return Mathf.Max(Mathf.Abs(ca.Q - cb.Q), Mathf.Abs(ca.R - cb.R), Mathf.Abs(ca.S - cb.S));
    }
}
