﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip synthesizeBlock;
    [SerializeField] private AudioClip createBlock;
    [SerializeField] private AudioClip fireBullet;
    [SerializeField] private AudioClip enemyDead;
    private AudioSource audio;
    private bool audioOn; //오디오 on/off에 대한 설정값
    [SerializeField] private List<Sprite> audioImages; //오디오 on/off에 대한 이미지

    private void Start()
    {
        Screen.SetResolution(1080, 1920, true); //현재 타이틀씬에서 매니저가 이것밖에 없기 때문에 여기서 해상도를 조정

        audio = gameObject.GetComponent<AudioSource>();
        audioOn = true;
        DontDestroyOnLoad(gameObject);
    }

    public void AudioOnOff() //오디오를 끄고 켜는 기능(게임 시작 전 타이틀씬에서 조절 가능)
    {
        if (audioOn)
        {
            audioOn = false;
            GameObject.Find("AudioImage").GetComponent<Image>().sprite = audioImages[1]; //오디오 off에 대한 이미지를 2번째 값으로 넣을것
        }
        else
        {
            audioOn = true;
            GameObject.Find("AudioImage").GetComponent<Image>().sprite = audioImages[0]; //오디오 on에 대한 이미지를 1번째 값으로 넣을것
        }
    }

    public void PlayAudio(string audioName)
    {
        if (audioOn) //오디오를 on으로 설정한 경우에만 재생
        {
            if (audioName == "Synthesize")
            {
                audio.PlayOneShot(synthesizeBlock);
            }
            else if (audioName == "CreateBlock")
            {
                audio.PlayOneShot(createBlock);
            }
            else if (audioName == "FireBullet")
            {
                audio.PlayOneShot(fireBullet);
            }
            else if (audioName == "EnemyDead")
            {
                audio.PlayOneShot(enemyDead);
            }
        }
    }
}
