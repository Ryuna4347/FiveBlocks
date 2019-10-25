using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;



/*
 * 구글플레이 관련 작업 및 전체적인 어플리케이션 내부 정보 관리를 하는 클래스
 */
public class TotalManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
        .Builder()
        .RequestServerAuthCode(false)
        .RequestIdToken()
        .Build();
        //커스텀된 정보로 GPGS 초기화
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        //GPGS 시작.
        PlayGamesPlatform.Activate();
        

        Invoke("Login", 15.0f);
    }
    

    private void Login()
    {
        Social.Active.localUser.Authenticate(success => {
            if (success)
            {
                Debug.Log("Authentication successful");
                string userInfo = "Username: " + Social.localUser.userName +
                    "\nUser ID: " + Social.localUser.id +
                    "\nIsUnderage: " + Social.localUser.underage;
            }
            else
            {
                Debug.Log("실패");
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
    }
}
