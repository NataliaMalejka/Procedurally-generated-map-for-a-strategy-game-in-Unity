using UnityEngine;

// Responsible for visualizing unit movement paths
// using pooled path markers.
public class PathVisual
{
    // Removes all path markers associated with the unit
    // and returns them to the pool.
    public void Clear(Unit unit)
    {
        if (unit == null || unit.PathMarkers == null || unit.PathMarkers.Count == 0)
            return;

        foreach (var m in unit.PathMarkers)
        {
            // Remove marker reference from the cell
            if (m.Cell != null)
                m.Cell.Marker = null;

            // Return marker to object pool
            PathMarkerPool.Instance.Release(m);
        }

        // Clear marker list
        unit.PathMarkers.Clear();
    }

    // Draws a preview of the path before it is accepted.
    // Shows reachable cells in gold and unreachable ones in gray.
    public void DrawPreview(Unit unit)
    {
        // Remove previous preview markers
        Clear(unit);

        var path = unit.Path.FullPath;
        var stepCost = unit.Path.StepCost;
        var cellTurn = unit.Path.CellTurn;

        int mp = unit.CurrentMovementPoints;
        bool canMove = true;

        // Skip first cell (unit current position)
        for (int i = 1; i < path.Count; i++)
        {
            var cell = path[i];
            // Get a marker from the pool
            var marker = PathMarkerPool.Instance.Get();
            marker.Cell = cell;
            cell.Marker = marker;
            // Position marker in world space
            marker.SetWorldPosition(cell.transform.position + cell.UiPos);

            Color color = Color.gray;
            string label = null;

            // Check if the unit can still move this step
            if (canMove)
            {
                int cost = stepCost[cell];
                if (cost <= mp)
                {
                    mp -= cost;
                    color = Color.gold;
                }
                else
                    canMove = false;
            }

            // Show turn number if cell is reached in a later turn
            if (cellTurn.TryGetValue(cell, out int turn) && turn > 1)
                label = turn.ToString();

            marker.Show(color, label);

            unit.PathMarkers.Add(marker);
            cell.Marker = marker;
        }
    }

    // Draws the committed path after the player confirms movement
    public void DrawCommitted(Unit unit)
    {
        // Clear existing markers
        Clear(unit);

        // Number of turns already passed since path commitment
        int passedTurns = TurnManager.Instance.CurrentTurn - unit.Path.CommitTurn;

        foreach (var cell in unit.Path.PlannedPath)
        {
            var marker = PathMarkerPool.Instance.Get();
            marker.Cell = cell;

            marker.SetWorldPosition(cell.transform.position + cell.UiPos);

            string label = null;

            // Display remaining turns for this cell
            if (unit.Path.CellTurn.TryGetValue(cell, out int turn))
            {
                int remaining = turn - passedTurns;
                if (remaining > 0)
                    label = remaining.ToString();
            }

            marker.Show(Color.gray, label);

            unit.PathMarkers.Add(marker);
            cell.Marker = marker;
        }
    }
}