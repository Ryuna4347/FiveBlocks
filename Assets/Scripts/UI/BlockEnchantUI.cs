using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; //json을 읽기 위함
using UnityEngine.UI;

public class BlockDescJSON{
    public string blockName;
    public string Description;
    public string SpecialEffectName;
}


/*
 *블럭의 강화정보 및 설명을 위한 페이지를 제어하는 스크립트
 *직접 블럭의 정보에 개입하는 것이 아닌 단순 표시만을 하며
 *블럭의 레벨업 및 정보 관리는 EnchantManager에 넘긴다.
 */
public class BlockEnchantUI : MonoBehaviour
{
    private EnchantManager enchantManager;

    //블럭유닛들의 정보를 미리 담아두는 변수들
    private List<BlockDescJSON> blockDescList;
    public List<Sprite> blockImageList;

    //블럭유닛의 정보를 작성할 위치
    public Text blockDescriptionText;
    public Text blockNameText;
    public Image blockImage;

    //블럭유닛의 강화정보(현재 공격력/특수효과와 다음 레벨정보)를 작성할 위치
    public Text dmgNowText; 
    public Text dmgNextText; //다음 레벨
    public Text SEType; //특수효과 종류, SE=Special Effect
    public Text SENowText;
    public Text SENextText;

    private void Awake()
    {
        enchantManager = GameObject.Find("EnchantManager").GetComponent<EnchantManager>();
        blockDescList = new List<BlockDescJSON>();
        LoadBlockDescription();
        transform.parent.gameObject.SetActive(false); //UI위에 외곽 터치 방지 오브젝트까지 꺼줘야한다.
    }

    private void LoadBlockDescription()
    {
        string[] blockDescText = File.ReadAllText("Assets/GameData/BlockDescription.txt").Split('\n');

        int fileLen = blockDescText.Length;
        Debug.Log(fileLen);

        if (fileLen < 1)
        {  //저장된 정보가 있어야 불러옴
            return;
        }
        for (int i = 0; i < fileLen; i++)
        {
            BlockDescJSON blockDesc = JsonUtility.FromJson<BlockDescJSON>(blockDescText[i]);
            blockDescList.Add(blockDesc);
        }
    }

    public void SetBlockInfo(string blockName) {
        BlockDescJSON blockDesc = blockDescList.Find(x => blockName.Contains(x.blockName));
        blockImage.sprite = blockImageList.Find(x => x.name.Contains(blockName));
        blockNameText.text = blockDesc.blockName;

        blockDescriptionText.text=blockDesc.Description;


        //공격력 관련부분
        dmgNowText.text = enchantManager.GetAttackDamage(blockName).ToString();
        int dmgNext = enchantManager.GetNextAttackDamage(blockName);
        if (dmgNext < 0)
        { //만약 다음 강화레벨이 없다면 빈칸 취급
            dmgNextText.text = "-";
        }
        else
        {
            dmgNextText.text = dmgNext.ToString();
        }


        //특수효과 부분. 공격력에 비해 구분할 점이 있어서 내용이 길다.
        string type = blockDesc.SpecialEffectName;
        if (type == "")
        {
            SEType.text = "X";
        }
        else
        {
            SEType.text = type;
        }


        float SENow= enchantManager.GetSpecialEffect(blockName);
        if (blockName == "PurpleBlock")
        {//보라색 블럭의 경우 특수능력이 존재하지 않음
            SENowText.text = "-";
        }
        else
        {
            string tempText = "";
            if (blockName == "BlueBlock" || blockName == "GreyBlock")
            {
                if (blockName == "BlueBlock")
                {
                    SENow *= 100;
                }
                tempText = SENow.ToString(); //소수인 파란 블럭때문에 우선 예외처리 후 문자열로 변경
                tempText += "%";
            }
            else if(blockName == "YellowBlock")
            {
                tempText = SENow.ToString();
                tempText += "x";
            }
            else //붉은 블럭의 경우 위의 어느것도 해당되지 않음
            {
                tempText = SENow.ToString();
            }

            SENowText.text = tempText; //이해 쉽게 정리된 문자열을 출력
        }


        float SENext = enchantManager.GetNextSpecialEffect(blockName);

        if (blockName=="PurpleBlock")
        { //보라색 블럭의 경우 특수능력이 존재하지 않음
            SENextText.text = "-";
        }
        else
        {
            string tempText="";
            if (blockName == "BlueBlock" || blockName == "GreyBlock")
            {
                if (blockName == "BlueBlock")
                {
                    SENext *= 100;
                }
                tempText = SENext.ToString(); //소수인 파란 블럭때문에 우선 예외처리 후 문자열로 변경
                tempText += "%";
            }
            else if (blockName == "YellowBlock")
            {
                tempText = SENext.ToString();
                tempText += "x";
            }
            else //붉은 블럭의 경우 위의 어느것도 해당되지 않음
            {
                tempText = SENext.ToString();
            }

            SENextText.text = tempText;
        }
    }

    //UI안의 블록관련 내용을 전부 삭제
    public void ResetBlockInfo()
    {
        blockImage.sprite = null;
        blockNameText.text = "";
        blockDescriptionText.text = "";

        dmgNowText.text = "";
        dmgNextText.text = "";

        SENowText.text = "";
        SENextText.text = "";


    }
}
