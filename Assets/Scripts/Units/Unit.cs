using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private int maxMovementPoints = 100;

    private int currentMovementPoints;

    public bool HasActionsRemaining => currentMovementPoints > 0;

    public void OnTurnStart()
    {
        currentMovementPoints = maxMovementPoints;
    }

    public void SpendMovementPoint()
    {
        if (currentMovementPoints <= 0)
            return;

        currentMovementPoints--;
    }
}
