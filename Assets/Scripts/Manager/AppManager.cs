using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 *전체적인 게임 진행에 관한 매니저
 */
public class AppManager : MonoBehaviour
{
    public List<GameObject> emptyArea; //현재 비어있는 공간
    public List<GameObject> usedArea; //현재 사용중인 공간
    public List<GameObject> waitBlocks; //대기 유닛들
    public List<GameObject> usedBlocks; //사용중인 유닛들
    private bool yellowAlreadyInstalled; //지원블럭(노란 블럭)은 전체 블럭 중 1개만 사용가능하므로 현재 설치여부를 나타내는 변수

    public GameObject blocksParent; //블럭유닛을 모아둘 상위 빈 오브젝트
    private TotalManager totalManager;
    private SoundManager audio; //게임진행 시 나올 소리를 위한 오디오매니저
    public WaveManager waveManager;
    public EnchantManager enchantManager;
    public TouchBlockUI touchBlockUI; //NoticeCanvas내의 이미지를 켜는 도중 다른 장소의 터치를 막는 이미지(일시정지, 블럭 정보, 게임 종료)
    public GameObject waveStartBtn; //웨이브 시작 버튼
    public GameObject clearWaveNotice; //웨이브 클리어 성공에 따른 안내문
    public Text moneyText; //현재 소지금을 표시
    public Text requiredMoneyText; //블럭 생성에 필요한 금액을 표시하는 오브젝트
    public GameObject speedControlBtn; //게임 진행 속도 변경 버튼
    public Text gameSpeedText; //게임 진행 속도를 표시하는 텍스트
    public GameObject tutorialUI; //튜토리얼 관련 UI를 관리하기 위한 오브젝트

    private bool isGameOver;
    public bool isWaveProcessing; //현재 웨이브가 진행중인가?(웨이브 도중 블럭 생성시 바로 탄환 발사가 되게 조절해야해서 추가함)
    private bool isFirstGame; //처음 게임을 하는가에 대한 bool값(처음이면 튜토리얼을 켠다. playerPrefs로 관리)

    private int money; //블럭 생성 및 강화에 사용되는 돈(적 유닛 제거시 지급)
    private int createBlockCost; //블럭 생성에 필요한 돈(누적해서 올라감)

    private void Awake()
    {
        Screen.SetResolution(1080, 1920, true);
        totalManager = GameObject.Find("TotalManager").GetComponent<TotalManager>();
        audio = GameObject.Find("SoundManager").GetComponent<SoundManager>(); //사운드매니저가 타이틀 씬에서 넘어오기 때문에 동적으로 추가해줘야 한다.
        audio.gameObject.transform.parent = transform.parent; //사운드매니저가 타이틀 씬에서 넘어오기 때문에 부모가 정해지지 않았기 때문에 매니저 그룹오브젝트를 부모로 설정한다.
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadBlockData();
        waveManager.LoadGameData();

        SetDefault();

        RegisterEmptyArea(); //맵을 프리팹으로 만듦에 따라 에디터에서 등록해서 사용하는 방법을 쓸수가 없다.
                             //따라서 맨 처음 맵이 불려진 이후 EmptyArea를 저장하고 맵이 변경되면 EmptyArea, usedArea를 복사해서 옮겨줘야한다.

        waveManager.ReadyForWave(1);

        isFirstGame = PlayerPrefs.GetInt("isFirstGame",-1)==-1 ? true : false;
        if (isFirstGame)
        {
            tutorialUI.SetActive(true);
            PlayerPrefs.SetInt("isFirstGame", 1); //최초 1번만 자동으로 튜토리얼이 뜬다.
        }
    }
    
