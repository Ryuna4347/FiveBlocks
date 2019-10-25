using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfoJson
{
    public string userId;
    public int bestScore;
    public int pauseGame; //게임 일시정지 여부(1-존재, 0-없음)(게임이 종료되지 않고 꺼진경우 해당 스테이지부터 다시 시작)
    public int pausedWave; //일시정지한 곳
    public string[] blockType; //블록이름이 아닌 블록 종류
    public string[] blockPos; //블록이 위치한 emptyArea의 번호(예시)0_0)
    public int[] blockLev;
    public int[] enchantLev; //각각 블록의 강화 상황
}

public class PausedGameInfo
{
    private int pausedWave; //일시정지한 곳
    private List<GameObject> block; //json에 있는 blockType을 기반으로 부여받음(pos와 lev은 로드시에 바로 추가해서 오브젝트만 받는걸로)
    private List<int> enchantLev; //각각 블록의 강화 상황

    public void SetWave(int wave)
    {
        pausedWave = wave;
    }
    public int GetWave()
    {
        return pausedWave;
    }

    public void SetBlockData(GameObject blockObj)
    {
        block.Add(blockObj);
    }
    public void SetEnchantData(int lev)
    {
        enchantLev.Add(lev);
    }

    public List<GameObject> GetBlockData()
    {
        return block;
    }
}

public class UserInformation : MonoBehaviour
{
    private PausedGameInfo paused;

    private void Awake()
    {
        paused = new PausedGameInfo();
    }

    //public bool LoadUserInfo() //반환 값은 이전 진행한 데이터가 있는가여부
    //{

    //}
}
