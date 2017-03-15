using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.Generic;

public class ScoreboardScript : MonoBehaviour
{
    public GameObject scoreTilePrefab;
    public GameObject scoreboardContainer;

    private List<GameObject> scoreTiles;

    private bool socketReady = false;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamReader reader;

    public void Start()
    {
        // default host / port
        string host = "127.0.0.1";
        int port = 6666;

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            reader = new StreamReader(stream);
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error : " + e.Message);
        }

        scoreTiles = new List<GameObject>();
    }

    private void Update()
    {
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

    private void OnIncomingData(string data)
    {
        string[] substring = data.Split('¤');
        if(substring[0] == "p")
        {
            //add new player
            GameObject scoreTile = Instantiate(scoreTilePrefab, scoreboardContainer.transform) as GameObject;
            scoreTile.AddComponent<ScoreTileScript>();
            scoreTile.GetComponent<ScoreTileScript>().setName(substring[1]);
            scoreTiles.Add(scoreTile);
        }
        else if(substring[0] == "a")
        {
            foreach(GameObject s in scoreTiles)
            {
                if (s.GetComponent<ScoreTileScript>().getName() == substring[1])
                {
                    s.GetComponent<ScoreTileScript>().setAnswer(substring[2]);
                    s.GetComponent("Answer").GetComponent<Text>().text = "[SUBMITTED]";
                }
            }
        }
        else if(substring[0] == "r")
        {
            foreach(GameObject s in scoreTiles)
            {
                s.GetComponent("Answer").GetComponent<Text>().text = s.GetComponent<ScoreTileScript>().getAnswer();
            }
        }
        else if(substring[0] == "s")
        {
            foreach (GameObject s in scoreTiles)
            {
                s.GetComponent("Answer").GetComponent<Text>().text = "";
            }
        }
    }
}
