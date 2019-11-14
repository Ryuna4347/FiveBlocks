using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 *유닛 강화 관련 버튼의 동작에 대한 스크립트
 */
public class EnchantButtonClicked : MonoBehaviour
{
    public TouchBlockUI touchBlockUI; //blockEnchantUI 오브젝트 상위의 터치방지 이미지
    public BlockEnchantUI blockEnchantUI;

    public void ActiveBlockEnchantInfo(string blockName)
    {
        touchBlockUI.ActiveUI("BlockEnchant");
        blockEnchantUI.SetBlockInfo(blockName); //BlockEnchantInformation 내에 blockName에 해당하는 내용을 보여주도록 설정
    }

    //Inactive 함수는 블럭 강화정보 페이지의 취소버튼에서 호출
    public void InactiveBlockEnchantInfo()
    {
        blockEnchantUI.ResetBlockInfo(); //BlockEnchantInformation 내에 blockName에 해당하는 내용을 보여주도록 설정
        touchBlockUI.UnactiveUI("BlockEnchant");
    }
}
