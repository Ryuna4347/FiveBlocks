using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

[System.Serializable]
public class WaveInfoJson
{//json파일을 읽어서 defunit에 넣기 전에 중간과정(유니티의 xml 파서가 기본형밖에 지원 안 해주기 때문)
    //스테이지별 모든 유닛의 정보가 담겨있다.
    public WaveInfo[] waves;
}

[System.Serializable]
public class WaveInfo
{
    public int waveNow; //현재 웨이브
    public string waveMapName; //웨이브가 진행 될 맵이름(맵이름이 다를 경우 gameManager에 신 이동 요구)
    public int pathInfo; //적 오브젝트들이 이동할 길의 이름

    public string[] unitName;
    public int[] numOfUnit;
}


class WaveUnitInfo //각 웨이브에 등장하는 각 유닛들의 이름과 갯수 등을 저장하는 클래스
{
    private string unitName;
    private int unitNum;

    public WaveUnitInfo(string name, int num)
    {
        unitName = name;
        unitNum = num;
    }

    public string GetUnitName()
    {
        return unitName;
    }
    public int GetUnitNum()
    {
        return unitNum;
    }
}

class Wave //각 웨이브의 정보를 소유하는 클래스
{
    private int waveNum;
    private string waveMapName;
    private GameObject wavePath;
    private List<WaveUnitInfo> unitInfo;

    public Wave(){
        unitInfo = new List<WaveUnitInfo>();
    }

    public void SetWave(int n, string mapName, GameObject pathInfo, List<string> EnemyList)
    {
        waveNum = n;
        waveMapName=mapName;
        wavePath = pathInfo;

        foreach (string enemyName in EnemyList)
        { //웨이브 내에 유닛 정보(유닛이름, 숫자) 저장

            string splitEnemyName = enemyName.Split('-')[0]; //enemyName의 구성 적군이름(Enemy_OOO)-적유닛 갯수
            int splitEnemyNum = int.Parse(enemyName.Split('-')[1]);

            WaveUnitInfo unit = new WaveUnitInfo(splitEnemyName,splitEnemyNum);
            unitInfo.Add(unit);
        }
    }

    public List<WaveUnitInfo> GetWaveUnitInfo()
    {
        return unitInfo;
    }
    public GameObject GetWavePathInfo()
    {
        return wavePath;
    }
    public string GetWaveMapName()
    {
        return waveMapName;
    }
    public int GetWaveNum()
    {
        return waveNum;
    }
}

public class WaveManager : MonoBehaviour
{
    private List<GameObject> allEnemy; //현재 로드 되어있는 모든 적 유닛
    private List<GameObject> usingEnemy; //현재 웨이브에서 사용되는 적 유닛
    private List<Wave> waveInfo;
    private List<GameObject> allMap;
    private List<GameObject> allPath;
    private List<GameObject> enemyPrefabs; //적 유닛들의 프리팹(랜덤 웨이브 생성시 사용하기 위해서 클래스 내 변수로 변경)

    public AppManager appManager;

    //현재 웨이브 맵 관련 오브젝트
    private GameObject mapNow; //현재 맵과 경로(다음 웨이브와 비교하여 변경 필요여부 조사)
    private GameObject pathNow;

    //게임 상단 UI관련
    public GameObject totalEnemyNumText; //현재 웨이브의 총 적 유닛 수를 나타내는 텍스트 UI
    public GameObject aliveEnemyNumText; //현재 살아있는 적 유닛 수
    public GameObject WaveNowText; //현재 

    private int aliveEnemyNow;
    private int waveNow;

    // Start is called before the first frame update
    private void Awake()
    {
        allEnemy = new List<GameObject>();
        usingEnemy = new List<GameObject>();
        allMap = new List<GameObject>();
        allPath = new List<GameObject>();
        waveInfo = new List<Wave>();

        SetDefault();
    }
    public void SetDefault()
    {
        foreach(GameObject aliveEnemy in usingEnemy)
        {
            aliveEnemy.GetComponent<EnemyInfo>().SwitchWaveStatus(false);
            aliveEnemy.SetActive(false);
        }
        usingEnemy = new List<GameObject>();
        
        waveNow = 1;
        aliveEnemyNow = 0;
    }

