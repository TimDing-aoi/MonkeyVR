using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using SFB;

public class FileSelect : MonoBehaviour
{
    // Start is called before the first frame update
    private TMP_InputField input;
    private GameObject obj;

    public void Select()
    {
        input = this.GetComponent<TMP_InputField>();
        obj = this.gameObject;
        var path = StandaloneFileBrowser.OpenFolderPanel("Set File Destination", Application.dataPath, true);
        input.text = path[0];
        PlayerPrefs.SetString(obj.name, path[0]);
    }
}
