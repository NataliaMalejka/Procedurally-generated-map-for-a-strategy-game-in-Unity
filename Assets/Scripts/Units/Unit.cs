using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Represents a unit on the hex grid.
public class Unit : MonoBehaviour
{
    [SerializeField] private Animator unitAnimator;

    // MOdel used when unit is on land or ocean
    [SerializeField] private GameObject LandPart;
    [SerializeField] private GameObject OceanPart;

    [SerializeField] private int maxMovementPoints = 6;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 720f;

    // Current cell the unit occupies
    private HexCell currentCell;
    // Remaining movement points for the current turn
    private int currentMovementPoints;
    // Indicates whether the unit is currently moving
    private bool isMoving;

    // Path data assigned to this unit
    public readonly PathData Path = new();
    // Event whenever the unit passes a cell
    public event Action<Unit, HexCell> OnCellPassed;
    // Visual markers used to display the path
    public readonly List<PathMarker> PathMarkers = new();
    public bool IsMoving => isMoving;
    public int MaxMovementPoints => maxMovementPoints;
    public HexCell CurrentCell => currentCell;
    public int CurrentMovementPoints => currentMovementPoints;

    // Layer index 
    private int layerIndex = 0;
    public int LayerIndex
    {
        get { return layerIndex; }
        set { layerIndex = value; }
    }

    private void OnEnable()
    {
        // Set movement points
        currentMovementPoints = maxMovementPoints;
    }

    // Resets movement points and starts movement if a path is accepted
    public void OnTurnStart()
    {
        currentMovementPoints = maxMovementPoints;

        StartMove();
    }

    // Starts unit movement along the planned path
    public void StartMove()
    {
        if (isMoving)
            return;

        if (!Path.Accepted || Path.PlannedPath.Count == 0)
            return;

        StartCoroutine(MoveRoutine());
    }

    // Sets the cell currently occupied by the unit
    public void SetCurrentCell(HexCell cell)
    {
        currentCell = cell;
    }

    // Handling step-by-step movement along the path.
    private IEnumerator MoveRoutine()
    {
        isMoving = true;

        unitAnimator.SetBool("Walk", true);

        while (Path.PlannedPath.Count > 0)
        {
            HexCell next = Path.PlannedPath.Peek();
            int stepCost = SelectObject.Instance.GetMoveCost(currentCell, next);

            // Stop if not enough movement points
            if (stepCost > currentMovementPoints)
                break;

            currentMovementPoints -= stepCost;
            Path.PlannedPath.Dequeue();

            HexCell passed = currentCell;

            yield return MoveToCell(next);
            FinalizeCellChange(next);

            Path.RemoveFirst();
            OnCellPassed?.Invoke(this, passed);
        }

        unitAnimator.SetBool("Walk", false);
        isMoving = false;

        // Stop if not enough movement points
        if (Path.PlannedPath.Count == 0)
            Path.Clear();
    }

    // Updates unit state after entering a new cell
    private void FinalizeCellChange(HexCell next)
    {
        if (currentCell != null)
            currentCell.IsUnit = false;

        // Switch model depending on terrain type
        if (!currentCell.IsOcean && next.IsOcean)
        {
            OceanPart.SetActive(true);
            LandPart.SetActive(false);
        }
        else if (currentCell.IsOcean && !next.IsOcean)
        {
            LandPart.SetActive(true);
            OceanPart.SetActive(false);
        }

        currentCell = next;
        currentCell.IsUnit = true;

        transform.SetParent(next.transform, true);

        // Adjust vertical position based on terrain
        var pos = Vector3.zero;

        if (next.IsOcean)
        {
            pos.y = HexData.waterLevel +1f;
        }
        else
            pos.y = next.CentreTerrainLevel + 4f;

        transform.localPosition = pos;
    }

    // Smoothly rotates and moves the unit to the target cell
    private IEnumerator MoveToCell(HexCell target)
    {
        Vector3 start = transform.position;
        Vector3 end = target.transform.position;

        // Rotate towards movement direction
        Vector3 dir = end - start;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

            while (Quaternion.Angle(transform.rotation, targetRot) > 0.5f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // Adjust target height based on terrain
        if (target.IsOcean)
            end.y += HexData.waterLevel + 1f;
        else
            end.y += target.CentreTerrainLevel + 4f;

        // Smooth movement interpolation
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        Vector3 euler = transform.eulerAngles;
        transform.eulerAngles = new Vector3(0f, euler.y, 0f);
    }
}
