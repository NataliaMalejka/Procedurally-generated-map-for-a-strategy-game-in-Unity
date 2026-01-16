using System.Collections.Generic;
using System;
using UnityEngine;

public class PathData
{
    public Queue<HexCell> PlannedPath = new();
    public List<HexCell> FullPath = new();
    public Dictionary<HexCell, int> CellTurn = new();
    public Dictionary<HexCell, int> StepCost = new();

    public bool Accepted { get; set; }
    public int CommitTurn;

    public void SetPath(List<HexCell> path, int movementPerTurn, int remainingMovement, Func<HexCell, HexCell, int> costFunc)
    {
        FullPath.Clear();
        FullPath.AddRange(path);

        PlannedPath.Clear();
        StepCost.Clear();
        CellTurn.Clear();

        for (int i = 1; i < path.Count; i++)
        {
            PlannedPath.Enqueue(path[i]);
            StepCost[path[i]] = costFunc(path[i - 1], path[i]);
        }

        int turn = 1;
        int lastTurn = 1;
        int mp = remainingMovement;

        for (int i = 1; i < path.Count; i++)
        {
            int cost = StepCost[path[i]];

            if (cost > mp)
            {
                turn++;
                mp = movementPerTurn;
            }

            mp -= cost;

            if ((turn != lastTurn || i == path.Count - 1) && turn != 1)
                CellTurn[path[i]] = turn;

            lastTurn = turn;
        }
    }

    public void Clear()
    {
        PlannedPath.Clear();
        FullPath.Clear();
        CellTurn.Clear();
        Accepted = false;
    }

    public void RemoveFirst()
    {
        if (FullPath.Count > 0)
            FullPath.RemoveAt(0);
    }
}