using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using UnityEngine.SceneManagement;

/* 
 * 구글플레이 관련 작업 및 전체적인 어플리케이션 내부 정보 관리를 하는 클래스 
 */
public class TotalManager : MonoBehaviour
{
    List<IAchievement> achievementList; //업적내역 
    List<IAchievementDescription> achievementDescList; //업적내역설명에 대한 리스트(string의 title을 통해서 achievementList에서 사용할 id를 얻기 위한 용도) 
    private GameObject errorUI;


    private void Awake()
    {
        achievementDescList = new List<IAchievementDescription>();
        achievementList = new List<IAchievement>();
        errorUI = GameObject.Find("ErrorUI");
        errorUI.SetActive(false);

        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
            .Builder()
            .RequestServerAuthCode(false)
            .RequestIdToken()
            .Build();

        //커스텀된 정보로 GPGS 초기화 
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = false;
        //GPGS 시작. 
        PlayGamesPlatform.Activate();

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Login();
    }

    ///<summary>GPGS에 로그인 시도</summary> 
    public void Login()
    {
        Social.Active.localUser.Authenticate((success, error) => {
            if (success)
            {
                string userInfo = "Username: " + Social.localUser.userName +
                    "\nUser ID: " + Social.localUser.id +
                    "\nIsUnderage: " + Social.localUser.underage;

            }
            else
            {
                Debug.Log(error.ToString());
            }
        });
    }
    public void ShowAchievements()
    {
        if (!Social.Active.localUser.authenticated)
        {
            Login();
        }
        Social.Active.ShowAchievementsUI();
    }

    public void ShowLeaderBoard()
    {
        if (!Social.Active.localUser.authenticated)
        {
            Login();
        }
        Social.Active.ReportScore(PlayerPrefs.GetInt("Score"), GPGSIds.leaderboard_top_score, success => { Debug.Log("Success"); });
        Social.Active.ShowLeaderboardUI();
    }

    /// <summary>
    /// 어플 삭제 후 재설치 시 자신의 점수를 복구하기 위해 사용
    /// </summary>
    public void GetPreviousScore()
    {
        Social.Active.LoadScores(GPGSIds.leaderboard_top_score, score =>
        {
            if (score.Length > 0)
            {
                PlayerPrefs.SetInt("Score", int.Parse(score[0].formattedValue)) ;
            }
            else
            {
                if (SceneManager.GetActiveScene().name.Equals("TitleScene"))
                { //타이틀 씬에서만 사용
                    errorUI.SetActive(true);
                    errorUI.GetComponent<ErrorUI>().ShowError("Cannot Find Previous Score");
                }
            }
        });
    }

    public void CheckAchievement(int wave)
    {
        if (Social.Active.localUser.authenticated)
        {
            //업적 확인을 위한 업적 불러오기 
            if (achievementDescList.Count == 0)
            { //로드했던 업적설명내역이 없는 경우 
                Social.LoadAchievementDescriptions(achievementDesc =>
                {
                    if (achievementDesc.Length > 0)
                    {
                        if (achievementDescList.Count == 0) //로드한 업적이 없는 경우 
                        {
                            foreach (IAchievementDescription achievement in achievementDesc)
                            {
                                achievementDescList.Add(achievement);
                                Debug.Log(achievement.title);
                            }
                        }
                    }
                    else
                        Debug.Log("No achievements returned");
                });
            }
            if (achievementList.Count == 0)
            { //로드했던 업적설명내역이 없는 경우 
                Social.LoadAchievements(achievements =>
                {
                    if (achievements.Length > 0)
                    {
                        if (achievementList.Count == 0) //로드한 업적이 없는 경우 
                        {
                            foreach (IAchievement achievement in achievements)
                            {
                                achievementList.Add(achievement);
                            }
                        }
                    }
                    else
                        Debug.Log("No achievements returned");
                });
            }

            string clearWave = "Wave " + wave; //현재는 업적이 웨이브 통과밖에 없으므로 이런식으로 체크한다. 
            if (achievementDescList.Find(x => x.title.Contains(clearWave)) != null)
            {
                string waveAchievementID = achievementDescList.Find(x => x.title.Contains(clearWave)).id; //해당 웨이브에 맞는 업적이 있는 경우 ID를 가져온다. 

                IAchievement waveAchievement = achievementList.Find(x => x.id == waveAchievementID);
                if (!waveAchievement.completed) //아직 완료된 업적이 아니라면 완료로 변경시킨다. 
                {
                    Social.Active.ReportProgress(waveAchievementID, 100.0f, success =>
                    { //GPGS에 해당 업적 완료 보고 
                        if (success)
                        {
                            Debug.Log("Success!");
                        }
                    });
                }
            }
        }
    }
}
