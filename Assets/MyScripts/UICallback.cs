using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using PupilLabs;
using System.IO;

public class UICallback : MonoBehaviour
{
    private CalibrationController calibrationController;
    private DataController dataController;
    private Slider slider;
    private TMP_InputField text;
    private Dropdown dropdown;
    private Button button;
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
            //dropdown.value = PlayerPrefs.GetInt((char.ToLowerInvariant(objectName[0]) + objectName.Substring(1)).Replace(" ", "") + "Value");
            dropdown.value = 2;
        }
        else if (GetComponent<Button>() != null)
        {
            button = this.GetComponent<Button>();
            
            if (File.Exists("C:\\Users\\Lab\\Desktop\\Calibration\\LeftMatrix.txt") && File.Exists("C:\\Users\\Lab\\Desktop\\Calibration\\RightMatrix.txt") && File.Exists("C:\\Users\\Lab\\Desktop\\Calibration\\BiMatrix0.txt") && File.Exists("C:\\Users\\Lab\\Desktop\\Calibration\\BiMatrix1.txt"))
            {
                button.interactable = true;
            }
            else
            {
                button.interactable = false;
                button.GetComponentInChildren<TMP_Text>().text = "Need Left and/or Right Calibrations";
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

    private void Update()
    {
        if (GetComponent<Button>() != null)
        {
            button = this.GetComponent<Button>();

            if (File.Exists("C:\\Users\\Lab\\Desktop\\Calibration\\LeftMatrix.txt") && File.Exists("C:\\Users\\Lab\\Desktop\\Calibration\\RightMatrix.txt") && File.Exists("C:\\Users\\Lab\\Desktop\\Calibration\\BiMatrix0.txt") && File.Exists("C:\\Users\\Lab\\Desktop\\Calibration\\BiMatrix1.txt"))
            {
                button.interactable = true;
                button.GetComponentInChildren<TMP_Text>().text = "Load New 3DHMDGazer Plugin";
            }
            else
            {
                button.interactable = false;
                button.GetComponentInChildren<TMP_Text>().text = "Need Left and/or Right Calibrations";
            }
        }

        if (GetComponent<Slider>() != null)
        {
            slider = this.GetComponent<Slider>();
            PlayerPrefs.SetFloat((char.ToLowerInvariant(objectName[0]) + objectName.Substring(1)).Replace(" ", "") + "Value", slider.value);
            //Debug.Log((char.ToLowerInvariant(name[0]) + name.Substring(1)).Replace(" ", "") + "Value");
            //text = GameObject.Find(objectName + " Text").GetComponent<TMP_InputField>();
            //text.text = slider.value.ToString();
            //flagSliderOrText = true;
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
                calibrationController.UpdatePreviewMarkers();
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

    public void LoadNewGazer3DHMD()
    {
        //calibrationController.subsCtrl.requestCtrl.StopPlugin("GazerHMD3D");

        //initialize 2D array
        object[][] LeftMatrix = new object[4][]; for (int i = 0; i < 4; i++) { LeftMatrix[i] = new object[4]; }
        object[][] RightMatrix = new object[4][]; for (int i = 0; i < 4; i++) { RightMatrix[i] = new object[4]; }
        object[][] BiMatrix0 = new object[4][]; for (int i = 0; i < 4; i++) { BiMatrix0[i] = new object[4]; }
        object[][] BiMatrix1 = new object[4][]; for (int i = 0; i < 4; i++) { BiMatrix1[i] = new object[4]; }

        StreamReader LeftSR = new StreamReader("C:\\Users\\Lab\\Desktop\\Calibration\\LeftMatrix.txt");
        StreamReader RightSR = new StreamReader("C:\\Users\\Lab\\Desktop\\Calibration\\RightMatrix.txt");
        StreamReader BiZeroSR = new StreamReader("C:\\Users\\Lab\\Desktop\\Calibration\\BiMatrix0.txt");
        StreamReader BiOneSR = new StreamReader("C:\\Users\\Lab\\Desktop\\Calibration\\BiMatrix1.txt");

        for (int i = 0; i < 4; i++)
        {
            for (int k = 0; k < 4; k++)
            {
                LeftMatrix[i][k] = float.Parse(LeftSR.ReadLine());
                RightMatrix[i][k] = float.Parse(RightSR.ReadLine());
                BiMatrix0[i][k] = float.Parse(BiZeroSR.ReadLine());
                BiMatrix1[i][k] = float.Parse(BiOneSR.ReadLine());
            }
        }

        //object[] LeftMatrix = File.ReadAllLines("C:\\Users\\Lab\\Desktop\\Calibration\\LeftMatrix.txt").Where(s => s != string.Empty).Select(s => float.Parse(s)).Cast<object>().ToArray();
        //object[] RightMatrix = File.ReadAllLines("C:\\Users\\Lab\\Desktop\\Calibration\\RightMatrix.txt").Where(s => s != string.Empty).Select(s => float.Parse(s)).Cast<object>().ToArray();
        //object[] BiMatrix0 = File.ReadAllLines("C:\\Users\\Lab\\Desktop\\Calibration\\BiMatrix0.txt").Where(s => s != string.Empty).Select(s => float.Parse(s)).Cast<object>().ToArray();
        //object[] BiMatrix1 = File.ReadAllLines("C:\\Users\\Lab\\Desktop\\Calibration\\BiMatrix1.txt").Where(s => s != string.Empty).Select(s => float.Parse(s)).Cast<object>().ToArray();

        Dictionary<object, object> leftModelDic = new Dictionary<object, object> {
            { "eye_camera_to_world_matrix", LeftMatrix },
            { "gaze_distance", 500 }
        };

        Dictionary<object, object> rightModelDic = new Dictionary<object, object>
        {
            { "eye_camera_to_world_matrix", RightMatrix },
            { "gaze_distance", 500 }
        };

        Dictionary<object, object> binocularModelDic = new Dictionary<object, object>
        {
            { "eye_camera_to_world_matrix0", BiMatrix0 },
            { "eye_camera_to_world_matrix1", BiMatrix1 },
            { "gaze_distance", 500 }
        };

        Dictionary<object, object> paramsDic = new Dictionary<object, object>
        {
            { "left_model", leftModelDic },
            { "right_model", rightModelDic },
            { "binocular_model", binocularModelDic }
        };

        Dictionary<string, object> args = new Dictionary<string, object>
        {
            { "params", paramsDic }
        };

        try
        {
            calibrationController.subsCtrl.requestCtrl.StartPlugin("GazerHMD3D", args);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }
}
