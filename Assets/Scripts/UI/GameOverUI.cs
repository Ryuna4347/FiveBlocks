using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    private WaveManager waveManager;
    private GameObject gameOverObjs; //GameOverBG : 애니메이션 효과를 주기위한 오브젝트
                                     //GameOverObjs : GameOverBG의 애니메이션 종료 후 내부 텍스트를 보여주기 위한 용도
    public Text bestScore;
    public Text nowScore;

    private void Awake()
    {
        waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();
        gameOverObjs = gameObject.transform.Find("GameOverObjs").gameObject;
    }

    private void OnDisable()
    {
        nowScore.text = "";
    }

    public void ShowScore()
    {
        gameOverObjs.SetActive(true);
        SetScoreText();
    }

    private void SetScoreText()
    {
        //highScore갱신
        nowScore.text = waveManager.GetWaveNow().ToString();
    }
}
