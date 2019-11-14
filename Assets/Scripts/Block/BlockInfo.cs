using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockInfo : MonoBehaviour
{
    private WaveManager waveManager;
    private Animator blockAnim;

    public string blockName;
    public List<Sprite> blockLevImage;
    public SpriteRenderer levImage; //레벨 이미지를 보여줄 자식 스프라이트렌더러 오브젝트
    private List<GameObject> bulletList;
    private Transform bulletListObj; //총알 오브젝트들의 그룹 오브젝트
    public int blockLevel;
    public List<int> damage; //현재 레벨에서의 데미지(기준점으로서 사용)->레벨을 사용한다면 리스트로 만들고 현재 레벨번째 값을 사용하는게 나을듯
    private float enhanceDmgBySupport; //보조 블럭(노란색)으로 인한 데미지 증가 배수(기본값 1)
    private int enchantDamage; //블럭 유닛 강화로 인한 데미지 상승량
    //데미지 공식 : (유닛 레벨별 기본 데미지+유닛 강화 데미지)*노란 블럭 데미지 상승배수
    private float damageNow; //여러가지 효과를 더한 상태에서의 데미지(실제 사용하는 값)

    [SerializeField]private bool isWaveStart;

    public string blockAttType; //블럭의 타입(일반/버프 2종류. 현재는 노란 블럭을 제외하면 모두 일반이다.)
    public float shootCoolTime;
    Coroutine shoot; //shoot 코루틴 해제를 위한 변수
    private GameObject targetEnemy;

    private void Awake()
    {
        waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();
        bulletListObj = transform.Find("BulletList"); //총알 오브젝트의 접근을 위해서 해당 그룹을 미리 찾아둔다.
        if (!gameObject.name.Contains("Yellow"))
        { //노란 블럭은 버프 애니메이션의 필요가 없음
            blockAnim = gameObject.GetComponent<Animator>();
        }
        gameObject.GetComponent<ButtonDrag>().previewObj=GameObject.Find("PreviewObj").gameObject;
        levImage.sprite = blockLevImage[0];
        blockLevel = 1;
        enhanceDmgBySupport = 1;
        isWaveStart = false;

        bulletList = new List<GameObject>();
        foreach(Transform child in bulletListObj)
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
        levImage.gameObject.SetActive(true);
        if (blockAttType == "support")
        {
            gameObject.GetComponent<SupportBlockInfo>().EnhanceNearBlocks();
        }
        GameObject.Find("EnchantManager").GetComponent<EnchantManager>().RequestEnchantInfo(this); 
    }    

    public void Refresh()
    { //유닛 초기화
        blockLevel = 1;
        enchantDamage = 0;
        isWaveStart = false;
        foreach(Transform bullet in transform)
        {
            bullet.gameObject.SetActive(false);
        }
    }

    public void SetBlockLevel(int lev)
    { //레벨업 시 바로 해당 레벨로 가야하므로 레벨을 설정하는 함수(AppManager에서 합성시 사용됨)
        blockLevel = lev;
        levImage.sprite = blockLevImage[lev-1];
    }
    
    public void SwitchWaveStatus(bool val)
    { //isWaveStart의 값을 바꾸는 함수
        isWaveStart = val;
        if (val == true)
        {
            bulletListObj.gameObject.SetActive(true);
            shoot = StartCoroutine("Shoot");
        }
        else
        {
            StopCoroutine(shoot);
        }
    }

    public void EnhancedBySupport(float mag)
    {
        enhanceDmgBySupport = mag;
        blockAnim.SetInteger("enhanced", 1);
    }
    public void ResetEnhance()
    { //노란 블럭이 근방에서 사라짐에 따라 데미지 상승효과 제거
        enhanceDmgBySupport = 1;
        blockAnim.SetInteger("enhanced", 0);
    }

    IEnumerator Shoot() { //shootCoolTime 간격으로 적을 향해 사격
        while (isWaveStart)
        {
            GameObject bullet = bulletList.Find(x => x.activeSelf == false);
            bullet.SetActive(true);

            damageNow = (damage[blockLevel - 1]+enchantDamage) * enhanceDmgBySupport;

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
        foreach (Transform bullet in bulletListObj)
        {
            if (bullet.gameObject.activeSelf == true)
            {
                bullet.gameObject.GetComponent<BulletInfo>().DeadTarget(enemy);
            }
        }
    }

    public void SetEnchantInfo(int att, float special)
    {
        enchantDamage = att;
        if (blockAttType!="support")
        {
            foreach (Transform child in bulletListObj) //노란블럭 제외 특수효과는 전부 탄환에서 이루어지므로 바로 적용
            {
                child.gameObject.GetComponent<BulletInfo>().SetSpecialEffect(special);
            }
        }
        else
        {
            gameObject.GetComponent<SupportBlockInfo>().SetEnhanceRatio(special);
        }
    }

}
