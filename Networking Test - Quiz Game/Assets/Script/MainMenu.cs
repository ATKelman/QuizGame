using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public InputField ServerNameField;

    public void OnHost()
    {
        GameState.serverName = ServerNameField.text;
        SceneManager.LoadScene(1);
    }

    public void OnJoin()
    {
        SceneManager.LoadScene(2);
    }
}
