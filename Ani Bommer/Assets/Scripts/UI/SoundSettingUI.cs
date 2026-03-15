using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SoundSettingUI : MonoBehaviour
{
    [SerializeField] private Slider soundSlider; // BGM
    [SerializeField] private Slider sfxSlider;   // SFX

    private void Start()
    {
        if (AudioManager.Instance == null) return;

        // set giá trị ban đầu theo PlayerPrefs trong AudioManager
        soundSlider.SetValueWithoutNotify(AudioManager.Instance.GetSavedBGM01());
        sfxSlider.SetValueWithoutNotify(AudioManager.Instance.GetSavedSFX01());

        soundSlider.onValueChanged.AddListener(v =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetBGMVolume01(v);
        });

        sfxSlider.onValueChanged.AddListener(v =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetSFXVolume01(v);
        });
    }
}