    /// <summary>
    /// 블럭생성 버튼을 클릭할 경우 빈 공간에 공격을 위한 블럭 오브젝트를 생성하는 함수
    /// </summary>
    public void MakeBlock()
    {
        if (!isGameOver) //게임 진행중에만 사용가능
        {
            if (usedBlocks.Count == 36)
            { //36칸이 다 차있다는 뜻이므로 블럭 생성이 불가
                return;
            }
            if (!CheckMoney(createBlockCost))
            { //돈이 부족한 경우도 실패
                return;
            }
            
            int yellowBlockInstalled = yellowAlreadyInstalled ? 1 : 0;

            int typeRandom = Random.Range(0, 5-yellowBlockInstalled); //지원블럭(노란색)의 설치여부에 따라 0~3/0~4로 랜덤 범위가 달라짐
            string BlockTypeString = "Block_"; //기본적으로 블럭 오브젝트 프리팹의 명칭이 '색상Block_'이므로 공통적인 부분은 미리 작성한다.
            switch (typeRandom)
            {
                case 0:
                    BlockTypeString = "Blue" + BlockTypeString;
                    break;
                case 1:
                    BlockTypeString = "Purple" + BlockTypeString;
                    break;
                case 2:
                    BlockTypeString = "Red" + BlockTypeString;
                    break;
                case 3:
                    BlockTypeString = "Grey" + BlockTypeString;
                    break;
                case 4:
                    BlockTypeString = "Yellow" + BlockTypeString;
                    yellowAlreadyInstalled = true; //노란색 블럭의 경우 동시에 1개만 설치가 가능하기 때문에 뒤에 생성되는 것을 방지
                    break;
                default:
                    break;
            }

            //현재 active가 꺼져있고 원하는 색상이 맞는 유닛을 불러온다.
            GameObject properBlockObj = waitBlocks.Find(x => x.name.Contains(BlockTypeString) && x.activeSelf == false); 
            properBlockObj.SetActive(true);
            if (isWaveProcessing) //이미 웨이브 진행중일 때 블럭을 생성시 바로 상태를 바꿔주어서 공격할 수 있게
            {
                properBlockObj.GetComponent<BlockInfo>().SwitchWaveStatus(true);
            }

            int randomPos = Random.Range(0, emptyArea.Count); //빈 곳 중에 랜덤하게 1곳
            Vector3 blockPos = emptyArea[randomPos].transform.position;
            blockPos.z = -1; //블럭이 제일 위에 보이도록 쌓아야해서 z축을 고정
            properBlockObj.GetComponent<BlockInfo>().InstallAtPos(blockPos); //설치하면서 blockType구분을 하기위해서 함수로 변경
            properBlockObj.GetComponent<BlockInfo>().SetBlockLevel(1);

            //현재 설치하는 블럭이 노란 블럭이 아니고 노란블럭이 설치가 되어있는 상태이면
            //현재 설치하는 블럭이 노란 블럭 근방인지 체크하고 맞다면 버프효과를 받게한다.
            if (yellowAlreadyInstalled&&(typeRandom!=4)) {
                GameObject yellowBlock = usedBlocks.Find(x => x.name.Contains("Yellow"));
                if(Vector2.Distance(yellowBlock.transform.position, properBlockObj.transform.position) <= 0.6f)
                {
                    yellowBlock.GetComponent<SupportBlockInfo>().EnhanceNearBlock(properBlockObj);
                }
            }

            audio.PlayAudio("CreateBlock"); //블럭 생성에 대한 사운드 재생

            waitBlocks.Remove(properBlockObj); //선택한 블럭의 정보를 대기중->사용중으로 이동
            usedBlocks.Add(properBlockObj);
            usedArea.Add(emptyArea[randomPos]); //블럭이 설치된 위치 역시 대기중->사용중으로 이동
            emptyArea.RemoveAt(randomPos);

            UseMoney(createBlockCost);
            createBlockCost += 5; //블럭을 생성할 때마다 가격이 계속 상승
            requiredMoneyText.text = createBlockCost.ToString();
        }
    }

