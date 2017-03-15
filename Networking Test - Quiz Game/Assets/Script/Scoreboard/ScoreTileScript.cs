using UnityEngine;
using System.Collections;

public class ScoreTileScript : MonoBehaviour
{
    private string playerName = "nothing";
    private string cAnswer;
    private int score;

    public void setName(string name)
    {
        playerName = name;
    }

    public string getName()
    {
        return playerName;
    }

    public void setAnswer(string a)
    {
        cAnswer = a;
    }

    public string getAnswer()
    {
        return cAnswer;
    }
}
