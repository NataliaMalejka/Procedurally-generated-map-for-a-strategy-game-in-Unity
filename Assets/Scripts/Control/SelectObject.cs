using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectObject : MonoBehaviour
{
    [SerializeField] private GridHex grid;
    [SerializeField] private LayerMask unitLayer;

    private HexCell selectedCell = null;
    private Unit selectedUnit = null;

    private HexCell hoveredCell = null;
    private List<HexCell> currentPath = new List<HexCell>();

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        HandleHover();

        if (Input.GetMouseButtonUp(0))
            SelectUnitClick();

        if (Input.GetMouseButtonUp(1))
            Deselect();
    }

    private void HandleHover()
    {
        if (selectedUnit == null)
            return;

        HexCell cell = GetCellUnderCursor();
        if (cell == null || cell == hoveredCell)
            return;

        hoveredCell = cell;
        PreviewPath(cell);
    }

    private void PreviewPath(HexCell targetCell)
    {
        ClearPath();

        HexCell start = selectedUnit.GetCurrentCell();
        if (start == null)
            return;

        currentPath = FindPath(start, targetCell);

        if (currentPath == null || currentPath.Count == 0)
            return;

        for (int i = 0; i < currentPath.Count; i++)
        {
            currentPath[i].SetSpriteColor(Color.white);
        }

        HexCell goal = currentPath[^1];
        goal.SetText((currentPath.Count - 1).ToString());
    }

    private void SelectUnitClick()
    {
        Unit unit = GetUnitUnderCursor();
        if (unit == null)
            return;

        if (selectedUnit == unit)
            return;

        ClearPath();
        hoveredCell = null;
        selectedUnit = unit;
    }

    private void Deselect()
    {
        ClearPath();
        hoveredCell = null;
        selectedUnit = null;
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

    private void SelectCell(HexCell cell)
    {
        if (selectedCell == cell)
            return;

        ClearPath();

        if (selectedCell != null)
            DeselectCell();

        selectedCell = cell;

        HexCell unitCell = selectedUnit.GetCurrentCell();

        if (unitCell != null)
        {
            currentPath = FindPath(unitCell, selectedCell);

            selectedCell.SetText("X");
            selectedCell.SetSpriteColor(Color.white);
            return;
        }
    }

    private void DeselectCell()
    {
        ClearPath();
        selectedCell.SetText("");
        selectedCell.SetSpriteColor(new Vector4(0, 0, 0, 0));
    }

    public List<HexCell> FindPath(HexCell start, HexCell goal)
    {
        var open = new List<PathNode>();
        var closed = new HashSet<HexCell>();

        open.Add(new PathNode(start, null, 0, HexData.HexDistance(start, goal)));

        while (open.Count > 0)
        {
            open.Sort((a, b) => a.f.CompareTo(b.f));
            PathNode current = open[0];
            open.RemoveAt(0);

            if (current.cell == goal)
                return ReconstructPath(current);

            closed.Add(current.cell);

            foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
            {
                HexCell neighbor = current.cell.GetNeighbor(dir);
                if (neighbor == null)
                    continue;

                if (neighbor.IsMountain)
                    continue;

                if (closed.Contains(neighbor))
                    continue;

                int heightDiff = Mathf.Abs(
                    neighbor.TerrainLevelIndex - current.cell.TerrainLevelIndex
                );

                if (heightDiff > 1)
                    continue;

                int tentativeG = current.g + 1;

                PathNode existing = open.Find(n => n.cell == neighbor);
                if (existing == null)
                {
                    open.Add(new PathNode(
                        neighbor,
                        current,
                        tentativeG,
                        HexData.HexDistance(neighbor, goal)
                    ));
                }
                else if (tentativeG < existing.g)
                {
                    existing.g = tentativeG;
                    existing.parent = current;
                }
            }
        }

        return null;
    }

    private List<HexCell> ReconstructPath(PathNode node)
    {
        List<HexCell> path = new List<HexCell>();
        int index = 0;    

        while (node != null)
        {
            path.Add(node.cell);
            node = node.parent;
        }

        path.Reverse();

        foreach (HexCell cell in path)
        {
            cell.SetSpriteColor(Color.white);
            cell.SetText(index.ToString());
            index++;
        }

        return path;
    }

    private void ClearPath()
    {
        foreach (HexCell cell in currentPath)
        {
            cell.SetText("");
            cell.SetSpriteColor(new Vector4(0, 0, 0, 0));
        }
        currentPath.Clear();
    }
}

class PathNode
{
    public HexCell cell;
    public PathNode parent;
    public int g; 
    public int h; 
    public int f => g + h;

    public PathNode(HexCell cell, PathNode parent, int g, int h)
    {
        this.cell = cell;
        this.parent = parent;
        this.g = g;
        this.h = h;
    }
}
