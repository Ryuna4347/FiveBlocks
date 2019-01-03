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
                GameObject.Find("gameManager").GetComponent<AppManager>().BlockLevelUp(gameObject, hitObj.transform.gameObject);
            }
            previewObj.SetActive(false);
        }
        else
        { //놓을 수 있는 곳이 아닌 경우는 previewObj를 지움
            previewObj.SetActive(false);
        }
    }
}
