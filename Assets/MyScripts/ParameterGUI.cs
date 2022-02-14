using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PupilLabs;
using UnityEngine.UI;
using TMPro;

public class ParameterGUI : MonoBehaviour
{
    public CalibrationController calibrationController;
    public DataController dataController;

    float xSliderValue = 1f;
    float ySliderValue = 1f;
    float xScaleSliderValue = 1f;
    float yScaleSliderValue = 1f;
    float markerSliderValue = 0.025f;
    float xOffsetSliderValue = 0f;
    float yOffsetSliderValue = 0f;

    TMP_InputField markerScaleText;
    TMP_InputField xThresholdText;
    TMP_InputField yThresholdText;
    TMP_InputField xScaleText;
    TMP_InputField yScaleText;
    TMP_InputField xOffsetText;
    TMP_InputField yOffsetText;

    Slider markerScaleSlider;
    Slider xThresholdSlider;
    Slider yThresholdSlider;
    Slider xScaleSlider;
    Slider yScaleSlider;
    Slider xOffsetSlider;
    Slider yOffsetSlider;

    int selection = 0;
    int mode = 0;
    string[] selStrings = new string[] { "25cm Targets", "45cm Targets", "100cm Targets" };
    string[] selMode = new string[] { "Left", "Right", "Both" };

    private void OnEnable()
    {
        xSliderValue = PlayerPrefs.GetFloat("xSliderValue");
        ySliderValue = PlayerPrefs.GetFloat("ySliderValue");
        markerSliderValue = PlayerPrefs.GetFloat("markerSliderValue");
        xScaleSliderValue = PlayerPrefs.GetFloat("xScaleSliderValue");
        yScaleSliderValue = PlayerPrefs.GetFloat("yScaleSliderValue");
        xOffsetSliderValue = PlayerPrefs.GetFloat("xOffsetSliderValue");
        yOffsetSliderValue = PlayerPrefs.GetFloat("yOffsetSliderValue");

        markerScaleText = GameObject.Find("Marker Scale Text").GetComponent<TMP_InputField>();
        xThresholdText = GameObject.Find("X Threshold Text").GetComponent<TMP_InputField>();
        yThresholdText = GameObject.Find("Y Threshold Text").GetComponent<TMP_InputField>();
        xScaleText = GameObject.Find("X Scale Text").GetComponent<TMP_InputField>();
        yScaleText = GameObject.Find("Y Scale Text").GetComponent<TMP_InputField>();
        xOffsetText = GameObject.Find("X Offset Text").GetComponent<TMP_InputField>();
        yOffsetText = GameObject.Find("Y Offset Text").GetComponent<TMP_InputField>();

        markerScaleSlider = GameObject.Find("Marker Scale Slider").GetComponent<Slider>();
        xThresholdSlider = GameObject.Find("X Threshold Slider").GetComponent<Slider>();
        yThresholdSlider = GameObject.Find("Y Threshold Slider").GetComponent<Slider>();
        xScaleSlider = GameObject.Find("X Scale Slider").GetComponent<Slider>();
        yScaleSlider = GameObject.Find("Y Scale Slider").GetComponent<Slider>();
        xOffsetSlider = GameObject.Find("X Offset Text").GetComponent<Slider>();
        yOffsetSlider = GameObject.Find("Y Offset Text").GetComponent<Slider>();
    }



    //private void OnGUI()
    //{
    //    GUI.BeginGroup(new Rect(Screen.width - boxWidth + boxPad, Screen.height - boxHeight + boxPad, boxWidth, boxHeight));

    //    GUI.Box(new Rect(0, 0, boxWidth, boxHeight), "Parameters");

    //    GUI.Label(new Rect(20, 40, labelWidth, 20), "X Threshold");
    //    GUI.Label(new Rect(20, 80, labelWidth, 20), "Y Threshold");
    //    GUI.Label(new Rect(20, 120, labelWidth, 20), "Marker Size");
    //    GUI.Label(new Rect(20, 160, labelWidth, 20), "X Scale");
    //    GUI.Label(new Rect(20, 200, labelWidth, 20), "Y Scale");
    //    GUI.Label(new Rect(20, 240, labelWidth, 20), "X Offset");
    //    GUI.Label(new Rect(20, 280, labelWidth, 20), "Y Offset");
        
