using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerTileScript : MonoBehaviour
{
    public Text scoreText;

    private int score = 0;

    public void IncreaseScore()
    {
        score++;
        scoreText.text = score.ToString();
    }
    public void DecreaseScore()
    {
        score--;
        scoreText.text = score.ToString();
    }
}
