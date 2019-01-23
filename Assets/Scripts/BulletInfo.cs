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
        if (target.activeSelf == false)
        {//발사는 되었지만 다른 탄환에 의해서 목표 적 유닛이 죽었을 경우 그냥 탄환을 끈다.
            isShot = false;
            gameObject.SetActive(false);
            return;
        }
        Vector3 nextPos  = Vector3.MoveTowards(transform.position,target.transform.position, 2.0f*Time.deltaTime);
        nextPos.z = -1.5f;
        gameObject.transform.position = nextPos;
    }

    private void DamageToEnemy(GameObject enemyObj)
    {
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
            case "slow":
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy"&&collision.gameObject.name==target.name) { //목표한 피격체여야 타격 성립
            isShot = false;
            DamageToEnemy(collision.gameObject);
            gameObject.SetActive(false);
        }
    }
}
