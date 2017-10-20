using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using UnityEngine.UI;
using System.Text;
using System.Threading;

public class ServerScript : MonoBehaviour
{
    private string serverName = "Unnamed Server";

    //Panels and Tiles
    public GameObject PlayerPanel;
    public GameObject PlayerTilePrefab;

    //Fields and Texts
    public Text serverNameText;
    public Text CurrentIPText;

    //Networking
    private int port = 6666;
    private List<QuizClient> players = new List<QuizClient>();
    private List<QuizClient> scoreboards = new List<QuizClient>();
    private List<QuizClient> clients = new List<QuizClient>();
    private List<QuizClient> disconnectedClients = new List<QuizClient>();

    private TcpListener gameServer;

    private UdpClient serverBrowserServer;
    private IPEndPoint broadcastEndpoint;

    //Gamestate Variables
    private bool serverStarted;

    //Instancing queue, for threading issues

    private List<QuizClient> clientsToReactivate = new List<QuizClient>();
    

    private void Start()
    {
        //Set the name of the server
        serverName = GameState.serverName;

        //Create an endpoint for broadcasting the servers existance
        broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, port);

        //Set the name and IP of the server on top of the screen
        CurrentIPText.text = Network.player.ipAddress;
        serverNameText.text = GameState.serverName;

        //Start server existance broadcast
        try
        {
            serverBrowserServer = new UdpClient(port);
            Debug.Log("Broadcasting the servers existance on port: " + port);
        }
        catch (Exception e)
        {
            Debug.Log("UDP Socket Error: " + e.Message);
        }
        
