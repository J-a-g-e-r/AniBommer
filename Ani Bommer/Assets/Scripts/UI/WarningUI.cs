using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Scroll Image")]
    [SerializeField] private RectTransform imageA;
    [SerializeField] private RectTransform imageB;
    [SerializeField] private float scrollSpeed = 200f;

    [Header("Warning Settings")]
    [SerializeField] private float fadeSpeed = 6f;
    [SerializeField] private float minAlpha = 0.5f;
    [SerializeField] private float maxAlpha = 1f;

    private float imageWidth;


    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        imageWidth = imageA.rect.width;
        gameObject.SetActive(false);
    }

    public void PlayWarning(float duration)
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(PlayRoutine(duration));
    }

    private IEnumerator PlayRoutine(float duration)
    {
        ResetPositions();


        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            TickBlink();
            TickScroll();
            yield return null;
        }

        gameObject.SetActive(false);
    }


    private void TickBlink()
    {
        float ping = Mathf.PingPong(Time.unscaledTime * fadeSpeed, 1f);
        canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, ping);
    }
    private void TickScroll()
    {
        float dx = scrollSpeed * Time.deltaTime;
        imageA.anchoredPosition -= new Vector2(dx, 0f);
        imageB.anchoredPosition -= new Vector2(dx, 0f);
        if (imageA.anchoredPosition.x <= -imageWidth)
            imageA.anchoredPosition = new Vector2(imageB.anchoredPosition.x + imageWidth, imageA.anchoredPosition.y);
        if (imageB.anchoredPosition.x <= -imageWidth)
            imageB.anchoredPosition = new Vector2(imageA.anchoredPosition.x + imageWidth, imageB.anchoredPosition.y);
    }

    private void ResetPositions()
    {
        imageA.anchoredPosition = Vector2.zero;
        imageB.anchoredPosition = new Vector2(imageWidth, 0f);
        canvasGroup.alpha = maxAlpha;
    }

}
