using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using UnityEngine.UI;

public class ServerScript : MonoBehaviour
{
    public GameObject PlayerPanel;
    public GameObject PlayerTilePrefab;

    private int port = 6666;

    private List<QuizClient> players;
    private List<QuizClient> scoreboards;
    private List<QuizClient> clients;
    private List<QuizClient> disconnectedClients;


    private TcpListener server;
    private bool serverStarted;

    private void Start()
    {
        players             = new List<QuizClient>();
        scoreboards         = new List<QuizClient>();
        clients             = new List<QuizClient>();
        disconnectedClients = new List<QuizClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            serverStarted = true;

            Debug.Log("server has been started on port " + port.ToString());
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }
    private void Update()
    {
        if (!serverStarted)
            return;

        ListenForClient();

    }

    private void ListenForClient()
    {
        foreach (QuizClient c in clients)
        {
            // Is the client still connected?
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectedClients.Add(c);
                continue;
            }
            // Check for messages from client
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(c, data);
                    }
                }
            }
        }
    }

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
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
    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;

        clients.Add(new QuizClient(listener.EndAcceptTcpClient(ar)));
        StartListening();
    }

    private void OnIncomingData(QuizClient c, string data)
    {
        string[] substring = data.Split('¤');
        if (substring[0] == "h")
        {
            if (substring[1] == "c") // player
            {
                players.Add(c);
                if (substring[2] != null)
                    c.SetName(substring[2]);
                else
                    c.SetName("retard");

                // instantiate new player
                InstantiateNewPlayer(c);
            }
            else // scoreboard
            {
                scoreboards.Add(c);
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

    private void InstantiateNewPlayer(QuizClient c)
    {
        GameObject instance = Instantiate(PlayerTilePrefab, PlayerPanel.transform) as GameObject;
        Text[] texts = instance.GetComponentsInChildren<Text>();
        foreach(Text t in texts)
        {
            if(t.name == "PlayerNameText")
            {
                t.text = c.clientName;
            }
            else if(t.name == "PlayersCurrentAnswerText")
            {
                t.text = "";
            }
        }
        c.tile = instance;
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
        string data = "s¤";
        Broadcast(data, players);
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
    public string clientName;
    public string currentAnswer;
    public int currentScore;

    public GameObject tile;

    public QuizClient(TcpClient clientSocket)
    {
        clientName = "";
        tcp = clientSocket;
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