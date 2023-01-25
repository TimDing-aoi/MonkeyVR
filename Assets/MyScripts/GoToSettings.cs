using UnityEngine;
using TMPro;
using SFB;
using System;
using System.Xml;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text;

public class GoToSettings : MonoBehaviour
{
    public Canvas MainMenuCanvas;
    public Canvas SettingsCanvas;
    public GameObject obj;
    public GameObject MenuButtons;
    public GameObject GeneralMenu;
    public GameObject OthersMenu;
    public GameObject NoisesMenu;
    public GameObject errormsg;
    private TMP_InputField GUI_input;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Screen.SetResolution(1920, 1080, true);
        Application.targetFrameRate = 90;
        if (obj.name == "Counter")
        {
            GetComponent<TMP_Text>().text = string.Format("Previous Run:\nGood: {0}, Total: {1}", PlayerPrefs.GetInt("Good Trials"), PlayerPrefs.GetInt("Total Trials"));
        }
        else
        {
            GUI_input = obj.GetComponent<TMP_InputField>();
        }
    }

    public void ToSettings()
    {
        MainMenuCanvas.enabled = false;
        SettingsCanvas.enabled = true;
        PlayerPrefs.SetInt("Scene", 0);
        if (obj.GetComponentInChildren<TMP_Text>().text == "monkey2d") PlayerPrefs.SetInt("Scene", 9);

        GeneralMenu.SetActive(true);
        foreach (Transform child in GeneralMenu.transform)
        {
            foreach (Transform children in child)
            {
                if (children.gameObject.CompareTag("Setting"))
                {
                    if (children.name == "multiple_FF_mode")
                    {
                        TMP_Dropdown drop = children.GetComponent<TMP_Dropdown>();
                        int LastValue = PlayerPrefs.GetInt(children.name);
                        drop.value = LastValue;
                    }
                    else if (children.name == "GaussianPTB" || children.name == "isFlashing")
                    {
                        UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                        bool LastValue = PlayerPrefs.GetInt(children.name) == 1;
                        toggle.isOn = LastValue;
                    }
                    else if (children.name == "Path" || children.name == "Name")
                    {
                        TMP_InputField field = children.GetComponent<TMP_InputField>();
                        string LastValue = PlayerPrefs.GetString(children.name);
                        field.text = LastValue;
                    }
                    else
                    {
                        TMP_InputField field = children.GetComponent<TMP_InputField>();
                        float LastValue = PlayerPrefs.GetFloat(children.name);
                        if(field != null)
                        {
                            field.text = LastValue.ToString();
                        }
                    }
                }
            }
        }

        OthersMenu.SetActive(true);
        foreach (Transform child in OthersMenu.transform)
        {
            foreach (Transform children in child)
            {
                if (children.gameObject.CompareTag("Setting"))
                {
                    if (children.name == "is2FFCOM" || children.name == "isColored" || children.name == "isSM" || children.name == "isFFstimu" || children.name == "isMoving" || children.name == "isLeftRightnotForBack")
                    {
                        UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                        bool LastValue = PlayerPrefs.GetInt(children.name) == 1;
                        toggle.isOn = LastValue;
                    }
                    else
                    {
                        TMP_InputField field = children.GetComponent<TMP_InputField>();
                        float LastValue = PlayerPrefs.GetFloat(children.name);
                        field.text = LastValue.ToString();
                    }
                }
            }
        }

        NoisesMenu.SetActive(true);
        foreach (Transform child in NoisesMenu.transform)
        {
            foreach (Transform children in child)
            {
                if (children.gameObject.CompareTag("Setting"))
                {
                    if (children.name == "Acceleration_Control_Type")
                    {
                        TMP_Dropdown drop = children.GetComponent<TMP_Dropdown>();
                        int LastValue = PlayerPrefs.GetInt(children.name);
                        drop.value = LastValue;
                    }
                    else if (children.name == "isProcessNoise" || children.name == "isObsNoise" || children.name == "isAuto" || children.name == "TauColoredFloor")
                    {
                        UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                        bool LastValue = PlayerPrefs.GetInt(children.name) == 1;
                        toggle.isOn = LastValue;
                    }
                    else
                    {
                        TMP_InputField field = children.GetComponent<TMP_InputField>();
                        float LastValue = PlayerPrefs.GetFloat(children.name);
                        field.text = LastValue.ToString();
                    }
                }
            }
        }

        GeneralMenu.SetActive(false);
        OthersMenu.SetActive(false);
        NoisesMenu.SetActive(false);
    }

    public void BeginCalibration()
    {
        PlayerPrefs.SetFloat("calib", 1);
        SceneManager.LoadScene(1);
    }

    public void BeginGame()
    {
        PlayerPrefs.SetFloat("calib", 0);
        SceneManager.LoadScene(2);
    }

    public void ToMainMenu()
    {
        if (PlayerPrefs.GetInt("isFFstimu") == 1)
        {
            saveStimConfig();
        }

        MainMenuCanvas.enabled = true;
        if (PlayerPrefs.GetInt("is2FFCOM") == 1)
        {
            string Config2FFpath = PlayerPrefs.GetString("Path") + "\\Config_2FF.csv";
            if (!File.Exists(Config2FFpath))
            {
                MainMenuCanvas.enabled = false;
                errormsg.SetActive(true);
            }
        }
        SettingsCanvas.enabled = false;
    }

    /// <summary>
    /// Instead of hard-coding every single setting, just use the name of the
    /// object that this function is currently acting upon as the key for its
    /// value. There is no avoiding hard-coding setting the respective varibles
    /// to the correct value, however; you need to remember what the names of
    /// the objects and what variable they are associated with.
    /// 
    /// For example:
    /// If I have an object whose name is "Distance" and, in the game, I set it
    /// to "90", as in the TMP_InputField.text = "90", that value gets stored in
    /// PlayerPrefs associated to the key "Distance", but there is no way to 
    /// store the keys in a separate class and use them later. Anyway, trying to
    /// get keys from somewhere is harder, so just hard-code it when retrieving
    /// the values.
    /// </summary>
    public void saveSetting()
    {
        try
        {
            if (obj.name == "GaussianPTB" || obj.name == "isFlashing"
                || obj.name == "is2FFCOM" || obj.name == "isColored" || obj.name == "isSM" || obj.name == "isFFstimu" || obj.name == "isMoving" || obj.name == "isLeftRightnotForBack"
                || obj.name == "isProcessNoise" || obj.name == "isObsNoise" || obj.name == "isAuto" || obj.name == "TauColoredFloor")
            {
                PlayerPrefs.SetInt(obj.name, obj.GetComponent<UnityEngine.UI.Toggle>().isOn ? 1 : 0);
            }
            else if (obj.name == "multiple_FF_mode" ||
                    obj.name == "Acceleration_Control_Type")
            {
                if (obj.name == "multiple_FF_mode" && obj.GetComponent<TMP_Dropdown>().value == 0)
                {
                    PlayerPrefs.SetInt("Number of Fireflies", 1);
                    PlayerPrefs.SetInt(obj.name, obj.GetComponent<TMP_Dropdown>().value);
                }
                else
                {
                    PlayerPrefs.SetInt(obj.name, obj.GetComponent<TMP_Dropdown>().value);
                }
            }
            //TODO: port variability
            else if (obj.name == "Port")
            {
                PlayerPrefs.SetString(obj.name, obj.GetComponent<TMP_Dropdown>().options[obj.GetComponent<TMP_Dropdown>().value].text);
            }
            else if (obj.name == "Name")
            {
                if (GUI_input.text == null)
                {
                    string errorText = obj.name + ": Invalid or missing TMP_InputField text.";
                    throw new Exception(errorText);
                }

                PlayerPrefs.SetString(obj.name, GUI_input.text);
            }
            else if (obj.name == "Path")
            {
                if (GUI_input.text == null)
                {
                    string errorText = obj.name + ": Invalid or missing TMP_InputField text.";
                    throw new Exception(errorText);
                }

                string temp = GUI_input.text + "\\test.txt";
                try
                {
                    File.WriteAllText(temp, "test");
                    PlayerPrefs.SetString(obj.name, GUI_input.text);
                }
                catch (Exception e)
                {
                    Debug.LogException(e, this);
                }
            }
            else if (obj.name == "Minimum Intertrial Wait")
            {
                if (GUI_input.text == null)
                {
                    string errorText = obj.name + ": Invalid or missing TMP_InputField text.";
                    throw new Exception(errorText);
                }

                var num = float.Parse(GUI_input.text);
                if (num < 0.2f)
                {
                    num = 0.2f;
                }

                GUI_input.text = num.ToString();

                PlayerPrefs.SetFloat(obj.name, num);
            }
            else
            {
                PlayerPrefs.SetFloat(obj.name, float.Parse(GUI_input.text));
                if (GUI_input.text == null)
                {
                    string errorText = obj.name + ": Invalid or missing TMP_InputField text.";
                    throw new Exception(errorText);
                }
            }
            if (obj.name == null)
            {
                throw new Exception("Invalid or missing object name.");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    public void SwitchGeneral()
    {
        MenuButtons.SetActive(false);
        GeneralMenu.SetActive(true);
        NoisesMenu.SetActive(false);
        OthersMenu.SetActive(false);
    }

    public void SwitchOthers()
    {
        MenuButtons.SetActive(false);
        GeneralMenu.SetActive(false);
        NoisesMenu.SetActive(false);
        OthersMenu.SetActive(true);
    }

    public void SwitchNoises()
    {
        MenuButtons.SetActive(false);
        GeneralMenu.SetActive(false);
        NoisesMenu.SetActive(true);
        OthersMenu.SetActive(false);
    }

    public void SwitchButtons()
    {
        MenuButtons.SetActive(true);
        GeneralMenu.SetActive(false);
        NoisesMenu.SetActive(false);
        OthersMenu.SetActive(false);
    }

    public void LoadXML()
    {
        try
        {
            var extensions = new[] {
                new ExtensionFilter("Extensible Markup Language ", "xml")
            };
            var path = StandaloneFileBrowser.OpenFilePanel("Open File Destination", "", extensions, false);

            XmlDocument doc = new XmlDocument();
            doc.Load(path[0]);

            GeneralMenu.SetActive(true);
            foreach (Transform child in GeneralMenu.transform)
            {
                foreach (Transform children in child)
                {
                    if (children.gameObject.CompareTag("Setting"))
                    {
                        if (children.name == "multiple_FF_mode")
                        {
                            TMP_Dropdown drop = children.GetComponent<TMP_Dropdown>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        drop.value = int.Parse(setting.InnerText);
                                        PlayerPrefs.SetInt(children.name, drop.value);
                                    }
                                }
                            }
                        }
                        else if (children.name == "GaussianPTB" || children.name == "isFlashing")
                        {
                            UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        toggle.isOn = int.Parse(setting.InnerText) == 1;
                                        PlayerPrefs.SetInt(children.name, toggle.isOn ? 1 : 0);
                                    }
                                }
                            }
                        }
                        else if (children.name == "Path" || children.name == "Name")
                        {
                            TMP_InputField field = children.GetComponent<TMP_InputField>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        field.text = setting.InnerText;
                                        PlayerPrefs.SetString(children.name, field.text);
                                    }
                                }
                            }
                        }
                        else
                        {
                            TMP_InputField field = children.GetComponent<TMP_InputField>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        field.text = setting.InnerText;
                                        PlayerPrefs.SetFloat(children.name, float.Parse(field.text));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            int currunnum = PlayerPrefs.GetInt("Run Number");
            PlayerPrefs.SetInt("Run Number", currunnum++);

            OthersMenu.SetActive(true);
            foreach (Transform child in OthersMenu.transform)
            {
                foreach (Transform children in child)
                {
                    if (children.gameObject.CompareTag("Setting"))
                    {
                        if (children.name == "is2FFCOM" || children.name == "isColored" || children.name == "isSM" || children.name == "isFFstimu" || children.name == "isMoving" || children.name == "isLeftRightnotForBack")
                        {
                            UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        toggle.isOn = int.Parse(setting.InnerText) == 1;
                                        PlayerPrefs.SetInt(children.name, toggle.isOn ? 1 : 0);
                                    }
                                }
                            }
                        }
                        else
                        {
                            TMP_InputField field = children.GetComponent<TMP_InputField>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        field.text = setting.InnerText;
                                        PlayerPrefs.SetFloat(children.name, float.Parse(field.text));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            NoisesMenu.SetActive(true);
            foreach (Transform child in NoisesMenu.transform)
            {
                foreach (Transform children in child)
                {
                    if (children.gameObject.CompareTag("Setting"))
                    {
                        if (children.name == "Acceleration_Control_Type")
                        {
                            TMP_Dropdown drop = children.GetComponent<TMP_Dropdown>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        drop.value = int.Parse(setting.InnerText);
                                        PlayerPrefs.SetInt(children.name, drop.value);
                                    }
                                }
                            }
                        }
                        else if (children.name == "isProcessNoise" || children.name == "isObsNoise" || children.name == "isAuto" || children.name == "TauColoredFloor")
                        {
                            UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        toggle.isOn = int.Parse(setting.InnerText) == 1;
                                        PlayerPrefs.SetInt(children.name, toggle.isOn ? 1 : 0);
                                    }
                                }
                            }
                        }
                        else
                        {
                            TMP_InputField field = children.GetComponent<TMP_InputField>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        field.text = setting.InnerText;
                                        PlayerPrefs.SetFloat(children.name, float.Parse(field.text));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            GeneralMenu.SetActive(false);
            OthersMenu.SetActive(false);
            NoisesMenu.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    public void saveStimConfig()
    {
        StringBuilder stimConf = new StringBuilder();
        string path = PlayerPrefs.GetString("Path");
        string stimPath = path + "/stimulation_parameters_" + PlayerPrefs.GetString("Name") + "_" + DateTime.Today.ToString("MMddyyyy") + "_" + PlayerPrefs.GetInt("Run Number").ToString("D3") + ".txt";
        stimConf.AppendLine("MonkeyName,Date,RunNumber,StimAmplitude,StimDur");
        stimConf.AppendLine(PlayerPrefs.GetString("Name").ToString() + "," + DateTime.Today.ToString("MMddyyyy").ToString() + "," + PlayerPrefs.GetInt("Run Number").ToString("D3")
            + "," + PlayerPrefs.GetFloat("StimuAmp").ToString() + "," + PlayerPrefs.GetFloat("StimuStimuDur").ToString());
        File.WriteAllText(stimPath, stimConf.ToString());
    }

    public void LoadDefault()
    {
        PlayerPrefs.DeleteAll();
        try
        {
            var extensions = new[] {
                new ExtensionFilter("Extensible Markup Language ", "xml")
            };
            string path = Environment.CurrentDirectory + "\\" +"config_Default.xml";

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            GeneralMenu.SetActive(true);
            foreach (Transform child in GeneralMenu.transform)
            {
                foreach (Transform children in child)
                {
                    if (children.gameObject.CompareTag("Setting"))
                    {
                        if (children.name == "multiple_FF_mode")
                        {
                            TMP_Dropdown drop = children.GetComponent<TMP_Dropdown>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        drop.value = int.Parse(setting.InnerText);
                                        PlayerPrefs.SetInt(children.name, drop.value);
                                    }
                                }
                            }
                        }
                        else if (children.name == "GaussianPTB" || children.name == "isFlashing")
                        {
                            UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        toggle.isOn = int.Parse(setting.InnerText) == 1;
                                        PlayerPrefs.SetInt(children.name, toggle.isOn ? 1 : 0);
                                    }
                                }
                            }
                        }
                        else if (children.name == "Path" || children.name == "Name")
                        {
                            TMP_InputField field = children.GetComponent<TMP_InputField>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        field.text = setting.InnerText;
                                        PlayerPrefs.SetString(children.name, field.text);
                                    }
                                }
                            }
                        }
                        else
                        {
                            TMP_InputField field = children.GetComponent<TMP_InputField>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        field.text = setting.InnerText;
                                        PlayerPrefs.SetFloat(children.name, float.Parse(field.text));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            int currunnum = PlayerPrefs.GetInt("Run Number");
            PlayerPrefs.SetInt("Run Number", currunnum++);

            OthersMenu.SetActive(true);
            foreach (Transform child in OthersMenu.transform)
            {
                foreach (Transform children in child)
                {
                    if (children.gameObject.CompareTag("Setting"))
                    {
                        if (children.name == "is2FFCOM" || children.name == "isColored" || children.name == "isSM" || children.name == "isFFstimu" || children.name == "isMoving" || children.name == "isLeftRightnotForBack")
                        {
                            UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    print(setting.Name);
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        toggle.isOn = int.Parse(setting.InnerText) == 1;
                                        PlayerPrefs.SetInt(children.name, toggle.isOn ? 1 : 0);
                                    }
                                }
                            }
                        }
                        else
                        {
                            TMP_InputField field = children.GetComponent<TMP_InputField>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        field.text = setting.InnerText;
                                        PlayerPrefs.SetFloat(children.name, float.Parse(field.text));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            NoisesMenu.SetActive(true);
            foreach (Transform child in NoisesMenu.transform)
            {
                foreach (Transform children in child)
                {
                    if (children.gameObject.CompareTag("Setting"))
                    {
                        if (children.name == "Acceleration_Control_Type")
                        {
                            TMP_Dropdown drop = children.GetComponent<TMP_Dropdown>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        print(setting.Name);
                                        drop.value = int.Parse(setting.InnerText);
                                        PlayerPrefs.SetInt(children.name, drop.value);
                                    }
                                }
                            }
                        }
                        else if (children.name == "isProcessNoise" || children.name == "isObsNoise" || children.name == "isAuto" || children.name == "TauColoredFloor")
                        {
                            UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        toggle.isOn = int.Parse(setting.InnerText) == 1;
                                        PlayerPrefs.SetInt(children.name, toggle.isOn ? 1 : 0);
                                    }
                                }
                            }
                        }
                        else
                        {
                            TMP_InputField field = children.GetComponent<TMP_InputField>();
                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                foreach (XmlNode setting in node.ChildNodes)
                                {
                                    if (setting.Name == children.name.Replace(" ", ""))
                                    {
                                        field.text = setting.InnerText;
                                        PlayerPrefs.SetFloat(children.name, float.Parse(field.text));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            GeneralMenu.SetActive(false);
            OthersMenu.SetActive(false);
            NoisesMenu.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }
}
