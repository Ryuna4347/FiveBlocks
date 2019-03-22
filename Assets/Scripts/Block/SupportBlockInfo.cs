using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *지원형 블럭 오브젝트에 관한 기능
 */
public class SupportBlockInfo : MonoBehaviour
{
    private float enhanceRatio; //증가시키는 데미지 배수
    private AppManager appManager;
    private List<GameObject> nearBlocks; //상하좌우에 존재하는 블럭들

    private void Awake()
    {
        appManager = GameObject.Find("gameManager").GetComponent<AppManager>();
        nearBlocks = new List<GameObject>();
    }

    private void OnDisable()
    {
        ResetEnhanceBlocks(); //만약 블럭이 사라진 경우 주변 강화도 다시 없애야함
        nearBlocks = new List<GameObject>();
    }


    /// <summary>
    /// 지원 블럭이 먼저 설치 된 이후 근방에 블럭 한개가 설치될 경우 그 하나만 강화 버프처리
    /// </summary>
    /// <param name="nearBlock">지원 블럭 주위에 설치되는 블럭 오브젝트</param>
    public void EnhanceNearBlock(GameObject nearBlock)
    { 
        nearBlock.GetComponent<BlockInfo>().EnhancedBySupport(enhanceRatio);
        nearBlocks.Add(nearBlock);
    }

    /// <summary>
    /// 지원 블럭의 설치시 주변의 블럭에 강화 버프를 제공
    /// </summary>
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
