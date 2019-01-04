using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip synthesizeBlock;
    public AudioClip fireBullet;
    private AudioSource audio;

    private void Start()
    {
        audio = gameObject.GetComponent<AudioSource>();   
    }

    public void PlayAudio(string audioName)
    {
        if (audioName == "Synthesize")
        {
            audio.PlayOneShot(synthesizeBlock);
        }
        else if (audioName == "FireBullet")
        {
            audio.PlayOneShot(fireBullet);
        }
    }
}
