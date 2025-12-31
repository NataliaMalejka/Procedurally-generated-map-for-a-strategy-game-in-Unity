using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MapCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform swivel;
    [SerializeField] private Transform stick;

    [SerializeField] private GridHex gridHex;

    [SerializeField] private float moveSpeedMin = 40f;
    [SerializeField] private float moveSpeedMax = 150f;
    [SerializeField] private float edgeSize = 20f;

    [SerializeField] private float zoom = 1f;
    [SerializeField] private float stickMinZoom = -250;
    [SerializeField] private float stickMaxZoom = -45;

    [SerializeField] private float swivelMinZoom = 90;
    [SerializeField] private float swivelMaxZoom = 45;

    [SerializeField] private float rotationSpeed = 180;
    private float rotationAngle;

    private int currentLayer = 0;

    private float UpBorder;
    private float downBorder;

    private float lastCameraX;

    private void Start()
    {
        SetStartPos();
        SetBorders();

        lastCameraX = transform.position.x;
    }

    private void Update()
    {
        float zoomDelta = Mouse.current.scroll.ReadValue().y * 0.01f;
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        float rotationDelta = 0f;
        if (Keyboard.current.aKey.isPressed) rotationDelta = -1f;
        if (Keyboard.current.dKey.isPressed) rotationDelta = 1f;

        if (rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        UpdatePosition();

        HandleHorizontalWrap();
        lastCameraX = transform.position.x;

        UpdateLayer();
    }

    private void SetStartPos()
    {
        if (GameSettings.Instance.IsEarthBiome() > -1)
        {
            SetLayer(GameSettings.Instance.IsEarthBiome());
        }
        else
        {
            SetLayer(0);
        }
    }

    private void SetBorders()
    {
        UpBorder = MapManager.Instance.zChunkCount * MapManager.Instance.zCellCount * 1.5f * HexData.distanceToCorner - HexData.distanceToCorner;
        downBorder = 0;
    }

    private void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);

        EnforceZBounds();
    }

    private void AdjustRotation(float delta)
    {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }
        else if (rotationAngle >= 360f)
        {
            rotationAngle -= 360f;
        }

        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);

        EnforceZBounds();
    }

    private void UpdatePosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        Vector2 move2D = Vector2.zero;

        if (mousePos.y >= screenSize.y - edgeSize)
        {
            float t = Mathf.InverseLerp(screenSize.y - edgeSize, screenSize.y, mousePos.y);
            float xOffset = (mousePos.x / screenSize.x - 0.5f) * 2f;
            move2D += new Vector2(xOffset, 1f) * t;
        }

        if (mousePos.y <= edgeSize)
        {
            float t = Mathf.InverseLerp(edgeSize, 0f, mousePos.y);
            float xOffset = (mousePos.x / screenSize.x - 0.5f) * 2f;
            move2D += new Vector2(xOffset, -1f) * t;
        }

        if (mousePos.x >= screenSize.x - edgeSize)
        {
            float t = Mathf.InverseLerp(screenSize.x - edgeSize, screenSize.x, mousePos.x);
            float yOffset = (mousePos.y / screenSize.y - 0.5f) * 2f;
            move2D += new Vector2(1f, yOffset) * t;
        }

        if (mousePos.x <= edgeSize)
        {
            float t = Mathf.InverseLerp(edgeSize, 0f, mousePos.x);
            float yOffset = (mousePos.y / screenSize.y - 0.5f) * 2f;
            move2D += new Vector2(-1f, yOffset) * t;
        }

        if (move2D.sqrMagnitude < 0.0001f)
            return;

        move2D = move2D.normalized;

        float zoomSpeed = Mathf.Lerp(moveSpeedMax, moveSpeedMin, zoom);

        Vector3 move3D = new Vector3(move2D.x, 0f, move2D.y);
        transform.position += move3D * zoomSpeed * Time.deltaTime;

        EnforceZBounds();
    }

    private void HandleHorizontalWrap()
    {
        float camX = transform.position.x;
        float delta = camX - lastCameraX;

        if (delta > 0f)
        {
            CheckMoveRight();
        }
        else if (delta < 0f)
        {
            CheckMoveLeft();
        }
    }

    private void CheckMoveRight()
    {
        float cameraRightEdge = cam.transform.position.x + cam.orthographicSize * cam.aspect;

        float rightmostX = GetColumnWorldX(gridHex.RightmostColumn);

        if (cameraRightEdge > rightmostX - gridHex.ColumnWidth * 3) 
        {
            MoveLeftColumnToRight();
        }
    }

    private void CheckMoveLeft()
    {
        float cameraLeftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;

        float leftmostX = GetColumnWorldX(gridHex.LeftmostColumn);

        if (cameraLeftEdge < leftmostX + gridHex.ColumnWidth * 3) 
        {
            MoveRightColumnToLeft();
        }
    }

    private void MoveLeftColumnToRight()
    {
        List<Chunk> column = gridHex.columns[gridHex.LeftmostColumn];
        gridHex.columns.Remove(gridHex.LeftmostColumn);

        int newColumnIndex = gridHex.RightmostColumn + 1;

        foreach (Chunk chunk in column)
        {
            Vector3 pos = chunk.transform.position;
            pos.x += gridHex.ColumnWidth * gridHex.xChunks;
            chunk.transform.position = pos;

            chunk.SetColumnIndex(newColumnIndex);
        }

        gridHex.columns[newColumnIndex] = column;

        gridHex.LeftmostColumn++;
        gridHex.RightmostColumn = newColumnIndex;
    }

    private void MoveRightColumnToLeft()
    {
        List<Chunk> column = gridHex.columns[gridHex.RightmostColumn];
        gridHex.columns.Remove(gridHex.RightmostColumn);

        int newColumnIndex = gridHex.LeftmostColumn - 1;

        foreach (Chunk chunk in column)
        {
            Vector3 pos = chunk.transform.position;
            pos.x -= gridHex.ColumnWidth * gridHex.xChunks;
            chunk.transform.position = pos;

            chunk.SetColumnIndex(newColumnIndex);
        }

        gridHex.columns[newColumnIndex] = column;

        gridHex.RightmostColumn--;
        gridHex.LeftmostColumn = newColumnIndex;
    }

    private float GetColumnWorldX(int columnIndex)
    {
        return columnIndex * gridHex.ColumnWidth;
    }

    private float GetCameraZViewExtent()
    {
        Vector3 camPos = cam.transform.position;

        Vector3 rayDir = Quaternion.Euler(cam.transform.eulerAngles.x + cam.fieldOfView * 0.5f,  cam.transform.eulerAngles.y, 0f) * Vector3.forward;

        if (Mathf.Abs(rayDir.y) < 0.0001f)
            return 0f;

        float t = -camPos.y / rayDir.y;

        if (t < 0f)
            return 0f;

        Vector3 hitPoint = camPos + rayDir * t;

        return hitPoint.z - transform.position.z;
    }

    private void GetCameraZViewRange(out float viewMinZ, out float viewMaxZ)
    {
        Vector3 camPos = cam.transform.position;

        float pitch = cam.transform.eulerAngles.x;
        float yaw = cam.transform.eulerAngles.y;
        float halfFov = cam.fieldOfView * 0.5f;

        Vector3 topDir = Quaternion.Euler(pitch - halfFov, yaw, 0f) * Vector3.forward;
        Vector3 bottomDir = Quaternion.Euler(pitch + halfFov, yaw, 0f) * Vector3.forward;

        viewMinZ = float.PositiveInfinity;
        viewMaxZ = float.NegativeInfinity;

        if (Mathf.Abs(topDir.y) > 0.0001f)
        {
            float t = -camPos.y / topDir.y;
            if (t > 0f)
            {
                float z = (camPos + topDir * t).z;
                viewMinZ = Mathf.Min(viewMinZ, z);
                viewMaxZ = Mathf.Max(viewMaxZ, z);
            }
        }

        if (Mathf.Abs(bottomDir.y) > 0.0001f)
        {
            float t = -camPos.y / bottomDir.y;
            if (t > 0f)
            {
                float z = (camPos + bottomDir * t).z;
                viewMinZ = Mathf.Min(viewMinZ, z);
                viewMaxZ = Mathf.Max(viewMaxZ, z);
            }
        }
    }

    private void EnforceZBounds()
    {
        GetCameraZViewRange(out float viewMinZ, out float viewMaxZ);

        Vector3 pos = transform.position;

        if (viewMinZ < downBorder)
        {
            pos.z += downBorder - viewMinZ;
        }
        else if (viewMaxZ > UpBorder)
        {
            pos.z -= viewMaxZ - UpBorder;
        }

        transform.position = pos;
    }

    private void ClampPositionZ()
    {
        Vector3 pos = transform.position;

        float viewExtent = GetCameraZViewExtent();
        float minZ = downBorder + viewExtent;
        float maxZ = UpBorder - viewExtent;

        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }

    private void UpdateLayer()
    {
        if (Input.GetKeyUp(KeyCode.W))
        {
            if (currentLayer < GameSettings.Instance.getMaxLayerIndex()-1)
                UpperLayer();
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            if(currentLayer > 0)
                LowerLayer();
        }
    }

    private void LowerLayer()
    {
        currentLayer--;

        var pos = transform.position;
        pos.y -= HexData.LayersDistance;
        transform.position = pos;
    }

    private void UpperLayer()
    {
        currentLayer++;

        var pos = transform.position;
        pos.y += HexData.LayersDistance;
        transform.position = pos;
    }

    private void SetLayer(int index)
    {
        currentLayer = index;

        var pos = transform.position;
        pos.y = index * HexData.LayersDistance;
        transform.position = pos;
    }
}
