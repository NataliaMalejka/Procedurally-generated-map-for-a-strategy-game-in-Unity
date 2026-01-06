using System;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private int maxMovementPoints = 6;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 720f;

    private HexCell currentCell;
    private int currentMovementPoints;
    private bool isMoving;

    public readonly PathData Path = new();

    public bool IsMoving => isMoving;
    public int MaxMovementPoints => maxMovementPoints;
    public HexCell CurrentCell => currentCell;
    public event Action<HexCell> OnCellPassed;
    public int CurrentMovementPoints => currentMovementPoints;

    public void OnTurnStart()
    {
        currentMovementPoints = maxMovementPoints;

        if (Path.Accepted && Path.PlannedPath.Count > 0)
            StartCoroutine(MoveRoutine(true));
    }

    //public void StartMove()
    //{
    //    if (Path.Accepted && Path.PlannedPath.Count > 0 && !isMoving)
    //        StartCoroutine(MoveRoutine(SelectObject.Instance.GetMoveCost));
    //}

    public void SetCurrentCell(HexCell cell)
    {
        currentCell = cell;
    }

    private IEnumerator MoveRoutine(bool fullTurn)
    {
        isMoving = true;

        while (Path.PlannedPath.Count > 0)
        {
            HexCell next = Path.PlannedPath.Peek();
            int stepCost = SelectObject.Instance.GetMoveCost(currentCell, next);

            if (stepCost > currentMovementPoints)
                break;

            currentMovementPoints -= stepCost;
            Path.PlannedPath.Dequeue();

            HexCell passed = currentCell;

            yield return MoveToCell(next);
            FinalizeCellChange(next);

            Path.RemoveFirst();
            OnCellPassed?.Invoke(passed);
        }

        isMoving = false;

        if (Path.PlannedPath.Count == 0)
            Path.Clear();
    }

    private void FinalizeCellChange(HexCell next)
    {
        if (currentCell != null)
            currentCell.IsUnit = false;

        currentCell = next;
        currentCell.IsUnit = true;

        transform.SetParent(next.transform, true);
        transform.localPosition = Vector3.zero;
    }

    public void StartMoveThisTurnOnly()
    {
        if (Path.Accepted && Path.PlannedPath.Count > 0 && !isMoving)
            StartCoroutine(MoveRoutine(false));
    }

    private IEnumerator MoveToCell(HexCell target)
    {
        Vector3 start = transform.position;
        Vector3 end = target.transform.position;
        //end.y += target.CentreTerrainLevel+4;


        Quaternion targetRot = Quaternion.LookRotation((end - start).normalized);

        while (Quaternion.Angle(transform.rotation, targetRot) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }
}