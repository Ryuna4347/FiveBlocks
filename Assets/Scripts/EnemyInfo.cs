using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyInfo : MonoBehaviour
{
    private float health;
    public float speed;
    private List<Vector3> pathList;
    private int targetPathIdx; //현재 이동목표로 삼고 있는 위치
    private string abnormal_status; //상태이상(현재는 슬로우/일시정지만 존재) 여부
    private float abnormalCoolTime; //상태이상이 남은 시간
    private bool isAbnormalChecked; //상태이상 중복 체크 방지를 위한 boolean값(기본 false)

    private GameObject appManager; //죽을때마다 find로 매니저 찾으려면 연산이 많아질거같아서 추가

    private GameObject unitHealthText; //체력 숫자 표시를 위해서 사용하는 텍스트 UI


    private void Awake()
    {
        GameObject path=GameObject.Find("path");
        pathList = new List<Vector3>();
        appManager = GameObject.Find("AppManager");
    }

    private void ResetValue()
    {
        targetPathIdx = 0;
        abnormal_status = "";
        unitHealthText.SetActive(true);
    }

    public void SetInfomation(int stageHealth)
    { //웨이브 시작전에 적 유닛의 체력 등을 설정(이동속도는 유닛별로 일정하므로 굳이 설정 x)
        health = stageHealth;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (abnormal_status != ""&&(isAbnormalChecked==false))
    //    {
    //        if (abnormal_status.Contains("Slow_Down"))
    //        {
    //            string seperatedStr = abnormal_status.Split('_')[2]; //형식이 Slow_Down_속도저하율 순으로 나오기 때문에 3번째 나오는 문자열을 받아야한다.
    //            speed *= (100-int.Parse(seperatedStr)) / 100f; //속도저하율 만큼 속도 하향
    //            abnormalCoolTime = 1f;
    //        }
    //        else if(abnormal_status.Contains("Stop")){
    //            speed = 0;
    //            abnormalCoolTime = 1f;
    //        }
    //    }

    //    if (abnormalCoolTime != 0)
    //    {
    //        abnormalCoolTime -= Time.deltaTime;
    //    }
    //    else if ((abnormalCoolTime <= 0) && (isAbnormalChecked == true)) //현재 상태이상이 걸려있고 시간이 지나서 해제해야하는 경우
    //    {
    //        abnormalCoolTime = 0;
    //        isAbnormalChecked = false;
    //        abnormal_status = "";
    //        speed = 1f;
    //    }
    //    Move();
        
    //}

    public void GetDamaged(int damage, string statusChanged="")
    {
        health -= damage;
        if ((health<=999)&&(health >= 0))
        {
            transform.GetChild(0).GetComponent<TextMesh>().text = health.ToString();
        }
        else if(health>999){ //이 이상의 체력은 너무 커서 텍스트가 오브젝트를 넘어감
            transform.GetChild(0).GetComponent<TextMesh>().text = "999+";
        }

        if (health <= 0)
        {
            appManager.GetComponent<AppManager>().EnemyDead(gameObject); //적 유닛 제거 및 남은 유닛수 표시, 폭파 사운드 재생를 위해 appManager에서 처리 요청
            Dead();
        }

        if (statusChanged != "")
        { //상태이상을 거는 탄환에 맞았을 경우 해당 상태 저장
            abnormal_status = statusChanged;
        }
    }

    void Dead()
    {
        ResetValue();
        unitHealthText.SetActive(false);
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, pathList[targetPathIdx], speed); //목표지점을 향해 speed의 속도로 진행
        if (transform.position == pathList[targetPathIdx]&&((targetPathIdx+1)!=pathList.Count))
        { //목표지점에 도착을 하고 그 목표지점이 종료지점이 아닌 경우에는 다음 목표지점으로 설정
            targetPathIdx++;
        }
    }

    //path오브젝트에 포함된 line Renderer에서 좌표를 가져옴
    public void SetPathInfomation(GameObject path)
    {
        LineRenderer pathVert = path.GetComponent<LineRenderer>();
        int pathVertNum = pathVert.positionCount;
        
        for(int i=0; i<pathVertNum; i++)
        {
            pathList.Add(pathVert.GetPosition(i));
        }
    }

    public void SetAtStartLine() //적 유닛 사용을 위해 시작 지점에 두기
    {
        transform.position = pathList[0];
    }
}
