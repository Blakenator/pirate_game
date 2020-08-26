using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AudioClipGroup
{
    [SerializeField]
    private AudioClip[] clips;

    public AudioClip Get()
    {
        return clips.Length == 0 ? null : clips[UnityEngine.Random.Range(0, clips.Length)];
    }

    public void PlayOne(AudioSource source)
    {
        PlayOne(source, 1f);
    }

    public void PlayOne(AudioSource source, float volumeMultiplier)
    {
        var clip = Get();
        if (clip != null)
        {
            source.PlayOneShot(clip, volumeMultiplier);
        }
    }
}
