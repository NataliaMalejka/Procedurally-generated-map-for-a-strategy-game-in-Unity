using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Handles main menu UI input and stores selected game settings
public class MainMenu : MonoBehaviour
{
    // Reads selected map size from UI and stores it in game settings
    public void ReadMapSize(int size)
    {
        GameSettings.Instance.SetMapSize(size);
    }

    // Reads seed value from input field
    public void ReadSeed(TMP_InputField seedInputField)
    {
        GameSettings.Instance.SetSeed(seedInputField.text);
    }

    // Enables or disables Earth biome
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

    // Enables or disables Cold biome
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

    // Enables or disables Hot biome
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

    // Starts the game using selected settings
    public void OnStartButton()
    {
        GameManager.Instance.StartGame();
    }

    // Quits the application
    public void OnQuitButton()
    {
        GameManager.Instance.QuitGame();
    }

}