    //    xSliderValue = GUI.HorizontalSlider(new Rect(20, 60, sliderWidth, 20), xSliderValue, 1f, 90f);
    //    ySliderValue = GUI.HorizontalSlider(new Rect(20, 100, sliderWidth, 20), ySliderValue, 1f, 90f);
    //    markerSliderValue = GUI.HorizontalSlider(new Rect(20, 140, sliderWidth, 20), markerSliderValue, 0.0025f, 0.1f);
    //    xScaleSliderValue = GUI.HorizontalSlider(new Rect(20, 180, sliderWidth, 20), xScaleSliderValue, 1f, 2.5f);
    //    yScaleSliderValue = GUI.HorizontalSlider(new Rect(20, 220, sliderWidth, 20), yScaleSliderValue, 1f, 2.5f);
    //    xOffsetSliderValue = GUI.HorizontalSlider(new Rect(20, 260, sliderWidth, 20), xOffsetSliderValue, -2f, 2f);
    //    yOffsetSliderValue = GUI.HorizontalSlider(new Rect(20, 300, sliderWidth, 20), yOffsetSliderValue, -2f, 2f);

    //    xSliderValue = float.Parse(GUI.TextField(new Rect(150, 50, textWidth, 20), xSliderValue.ToString()));
    //    ySliderValue = float.Parse(GUI.TextField(new Rect(150, 90, textWidth, 20), ySliderValue.ToString()));
    //    markerSliderValue = float.Parse(GUI.TextField(new Rect(150, 130, textWidth, 20), markerSliderValue.ToString()));
    //    xScaleSliderValue = float.Parse(GUI.TextField(new Rect(150, 170, textWidth, 20), xScaleSliderValue.ToString()));
    //    yScaleSliderValue = float.Parse(GUI.TextField(new Rect(150, 210, textWidth, 20), yScaleSliderValue.ToString()));
    //    xOffsetSliderValue = float.Parse(GUI.TextField(new Rect(150, 250, textWidth, 20), xOffsetSliderValue.ToString()));
    //    yOffsetSliderValue = float.Parse(GUI.TextField(new Rect(150, 290, textWidth, 20), yOffsetSliderValue.ToString()));

    //    selection = GUI.SelectionGrid(new Rect(20, 330, 150, 100), selection, selStrings, 1);

    //    mode = GUI.SelectionGrid(new Rect(20, 460, 150, 50), mode, selMode, 3);

    //    //print(selection);

    //    GUI.EndGroup();

    //    calibrationController.xAngle = xSliderValue;
    //    calibrationController.yAngle = ySliderValue;
    //    calibrationController.markerSize = markerSliderValue;
    //    dataController.xScale = xScaleSliderValue;
    //    dataController.yScale = yScaleSliderValue;
    //    dataController.xOffset = xOffsetSliderValue;
    //    dataController.yOffset = yOffsetSliderValue;

    //    if (!calibrationController.IsCalibrating)
    //    {
    //        switch (selection)
    //        {
    //            case 0:
    //                calibrationController.plane.transform.position = new Vector3(0f, 0f, 0.25f);
    //                calibrationController.scale = 0.25f;
    //                calibrationController.UpdatePreviewMarkers();
    //                break;

    //            case 1:
    //                calibrationController.plane.transform.position = new Vector3(0f, 0f, 0.45f);
    //                calibrationController.scale = 0.45f;
    //                calibrationController.UpdatePreviewMarkers();
    //                break;

    //            case 2:
    //                calibrationController.plane.transform.position = new Vector3(0f, 0f, 1f);
    //                calibrationController.scale = 1f;
    //                calibrationController.UpdatePreviewMarkers();
    //                break;
    //        }
    //    }

    //    if (calibrationController.IsCalibrating)
    //    {
    //        switch (mode)
    //        {
    //            // Left Eye Open
    //            case 0:
    //                calibrationController.rightMask.SetActive(true);
    //                calibrationController.leftMask.SetActive(false);
    //                break;

    //            // Right Eye Open
    //            case 1:
    //                calibrationController.rightMask.SetActive(false);
    //                calibrationController.leftMask.SetActive(true);
    //                break;

    //            // Both Eyes Open
    //            case 2:
    //                calibrationController.rightMask.SetActive(false);
    //                calibrationController.leftMask.SetActive(false);
    //                break;
    //        }
    //    }

    //}
    private void OnDisable()
    {
        PlayerPrefs.SetFloat("xSliderValue", xSliderValue);
        PlayerPrefs.SetFloat("ySliderValue", ySliderValue);
        PlayerPrefs.SetFloat("markerSliderValue", markerSliderValue);
        PlayerPrefs.SetFloat("xScaleSliderValue", xScaleSliderValue);
        PlayerPrefs.SetFloat("yScaleSliderValue", yScaleSliderValue);
        PlayerPrefs.SetFloat("xOffsetSliderValue", xOffsetSliderValue);
        PlayerPrefs.SetFloat("yOffsetSliderValue", yOffsetSliderValue);
    }
}
