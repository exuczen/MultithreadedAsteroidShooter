using UnityEngine;
using UnityEngine.UI;
using DC;

public class MainPanel : Singleton<MainPanel>
{
    [SerializeField]
    private Text scoreText;

    [SerializeField]
    private Text failText;

    [SerializeField]
    private Button restartButton;

    public string ScoreVaule { get => scoreText.text; set => scoreText.text = value; }
    public Text FailText { get => failText; set => failText = value; }
    public Button RestartButton { get => restartButton; set => restartButton = value; }

    protected override void OnAwake()
    {
        ResetScoreText();
        failText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        restartButton.onClick.AddListener(() =>
        {
            restartButton.gameObject.SetActive(false);
            AppManager.Instance.RestartLevel();
        });
    }


    public void ResetScoreText()
    {
        ScoreVaule = "0";
    }
}
