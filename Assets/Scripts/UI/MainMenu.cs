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
        if(toggle.isOn)
        {
            GameSettings.Instance.SetEarthBiome(0);
        }
        else
        {
            GameSettings.Instance.SetEarthBiome(-1);
        }
        
    }

    public void ReadColdBiome(Toggle toggle)
    {
        if(toggle.isOn)
        {
            GameSettings.Instance.SetColdBiome(0);
        }
        else
        {
            GameSettings.Instance.SetColdBiome(-1);
        }     
    }

    public void ReadHotBiome(Toggle toggle)
    {
        if(toggle.isOn)
        {
            GameSettings.Instance.SetHotBiome(0);
        }
        else
        {
            GameSettings.Instance.SetHotBiome(-1);
        }
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
