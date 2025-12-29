using UnityEngine;
using UnityEngine.InputSystem;

public class MapCamera : MonoBehaviour
{
    [SerializeField] private Transform swivel;
    [SerializeField] private Transform stick;

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

    private void Start()
    {
        SetStartPos();
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

    private void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
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
    }

    private void UpdateLayer()
    {
        if (Input.GetKey(KeyCode.W))
        {
            if (currentLayer < GameSettings.Instance.getMaxLayerIndex())
                UpperLayer();
        }
        else if (Input.GetKey(KeyCode.S))
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