    /*
     게임진행에 필요한 리소스(프리팹, 스테이지 데이터 등)를 불러오는 함수들
         */
    private void LoadEnemyPrefabs()
    { //게임 진행에 필요한 적 유닛 프리팹을 불러옴(게임 시작 전에)

        enemyPrefabs=new List<GameObject>(Resources.LoadAll<GameObject>("Prefabs/Enemy"));

        foreach(GameObject enemy in enemyPrefabs)
        {
            if (!enemy.name.Contains("Boss")) //보스오브젝트 이외의 일반 적 오브젝트
            {
                GameObject enemyGroup = GameObject.Find(enemy.name + "_Group");
                for (int i = 0; i < 150; i++)
                {
                    GameObject newEnemy = GameObject.Instantiate(enemy);
                    newEnemy.name = enemy.name + "_" + i;
                    newEnemy.transform.parent = enemyGroup.transform;
                    allEnemy.Add(newEnemy);
                    newEnemy.SetActive(false);
                }
            }
            else //보스오브젝트(1개씩만)
            {
                GameObject enemyGroup = GameObject.Find("Boss_Group");
                GameObject newEnemy = GameObject.Instantiate(enemy);
                newEnemy.name = enemy.name;
                newEnemy.transform.parent = enemyGroup.transform;
                allEnemy.Add(newEnemy);
                newEnemy.SetActive(false);
            }
        }
    }

    private void LoadMapNPaths()
    { //게임 진행에 필요한 적 유닛 프리팹을 불러옴(게임 시작 전에)
        GameObject[] mapPrefabs = Resources.LoadAll<GameObject>("Prefabs/Maps");

        GameObject mapGroup = GameObject.Find("MapGroup");
        foreach (GameObject map in mapPrefabs)
        {
            GameObject newMap = GameObject.Instantiate(map);
            newMap.name = map.name;
            newMap.transform.parent = mapGroup.transform;
            allMap.Add(newMap);
            newMap.SetActive(false);
        }

        GameObject[] pathPrefabs = Resources.LoadAll<GameObject>("Prefabs/PathInfo");
        GameObject pathGroup = GameObject.Find("PathGroup");

        foreach (GameObject path in pathPrefabs)
        {
            GameObject newPath = GameObject.Instantiate(path);
            newPath.name = path.name;
            newPath.transform.parent = pathGroup.transform;
            allPath.Add(newPath);
            newPath.SetActive(false);
        }
    }

    private void LoadWaveData()
    {
        string WaveDataTxt = Resources.Load<TextAsset>("GameData/Wave").text; //웨이브 정보를 담은 텍스트 파일을 지정하여 읽어온다.

        WaveInfoJson tempStageInfo = JsonUtility.FromJson<WaveInfoJson>(WaveDataTxt); //1줄씩 문자열을 WaveInfoJson클래스로 변환한다.
        
        foreach (WaveInfo wave in tempStageInfo.waves)
        {
            List<string> waveEnemyList = new List<string>(); //웨이브에 필요한 적 오브젝트의 정보를 담기위한 리스트

            int enemyLen = wave.unitName.Length;
            for (int j = 0; j < enemyLen; j++)
            {
                string enemyInfoCombined = wave.unitName[j] + '-' + wave.numOfUnit[j]; //enemy의 구성 적군이름(Enemy_OOO)-적유닛 갯수
                waveEnemyList.Add(enemyInfoCombined);
            }

            GameObject[] pathArr = Resources.LoadAll<GameObject>("Prefabs/PathInfo"); //적 오브젝트가 사용할 길에 대한 프리팹을 로드
            List<GameObject> pathGroup = new List<GameObject>(); //배열->리스트로 전환
            foreach (GameObject path in pathArr)
            {
                pathGroup.Add(path);
            }

            Wave newWave = new Wave();
            GameObject wavePath = pathGroup.Find(x => x.name.Contains(wave.pathInfo.ToString())); //길의 이름(뒤의 숫자로 분별)을 포함한 길 좌표 오브젝트
            newWave.SetWave(wave.waveNow, wave.waveMapName, wavePath, waveEnemyList); //읽은 적의 정보/길의 정보를 Wave클래스에 추가한다.

            waveInfo.Add(newWave); //새로운 wave정보를 추가
        }
    
    }

