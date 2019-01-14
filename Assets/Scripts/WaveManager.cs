﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class XmlWaveInfo
{//xml파일을 읽어서 defunit에 넣기 전에 중간과정(유니티의 xml 파서가 기본형밖에 지원 안 해주기 때문)
    //스테이지별 모든 유닛의 정보가 담겨있다.
    public int Wavenow; //현재 웨이브
    public string waveMapName; //웨이브가 진행 될 맵이름(맵이름이 다를 경우 gameManager에 신 이동 요구)
    public string path;

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
    private GameObject wavePath;
    private List<WaveUnitInfo> unitInfo;

    public void SetWave(int n, GameObject pathInfo, List<string> EnemyList)
    {
        waveNum = n;
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
    public int GetWaveNum()
    {
        return waveNum;
    }
}

public class WaveManager : MonoBehaviour
{
    private List<GameObject> allEnemy; //현재 로드 되어있는 모든 적 유닛
    private List<Wave> waveInfo;

    // Start is called before the first frame update
    private void Awake()
    {
        allEnemy = new List<GameObject>();
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
            }
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
            GameObject wavePath = pathGroup.Find(x => x.name.Contains(tempStageInfo.waveMapName)); //길의 이름(뒤의 숫자로 분별)을 포함한 길 좌표 오브젝트
            newWave.SetWave(tempStageInfo.Wavenow,wavePath, waveEnemyList);

            waveInfo.Add(newWave); //새로운 wave정보를 추가
        }
    }

    public void LoadGameData() //gameManager에게 게임로드를 전달받아 필요한 리소스를 로드
    {
        LoadEnemyPrefabs();
        LoadWaveData();
    }


    /*
     게임 진행중에 스테이지(웨이브)에 맞는 적 오브젝트 소환
         */
    public void ReadyForWave(int n) //n번째 웨이브 준비
    {
        Wave unitInfo = FindWave(n);

        if (unitInfo != null)
        {
            //길과 맵의 변동여부 판단필요


            //여기부터는 적 유닛 포지셔닝에 관한 부분
            List<WaveUnitInfo> waveUnitInfo = unitInfo.GetWaveUnitInfo();   

            foreach(WaveUnitInfo tempUnit in waveUnitInfo)
            {
                string unitName=tempUnit.GetUnitName();
                int unitNum = tempUnit.GetUnitNum();

                for(int i=0; i<unitNum; i++)
                {
                    //찾으려는 유닛(이름으로 구분)이며 현재 사용중이지 않은 유닛 한개를 선택
                    GameObject unit = allEnemy.Find(x => x.name.Contains(unitName) && (x.activeSelf == false));
                    unit.SetActive(true);
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
}
