using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using UnityEngine.UI;
using System.IO;
using System;

public class Client : MonoBehaviour
{
    public GameObject chatContainer;
    public GameObject messagePrefab;

    private bool socketReady = false;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public void ConnectToServer()
    {
        // if already conneted, ignore
        if (socketReady)
            return;

        // default host / port
        string host = "127.0.0.1";
        int port = 6321;

        // Overwrite default host / port values, if there is something in those boxes
        string h;
        int p;
        h = GameObject.Find("HostInput").GetComponent<InputField>().text;
        if (h != "")
            host = h;
        int.TryParse(GameObject.Find("PortInput").GetComponent<InputField>().text, out p);
        if (p != 0)
            port = p;

        // Create the socket
        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true;
        }
        catch(Exception e)
        {
            Debug.Log("Socket error : " + e.Message);
        }
    }

    private void Update()
    {
        if(socketReady)
        {
            if(stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                    OnIncomingData(data); 
            }
        }
    }

    private void OnIncomingData(string data)
    {
        GameObject instance = Instantiate(messagePrefab, chatContainer.transform) as GameObject;
        instance.GetComponentInChildren<Text>().text = data;
    }

    private void Send(string data)
    {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    public void OnSendButton()
    {
        string message = GameObject.Find("SendInput").GetComponent<InputField>().text;
        Send(message);
    }

}
