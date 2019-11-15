using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletInfo : MonoBehaviour
{
    private GameObject target; //총알이 발사될 목표
    private float damage;
    private float specialEffect; //특수효과 관련 확률 또는 데미지
    private bool isShot; //총알이 발사되었는가를 판단하는 bool변수
    public string bulletType;
    private WaveManager waveManager;
    private Animator anim;

    private void Awake()
    {
        waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();
    }

    private void OnEnable()
    {
        transform.position = transform.parent.transform.position;
    }

    private void Update()
    {
        if (isShot)
        {
            MoveBullet();
        }
    }


    public void Shoot(GameObject obj, float dmg)
    {
        target = obj;
        damage = dmg;
        isShot = true;
    }

    private void MoveBullet()
    {
        Vector3 nextPos  = Vector3.MoveTowards(transform.position,target.transform.position, 3.0f*Time.deltaTime);
        nextPos.z = -2f;
        gameObject.transform.position = nextPos;
    }

    private void DamageToEnemy(GameObject enemyObj)
    {
        float percent;
        switch (bulletType)
        {
            case "splash": //splash의 경우 먼저 적을 타격 후 해당 적 포함 일정 범위 내의 모든 적에게 약한 데미지를 가한다.
                enemyObj.GetComponent<EnemyInfo>().GetDamaged(damage);

                List<GameObject> damagedBySplash = waveManager.GetEnemyInRange(enemyObj,0.5f);

                if (anim == null)
                {
                    anim = gameObject.GetComponent<Animator>();
                }
                gameObject.transform.position = enemyObj.transform.position; //적 위치에서 고정되서 애니메이션 재생
                anim.SetInteger("Effect", 0); //애니메이션 재생(0->폭파)
                isShot = false; //충돌하고 애니메이션 도중에는 이동 불가하도록 

                if (damagedBySplash == null) //맞출 대상이 없는경우 취소
                {
                    break;
                }
                foreach (GameObject enemy in damagedBySplash)
                {
                    //추가예정 : 붉은 색으로 적 유닛을 잠깐 표시
                    enemy.GetComponent<EnemyInfo>().GetDamaged((int)specialEffect); //붉은 블럭의 강화수치에 따른 특수효과 데미지를 따른다.
                }
                break;
            case "normal":
                enemyObj.GetComponent<EnemyInfo>().GetDamaged(damage);
                break;
            case "pause":
                enemyObj.GetComponent<EnemyInfo>().GetDamaged(damage);

                percent = Random.Range(0, 1000)/10.0f; //소수 첫째자리까지 계산
                if (percent < specialEffect) //일정 확률로 일시정지
                {
                    enemyObj.GetComponent<EnemyInfo>().SetAbnormalStatus("pause",0.5f);
                }
                break;
            case "slow":
                enemyObj.GetComponent<EnemyInfo>().GetDamaged(damage);
                
                enemyObj.GetComponent<EnemyInfo>().SetAbnormalStatus("slow", 2.0f, specialEffect);
                break;
        }
    }
    /// <summary>
    /// 폭파 애니메이션 이후 오브젝트를 꺼야해서 invoke 사용하기 위해서 따로 만든 함수
    /// </summary>
    private void Hide()
    {
        Debug.Log("숨기기");
        gameObject.SetActive(false);
    }

    public void DeadTarget(GameObject enemy)
    {
        if (target.name == enemy.name)
        {
            target = null;
            isShot = false;
            if (anim != null)
            { //애니메이션을 가진 탄환이라면 따로 setactive처리를 하도록 한다.
                return;
            }
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy"&&target!=null){
            if (collision.gameObject.name == target.name)
            { //목표한 피격체여야 타격 성립
                isShot = false;
                DamageToEnemy(collision.gameObject);
                if (!bulletType.Equals("splash")) //폭파 애니메이션을 위해서 따로 애니메이션에 setactive를 애니메이션 뒤에 붙임
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    //각 블럭의 고유 특수효과에 대한 확률/데미지를 설정
    public void SetSpecialEffect(float value) {
        specialEffect = value;
    }
}
