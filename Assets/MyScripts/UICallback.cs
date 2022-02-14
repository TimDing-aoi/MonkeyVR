using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using PupilLabs;

public class UICallback : MonoBehaviour
{
    private CalibrationController calibrationController;
    private DataController dataController;
    private Slider slider;
    private TMP_InputField text;
    private Dropdown dropdown;
    private bool flagSliderOrText;
    private string objectName;

    // Start is called before the first frame update
    void Start()
    {
        objectName = string.Join(" ", this.name.Split(' ').Take(2));

        calibrationController = GameObject.Find("Calibration Controller").GetComponent<CalibrationController>();
        dataController = GameObject.Find("Data Controller").GetComponent<DataController>();

        if (GetComponent<Slider>() != null)
        {
            slider = this.GetComponent<Slider>();
            slider.value = PlayerPrefs.GetFloat((char.ToLowerInvariant(objectName[0]) + objectName.Substring(1)).Replace(" ", "") + "Value");
            //Debug.Log((char.ToLowerInvariant(name[0]) + name.Substring(1)).Replace(" ", "") + "Value");
            text = GameObject.Find(objectName + " Text").GetComponent<TMP_InputField>();
            text.text = slider.value.ToString();
            flagSliderOrText = true;
        } 
        else if (GetComponent<TMP_InputField>() != null)
        {
            text = this.GetComponent<TMP_InputField>();
            text.text = PlayerPrefs.GetFloat((char.ToLowerInvariant(objectName[0]) + objectName.Substring(1)).Replace(" ", "") + "Value").ToString();
            //Debug.Log((char.ToLowerInvariant(name[0]) + name.Substring(1)).Replace(" ", "") + "Value");
            slider = GameObject.Find(objectName + " Slider").GetComponent<Slider>();
            slider.value = float.Parse(text.text);
            flagSliderOrText = false;
        }
        else if (GetComponent<Dropdown>() != null)
        {
            dropdown = this.GetComponent<Dropdown>();
            dropdown.value = PlayerPrefs.GetInt((char.ToLowerInvariant(objectName[0]) + objectName.Substring(1)).Replace(" ", "") + "Value");
        }

        switch (objectName)
        {
            case "Marker Scale":
                calibrationController.markerSize = slider.value;
                break;

            case "X Threshold":
                calibrationController.xAngle = slider.value;
                break;

            case "Y Threshold":
                calibrationController.yAngle = slider.value;
                break;

            case "X Scale":
                dataController.xScale = slider.value;
                break;

            case "Y Scale":
                dataController.yScale = slider.value;
                break;

            case "X Offset":
                dataController.xOffset = slider.value;
                break;

            case "Y Offset":
                dataController.yOffset = slider.value;
                break;

            case "Target Selection":
                switch (dropdown.value)
                {
                    case 0:
                        calibrationController.plane.transform.position = new Vector3(0f, 0f, 0.25f);
                        calibrationController.scale = 0.25f;
                        calibrationController.UpdatePreviewMarkers();
                        break;

                    case 1:
                        calibrationController.plane.transform.position = new Vector3(0f, 0f, 0.45f);
                        calibrationController.scale = 0.45f;
                        calibrationController.UpdatePreviewMarkers();
                        break;

                    case 2:
                        calibrationController.plane.transform.position = new Vector3(0f, 0f, 1f);
                        calibrationController.scale = 1f;
                        calibrationController.UpdatePreviewMarkers();
                        break;
                }
                break;

            case "Eye Mode":
                switch (dropdown.value)
                {
                    // Left Eye Open
                    case 0:
                        calibrationController.rightMask.SetActive(true);
                        calibrationController.leftMask.SetActive(false);
                        break;

                    // Right Eye Open
                    case 1:
                        calibrationController.rightMask.SetActive(false);
                        calibrationController.leftMask.SetActive(true);
                        break;

                    // Both Eyes Open
                    case 2:
                        calibrationController.rightMask.SetActive(false);
                        calibrationController.leftMask.SetActive(false);
                        break;
                }
                break;
        }
    }

    public void OnValueChanged()
    {
        if (flagSliderOrText)
        {
            text.text = slider.value.ToString();
        }
        else
        {
            float prevValue = slider.value;
            try
            {
                slider.value = float.Parse(text.text);
                PlayerPrefs.SetFloat((char.ToLowerInvariant(objectName[0]) + objectName.Substring(1)).Replace(" ", "") + "Value", slider.value);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
                slider.value = prevValue;
            }
        }

        switch (objectName)
        {
            case "Marker Scale":
                calibrationController.markerSize = slider.value;
                break;

            case "X Threshold":
                calibrationController.xAngle = slider.value;
                break;

            case "Y Threshold":
                calibrationController.yAngle = slider.value;
                break;

            case "X Scale":
                dataController.xScale = slider.value;
                break;

            case "Y Scale":
                dataController.yScale = slider.value;
                break;

            case "X Offset":
                dataController.xOffset = slider.value;
                break;

            case "Y Offset":
                dataController.yOffset = slider.value;
                break;
        }
    }

    public void OnDropdownValueChanged()
    {
        switch (objectName)
        {
            case "Target Selection":
                switch (dropdown.value)
                {
                    case 0:
                        calibrationController.plane.transform.position = new Vector3(0f, 0f, 0.25f);
                        calibrationController.scale = 0.25f;
                        calibrationController.UpdatePreviewMarkers();
                        break;

                    case 1:
                        calibrationController.plane.transform.position = new Vector3(0f, 0f, 0.45f);
                        calibrationController.scale = 0.45f;
                        calibrationController.UpdatePreviewMarkers();
                        break;

                    case 2:
                        calibrationController.plane.transform.position = new Vector3(0f, 0f, 1f);
                        calibrationController.scale = 1f;
                        calibrationController.UpdatePreviewMarkers();
                        break;
                }
                break;

            case "Eye Mode":
                switch (dropdown.value)
                {
                    // Left Eye Open
                    case 0:
                        calibrationController.rightMask.SetActive(true);
                        calibrationController.leftMask.SetActive(false);
                        break;

                    // Right Eye Open
                    case 1:
                        calibrationController.rightMask.SetActive(false);
                        calibrationController.leftMask.SetActive(true);
                        break;

                    // Both Eyes Open
                    case 2:
                        calibrationController.rightMask.SetActive(false);
                        calibrationController.leftMask.SetActive(false);
                        break;
                }
                break;
        }

        PlayerPrefs.SetInt((char.ToLowerInvariant(objectName[0]) + objectName.Substring(1)).Replace(" ", "") + "Value", dropdown.value);
    }
}
