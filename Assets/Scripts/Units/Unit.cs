using System;
using System.Collections;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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
    public event Action<HexCell> OnCellPassed;

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

    public void OnTurnStart()
    {
        currentMovementPoints = maxMovementPoints;

        StartMove();
    }

    public void StartMove()
    {
        if (Path.Accepted && Path.PlannedPath.Count > 0)
        {
            unitAnimator.SetBool("Walk", true);
            Debug.Log("true");
            StartCoroutine(MoveRoutine());
        }
    }

    public void SetCurrentCell(HexCell cell)
    {
        currentCell = cell;
    }

    private IEnumerator MoveRoutine()
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


        unitAnimator.SetBool("Walk", false);
        Debug.Log("false");
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
            pos.y = HexData.waterLevel + 3f;
        }
        else
            pos.y = next.CentreTerrainLevel + 4f;

        transform.localPosition = pos;
    }

    private IEnumerator MoveToCell(HexCell target)
    {
        Vector3 start = transform.position;
        Vector3 end = target.transform.position;

        Quaternion targetRot = Quaternion.LookRotation((end - start).normalized);

        while (Quaternion.Angle(transform.rotation, targetRot) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        if (target.IsOcean)
        {
            end.y += HexData.waterLevel + 3f;
        }
        else
            end.y += target.CentreTerrainLevel + 4f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }
}