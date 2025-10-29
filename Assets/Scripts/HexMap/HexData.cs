using UnityEngine;

public static class HexData
{
    public static float distanceToCorner = 10f;                             // a
    public static float distanceToEdge= distanceToCorner * 0.8660254038f;   // a * sqrt(3) / 2

    public static Vector3[] corners =
    {
        new Vector3(0f, 0f, distanceToCorner),                      //right
        new Vector3(distanceToEdge, 0f, 0.5f * distanceToCorner),   //right upper
        new Vector3(distanceToEdge, 0f, -0.5f * distanceToCorner),  //left upper
        new Vector3(0f, 0f, -distanceToCorner),                     //left
        new Vector3(-distanceToEdge, 0f, -0.5f * distanceToCorner), //left lower
        new Vector3(-distanceToEdge, 0f, 0.5f * distanceToCorner)   //right lower
    };
}
