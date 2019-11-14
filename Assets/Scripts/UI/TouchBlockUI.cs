using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 두개의 캔버스가 동시에 클릭되지 않도록 방지하기 위한 스크립트
 */
public class TouchBlockUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject blockEnchantUI;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private CanvasGroup backgroundCanvas; //NoticeCanvas 내의 오브젝트가 켜질 시 BackGroundCanvas와 클릭이 겹치게 되어서 해당 캔버스의 클릭을 방지하기 위해서 선언함



    ///<summary>NoticeCanvas 내의 특정 UI의 active를 true로 변경</summary>
    public void ActiveUI(string UIName)
    {
        switch (UIName)
        {
            case "Pause":
                pauseUI.SetActive(true);
                break;
            case "GameOver":
                gameOverUI.SetActive(true);
                break;
            case "BlockEnchant":
                blockEnchantUI.SetActive(true);
                break;
        }
        backgroundCanvas.blocksRaycasts = false;
    }
    ///<summary>NoticeCanvas 내의 특정 UI의 active를 false로 변경</summary>
    public void UnactiveUI(string UIName)
    {
        switch (UIName)
        {
            case "Pause":
                pauseUI.SetActive(false);
                break;
            case "GameOver":
                gameOverUI.SetActive(false);
                break;
            case "BlockEnchant":
                blockEnchantUI.SetActive(false);
                break;
        }
        backgroundCanvas.blocksRaycasts = true;
    }
}
