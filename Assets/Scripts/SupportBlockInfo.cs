using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportBlockInfo : MonoBehaviour
{
    private float enhanceRatio; //증가시키는 데미지 배수
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
        nearBlock.GetComponent<BlockInfo>().EnhancedBySupport(enhanceRatio);
    }
    public void EnhanceNearBlocks()
    {
        nearBlocks = appManager.GetNearBlocks(transform);
        foreach(GameObject block in nearBlocks)
        {
            block.GetComponent<BlockInfo>().EnhancedBySupport(enhanceRatio);
        }
    }

    private void ResetEnhanceBlocks()
    {
        foreach (GameObject block in nearBlocks)
        {
            block.GetComponent<BlockInfo>().ResetEnhance();
        }
    }

    public void SetEnhanceRatio(float enhance)
    {
        enhanceRatio = enhance;
        EnhanceNearBlocks(); //강화 배율이 갱신되었으므로 주변 블럭들에게 효과를 갱신해주어야 한다.
    }
}
