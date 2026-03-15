using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button bgmButton;
    [SerializeField] private Image bgmImage;

    [SerializeField] private Button sfxButton;
    [SerializeField] private Image sfxImage;

    [SerializeField] private Button quitButton;

    [Header("Visual")]
    [SerializeField] private Color onColor = Color.white;                          // màu khi đang bật
    [SerializeField] private Color offColor = new Color(0f, 0f, 0f, 0.5f);        // màu khi tắt (đen/mờ)

    private bool bgmOn = true;
    private bool sfxOn = true;

    private void Start()
    {
        // Đọc trạng thái đã lưu (nếu có)
        if (AudioManager.Instance != null)
        {
            bgmOn = AudioManager.Instance.GetSavedBGM01() > 0.001f;
            sfxOn = AudioManager.Instance.GetSavedSFX01() > 0.001f;
        }

        UpdateBgmVisual();
        UpdateSfxVisual();

        // Gán sự kiện click cho các nút
        if (bgmButton != null) bgmButton.onClick.AddListener(ToggleBgm);
        if (sfxButton != null) sfxButton.onClick.AddListener(ToggleSfx);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    private void ToggleBgm()
    {
        bgmOn = !bgmOn;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume01(bgmOn ? 1f : 0f);
        }

        UpdateBgmVisual();
    }

    private void ToggleSfx()
    {
        sfxOn = !sfxOn;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume01(sfxOn ? 1f : 0f);
        }

        UpdateSfxVisual();
    }

    private void UpdateBgmVisual()
    {
        if (bgmImage != null)
            bgmImage.color = bgmOn ? onColor : offColor;
    }

    private void UpdateSfxVisual()
    {
        if (sfxImage != null)
            sfxImage.color = sfxOn ? onColor : offColor;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // thoát Play Mode trong Editor
#else
        Application.Quit(); // build thật sẽ thoát app
#endif
    }

}
