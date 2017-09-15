using UnityEngine;
using System.Collections;

public class GameState : MonoBehaviour
{
    public static string serverName;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
}
