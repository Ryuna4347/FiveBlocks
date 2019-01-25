using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyInfo : MonoBehaviour
{
    private List<Vector3> pathList;
    private int targetPathIdx; //현재 이동목표로 삼고 있는 위치
    
    private float health; //적 유닛의 체력
    public float speed;
    
    private float speedNow; //상태변화 적용을 위해서 speed이외에 별도로 준비(speed는 default값이고 Enable시/상태변화 해제시에 원래 값으로 돌아가기 위해서 필요하다.)

    private string abnormal_status; //상태이상(현재는 슬로우/일시정지만 존재) 여부
    private float abnormalCoolTime; //상태이상이 남은 시간
    private bool isAbnormalChecked; //상태이상 중복 체크 방지를 위한 boolean값(기본 false)

    private GameObject appManager; //죽을때마다 find로 매니저 찾으려면 연산이 많아질거같아서 추가
    private WaveManager waveManager; //사망처리 요구
    private TextMesh HP_UI; //체력 잔량 표시를 위한 자식 텍스트 메쉬

    private GameObject unitHealthText; //체력 숫자 표시를 위해서 사용하는 텍스트 UI

    private bool isWaveStart;

    private void Awake()
    {
        GameObject path=GameObject.Find("path");
        pathList = new List<Vector3>();
        appManager = GameObject.Find("gameManager");
        waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();

        unitHealthText = gameObject.transform.Find("HP_UI").gameObject;
        isWaveStart = false;
        
        abnormal_status = "";
        HP_UI = transform.Find("HP_UI").gameObject.GetComponent<TextMesh>();
        speedNow = speed;
    }

    private void ResetValue()
    {
        targetPathIdx = 0;
        abnormal_status = "";
        unitHealthText.SetActive(true);
        speedNow = speed;
    }

    //웨이브 시작전에 적 유닛의 체력 등을 설정(이동속도는 유닛별로 일정하므로 굳이 설정 x)
    //stage를 인자로 받는 이유 : 체력이 stage에 따라서 변동되기 때문에
    public void SetInfomation(int stage)
    { 
        health = stage;
    }

    //Update is called once per frame
    void Update()
    {
        if (isWaveStart)
        {
            Move();
        }
        HP_UI.text = health.ToString();
    }

    private void OnDisable()
    {
        ResetValue();
    }

    public void GetDamaged(float damage)
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
            health = 0;
            Dead();
            waveManager.EnemyDead(gameObject);
        }
    }

    public void SetAbnormalStatus(string abnormal_Type, float time, float slowPercent=0) //인자 : 상태이상 종류, 상태이상 유지시간, 이동속도 감소율(이건 필수가 아님)
    {
        if (abnormal_Type == "slow")
        {
            speed *= (1 - slowPercent);
        }
        else if(abnormal_Type=="pause")
        {
            speed = 0f;
        }
        Invoke("ReturnNormalStatus", time);
    }

    private void ReturnNormalStatus()
    {
        speedNow = speed;
    }


    void Dead()
    {
        SwitchWaveStatus(false);
        gameObject.SetActive(false); //EnemyDead()를 통해 active를 조절하면 시간이 걸려서 총알이 바로 사라지지 않음
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, pathList[targetPathIdx], speedNow*Time.deltaTime); //목표지점을 향해 speed의 속도로 진행
        if (transform.position == pathList[targetPathIdx]&&(targetPathIdx!=pathList.Count))
        { //목표지점에 도착을 하고 그 목표지점이 종료지점이 아닌 경우에는 다음 목표지점으로 설정
            targetPathIdx++;
        }
        if(targetPathIdx == pathList.Count){ //맨 끝까지 도착시 더는 움직일 필요가 없다.
            waveManager.GameOver();
            isWaveStart = false;
        }
    }

    public void SetEnemyInformation(int heal, GameObject path)
    { //적 유닛의 정보를 웨이브에 맞게 전달받는다.
        health = heal;
        SetPathInfomation(path);
    }

    //path오브젝트에 포함된 line Renderer에서 좌표를 가져옴
    private void SetPathInfomation(GameObject path)
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
        //Debug.Log(gameObject.name + " " + pathList[0]);
        transform.position = pathList[0];
    }

    public void SwitchWaveStatus(bool val)
    { //웨이브의 시작을 알림
        isWaveStart = val;
    }
}
