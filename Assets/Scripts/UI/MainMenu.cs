using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public void ReadMapSize(int size)
    {
        GameSettings.Instance.SetMapSize(size);
    }

    public void ReadSeed(TMP_InputField seedInputField)
    {
        GameSettings.Instance.SetSeed(seedInputField.text);
    }

    public void ReadEarthBiome(Toggle toggle)
    {
        GameSettings.Instance.SetEarthBiome(toggle.isOn);
    }

    public void ReadColdBiome(Toggle toggle)
    {
        GameSettings.Instance.SetColdBiome(toggle.isOn);
    }

    public void ReadHotBiome(Toggle toggle)
    {
        GameSettings.Instance.SetHotBiome(toggle.isOn);
    }

    public void OnStartButton()
    {
        GameManager.Instance.StartGame();
    }

    public void OnQuitButton()
    {
        GameManager.Instance.QuitGame();
    }

}
