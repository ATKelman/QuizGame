using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using UnityEngine.UI;
using System.IO;
using System;

public class ClientScript : MonoBehaviour
{
    public GameObject startScreen;
    public GameObject GamePanel;
    public GameObject answerGroup;
    public GameObject answerField;

    private bool            socketReady = false;
    private TcpClient       socket;
    private NetworkStream   stream;
    private StreamWriter    writer;
    private StreamReader    reader;

    private string  host = "127.0.0.1";
    private int     port = 6666;

    private string  clientName;

    public void ConnectToServer()
    {
        // if already connected, ignore
        if (socketReady)
            return;

        clientName = GameObject.Find("NameInput").GetComponent<InputField>().text;

        // Create the socket
        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true;

            // Change what screen is shown
            startScreen.SetActive(false);
            GamePanel.SetActive(true);

            // Fix name for client
            if (clientName == "")
                clientName = "retard";
            GameObject.Find("PlayerName").GetComponent<Text>().text = clientName;

            // Send handshake message
            string message = "h¤c¤" + clientName;
            Send(message);

            Debug.Log("Successfully Connected");
        }
        catch (Exception e)
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
        string[] messages = data.Split('¤');
        if(messages[0] == "s")  // Start new round
        {
            answerField.GetComponent<InputField>().text = "";
            answerGroup.SetActive(true);
        }
        Debug.Log("message recieved " + data);
    }

    private void Send(string data)
    {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    // Gameplay Commands
    public void OnSubmit()
    {
        string answer = GameObject.Find("AnswerField").GetComponent<InputField>().text;
        string message = "a¤" + answer;
        answerGroup.SetActive(false);
        Send(message);
    }
}
