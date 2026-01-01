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
        TurnManager.Instance.EndTurn();
    }

    public void NextUnit()
    {

    }
}
