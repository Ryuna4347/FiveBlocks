using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveNotice : MonoBehaviour
{
    private GameObject controlChildNow; //현재 on/off를 사용해야하는 자식 텍스트 오브젝트
    private bool isFadingIn;

    private void OnEnable()
    {
        isFadingIn = true; //처음에는 fade in부터이므로 true로 설정
    }

    public void ControlChildNotice(int stage)
    { //성공UI의 경우 다음 웨이브에 따라 켜야하는 하위 텍스트가 다름
        if (gameObject.name.Contains("Clear"))
        {
            if ((stage + 1) % 15 == 0)
            {//15스테이지 기준으로 보스 웨이브 진행
                controlChildNow = transform.Find("BossAppearClear").gameObject;
            }
            else if ((stage + 1) % 15 != 0)
            {
                controlChildNow=transform.Find("NormalClear").gameObject;
            }
        }
        else if (gameObject.name.Contains("Failed"))
        {
            controlChildNow = transform.GetChild(0).gameObject;
        }
        controlChildNow.SetActive(true);

        StartCoroutine("FadeInNOut");
    }

    private IEnumerator FadeInNOut() {
        while (isFadingIn)
        {
            Color imageColor = gameObject.GetComponent<Image>().color;
            Color textColor = controlChildNow.GetComponent<Text>().color;

            imageColor.a += 0.02f;
            textColor.a += 0.02f;
            
            gameObject.GetComponent<Image>().color = imageColor;
            controlChildNow.GetComponent<Text>().color = textColor;

            if (imageColor.a >= 1f)
            {
                isFadingIn = false;
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return null;
            }
        }
        while (!isFadingIn)
        {
            Color imageColor = gameObject.GetComponent<Image>().color;
            Color textColor = controlChildNow.GetComponent<Text>().color;

            imageColor.a -= 0.02f;
            textColor.a -= 0.03f;

            gameObject.GetComponent<Image>().color = imageColor;
            controlChildNow.GetComponent<Text>().color = textColor;

            if (imageColor.a <= 0)
            {
                isFadingIn = true;
            }
            else
            {
                yield return null;
            }
        }
        gameObject.SetActive(false);
        controlChildNow.SetActive(false);
    }
}
