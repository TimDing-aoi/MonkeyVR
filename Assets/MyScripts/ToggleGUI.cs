using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PupilLabs;
using UnityEngine.InputSystem;

public class ToggleGUI : MonoBehaviour
{
    bool toggle = false;
    string toggleText = "Open";
    public GameObject gui;
    public Texture texture;
    public CalibrationController calibrationController;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 220, Screen.height - 100, 150, 60), toggleText))
        {
            toggle = !toggle;
            gui.SetActive(toggle);
        }

        if (toggle)
        {
            toggleText = "Close";
        }
        else
        {
            toggleText = "Open";
        }

        if (calibrationController.flagReward || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GUI.contentColor = Color.red;
        }
        else
        {
            GUI.contentColor = Color.black;
        }
        GUI.Box(new Rect(0f, 65f, 50f, 50f), texture);
    }

}
