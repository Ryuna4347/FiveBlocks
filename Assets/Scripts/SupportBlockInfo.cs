using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportBlockInfo : MonoBehaviour
{
    private AppManager appManager;
    private List<GameObject> nearBlocks;

    private void Awake()
    {
        appManager = GameObject.Find("gameManager").GetComponent<AppManager>();
        nearBlocks = new List<GameObject>();
    }

    private void OnDisable()
    {
        ResetEnhanceBlocks(); //만약 블럭이 사라진 경우 주변 강화도 다시 없애야함
    }

    public void EnhanceNearBlock(GameObject nearBlock)
    { //노란 블럭이 먼저 설치 된 이후 근방에 블럭 한개가 설치될 경우 그 하나만 버프처리
        nearBlock.GetComponent<BlockInfo>().EnhancedBySupport(1.5f);
    }
    public void EnhanceNearBlocks()
    {
        nearBlocks = appManager.GetNearBlocks(transform);
        foreach(GameObject block in nearBlocks)
        {
            block.GetComponent<BlockInfo>().EnhancedBySupport(1.5f);
        }
    }

    private void ResetEnhanceBlocks()
    {
        foreach (GameObject block in nearBlocks)
        {
            block.GetComponent<BlockInfo>().ResetEnhance();
        }
    }
}
