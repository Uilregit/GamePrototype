using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHitSoundController : MonoBehaviour
{
    public List<AudioSource> effectSource;

    public AudioClip[] footStepSounds;

    public AudioClip armorHitSound;
    public AudioClip armorBreakSound;
    public AudioClip armorBrokenSounds;
    public AudioClip immunitySound;

    public AudioClip[] swordSounds;
    public AudioClip[] swordHeavySounds;
    public AudioClip[] arrowSounds;
    public AudioClip[] clawSounds;

    public AudioClip[] fireSounds;
    public AudioClip[] fireHeavySounds;
    public AudioClip[] thunderSounds;
    public AudioClip[] thunderHeavySounds;
    public AudioClip[] iceSounds;
    public AudioClip[] iceHeavySounds;
    public AudioClip[] windSounds;

    public AudioClip[] healSounds;
    public AudioClip[] buffSounds;
    public AudioClip[] debuffSounds;
    public AudioClip[] armorGainSounds;
    public AudioClip[] stunSounds;
    public AudioClip[] tauntSounds;

    private Queue<AudioSource> effectSourceQueue;
    private List<AudioSource> usedEffectSource = new List<AudioSource>();

    private void Start()
    {
        effectSourceQueue = new Queue<AudioSource>();
        foreach (AudioSource audio in effectSource)
        {
            audio.enabled = false;
            effectSourceQueue.Enqueue(audio);
        }
    }

    private void FixedUpdate()
    {
        List<AudioSource> finishedAudioSources = new List<AudioSource>();
        foreach (AudioSource audio in usedEffectSource)
            if (!audio.isPlaying)
                finishedAudioSources.Add(audio);
        foreach (AudioSource audio in finishedAudioSources)
        {
            audio.enabled = false;
            effectSourceQueue.Enqueue(audio);
            usedEffectSource.Remove(audio);
        }
    }

    private AudioSource GetAudioSource()
    {
        AudioSource output = effectSourceQueue.Dequeue();
        usedEffectSource.Add(output);
        output.enabled = true;
        return output;
    }

    public void PlayFootStepSound()
    {
        AudioSource source = GetAudioSource();

        source.pitch = Random.Range(0.8f, 1.3f);

        source.clip = footStepSounds[Random.Range(0, footStepSounds.Length)];
        source.volume = MusicController.music.sFXSource.volume;
        source.Play();
    }

    public void PlaySound(Card.SoundEffect soundType)
    {
        AudioClip[] usedList = GetSoundsFromType(soundType);
        AudioSource source = GetAudioSource();

        if (usedList.Length > 0)
        {
            source.pitch = Random.Range(0.8f, 1.3f);

            source.clip = usedList[Random.Range(0, usedList.Length)];
            source.volume = MusicController.music.sFXSource.volume;
            source.Play();
        }
    }

    public void PlayArmorSound(Card.SoundEffect armorSoundType, float percentDamageBlocked)
    {
        AudioSource source = GetAudioSource();

        source.pitch = Random.Range(0.8f, 1.3f);

        switch (armorSoundType)
        {
            case Card.SoundEffect.ArmorHit:
                source.clip = armorHitSound;
                source.volume = MusicController.music.sFXSource.volume * (percentDamageBlocked / 2.0f + 0.5f);  //0% blocked -> 50% volume, 100% blocked -> 100% volume
                source.Play();
                break;
            case Card.SoundEffect.ArmorBreak:
                source.clip = armorBreakSound;
                source.volume = MusicController.music.sFXSource.volume;
                source.Play();
                break;
            case Card.SoundEffect.ArmorBroken:
                source.clip = armorBrokenSounds;
                source.volume = MusicController.music.sFXSource.volume;
                source.Play();
                break;
            case Card.SoundEffect.Immunity:
                source.clip = immunitySound;
                source.volume = MusicController.music.sFXSource.volume;
                source.Play();
                break;
        }
    }

    private AudioClip[] GetSoundsFromType(Card.SoundEffect soundType)
    {
        AudioClip[] usedList = new AudioClip[0];
        switch (soundType)
        {
            case Card.SoundEffect.Sword:
                usedList = swordSounds;
                break;
            case Card.SoundEffect.SwordHeavy:
                usedList = swordHeavySounds;
                break;
            case Card.SoundEffect.Arrow:
                usedList = arrowSounds;
                break;
            case Card.SoundEffect.Claw:
                usedList = clawSounds;
                break;
            case Card.SoundEffect.Fire:
                usedList = fireSounds;
                break;
            case Card.SoundEffect.FireHeavy:
                usedList = fireHeavySounds;
                break;
            case Card.SoundEffect.Thunder:
                usedList = thunderSounds;
                break;
            case Card.SoundEffect.ThunderHeavy:
                usedList = thunderHeavySounds;
                break;
            case Card.SoundEffect.Ice:
                usedList = iceSounds;
                break;
            case Card.SoundEffect.IceHeavy:
                usedList = iceHeavySounds;
                break;
            case Card.SoundEffect.Wind:
                usedList = windSounds;
                break;
            case Card.SoundEffect.Heal:
                usedList = healSounds;
                break;
            case Card.SoundEffect.Buff:
                usedList = buffSounds;
                break;
            case Card.SoundEffect.Debuff:
                usedList = debuffSounds;
                break;
            case Card.SoundEffect.ArmorGain:
                usedList = armorGainSounds;
                break;
            case Card.SoundEffect.Stun:
                usedList = stunSounds;
                break;
            case Card.SoundEffect.Taunt:
                usedList = tauntSounds;
                break;
        }
        return usedList;
    }
}
