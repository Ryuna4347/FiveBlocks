using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggedButton : MonoBehaviour
{
    GameObject parent;
    public void SetParent(GameObject par)
    {
        parent = par;
    }
    private void OnMouseUp()
    {
        parent.transform.position = transform.position;
    }
}
