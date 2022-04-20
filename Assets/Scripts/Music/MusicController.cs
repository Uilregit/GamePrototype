using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public static MusicController music;

    public AudioSource backgroundSource;
    public AudioSource sFXSource;

    public AudioLowPassFilter lowPass;
    public AudioHighPassFilter highPass;

    public AudioClip goldSFX;
    public AudioClip uiUseHighSFX;
    public List<AudioClip> uiUseLowSFX;
    public List<AudioClip> footStepSFX;
    public List<AudioClip> paperMoveSFX;

    public float backgroundVolume = 1;
    public float soundFXVolume = 1;
    private float backgroundVolumeMultplier = 1;

    private AudioClip currentBackgroundMusic;

    private float[] lastbackgroundAmplitude = new float[8] { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f };
    private float backgroundAmplitudeMaxDecrease = 0.000005f;
    private float[] maxBackgroundAmplitude = new float[8] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };

    private void Start()
    {
        if (MusicController.music != null)
            Destroy(this.gameObject);
        else
            MusicController.music = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public void SetBackGroundVolume(float volume)
    {
        backgroundVolume = volume;
        backgroundVolumeMultplier = 1;
        if (highPass.enabled)
            backgroundVolumeMultplier *= 2f;
        if (lowPass.enabled)
            backgroundVolumeMultplier *= 2f;

        backgroundSource.volume = volume * backgroundVolumeMultplier;
        backgroundSource.enabled = !(volume * backgroundVolumeMultplier == 0);
    }

    public void SetSFXVolume(float volume)
    {
        soundFXVolume = volume;
        sFXSource.volume = volume;
        sFXSource.enabled = !(volume == 0);
    }

    public void PlayBackground(AudioClip clip, bool loop)
    {
        if (clip != currentBackgroundMusic)
        {
            backgroundSource.clip = clip;
            backgroundSource.loop = loop;
            backgroundSource.Play();
            currentBackgroundMusic = clip;
        }
    }

    public void PlaySFX(AudioClip clip, bool loop = false)
    {
        sFXSource.pitch = Random.Range(0.8f, 1.3f);

        sFXSource.clip = clip;
        sFXSource.loop = loop;
        sFXSource.Play();
    }

    public void SetHighPassFilter(bool state)
    {
        highPass.enabled = state;
        SetBackGroundVolume(backgroundVolume);
    }

    public void SetLowPassFilter(bool state)
    {
        lowPass.enabled = state;
        SetBackGroundVolume(backgroundVolume);
    }

    //Returns the normalized (0 --> 1) amplitude of the background music in 8 frequency bands ([0] low freq, [7] high freq)
    public float[] GetBackgroundAmplitude()
    {
        if (currentBackgroundMusic == null || backgroundVolume == 0)
            return new float[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

        float[] output = new float[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

        float[] clipSampleData = new float[512];
        backgroundSource.GetSpectrumData(clipSampleData, 0, FFTWindow.Blackman);

        int counter = 0;
        for (int i = 0; i < 8; i++)
        {
            int band = (int)Mathf.Pow(2, i) * 2;
            if (i == 7)
                band += 2;

            for (int j = 0; j < band; j++)
            {
                output[i] += clipSampleData[counter] * (counter + 1);
                counter++;
            }

            output[i] = output[i] / counter * 10;
            maxBackgroundAmplitude[i] = Mathf.Max(output[i] / backgroundVolume, maxBackgroundAmplitude[i]);

            output[i] = output[i] / maxBackgroundAmplitude[i];

            if (lastbackgroundAmplitude[i] > 0)
            {
                lastbackgroundAmplitude[i] = output[i];
            }
            else
            {
                lastbackgroundAmplitude[i] = Mathf.Max(output[i], lastbackgroundAmplitude[i] - backgroundAmplitudeMaxDecrease);
            }
        }
        return lastbackgroundAmplitude;
    }
}
