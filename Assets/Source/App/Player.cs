using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int score;

    public int Score { get => score; set => score = value; }

    private void Start()
    {
        ResetScore();
    }

    public void AddPoints(int points)
    {
        score += points;
    }

    public void ResetScore()
    {
        score = 0;
        MainPanel.Instance.ResetScoreText();
    }

}
