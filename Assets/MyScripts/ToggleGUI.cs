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
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 50);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        string text = string.Format("Good Trials / Total Trials: {0}/{1}", calibrationController.numCorrect, calibrationController.trialNum);
        string stimutext = string.Format("Total Trials: {0}", calibrationController.trialNum + 1);
        if (calibrationController.flagMicroStimu)
        {
            GUI.Label(rect, stimutext, style);
        }
        else if(calibrationController.flagFuseTest)
        {
            GUI.Label(rect, text, style);
        }
        else
        {
            GUI.Label(rect, "", style);
        }

        if (GUI.Button(new Rect(Screen.width - 275, Screen.height - 70, 150, 60), toggleText))
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

        if (calibrationController.flagReward || Keyboard.current.spaceKey.isPressed)
        {
            GUI.contentColor = Color.red;
        }
        else
        {
            GUI.contentColor = Color.black;
        }
        GUI.Box(new Rect(0f, 30f, 50f, 50f), texture);

        if (calibrationController.flagStimu /*|| Keyboard.current.rightAltKey.isPressed*/)
        {
            GUI.contentColor = Color.green;
        }
        else
        {
            GUI.contentColor = Color.black;
        }
        GUI.Box(new Rect(1000f, 30f, 50f, 50f), texture);
    }
}
