using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletInfo : MonoBehaviour
{
    private GameObject target; //총알이 발사될 목표
    private float damage;
    private bool isShot; //총알이 발사되었는가를 판단하는 bool변수
    public string bulletType;
    private WaveManager waveManager;

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
        nextPos.z = -1.5f;
        gameObject.transform.position = nextPos;
    }

    private void DamageToEnemy(GameObject enemyObj)
    {
        float percent;
        switch (bulletType)
        {
            case "splash": //splash의 경우 먼저 적을 타격 후 해당 적 포함 일정 범위 내의 모든 적에게 약한 데미지를 가한다.
                enemyObj.GetComponent<EnemyInfo>().GetDamaged(damage);

                List<GameObject> damagedBySplash = waveManager.GetEnemyInRange(enemyObj,0.3f);
                foreach(GameObject enemy in damagedBySplash)
                {
                    //추가예정 : 붉은 색으로 적 유닛을 잠깐 표시
                    enemy.GetComponent<EnemyInfo>().GetDamaged(damage);
                }
                break;
            case "normal":
                enemyObj.GetComponent<EnemyInfo>().GetDamaged(damage);
                break;
            case "pause":
                //enemyObj.GetComponent<EnemyInfo>().GetDamaged(damage);
                percent = Random.Range(0, 100);
                if (percent < 3.0f) //일정 확률로 일시정지
                {
                    enemyObj.GetComponent<EnemyInfo>().SetAbnormalStatus("pause",0.4f);
                }
                break;
            case "slow":
                enemyObj.GetComponent<EnemyInfo>().GetDamaged(damage);
                percent = Random.Range(0, 100);
                if (percent < 4.0f) //일정 확률로 일시정지
                {
                    enemyObj.GetComponent<EnemyInfo>().SetAbnormalStatus("slow", 1.0f, 0.1f);
                }
                break;
        }
    }

    public void DeadTarget(GameObject enemy)
    {
        if (target.name == enemy.name)
        {
            target = null;
            isShot = false;
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
                gameObject.SetActive(false);
            }
        }
    }
}
