using UnityEngine;

public class GameplayPanel : MonoBehaviour
{
    [SerializeField] private MapCamera mapCamera;

    public void UpperLayer()
    {
        mapCamera.UpperLayer();
    }

    public void LowerLayer()
    {
        mapCamera.LowerLayer();
    }

    public void NextTurn()
    {
        SoundsManager.Instance.PlaySounds(SoundsManager.Sounds.NextTurn);

        TurnManager.Instance.EndTurn();
    }

    public void NextUnit()
    {
        int index = mapCamera.UnitIndex + 1;

        if (index >= TurnManager.Instance.GetUnitCount() || index < 0) 
            index = 0;

        Unit unit = TurnManager.Instance.GetUnit(index);

        mapCamera.SetCameraUnitPos(unit, index);
    }
}
