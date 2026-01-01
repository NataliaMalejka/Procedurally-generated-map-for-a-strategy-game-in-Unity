using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public int CurrentTurn { get; private set; } = 1;

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
            units.Add(unit);
    }

    public void UnregisterUnit(Unit unit)
    {
        units.Remove(unit);
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
        }
    }

    public bool AreAllUnitsDone()
    {
        foreach (var unit in units)
        {
            if (unit.HasActionsRemaining)
                return false;
        }

        return true;
    }
}