    public void LoadGameData() //gameManager에게 게임로드를 전달받아 필요한 리소스를 로드
    {
        LoadEnemyPrefabs();
        LoadMapNPaths();
        LoadWaveData();
        SetDefaultMap();
    }
    private void SetDefaultMap()
    {
        mapNow = allMap.Find(x => x.name.Contains("0"));
        pathNow = allPath.Find(x => x.name.Contains("0"));

        mapNow.SetActive(true);
        pathNow.SetActive(true);
    }

    public int GetWaveNow()
    {
        return waveNow;
    }

    /*
     게임 진행중에 스테이지(웨이브)에 맞는 적 오브젝트 소환
         */
    public void ReadyForWave(int n) //n번째 웨이브 준비
    {
        waveNow = n;

        Wave unitInfo = FindWave(n);

        if (unitInfo == null) { //더 이상 진행할 웨이브가 없는 경우 자체 생성
            CreateWave();

            unitInfo = FindWave(n); //새로 생성하였으므로 다시 불러온다.
        }
        if (unitInfo != null)
        {
            List<WaveUnitInfo> waveUnitInfo = unitInfo.GetWaveUnitInfo();
            
            //맵(블럭 분리상태)과 경로의 변경이 필요한 경우를 비교
            if (!mapNow.name.Contains(unitInfo.GetWaveMapName()))
            {//맵 이름과 다르므로 맵과 경로 변경 필요
                mapNow.SetActive(false);

                mapNow = allMap.Find(x => x.name.Contains(unitInfo.GetWaveMapName()));

                appManager.RegisterEmptyArea(mapNow); //맵의 변경에 따른 appManager의 block과 emptyArea/usedArea의 정보를 변경해줘야한다.

                mapNow.SetActive(true);
            }
            if (!pathNow.name.Contains(unitInfo.GetWavePathInfo().name))
            {
                pathNow.SetActive(false);

                pathNow = allPath.Find(x => x.name.Contains(unitInfo.GetWavePathInfo().name));
                pathNow.SetActive(true);
            }


            //여기부터는 적 유닛 포지셔닝에 관한 부분
            foreach (WaveUnitInfo tempUnit in waveUnitInfo)
            {
                string unitName=tempUnit.GetUnitName();
                int unitNum = tempUnit.GetUnitNum();

                aliveEnemyNow += unitNum; //각 적 유닛의 갯수를 더해 해당 웨이브의 총 적 유닛 수를 저장
                
                int enemyPerPath=0;
                if (pathNow.transform.childCount > 0)
                { //단일 path의 경우 자식이 없으므로
                    enemyPerPath = unitNum / pathNow.transform.childCount;
                }
                int pathCount = 0; //현재 한 path에 얼마나 적이 들어갔는가를 세는 변수. enemyPerPath만큼이 되면 초기화
                int pathIdx = 0; //현재 지정한 path. pathCount가 초기화될때 +1이 된다.

                for (int i=0; i<unitNum; i++)
                {
                    //찾으려는 유닛(이름으로 구분)이며 현재 사용중이지 않은 유닛 한개를 선택
                    GameObject unit = allEnemy.Find(x => x.name.Contains(unitName) && (x.activeSelf == false));
                    unit.SetActive(true);
                    usingEnemy.Add(unit);

                    if (pathNow.transform.childCount != 0)
                    { //각 웨이브의 경로가 1개인 경우는 자녀가 없고, 2개 이상인 경우 자식이 각각의 경로를 가지고 있으므로
                      //자식이 0이 아닌경우에는 여러갈래의 경로를 나누어 배분해야한다.
                        GameObject pathChild = pathNow.transform.GetChild(pathIdx).gameObject; // i/child로 할 시 유닛이 2개가 있으면 둘다 0, 0.5여서 0으로 배정이 된다. 따라서 확실히 나누기 위해 i에 1을 더함
                        if (++pathCount == enemyPerPath&&(pathIdx<pathNow.transform.childCount-1))
                        { //path 1개당 허용된 적 유닛의 배분이 끝났을 경우 다음 path로 변경(단, pathNow의 마지막 자식 path까지 온 상황이면 그냥 나머지 적 유닛을 마지막 자식 path에 추가한다.->인덱스 오류 제거)
                            ++pathIdx;
                            pathCount = 0; //pathCount 초기화
                        }
                        unit.GetComponent<EnemyInfo>().SetEnemyInformation(unitInfo.GetWaveNum(),pathChild); //현재 체력은 웨이브에 비례해서 증가
                    }
                    else
                    {
                        unit.GetComponent<EnemyInfo>().SetEnemyInformation(unitInfo.GetWaveNum(),pathNow);
                    }
                    unit.GetComponent<EnemyInfo>().SetAtStartLine();
                }
            }

            WaveNowText.GetComponent<Text>().text = unitInfo.GetWaveNum().ToString();
            totalEnemyNumText.GetComponent<Text>().text = aliveEnemyNow.ToString(); //유닛의 수를 전부 센 이후에야 수정이 가능
            aliveEnemyNumText.GetComponent<Text>().text = aliveEnemyNow.ToString(); //웨이브의 처음에는 총 적의 수와 생존 수가 같다
        }
    }

