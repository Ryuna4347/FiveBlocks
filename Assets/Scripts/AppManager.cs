using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    public List<GameObject> emptyArea; //현재 비어있는 공간
    public List<GameObject> usedArea; //현재 비어있는 공간
    public List<GameObject> blocks; //유닛들

    // Start is called before the first frame update
    void Start()
    {
        usedArea = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void MakeBlock()
    {
        int random = Random.Range(0, emptyArea.Count);

        GameObject usingBlock = blocks.Find(x => x.gameObject.activeSelf == false); //차후에 블럭 종류에 대해서도 조건을 달아야한다. 현재는 1가지만 있으므로 그냥 꺼져있는 거 가져옴
        usingBlock.SetActive(true);
        usingBlock.transform.position = emptyArea[random].transform.position;

        usedArea.Add(emptyArea[random]);
        emptyArea.RemoveAt(random);
    }
}
