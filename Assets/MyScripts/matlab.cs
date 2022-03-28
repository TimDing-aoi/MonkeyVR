using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System;
using System.IO;
using System.Text;
using static Monkey2D;
public class readSocket : MonoBehaviour
{
    // Use this for initialization
    TcpListener listener;
    String msg;

    [Obsolete]
    void Start()
    {
        listener = new TcpListener(55001);
        listener.Start();
        print("is listening");
    }
    // Update is called once per frame
    void Update()
    {
        if (!listener.Pending())
        {
        }
        else
        {
            print("socket comes");
            TcpClient client = listener.AcceptTcpClient();
            NetworkStream ns = client.GetStream();
            StreamReader reader = new StreamReader(ns);
            msg = reader.ReadToEnd();
            print(msg);
        }
    }
}