    private Wave FindWave(int wave) {
        foreach(Wave tempWave in waveInfo)
        {
            if (tempWave.GetWaveNum() == wave)
            {
                return tempWave;
            }
        }
        return null; //찾아도 없는 경우는 뭔가 잘못된 케이스
    }

    public GameObject GetEnemyPosition()
    { //block 오브젝트에서 사격할 대상을 달라고 요청할때 사용하는 함수. 생존한 적들 중에서 랜덤하게 선정
        GameObject selected = usingEnemy[Random.Range(0, usingEnemy.Count)];

        if (selected != null)
        { //적 유닛이 없을 경우 range오류가 난다.
            return selected;
        }
        else
        {
            return null;
        }
    }

    //적 유닛 targetObj의 위치에서 range거리 이내에 존재하는 모든 적 유닛을 반환한다.(Bullet에서 스플래시 데미지 대상을 정하기 위한 함수)
    public List<GameObject> GetEnemyInRange(GameObject targetObj, float range)
    {
        if (appManager.isWaveProcessing) //빨간 유닛의 확산피해(스플래시 데미지)가 웨이브 종료 후 다음 웨이브때 영향을 미치는 경우가 생겨서 웨이브 진행중이 아니면 막음
        {
            List<GameObject> enemyInRange = usingEnemy.FindAll(x => Vector3.Distance(x.transform.position, targetObj.transform.position) <= range); //range거리 이내에 살아있는 적 유닛리스트 뽑아냄

            return enemyInRange;
        }
        return null;
    }

    public void WaveStart()
    {
        if (!appManager.isWaveProcessing) //중복클릭에 반응하지 않도록
        {
            Debug.Log("ws "+aliveEnemyNow);
            appManager.WaveStart();

            StartCoroutine("StartEnemyMove");
        }
    }
    
    /*
     *일정 시간을 두고 적 오브젝트를 출발시킨다
     */
    private IEnumerator StartEnemyMove()
    {
        List<Transform> tempPath = new List<Transform>();
        if (pathNow.transform.childCount > 0) //길이 여러개인 경우 각각의 길에서 따로 적을 동시에 1개씩 출발시키기 위함
        {
            foreach(Transform childPath in pathNow.transform)
            {
                tempPath.Add(childPath);
            }
        }
        else
        {
            tempPath.Add(pathNow.transform);
        }
        while (true)
        {
            GameObject block=null;
            foreach (Transform path in tempPath)
            {
                block = allEnemy.Find(x => x.activeSelf == true && x.GetComponent<EnemyInfo>().isWaveStart == false && x.GetComponent<EnemyInfo>().pathName == path.name); //현재 준비는 되었으나 아직 이동이 시작되지 않은 오브젝트 선택

                if (block == null) //더이상 출발시킬 블럭이 없는 경우 탈출(아래에서 에러가 남)
                {
                    break;
                }
                block.GetComponent<EnemyInfo>().SwitchWaveStatus(true);
            }

            if (block == null) //더이상 블록이 없을경우 코루틴 종료
            {
                break;
            }
            yield return new WaitForSeconds(0.4f);
        }
    }


    public void EnemyDead(GameObject deadEnemy)
    { //잔여 적 갯수 갱신을 위한 함수
        usingEnemy.Remove(deadEnemy);
        --aliveEnemyNow;
        aliveEnemyNumText.GetComponent<Text>().text = aliveEnemyNow.ToString();
        appManager.IncreaseMoney();

        appManager.CheckBlockTarget(deadEnemy);

        Debug.Log(aliveEnemyNow);
        if (aliveEnemyNow < 1)
        {
            appManager.WaveEnd(waveNow);
        }
    }

