using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnHost()
    {
        SceneManager.LoadScene(1);
    }

    public void OnJoin()
    {
        SceneManager.LoadScene(2);
    }
}
