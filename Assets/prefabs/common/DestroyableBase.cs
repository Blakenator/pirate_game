using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableBase : MonoBehaviour
{
    protected bool isDestroying = false;
    public AudioSource audioSource;

    protected void DestroyAfterAudio(GameObject obj, AudioClip clip)
    {
        StartCoroutine(_DoDestroy(obj, clip));
    }

    IEnumerator _DoDestroy(GameObject obj, AudioClip clip)
    {
        isDestroying = true;

        GetComponentInChildren<MeshRenderer>().enabled = false;
        if (clip == null)
        {
            Destroy(obj);
        }
        else
        {
            audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
            Destroy(obj);
        }
    }
}
