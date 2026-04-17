using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("SFX")]
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioClip _loseSound;
    [SerializeField] private AudioClip _winSound;
    [SerializeField] private AudioClip _star1Sound;
    [SerializeField] private AudioClip _star2Sound;
    [SerializeField] private AudioClip _star3Sound;

    [Header("BGM")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioClip _menuBGM;
    [SerializeField] private AudioClip _pirateBGM;
    [SerializeField] private AudioClip _christmasBGM;

    [Header("Audio Mixer (Volume)")]
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private string _bgmVolumeParam = "BGMVol"; // tên exposed param trong mixer
    [SerializeField] private string _sfxVolumeParam = "SFXVol"; // tên exposed param trong mixer
    private const float MIN_DB = -80f;
    private const string PREF_BGM = "VOL_BGM";
    private const string PREF_SFX = "VOL_SFX";

    private MapType _currentMapType = MapType.None; 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (_sfxSource == null) _sfxSource = GetComponentInChildren<AudioSource>();
            if (_bgmSource == null) _bgmSource = GetComponentInChildren<AudioSource>();
            _bgmSource.enabled = true;

            float bgm = PlayerPrefs.GetFloat(PREF_BGM, 1f);
            float sfx = PlayerPrefs.GetFloat(PREF_SFX, 1f);
            SetBGMVolume01(bgm, save: false);
            SetSFXVolume01(sfx, save: false);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void PlayBGMByMapType(MapType mapType)
    {
        if (_currentMapType == mapType) return;   // đang phát r?i

        _currentMapType = mapType;
        AudioClip clip = null;

        switch (mapType)
        {
            case MapType.Menu:
                clip = _menuBGM;
                break;
            case MapType.Pirate:
                clip = _pirateBGM;
                break;
            case MapType.Christmas:
                clip = _christmasBGM;
                break;
            default:
                clip = null;
                break;
        }

        if (clip == null)
        {
            _bgmSource.Stop();
            return;
        }

        _bgmSource.clip = clip;
        _bgmSource.Play();
    }


    //SFX

    public void PlaySound(AudioClip clip) => _sfxSource.PlayOneShot(clip);

    public void PlayLoseSound() => _sfxSource.PlayOneShot(_loseSound);

    public void PlayWinSound() => _sfxSource.PlayOneShot(_winSound);

    // ====== VOLUME API cho UI Slider (0..1) ======

    public void SetBGMVolume01(float value01) => SetBGMVolume01(value01, save: true);
    public void SetSFXVolume01(float value01) => SetSFXVolume01(value01, save: true);

    private void SetBGMVolume01(float value01, bool save)
    {
        value01 = Mathf.Clamp01(value01);
        SetMixerVolume01(_bgmVolumeParam, value01);
        if (save) PlayerPrefs.SetFloat(PREF_BGM, value01);
    }

    private void SetSFXVolume01(float value01, bool save)
    {
        value01 = Mathf.Clamp01(value01);
        SetMixerVolume01(_sfxVolumeParam, value01);
        if (save) PlayerPrefs.SetFloat(PREF_SFX, value01);
    }

    private void SetMixerVolume01(string param, float value01)
    {
        if (_mixer == null || string.IsNullOrEmpty(param)) return;

        float db = (value01 <= 0.0001f) ? MIN_DB : Mathf.Log10(value01) * 20f;
        _mixer.SetFloat(param, db);
    }

    // (Tuỳ chọn) để UI scene nào cũng tự set slider đúng giá trị đã lưu
    public float GetSavedBGM01() => PlayerPrefs.GetFloat(PREF_BGM, 1f);
    public float GetSavedSFX01() => PlayerPrefs.GetFloat(PREF_SFX, 1f);

    private Coroutine _fadeCoroutine;

    public void FadeOutBGM(float fadeDuration = 1.5f)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeOutRoutine(fadeDuration));
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        // Lấy volume hiện tại của mixer (dB)
        _mixer.GetFloat(_bgmVolumeParam, out float currentDb);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newDb = Mathf.Lerp(currentDb, MIN_DB, elapsed / duration);
            _mixer.SetFloat(_bgmVolumeParam, newDb);
            yield return null;
        }

        _bgmSource.Stop();
        _fadeCoroutine = null;

        // Reset lại volume như đã lưu để khi restart play lại bình thường
        float savedVolume = PlayerPrefs.GetFloat(PREF_BGM, 1f);
        SetBGMVolume01(savedVolume, save: false);
    }

    public void RestartBGM()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        float savedVolume = PlayerPrefs.GetFloat(PREF_BGM, 1f);
        SetBGMVolume01(savedVolume, save: false);
        _bgmSource.Play();
    }

    public void PlayStarSoundByIndex(int starIndex)
    {
        AudioClip clip = null;
        switch (starIndex)
        {
            case 0: clip = _star1Sound; break; // sao thứ 1
            case 1: clip = _star2Sound; break; // sao thứ 2
            case 2: clip = _star3Sound; break; // sao thứ 3
        }
        if (clip != null) _sfxSource.PlayOneShot(clip);
    }
}
