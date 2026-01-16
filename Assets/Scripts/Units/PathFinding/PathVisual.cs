using UnityEngine;

public class PathVisual
{
    public void Clear(Unit unit)
    {
        if (unit == null || unit.PathMarkers == null || unit.PathMarkers.Count == 0)
            return;

        foreach (var m in unit.PathMarkers)
        {
            if (m.Cell != null)
                m.Cell.Marker = null;

            PathMarkerPool.Instance.Release(m);
        }

        unit.PathMarkers.Clear();
    }

    public void DrawPreview(Unit unit)
    {
        Clear(unit);

        var path = unit.Path.FullPath;
        var stepCost = unit.Path.StepCost;
        var cellTurn = unit.Path.CellTurn;

        int mp = unit.CurrentMovementPoints;
        bool canMove = true;

        for (int i = 1; i < path.Count; i++)
        {
            var cell = path[i];
            var marker = PathMarkerPool.Instance.Get();
            marker.Cell = cell;
            cell.Marker = marker;
            marker.SetWorldPosition(cell.transform.position + cell.UiPos);

            Color color = Color.gray;
            string label = null;

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

            if (cellTurn.TryGetValue(cell, out int turn) && turn > 1)
                label = turn.ToString();

            marker.Show(color, label);

            unit.PathMarkers.Add(marker);
            cell.Marker = marker;
        }
    }

    public void DrawCommitted(Unit unit)
    {
        Clear(unit);

        int passedTurns = TurnManager.Instance.CurrentTurn - unit.Path.CommitTurn;

        foreach (var cell in unit.Path.PlannedPath)
        {
            var marker = PathMarkerPool.Instance.Get();
            marker.Cell = cell;

            marker.SetWorldPosition(cell.transform.position + cell.UiPos);

            string label = null;

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