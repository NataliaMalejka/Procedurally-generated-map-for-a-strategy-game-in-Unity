using System.Collections.Generic;
using System;

// Stores all movement related path data for a unit
// Handles path planning, movement costs and turn calculation
public class PathData
{
    // Queue of cells the unit will traverse in order
    public Queue<HexCell> PlannedPath = new();
    // Full path including starting cell
    public List<HexCell> FullPath = new();
    // Maps each cell to the turn number when it will be reached
    public Dictionary<HexCell, int> CellTurn = new();
    // Movement cost for each step
    public Dictionary<HexCell, int> StepCost = new();

    // Indicates whether the path has been accepted by the player
    public bool Accepted { get; set; }
    // Turn number when the path was committed
    public int CommitTurn;

    // Initializes and calculates path data.
    // path - Full path including starting cell
    // movementPerTurn - Max movement points per turn
    // remainingMovement - Remaining movement points this turn
    // costFunc -Function calculating movement cost between cells
    public void SetPath(List<HexCell> path, int movementPerTurn, int remainingMovement, Func<HexCell, HexCell, int> costFunc)
    {
        FullPath.Clear();
        FullPath.AddRange(path);

        PlannedPath.Clear();
        StepCost.Clear();
        CellTurn.Clear();

        // Fill planned path and step costs
        for (int i = 1; i < path.Count; i++)
        {
            PlannedPath.Enqueue(path[i]);
            StepCost[path[i]] = costFunc(path[i - 1], path[i]);
        }

        int turn = 1;
        int lastTurn = 1;
        int mp = remainingMovement;

        // Calculate in which turn each cell will be reached
        for (int i = 1; i < path.Count; i++)
        {
            int cost = StepCost[path[i]];

            if (cost > mp)
            {
                turn++;
                mp = movementPerTurn;
            }

            mp -= cost;

            // Store turn number 
            if ((turn != lastTurn || i == path.Count - 1) && turn != 1)
                CellTurn[path[i]] = turn;

            lastTurn = turn;
        }
    }

    // Clears all path data.
    public void Clear()
    {
        PlannedPath.Clear();
        FullPath.Clear();
        CellTurn.Clear();
        Accepted = false;
    }

    // Removes the first cell from the full path
    public void RemoveFirst()
    {
        if (FullPath.Count > 0)
            FullPath.RemoveAt(0);
    }
}