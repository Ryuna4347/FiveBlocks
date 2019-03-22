using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 적 오브젝트에 대한 전반적인 기능을 작성한 스크립트
 */

public class EnemyInfo : MonoBehaviour
{
    public string pathName;
    private List<Vector3> pathList;
    private int targetPathIdx; //현재 이동목표로 삼고 있는 위치
    
    private float health; //적 유닛의 체력
    public float speed;
    public List<Sprite> effects; //피해를 입게되는 효과들의 이미지 종류
    
    private float speedNow; //상태변화 적용을 위해서 speed이외에 별도로 준비(speed는 default값이고 Enable시/상태변화 해제시에 원래 값으로 돌아가기 위해서 필요하다.)

    private string abnormal_status; //상태이상(현재는 슬로우/일시정지만 존재) 여부
    private float abnormalCoolTime; //상태이상이 남은 시간
    private bool isAbnormalChecked; //상태이상 중복 체크 방지를 위한 boolean값(기본 false)

    private GameObject appManager; //죽을때마다 find로 매니저 찾으려면 연산이 많아질거같아서 추가
    private SoundManager soundManager;
    private WaveManager waveManager; //사망처리 요구를 위함
    private TextMesh HP_UI; //체력 잔량 표시를 위한 자식 텍스트 메쉬
    private SpriteRenderer effectSprite; //특수효과 피해를 받았을 경우 나타나는 이미지들을 표시하기 위한 오브젝트

    private GameObject unitHealthText; //체력 숫자 표시를 위해서 사용하는 텍스트 UI

    public bool isWaveStart;

    private void Awake()
    {
        GameObject path=GameObject.Find("path");
        pathList = new List<Vector3>();
        appManager = GameObject.Find("gameManager");
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();

        unitHealthText = gameObject.transform.Find("HP_UI").gameObject;
        isWaveStart = false;
        
        abnormal_status = "";
        effectSprite = transform.Find("EffectSprite").GetComponent<SpriteRenderer>();
        HP_UI = transform.Find("HP_UI").gameObject.GetComponent<TextMesh>();
        speedNow = speed;
    }

    /// <summary>
    /// 적 오브젝트 재활용을 위한 초기화
    /// </summary>
    private void ResetValue()
    {
        targetPathIdx = 0;
        abnormal_status = "";
        pathList = new List<Vector3>();
        unitHealthText.SetActive(true);
        speedNow = speed;
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
        ResetValue(); //적 오브젝트 파괴 시 정보 초기화
    }

    /// <summary>
    /// 탄환에 맞아 피해를 입음
    /// </summary>
    /// <param name="damage">탄환의 피해량</param>
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
            //해당 오브젝트를 바로 재사용할 시 activeSelf가 순식간에 바뀌어 bullet의 특수효과 적용가능여부를 조사할 때
            //true=>true로 인식된다. 따라서 바로 재사용 되지 않도록 방지하기 위해서 죽음처리를 먼저한다.
            waveManager.EnemyDead(gameObject); 
            Dead();
        }
    }

    /// <summary>
    /// 적 오브젝트에 상태이상 설정
    /// </summary>
    /// <param name="abnormal_Type">상태이상 종류</param>
    /// <param name="time">상태이상 유지시간</param>
    /// <param name="slowPercent">이동속도 감소율(탄환 종류에 따라 필요)</param>
    public void SetAbnormalStatus(string abnormal_Type, float time, float slowPercent=0) 
    {
        if (abnormal_Type == "slow")
        {
            speedNow *= (1 - slowPercent);
            Debug.Log(speedNow);
            effectSprite.sprite = effects[0];
        }
        else if(abnormal_Type=="pause")
        {
            speedNow = 0f;
            effectSprite.sprite = effects[1];
        }
        Invoke("ReturnNormalStatus", time); //일정 시간 이후 원래 이동속도로 복귀
    }

    private void ReturnNormalStatus()
    {
        speedNow = speed;
        effectSprite.sprite = null;
    }
    
    /// <summary>
    /// 적 오브젝트 사망시 처리과정
    /// </summary>
    void Dead()
    {
        SwitchWaveStatus(false); //사망했기 때문에 이동하지 못하도록 처리
        soundManager.PlayAudio("EnemyDead");
        CancelInvoke("ReturnNormalStatus"); //invoke가 걸려있으면 취소
        effectSprite.sprite = null;
        gameObject.SetActive(false); //EnemyDead()를 통해 active를 조절하면 시간이 걸려서 탄환이 바로 사라지지 않음
    }

    private void Move()
    {
        Vector3 movePos= Vector2.MoveTowards((Vector2)transform.position, (Vector2)pathList[targetPathIdx], speedNow*Time.deltaTime); //목표지점을 향해 speed의 속도로 진행
        movePos.z = -1.5f;
        transform.position = movePos;
        
        if (((Vector2)transform.position == (Vector2)pathList[targetPathIdx])&&(targetPathIdx!=pathList.Count)) //movePos인 이유 : transform.position은 z좌표가 달라서 맞지 않는다.
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

    /// <summary>
    /// path 오브젝트에 포함된 Line Renderer에서 좌표를 가져옴
    /// </summary>
    private void SetPathInfomation(GameObject path)
    {
        pathName = path.name;
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
        Vector3 startPos= pathList[0];
        startPos.z = -1.5f;
        transform.position = startPos;
        targetPathIdx++;
    }

    /// <summary>
    /// 웨이브의 시작/종료를 설정(적 오브젝트의 이동을 결정)
    /// </summary>
    /// <param name="val">true/false</param>
    public void SwitchWaveStatus(bool val)
    { 
        isWaveStart = val;
    }
}