    //최종 웨이브 도달 이후 무한으로 웨이브 제작
    private void CreateWave()
    {
        int waveNum; //생성할 웨이브 번호
        List<string> waveEnemyList; //해당 웨이브에 사용할 적 유닛의 명칭과 갯수
        Wave randomWave = waveInfo[Random.Range(0, waveInfo.Count)]; //랜덤하게 전체 웨이브 중 1개 선택(맵과 길을 한번에 정하기 위함)

        for(int i=0; i<14; i++)
        {
            Wave newWave; //웨이브 추가를 위한 Wave 객체
            waveNum =waveInfo.Count+i+1; //현재 만드는 웨이브의 위치(현재 마지막 스테이지 다음부터 진행하기 때문에 1을 더해준다.)
            int totalUnitNum = waveNum; //현재 웨이브에서 만들 수 있는 적 유닛의 총 수
            float[] unitPercent = { 0.34f, 0.33f, 0.33f }; //각 유닛 별 생성 점유율(순서대로 삼각/사각/원)

            newWave = new Wave();

            waveEnemyList = new List<string>();

            float adjustPercent = Mathf.Clamp(Random.Range(0, (waveNum-100) * 0.003f),0,0.33f); //각 유닛 별 생성 점유율을 조절하기 위한 확률. 스테이지가 올라갈 수록 삼각형 유닛이 늘어난다.(삼각은 증가, 사각은 유지, 원은 감소)
            unitPercent[0] += adjustPercent;
            unitPercent[2] -= adjustPercent;

            foreach(GameObject enemyPref in enemyPrefabs)
            {
                if (enemyPref.name.Contains("Boss")) { continue; } //보스는 일반 웨이브에서는 추가되지 않으므로 제외

                string waveEnemyNum = enemyPref.name.Split('(')[0] + "-"; //프리팹이기 때문에 이름 뒤에 (UnityEngine.GameObject)가 추가되므로 그걸 없애준다.
                if (waveEnemyNum.Contains("Triangle"))
                {
                    waveEnemyNum += Mathf.Ceil(waveNum * unitPercent[0]);
                }
                else if (waveEnemyNum.Contains("Rect"))
                {
                    waveEnemyNum += Mathf.Ceil(waveNum * unitPercent[1]);
                }
                else if (waveEnemyNum.Contains("Round"))
                {
                    waveEnemyNum += Mathf.Ceil(waveNum * unitPercent[2]);
                }
                waveEnemyList.Add(waveEnemyNum); //적 유닛별 생성 갯수 추가
            }

            newWave.SetWave(waveNum, randomWave.GetWaveMapName(), randomWave.GetWavePathInfo(),waveEnemyList); //새로운 웨이브 추가
            waveInfo.Add(newWave);
        }

        //보스 웨이브 추가
        waveNum = waveInfo.Count+1; //현재 만드는 웨이브의 위치
        List<GameObject> bossPrefs = enemyPrefabs.FindAll(x => x.name.Contains("Boss"));
        string bossType = bossPrefs[Random.Range(0, bossPrefs.Count)].name+"-1"; //보스 관련 오브젝트 중에 1개의 이름을 추가(-1은 1개를 의미)

        Wave newBossWave = new Wave();
        waveEnemyList = new List<string>();
        waveEnemyList.Add(bossType);
        newBossWave.SetWave(waveNum, randomWave.GetWaveMapName(), randomWave.GetWavePathInfo(), waveEnemyList); //보스 웨이브 추가
        waveInfo.Add(newBossWave);
    }

    public void GameOver()
    { //적 유닛이 종착점에 도착하여 게임이 아예 종료됨
        List<GameObject> usingEnemy = allEnemy.FindAll(x => x.activeSelf == true); //현재 웨이브에 사용하기 위해 active를 켜둔 상태인 적 유닛들에게 웨이브 시작을 알림

        foreach (GameObject enemy in usingEnemy)
        {
            enemy.GetComponent<EnemyInfo>().SwitchWaveStatus(false);
            enemy.SetActive(false);
        }

        appManager.GameOver();
    }
    
}
