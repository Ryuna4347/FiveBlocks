using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; //json을 읽기 위함
using UnityEngine.UI;

public class EnchantJson
{
    public string blockName;
    public int[] blockAttEnchant;
    public float[] blockSpecialEffect;
    public int[] blockRequiredMoney;
}


/*
 *블럭의 레벨 이외에 강화에 관련된 사항(레벨 : 단순 공격력 증가, 강화 : 공격력 및 특수능력 증대)
 * 레벨별 특수효과(
 *  보라-없음(0),
 *  빨강-스플래시 데미지,
 *  파랑-이동속도 감소율(%),
 *  노랑-공격력 증가율(%), 
 *  회색-효과 발동확률(%)
 * )
 */
public class EnchantInfo //블럭 이름
{
    public string enchantBlockName;
    private int enchantLev; //현재 블럭 강화 레벨
    private List<int> attEnchant;
    private List<float> specialEffect; //붉은 블럭의 경우 float->int로 변경하여 사용할 것
    public List<int> requiredMoney;

    public EnchantInfo()
    {
        enchantLev = 1;
        attEnchant = new List<int>();
        specialEffect = new List<float>();
        requiredMoney = new List<int>();
    }
    public void AddAttInfo(int att) {
        attEnchant.Add(att);
    }
    public int GetAttInfo() //현재 강화레벨의 공격력을 돌려줌
    {
        return attEnchant[enchantLev - 1];
    }
    public int GetNextAttInfo() //다음 강화레벨의 공격력을 돌려줌
    {
        if (enchantLev < attEnchant.Count) //다음 강화레벨이 존재한다면 반환
        {
            return attEnchant[enchantLev];
        }
        else
        {
            return -1;
        }
    }
    public void AddSpecialInfo(float value)
    {
        specialEffect.Add(value);
    }
    public float GetSpecialInfo() //현재 강화레벨의 공격력을 돌려줌
    {
        return specialEffect[enchantLev-1];
    }
    public float GetNextSpecialInfo() //현재 강화레벨의 공격력을 돌려줌
    {
        if (enchantLev < specialEffect.Count) //다음 강화레벨이 존재한다면 반환
        {
            return specialEffect[enchantLev];
        }
        else
        {
            return -100;
        }
    }
    public void AddRequiredMoney(int value)
    {
        requiredMoney.Add(value);
    }
    public int GetRequiredMoney() //현재 강화레벨의 공격력을 돌려줌
    {
        if (enchantLev < requiredMoney.Count) //다음 강화레벨이 존재한다면 반환
        {
            return requiredMoney[enchantLev-1];
        }
        else
        {
            return -1;
        }
    }

    public bool LevelUp()
    {
        if (enchantLev>=attEnchant.Count) //레벨업 가능 범위(attEnchant나 specialEffect의 수만큼만 가능)가 아니라면 레벨업 불가
        {
            return false;
        }
        ++enchantLev;
        return true;
    }
}

public class EnchantManager : MonoBehaviour
{
    List<EnchantInfo> allEnchant;
    private AppManager appManager;
    public List<GameObject> enchantBtnList;

    private void Awake()
    {
        allEnchant = new List<EnchantInfo>();
        appManager = GameObject.Find("gameManager").GetComponent<AppManager>();
        LoadEnchantData();

        foreach (EnchantInfo enchant in allEnchant)
        {
            GameObject enchantBtn = enchantBtnList.Find(x => x.name.Contains(enchant.enchantBlockName));
            enchantBtn.transform.Find("Att").gameObject.GetComponent<Text>().text = enchant.GetAttInfo().ToString();
            if (enchantBtn.transform.Find("SpecialEffect") != null)
            {
                if (enchantBtn.name.Contains("BlueBlock"))
                {
                    enchantBtn.transform.Find("SpecialEffect").gameObject.GetComponent<Text>().text = (enchant.GetSpecialInfo() * 100).ToString();
                }
                else
                {
                    enchantBtn.transform.Find("SpecialEffect").gameObject.GetComponent<Text>().text = enchant.GetSpecialInfo().ToString();
                }
            }
        }
    }

