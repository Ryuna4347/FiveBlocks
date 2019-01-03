using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockInfo : MonoBehaviour
{
    public string blockName;
    public List<Sprite> blockImage;
    public int blockLevel;

    private void Awake()
    {
        gameObject.GetComponent<ButtonDrag>().previewObj=GameObject.Find("PreviewObj").gameObject;
        blockLevel = 1;
    }

    private void OnDisable()
    {
        if (blockLevel != 1)
        { //다른 곳에서 초기화를 하겠지만 안되어있을 경우 초기화
            Refresh();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Refresh()
    { //유닛 초기화
        blockLevel = 1;
        gameObject.GetComponent<SpriteRenderer>().sprite = blockImage[0];
    }

    public void SetBlockLevel(int lev)
    { //레벨업 시 바로 해당 레벨로 가야하므로 레벨을 설정하는 함수(AppManager에서 합성시 사용됨)
        blockLevel = lev;
        gameObject.GetComponent<SpriteRenderer>().sprite = blockImage[lev-1];
    }
}
