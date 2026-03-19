using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip _explosionClip;
    [SerializeField] private AudioClip _coinCollectClip;
    [SerializeField] private AudioClip _getHitClip;
    [SerializeField] private AudioClip _healingClip;
    [SerializeField] private AudioClip _shieldClip;
    [SerializeField] private AudioClip _speedClip;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem _hitEffect;
    [SerializeField] private GameObject _poisonedEffect;
    [SerializeField] private GameObject _poisonedBodyEffect;
    [SerializeField] private GameObject _slowEffect;
    [SerializeField] private GameObject _slowBodyEffect;    


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

    public void PlayPoisonedEffect()
    {
        if (_poisonedEffect != null && _poisonedBodyEffect !=null)
        {
            Instantiate(_poisonedBodyEffect, transform.position, Quaternion.identity, transform);
            Instantiate(_poisonedEffect, transform.position + new Vector3 (0,4f,0) , _poisonedEffect.transform.rotation, transform);
        }
    }

    public void PlaySlowEffect()
    {
        if (_slowEffect != null && _slowBodyEffect != null)
        {
            Instantiate(_slowBodyEffect, transform.position, Quaternion.identity, transform);
            Instantiate(_slowEffect, transform.position + new Vector3(0, 4f, 0), _slowEffect.transform.rotation, transform);
        }
    }

    public void PlayGetHitSound()
    {
        AudioManager.Instance.PlaySound(_getHitClip);
    }

    public void PlayHealingSound()
    {
        AudioManager.Instance.PlaySound(_healingClip);
    }

    public void PlayShieldSound()
    {
        AudioManager.Instance.PlaySound(_shieldClip);
    }

    public void PlayBoostSpeedSound()
    {
        AudioManager.Instance.PlaySound(_speedClip);
    }

    #endregion
}