    private void LoadEnchantData()
    {
        string[] EnchantDataTxt = File.ReadAllText("Assets/GameData/Enchant.txt").Split('\n');

        int fileLen = EnchantDataTxt.Length;

        if (fileLen < 1)
        {  //저장된 정보가 있어야 불러옴
            return;
        }
        for (int i = 0; i < fileLen; i++)
        {
            if (EnchantDataTxt[i] == "")
            { //빈칸이었을 경우 제외(2중엔터시 나올 수 있음)
                continue;
            }

            EnchantJson tempEnchatInfo = JsonUtility.FromJson<EnchantJson>(EnchantDataTxt[i]);

            EnchantInfo enchant = new EnchantInfo();

            enchant.enchantBlockName = tempEnchatInfo.blockName;
            foreach (int value in tempEnchatInfo.blockAttEnchant) //강화로 인한 공격력 정보 추가
            {
                enchant.AddAttInfo(value);
            }
            foreach (float value in tempEnchatInfo.blockSpecialEffect) //강화로 인한 특수능력 정보 추가
            {
                enchant.AddSpecialInfo(value);
            }
            foreach (int value in tempEnchatInfo.blockRequiredMoney) //강화로 인한 특수능력 정보 추가
            {
                enchant.AddRequiredMoney(value);
            }


            allEnchant.Add(enchant);
        }
    }

    public int GetAttackDamage(string blockName) {
        EnchantInfo blockEnchant = allEnchant.Find(x => x.enchantBlockName == blockName);
        return blockEnchant.GetAttInfo(); //해당 블록의 공격력 강화수치 반환
    }
    public int GetNextAttackDamage(string blockName)
    {
        EnchantInfo blockEnchant = allEnchant.Find(x => x.enchantBlockName == blockName);
        return blockEnchant.GetNextAttInfo(); //해당 블록의 공격력 강화수치 반환
    }
    public float GetSpecialEffect(string blockName)
    {
        EnchantInfo blockEnchant = allEnchant.Find(x => x.enchantBlockName == blockName);
        return blockEnchant.GetSpecialInfo(); //해당 블록의 특수능력 강화 수치 반환
    }
    public float GetNextSpecialEffect(string blockName)
    {
        EnchantInfo blockEnchant = allEnchant.Find(x => x.enchantBlockName == blockName);
        return blockEnchant.GetNextSpecialInfo(); //해당 블록의 특수능력 강화 수치 반환
    }
    public int GetRequiredMoney(string blockName) //강화 레벨업을 위한 비용 요구
    {
        EnchantInfo blockEnchant = allEnchant.Find(x => x.enchantBlockName == blockName);
        return blockEnchant.GetRequiredMoney(); //해당 블록의 특수능력 강화 수치 반환
    }

    /*
     * 유닛 강화를 통해 강화를 할 경우 사용되는 함수
     * 유닛 강화 정보를 갱신하고 현재 사용중인 블럭유닛에도 갱신을 해준다.
     */
    public bool EnchantLevelUp(string blockName)
    {
        EnchantInfo blockEnchant = allEnchant.Find(x => x.enchantBlockName == blockName);

        int reqCost = blockEnchant.GetRequiredMoney();

        if (appManager.CheckMoney(reqCost)) {
            if (blockEnchant.LevelUp())
            { //레벨업을 시키고
                appManager.UseMoney(reqCost); //돈을 차감(reqCost의 갱신은 BlockEnchantUI에서 실행)

                GameObject enchantBtn = enchantBtnList.Find(x => x.name.Contains(blockName));
                enchantBtn.transform.Find("Att").gameObject.GetComponent<Text>().text = blockEnchant.GetAttInfo().ToString();
                if (enchantBtn.transform.Find("SpecialEffect") != null)
                {
                    if (blockName == "BlueBlock")
                    {
                        enchantBtn.transform.Find("SpecialEffect").gameObject.GetComponent<Text>().text = (blockEnchant.GetSpecialInfo() * 100).ToString();
                    }
                    else
                    {
                        enchantBtn.transform.Find("SpecialEffect").gameObject.GetComponent<Text>().text = blockEnchant.GetSpecialInfo().ToString();
                    }
                }

                List<GameObject> blockList = appManager.GetBlocksByType(blockName); //blockName에 해당하는 현재 사용중인 블럭 호출
                foreach (GameObject block in blockList)
                {
                    block.GetComponent<BlockInfo>().SetEnchantInfo(blockEnchant.GetAttInfo(), blockEnchant.GetSpecialInfo()); //강화된 내역을 블럭에게 넘겨줌
                }
                return true;
            }
        }
        return false;
    }

    /*
     *블럭으로부터 강화 정보 요청시 사용되는 함수
     * 보통 블럭이 강화된 이후 생성될 시 사용된다.
     */
    public void RequestEnchantInfo(BlockInfo block)
    { 
        EnchantInfo blockEnchant = allEnchant.Find(x => x.enchantBlockName == block.blockName);
        block.GetComponent<BlockInfo>().SetEnchantInfo(blockEnchant.GetAttInfo(), blockEnchant.GetSpecialInfo()); //강화된 내역을 블럭에게 넘겨줌
    }


}
