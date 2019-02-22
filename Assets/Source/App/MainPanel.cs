using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    [SerializeField]
    private Text scoreVaule;

    private void Awake()
    {
        ResetScoreText();
    }

    public string ScoreVaule { get => scoreVaule.text; set => scoreVaule.text = value; }

    public void ResetScoreText()
    {
        ScoreVaule = "0";
    }
}
