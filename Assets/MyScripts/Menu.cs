using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Menu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    /// <summary>
    /// For now, quit when you press ESC but I might add a pause menu
    /// </summary>
    void Update()
    {
        if (Input.GetKey(KeyCode.Return))
        {
            // Environment.Exit(Environment.ExitCode);
            Application.Quit();
        }
    }
}
