using UnityEngine;
using UnityEngine.UI;
using Mindpower;
using UnityEngine.Events;

public class MainPanel : UISingleton<MainPanel>
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

    public UnityAction onRectTransformDimensionsChange;

    protected override void Awake()
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

    protected override void OnRectTransformDimensionsChange()
    {
        //Debug.LogWarning(GetType() + ".OnRectTransformDimensionsChange");
        onRectTransformDimensionsChange?.Invoke();
    }
}
