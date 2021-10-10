using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSoundController : MonoBehaviour
{
    public AudioSource source;
    public AudioClip[] selectSounds;
    public AudioClip[] castingSounds;
    public AudioClip[] dragSounds;
    public AudioClip[] castSounds;
    public AudioClip[] uncastSounds;
    public AudioClip[] replaceSounds;

    // Start is called before the first frame update
    void Start()
    {
        source.volume = MusicController.music.sFXSource.volume;
        source.loop = false;
    }

    public void PlaySelectSound()
    {
        source.clip = selectSounds[Random.Range(0, selectSounds.Length)];
        source.loop = false;
        source.Play();
    }

    public void PlayDragSound()
    {
        source.clip = dragSounds[Random.Range(0, dragSounds.Length)];
        source.loop = false;
        source.Play();
    }
    public void PlayCastingSound()
    {
        source.Stop();
        source.clip = castingSounds[Random.Range(0, castingSounds.Length)];
        source.loop = true;
        source.Play();
    }
    public void PlayCastSound()
    {
        source.clip = castSounds[Random.Range(0, castSounds.Length)];
        source.loop = false;
        source.Play();
    }
    public void PlayUncastSound()
    {
        source.clip = uncastSounds[Random.Range(0, uncastSounds.Length)];
        source.loop = false;
        source.Play();
    }
    public void PlayReplaceSound()
    {
        source.clip = replaceSounds[Random.Range(0, replaceSounds.Length)];
        source.loop = false;
        source.Play();
    }
}
