using UnityEngine;

public class MonsterEffect : MonoBehaviour
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip _monsterHitClip;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem _bloodHitEffect;
    [SerializeField] private GameObject _deathEffect;

    public void PlayGetHitSound()
    {
        if (_monsterHitClip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySound(_monsterHitClip);
    }

    public void PlayBloodEffect()
    {
        if (_bloodHitEffect == null) return;
        _bloodHitEffect.Play();
        PlayGetHitSound();
    }

    public void PlayDeathEffect()
    {
        if (_deathEffect == null) return;
        Instantiate(_deathEffect, transform.position + new Vector3(0,1,0), Quaternion.identity);
    }
}