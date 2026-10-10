using System.Collections.Generic;
using UnityEngine;

// Manages turn-based system
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public int CurrentTurn { get; private set; } = 0;

    // All registered units in the game
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

    // Registers a unit and subscribes to its events
    public void RegisterUnit(Unit unit)
    {
        if (!units.Contains(unit))
        {
            units.Add(unit);
            unit.OnCellPassed += SelectObject.Instance.HandleUnitPassedCell;
        }
    }

    // Attempts to end the current turn
    // Will not proceed if any unit is still moving
    public void EndTurn()
    {
        foreach (var unit in units)
        {
            if(unit.IsMoving)
                return;
        }

        SoundsManager.Instance.PlaySounds(SoundsManager.Sounds.NextTurn);

        if (GameManager.Instance.State == GameState.Playing)
            StartNextTurn();
    }

    // Starts a new turn and updates all units
    private void StartNextTurn()
    {
        CurrentTurn++;

        foreach (var unit in units)
        {
            unit.OnTurnStart();

            RedrawAllAcceptedPaths();
        }
    }

    // Redraws committed paths for all units
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

    // Returns a unit by index
    public Unit GetUnit(int index)
    {
        if (index < 0 || index >= units.Count) 
            return null;

        return units[index];
    }

    // Returns total number of registered units
    public int GetUnitCount()
    {
        return units.Count;
    }
}
