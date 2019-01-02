using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockInfo : MonoBehaviour
{
    public string blockName;
    public Sprite blockImage;

    private void Awake()
    {
        gameObject.GetComponent<ButtonDrag>().previewObj=GameObject.Find("PreviewObj").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
