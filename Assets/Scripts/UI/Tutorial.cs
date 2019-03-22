using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *초기 실행 시 또는 임의로 띄우는 튜토리얼 UI에 대한 스크립트
 */
public class Tutorial : MonoBehaviour
{
    [SerializeField] private Animator tutorialAnim; //튜토리얼 내 애니메이션
    [SerializeField] private CanvasGroup backgroundCanvas; //뒷 배경 Canvas의 클릭 방지를 위함

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
