using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int score;

    public int Score { get => score; set => score = value; }

    private void Start()
    {
        score = 0;
    }

    public void AddPoints(int points)
    {
        score += points;
    }

    public void ResetScore()
    {
        score = 0;
    }

}