    /// <summary>
    /// 블럭 프리팹을 로드하여 생성
    /// </summary>
    private void LoadBlockData()
    {
        GameObject[] blocks = Resources.LoadAll<GameObject>("Prefabs/Blocks");

        foreach (GameObject obj in blocks)
        {
            GameObject temp = new GameObject();
            temp.name = obj.name; //유닛별로 모아두기 위한 빈 오브젝트
            temp.transform.parent = blocksParent.transform;

            for (int i = 0; i < 36; i++)
            {
                GameObject block = GameObject.Instantiate(obj);
                block.name = obj.name + i;
                block.transform.parent = temp.transform;

                block.SetActive(true);
                waitBlocks.Add(block);
                block.SetActive(false);
            }
        }
    }

    /*

     */
     /// <summary>
     /// 블럭 레벨업 함수에서 옮겨지는 블럭(빈 장소가 될 위치)의 처리를 위한 함수
     /// 블럭과 블럭이 위치한 곳의 emptyArea처리
    /// </summary>
    /// <param name="movingBlock">현재 드래그한 블럭</param>
    public void MoveUsedToEmpty(GameObject movingBlock)
    {
        GameObject willBeEmpty = usedArea.Find(x => (x.transform.position.x == movingBlock.transform.position.x) && (x.transform.position.y == movingBlock.transform.position.y));
        //z는 Block과 EmptyArea가 다르므로 z를 빼고 비교를 해야 된다.

        usedArea.Remove(willBeEmpty);
        emptyArea.Add(willBeEmpty);
        movingBlock.transform.position = new Vector3(-5, 0, 0);
        movingBlock.SetActive(false);
        usedBlocks.Remove(movingBlock);
        waitBlocks.Add(movingBlock);
    }
    
     /// <summary>
     /// 블럭 레벨업 함수조건 : 동일 레벨&동일 종류
     /// (단, 노란 블럭은 동일 레벨만 충족하면 어느 블록과도 레벨업 가능)
     /// 레벨업 시 합치는 두 개의 블럭은 제거, 랜덤하게 다음 레벨의 블럭이 생성
    /// </summary>
    /// <param name="obj_1"></param>
    /// <param name="obj_2"></param>
    public void BlockLevelUp(GameObject obj_1, GameObject obj_2) //obj_2 위치에 다음 레벨을 생성
    { 
        string obj2_type = obj_2.GetComponent<BlockInfo>().blockName; //종류 비교를 위해 블럭정보 스크립트에서 이름을 획득
        
        if ((obj_1.name.Contains(obj2_type))||(obj_1.GetComponent<BlockInfo>().blockAttType=="support"||obj_2.GetComponent<BlockInfo>().blockAttType=="support")) //동일한 종류(소문자/대문자 착각 방지), 아니면 둘 중 하나가 지원블럭일 경우 레벨업 가능
        {
            if (obj_1.GetComponent<BlockInfo>().blockLevel == obj_2.GetComponent<BlockInfo>().blockLevel)
            {//동일레벨, 동일종류일 시 레벨업
                
                //새로 블럭이 생길 위치(obj_2)부터 제거
                Vector3 obj2Pos = obj_2.transform.position;
                int blockLev = obj_2.GetComponent<BlockInfo>().blockLevel;
                obj_2.transform.position = new Vector3(-5, 0, 0);
                obj_2.SetActive(false); //먼저 기존에 있던 블럭을 제거해야 Support블럭에서 버프제거를 하고 새로운 블럭이 들어올 수 있다.
                usedBlocks.Remove(obj_2); 
                waitBlocks.Add(obj_2);

                //블럭이 옮겨진 위치(obj_1) 부분을 제거(obj_1은 emptyArea까지 처리해야함)
                MoveUsedToEmpty(obj_1);
                if ((obj_1.GetComponent<BlockInfo>().blockAttType == "support")|| (obj_1.GetComponent<BlockInfo>().blockAttType == "support"))
                {//합쳐지는 두 블럭 중 노란 블럭이 존재할 경우 노란 블럭이 제거되는 것이므로 설치여부를 false로 돌림
                    yellowAlreadyInstalled = false;
                }

                int yellowBlockInstalled = yellowAlreadyInstalled ? 1 : 0;

                int typeRandom = Random.Range(0, 5 - yellowBlockInstalled); //지원블럭(노란색)의 설치여부에 따라 0~3/0~4로 랜덤 범위가 달라짐
                string BlockTypeString = "Block_";
                switch (typeRandom)
                {
                    case 0:
                        BlockTypeString = "Blue" + BlockTypeString;
                        break;
                    case 1:
                        BlockTypeString = "Purple" + BlockTypeString;
                        break;
                    case 2:
                        BlockTypeString = "Red" + BlockTypeString;
                        break;
                    case 3:
                        BlockTypeString = "Grey" + BlockTypeString;
                        break;
                    case 4:
                        BlockTypeString = "Yellow" + BlockTypeString;
                        yellowAlreadyInstalled = true;
                        break;
                    default:
                        break;
                }

                //레벨업한 블럭 생성
                GameObject properBlockObj = waitBlocks.Find(x => x.name.Contains(BlockTypeString) && x.activeSelf == false); //현재 active가 꺼져있고 색상이 맞는 유닛을 불러온다.
                
                properBlockObj.SetActive(true); //새로 들어올 블럭은 obj_2의 위치에 들어가야하기 때문에 obj_2를 없애기 전에 위치를 전달받고 옮겨놓기
                properBlockObj.GetComponent<BlockInfo>().InstallAtPos(obj2Pos);
                properBlockObj.GetComponent<BlockInfo>().SetBlockLevel(blockLev + 1);
                audio.PlayAudio("Synthesize");

                usedBlocks.Add(properBlockObj); //새로 레벨업 한 블럭 추가

                if (isWaveProcessing) //이미 웨이브 진행중일 때 블럭을 생성시 바로 상태를 바꿔주어서 공격할 수 있게
                {
                    properBlockObj.GetComponent<BlockInfo>().SwitchWaveStatus(true);
                }

                if (yellowAlreadyInstalled && (typeRandom != 4)) //현재 설치하는 블럭이 노란 블럭이 아니고 노란블럭이 설치가 되어있는 상태이면 현재 설치하는 블럭이 노란 블럭 근방인지 체크하고 맞다면 버프효과를 받게한다.
                {
                    GameObject yellowBlock = usedBlocks.Find(x => x.name.Contains("Yellow"));
                    if (Vector2.Distance(yellowBlock.transform.position, properBlockObj.transform.position) <= 0.6f)
                    {
                        yellowBlock.GetComponent<SupportBlockInfo>().EnhanceNearBlock(properBlockObj);
                    }
                }
            }
        }

    }
    
