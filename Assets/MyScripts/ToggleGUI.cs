using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PupilLabs;
using UnityEngine.InputSystem;
using static PupilLabs.DataController;
using static Serial;
using System.IO.Ports;

public class ToggleGUI : MonoBehaviour
{
    bool CalibMenuToggle = false;
    public bool isRecording = false;
    bool isStimulating = false;
    float lastStimulate;
    public int marker = 0;
    string toggleText = "Open";
    string recordingText = "Start Recording";
    string stimulateText = "Stimulate";
    public GameObject gui;
    public GameObject stimgui;
    public GameObject stimcanvas;
    public Texture texture;
    public CalibrationController calibrationController;
    public AudioSource AudioSource;
    public AudioClip StimuTest;
    SerialPort sp = serial.sp;
    public static ToggleGUI guiref;

    private void OnEnable()
    {
        guiref = this;
    }

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
            stimcanvas.SetActive(false);
            GUI.Label(rect, stimutext, style);
        }
        else if(calibrationController.flagFuseTest)
        {
            stimgui.SetActive(false);
            GUI.Label(rect, text, style);
        }
        else
        {
            if (GUI.Button(new Rect(Screen.width - 1300, Screen.height - 220, 150, 60), recordingText))
            {
                isRecording = !isRecording;
                if (isRecording)
                {
                    dataController.startExtraRecording();
                    recordingText = "Recording";
                    print("Start Recording");
                }
                else
                {
                    dataController.stopExtraRecording();
                    recordingText = "Start Recording";
                    print("Stop Recording");
                }
            }

            float stimulationDuration = PlayerPrefs.GetFloat("StimuStimuDur");
            if (GUI.Button(new Rect(Screen.width - 1300, Screen.height - 570, 150, 60), stimulateText))
            {
                isStimulating = true;
                stimulateText = "Stimulating";
                print("Start Stimulating");
                lastStimulate = Time.time;
                SendMarker("m", stimulationDuration * 1000.0f);
                AudioSource.clip = StimuTest;
                AudioSource.Play();
            }

            if(Time.time - lastStimulate > stimulationDuration && isStimulating)
            {
                isStimulating = false;
                stimulateText = "Stimulate";
                print("Stop Stimulating");
            }

            GUI.Label(rect, "", style);
        }

        if (GUI.Button(new Rect(Screen.width - 275, Screen.height - 70, 150, 60), toggleText))
        {
            CalibMenuToggle = !CalibMenuToggle;
            gui.SetActive(CalibMenuToggle);
        }

        if (CalibMenuToggle)
        {
            toggleText = "Close";
        }
        else
        {
            toggleText = "Open";
        }

        if (calibrationController.flagReward || Keyboard.current.spaceKey.isPressed)
        {
            marker = 4;
            GUI.contentColor = Color.red;
        }
        else
        {
            marker = 0;
            GUI.contentColor = Color.black;
        }
        GUI.Box(new Rect(0f, 30f, 50f, 50f), texture);

        if (calibrationController.flagStimu || isStimulating /*|| Keyboard.current.rightAltKey.isPressed*/)
        {
            marker = 5;
            GUI.contentColor = Color.green;
        }
        else
        {
            if(marker != 4)
            {
                marker = 0;
            }
            GUI.contentColor = Color.black;
        }
        GUI.Box(new Rect(1000f, 30f, 50f, 50f), texture);
    }

    public async void SendMarker(string mark, float time)
    {
        string toSend = "i" + mark + time.ToString();

        /*switch (mark)
        {
            case "j":
                rewardTime.Add(Time.time);
                break;
            case "s":
                beginTime.Add(Time.time);
                break;
            case "e":
                endTime.Add(Time.time);
                break;
            default:
                break;
        }*/

        sp.Write(toSend);

        await new WaitForSeconds(time / 1000.0f);
    }
}
