using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectObject : MonoBehaviour
{
    public static SelectObject Instance { get; private set; }

    [SerializeField] private GridHex grid;
    [SerializeField] private LayerMask unitLayer;

    private Unit selectedUnit = null;

    private HexCell hoveredCell = null;
    private List<HexCell> currentPath = new List<HexCell>();

    private PathVisual pathVisual = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private Unit GetUnitUnderCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, unitLayer))
        {
            return hit.collider.GetComponentInParent<Unit>();
        }

        return null;
    }

    private HexCell GetCellUnderCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float layerY = GameSettings.Instance.CurrentLayer * HexData.LayersDistance;

        Plane plane = new Plane(Vector3.up, new Vector3(0f, layerY, 0f));

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hit = ray.GetPoint(distance);
            return grid.GetCell(hit);
        }

        return null;
    }

    private bool CanSelect()
    {
        if (GameManager.Instance.State != GameState.Playing)
            return false;

        if (EventSystem.current.IsPointerOverGameObject())
            return false;

        return true;
    }

    private void Update()
    {
        if (!CanSelect())
            return;

        if (Input.GetMouseButtonUp(0))
            HandleLeftClick();

        if (Input.GetMouseButtonUp(1))
            HandleRightClick();

    }

    private void FixedUpdate()
    {
        if (!CanSelect())
            return;

        HandleHover();
    }

    private void HandleHover()
    {
        if (selectedUnit == null)
            return;

        if (selectedUnit.LayerIndex != GameSettings.Instance.CurrentLayer)
            return;

        if (selectedUnit.IsMoving || selectedUnit.Path.Accepted)
            return;

        HexCell cell = GetCellUnderCursor();
        if (cell == null || cell == hoveredCell)
            return;

        if (cell.LayerIndex != selectedUnit.LayerIndex)
            return;

        hoveredCell = cell;
        PreviewPath(cell);
    }

    private void HandleLeftClick()
    {
        Unit unit = GetUnitUnderCursor();
        if (unit != null)
        {
            SelectUnit(unit);
            return;
        }

        if (selectedUnit == null)
            return;

        if (selectedUnit.Path == null)
            return;

        if (currentPath == null || currentPath.Count == 0)
            return;

        if (!selectedUnit.Path.Accepted)
        {
            AcceptPath();
        }
    }


    private void HandleRightClick()
    {
        if (selectedUnit == null)
            return;

        if (selectedUnit.IsMoving)
            return;

        if (selectedUnit.Path.Accepted)
        {
            ClearUnitPathVisual(selectedUnit);
            selectedUnit.Path.Clear();
            selectedUnit.Path.Accepted = false;
            pathVisual.Clear(selectedUnit);
            hoveredCell = null;
            return;
        }
        else
        {
            pathVisual.Clear(selectedUnit);
            selectedUnit = null;
        }
    }

    public void UnselectUnit()
    {
        if (selectedUnit == null)
            return;

        pathVisual.Clear(selectedUnit);
        currentPath.Clear();
        hoveredCell = null;

        selectedUnit = null;
    }

    public void SelectUnit(Unit unit)
    {
        if (unit == null)
            return;

        if (unit.LayerIndex != GameSettings.Instance.CurrentLayer)
            return;

        pathVisual.Clear(selectedUnit);
        currentPath.Clear();
        hoveredCell = null;

        selectedUnit = unit;
    }

    private void AcceptPath()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        selectedUnit.Path.SetPath(currentPath, selectedUnit.MaxMovementPoints, selectedUnit.CurrentMovementPoints, GetMoveCost);

        selectedUnit.Path.CommitTurn = TurnManager.Instance.CurrentTurn;
        selectedUnit.Path.Accepted = true;

        pathVisual.DrawPreview(selectedUnit);

        TurnManager.Instance.RedrawAllAcceptedPaths();

        selectedUnit.StartMove();
    }

    private void PreviewPath(HexCell target)
    {
        if (selectedUnit == null)
            return;

        if (target.LayerIndex != selectedUnit.LayerIndex)
            return;

        if (selectedUnit.CurrentCell.LayerIndex != selectedUnit.LayerIndex)
            return;

        pathVisual.Clear(selectedUnit);

        HexCell start = selectedUnit.CurrentCell;
        if (start == null)
            return;

        currentPath = FindPath(start, target);
        if (currentPath == null || currentPath.Count == 0)
            return;

        selectedUnit.Path.SetPath(currentPath, selectedUnit.MaxMovementPoints, selectedUnit.CurrentMovementPoints, GetMoveCost);

        pathVisual.DrawPreview(selectedUnit);
    }

    public void HandleUnitPassedCell(Unit unit, HexCell cell)
    {
        if (cell.Marker != null)
        {
            PathMarkerPool.Instance.Release(cell.Marker);
            unit.PathMarkers.Remove(cell.Marker);
            cell.Marker = null;
        }

        unit.Path.CellTurn.Remove(cell);

        if (unit.Path.FullPath.Count > 0 && unit.Path.FullPath[0] == cell)
            unit.Path.FullPath.RemoveAt(0);
    }

    private void ClearUnitPathVisual(Unit unit)
    {
        if (unit.Path.FullPath == null)
            return;

        foreach (var cell in unit.Path.FullPath)
        {
            if (cell.Marker != null)
            {
                PathMarkerPool.Instance.Release(cell.Marker);
                unit.PathMarkers.Remove(cell.Marker);
                cell.Marker = null;
            }

        }
    }

    public void DrawUnitPath(Unit unit)
    {
        pathVisual.DrawCommitted(unit);
    }

    public List<HexCell> FindPath(HexCell start, HexCell goal)
    {
        if (start.LayerIndex != goal.LayerIndex)
            return null;

        var open = new List<PathNode>();
        var closed = new HashSet<HexCell>();

        open.Add(new PathNode(start, null, 0, HexData.HexDistance(start, goal)));

        while (open.Count > 0)
        {
            open.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            PathNode current = open[0];
            open.RemoveAt(0);

            if (current.Cell == goal)
                return ReconstructPath(current);

            closed.Add(current.Cell);

            foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
            {
                HexCell neighbour = current.Cell.GetNeighbor(dir);

                if (neighbour.LayerIndex != start.LayerIndex)
                    continue;

                if (neighbour == null || neighbour.IsMountain || closed.Contains(neighbour))
                    continue;

                if (Mathf.Abs(neighbour.TerrainLevelIndex - current.Cell.TerrainLevelIndex) > 1)
                    continue;

                int moveCost = GetMoveCost(current.Cell, neighbour);
                int costFromStart = current.costFromStart + moveCost;

                PathNode existing = open.Find(p => p.Cell == neighbour);

                if (existing == null)
                {
                    open.Add(new PathNode(neighbour, current, costFromStart, HexData.HexDistance(neighbour, goal) * 2));
                }
                else if (costFromStart < existing.costFromStart)
                {
                    existing.Update(current, costFromStart);
                }
            }
        }

        return null;
    }

    private List<HexCell> ReconstructPath(PathNode node)
    {
        List<HexCell> path = new();
        while (node != null)
        {
            path.Add(node.Cell);
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }

    public int GetMoveCost(HexCell from, HexCell to)
    {
        int cost;

        if (to.TerrainLevelIndex == from.TerrainLevelIndex)
            cost = 2;
        else if (to.TerrainLevelIndex > from.TerrainLevelIndex)
            cost = 3;
        else
            cost = 1;

        if (from.IsRiver != to.IsRiver)
            cost += 1;

        if (from.IsOcean != to.IsOcean)
            cost += 1;

        if (from.IsLake != to.IsLake)
            cost += 1;

        return cost;
    }
}
