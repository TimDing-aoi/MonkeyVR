using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PupilLabs;

public class ParameterGUI : MonoBehaviour
{
    public CalibrationController calibrationController;
    public DataController dataController;

    int boxWidth = 250;
    int boxHeight = 400;
    int sliderWidth = 100;
    int boxPad = -20;
    int sliderOffset = -50;
    int labelPad = 5;
    int labelWidth = 100;
    int textWidth = 75;
    int textOffset = 65;

    float xSliderValue = 1f;
    float ySliderValue = 1f;
    float xScaleSliderValue = 1f;
    float yScaleSliderValue = 1f;
    float markerSliderValue = 0.025f;


    private void OnEnable()
    {
        xSliderValue = PlayerPrefs.GetFloat("X Threshold");
        ySliderValue = PlayerPrefs.GetFloat("Y Threshold");
        markerSliderValue = PlayerPrefs.GetFloat("Marker Size");
    }

    private void OnGUI()
    {
        GUI.BeginGroup(new Rect(Screen.width - boxWidth + boxPad, Screen.height - boxHeight + boxPad, boxWidth, boxHeight));

        GUI.Box(new Rect(0, 0, boxWidth, boxHeight), "Parameters");

        GUI.Label(new Rect(20, 40, labelWidth, 20), "X Threshold");
        GUI.Label(new Rect(20, 80, labelWidth, 20), "Y Threshold");
        GUI.Label(new Rect(20, 120, labelWidth, 20), "Marker Size");
        GUI.Label(new Rect(20, 160, labelWidth, 20), "X Scale");
        GUI.Label(new Rect(20, 200, labelWidth, 20), "Y Scale");
        
        xSliderValue = GUI.HorizontalSlider(new Rect(20, 60, sliderWidth, 20), xSliderValue, 1f, 90f);
        ySliderValue = GUI.HorizontalSlider(new Rect(20, 100, sliderWidth, 20), ySliderValue, 1f, 90f);
        markerSliderValue = GUI.HorizontalSlider(new Rect(20, 140, sliderWidth, 20), markerSliderValue, 0.005f, 0.1f);
        xScaleSliderValue = GUI.HorizontalSlider(new Rect(20, 180, sliderWidth, 20), xScaleSliderValue, 1f, 2.5f);
        yScaleSliderValue = GUI.HorizontalSlider(new Rect(20, 220, sliderWidth, 20), yScaleSliderValue, 1f, 2.5f);

        xSliderValue = float.Parse(GUI.TextField(new Rect(150, 50, textWidth, 20), xSliderValue.ToString()));
        ySliderValue = float.Parse(GUI.TextField(new Rect(150, 90, textWidth, 20), ySliderValue.ToString()));
        markerSliderValue = float.Parse(GUI.TextField(new Rect(150, 130, textWidth, 20), markerSliderValue.ToString()));
        xScaleSliderValue = float.Parse(GUI.TextField(new Rect(150, 170, textWidth, 20), xScaleSliderValue.ToString()));
        yScaleSliderValue = float.Parse(GUI.TextField(new Rect(150, 210, textWidth, 20), yScaleSliderValue.ToString()));

        GUI.EndGroup();

        calibrationController.xAngle = xSliderValue;
        calibrationController.yAngle = ySliderValue;
        calibrationController.markerSize = markerSliderValue;
        dataController.xScale = xScaleSliderValue;
        dataController.yScale = yScaleSliderValue;
    }
}