    /// <summary>
    /// 게임 처음 시작시 EmptyArea 오브젝트를 리스트에 넣기 위한 함수
    /// </summary>
    private void RegisterEmptyArea()
    {
        foreach (Transform childMapData in GameObject.Find("MapGroup").transform)
        {
            if ((childMapData.gameObject.name.Contains("Map_")) && (childMapData.gameObject.activeSelf == true))
            {
                GameObject EmptyAreaGroup = childMapData.Find("EmptyGroup").gameObject;
                foreach (Transform emptyAreaData in EmptyAreaGroup.transform)
                {
                    emptyArea.Add(emptyAreaData.gameObject);
                }
            }
        }
    }
    
    /// <summary>
    /// 게임 진행 중 맵의 변경으로 emptyArea에 해당하는 블럭들을 새로운 맵의 emptyArea로 옮기기 위해 사용하는 함수
    /// (게임 시작 직후에 사용하는 함수 아님)
    /// </summary>
    /// <param name="newMap"></param>
    public void RegisterEmptyArea(GameObject newMap)
    {
        List<GameObject> newEmptyArea = new List<GameObject>();

        GameObject EmptyAreaGroup = newMap.transform.Find("EmptyGroup").gameObject;
        foreach (GameObject existedEmpty in emptyArea)
        { //현재 emptyArea에 포함된 위치를 돌아다니면서 newMap 하위에 이름이 같은 오브젝트를 껴넣는다.
            Transform newMapEmptyAreaObj = EmptyAreaGroup.transform.Find(existedEmpty.name);
            if (newMapEmptyAreaObj != null)
            { //emptyArea에 있는 이름들로 새 맵에 있는 동일한 emptyArea들을 찾아냄
                newEmptyArea.Add(newMapEmptyAreaObj.gameObject);
            }
        }
        emptyArea = newEmptyArea;

        List<GameObject> newUsedArea = new List<GameObject>();
        foreach (GameObject existedUsed in usedArea)
        { //현재 emptyArea에 포함된 위치를 돌아다니면서 newMap 하위에 이름이 같은 오브젝트를 껴넣는다.

            Transform newMapUsedAreaObj = EmptyAreaGroup.transform.Find(existedUsed.name);
            if (newMapUsedAreaObj != null)
            { //emptyArea에 있는 이름들로 새 맵에 있는 동일한 emptyArea들을 찾아냄
                newUsedArea.Add(newMapUsedAreaObj.gameObject);

                //기존에 usedArea의 위치에 있던 block을 nesMapUsedAreaObj의 위치로 옮김
                GameObject block = usedBlocks.Find(x => (x.transform.position.x == existedUsed.transform.position.x) && (x.transform.position.y == existedUsed.transform.position.y));
                Vector3 newBlockPos = newMapUsedAreaObj.transform.position;
                newBlockPos.z = -1; //z축이 area와 같으면 겹치니까 z는 따로 빼준다.
                block.transform.position = newBlockPos;
            }
        }
        usedArea = newUsedArea;

    }

