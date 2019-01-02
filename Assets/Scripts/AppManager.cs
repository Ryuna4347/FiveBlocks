using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    public List<GameObject> emptyArea; //현재 비어있는 공간
    public List<GameObject> usedArea; //현재 비어있는 공간
    public List<GameObject> waitBlocks; //대기 유닛들
    public List<GameObject> usedBlocks; //대기 유닛들

    public GameObject blocksParent; //블럭유닛을 모아둘 상위 빈 오브젝트

    // Start is called before the first frame update
    void Start()
    {
        usedArea = new List<GameObject>();
        usedBlocks = new List<GameObject>();

        LoadBlockData();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void MakeBlock()
    {
        if (usedBlocks.Count==25)
        { //25칸이 다 차있다는 뜻이므로 블럭생성 불가
            return;
        }

        int random = Random.Range(0, waitBlocks.Count); //대기 유닛 중에 랜덤하게 한개

        GameObject usingBlock = waitBlocks[random]; //차후에 블럭 종류에 대해서도 조건을 달아야한다. 현재는 1가지만 있으므로 그냥 꺼져있는 거 가져옴
        usingBlock.SetActive(true);


        int randomPos = Random.Range(0, emptyArea.Count); //빈 곳 중에 랜덤하게 1곳
        Vector3 blockPos = emptyArea[randomPos].transform.position;
        blockPos.z = 0;
        usingBlock.transform.position = blockPos;


        waitBlocks.Remove(usingBlock);
        usedBlocks.Add(usingBlock);
        usedArea.Add(emptyArea[randomPos]);
        emptyArea.RemoveAt(randomPos);
    }
    private void LoadBlockData()
    {
        GameObject[] blocks = Resources.LoadAll<GameObject>("Prefabs");
        
        foreach (GameObject obj in blocks)
        {
            GameObject temp = new GameObject();
            temp.name = obj.name; //유닛별로 모아두기 위한 빈 오브젝트
            temp.transform.parent = blocksParent.transform;

            for (int i = 0; i < 25; i++)
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
        GameObject willBeEmpty = usedArea.Find(x => x.transform.position == movingBlock.transform.position);

        usedArea.Remove(willBeEmpty);
        emptyArea.Add(willBeEmpty);
        movingBlock.transform.position = new Vector3(-5, 0, 0);
        movingBlock.SetActive(false);
    }
}
