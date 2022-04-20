using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static Monkey2D;
using PupilLabs;
using System.Text;

public class UDPTransmitter : MonoBehaviour
{
    public string IP = "0.0.0.0";
    public int TransmitPort = 31000;
    private IPEndPoint _RemoteEndPoint;
    private UdpClient _TransmitClient;

    private void Start()
    {
        Initialize();
    }

    void Update()
    {
        int framecounter = Time.frameCount;
        //Sends whenever 270 frames
        if (framecounter % 270 == 0)
        {
            Send();
        }
    }

    /// Initialize objects.
    private void Initialize()
    {
        _RemoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), TransmitPort);
        _TransmitClient = new UdpClient();
    }

    /// sends the sbpacket string
    public void Send()
    {
        try
        {
            // Convert string message to byte array.  
            string continuousPacket = PupilLabs.DataController.dataController.sbPacket;
            byte[] serverMessageAsByteArray = Encoding.UTF8.GetBytes(continuousPacket);
            _TransmitClient.Send(serverMessageAsByteArray, serverMessageAsByteArray.Length, _RemoteEndPoint);
        }
        catch (Exception err)
        {
            Debug.Log("<color=red>" + err.Message + "</color>");
        }
    }

    /// Deinitialize everything on quiting the application
    private void OnApplicationQuit()
    {
        try
        {
            _TransmitClient.Close();
        }
        catch (Exception err)
        {
            Debug.Log("<color=red>" + err.Message + "</color>");
        }
    }
}