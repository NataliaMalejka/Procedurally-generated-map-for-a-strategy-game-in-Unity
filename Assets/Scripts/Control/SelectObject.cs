using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Handles unit selection, path preview, path acceptance
// A* pathfinding
public class SelectObject : MonoBehaviour
{
    public static SelectObject Instance { get; private set; }

    [SerializeField] private GridHex grid;
    [SerializeField] private LayerMask unitLayer;

    // Currently selected unit
    private Unit selectedUnit = null;

    // Cell currently hovered by the mouse
    private HexCell hoveredCell = null;
    // Current previewed path
    private List<HexCell> currentPath = new List<HexCell>();

    // Responsible for drawing path markers
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

    // Returns the unit currently under the mouse cursor (if any).
    private Unit GetUnitUnderCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, unitLayer))
        {
            return hit.collider.GetComponentInParent<Unit>();
        }

        return null;
    }

    // Returns the hex cell under the mouse cursor on the currently active layer
    private HexCell GetCellUnderCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Height of the currently active layer
        float layerY = GameSettings.Instance.CurrentLayer * HexData.LayersDistance;

        // Plane representing the current layer
        Plane plane = new Plane(Vector3.up, new Vector3(0f, layerY, 0f));

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hit = ray.GetPoint(distance);
            return grid.GetCell(hit);
        }

        return null;
    }

    // Checks if selection is currently allowed
    private bool CanSelect()
    {
        // Selection disabled if game is not in playing state
        if (GameManager.Instance.State != GameState.Playing)
            return false;

        // Prevent selection when hovering UI
        if (EventSystem.current.IsPointerOverGameObject())
            return false;

        return true;
    }

    private void Update()
    {
        if (!CanSelect())
            return;

        // Left mouse button
        if (Input.GetMouseButtonUp(0))
            HandleLeftClick();

        // Left mouse button
        if (Input.GetMouseButtonUp(1))
            HandleRightClick();

    }

    private void FixedUpdate()
    {
        if (!CanSelect())
            return;

        // Handle mouse hover over cells
        HandleHover();
    }

    // Handles path preview when hovering over cells
    private void HandleHover()
    {
        if (selectedUnit == null)
            return;

        // Only interact with units on the active layer
        if (selectedUnit.LayerIndex != GameSettings.Instance.CurrentLayer)
            return;

        // No preview if unit is already moving or path is accepted
        if (selectedUnit.IsMoving || selectedUnit.Path.Accepted)
            return;

        HexCell cell = GetCellUnderCursor();
        if (cell == null || cell == hoveredCell)
            return;

        // Only allow the same layer
        if (cell.LayerIndex != selectedUnit.LayerIndex)
            return;

        hoveredCell = cell;
        PreviewPath(cell);
    }

    // Handles left mouse button click
    private void HandleLeftClick()
    {
        // Try selecting a unit
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

        // Accept the previewed path
        if (!selectedUnit.Path.Accepted)
        {
            AcceptPath();
        }
    }

    // Handles right mouse button click
    private void HandleRightClick()
    {
        if (selectedUnit == null)
            return;

        if (selectedUnit.IsMoving)
            return;

        // Cancel accepted path
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
            // Unselect unit
            pathVisual.Clear(selectedUnit);
            selectedUnit = null;
        }
    }

    // Unselects the currently selected unit
    public void UnselectUnit()
    {
        if (selectedUnit == null)
            return;

        pathVisual.Clear(selectedUnit);
        currentPath.Clear();
        hoveredCell = null;

        selectedUnit = null;
    }

    // Selects a unit on the current layer
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

    // Accepts the current previewed path and starts unit movement
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

    // Previews a path from the unit to the target cell.
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


    // Removes path markers and updates path data
    // Called when a unit passes through a cell
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

    // Clears all path markers of a unit
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

    // Finds a path between two hex
    public List<HexCell> FindPath(HexCell start, HexCell goal)
    {
        // Pathfinding is allowed only within the same layer
        if (start.LayerIndex != goal.LayerIndex)
            return null;

        var open = new List<PathNode>();
        var closed = new HashSet<HexCell>();

        // Add the start node with cost = 0 and heuristic = hex distance to goal
        open.Add(new PathNode(start, null, 0, HexData.HexDistance(start, goal)));

        while (open.Count > 0)
        {
            // Sort by cost 
            open.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            PathNode current = open[0];
            open.RemoveAt(0);

            // If the goal is reached, reconstruct and return the path
            if (current.Cell == goal)
                return ReconstructPath(current);

            closed.Add(current.Cell);

            foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
            {
                HexCell neighbour = current.Cell.GetNeighbor(dir);

                //skip uncorrect cells
                if (neighbour.LayerIndex != start.LayerIndex)
                    continue;

                if (neighbour == null || neighbour.IsMountain || closed.Contains(neighbour))
                    continue;

                if (Mathf.Abs(neighbour.TerrainLevelIndex - current.Cell.TerrainLevelIndex) > 1)
                    continue;

                // Calculate movement cost
                int moveCost = GetMoveCost(current.Cell, neighbour);
                int costFromStart = current.costFromStart + moveCost;

                PathNode existing = open.Find(p => p.Cell == neighbour);

                if (existing == null)// Add new node
                {
                    open.Add(new PathNode(neighbour, current, costFromStart, HexData.HexDistance(neighbour, goal)));
                }
                else if (costFromStart < existing.costFromStart)// Update node
                {
                    existing.Update(current, costFromStart);
                }
            }
        }
        return null; // No path found
    }

    // Reconstructs path by walking backwards from goal node
    private List<HexCell> ReconstructPath(PathNode node)
    {
        List<HexCell> path = new();
        // From goal to start
        while (node != null)
        {
            path.Add(node.Cell);
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }

    // Calculates movement cost between two hex cells
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
