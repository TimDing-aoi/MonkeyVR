﻿using UnityEngine;
using System.IO.Ports;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class Serial : MonoBehaviour
{
    public static Serial serial { get; private set; }
    public SerialPort sp;
    bool juice = true;
    bool isMonkey = true;

    float juiceTime;

    // Start is called before the first frame update
    void OnEnable()
    {
        serial = this;
        juiceTime = PlayerPrefs.GetFloat("maxJuiceTime");
        sp = new SerialPort("COM5", 1000000);
        isMonkey = PlayerPrefs.GetInt("isHuman") == 0;
        if (isMonkey)
        {
            sp.Open();
            sp.ReadTimeout = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard.spaceKey.isPressed && juice && isMonkey) GiveJuice();
    }

    async void GiveJuice()
    {
        //print(string.Format("juice time = {0}", juiceTime));
        juice = false;
        sp.Write(string.Format("j{0}", juiceTime));
        await new WaitForSeconds(juiceTime / 1000.0f);
        juice = true;
    }

    private void OnDisable()
    {
        if (sp.IsOpen) sp.Close();
    }
}
