using UnityEngine;
using System.IO.Ports;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class Serial : MonoBehaviour
{
    public static Serial serial { get; private set; }
    public SerialPort sp;
    bool juice = true;

    float juiceTime;

    // Start is called before the first frame update
    void OnEnable()
    {
        serial = this;
        juiceTime = PlayerPrefs.GetFloat("Max Juice Time");
        sp = new SerialPort("COM4", 1000000);
        sp.Open();
        sp.ReadTimeout = 1;
    }

    // Update is called once per frame
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard.spaceKey.isPressed && juice) GiveJuice();
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
