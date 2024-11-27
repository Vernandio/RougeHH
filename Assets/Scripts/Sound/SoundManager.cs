using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public AudioSource audioSource; 
    public AudioClip normalMusic; 
    public AudioClip combatMusic; 
    public AudioClip sword;
    public AudioClip death;
    public AudioClip walk;
    public AudioClip punch;

    void Awake()
    {
        // If there's no instance of SoundManager, set it to this one
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Optional: Keep this object persistent across scenes
        }
        else
        {
            Destroy(gameObject);  // If an instance already exists, destroy this one
        }
    }

    // Start is called before the first frame update

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void combatSound(){
        audioSource.Stop();
        audioSource.clip = combatMusic;
        audioSource.Play();
    }

    public void normalSound(){
        audioSource.Stop();
        audioSource.clip = normalMusic;
        audioSource.Play();
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
