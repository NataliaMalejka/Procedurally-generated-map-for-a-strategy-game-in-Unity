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

    private void Update()
    {
        if(GameManager.Instance.State != GameState.Playing)
            return;

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        HandleHover();

        if (Input.GetMouseButtonUp(0))
            HandleLeftClick();

        if (Input.GetMouseButtonUp(1))
            HandleRightClick();
    }

    private void HandleHover()
    {
        if (selectedUnit == null)
            return;

        if (selectedUnit.IsMoving)
            return;

        if (selectedUnit.Path.Accepted)
            return;

        HexCell cell = GetCellUnderCursor();
        if (cell == null || cell == hoveredCell)
            return;

        hoveredCell = cell;
        PreviewPath(cell);
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

    private void HandleLeftClick()
    {
        Unit unit = GetUnitUnderCursor();
        if (unit != null)
        {
            SelectUnit(unit);
            return;
        }

        if (selectedUnit != null && currentPath.Count > 0 && !selectedUnit.Path.Accepted)
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
            pathVisual.Clear(currentPath);
            hoveredCell = null;
            return;
        }
        else
            selectedUnit = null;
        pathVisual.Clear(currentPath);
    }

    public void UnselectUnit()
    {
        if (selectedUnit == null)
            return;

        if (selectedUnit.Path.Accepted)
        {
            //ClearUnitPathVisual(selectedUnit);
            //selectedUnit.Path.Clear();

            //pathVisual.Clear(currentPath);
            hoveredCell = null;
            return;
        }

        //pathVisual.Clear(currentPath);
        selectedUnit = null;

    }

    public void SelectUnit(Unit unit)
    {
        selectedUnit = unit;
    }

    private void AcceptPath()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        selectedUnit.Path.SetPath(currentPath, selectedUnit.MaxMovementPoints, selectedUnit.CurrentMovementPoints, GetMoveCost);

        selectedUnit.Path.CommitTurn = TurnManager.Instance.CurrentTurn;
        selectedUnit.Path.Accepted = true;

        //pathVisual.Clear(currentPath);
        pathVisual.DrawPreview(selectedUnit);

        TurnManager.Instance.RedrawAllAcceptedPaths();

        selectedUnit.StartMove();
    }

    private void PreviewPath(HexCell target)
    {
        pathVisual.Clear(currentPath);

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
        cell.SetText("");
        cell.SetSpriteColor(new Color(0, 0, 0, 0));

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
            cell.SetText("");
            cell.SetSpriteColor(new Color(0, 0, 0, 0));
        }
    }

    public void DrawUnitPath(Unit unit)
    {
        pathVisual.DrawCommitted(unit);
    }

    public List<HexCell> FindPath(HexCell start, HexCell goal)
    {
        var open = new List<PathNode>();
        var closed = new HashSet<HexCell>();

        open.Add(new PathNode(start, null, 0, HexData.HexDistance(start, goal)));

        while (open.Count > 0)
        {
            open.Sort((a, b) => a.F.CompareTo(b.F));
            PathNode current = open[0];
            open.RemoveAt(0);

            if (current.Cell == goal)
                return ReconstructPath(current);

            closed.Add(current.Cell);

            foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
            {
                HexCell n = current.Cell.GetNeighbor(dir);
                if (n == null || n.IsMountain || closed.Contains(n))
                    continue;

                if (Mathf.Abs(n.TerrainLevelIndex - current.Cell.TerrainLevelIndex) > 1)
                    continue;

                int moveCost = GetMoveCost(current.Cell, n);
                int g = current.G + moveCost;

                PathNode existing = open.Find(p => p.Cell == n);

                if (existing == null)
                {
                    open.Add(new PathNode(
                        n,
                        current,
                        g,
                        HexData.HexDistance(n, goal) * 2
                    ));
                }
                else if (g < existing.G)
                {
                    existing.Update(current, g);
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

class PathNode
{
    public HexCell Cell;
    public PathNode Parent;
    public int G;
    public int H;
    public int F => G + H;

    public PathNode(HexCell cell, PathNode parent, int g, int h)
    {
        Cell = cell;
        Parent = parent;
        G = g;
        H = h;
    }

    public void Update(PathNode parent, int g)
    {
        Parent = parent;
        G = g;
    }
}

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

            if ((turn != lastTurn || i==path.Count-1) && turn!=1) 
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

public class PathVisual
{
    public void Clear(IEnumerable<HexCell> path)
    {
        if (path == null) return;

        foreach (var cell in path)
        {
            cell.SetText("");
            cell.SetSpriteColor(new Color(0, 0, 0, 0));
        }
    }

    public void DrawPreview(Unit unit)
    {
        var path = unit.Path.FullPath;
        var stepCost = unit.Path.StepCost;
        var cellTurn = unit.Path.CellTurn;

        int mp = unit.CurrentMovementPoints;
        bool canMoveThisTurn = true;

        for (int i = 1; i < path.Count; i++)
        {
            HexCell cell = path[i];

            cell.SetText("");
            cell.SetSpriteColor(Color.gray5);

            if (canMoveThisTurn)
            {
                int cost = stepCost[cell];

                if (cost <= mp)
                {
                    cell.SetSpriteColor(Color.gold);
                    mp -= cost;
                }
                else
                {
                    canMoveThisTurn = false;
                }
            }

            if (cellTurn.TryGetValue(cell, out int turn) && turn > 1)
            {
                cell.SetText(turn.ToString());
            }
        }
    }

    public void DrawCommitted(Unit unit)
    { 
        var path = unit.Path.FullPath; 
        var cellTurn = unit.Path.CellTurn; 

        int passedTurns = TurnManager.Instance.CurrentTurn - unit.Path.CommitTurn; 

        for (int i = 0; i < path.Count; i++) 
        { 
            if (i > 0) 
                path[i].SetSpriteColor(Color.gray5); 
            
            if (path[i] == unit.CurrentCell) 
                continue; 

            if (!cellTurn.TryGetValue(path[i], out int absoluteTurn)) 
                continue; 

            int remainingTurns = absoluteTurn - passedTurns; 

            if (remainingTurns <= 0) 
                continue; 

            path[i].SetText(remainingTurns.ToString());
        } 
    }
}

