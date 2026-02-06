using UnityEngine;


// changing layers, ending turn and switching units
public class GameplayPanel : MonoBehaviour
{
    // Reference to the map camera controller
    [SerializeField] private MapCamera mapCamera;

    // Moves the camera to the upper map layer
    public void UpperLayer()
    {
        mapCamera.UpperLayer();
    }

    // Moves the camera to the lower map layer
    public void LowerLayer()
    {
        mapCamera.LowerLayer();
    }

    // Ends the current turn
    public void NextTurn()
    {
        TurnManager.Instance.EndTurn();
    }

    // Selects the next unit and moves the camera to its position
    public void NextUnit()
    {
        int index = mapCamera.UnitIndex + 1;

        if (index >= TurnManager.Instance.GetUnitCount() || index < 0) 
            index = 0;

        Unit unit = TurnManager.Instance.GetUnit(index);

        mapCamera.SetCameraUnitPos(unit);
    }
}
