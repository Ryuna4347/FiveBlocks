using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public Animator tutorialAnim;

    private void OnEnable()
    {
        StartAnim();
    }

    public void StartAnim()
    {
        tutorialAnim.SetInteger("startAnim", 1);
    }

    private void OnDisable()
    {
        tutorialAnim.SetInteger("startAnim", 0);
    }
}
