﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    public List<GameObject> emptyArea; //현재 비어있는 공간
    public List<GameObject> usedArea; //현재 비어있는 공간
    public List<GameObject> waitBlocks; //대기 유닛들
    public List<GameObject> usedBlocks; //대기 유닛들

    public List<GameObject> EnemyList;

    public GameObject blocksParent; //블럭유닛을 모아둘 상위 빈 오브젝트
    public SoundManager audio; //게임진행 시 나올 소리를 위한 오디오매니저
    public WaveManager waveManager;

    // Start is called before the first frame update
    void Start()
    {
        usedArea = new List<GameObject>();
        usedBlocks = new List<GameObject>();

        LoadBlockData();
        waveManager.LoadGameData();
        waveManager.ReadyForWave(1); //바로 시작할것이므로 1탄 준비
        RegisterEmptyArea(); //맵을 프리팹으로 만듦에 따라 에디터에서 등록해서 사용하는 방법을 쓸수가 없다.
                            //따라서 맨 처음 맵이 불려진 이후 EmptyArea를 저장하고 맵이 변경되면 EmptyArea, usedArea를 복사해서 옮겨줘야한다.

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void MakeBlock()
    {
        if (usedBlocks.Count==36)
        { //36칸이 다 차있다는 뜻이므로 블럭생성 불가
            return;
        }
        int typeRandom = Random.Range(0, 2); //현재는 2가지 밖에 없어서. 차후에 5로 늘릴것
        string BlockTypeString = "Block_";
        switch (typeRandom)
        {
            case 0:
                BlockTypeString = "Blue" + BlockTypeString;
                break;
            case 1:
                BlockTypeString = "Green" + BlockTypeString;
                break;
            case 2:
                BlockTypeString = "Red" + BlockTypeString;
                break;
            case 3:
                BlockTypeString = "White" + BlockTypeString;
                break;
            case 4:
                BlockTypeString = "Grey" + BlockTypeString;
                break;
            default:
                break;
        }

        GameObject properBlockObj = waitBlocks.Find(x => x.name.Contains(BlockTypeString) && x.activeSelf == false); //현재 active가 꺼져있고 색상이 맞는 유닛을 불러온다.
        properBlockObj.SetActive(true);

        int randomPos = Random.Range(0, emptyArea.Count); //빈 곳 중에 랜덤하게 1곳
        Vector3 blockPos = emptyArea[randomPos].transform.position;
        blockPos.z = -1; //블럭이 제일 위에 보이도록 쌓아야해서 z축을 고정
        properBlockObj.transform.position = blockPos;

        audio.PlayAudio("CreateBlock"); //블럭 생성에 대한 사운드 재생
        
        waitBlocks.Remove(properBlockObj);
        usedBlocks.Add(properBlockObj);
        usedArea.Add(emptyArea[randomPos]);
        emptyArea.RemoveAt(randomPos);

        if (usedArea.Count > 5)
        { //테스트용
            waveManager.ReadyForWave(2);
        }
    }
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
                block.name =obj.name+i;
                block.transform.parent = temp.transform;

                block.SetActive(true);
                waitBlocks.Add(block);
                block.SetActive(false);
            }
        }
    }

    public void MoveUsedToEmpty(GameObject movingBlock) //옮길 블럭의 원래 위치에 있는 EmptyArea를 반환해준다.()
    {
        GameObject willBeEmpty = usedArea.Find(x => (x.transform.position.x == movingBlock.transform.position.x)&&(x.transform.position.y==movingBlock.transform.position.y));
        //z는 Block과 EmptyArea가 다르므로 z를 빼고 비교를 해야 된다.

        usedArea.Remove(willBeEmpty);
        emptyArea.Add(willBeEmpty);
        movingBlock.transform.position = new Vector3(-5, 0, 0);
        movingBlock.SetActive(false);
    }

    public void BlockLevelUp(GameObject obj_1, GameObject obj_2) //obj_2 위치에 다음 레벨을 생성
    { //레벨업에 필요한 과정

        string blockType = obj_2.GetComponent<BlockInfo>().blockName.ToUpper(); //종류 비교를 위해 블럭정보 스크립트에서 이름을 획득
        
        if (obj_1.name.ToUpper().Contains(blockType)) //동일한 종류(혹시 소문자/대문자 착각이 있을 수 있으므로 대문자로 변형해서 확인)
        {
            if (obj_1.GetComponent<BlockInfo>().blockLevel == obj_2.GetComponent<BlockInfo>().blockLevel)
            {//동일레벨, 동일종류일 시 레벨업

                int typeRandom = Random.Range(0, 2);  //현재는 2가지 밖에 없어서. 차후에 5로 늘릴것
                string BlockTypeString = "Block_";
                switch (typeRandom)
                {
                    case 0:
                        BlockTypeString = "Blue" + BlockTypeString;
                        break;
                    case 1:
                        BlockTypeString = "Green" + BlockTypeString;
                        break;
                    case 2:
                        BlockTypeString = "Red" + BlockTypeString;
                        break;
                    case 3:
                        BlockTypeString = "White" + BlockTypeString;
                        break;
                    case 4:
                        BlockTypeString = "Grey" + BlockTypeString;
                        break;
                    default:
                        break;
                }

                GameObject properBlockObj = waitBlocks.Find(x => x.name.Contains(BlockTypeString) && x.activeSelf == false); //현재 active가 꺼져있고 색상이 맞는 유닛을 불러온다.
                
                MoveUsedToEmpty(obj_1); //obj_1->obj_2로 이동이기 때문에 obj_1의 자리는 비워준다.
                obj_2.GetComponent<BlockInfo>().Refresh(); //obj_2의 경우는 empty가 되는건 아니라서 그냥 obj_2만 초기화시키고 지운다.
                properBlockObj.SetActive(true); //새로 들어올 블럭은 obj_2의 위치에 들어가야하기 때문에 obj_2를 없애기 전에 위치를 전달받고 옮겨놓기
                properBlockObj.transform.position = obj_2.transform.position;
                audio.PlayAudio("Synthesize");

                obj_2.transform.position = new Vector3(-5, 0, 0); 
                obj_2.SetActive(false);


            }
        }

    }

    private void RegisterEmptyArea()
    { //인자가 없는 함수는 게임 시작시에 appManager에서 자체적으로 실행
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
    public void RegisterEmptyArea(GameObject newMap)
    {//게임 시작 직후가 아닌 게임 진행 중 맵의 변경으로 옮길 시 사용하는 함수(WaveManager에서 요청)
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
                GameObject block = usedBlocks.Find(x => (x.transform.position.x == existedUsed.transform.position.x)&&(x.transform.position.y == existedUsed.transform.position.y));
                Vector3 newBlockPos = newMapUsedAreaObj.transform.position;
                newBlockPos.z = -1; //z축이 area와 같으면 겹치니까 z는 따로 빼준다.
                block.transform.position = newBlockPos;
            }
        }
        usedArea = newUsedArea;
        
    }

    public void EnemyDead(GameObject enemyUnit)
    {
        if (EnemyList.Contains(enemyUnit)) {
            EnemyList.Remove(enemyUnit);
        }
    }
}