    /// <summary>
    /// 적 오브젝트 파괴 시 해당 적 오브젝트를 타겟으로 두고있는 탄환들을 전부 무효화
    /// </summary>
    /// <param name="deadEnemy">파괴된 적 오브젝트</param>
    public void CheckBlockTarget(GameObject deadEnemy)
    {
        foreach(GameObject block in usedBlocks)
        {
            block.GetComponent<BlockInfo>().CheckTarget(deadEnemy);
        }
    }
    
    /// <summary>
    /// 전달받은 지원형 유닛의 위치 근방(대각선 제외 상하좌우만)에 위치한 유닛을 불러온다.
    /// </summary>
    /// <param name="yellowPos"></param>
    /// <returns></returns>
    public List<GameObject> GetNearBlocks(Transform yellowPos)
    {

        List<GameObject> nearBlocks = usedBlocks.FindAll(x=>Vector3.Distance(yellowPos.position,x.transform.position)<=0.6f); //블럭 1개정도 차이(빈칸을 생각해서 0.05 정도 오차를 둠)가 나는 주위의 블럭들을 가져옴
        return nearBlocks;
    }
    
    /// <summary>
    /// 현재 사용중인 블럭 중에 blockType의 블럭을 반환
    /// (이름에 블럭 종류가 포함되어있으니 그것으로 구분)
    /// </summary>
    /// <param name="blockType"></param>
    /// <returns></returns>
    public List<GameObject> GetBlocksByType(string blockType)
    {
        return usedBlocks.FindAll(x => x.name.Contains(blockType));
    }
    
    /// <summary>
    /// 웨이브의 시작 알림
    /// </summary>
    public void WaveStart()
    {
        isWaveProcessing = true;
        foreach (GameObject block in usedBlocks)
        {
            block.GetComponent<BlockInfo>().SwitchWaveStatus(true);
        }

        waveStartBtn.SetActive(false);
        speedControlBtn.SetActive(true);
    }
    
    /// <summary>
    /// 웨이브 클리어로 더 이상 블럭들의 공격을 방지
    /// </summary>
    /// <param name="n">클리어한 웨이브 번호</param>
    public void WaveEnd(int n)
    {
        isWaveProcessing = false;
        foreach (GameObject block in usedBlocks)
        {
            block.GetComponent<BlockInfo>().SwitchWaveStatus(false);
        }

        clearWaveNotice.SetActive(true); //웨이브 성공에 따른 안내 UI On
        clearWaveNotice.GetComponent<WaveNotice>().ControlChildNotice(n + 1); //다음 웨이브를 전달하여 보스출현/일반 웨이브인지 구별하여 텍스트를 켤수있도록 함

        waveStartBtn.SetActive(true);
        speedControlBtn.SetActive(false);

        totalManager.CheckAchievement(n); //n스테이지에 대한 업적 체크

        waveManager.ReadyForWave(n + 1);
    }
    
