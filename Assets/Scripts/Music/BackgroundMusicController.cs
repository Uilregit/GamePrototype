using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicController : MonoBehaviour
{
    public AudioClip music;
    public bool loop = true;

    // Start is called before the first frame update
    void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        MusicController.music.PlayBackground(music, loop);
    }
}
