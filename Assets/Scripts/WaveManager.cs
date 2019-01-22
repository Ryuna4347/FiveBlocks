using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class XmlWaveInfo
{//xml파일을 읽어서 defunit에 넣기 전에 중간과정(유니티의 xml 파서가 기본형밖에 지원 안 해주기 때문)
    //스테이지별 모든 유닛의 정보가 담겨있다.
    public int waveNow; //현재 웨이브
    public string waveMapName; //웨이브가 진행 될 맵이름(맵이름이 다를 경우 gameManager에 신 이동 요구)
    public string pathInfo;

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
    private List<Wave> waveInfo;
    private List<GameObject> allMap;
    private List<GameObject> allPath;

    public AppManager appManager;

    private GameObject mapNow; //현재 맵과 경로(다음 웨이브와 비교하여 변경 필요여부 조사)
    private GameObject pathNow;

    private int aliveEnemyNow;
    private int waveNow;

    // Start is called before the first frame update
    private void Awake()
    {
        allEnemy = new List<GameObject>();
        allMap = new List<GameObject>();
        allPath = new List<GameObject>();

        waveInfo = new List<Wave>();
    }

    /*
     게임진행에 필요한 리소스(프리팹, 스테이지 데이터 등)를 불러오는 함수들
         */
    private void LoadEnemyPrefabs()
    { //게임 진행에 필요한 적 유닛 프리팹을 불러옴(게임 시작 전에)
        GameObject[] enemyPrefabs=Resources.LoadAll<GameObject>("Prefabs/Enemy");

        foreach(GameObject enemy in enemyPrefabs)
        {
            GameObject enemyGroup = GameObject.Find(enemy.name+"_Group");
            for (int i = 0; i < 150; i++) {
                GameObject newEnemy = GameObject.Instantiate(enemy);
                newEnemy.name = enemy.name +"_"+ i;
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
        string[] WaveDataTxt = File.ReadAllText("Assets/GameData/Wave.txt").Split('\n');

        int fileLen = WaveDataTxt.Length;

        if (fileLen < 1)
        {  //저장된 정보가 있어야 불러옴
            return;
        }
        for (int i = 0; i < fileLen; i++)
        {
            if (WaveDataTxt[i] == "")
            { //빈칸이었을 경우 제외(2중엔터시 나올 수 있음)
                continue;
            }

            XmlWaveInfo tempStageInfo = JsonUtility.FromJson<XmlWaveInfo>(WaveDataTxt[i]);
            
            List<string> waveEnemyList = new List<string>();

            int enemyLen = tempStageInfo.unitName.Length;
            for(int j=0; j<enemyLen; j++)
            {
                string enemyInfoCombined = tempStageInfo.unitName[j] +'-'+ tempStageInfo.numOfUnit[j]; //enemy의 구성 적군이름(Enemy_OOO)-적유닛 갯수
                waveEnemyList.Add(enemyInfoCombined);
            }

            GameObject[] pathArr = Resources.LoadAll<GameObject>("Prefabs/PathInfo");
            List<GameObject> pathGroup = new List<GameObject>();//find함수를 찾기 위해서 리스트로 변환()
            foreach (GameObject path in pathArr) {
                pathGroup.Add(path);
            }

            Wave newWave = new Wave();
            GameObject wavePath = pathGroup.Find(x => x.name.Contains(tempStageInfo.pathInfo)); //길의 이름(뒤의 숫자로 분별)을 포함한 길 좌표 오브젝트
            newWave.SetWave(tempStageInfo.waveNow, tempStageInfo.waveMapName, wavePath, waveEnemyList);

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

    /*
     게임 진행중에 스테이지(웨이브)에 맞는 적 오브젝트 소환
         */
    public void ReadyForWave(int n) //n번째 웨이브 준비
    {
        waveNow = n;

        Wave unitInfo = FindWave(n);

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
                
                for (int i=0; i<unitNum; i++)
                {
                    //찾으려는 유닛(이름으로 구분)이며 현재 사용중이지 않은 유닛 한개를 선택
                    GameObject unit = allEnemy.Find(x => x.name.Contains(unitName) && (x.activeSelf == false));
                    unit.SetActive(true);

                    if (pathNow.transform.childCount != 0)
                    { //각 웨이브의 경로가 1개인 경우는 자녀가 없고, 2개 이상인 경우 자식이 각각의 경로를 가지고 있으므로
                      //자식이 0이 아닌경우에는 여러갈래의 경로를 나누어 배분해야한다.
                        GameObject pathChild = pathNow.transform.GetChild((int)((i+1)/pathNow.transform.childCount)).gameObject; // i/child로 할 시 유닛이 2개가 있으면 둘다 0, 0.5여서 0으로 배정이 된다. 따라서 확실히 나누기 위해 i에 1을 더함

                        unit.GetComponent<EnemyInfo>().SetPathInfomation(pathChild);
                    }
                    else
                    {
                        unit.GetComponent<EnemyInfo>().SetPathInfomation(pathNow);
                    }
                    unit.GetComponent<EnemyInfo>().SetAtStartLine();
                }
            }
            
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
        List<GameObject> aliveEnemy = allEnemy.FindAll(x => x.activeSelf == true);
        GameObject selected= aliveEnemy[Random.Range(0, aliveEnemy.Count)];

        if (selected != null)
        { //적 유닛이 없을 경우 range오류가 난다.
            return aliveEnemy[Random.Range(0, aliveEnemy.Count)];
        }
        else
        {
            return null;
        }
    }

    //적 유닛 targetObj의 위치에서 range거리 이내에 존재하는 모든 적 유닛을 반환한다.(Bullet에서 스플래시 데미지 대상을 정하기 위한 함수)
    public List<GameObject> GetEnemyInRange(GameObject targetObj, float range)
    {
        List<GameObject> enemyInRange = allEnemy.FindAll(x => x.activeSelf == true&& Vector3.Distance(x.transform.position,targetObj.transform.position)<=range); //range거리 이내에 살아있는 적 유닛리스트 뽑아냄
        
        return enemyInRange;
    }

    public void WaveStart()
    {
        List<GameObject> usingEnemy = allEnemy.FindAll(x=>x.activeSelf==true); //현재 웨이브에 사용하기 위해 active를 켜둔 상태인 적 유닛들에게 웨이브 시작을 알림

        foreach(GameObject enemy in usingEnemy)
        {
            enemy.GetComponent<EnemyInfo>().SwitchWaveStatus(true);
        }

        appManager.WaveStart();
    }

    public void EnemyDead(GameObject deadEnemy)
    { //AppManager에게 점수판정 요청 및 잔여 적 갯수 갱신, 웨이브 종료여부 판단
        deadEnemy.SetActive(false);
        //현재 남은 적의 수가 필요함. 따로 waveInfo 클래스변수를 만들어서 써야할거 같다.

        if (--aliveEnemyNow < 1)
        {
            appManager.WaveEnd(waveNow);

            List<GameObject> usingEnemy = allEnemy.FindAll(x => x.activeSelf == true); //현재 웨이브에 사용하기 위해 active를 켜둔 상태인 적 유닛들에게 웨이브 시작을 알림

            foreach (GameObject enemy in usingEnemy)
            {
                enemy.GetComponent<EnemyInfo>().SwitchWaveStatus(false);
            }
        }
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
