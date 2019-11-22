using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int pointsForAsteroid = default;

    private int score = default;

    public int Score { get => score; set => score = value; }

    private void Start()
    {
        ResetScore();
    }

    public void AddPointsForAsteroid()
    {
        score += pointsForAsteroid;
    }

    public void ResetScore()
    {
        score = 0;
        MainPanel.Instance.ResetScoreText();
    }

}
