using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _loseSound;
    [SerializeField] private AudioClip _winSound;
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
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

    public void PlaySound(AudioClip clip) => _audioSource.PlayOneShot(clip);

    public void PlayLoseSound() => _audioSource.PlayOneShot(_loseSound);

    public void PlayWinSound() => _audioSource.PlayOneShot(_winSound);
}
