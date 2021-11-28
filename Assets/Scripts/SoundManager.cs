using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip success;
    public AudioClip failure;

    public void OnSuccess()
    {
        Camera.main.transform.GetComponent<AudioSource>().PlayOneShot(success);
    }

    public void OnFailure()
    {
        Camera.main.transform.GetComponent<AudioSource>().PlayOneShot(failure);
    }
}