        //Start Game Hosting
        try
        {
            gameServer = new TcpListener(IPAddress.Any, port);
            gameServer.Start();
            StartListening();
            serverStarted = true;

            Debug.Log("Gameplay server has been started on port " + port.ToString());
        }
        catch (Exception e)
        {
            Debug.Log("TCP Socket Error: " + e.Message);
        }
    }

    private void Update()
    {
        //Don't Update if the server has not started
        if (!serverStarted)
            return;

        ListenForClient();
        ReactivateQueue();

        //Broadcast Existence
        Byte[] sendBytes = Encoding.ASCII.GetBytes(serverName);
        serverBrowserServer.Send(sendBytes, sendBytes.Length, broadcastEndpoint);
    }

    private void StartListening()
    {
        gameServer.BeginAcceptTcpClient(AcceptTcpClient, gameServer);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        Debug.Log("Boop");
        TcpListener listener = (TcpListener)ar.AsyncState;
        
        //Reconnect Old Client
        bool wasConnected = false;
        foreach(QuizClient q in disconnectedClients)
        {
            if (listener.LocalEndpoint.ToString() == q.ip)
            {
                Debug.Log("Reconnection Attempted");
                q.tcp = listener.EndAcceptTcpClient(ar);
                clients.Add(q);
                disconnectedClients.Remove(q);
                lock (clientsToReactivate)
                {
                    clientsToReactivate.Add(q);
                }
                wasConnected = true;
                break;
            }
        }
        
        //Connect New Client
        if(!wasConnected)
        {
            Debug.Log("Attempted To Connect");
            QuizClient c = new QuizClient(listener.EndAcceptTcpClient(ar));
            c.setIP(listener.LocalEndpoint);
            clients.Add(c);
            Debug.Log("Connection made, current clients: " + clients.Count);
        }
        
        StartListening();
    }

    private void InstantiateNewPlayer(QuizClient c)
    {
        GameObject instance = Instantiate(PlayerTilePrefab) as GameObject;
        instance.transform.SetParent(PlayerPanel.transform, false);
        Text[] texts = instance.GetComponentsInChildren<Text>();
        foreach (Text t in texts)
        {
            if (t.name == "PlayerNameText")
            {
                t.text = c.clientName;
            }
            else if (t.name == "PlayersCurrentAnswerText")
            {
                t.text = "";
            }
        }
        c.tile = instance;

        string message = "p¤" + c.clientName + "¤";
        Broadcast(message, scoreboards);
    }

    private void ReactivateQueue()
    {
        try
        {
            foreach (QuizClient c in clientsToReactivate)
            {
                c.tile.SetActive(true);
                {
                    clientsToReactivate.Remove(c);
                }
            }
        }
        catch(InvalidOperationException e)
        {
            //Still fiiine
        }
        
    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

                return true;
            }
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    private void ListenForClient()
    {
        Debug.Log("Amount of clients: " + clients.Count);
        try
        {
            foreach (QuizClient c in clients)
            {
                // Is the client still connected?
                Debug.Log("Actually Listening");
                if (!IsConnected(c.tcp))
                {
                    Debug.Log("Disconnected Client: " + c.clientName);
                    c.tcp.Close();
                    c.tile.SetActive(false);
                    clients.Remove(c);
                    disconnectedClients.Add(c);
                    continue;
                }
                // Check for messages from client
                else
                {
                    NetworkStream s = c.tcp.GetStream();
                    if (s.DataAvailable)
                    {
                        Debug.Log("GotData!");
                        StreamReader reader = new StreamReader(s, true);
                        string data = reader.ReadLine();

                        if (data != null)
                        {
                            OnIncomingData(c, data);
                        }
                    }
                    else
                    {
                        Debug.Log("No Data Found...");
                    }
                }
            }
        }
        catch(InvalidOperationException e)
        {
            //It's fiiiine
        }     
    }

    private void OnIncomingData(QuizClient c, string data)
    {
        Debug.Log("Incoming data: " + data);
        string[] substring = data.Split('¤');
        if (substring[0] == "h")
        {
            if (substring[1] == "c") // player
            {
                if(!c.initialized)
                {
                    players.Add(c);
                    if (substring[2] != null)
                        c.SetName(substring[2]);
                    else
                        c.SetName("Retard");

                    // instantiate new player
                    InstantiateNewPlayer(c);
                    c.initialized = true;
                }  
            }
            else // scoreboard
            {
                scoreboards.Add(c);
                foreach(QuizClient q in players)
                {
                    string message = "p¤" + q.clientName + "¤";
                    Broadcast(message, scoreboards);
                }
            }
        }
        else if (substring[0] == "a")
        {
            c.SetAnswer(substring[1]);
            UpdateAnswer(c);

            string message = "a¤" + c.clientName + "¤" + c.currentAnswer;
            Broadcast(message, scoreboards);
        }
        else
        {
            print("message failed");
        }
    } 
    
    private void Send(string data, QuizClient q)
    {
        try
        {
            StreamWriter writer = new StreamWriter(q.tcp.GetStream());
            writer.WriteLine(data);
            writer.Flush();
        }
        catch (Exception e)
        {
            Debug.Log("Write error : " + e + " to client " + q.clientName);
        }
    }   

    private void Broadcast(string data, List<QuizClient> qc)
    {
        foreach (QuizClient c in qc)
        {
            try
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error : " + e + " to client " + c.clientName);
            }
        }
    }

    // Gameplay Commands
    public void OnReveal()
    {
        foreach(QuizClient c in players)
        {
            string answer = "r¤" + c.clientName + "¤" + c.currentAnswer + "¤";
            Broadcast(answer, scoreboards);
        }
    }

    public void OnStart()
    {
        foreach(QuizClient q in players)
        {
            q.currentAnswer = "";
            UpdateAnswer(q);
        }
        string data = "s¤";
        Broadcast(data, clients);
    }

    private void UpdateAnswer(QuizClient c)
    {
        Text[] texts = c.tile.GetComponentsInChildren<Text>();
        foreach(Text t in texts)
        {
            if (t.name == "PlayersCurrentAnswerText")
                t.text = c.currentAnswer;
        }
    }
}

public class QuizClient
{
    public TcpClient tcp;
    public string ip;
    public string clientName;
    public string currentAnswer;
    public int currentScore;

    public GameObject tile;

    public bool initialized = false;

    public QuizClient(TcpClient clientSocket)
    {
        Debug.Log("Creating new QuizClient!");
        clientName = "";
        tcp = clientSocket;
    }
    public void setIP(EndPoint e)
    {
        ip = e.ToString();
    }

    public void SetName(string name)
    {
        clientName = name;
    }
    public void SetAnswer(string answer)
    {
        currentAnswer = answer;
    }
    public void IncreaseScore()
    {
        currentScore++;
    }
    public void DecreaseScore()
    {
        currentScore--;
    }
}