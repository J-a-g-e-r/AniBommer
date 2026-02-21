using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip _explosionClip;
    [SerializeField] private AudioClip _coinCollectClip;

    #region Collection Effect

    public void PlayCollectionEffect(AudioClip clip)
    {
        //Play sound
        AudioManager.Instance.PlaySound(clip);
    }

    #endregion
}
