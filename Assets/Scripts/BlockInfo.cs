using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockInfo : MonoBehaviour
{
    private WaveManager waveManager;

    public string blockName;
    public List<Sprite> blockImage;
    private List<GameObject> bulletList;
    public int blockLevel;
    public List<int> damage; //현재 레벨에서의 데미지(기준점으로서 사용)->레벨을 사용한다면 리스트로 만들고 현재 레벨번째 값을 사용하는게 나을듯
    private float enhanceDmgBySupport; //보조 블럭(노란색)으로 인한 데미지 증가 배수(기본값 1)
    private float damageNow; //여러가지 효과를 더한 상태에서의 데미지(실제 사용하는 값)

    private bool isWaveStart;

    public string blockAttType; //블럭의 타입(일반/버프 2종류. 현재는 노란 블럭을 제외하면 모두 일반이다.)
    public float shootCoolTime;
    Coroutine shoot; //shoot 코루틴 해제를 위한 변수
    private GameObject targetEnemy;

    private void Awake()
    {
        waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();
        gameObject.GetComponent<ButtonDrag>().previewObj=GameObject.Find("PreviewObj").gameObject;
        blockLevel = 1;
        enhanceDmgBySupport = 1;
        isWaveStart = false;

        bulletList = new List<GameObject>();
        foreach(Transform child in gameObject.transform)
        {
            bulletList.Add(child.gameObject);
            child.gameObject.SetActive(false); //총알 오브젝트를 리스트에 넣고 끄기
        }

    }

    private void OnDisable()
    {
        Refresh();
    }
    public void InstallAtPos(Vector3 pos)
    {
        transform.position = pos;
        if (blockAttType == "support")
        {
            gameObject.GetComponent<SupportBlockInfo>().EnhanceNearBlocks();
        }
    }    

    public void Refresh()
    { //유닛 초기화
        blockLevel = 1;
        gameObject.GetComponent<SpriteRenderer>().sprite = blockImage[0];
        isWaveStart = false;
        foreach(Transform bullet in transform)
        {
            bullet.gameObject.SetActive(false);
        }
    }

    public void SetBlockLevel(int lev)
    { //레벨업 시 바로 해당 레벨로 가야하므로 레벨을 설정하는 함수(AppManager에서 합성시 사용됨)
        blockLevel = lev;
        gameObject.GetComponent<SpriteRenderer>().sprite = blockImage[lev-1];
    }
    
    public void SwitchWaveStatus(bool val)
    { //isWaveStart의 값을 바꾸는 함수
        isWaveStart = val;
        if (val == true)
        {
            Coroutine shoot = StartCoroutine("Shoot");
        }
    }

    public void EnhancedBySupport(float mag)
    {
        enhanceDmgBySupport = mag;
    }
    public void ResetEnhance()
    { //노란 블럭이 근방에서 사라짐에 따라 데미지 상승효과 제거
        enhanceDmgBySupport = 1;
    }

    IEnumerator Shoot() { //shootCoolTime 간격으로 적을 향해 사격
        while (isWaveStart)
        {
            GameObject bullet = bulletList.Find(x => x.activeSelf == false);
            bullet.SetActive(true);

            damageNow = damage[blockLevel - 1] * enhanceDmgBySupport;

            targetEnemy = waveManager.GetEnemyPosition();

            bullet.GetComponent<BulletInfo>().Shoot(targetEnemy,(int)Mathf.Round(damageNow)); //targetEnemy를 향해서 1.0f 데미지의 총알을 발사(총알 오브젝트는 자신의 하위 오브젝트에 각각 존재)
            //탄환의 데미지는 현재 블럭의 레벨에 따른 데미지와 차후 추가할 블럭 강화레벨에 따른 데미지의 합에 노란 블럭의 강화배수를 곱해 반올림처리하여 사용한다.

            yield return new WaitForSeconds(shootCoolTime);
        }
    }

    //enemy 유닛이 사망시 현재 사용중인 모든 block에게 요청하여 
    //현재 사격중인 target이 사망한 enemy일 경우 하위 bullet을 전부 off로 돌림
    public void CheckTarget(GameObject enemy)
    {
        foreach (Transform bullet in transform)
        {
            if (bullet.gameObject.activeSelf == true)
            {
                bullet.gameObject.GetComponent<BulletInfo>().DeadTarget(enemy);
            }
        }
    }

}
