using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonDrag : MonoBehaviour
{
    public GameObject previewObj; //previewObj자체가 투명도 0.3~0.5정도로 되어있음(조절 x)
    Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        previewObj.SetActive(true);
        previewObj.transform.position = transform.position; //드래그 효과를 위해서 가져온다.
        previewObj.GetComponent<SpriteRenderer>().sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
    }
    private void OnMouseDrag()
    {
        Vector3 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        temp.z = -0.5f;
        previewObj.SetActive(true);
        previewObj.transform.position =  temp;
    }
    private void OnMouseUp()
    {
        RaycastHit2D hitObj= Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        
        if (hitObj.transform!= null)
        {
            if (hitObj.transform.gameObject.tag=="Block")
            {
                string blockType = hitObj.transform.gameObject.GetComponent<BlockInfo>().blockName.ToUpper(); //종류 비교를 위해 블럭정보 스크립트에서 이름을 획득
                if (gameObject.name.ToUpper().Contains(blockType)) //동일한 종류(혹시 소문자/대문자 착각이 있을 수 있으므로 대문자로 변형해서 확인)
                {
                    //레벨이 같은지도 차후에 추가해야됨

                    GameObject.Find("gameManager").LevelUp(gameObject,hitObj.transform.gameObject);

                    GameObject.Find("gameManager").GetComponent<AppManager>().MoveUsedToEmpty(gameObject);
                }
            }
            previewObj.SetActive(false);
        }
        else
        { //놓을 수 있는 곳이 아닌 경우는 previewObj를 지움
            previewObj.SetActive(false);
        }
    }
}
