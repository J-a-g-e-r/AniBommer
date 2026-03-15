using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip _explosionClip;
    [SerializeField] private AudioClip _coinCollectClip;
    [SerializeField] private AudioClip _getHitClip;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem _hitEffect;
    //[SerializeField] private ParticleSystem _spawnEffect;

    #region SFX Effect

    public void PlayCollectionEffect(AudioClip clip)
    {
        //Play sound
        AudioManager.Instance.PlaySound(clip);
    }



    #endregion


    #region Effect
    public void PlayDamageEffect()
    {
        //Play particle effect
        if (_hitEffect != null)
        {
            _hitEffect.Play();
        }
    }
    public void PlayGetHitSound()
    {
        AudioManager.Instance.PlaySound(_getHitClip);
    }

    //public void PlaySpawnEffect()
    //{
    //    if(_spawnEffect != null)
    //    {
    //        _spawnEffect.Play();
    //    }
    //}

    #endregion
}
