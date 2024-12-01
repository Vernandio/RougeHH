using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public AudioSource audioSource; 
    public AudioClip sword;
    public AudioClip death;
    public AudioClip walk;
    public AudioClip punch;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void swordSound(){
        audioSource.PlayOneShot(sword);
    }

    public void deathSound(){
        audioSource.PlayOneShot(death);
    }

    public void walkSound(){
        audioSource.PlayOneShot(walk);
    }

    public void punchSound(){
        audioSource.PlayOneShot(punch);
    }
}
