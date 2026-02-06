using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


// Controls the map camera:
// - zoom in/out
// - horizontal world wrapping
// - changing vertical layers
public class MapCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    // Control camera angle
    [SerializeField] private Transform swivel;
    // Control camera zoom
    [SerializeField] private Transform stick;

    [SerializeField] private GridHex gridHex;

    [SerializeField] private float moveSpeedMin = 40f;
    [SerializeField] private float moveSpeedMax = 150f;
    [SerializeField] private float edgeSize = 20f;

    [SerializeField] private float zoom = 1f;
    [SerializeField] private float stickMinZoom = -250;
    [SerializeField] private float stickMaxZoom = -45;

    [SerializeField] private float swivelMinZoom = 90;
    [SerializeField] private float swivelMaxZoom = 70;

    [SerializeField] private float cameraMoveSpeed = 50f;
    private Coroutine moveCameraCoroutine;

    // Z-axis map limits
    private float UpBorder;
    private float downBorder;

    // Used to detect horizontal movement direction
    private float lastCameraX;

    private int unitIndex = 0;

    public int UnitIndex
    {
        get { return unitIndex; }
        set { unitIndex = value; }
    }

    private void Start()
    {
        SetStartPos();
        SetBorders();

        lastCameraX = transform.position.x;
    }

    // Sets initial camera layer and position
    private void SetStartPos()
    {
        // If earth biome exists, use it. Otherwise start from layer 0
        if (GameSettings.Instance.IsEarthBiome() > -1)
        {
            SetLayer(GameSettings.Instance.IsEarthBiome());
        }
        else
        {
            SetLayer(0);
        }

        int currentlayer = GameSettings.Instance.CurrentLayer;
        // Center camera on the unit from the current layer
        SetPos(TurnManager.Instance.GetUnit(GameSettings.Instance.CurrentLayer));
    }

    // Smoothly moves the camera to the unit
    private void SetPos(Unit unit)
    {
        if (unit == null)
            return;

        SelectObject.Instance.UnselectUnit();

        // Set camera layer to match the unit
        SetLayer(unit.LayerIndex);
        unitIndex = unit.LayerIndex;

        // Keep pos Y unchanged, move only X and Z
        Vector3 targetPos = new Vector3(unit.transform.position.x, transform.position.y, unit.transform.position.z);

        if (moveCameraCoroutine != null)
            StopCoroutine(moveCameraCoroutine);

        moveCameraCoroutine = StartCoroutine(MoveCameraSmooth(targetPos));
    }

    // Selects a unit and redraws accepted paths.
    private void SelectUnit(Unit unit)
    {
        SelectObject.Instance.SelectUnit(unit);
        TurnManager.Instance.RedrawAllAcceptedPaths();
    }

    public void SetCameraUnitPos(Unit unit)
    {
        SetPos(unit);
        SelectUnit(unit);
    }

    // Smooth camera movement
    private IEnumerator MoveCameraSmooth(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, cameraMoveSpeed * Time.deltaTime);

            yield return null;
        }

        transform.position = targetPosition;
    }

    // Calculates map boundaries
    private void SetBorders()
    {
        UpBorder = MapManager.Instance.zChunkCount * MapManager.Instance.zCellCount * 1.5f * HexData.distanceToCorner - HexData.distanceToCorner;
        downBorder = 0;
    }

    private void LateUpdate()
    {
        // Mouse wheel zoom
        float zoomDelta = Mouse.current.scroll.ReadValue().y * 0.01f;
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        // Mouse edge movement
        UpdatePosition();

        // Horizontal infinite scrolling
        HandleHorizontalWrap();
        lastCameraX = transform.position.x;

        // Layer switching input
        UpdateLayer();
    }

    // Adjusts zoom level, camera angle and distance
    private void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        // Camera distance
        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        // Camera tilt
        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);

        EnforceZBounds();
    }

    // Handles camera movement when mouse is near screen edges
    private void UpdatePosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        Vector2 move2D = Vector2.zero;

        // Top edge
        if (mousePos.y >= screenSize.y - edgeSize)
        {
            float t = Mathf.InverseLerp(screenSize.y - edgeSize, screenSize.y, mousePos.y);
            float xOffset = (mousePos.x / screenSize.x - 0.5f) * 2f;
            move2D += new Vector2(xOffset, 1f) * t;
        }

        // Bottom edge
        if (mousePos.y <= edgeSize)
        {
            float t = Mathf.InverseLerp(edgeSize, 0f, mousePos.y);
            float xOffset = (mousePos.x / screenSize.x - 0.5f) * 2f;
            move2D += new Vector2(xOffset, -1f) * t;
        }

        // Right edge
        if (mousePos.x >= screenSize.x - edgeSize)
        {
            float t = Mathf.InverseLerp(screenSize.x - edgeSize, screenSize.x, mousePos.x);
            float yOffset = (mousePos.y / screenSize.y - 0.5f) * 2f;
            move2D += new Vector2(1f, yOffset) * t;
        }

        // Left edge
        if (mousePos.x <= edgeSize)
        {
            float t = Mathf.InverseLerp(edgeSize, 0f, mousePos.x);
            float yOffset = (mousePos.y / screenSize.y - 0.5f) * 2f;
            move2D += new Vector2(-1f, yOffset) * t;
        }

        if (move2D.sqrMagnitude < 0.0001f)
            return;

        move2D = move2D.normalized;

        // Speed depends on zoom level
        float zoomSpeed = Mathf.Lerp(moveSpeedMax, moveSpeedMin, zoom);

        Vector3 move3D = new Vector3(move2D.x, 0f, move2D.y);
        transform.position += move3D * zoomSpeed * Time.deltaTime;

        EnforceZBounds();
    }

    // Handles infinite horizontal scrolling of the map
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
        float cameraRightEdge = mainCam.transform.position.x + mainCam.orthographicSize * mainCam.aspect;

        float rightmostX = GetColumnWorldX(gridHex.RightmostColumn);

        if (cameraRightEdge > rightmostX - gridHex.ColumnWidth * 3)
        {
            MoveLeftColumnToRight();
        }
    }

    private void CheckMoveLeft()
    {
        float cameraLeftEdge = mainCam.transform.position.x - mainCam.orthographicSize * mainCam.aspect;

        float leftmostX = GetColumnWorldX(gridHex.LeftmostColumn);

        if (cameraLeftEdge < leftmostX + gridHex.ColumnWidth * 3)
        {
            MoveRightColumnToLeft();
        }
    }

    // Moves the leftmost column to the right side of the map
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

    // Moves the rightmost column to the left side of the map
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

    // Calculates visible Z range 
    private void GetCameraZViewRange(out float viewMinZ, out float viewMaxZ)
    {
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        viewMinZ = float.PositiveInfinity;
        viewMaxZ = float.NegativeInfinity;

        Ray bottomRay = mainCam.ViewportPointToRay(new Vector3(0.5f, 0f, 0f));
        if (groundPlane.Raycast(bottomRay, out float bottomDist))
        {
            float z = bottomRay.GetPoint(bottomDist).z;
            viewMinZ = Mathf.Min(viewMinZ, z);
            viewMaxZ = Mathf.Max(viewMaxZ, z);
        }

        Ray topRay = mainCam.ViewportPointToRay(new Vector3(0.5f, 1f, 0f));
        if (groundPlane.Raycast(topRay, out float topDist))
        {
            float z = topRay.GetPoint(topDist).z;
            viewMinZ = Mathf.Min(viewMinZ, z);
            viewMaxZ = Mathf.Max(viewMaxZ, z);
        }
    }

    // Prevents the camera from leaving Z map bounds
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

    // Handles layer change input (W/S).
    private void UpdateLayer()
    {
        if (Input.GetKeyUp(KeyCode.W))
        {
            UpperLayer();
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            LowerLayer();
        }
    }

    public void UpperLayer()
    {
        if (GameSettings.Instance.CurrentLayer < GameSettings.Instance.getMaxLayerIndex() - 1 && GameManager.Instance.State == GameState.Playing)
        {
            GameSettings.Instance.CurrentLayer++;

            SelectObject.Instance.UnselectUnit();

            var pos = transform.position;
            pos.y += HexData.LayersDistance;
            transform.position = pos;

            UpdateCullingMask();
        }
    }

    public void LowerLayer()
    {
        if (GameSettings.Instance.CurrentLayer > 0 && GameManager.Instance.State == GameState.Playing)
        {
            GameSettings.Instance.CurrentLayer--;

            SelectObject.Instance.UnselectUnit();

            var pos = transform.position;
            pos.y -= HexData.LayersDistance;
            transform.position = pos;

            UpdateCullingMask();
        }
    }

    // Sets camera to a specific layer index.
    private void SetLayer(int index)
    {
        GameSettings.Instance.CurrentLayer = index;

        var pos = transform.position;
        pos.y = index * HexData.LayersDistance;
        transform.position = pos;

        UpdateCullingMask();
    }

    // Updates camera culling mask for current layer
    private void UpdateCullingMask()
    {
        int mapLayer = LayerMask.NameToLayer($"Layer_{GameSettings.Instance.CurrentLayer}");
        int unitLayer = LayerMask.NameToLayer("Unit");
        int uiLayer = LayerMask.NameToLayer("UI");
        int defaultlayer = LayerMask.NameToLayer("Default");

        int mask = (1 << mapLayer) | (1 << unitLayer) | (1 << uiLayer) | (1 << defaultlayer);

        mainCam.cullingMask = mask;
    }
}
