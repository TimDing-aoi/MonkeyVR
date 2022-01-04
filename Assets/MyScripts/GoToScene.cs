using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //UnityEngine.Debug.Log("Device Name: " + SystemInfo.deviceName + "\nDevice Model: " + SystemInfo.deviceModel + "\nCPU Name: " + SystemInfo.processorType + "\nCPU Frequency: " + SystemInfo.processorFrequency + "\nGPU Name: " + SystemInfo.graphicsDeviceName);

        PlayerPrefs.SetString("Switch Mode", "experiment");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        try
        {
            switch (PlayerPrefs.GetInt("Scene"))
            {
                case 0:
                    SceneManager.LoadScene("Monkey2D");
                    break;
                case 1:
                    SceneManager.LoadScene("Human Arena");
                    break;
                case 2:
                    // mouse2D
                    break;
                case 3:
                    // mouse arena
                    break;
                case 4:
                    // mouse corridor
                    break;
                case 9:
                    SceneManager.LoadScene("Monkey2D");
                    break;
                default:
                    throw new Exception("Invalid Scene Selected.\n");
            }
            //SceneManager.UnloadSceneAsync("MainMenu");
        }
        catch (Exception e)
        {
            Debug.Log(e, this);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
