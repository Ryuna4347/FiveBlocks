using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip synthesizeBlock;
    public AudioClip createBlock;
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
        else if (audioName == "CreateBlock")
        {
            audio.PlayOneShot(createBlock);
        }
        else if (audioName == "FireBullet")
        {
            audio.PlayOneShot(fireBullet);
        }
    }
}
