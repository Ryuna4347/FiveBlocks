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
    private bool isWaveStart;

    public float shootCoolTime;
    Coroutine shoot; //shoot 코루틴 해제를 위한 변수

    private void Awake()
    {
        waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();
        gameObject.GetComponent<ButtonDrag>().previewObj=GameObject.Find("PreviewObj").gameObject;
        blockLevel = 1;
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
        if (blockLevel != 1)
        { //다른 곳에서 초기화를 하겠지만 안되어있을 경우 초기화
            Refresh();
        }
    }
    

    // Update is called once per frame
    void Update()
    {
    }

    public void Refresh()
    { //유닛 초기화
        blockLevel = 1;
        gameObject.GetComponent<SpriteRenderer>().sprite = blockImage[0];
        isWaveStart = false;
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

    IEnumerator Shoot() { //shootCoolTime 간격으로 적을 향해 사격
        while (isWaveStart)
        {
            GameObject bullet = bulletList.Find(x => x.activeSelf == false);
            bullet.SetActive(true);

            GameObject targetEnemy = waveManager.GetEnemyPosition();

            bullet.GetComponent<BulletInfo>().Shoot(targetEnemy, 1.0f); //targetEnemy를 향해서 1.0f 데미지의 총알을 발사(총알 오브젝트는 자신의 하위 오브젝트에 각각 존재)

            yield return new WaitForSeconds(shootCoolTime);
        }
    }


}
