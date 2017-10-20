using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Net;
using UnityEngine.UI;
using System.IO;
using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;

public class ClientScript : MonoBehaviour
{
    //Game Screens
    public GameObject startScreen;
    public GameObject gameScreen;

    //Fields and Texts
    public GameObject answerGroup;
    public GameObject answerField;
    public InputField nameInputField;
    public InputField IPInputField;
    public GameObject serverListWindow;
    public GameObject serverTile;

    //Networking
    private bool            socketReady = false;
    private TcpClient       socket;
    private NetworkStream   stream;
    private StreamWriter    writer;
    private StreamReader    reader;

    System.Threading.Thread serverBrowserThread;
    private UdpClient serverBrowserSocket;
    private List<potentialServer> foundServers = new List<potentialServer>();
    private int oldServerAmount = 0;

    //private string  host = "127.0.0.1";
    string host = "0";
    private int port = 6666;

    private string  clientName;

    //Game State Variables
    bool isInGame = false;
    bool searchingForServers;

    private void Start()
    {
        serverBrowserSocket = new UdpClient(port);
        serverBrowserThread = new System.Threading.Thread(listenForSevers);
    }

    private void Update()
    {
        //Before game is entered
        if(!isInGame && !searchingForServers)
        {
            serverBrowserThread.Start();
            searchingForServers = true;
        }

        //If a server is chosen from the list
        string chosenIP = "0";
        foreach(Transform child in serverListWindow.transform)
        {
            if(child.GetComponent<ServerPanelScript>().isPressed)
            {
                chosenIP = child.GetComponent<ServerPanelScript>().getIP();
            }
        }
        if(chosenIP != "0")
        {
            ListConnectToServer(chosenIP);
        }

        //Refresh Server List
        if(foundServers.Count != oldServerAmount)
        {
            oldServerAmount = foundServers.Count;
            refreshServerList();
        }

        //After game is entered
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                    OnIncomingData(data);
            }
        }
    }

    void listenForSevers()
    {
        IPEndPoint hostingServerEndpoint = new IPEndPoint(IPAddress.Any, 0);

        try
        {
            Byte[] recieveBytes = serverBrowserSocket.Receive(ref hostingServerEndpoint);
            
            string newServerName = Encoding.ASCII.GetString(recieveBytes);

            lock (foundServers)
            {
                foundServers.Add(new potentialServer(newServerName, hostingServerEndpoint.Address.ToString()));
            }
        } 
        catch(Exception e)
        {
            Debug.Log("Server Listing Error in Client: " + e);
        }   
    }

    void refreshServerList()
    {
        //Refresh Server List
        foreach (Transform child in serverListWindow.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (potentialServer s in foundServers)
        {
            GameObject g = Instantiate(serverTile) as GameObject;
            g.transform.SetParent(serverListWindow.transform, false);
            ServerPanelScript p = g.GetComponent<ServerPanelScript>();
            p.initializePanel(s.serverName, s.serverIP);
            g.name = ("Panel - " + s.serverName);
        }
    }

    public void ManualConnectToServer()
    {
        // if already connected, ignore
        if (socketReady)
            return;

        clientName = nameInputField.text;
        host = IPInputField.text;

        createSocket();  
    }

    public void ListConnectToServer(string IP)
    {
        if (socketReady)
            return;

        clientName = nameInputField.text;
        host = IP;

        serverBrowserSocket.Close();
        serverBrowserThread.Abort();

        createSocket();
    }

    void createSocket()
    {
        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true;

            // Change what screen is shown
            startScreen.SetActive(false);
            gameScreen.SetActive(true);
            isInGame = true;

            // Fix name for client
            if (clientName == "")
                clientName = "Retard";
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

    //Sending and recieving messages
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
        Debug.Log("Sent Data: " + data);
    }

    // Gameplay Commands
    public void OnSubmit()
    {
        string answer = GameObject.Find("AnswerField").GetComponent<InputField>().text;
        string message = "a¤" + answer;
        answerGroup.SetActive(false);
        Send(message);
    }

    public void OnBackButton()
    {
        serverBrowserSocket.Close();
        SceneManager.LoadScene(0);
    }

    public void OnInGameBackButton()
    {
        socket.Close();
        serverBrowserSocket.Close();
        SceneManager.LoadScene(0);
    }
}

public class potentialServer
{
    public string serverName;
    public string serverIP;

    public potentialServer(string name, string IP)
    {
        serverName = name;
        serverIP = IP;
    }
}
