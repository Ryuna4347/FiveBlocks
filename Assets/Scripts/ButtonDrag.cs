using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonDrag : MonoBehaviour
{
    public GameObject obj; //obj자체가 투명도 0.3~0.5정도로 되어있음(조절 x)
    Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        obj = GameObject.Find("CloneObj");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        obj.transform.position = transform.position; //드래그 효과를 위해서 가져온다.
        obj.GetComponent<SpriteRenderer>().sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
    }
    private void OnMouseDrag()
    {
        Vector3 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        temp.z = 0;
        obj.SetActive(true);
        obj.transform.position =  temp;
    }
    private void OnMouseUp()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit2D hitObj= Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hitObj.transform != null)
        {
            if (hitObj.transform.tag.Equals("EmptyArea"))
            {
                Vector3 emptyPos = hitObj.transform.position;
                emptyPos.z = 0; //EmptyArea가 겹쳐있기 위해서 뒤에 위치해 있기 때문에 z축을 조절해 줘야한다.
                transform.position = emptyPos;
            }
            obj.SetActive(false);
        }
        else
        { //놓을 수 있는 곳이 아닌 경우는 obj를 지움
            obj.SetActive(false);
        }
    }
}
