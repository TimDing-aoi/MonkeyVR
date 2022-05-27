using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System;
using System.IO;
using System.Text;
using static Monkey2D;
using PupilLabs;
public class Socket : MonoBehaviour
{
    // Use this for initialization
    internal Boolean socketReady = false;
    TcpClient mySocket;
    NetworkStream theStream;
    StreamWriter theWriter;
    StreamReader theReader;
    String Host = "127.0.0.1";
    Int32 Port = 55000;
    void Start()
    {
        setupSocket();
        Debug.Log("socket is set up");
    }

    void OnEnable()
    {
        setupSocket();
        Debug.Log("socket is set up");
    }

    // Update is called once per frame
    void Update()
    {
        int framecounter = Time.frameCount;
        SendPacket();
    }
    public void setupSocket()
    {
        try
        {
            mySocket = new TcpClient(Host, Port);
            theStream = mySocket.GetStream();
            theWriter = new StreamWriter(theStream);
            socketReady = true;
            Byte[] sendBytes = Encoding.UTF8.GetBytes("yah!! it works");
            mySocket.GetStream().Write(sendBytes, 0, sendBytes.Length);
            Debug.Log("socket is sent");
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e);
        }
    }

    void SendPacket()
    {

        try
        {
            //mySocket = new TcpClient(Host, Port);
            //theStream = mySocket.GetStream();
            //theWriter = new StreamWriter(theStream);
            //socketReady = true;
            string continuousPacket = PupilLabs.DataController.dataController.sbPacket;
            Byte[] sendBytes = Encoding.UTF8.GetBytes(continuousPacket);
            mySocket.GetStream().Write(sendBytes, 0, sendBytes.Length);
            Debug.Log(string.Format("packet of {0} is sent", sendBytes.Length));
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e);
        }
    }
}