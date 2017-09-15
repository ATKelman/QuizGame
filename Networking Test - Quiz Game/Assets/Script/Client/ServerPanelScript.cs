using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ServerPanelScript : MonoBehaviour
{
    private string serverName;
    private string serverIP;

    public Text nameText;
    public Text IPText;

    public bool isPressed = false;

    public void initializePanel(string name, string IP)
    {
        serverIP = IP;
        serverName = name;
        nameText.text = serverName;
        IPText.text = serverIP;
    }

    public void onClick()
    {
        isPressed = true;
    }

    public string getIP()
    {
        return serverIP;
    }
}
