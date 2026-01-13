using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public int CurrentTurn { get; private set; } = 0;

    private readonly List<Unit> units = new List<Unit>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterUnit(Unit unit)
    {
        if (!units.Contains(unit))
        {
            units.Add(unit);
            unit.OnCellPassed += SelectObject.Instance.HandleUnitPassedCell;
        }
    }

    public void EndTurn()
    {
        if(GameManager.Instance.State == GameState.Playing)
            StartNextTurn();
    }

    private void StartNextTurn()
    {
        CurrentTurn++;

        foreach (var unit in units)
        {
            unit.OnTurnStart();

            RedrawAllAcceptedPaths();
        }
    }

    public void RedrawAllAcceptedPaths()
    {
        foreach (var unit in units)
        {
            if (unit.Path.Accepted && unit.Path.FullPath.Count > 0)
            {
                SelectObject.Instance.DrawUnitPath(unit);
            }
        }
    }

    public Unit GetUnit(int index)
    {
        if (index < 0 || index >= units.Count) 
            return null;

        return units[index];
    }

    public int GetUnitCount()
    {
        return units.Count;
    }
}
