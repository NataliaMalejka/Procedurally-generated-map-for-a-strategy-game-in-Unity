using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private Animator unitAnimator;

    [SerializeField] private GameObject LandPart;
    [SerializeField] private GameObject OceanPart;

    [SerializeField] private int maxMovementPoints = 6;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 720f;

    private HexCell currentCell;
    private int currentMovementPoints;
    private bool isMoving;

    public readonly PathData Path = new();
    public event Action<Unit, HexCell> OnCellPassed;

    public readonly List<PathMarker> PathMarkers = new();
    public bool IsMoving => isMoving;
    public int MaxMovementPoints => maxMovementPoints;
    public HexCell CurrentCell => currentCell;
    public int CurrentMovementPoints => currentMovementPoints;

    private int layerIndex = 0;
    public int LayerIndex
    {
        get { return layerIndex; }
        set { layerIndex = value; }
    }


    private void OnEnable()
    {
        currentMovementPoints = maxMovementPoints;
    }

    public void OnTurnStart()
    {
        currentMovementPoints = maxMovementPoints;

        StartMove();
    }

    public void StartMove()
    {
        if (isMoving)
            return;

        if (!Path.Accepted || Path.PlannedPath.Count == 0)
            return;

        StartCoroutine(MoveRoutine());
    }

    public void SetCurrentCell(HexCell cell)
    {
        currentCell = cell;
    }

    private IEnumerator MoveRoutine()
    {
        isMoving = true;

        unitAnimator.SetBool("Walk", true);

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
            OnCellPassed?.Invoke(this, passed);
        }

        unitAnimator.SetBool("Walk", false);
        isMoving = false;

        if (Path.PlannedPath.Count == 0)
            Path.Clear();
    }

    private void FinalizeCellChange(HexCell next)
    {
        if (currentCell != null)
            currentCell.IsUnit = false;

        if(!currentCell.IsOcean && next.IsOcean)
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

        var pos = Vector3.zero;

        if (next.IsOcean)
        {
            pos.y = HexData.waterLevel +1f;
        }
        else
            pos.y = next.CentreTerrainLevel + 4f;

        transform.localPosition = pos;
    }

    private IEnumerator MoveToCell(HexCell target)
    {
        Vector3 start = transform.position;
        Vector3 end = target.transform.position;

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

        if (target.IsOcean)
            end.y += HexData.waterLevel + 1f;
        else
            end.y += target.CentreTerrainLevel + 4f;

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
