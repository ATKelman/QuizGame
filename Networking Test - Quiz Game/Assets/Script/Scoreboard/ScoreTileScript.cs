using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreTileScript : MonoBehaviour
{
    public Text answer;
    public Text pName;

    private string playerName = "nothing";
    private string cAnswer;
    private int score;

    public void setName(string name)
    {
        playerName = name;
        pName.text = playerName;
        answer.text = "Waiting...";
    }

    public string getName()
    {
        return playerName;
    }

    public void setAnswer(string a)
    {
        cAnswer = a;
        answer.text = "[SUBMITTED!]";
    }

    public string getAnswer()
    {
        return cAnswer;
    }

    public void RevealAnswer()
    {
        answer.text = cAnswer;
    }

    public void ResetAnswer()
    {
        answer.text = "Waiting...";
    }
}
