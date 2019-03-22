using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 *사운드 On/Off 조절 관련 버튼에 대한 스크립트.
 *사운드 매니저가 씬을 넘어다니기 때문에 사운드 조절 버튼에서 직접 onClick이벤트를 연결한다.
 */
public class AudioBtn : MonoBehaviour
{
    private SoundManager soundManager;

    // Start is called before the first frame update
    void Awake()
    {
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        soundManager.AudioImage = gameObject.GetComponent<Image>();
        gameObject.GetComponent<Button>().onClick.AddListener(()=>soundManager.AudioOnOff());
    }
}