    /// <summary>
    /// 게임 종료
    /// </summary>
    public void GameOver()
    {
        isGameOver = true;
        isWaveProcessing = false;

        foreach (GameObject block in usedBlocks)
        {
            block.GetComponent<BlockInfo>().SwitchWaveStatus(false);
            block.SetActive(false);
        }

        //게임 종료를 보여주는 ui를 켜고, 다시하기 버튼 등을 자리시킴
        touchBlockUI.gameObject.SetActive(true);
        touchBlockUI.ActiveUI("GameOver");

        int score = waveManager.GetWaveNow()-1;
        if (score > PlayerPrefs.GetInt("Score")) //최고점수가 갱신되야 한다면
        {
            PlayerPrefs.SetInt("Score", (waveManager.GetWaveNow() - 1));
        }

        speedControlBtn.SetActive(false); //게임이 종료되었으므로 처음 상태로 웨이브 시작버튼을 돌려놓는다.
        waveStartBtn.SetActive(true);
    }
    
    /// <summary>
    /// 게임 진행 중 일시정지
    /// </summary>
    /// <param name="val"></param>
    public void GamePause(bool val)
    {
        Time.timeScale = (val == true) ? 0 : 1;
        if (!val)
        {
            touchBlockUI.UnactiveUI("Pause");
        }
        touchBlockUI.gameObject.SetActive(val);
        if (val)
        {
            touchBlockUI.ActiveUI("Pause");
        }
    }

    /// <summary>
    /// 웨이브의 진행속도 변경(1,3,9배속 토글 변경)
    /// </summary>
    public void GameSpeedChange()
    {
        Time.timeScale *= 3;
        if (Time.timeScale > 9) //9배율을 넘을경우 1배로 귀환
        {
            Time.timeScale = 1.0f;
        }
        gameSpeedText.text = Time.timeScale.ToString();
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void GameRestart()
    {
        waveManager.SetDefault();
        enchantManager.SetDefault();
        SetDefault();
        Time.timeScale = 1.0f;
        gameSpeedText.text = "1";
        if (speedControlBtn.activeSelf == true) { speedControlBtn.SetActive(false); }
        waveStartBtn.SetActive(true);

        waveManager.ReadyForWave(1);
    }

    /// <summary>
    /// 초기시작/재시작 시 모든 변수 초기화
    /// </summary>
    private void SetDefault()
    {
        isGameOver = false;

        if (usedArea.Count > 0)
        {
            foreach (GameObject area in usedArea)
            {
                emptyArea.Add(area);
            }
        }
        if (usedBlocks.Count > 0)
        {
            foreach (GameObject block in usedBlocks)
            {
                waitBlocks.Add(block);
                block.SetActive(false);
            }
        }
        
        usedArea = new List<GameObject>();
        usedBlocks = new List<GameObject>();

        yellowAlreadyInstalled = false;
        money = 5; //소지금 및 블럭 생성 요구 비용 초기화
        createBlockCost = 0;
        moneyText.text = money.ToString();
        requiredMoneyText.text = createBlockCost.ToString();

        touchBlockUI.UnactiveUI("GameOver"); //게임종료 UI 끄기
        touchBlockUI.gameObject.SetActive(false);
    }
    
     /// <summary>
     /// 소지금이 충분한가?
     /// </summary>
     /// <param name="cost">비용</param>
     /// <returns></returns>
    public bool CheckMoney(int cost)
    {
        if (money >= cost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public void UseMoney(int cost)
    {
        if (cost > 0)
        {
            money -= cost;
            moneyText.text = money.ToString();
        }
    }
    
    /// <summary>
    /// 적 유닛의 제거로 돈 획득
    /// </summary>
    public void IncreaseMoney()
    {
        money += 5;
        moneyText.text = money.ToString();
    }
}
