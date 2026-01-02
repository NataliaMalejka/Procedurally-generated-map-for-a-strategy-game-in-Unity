using UnityEngine;
using UnityEngine.EventSystems;

public class SelectObject : MonoBehaviour
{
    [SerializeField] private GridHex grid;

    private HexCell selectedCell=null;
    private Unit selectedUnit = null;

    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonUp(0))
            {
                Select();
            }
            if(Input.GetMouseButtonUp(1))
            {

            }
        }
    }

    private void Select()
    {
        HexCell currentCell = GetCellUnderCursor();
        if (currentCell)
        {
            selectedCell = currentCell;
        }
    }

    private void Deselect()
    {
        
    }

    private HexCell GetCellUnderCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float layerY = GameSettings.Instance.CurrentLayer * HexData.LayersDistance;

        Plane plane = new Plane(Vector3.up, new Vector3(0f, layerY, 0f));

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hit = ray.GetPoint(distance);
            return grid.GetCell(hit);
        }

        return null;
    }
}
