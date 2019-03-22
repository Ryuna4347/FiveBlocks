using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private Animator tutorialAnim;
    [SerializeField] private CanvasGroup backgroundCanvas;

    private void OnEnable()
    {
        StartAnim();
        backgroundCanvas.blocksRaycasts = false;
    }

    public void StartAnim()
    {
        tutorialAnim.SetInteger("startAnim", 1);
    }

    private void OnDisable()
    {
        tutorialAnim.SetInteger("startAnim", 0);
        backgroundCanvas.blocksRaycasts = true;
    }
}
